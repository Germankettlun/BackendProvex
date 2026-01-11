using System;

namespace ProvexApi.Models.GraphMail
{
    public class EmailMetadata
    {
        public string Id { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;
        public string Mailbox { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public long Size { get; set; }
        public DateTime ReceivedDate { get; set; }
        public DateTime ProcessedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}