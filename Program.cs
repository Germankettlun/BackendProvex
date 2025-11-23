using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProvexApi.Configuration;
using ProvexApi.Data;
using ProvexApi.Data.Rossi;
using ProvexApi.Filters;
using ProvexApi.Handlers;
using ProvexApi.Helper;
using ProvexApi.Services;
using ProvexApi.Services.Integrations;
using ProvexApi.Services.Logs;
using ProvexApi.Services.Reports;
using ProvexApi.Services.Token;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ProvexApi.Services.Reports;
using System.Linq; // para Reverse() sobre providers
// NUEVO: Imports para SensiWatch
using ProvexApi.Services.SensiWatch;
using ProvexApi.Services.SensiWatch.API;
using ProvexApi.Models.SensiWatch.API;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// =============================================
// 1. CONFIGURACIÓN DE LOGGING
// =============================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

#pragma warning disable ASP0013
// ---------------------------------------------
// 2) CONFIGURAR FUENTES DE CONFIGURACIÓN
// ---------------------------------------------
builder.Host.ConfigureAppConfiguration((ctx, cfg) =>
{
    // 1.a) appsettings.json + envvars
    cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
       .AddEnvironmentVariables();

    // 1.b) User Secrets en Development
    if (ctx.HostingEnvironment.IsDevelopment())
        cfg.AddUserSecrets<Program>(optional: true, reloadOnChange: true);

    // 1.c) Build temporal para leer cosas antes de inyectar
    var tmp = (IConfigurationRoot)cfg.Build();

    // Helper para identificar el proveedor que resolvió la clave
    static string? ResolveKey(IConfigurationRoot root, string key, out string providerTypeName)
    {
        providerTypeName = string.Empty;
        foreach (var p in root.Providers.Reverse())
        {
            if (p.TryGet(key, out var v) && !string.IsNullOrWhiteSpace(v))
            {
                providerTypeName = p.GetType().Name; // e.g. JsonConfigurationProvider, EnvironmentVariablesConfigurationProvider, UserSecretsConfigurationProvider
                return v;
            }
        }
        return null;
    }

    try
    {
        // Claves diferenciadas por entorno
        var env = ctx.HostingEnvironment.EnvironmentName;
        var provexKey = $"ConnectionProvexStrings:DatabaseConnection:{env}";
        var extranetKey = $"ConnectionExtranetStrings:DatabaseConnection:{env}";

        // Añadir fallback a ConnectionStrings:DatabaseConnection (legacy)
        string sourceProvex = string.Empty;
        var provexConn = ResolveKey(tmp, provexKey, out sourceProvex)
                          ?? ResolveKey(tmp, "ConnectionProvexStrings:DatabaseConnection", out sourceProvex)
                          ?? ResolveKey(tmp, "ConnectionStrings:DatabaseConnection", out sourceProvex);

        string sourceExtranet = string.Empty;
        var extranetConn = ResolveKey(tmp, extranetKey, out sourceExtranet)
                           ?? ResolveKey(tmp, "ConnectionExtranetStrings:DatabaseConnection", out sourceExtranet)
                           ?? ResolveKey(tmp, "ConnectionStrings:ExtranetDb", out sourceExtranet);

        // LOG DEL STRING DE CONEXIÓN
        Console.WriteLine($"[DEBUG] String de conexión usado para configuración: {provexConn}");
        if (!string.IsNullOrWhiteSpace(sourceProvex) && sourceProvex.Contains("UserSecrets", StringComparison.OrdinalIgnoreCase))
        {
            LogHelper.Log("ConnectionStrings:DatabaseConnection resuelto desde UserSecrets.", "Configuración");
        }

        // 1.d) Inyectar la configuración JSON de la BD (incluye Jwt:Issuer, etc.)
        if (!string.IsNullOrWhiteSpace(provexConn))
        {
            cfg.Add(new DbJsonConfigurationSource(
                connectionString: provexConn,
                environment: ctx.HostingEnvironment.EnvironmentName
            ));
        }
        else
        {
            LogHelper.Log("DbJsonConfigurationSource omitido: no se resolvió ConnectionString para configuración en BD.", "Configuración");
        }

        // 1.e) Finalmente, sobreescribir solo los connection strings
        var dict = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(provexConn))
            dict["ConnectionStrings:ProvexDb"] = provexConn;
        if (!string.IsNullOrWhiteSpace(extranetConn))
            dict["ConnectionStrings:ExtranetDb"] = extranetConn;
        if (dict.Count > 0)
            cfg.AddInMemoryCollection(dict);
    }
    catch (Exception ex)
    {
        LogHelper.Log($"❌ Error cargando conexiones desde config BD: {ex.Message}", "Configuración");
        throw;
    }

    Console.WriteLine($"[DEBUG] Entorno de configuración: {ctx.HostingEnvironment.EnvironmentName}");
});
#pragma warning restore ASP0013

