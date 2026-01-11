using System;
using System.Net;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;


public static class SendMailHelper
{
    public class SendMailResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public static async Task<SendMailResult> SendMailAsync(IConfiguration configuration, string integracion, string bodyContent, string _subjec)
    {
        // Parámetros de configuración para el envío de correo
        string usuario = configuration["Email:usuario"];             // Buzón remitente (ej: gkettlun@provexsa.com)
        string recipient = configuration["Email:recipient"];           // Destinatario
        // (Otros parámetros de configuración pueden usarse según tus necesidades)

        // Configuración de la aplicación para Microsoft Graph
        string tenantId = configuration["Email:tenantId"];             // Ej: "b53c62c4-9e71-4475-a0c8-abb93ec8d270"
        string clientId = configuration["Email:clientId"];             // GUID de la aplicación registrada
        string clientSecret = configuration["Email:secretSmtpApp"];      // Client secret
        string scopes = configuration["Email:scopes"];      // Client secret
        // En este flujo no se requiere redirectUri

        // Construye la aplicación confidencial para el flujo app-only
        IConfidentialClientApplication confidentialClient = ConfidentialClientApplicationBuilder
            .Create(clientId)
            .WithClientSecret(clientSecret)
            .WithTenantId(tenantId)
            .Build();

        // Definir el scope para Graph: en este caso usamos .default para obtener todos los permisos concedidos
        string[] graphScopes = new string[] { scopes };

        AuthenticationResult authResult;
        try
        {
            authResult = await confidentialClient
                .AcquireTokenForClient(graphScopes)
                .ExecuteAsync();
            Console.WriteLine("Token obtenido correctamente.");
        }
        catch (Exception ex)
        {
            string errorMsg = $"Error al obtener el token: {ex.Message}";
            Console.WriteLine(errorMsg);
            return new SendMailResult { Success = false, Message = errorMsg };
        }

        var JSonTokenDecode = DecodeTokenToJson(authResult.AccessToken);
        //logger.LogInformation("Token decodificado: {TokenJson}", JSonTokenDecode);

        // Crea el proveedor de autenticación personalizado
        var authProvider = new CustomAuthenticationProvider(() => Task.FromResult(authResult.AccessToken));

        // Crea el GraphServiceClient usando el proveedor de autenticación
        var graphClient = new GraphServiceClient(authProvider);

        // Construye el mensaje de correo
        var message = new Message
        {
            Subject = _subjec,
            Body = new ItemBody
            {
                ContentType = BodyType.Text,
                Content = bodyContent
            },
            ToRecipients = new List<Recipient>
            {
                new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = recipient
                    }
                }
            }
        };

        var sendMailRequestBody = new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
        {
            Message = message,
            SaveToSentItems = true
        };

        try
        {
           await graphClient.Users[usuario].SendMail.PostAsync(sendMailRequestBody);
            string okMsg = $"Correo enviado correctamente desde {usuario}. OK";
            Console.WriteLine(okMsg);
            return new SendMailResult { Success = true, Message = okMsg };
        }
        catch (Exception ex)
        {
            string errorMsg = $"Error al enviar el correo: {ex.Message}";
            Console.WriteLine(errorMsg);
            return new SendMailResult { Success = false, Message = errorMsg };
        }
    }

    public static string DecodeTokenToJson(string token)
    {
        // Instanciamos el manejador de tokens
        var handler = new JwtSecurityTokenHandler();

        // Leemos y decodificamos el token
        var jwtToken = handler.ReadJwtToken(token);

        // Creamos un diccionario para almacenar los claims
        var claimsDict = new Dictionary<string, object>();

        // Recorremos cada claim del token
        foreach (var claim in jwtToken.Claims)
        {
            // Si ya existe el claim, concatenamos los valores
            if (claimsDict.ContainsKey(claim.Type))
            {
                claimsDict[claim.Type] = $"{claimsDict[claim.Type]}, {claim.Value}";
            }
            else
            {
                claimsDict.Add(claim.Type, claim.Value);
            }
        }

        // Serializamos el diccionario a un JSON formateado (indentado)
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        string json = JsonSerializer.Serialize(claimsDict, jsonOptions);

        return json;
    }

    private static readonly ILogger logger = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
    }).CreateLogger("SendMailHelper");

}

public class CustomAuthenticationProvider : IAuthenticationProvider
{
    private readonly Func<Task<string>> _getTokenAsync;

    public CustomAuthenticationProvider(Func<Task<string>> getTokenAsync)
    {
        _getTokenAsync = getTokenAsync;
    }

    public async Task AuthenticateRequestAsync(HttpRequestMessage request)
    {
        var token = await _getTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
    {
        var token = await _getTokenAsync();
        request.Headers.Add("Authorization", $"Bearer {token}");
    }
}
