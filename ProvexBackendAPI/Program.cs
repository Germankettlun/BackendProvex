using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
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
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// Evitar duplicados: limpiar proveedores por defecto al usar Serilog
builder.Logging.ClearProviders();

// Forzar UTF-8 en salida de consola (evita “respondi¢” en Windows)
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

// ===== Serilog =====
var environmentName = builder.Environment.EnvironmentName;
// Normalizar nombre de carpeta: Development -> dev, Production -> prod, Staging -> staging
var envFolder = environmentName.Equals("Development", StringComparison.OrdinalIgnoreCase)
    ? "dev"
    : environmentName.Equals("Production", StringComparison.OrdinalIgnoreCase)
        ? "prod"
        : environmentName.ToLowerInvariant();
var logDir = Path.Combine("Logs", envFolder);
Directory.CreateDirectory(logDir);
var logFilePath = Path.Combine(logDir, $"provex-api-{envFolder}-.log");

// Configura Serilog: lee appsettings y asegura sinks de consola y archivo por entorno
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// ===== Repo + Service
builder.Services.AddScoped<ProvexBackendAPI.Repository.IRepository.IUserRepository,
                           ProvexBackendAPI.Repository.UserRepository>();
builder.Services.AddScoped<IGenericRepository, GenericRepository>();
builder.Services.AddScoped<ProvexBackendAPI.Repository.IRepository.IUnitOfWork,
    ProvexBackendAPI.Repository.UnitOfWork>();

// Service 
builder.Services.AddScoped<ProvexBackendAPI.Services.IServices.IUserService,
                           ProvexBackendAPI.Services.UserService>();
builder.Services.AddScoped<ProvexBackendAPI.Services.IServices.IAuthService,
                           ProvexBackendAPI.Services.AuthService>();
builder.Services.AddScoped<IComboService, ComboService>();
builder.Services.AddScoped<ITemporadasService, TemporadasService>();
builder.Services.AddScoped<ICierreService, CierreService>();
builder.Services.AddScoped<IDistribucionService, DistribucionService>();
builder.Services.AddScoped<IEstimacionService, EstimacionService>();
builder.Services.AddScoped<IComercial, ComercialService>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<ITokenService, TokenService>();

// ===== EF Core + SQL Server =====
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
#if DEBUG
    options.EnableSensitiveDataLogging();
#endif
    options.LogTo(message => Log.Information("[EF] {Message}", message), LogLevel.Information);
});

// Controllers 
builder.Services.AddControllers(options =>
{
    //Filtro del middleware
    options.Filters.Add<ApiResponseWrapperFilter>();
    options.Filters.Add(new RequestLoggingActionFilter());
});

// .NET Identity con GUID
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

// Authentication (sin eventos extra para no alterar comportamiento)
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
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Swagger (sin cambios de esquema)
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
          Description = "API para gestionar back"
      });
  }
);

// Versionamiento API
var apiVersioningBuilder = builder.Services.AddApiVersioning(option =>
{
    option.AssumeDefaultVersionWhenUnspecified = true;
    option.DefaultApiVersion = new ApiVersion(1, 0);
    option.ReportApiVersions = true;
});
// Versionamiento API Swagger
apiVersioningBuilder.AddApiExplorer(option =>
{
    option.GroupNameFormat = "'v'VVV";
    option.SubstituteApiVersionInUrl = true;
});

// ===== CORS - Configuración Flexible por Entorno =====
builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p =>
    {
        if (builder.Environment.IsDevelopment() ||
            builder.Environment.EnvironmentName == "Staging")
        {
            Console.WriteLine("🔓 CORS: Modo PERMISIVO activado (Development/Staging)");
            //p.AllowAnyOrigin()
            //p.WithOrigins("https://localhost:3000")
            p
            .SetIsOriginAllowed(origin => true)
             .AllowAnyHeader()
             .WithExposedHeaders("X-XSRF-TOKEN")
             .AllowCredentials()
             .AllowAnyMethod();
        }
        else
        {
            Console.WriteLine("🔒 CORS: Modo RESTRINGIDO activado (Production)");
            var allowedOrigins = new[]
            {
                "https://intranet.provexsa.com",
                "https://provexsa.cl",
                "https://www.provexsa.cl"
            };
            p.SetIsOriginAllowed(origin => true)
             .AllowAnyHeader()
             .WithExposedHeaders("X-XSRF-TOKEN")
             .AllowCredentials()
             .AllowAnyMethod();
        }
    });
});

builder.Services.AddAntiforgery(options => {
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.SameSite = 0; //SameSiteMode.Lax
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; //CookieSecurePolicy.SameAsRequest
});

var app = builder.Build();

// Enriquecimiento de RequestId y User para todos los logs
app.Use(async (context, next) =>
{
    var requestId = context.TraceIdentifier;
    var user = context.User?.Identity?.IsAuthenticated == true
        ? (context.User.Identity?.Name
            ?? context.User.FindFirst("sub")?.Value
            ?? context.User.FindFirst("id")?.Value
            ?? "Authenticated")
        : "Anonymous";

    using (Serilog.Context.LogContext.PushProperty("RequestId", requestId))
    using (Serilog.Context.LogContext.PushProperty("User", user))
    {
        await next();
    }
});

// Mostrar entorno actual en los logs
Log.Information("🚀 [STARTUP] API iniciada en entorno: {Env}", environmentName);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}

// Serilog de requests HTTP (no cambia respuesta)
app.UseSerilogRequestLogging(opts =>
{
    opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000} ms";
});

app.UseMiddleware<ExceptionMiddleware>();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Orden crítico del pipeline
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();

app.Run();
