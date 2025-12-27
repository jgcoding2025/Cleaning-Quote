using System;

namespace Cleaning_Quote.Models
{
    public class QuotePet
    {
        public Guid QuotePetId { get; set; } = Guid.NewGuid();
        public Guid QuoteId { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Notes { get; set; } = "";
    }
}
