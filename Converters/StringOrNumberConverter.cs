using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProvexApi.Converters
{
    /// <summary>
    /// Converter robusto: Convierte cualquier número (int, long, double) o string en string, y soporta null.
    /// </summary>
    public class NumericToStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
                return reader.GetString();

            if (reader.TokenType == JsonTokenType.Number)
            {
                // Primero intenta long, luego double
                if (reader.TryGetInt64(out long l))
                    return l.ToString();
                if (reader.TryGetDouble(out double d))
                    return d.ToString("0.##");
            }

            // Si llega aquí, retorna null seguro
            return null;
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