// ----------------------------------------------------
// 3) REGISTRAR DbContext (CORREGIDO)
// ----------------------------------------------------

builder.Services.AddDbContext<ProvexDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("ProvexDb"))
    .UseLoggerFactory(LoggerFactory.Create(loggingBuilder =>
    {
        // Filtro específico para comandos SQL
        loggingBuilder.AddFilter(DbLoggerCategory.Database.Command.Name, LogLevel.Warning);

        // Filtro general para todo EF Core
        loggingBuilder.AddFilter((category, level) =>
            category.StartsWith("Microsoft.EntityFrameworkCore") &&
            level >= LogLevel.Warning
        );
    })) // <-- PARÉNTESIS AÑADIDO AQUÍ
);

builder.Services.AddDbContext<RossiDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("ProvexDb"))
       .UseLoggerFactory(LoggerFactory.Create(loggingBuilder =>
       {
           // Puedes usar los mismos filtros que en ProvexDbContext
           loggingBuilder.AddFilter(DbLoggerCategory.Database.Command.Name, LogLevel.Warning);
           loggingBuilder.AddFilter((category, level) =>
               category.StartsWith("Microsoft.EntityFrameworkCore") &&
               level >= LogLevel.Warning);
       }))
);

builder.Services.AddDbContext<ExtranetDBContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("ExtranetDb"))
       .UseLoggerFactory(LoggerFactory.Create(loggingBuilder =>
       {
           // Puedes usar los mismos filtros que en ProvexDbContext
           loggingBuilder.AddFilter(DbLoggerCategory.Database.Command.Name, LogLevel.Warning);
           loggingBuilder.AddFilter((category, level) =>
               category.StartsWith("Microsoft.EntityFrameworkCore") &&
               level >= LogLevel.Warning);
       }))
);


// ----------------------------------------------------
// 4) RESTO DE SERVICIOS COMUNES
// ----------------------------------------------------
if (builder.Environment.IsDevelopment())
{
    // Solo HTTP en desarrollo para evitar certificados dev
    builder.WebHost.ConfigureKestrel(o =>
    {
        o.ListenAnyIP(5000);
    });
}

// CORS: permitir origen de tu React/Vite según entorno
// -------------------------------------------------------------
if (builder.Environment.IsDevelopment())
{
    // En desarrollo permitimos localhost:3000
    builder.Services.AddCors(opt =>
    {
        opt.AddPolicy("AllowReact", policy =>
            policy.WithOrigins("http://localhost:3001", "http://localhost:3002")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials());
    });
}
else
{
    // En producción permitimos sólo tu dominio real
    builder.Services.AddCors(opt =>
    {
        opt.AddPolicy("AllowReact", policy =>
            policy.WithOrigins("https://api.provexsa.cl")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials());
    });
}

// HttpClient genérico
builder.Services.AddHttpClient();

