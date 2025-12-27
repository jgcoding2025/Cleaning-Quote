using System;

namespace Cleaning_Quote.Models
{
    public class QuoteOccupant
    {
        public Guid QuoteOccupantId { get; set; } = Guid.NewGuid();
        public Guid QuoteId { get; set; }
        public string Name { get; set; } = "";
        public string Relationship { get; set; } = "";
        public string Notes { get; set; } = "";
    }
}
