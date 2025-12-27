using System;

namespace Cleaning_Quote.Models
{
    public class QuoteListItem
    {
        public Guid QuoteId { get; set; }
        public DateTime QuoteDate { get; set; }
        public decimal Total { get; set; }
        public decimal Hours { get; set; }

        public string DisplayText =>
            $"{QuoteDate:MM/dd/yyyy}  |  {Hours:0.##} hrs  |  {Total:C}";
    }
}
