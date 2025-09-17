using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProvexBackendAPI.Infrastructure.Auth;
using ProvexBackendAPI.Services.IServices;
using ProvexBackendAPI.Services.IServices.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ProvexBackendAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwt;
        private readonly SymmetricSecurityKey _signingKey;

        public TokenService(IOptions<JwtSettings> jwtOptions)
        {
            _jwt = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
            if (string.IsNullOrWhiteSpace(_jwt.Key))
                throw new InvalidOperationException("Jwt:Key no está configurado");
            _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        }

        public async Task<AccessTokenResult> GenerateAsync<T>(
        T subject,
        Func<T, Task<IEnumerable<string>>>? rolesProvider = null,
        IEnumerable<ClaimMap<T>>? maps = null,
        IEnumerable<Claim>? extraClaims = null)
        {
            if (subject is null) throw new ArgumentNullException(nameof(subject));

            var claims = new List<Claim>();

            // 1) Atributos
            claims.AddRange(BuildClaimsFromAttributes(subject));

            // 2) Convención: Id, Email, UserName/Username/Name/Login
            claims.AddRange(BuildClaimsByConvention(subject));

            // 3) Map fluido
            if (maps is not null)
            {
                foreach (var map in maps)
                {
                    var v = map.Selector(subject);
                    if (string.IsNullOrWhiteSpace(v) && map.SkipIfNullOrEmpty) continue;
                    claims.Add(new Claim(map.Type, v ?? string.Empty));
                }
            }

            // 4) Extras explícitos
            if (extraClaims is not null) claims.AddRange(extraClaims);

            // 5) Roles
            if (rolesProvider is not null)
            {
                var roles = await rolesProvider(subject);
                if (roles is not null)
                {
                    foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));
                }
            }

            // 6) sub/jti si faltan
            EnsureStandardClaims(subject, claims);

            var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            var minutes = _jwt.AccessTokenMinutes <= 0 ? 120 : _jwt.AccessTokenMinutes;
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(minutes);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims.Distinct(new ClaimEq())),
                Expires = expiresAtUtc,
                Issuer = _jwt.Issuer,
                Audience = _jwt.Audience,
                SigningCredentials = creds
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(descriptor);
            var tokenString = handler.WriteToken(token);

            return new AccessTokenResult(
                Token: tokenString,
                ExpiresAtUtc: expiresAtUtc,
                ExpiresAtUnix: new DateTimeOffset(expiresAtUtc).ToUnixTimeSeconds()
            );
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