// HttpClient específico para ROPC contra Azure AD (usado por BasicAuthenticationHandler)
builder.Services.AddHttpClient("aad-ropc", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<IAsoexService, AsoexService>();
builder.Services.AddScoped<IAsoexMaestrosService, AsoexMaestrosService>();
builder.Services.AddScoped<ApiCallLogger>();
builder.Services.AddScoped<ApiCallLoggingFilter>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddTransient<ProvexApi.Services.SDT.TemporadaService>();
builder.Services.AddScoped<EmbarquesReportService>();
builder.Services.AddScoped<ISharePointService, SharePointService>();

builder.Services.AddHttpClient("graph");
builder.Services.AddScoped<ISharePointService, SharePointService>();

builder.Services.AddScoped<ProvexApi.Services.Lab.LabResiduoImportService>();
builder.Services.AddScoped<ProvexApi.Services.GraphMail.IGraphMailService, ProvexApi.Services.GraphMail.GraphMailService>();

// NUEVO: Registro de servicios SensiWatch
// ========================================
// Configuración de opciones para SensiWatch API
builder.Services.Configure<SensiWatchApiOptions>(
    builder.Configuration.GetSection("SensiWatch"));

// HttpClient específico para SensiWatch con configuraciones personalizadas
builder.Services.AddHttpClient<ISensiWatchApiClient, SensiWatchApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "ProvexApi-SensiWatch/1.0");
});

// Servicios de SensiWatch
builder.Services.AddScoped<ISensiWatchService, SensiWatchService>();
builder.Services.AddScoped<ISensiWatchApiClient, SensiWatchApiClient>();

builder.Services.AddControllers(opts =>
{
    opts.Filters.AddService<ApiCallLoggingFilter>();
}).AddJsonOptions(options =>
{
    // Configurar para manejar valores especiales como infinito y NaN
    options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
    options.JsonSerializerOptions.PropertyNamingPolicy = null; // Mantener nombres originales
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProvexApi", Version = "v1" });

    // NUEVO: Documentación para SensiWatch endpoints
    if (builder.Environment.IsDevelopment())
    {
        c.AddServer(new OpenApiServer
        {
            Url = "https://localhost:5001",
            Description = "Development Server"
        });
    }

    // Configurar seguridad para endpoints de SensiWatch
    c.AddSecurityDefinition("BasicAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        Description = "HTTP Basic Authentication for SensiWatch push endpoints"
    });
});

// ----------------------------------------------------
// 5) AUTENTICACIÓN + JWT (con logging mejorado)
// ----------------------------------------------------
var jwtIssuer = GetSetting("Jwt:Issuer", "Jwt:Issuer");
var jwtAudience = GetSetting("Jwt:Audience", "Jwt:Audience");
var jwtKey = GetSetting("Jwt:Key", "Jwt:Key");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = "Bearer";
        options.DefaultAuthenticateScheme = "Bearer";
        options.DefaultChallengeScheme = "Bearer";
    })
    .AddPolicyScheme("Bearer", "Basic or LocalJwt", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var hdr = context.Request.Headers["Authorization"].FirstOrDefault() ?? "";
            return hdr.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase)
                ? "Basic" : "LocalJwt";
        };
    })
    .AddJwtBearer("LocalJwt", opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        opts.Events = new JwtBearerEvents
        {
            OnTokenValidated = MyCustomValidation,
            // Reducir logging de autenticación
            OnMessageReceived = context =>
            {
                context.HttpContext.Items["JwtMessage"] = context.Token;
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    })
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", _ => { });

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("LocalOnly", policy =>
        policy.AddAuthenticationSchemes("LocalJwt").RequireAuthenticatedUser());
    opts.AddPolicy("BasicOnly", policy =>
        policy.AddAuthenticationSchemes("Basic").RequireAuthenticatedUser());
});

// ----------------------------------------------------
// 6) BUILD + PIPELINE HTTP
// ----------------------------------------------------
var app = builder.Build();

// Configuración de Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProvexApi v1");
    c.RoutePrefix = "swagger";
    // NUEVO: Información adicional para SensiWatch en Swagger
    c.DocumentTitle = "ProvexApi - Incluye integración SensiWatch";
});

app.UseCors("AllowReact");

// Evitar redirección a HTTPS en Development (si no hay certificado)
if (!builder.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("Healthy"));

// NUEVO: Health check específico para SensiWatch
app.MapGet("/health/sensiwatch", async (ISensiWatchApiClient sensiWatchClient) =>
{
    try
    {
        var isConnected = await sensiWatchClient.TestConnectionAsync();
        return isConnected
            ? Results.Ok(new { status = "healthy", sensiwatch = "connected" })
            : Results.Ok(new { status = "degraded", sensiwatch = "disconnected" });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { status = "unhealthy", sensiwatch = "error", message = ex.Message });
    }
});

