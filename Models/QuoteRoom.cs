using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cleaning_Quote.Models
{
    public class QuoteRoom
    {
        public Guid QuoteRoomId { get; set; } = Guid.NewGuid();
        public Guid QuoteId { get; set; }
        public Guid? ParentRoomId { get; set; }

        public string RoomType { get; set; } = "Bedroom";
        public string Size { get; set; } = "M";   // S/M/L
        public int Complexity { get; set; } = 1;  // 1/2/3

        public string Level { get; set; } = "";
        public string ItemCategory { get; set; } = "";
        public bool IsSubItem { get; set; }
        public bool IncludedInQuote { get; set; } = true;
        public bool WindowInside { get; set; }
        public bool WindowOutside { get; set; }

        public int? FullGlassShowersCount { get; set; } = 0;
        public int? PebbleStoneFloorsCount { get; set; } = 0;
        public int? FridgeCount { get; set; } = 0;
        public int? OvenCount { get; set; } = 0;

        public decimal RoomLaborHours { get; set; }
        public decimal RoomAmount { get; set; }
        public string RoomNotes { get; set; } = "";
    }
}
