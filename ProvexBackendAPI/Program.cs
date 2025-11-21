using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProvexBackendAPI.Data;
using ProvexBackendAPI.Data.Models.Users;
using ProvexBackendAPI.Filters;
using ProvexBackendAPI.Infrastructure.Auth;
using ProvexBackendAPI.Middleware;
using ProvexBackendAPI.Repository;
using ProvexBackendAPI.Repository.IRepository;
using ProvexBackendAPI.Services;
using ProvexBackendAPI.Services.IServices;
using System.Text;


var builder = WebApplication.CreateBuilder(args);



// Repo + Service
// Repo
builder.Services.AddScoped<ProvexBackendAPI.Repository.IRepository.IUserRepository,
                           ProvexBackendAPI.Repository.UserRepository>();

builder.Services.AddScoped<IGenericRepository,GenericRepository>();


builder.Services.AddScoped<ProvexBackendAPI.Repository.IRepository.IUnitOfWork,
    ProvexBackendAPI.Repository.UnitOfWork>();

builder.Services.AddScoped<IDistribucionRepository, DistribucionRepository>();






// Service 
builder.Services.AddScoped<ProvexBackendAPI.Services.IServices.IUserService,
                           ProvexBackendAPI.Services.UserService>();

builder.Services.AddScoped<ProvexBackendAPI.Services.IServices.IAuthService,
                           ProvexBackendAPI.Services.AuthService>();


builder.Services.AddScoped<IComboService, ComboService>();
builder.Services.AddScoped<ITemporadasService, TemporadasService>();
builder.Services.AddScoped<IDistribucionService, DistribucionService>();
builder.Services.AddScoped<IEstimacionService, EstimacionService>();


builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<ITokenService, TokenService>();



// ===== EF Core + SQL Server =====
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers 
builder.Services.AddControllers(options =>
{
    //Filtro del middleware
    options.Filters.Add<ApiResponseWrapperFilter>();
}
);





//.NET Identity con GUID
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<Guid>>(o =>
    {
        o.User.RequireUniqueEmail = true;
        o.Password.RequiredLength = 6;
        o.Password.RequireDigit = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

//var secretKey = builder.Configuration.GetValue<String>("ApiSettings:SecretKey");
var jwt = builder.Configuration.GetSection("Jwt");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

//if (string.IsNullOrEmpty(secretKey))
//{
//    throw new InvalidOperationException("SecretKey no esta configurada");
//}

//Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
       
        ValidateIssuer = false,
        //ValidIssuer = jwt["Issuer"],
        ValidateAudience = false,            
        // ValidAudience = jwt["Audience"],   
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
}

);

//Para utilizar Auth JWT desde Swagger
builder.Services.AddSwaggerGen(
  options =>
  {
      options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
      {
          Description = "Nuestra API utiliza la Autenticación JWT usando el esquema Bearer. \n\r\n\r" +
                      "Ingresa la palabra a continuación el token generado en login.\n\r\n\r" +
                      "Ejemplo: \"12345abcdef\"",
          Name = "Authorization",
          In = ParameterLocation.Header,
          Type = SecuritySchemeType.Http,
          Scheme = "Bearer"
      });
      options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
      {
        new OpenApiSecurityScheme
        {
          Reference = new OpenApiReference
          {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
          },
          Scheme = "oauth2",
          Name = "Bearer",
          In = ParameterLocation.Header
        },
        new List<string>()
      }
    });
      //Versionamiento API Swagger
      //V1
      options.SwaggerDoc("v1", new OpenApiInfo
      {
          Version = "v1",
          Title = "API Provex Back",
          Description = "API para gestionar back",
          // TermsOfService = new Uri("http://example.com/terms"),
          //Contact = new OpenApiContact
          //{
          //    Name = "Provex",
          //    Url = new Uri("")
          //},
          //License = new OpenApiLicense()
          //{
          //    Name = "Licencia de uso",
          //    Url = new Uri("")
          //}
      }

       );


  }
);
//Versionamiento API
var apiVersioningBuilder = builder.Services.AddApiVersioning(option =>
{
    option.AssumeDefaultVersionWhenUnspecified = true;
    option.DefaultApiVersion = new ApiVersion(1, 0);
    option.ReportApiVersions = true;

}
);
//Versionamiento API Swagger
apiVersioningBuilder.AddApiExplorer(option =>
{
    option.GroupNameFormat = "'v'VVV"; //v2, v2,v3...
    option.SubstituteApiVersionInUrl = true; //api/v{version}/products

}
);

