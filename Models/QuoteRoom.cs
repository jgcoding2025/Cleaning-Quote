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
        private string _windowSideSelection = "";
        private int _sortOrder;
        private decimal _roomLaborHours;
        private decimal _roomAmount;

        public string RoomType { get; set; } = "Bedroom";
        public string Size { get; set; } = "M";   // S/M/L
        public int Complexity { get; set; } = 1;  // 1/2/3

        public string Level { get; set; } = "";
        public string ItemCategory { get; set; } = "";
        public bool IsSubItem { get; set; }
        public bool IncludedInQuote { get; set; } = true;
        public bool WindowInside { get; set; }
        public bool WindowOutside { get; set; }
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
        public string WindowSideSelection
        {
            get => _windowSideSelection;
            set
            {
                var normalized = NormalizeWindowSideSelection(value);
                _windowSideSelection = normalized;
                ApplyWindowSideSelection(normalized);
            }
        }
        public bool IsWindowRoom =>
            !string.IsNullOrWhiteSpace(RoomType) &&
            RoomType.IndexOf("Window", StringComparison.OrdinalIgnoreCase) >= 0;

        public int? FullGlassShowersCount { get; set; } = 0;
        public int? PebbleStoneFloorsCount { get; set; } = 0;
        public int? FridgeCount { get; set; } = 0;
        public int? OvenCount { get; set; } = 0;

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

        public string WindowSideDisplay
        {
            get
            {
                return WindowSideSelection;
            }
        }

        public void SyncWindowSideSelectionFromFlags()
        {
            _windowSideSelection = GetWindowSideSelectionFromFlags();
            ApplyWindowSideSelection(_windowSideSelection);
        }

        private void ApplyWindowSideSelection(string selection)
        {
            if (!IsWindowRoom)
            {
                WindowInside = false;
                WindowOutside = false;
                return;
            }

            WindowInside = false;
            WindowOutside = false;
            var isExcluded = string.IsNullOrWhiteSpace(selection) || selection == "Excluded";
            IncludedInQuote = !isExcluded;

            if (isExcluded)
                return;

            switch (selection)
            {
                case "Inside":
                    WindowInside = true;
                    break;
                case "Outside":
                    WindowOutside = true;
                    break;
                case "Inside & Outside":
                    WindowInside = true;
                    WindowOutside = true;
                    break;
                case "Window Tract":
                    WindowInside = true;
                    break;
            }
        }

        private string NormalizeWindowSideSelection(string selection)
        {
            if (!IsWindowRoom)
                return "";

            if (string.IsNullOrWhiteSpace(selection) || selection == "Excluded")
                return "Excluded";

            return selection switch
            {
                "Inside" => "Inside",
                "Outside" => "Outside",
                "Inside & Outside" => "Inside & Outside",
                "Window Tract" => "Window Tract",
                _ => ""
            };
        }

        private string GetWindowSideSelectionFromFlags()
        {
            if (!IsWindowRoom)
                return "";

            if (!IncludedInQuote)
                return "Excluded";

            if (WindowInside && WindowOutside)
                return "Inside & Outside";
            if (WindowInside)
                return "Inside";
            if (WindowOutside)
                return "Outside";
            return "Excluded";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
