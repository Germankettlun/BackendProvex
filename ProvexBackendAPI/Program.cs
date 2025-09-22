using Asp.Versioning;
using AutoMapper;
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

builder.Services.AddScoped(
    typeof(ProvexBackendAPI.Repository.IRepository.IGenericRepository<>),
    typeof(ProvexBackendAPI.Repository.GenericRepository<>));

builder.Services.AddScoped<ProvexBackendAPI.Repository.IRepository.IUnitOfWork,
    ProvexBackendAPI.Repository.UnitOfWork>();


// Service 
builder.Services.AddScoped<ProvexBackendAPI.Services.IServices.IUserService,
                           ProvexBackendAPI.Services.UserService>();

builder.Services.AddScoped<ProvexBackendAPI.Services.IServices.IAuthService,
                           ProvexBackendAPI.Services.AuthService>();

builder.Services.AddScoped(
    typeof(ProvexBackendAPI.Services.IServices.IGenericService<>),
    typeof(ProvexBackendAPI.Services.GenericService<>));


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



//AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.LicenseKey = "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxNzkwMDM1MjAwIiwiaWF0IjoiMTc1ODU2NTg3NyIsImFjY291bnRfaWQiOiIwMTk5NzJiMTY4YjM3YmVmYWFhNjVkMTdhMDcwNzIzMiIsImN1c3RvbWVyX2lkIjoiY3RtXzAxazVzYjNqNHAzdmI0bjJmNHZnODBrZXh3Iiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.XoMSMkGP_0koJYvE5Jdh0JRoFB4kxnVfOWi1fi6lWaucb0AnLEyJwp7TXpZh5r7HoM-xRVyNrMt0flXObNKIblPtYFefpHMvr6dA0V8JXoyw_81NWnGE22aI7wXpg79OgiohUWBMvsavHJhiE_zcs1VGZqmUHiM7zLX6rmG90UZl-8-xgnkJFnUVSkyplk6PxAxNHXXCbzfl4OnykgQDK2WhfvNldw6ui9YsQ6ye_YUnO5Xg2BdVu9Hu70pEl_g_pH2JLUrJFZqVA1i1lJM4kc8tFWBNwY3HarApO-tYtgB9jLqdrHUNCiaXYcF_SoKudg5g50a-wz2nFfDCRNQsdA";
}, AppDomain.CurrentDomain.GetAssemblies());


//:NET Identity con GUID
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
        //ValidateIssuerSigningKey = true,
        ////IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        //IssuerSigningKey = signingKey,
        //ValidateIssuer = false,
        //ValidateAudience = false
        ValidateIssuer = true,
        ValidIssuer = jwt["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwt["Audience"],
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

//CORS 
builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p => p
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithOrigins("http://localhost:3000", "http://localhost:5173"));
});


var app = builder.Build();

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
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); 

app.Run();
