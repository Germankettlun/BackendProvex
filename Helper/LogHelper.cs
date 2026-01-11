using System;
using System.IO;

namespace ProvexApi.Helper
{
    public static class LogHelper
    {
        private static readonly string LogPath;

        static LogHelper()
        {
            try
            {
                var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                Directory.CreateDirectory(folder);  // ✅ Crea la carpeta si no existe

                LogPath = Path.Combine(folder, $"provex-log-{DateTime.UtcNow:yyyyMMdd}.log");
            }
            catch
            {
                LogPath = "provex-fallback.log";  // Carpeta no accesible
            }
        }

        public static void Log(string message, string? context = null)
        {
            try
            {
                var line = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} [LOG] {(context != null ? "[" + context + "] " : "")}{message}";
                File.AppendAllText(LogPath, line + Environment.NewLine);
            }
            catch
            {
                // Falla silenciosamente si no puede escribir
            }

#if DEBUG
            Console.WriteLine(message);
#endif
        }
    }
}
