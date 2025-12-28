using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Cleaning_Quote.Models
{
    public class QuoteRoom : INotifyPropertyChanged
    {
        public Guid QuoteRoomId { get; set; } = Guid.NewGuid();
        public Guid QuoteId { get; set; }
        public Guid? ParentRoomId { get; set; }
        private int _sortOrder;
        private decimal _roomLaborHours;
        private decimal _roomAmount;
        private bool _includedInQuote = true;

        public string RoomType { get; set; } = "Bedroom";
        public string Size { get; set; } = "M";   // S/M/L
        public int Complexity { get; set; } = 1;  // 1/2/3

        public string Level { get; set; } = "";
        public string ItemCategory { get; set; } = "";
        public bool IsSubItem { get; set; }
        public bool IncludedInQuote
        {
            get => _includedInQuote;
            set
            {
                if (_includedInQuote == value)
                    return;
                _includedInQuote = value;
                OnPropertyChanged();
            }
        }
        public int SortOrder
        {
            get => _sortOrder;
            set
            {
                if (_sortOrder == value)
                    return;
                _sortOrder = value;
                OnPropertyChanged();
            }
        }
        public bool IsWindowRoom =>
            !string.IsNullOrWhiteSpace(RoomType) &&
            RoomType.IndexOf("Window", StringComparison.OrdinalIgnoreCase) >= 0;

        public decimal RoomLaborHours
        {
            get => _roomLaborHours;
            set
            {
                if (_roomLaborHours == value)
                    return;
                _roomLaborHours = value;
                OnPropertyChanged();
            }
        }

        public decimal RoomAmount
        {
            get => _roomAmount;
            set
            {
                if (_roomAmount == value)
                    return;
                _roomAmount = value;
                OnPropertyChanged();
            }
        }
        public string RoomNotes { get; set; } = "";

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
