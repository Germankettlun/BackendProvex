using System;

namespace ProvexApi.Models.GraphMail
{
    public class GraphMailResult
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string Sender { get; set; } = string.Empty;
        public long TotalSize { get; set; }
        public int Count { get; set; }
        public double TotalSizeKb => TotalSize / 1024.0;
        public double TotalSizeMb => TotalSizeKb / 1024.0;
        
        // Propiedades de procesamiento
        public int BatchNumber { get; set; }
        public double ProcessingRate { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public int FailedMessages { get; set; }
        
        // Para compatibilidad con análisis temporal
        public string FormattedDate => ReceivedDate?.ToString("yyyy-MM-dd") ?? $"{Year:0000}-{Month:00}-{Day:00}";
        public string YearMonth => ReceivedDate?.ToString("yyyy-MM") ?? $"{Year:0000}-{Month:00}";
    }
}