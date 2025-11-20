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
using ProvexBackendAPI.Features.Estimaciones.Repository;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using ProvexBackendAPI.Features.Estimaciones.Services;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;
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

builder.Services.AddScoped<ITemporadasRepository, TemporadasRepository>();

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

builder.Services.AddScoped<ISemanaVigenteProvider, SemanaVigenteProvider>();


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
            // En Development/Staging: Permitir todo (súper permisivo)
            Console.WriteLine("🔓 CORS: Modo PERMISIVO activado (Development/Staging)");
            p.AllowAnyOrigin()
             .AllowAnyHeader()
             .AllowAnyMethod();
        }
        else
        {
            // En Production: Solo orígenes específicos
            Console.WriteLine("🔒 CORS: Modo RESTRINGIDO activado (Production)");
            p.SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrEmpty(origin)) return false;

                try
                {
                    var uri = new Uri(origin);
                    var host = uri.Host;

                    // Permitir red interna 10.115.x.x
                    if (host.StartsWith("10.115.")) return true;

                    // Permitir dominios de producción específicos
                    if (host.EndsWith(".provex.com") || host == "provex.com") return true;

                    // Agregar más dominios según necesites
                    return false;
                }
                catch
                {
                    return false;
                }
            })
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials(); // Permite cookies/auth headers
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
app.UseHttpsRedirection();

// ⚠️ IMPORTANTE: UseCors() debe ir ANTES de UseAuthentication() y UseAuthorization()
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();