// ===== CORS - Configuración Flexible por Entorno =====
builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p =>
    {
        if (builder.Environment.IsDevelopment() ||
            builder.Environment.EnvironmentName == "Staging")
        {
            // ========================================
            // DEVELOPMENT / STAGING: Modo permisivo
            // ========================================
            // Permite cualquier origen, método y header
            // Útil para desarrollo local y pruebas
            Console.WriteLine("🔓 CORS: Modo PERMISIVO activado (Development/Staging)");
            p.AllowAnyOrigin()
             .AllowAnyHeader()
             .AllowAnyMethod();
        }
        else
        {
            // ========================================
            // PRODUCTION: Modo restringido
            // ========================================
            // Solo permite orígenes específicos por seguridad
            Console.WriteLine("🔒 CORS: Modo RESTRINGIDO activado (Production)");
            p.SetIsOriginAllowed(origin =>
            {
                // Validar que el origen no sea nulo o vacío
                if (string.IsNullOrEmpty(origin)) 
                {
                    Console.WriteLine("⚠️ CORS: Origen vacío o nulo - RECHAZADO");
                    return false;
                }

                try
                {
                    var uri = new Uri(origin);
                    var host = uri.Host;
                    var scheme = uri.Scheme;

                    // Log para debugging: muestra cada petición evaluada
                    Console.WriteLine($"🌍 CORS: Evaluando origen: {origin}");
                    Console.WriteLine($"   └─ Host: {host} | Scheme: {scheme} | Port: {uri.Port}");

                    // ========================================
                    // REGLA 1: Red interna 10.115.x.x
                    // ========================================
                    // Permite cualquier IP de la red interna (HTTP o HTTPS)
                    // Ejemplo: http://10.115.1.253:3000
                    if (host.StartsWith("10.115."))
                    {
                        Console.WriteLine($"   ✅ PERMITIDO - Red interna 10.115.x.x");
                        return true;
                    }

                    // ========================================
                    // REGLA 2: Localhost (desarrollo local)
                    // ========================================
                    // Permite peticiones desde localhost en cualquier puerto
                    // Útil para desarrolladores trabajando localmente
                    if (host == "localhost" || host == "127.0.0.1")
                    {
                        Console.WriteLine($"   ✅ PERMITIDO - Localhost");
                        return true;
                    }

                    // ========================================
                    // REGLA 3: Dominios de producción (solo HTTPS)
                    // ========================================
                    // Solo permite dominios públicos si usan HTTPS por seguridad
                    if (scheme == "https")
                    {
                        // Dominio principal: provexsa.cl
                        if (host.EndsWith(".provexsa.cl") || host == "provexsa.cl")
                        {
                            Console.WriteLine($"   ✅ PERMITIDO - Dominio provexsa.cl (HTTPS)");
                            return true;
                        }

                        // Dominio alternativo: provex.com
                        if (host.EndsWith(".provex.com") || host == "provex.com")
                        {
                            Console.WriteLine($"   ✅ PERMITIDO - Dominio provex.com (HTTPS)");
                            return true;
                        }
                    }

                    // ========================================
                    // Origen no permitido
                    // ========================================
                    Console.WriteLine($"   ❌ RECHAZADO - No cumple con ninguna regla");
                    return false;
                }
                catch (Exception ex)
                {
                    // Error al parsear el origen
                    Console.WriteLine($"⚠️ CORS: Error al validar origen '{origin}': {ex.Message}");
                    return false;
                }
            })
             .AllowAnyHeader()        // Permite cualquier header (Authorization, Content-Type, etc.)
             .AllowAnyMethod()        // Permite todos los métodos HTTP (GET, POST, PUT, DELETE, etc.)
             .AllowCredentials();     // Permite envío de cookies y headers de autenticación
        }
    });
});


var app = builder.Build();

// Mostrar entorno actual en los logs
Console.WriteLine($"🚀 Iniciando API en entorno: {app.Environment.EnvironmentName}");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        // options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
    });
}

app.UseMiddleware<ExceptionMiddleware>();

// ========================================
// ⚠️ ORDEN CRÍTICO DEL PIPELINE DE MIDDLEWARE
// ========================================
// 1. CORS debe ir ANTES de HTTPS Redirection
//    Esto permite que las peticiones OPTIONS (preflight) reciban
//    los headers CORS sin ser redirigidas primero
app.UseCors();

// 2. HTTPS Redirection después de CORS
//    Las redirecciones HTTPS se aplicarán después de validar CORS
app.UseHttpsRedirection();

// 3. Authentication y Authorization al final
//    Se ejecutan después de CORS y redirecciones
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
