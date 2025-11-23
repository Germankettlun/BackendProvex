using System;
using System.IO;
using System.Collections.Generic;

namespace ProvexApi.Models.GraphMail
{
    public class ProcessingOptions
    {
        public int? MessageLimit { get; set; }
        public int? StartFromBatch { get; set; }
        public int TimeoutSeconds { get; set; } = 300;
        public int MaxRetries { get; set; } = 3;
        public int BatchSize { get; set; } = 20;
        public string CsvPath { get; private set; }

        public ProcessingOptions()
        {
        }

        public void SetupCsvPath(string mailbox)
        {
            // Cambiar la ruta a wwwroot\Informes como solicita el usuario
            var baseDir = Directory.GetCurrentDirectory(); // Usar directorio actual en lugar de AppDomain
            var reportDir = Path.Combine(baseDir, "wwwroot", "Informes");
            Directory.CreateDirectory(reportDir);

            CsvPath = Path.Combine(
                reportDir,
                $"email_details_{mailbox.Replace("@", "_at_")}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
    }
}