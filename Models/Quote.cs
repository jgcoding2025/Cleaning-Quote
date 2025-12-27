using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cleaning_Quote.Models
{
    public class Quote
    {
        public Guid QuoteId { get; set; } = Guid.NewGuid();
        public Guid ClientId { get; set; }
        public DateTime QuoteDate { get; set; } = DateTime.Today;

        public decimal LaborRate { get; set; } = 50m;
        public decimal TaxRate { get; set; } = 0.08m;
        public decimal CreditCardFeeRate { get; set; } = 0.03m;
        public bool CreditCard { get; set; } = true;

        public int PetsCount { get; set; } = 0;
        public int HouseholdSize { get; set; } = 2;
        public bool SmokingInside { get; set; } = false;

        public decimal TotalLaborHours { get; set; }
        public decimal Subtotal { get; set; }
        public decimal CreditCardFee { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }

        public string Notes { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<QuoteRoom> Rooms { get; set; } = new List<QuoteRoom>();
        public string Status { get; set; } = "Draft";

    }
}
