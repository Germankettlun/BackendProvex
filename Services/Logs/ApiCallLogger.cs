using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProvexApi.Data;
using ProvexApi.Helper;
using ProvexApi.Models;
using ProvexApi.Models.BackEnd.Logs;

namespace ProvexApi.Services.Logs
{
    public class ApiCallLogger
    {
        private readonly ProvexDbContext _db;
        public ApiCallLogger(ProvexDbContext db) => _db = db;

        public async Task LogAsync(
            string apiName,
            string httpMethod,
            string url,
            int statusCode,
            bool isSuccess,
            bool authSuccess,  // [ADDED]
            int durationMs,
            string responseBody,
            string? userName,      // ← parámetro nuevo
            string? clientIp,       // ← parámetro nuevo
            string? token
        )
        {
            try
            {
                // snippet de hasta 1000 caracteres
                var snippet = responseBody?.Length > 1000
                    ? responseBody.Substring(0, 1000)
                    : responseBody;

                var log = new ServiceApiCallLog
                {
                    Timestamp = DateTime.UtcNow,
                    ApiName = apiName,
                    HttpMethod = httpMethod,
                    RequestUrl = url,
                    StatusCode = statusCode,
                    IsSuccess = isSuccess,
                    AuthSuccess = authSuccess,
                    DurationMs = durationMs,
                    ResponseSize = responseBody?.Length ?? 0,
                    Snippet = snippet,
                    UserName = userName,   // ← asignar aquí
                    ClientIp = clientIp,    // ← y aquí
                    token = token    // ← y aquí
                };

                _db.ServiceApiCallLogs.Add(log);
                //LogHelper.Log("💬 Log : Cadena usada por DbContext: " + _db.Database.GetDbConnection().ConnectionString, "LogsBd");                
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                LogHelper.Log("Error al insertar log : " + ex.Message, "LogsBd");
                throw new Exception("Error al insertar log : " + ex.Message);
            }
        }
    }
}
