using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cleaning_Quote.Models
{
    public class Client
    {
        public Guid ClientId { get; set; } = Guid.NewGuid();

        public string DisplayName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }
        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
