using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProvexBackendAPI.Infrastructure.Auth;
using ProvexBackendAPI.Repository.IRepository;
using ProvexBackendAPI.Services.IServices;
using ProvexBackendAPI.Services.IServices.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProvexBackendAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwt;
        private readonly SymmetricSecurityKey _signingKey;
        private readonly IUserRepository _userRepository;

        public TokenService(IOptions<JwtSettings> jwtOptions, IUserRepository userRepository)
        {
            _jwt = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
            if (string.IsNullOrWhiteSpace(_jwt.Key))
                throw new InvalidOperationException("Jwt:Key no está configurado");
            _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            _userRepository = userRepository;
        }

        public async Task<AccessTokenResult> GenerateTokenRobustoAsync<T>(
        T subject,
        Func<T, Task<IEnumerable<string>>>? rolesProvider = null,
        IEnumerable<ClaimMap<T>>? maps = null,
        IEnumerable<Claim>? extraClaims = null)
        {
            if (subject is null) throw new ArgumentNullException(nameof(subject));

            var claims = new List<Claim>();

            //Atributos
            claims.AddRange(BuildClaimsFromAttributes(subject));

            
            claims.AddRange(BuildClaimsByConvention(subject));

            //Map fluido
            if (maps is not null)
            {
                foreach (var map in maps)
                {
                    var v = map.Selector(subject);
                    if (string.IsNullOrWhiteSpace(v) && map.SkipIfNullOrEmpty) continue;
                    claims.Add(new Claim(map.Type, v ?? string.Empty));
                }
            }

            //Extras explícitos
            if (extraClaims is not null) claims.AddRange(extraClaims);

            //Roles
            if (rolesProvider is not null)
            {
                var roles = await rolesProvider(subject);
                if (roles is not null)
                {
                    foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));
                }
            }

            //sub/jti si faltan
            EnsureStandardClaims(subject, claims);

            var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            var minutes = _jwt.AccessTokenMinutes <= 0 ? 120 : _jwt.AccessTokenMinutes;
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(minutes);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims.Distinct(new ClaimEq())),
                Expires = expiresAtUtc,
                // Issuer = _jwt.Issuer,
                // Audience = _jwt.Audience,
                SigningCredentials = creds
            };

            var handler = new JwtSecurityTokenHandler
            {
                SetDefaultTimesOnTokenCreation = false
            };
            var token = handler.CreateToken(descriptor);
            var tokenString = handler.WriteToken(token);

            return new AccessTokenResult(
                Token: tokenString,
                ExpiresAtUtc: expiresAtUtc,
                ExpiresAtUnix: new DateTimeOffset(expiresAtUtc).ToUnixTimeSeconds()
            );
        }

        public async Task<AccessTokenResult> GenerateTokenAsync(string username, List<string> roles)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username requerido", nameof(username));

            if (roles == null || roles.Count == 0)
                throw new ArgumentException("Al menos un rol es requerido", nameof(roles));

            // Validación roles vacío
            var rolesLimpios = roles
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToList();

            if (rolesLimpios.Count == 0)
                throw new ArgumentException("Todos los roles están vacíos o en blanco", nameof(roles));


            // Claims de negocio
            var claims = new List<Claim>
            {
                new Claim("username", username),
                new Claim("roles", JsonSerializer.Serialize(rolesLimpios))
            };

            
            var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            var minutes = _jwt.AccessTokenMinutes <= 0 ? 120 : _jwt.AccessTokenMinutes;
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(minutes);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAtUtc,
                SigningCredentials = creds                
            };

            //Para NO agregar datos innecesarios como iat/nbf automáticamente
            var handler = new JwtSecurityTokenHandler
            {
                SetDefaultTimesOnTokenCreation = false
            };

            var token = handler.CreateToken(descriptor);
            var tokenString = handler.WriteToken(token);

            return new AccessTokenResult(
                Token: tokenString,
                ExpiresAtUtc: expiresAtUtc,
                ExpiresAtUnix: new DateTimeOffset(expiresAtUtc).ToUnixTimeSeconds()
            );
        }

        public async Task<Guid?> GetUserIdFromClaimsAsync(ClaimsPrincipal user)
        {
            if (user == null || !user.Identity?.IsAuthenticated == true)
                return null;

            var username = user.FindFirst("username")?.Value ?? user.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(username))
                return null;

            var appUser = await _userRepository.GetUserByUsername(username);
            return appUser?.Id;
        }

        public async Task<Guid?> GetUserIdFromTokenAsync2(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token requerido", nameof(token));

            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = token.Substring("Bearer ".Length).Trim();

            var tokenHandler = new JwtSecurityTokenHandler();

            JwtSecurityToken jwt;
            try
            {
                jwt = tokenHandler.ReadJwtToken(token);
            }
            catch
            {
                return null;
            }

            var username =
                jwt.Claims.FirstOrDefault(c => c.Type == "username")?.Value ??
                jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(username))
                return null;

            var user = await _userRepository.GetUserByUsername(username);
            return user?.Id;
        }

        private static IEnumerable<Claim> BuildClaimsFromAttributes<T>(T subject)
        {
            var list = new List<Claim>();
            var t = subject!.GetType();
            foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var attr = p.GetCustomAttribute<ClaimFromPropertyAttribute>();
                if (attr is null) continue;

                var str = p.GetValue(subject)?.ToString();
                if (attr.Required && string.IsNullOrWhiteSpace(str))
                    throw new InvalidOperationException($"Claim requerido '{attr.ClaimType}' no pudo mapearse desde '{p.Name}' (nulo/vacío).");

                if (!string.IsNullOrWhiteSpace(str))
                    list.Add(new Claim(attr.ClaimType, str!));
            }
            return list;
        }

        private static IEnumerable<Claim> BuildClaimsByConvention<T>(T subject)
        {
            var list = new List<Claim>();
            var t = subject!.GetType();

            var idProp = t.GetProperty("Id") ?? t.GetProperty($"{t.Name}Id") ?? t.GetProperty("UserId");
            var idVal = idProp?.GetValue(subject)?.ToString();
            if (!string.IsNullOrWhiteSpace(idVal))
            {
                list.Add(new Claim(ClaimTypes.NameIdentifier, idVal!));
                list.Add(new Claim(JwtRegisteredClaimNames.Sub, idVal!));
            }

            var emailProp = t.GetProperty("Email") ?? t.GetProperty("EmailAddress");
            var emailVal = emailProp?.GetValue(subject)?.ToString();
            if (!string.IsNullOrWhiteSpace(emailVal))
                list.Add(new Claim(JwtRegisteredClaimNames.Email, emailVal!));

            var userNameProp = t.GetProperty("UserName") ?? t.GetProperty("Username") ?? t.GetProperty("Name") ?? t.GetProperty("Login");
            var userNameVal = userNameProp?.GetValue(subject)?.ToString();
            if (!string.IsNullOrWhiteSpace(userNameVal))
                list.Add(new Claim("username", userNameVal!));

            return list;
        }

        private static void EnsureStandardClaims<T>(T subject, List<Claim> claims)
        {
            if (!claims.Any(c => c.Type == JwtRegisteredClaimNames.Sub))
            {
                var id = subject?.GetType().GetProperty("Id")?.GetValue(subject)?.ToString();
                if (!string.IsNullOrWhiteSpace(id))
                    claims.Add(new Claim(JwtRegisteredClaimNames.Sub, id!));
            }
            if (!claims.Any(c => c.Type == JwtRegisteredClaimNames.Jti))
                claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        }

        private sealed class ClaimEq : IEqualityComparer<Claim>
        {
           public bool Equals(Claim? x, Claim? y) => x?.Type == y?.Type && x?.Value == y?.Value;
            public int GetHashCode(Claim obj) => HashCode.Combine(obj.Type, obj.Value);
        }
    }
}
