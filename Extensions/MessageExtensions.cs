using Microsoft.Graph.Models;
using System.Linq;

namespace ProvexApi.Extensions
{
    public static class MessageExtensions
    {
        public static long GetMessageSize(this Message message)
        {
            if (message == null) return 0;

            long totalSize = 0;

            try
            {
                // 1. Tamaño del cuerpo del mensaje
                if (!string.IsNullOrEmpty(message.Body?.Content))
                {
                    totalSize += System.Text.Encoding.UTF8.GetByteCount(message.Body.Content);
                }

                // 2. Buscar tamaño en headers
                if (message.InternetMessageHeaders != null)
                {
                    var contentLength = message.InternetMessageHeaders
                        .FirstOrDefault(h => h.Name?.ToLower() == "content-length");
                    if (contentLength?.Value != null && long.TryParse(contentLength.Value, out var headerSize))
                    {
                        // Si encontramos el content-length, lo usamos en lugar del cálculo anterior
                        return headerSize;
                    }
                }

                // 3. Si no tenemos información del cuerpo o headers, usar un tamaño estimado
                if (totalSize == 0)
                {
                    totalSize = 25 * 1024; // 25KB como tamaño promedio base
                }

                return totalSize;
            }
            catch
            {
                return 25 * 1024; // Valor por defecto en caso de error
            }
        }

        public static bool HasContent(this Message message)
        {
            return message != null && 
                   (!string.IsNullOrEmpty(message.Body?.Content) || 
                    message.HasAttachments == true);
        }

        public static long? GetSize(this Message message)
        {
            if (message == null) return null;

            // Intentar obtener el tamaño de las propiedades del mensaje
            try
            {
                // Usar reflexión para acceder a la propiedad Size si existe
                var sizeProperty = message.GetType().GetProperty("Size");
                if (sizeProperty != null)
                {
                    return (long?)sizeProperty.GetValue(message);
                }

                // Si no hay propiedad Size, intentar obtener del body
                var bodySize = 0L;
                if (message.Body?.Content != null)
                {
                    bodySize = System.Text.Encoding.UTF8.GetByteCount(message.Body.Content);
                }

                return bodySize;
            }
            catch
            {
                return 0;
            }
        }
    }
}