// NUEVO: endpoint de diagnóstico para listar rutas registradas
app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> sources) =>
{
    var routes = sources
        .SelectMany(s => s.Endpoints)
        .OfType<RouteEndpoint>()
        .Select(e => new
        {
            pattern = e.RoutePattern.RawText,
            methods = e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? Array.Empty<string>(),
            displayName = e.DisplayName
        })
        .OrderBy(r => r.pattern)
        .ToList();
    return Results.Ok(routes);
})
.RequireAuthorization("BasicOnly");

// NUEVO: endpoint de diagnóstico para ver entorno y versión del binario
app.MapGet("/debug/env", (IWebHostEnvironment env) =>
{
    var asm = typeof(Program).Assembly;
    var asmLocation = asm.Location;
    var fileTime = System.IO.File.Exists(asmLocation) ? System.IO.File.GetLastWriteTimeUtc(asmLocation) : DateTime.MinValue;
    var proc = Process.GetCurrentProcess();
    return Results.Ok(new
    {
        environment = env.EnvironmentName,
        machine = Environment.MachineName,
        processPath = proc.MainModule?.FileName,
        startedAtUtc = proc.StartTime.ToUniversalTime(),
        assembly = new
        {
            name = asm.GetName().Name,
            version = asm.GetName().Version?.ToString(),
            location = asmLocation,
            lastWriteUtc = fileTime
        }
    });
}).RequireAuthorization("BasicOnly");

app.UseDefaultFiles();
app.UseStaticFiles();

// Middleware para logging personalizado
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    // Logging especial para endpoints de SensiWatch
    if (context.Request.Path.StartsWithSegments("/api/sensiwatch"))
    {
        logger.LogInformation("[SensiWatch] Solicitud recibida: {Method} {Path} desde {RemoteIp}",
            context.Request.Method,
            context.Request.Path,
            context.Connection.RemoteIpAddress);
    }
    else
    {
        logger.LogInformation($"Iniciando solicitud: {context.Request.Method} {context.Request.Path}");
    }

    await next();

    if (context.Request.Path.StartsWithSegments("/api/sensiwatch"))
    {
        logger.LogInformation("[SensiWatch] Respuesta enviada: {StatusCode} para {Path}",
            context.Response.StatusCode, context.Request.Path);
    }
    else
    {
        logger.LogInformation($"Finalizada solicitud: {context.Response.StatusCode}");
    }
});

app.Run();

// ----------------------------------------------------
// MÉTODOS AUXILIARES
// ----------------------------------------------------
static Task MyCustomValidation(TokenValidatedContext ctx)
{
    string rawToken = ctx.SecurityToken switch
    {
        JwtSecurityToken sjs => sjs.RawData,
        JsonWebToken mjt => mjt.EncodedToken,
        _ => (ctx.Request.Headers["Authorization"].FirstOrDefault() ?? "")
                .Substring("Bearer ".Length)
    };

    var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogDebug("Validando token JWT...");

    var db = ctx.HttpContext.RequestServices.GetRequiredService<ProvexDbContext>();
    if (!db.ApiTokens.Any(t => t.Token == rawToken && t.IsActive))
    {
        logger.LogWarning("Token revocado: {token}", rawToken);
        ctx.Fail("Token revocado");
    }

    return Task.CompletedTask;
}

string GetSetting(string key, string nombreAmigable)
{
    var value = builder.Configuration[key];
    if (!string.IsNullOrWhiteSpace(value))
    {
        return value;
    }

    var fallbackKey = key.Replace(":", "__");
    value = Environment.GetEnvironmentVariable(fallbackKey);
    if (!string.IsNullOrWhiteSpace(value))
    {
        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
        logger.LogWarning("{nombre} no en configuración, usando variable de entorno '{var}'",
                         nombreAmigable, fallbackKey);
        return value;
    }

    throw new InvalidOperationException($"Falta configuración: {nombreAmigable}");
}
