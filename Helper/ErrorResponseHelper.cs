using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using static SendMailHelper;

namespace ProvexApi.Helper
{
    public static class ErrorResponseHelper
    {
        /// <summary>
        /// Maneja una excepción enviando una notificación por correo y devolviendo una respuesta HTTP 500.
        /// </summary>
        /// <param name="ex">La excepción ocurrida.</param>
        /// <param name="configuration">La configuración de la aplicación.</param>
        /// <param name="context">Una cadena que identifica el contexto o proceso (por ejemplo, "Existencia a proceso").</param>
        /// <returns>Un IActionResult con el error, el resultado de la notificación y el código de estado 500.</returns>
        public static async Task<IActionResult> HandleExceptionAsync(Exception ex, IConfiguration configuration, string context)
        {
            // Llama a SendMailHelper para notificar el error vía correo.
            var mailResult = await SendMailHelper.SendMailAsync(
                configuration,
                context,
                $"Ocurrio un problema al procesar el servicio {context}: {Environment.NewLine} {ex.Message}",
                "Error en Backend en servicio : " + context);

            // Si por alguna razón mailResult es null, se asigna un valor predeterminado.
            if (mailResult == null)
            {
                mailResult = new SendMailResult
                {
                    Success = false,
                    Message = "No se pudo enviar la notificación (mailResult es null)."
                };
            }

            // Construye el objeto de respuesta con los detalles.
            var errorResponse = new
            {
                Error = $"Error llamando {context}: {ex.Message}",
                Notificacion = mailResult.Message,
                Enviado = mailResult.Success
            };

            // Devuelve un ObjectResult con código de estado 500.
            return new ObjectResult(errorResponse)
            {
                StatusCode = 500
            };
        }
    }
}
