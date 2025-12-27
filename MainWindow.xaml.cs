using Cleaning_Quote.Data;
using Cleaning_Quote.Models;
using Cleaning_Quote.Services;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Cleaning_Quote
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Repositories / selection
        private readonly ClientRepository _clientRepo = new ClientRepository();
        private readonly ServiceTypePricingRepository _serviceTypePricingRepo = new ServiceTypePricingRepository();
        private Client _selectedClient;

        // Quotes
        private readonly QuoteRepository _quoteRepo = new QuoteRepository();
        private PricingService _pricing = new PricingService(PricingRules.Default());
        private Quote _currentQuote;
        private readonly System.Collections.ObjectModel.ObservableCollection<QuoteRoom> _rooms =
            new System.Collections.ObjectModel.ObservableCollection<QuoteRoom>();
        private readonly System.Collections.ObjectModel.ObservableCollection<QuotePet> _pets =
            new System.Collections.ObjectModel.ObservableCollection<QuotePet>();
        private readonly System.Collections.ObjectModel.ObservableCollection<QuoteOccupant> _occupants =
            new System.Collections.ObjectModel.ObservableCollection<QuoteOccupant>();
        private ServiceTypePricing _currentServiceTypePricing;
        
        // Quote list selection
        private QuoteListItem _selectedQuoteListItem;

        private bool _suppressQuoteNameTracking = false;
        private bool _quoteNameManuallyEdited = false;
        private bool _isLoadingQuote = false;

        // Dirty tracking for client form
        private bool _isDirty = false;
        private bool _suppressDirtyTracking = false;
        private bool _suppressSelectionHandler = false;

        // Totals (bind to XAML)
        private string _hoursText;
        private string _subtotalText;
        private string _ccFeeText;
        private string _taxText;
        private string _totalText;
        private string _minimumWarningText;
        private string _clientDisplayName;
        private string _clientAddressLine;
        private string _estimatedSqFtText;
        private string _sqFtCalcText;
        private string _firstCleanText;
        private string _deepCleanText;
        private string _maintenanceText;
        private string _oneTimeDeepCleanText;
        private string _windowInsideText;
        private string _windowOutsideText;
        private Visibility _quotePanelVisibility = Visibility.Collapsed;

        private bool _suppressServiceTypeSettingsChange = false;

        public string HoursText { get => _hoursText; set { _hoursText = value; OnPropertyChanged(); } }
        public string SubtotalText { get => _subtotalText; set { _subtotalText = value; OnPropertyChanged(); } }
        public string CcFeeText { get => _ccFeeText; set { _ccFeeText = value; OnPropertyChanged(); } }
        public string TaxText { get => _taxText; set { _taxText = value; OnPropertyChanged(); } }
        public string TotalText { get => _totalText; set { _totalText = value; OnPropertyChanged(); } }
        public string MinimumWarningText { get => _minimumWarningText; set { _minimumWarningText = value; OnPropertyChanged(); } }
        public string ClientDisplayName { get => _clientDisplayName; set { _clientDisplayName = value; OnPropertyChanged(); } }
        public string ClientAddressLine { get => _clientAddressLine; set { _clientAddressLine = value; OnPropertyChanged(); } }
        public string EstimatedSqFtText { get => _estimatedSqFtText; set { _estimatedSqFtText = value; OnPropertyChanged(); } }
        public string SqFtCalcText { get => _sqFtCalcText; set { _sqFtCalcText = value; OnPropertyChanged(); } }
        public string FirstCleanText { get => _firstCleanText; set { _firstCleanText = value; OnPropertyChanged(); } }
        public string DeepCleanText { get => _deepCleanText; set { _deepCleanText = value; OnPropertyChanged(); } }
        public string MaintenanceText { get => _maintenanceText; set { _maintenanceText = value; OnPropertyChanged(); } }
        public string OneTimeDeepCleanText { get => _oneTimeDeepCleanText; set { _oneTimeDeepCleanText = value; OnPropertyChanged(); } }
        public string WindowInsideText { get => _windowInsideText; set { _windowInsideText = value; OnPropertyChanged(); } }
        public string WindowOutsideText { get => _windowOutsideText; set { _windowOutsideText = value; OnPropertyChanged(); } }
        public Visibility QuotePanelVisibility { get => _quotePanelVisibility; set { _quotePanelVisibility = value; OnPropertyChanged(); } }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Load client list on startup
            LoadClients();

            RoomsGrid.ItemsSource = _rooms;
            PetsGrid.ItemsSource = _pets;
            OccupantsGrid.ItemsSource = _occupants;
            InitQuoteInputsDefaults();
            HideQuotePanel();

        }

        private void RunPricingDemo()
        {
            var rules = PricingRules.Default();
            var pricing = new PricingService(rules);

            var quote = new Quote
            {
                LaborRate = 50m,
                TaxRate = 0.08m,
                CreditCardFeeRate = 0.03m,
                CreditCard = true,
                PetsCount = 2,
                HouseholdSize = 4,
                SmokingInside = false,
            };

            quote.Rooms.Add(new QuoteRoom { RoomType = "Bedroom", Size = "M", Complexity = 2 });
            quote.Rooms.Add(new QuoteRoom { RoomType = "Bathroom", Size = "M", Complexity = 3, FullGlassShowersCount = 1 });
            quote.Rooms.Add(new QuoteRoom { RoomType = "Kitchen", Size = "L", Complexity = 2, FridgeCount = 1, OvenCount = 1 });

            var totals = pricing.CalculateTotals(quote);

            HoursText = $"Hours: {totals.TotalLaborHours}";
            SubtotalText = $"Subtotal: {totals.Subtotal:C}";
            CcFeeText = $"CC Fee: {totals.CreditCardFee:C}";
            TaxText = $"Tax: {totals.Tax:C}";
            TotalText = $"Total: {totals.Total:C}";
        }

        // ----- Client UI handlers -----

        private void LoadClients()
        {
            ClientsList.ItemsSource = _clientRepo.GetAll();
        }

        private void LoadQuotesForSelectedClient(bool hideQuotePanel = true)
        {
            if (_selectedClient == null)
            {
                QuotesList.ItemsSource = null;
                if (hideQuotePanel)
                    HideQuotePanel();
                return;
            }

            QuotesList.ItemsSource = _quoteRepo.GetForClient(_selectedClient.ClientId);
            if (hideQuotePanel)
                HideQuotePanel();
        }


        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var q = SearchBox.Text?.Trim() ?? "";
            ClientsList.ItemsSource = string.IsNullOrWhiteSpace(q)
                ? _clientRepo.GetAll()
                : _clientRepo.SearchByName(q);
        }

        private void ClientsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressSelectionHandler) return;

            var newlySelected = ClientsList.SelectedItem as Client;


            // If we're switching away and there are unsaved changes, confirm
            if (newlySelected != null && _selectedClient != null && newlySelected.ClientId != _selectedClient.ClientId)
            {
                if (!ConfirmDiscardIfDirty())
                {
                    // Revert selection back to previous client
                    _suppressSelectionHandler = true;
                    ClientsList.SelectedItem = _selectedClient;
                    _suppressSelectionHandler = false;
                    return;
                }
            }

            _selectedClient = newlySelected;
            if (_selectedClient == null) return;

            // Loading values into fields should NOT mark dirty
            _suppressDirtyTracking = true;

            NameBox.Text = _selectedClient.DisplayName;
            Addr1Box.Text = _selectedClient.AddressLine1;
            Addr2Box.Text = _selectedClient.AddressLine2;
            CityBox.Text = _selectedClient.City;
            StateBox.Text = _selectedClient.State;
            ZipBox.Text = _selectedClient.Zip;
            PhoneBox.Text = _selectedClient.Phone;
            EmailBox.Text = _selectedClient.Email;
            NotesBox.Text = _selectedClient.Notes;
            ClientDisplayName = _selectedClient.DisplayName;
            ClientAddressLine = BuildClientAddressLine(_selectedClient);

            _suppressDirtyTracking = false;
            _isDirty = false;

            SaveButton.Content = "Update";
            StatusText.Text = $"Editing: {_selectedClient.DisplayName}";

            LoadQuotesForSelectedClient();
            HideQuotePanel();

        }

        private void NewClient_Click(object sender, RoutedEventArgs e)
        {
            
            if (!ConfirmDiscardIfDirty())
                return;

            _selectedClient = null;

            NameBox.Text = "";
            Addr1Box.Text = "";
            Addr2Box.Text = "";
            CityBox.Text = "";
            StateBox.Text = "";
            ZipBox.Text = "";
            PhoneBox.Text = "";
            EmailBox.Text = "";
            NotesBox.Text = "";
            ClientDisplayName = "";
            ClientAddressLine = "";

            ClientsList.SelectedItem = null;
            _isDirty = false;
            SaveButton.Content = "Save";
            StatusText.Text = "New client ready.";
            HideQuotePanel();

        }

        private void AnyField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressDirtyTracking) return;
            _isDirty = true;
        }

        private bool ConfirmDiscardIfDirty()
        {
            if (!_isDirty) return true;

            var result = MessageBox.Show(
                "You have unsaved changes. If you continue, those changes will be lost.\n\nDiscard changes?",
                "Unsaved Changes",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _isDirty = false;
                return true;
            }

            return false;
        }

        private void SaveClient_Click(object sender, RoutedEventArgs e)
        {
            var name = NameBox.Text?.Trim();
            var addr1 = Addr1Box.Text?.Trim();
            var city = CityBox.Text?.Trim();
            var state = StateBox.Text?.Trim();
            var zip = ZipBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                StatusText.Text = "Client name is required.";
                return;
            }
            if (string.IsNullOrWhiteSpace(addr1) ||
                string.IsNullOrWhiteSpace(city) ||
                string.IsNullOrWhiteSpace(state) ||
                string.IsNullOrWhiteSpace(zip))
            {
                StatusText.Text = "Street Address, City, State, and Zip are required.";
                return;
            }

            bool isNew = false;
            if (_selectedClient == null)
            {
                _selectedClient = new Client();
                isNew = true;
            }

            _selectedClient.DisplayName = name;
            _selectedClient.AddressLine1 = addr1;
            _selectedClient.AddressLine2 = Addr2Box.Text?.Trim() ?? "";
            _selectedClient.City = city;
            _selectedClient.State = state;
            _selectedClient.Zip = zip;
            _selectedClient.Phone = PhoneBox.Text?.Trim() ?? "";
            _selectedClient.Email = EmailBox.Text?.Trim() ?? "";
            _selectedClient.Notes = NotesBox.Text?.Trim() ?? "";

            // If this is a NEW client, warn if same name+address already exists
            if (!_clientRepo.Exists(_selectedClient.ClientId))
            {
                bool dup = _clientRepo.ExistsByNameAndAddress(
                    _selectedClient.DisplayName,
                    _selectedClient.AddressLine1,
                    _selectedClient.City,
                    _selectedClient.State,
                    _selectedClient.Zip);

                if (dup)
                {
                    var result = MessageBox.Show(
                        "A client with this same name and address already exists.\n\nCreate a duplicate anyway?",
                        "Possible Duplicate Client",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes)
                    {
                        StatusText.Text = "Save cancelled (duplicate not created).";
                        return;
                    }
                }
            }


            try
            {
                // Decide Insert vs Update based on DB reality
                bool existsInDb = _selectedClient != null
                  && _selectedClient.ClientId != Guid.Empty
                  && _clientRepo.Exists(_selectedClient.ClientId);

                if (!existsInDb)
                {
                    _clientRepo.Insert(_selectedClient);
                    StatusText.Text = $"Saved NEW client: {_selectedClient.DisplayName}";
                    SaveButton.Content = "Update";
                }
                else
                {
                    _clientRepo.Update(_selectedClient);

                    MessageBox.Show(
                        "Client information updated successfully.",
                        "Update Complete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    StatusText.Text = $"Updated client: {_selectedClient.DisplayName}";
                    _isDirty = false;

                }

                // Capture the ID before refreshing the list (because refresh can clear selection)
                var savedId = _selectedClient.ClientId;

                // Refresh list
                SearchBox_TextChanged(null, null);

                // Re-select client in list using savedId (NOT _selectedClient)
                foreach (var item in (IEnumerable)ClientsList.ItemsSource)
                {
                    var c = item as Client;
                    if (c != null && c.ClientId == savedId)
                    {
                        ClientsList.SelectedItem = c;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Save failed");
            }
        }

        private void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient == null)
            {
                StatusText.Text = "Select a client to delete.";
                return;
            }

            var result = MessageBox.Show(
                $"Delete client?\n\n{_selectedClient.DisplayName}\n{_selectedClient.AddressLine1}, {_selectedClient.City}, {_selectedClient.State} {_selectedClient.Zip}\n\nThis cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                var deletedId = _selectedClient.ClientId;

                _clientRepo.Delete(deletedId);

                // Clear UI
                _selectedClient = null;
                ClientsList.SelectedItem = null;

                NameBox.Text = "";
                Addr1Box.Text = "";
                Addr2Box.Text = "";
                CityBox.Text = "";
                StateBox.Text = "";
                ZipBox.Text = "";
                PhoneBox.Text = "";
                EmailBox.Text = "";
                NotesBox.Text = "";

                // Refresh list
                SearchBox_TextChanged(null, null);

                StatusText.Text = "Client deleted.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Delete failed");
            }
        }


        // ----- INotifyPropertyChanged -----
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void InitQuoteInputsDefaults()
        {
            LaborRateBox.Text = "50";
            TaxRateBox.Text = "0.08";
            CcFeeRateBox.Text = "0.03";
            CreditCardCheck.IsChecked = true;
            ServiceTypeBox.SelectedIndex = 0;
            ServiceFrequencyBox.SelectedIndex = 0;
            LastProfessionalCleaningBox.SelectedIndex = 0;
            QuoteNameBox.Text = "";
            TotalSqFtBox.Text = "0";
            SqFtOverrideCheck.IsChecked = false;
            QuoteDatePicker.SelectedDate = DateTime.Today;
            PaymentMethodBox.SelectedIndex = 0;
            PaymentMethodOtherBox.Text = "";
            FeedbackDiscussedCheck.IsChecked = false;
            EntryInstructionsBox.Text = "";
            QuoteNotesBox.Text = "";
            PetsYesCheck.IsChecked = false;
            PetsNoCheck.IsChecked = true;
            OccupantsYesCheck.IsChecked = false;
            OccupantsNoCheck.IsChecked = true;
            PetsGrid.Visibility = Visibility.Collapsed;
            PetsButtonsPanel.Visibility = Visibility.Collapsed;
            OccupantsGrid.Visibility = Visibility.Collapsed;
            OccupantsButtonsPanel.Visibility = Visibility.Collapsed;
            _pets.Clear();
            _occupants.Clear();
            SubItemTypeBox.SelectedIndex = 0;
            WindowSizeBox.SelectedIndex = 1;
            WindowInsideCheck.IsChecked = false;
            WindowOutsideCheck.IsChecked = false;
            DefaultRoomLevelBox.SelectedIndex = 1;
            DefaultRoomSizeBox.SelectedIndex = 1;
            DefaultRoomComplexityBox.SelectedIndex = 0;
            LoadServiceTypePricing(GetSelectedServiceType());
        }

        private void OpenServiceTypeDefaults_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new ServiceTypeSettingsWindow(GetSelectedServiceType())
            {
                Owner = this
            };
            settingsWindow.ShowDialog();
            LoadServiceTypePricing(GetSelectedServiceType());
        }

        private void OpenComplexityReference_Click(object sender, RoutedEventArgs e)
        {
            var referenceWindow = new ComplexityReferenceWindow(GetSelectedServiceType())
            {
                Owner = this
            };
            referenceWindow.ShowDialog();
        }

        private void NewQuote_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient == null)
            {
                MessageBox.Show("Select a client first.", "No client selected");
                return;
            }

            _currentQuote = new Quote
            {
                ClientId = _selectedClient.ClientId,
                QuoteDate = DateTime.Today,
                Status = "Draft"
            };

            _rooms.Clear();
            _rooms.Add(BuildDefaultRoom());
            _pets.Clear();
            _occupants.Clear();
            QuoteDatePicker.SelectedDate = _currentQuote.QuoteDate;
            TotalSqFtBox.Text = "0";
            SqFtOverrideCheck.IsChecked = false;
            EntryInstructionsBox.Text = "";
            PaymentMethodBox.SelectedIndex = 0;
            PaymentMethodOtherBox.Text = "";
            FeedbackDiscussedCheck.IsChecked = false;
            QuoteNotesBox.Text = "";
            PetsYesCheck.IsChecked = false;
            PetsNoCheck.IsChecked = true;
            OccupantsYesCheck.IsChecked = false;
            OccupantsNoCheck.IsChecked = true;
            PetsGrid.Visibility = Visibility.Collapsed;
            PetsButtonsPanel.Visibility = Visibility.Collapsed;
            OccupantsGrid.Visibility = Visibility.Collapsed;
            OccupantsButtonsPanel.Visibility = Visibility.Collapsed;

            _quoteNameManuallyEdited = false;
            UpdateQuoteNameIfAuto();

            ShowQuotePanel();
            RecalculateTotals();
            StatusText.Text = "New quote started.";
        }

        private void AddRoom_Click(object sender, RoutedEventArgs e)
        {
            _rooms.Add(BuildDefaultRoom());
            RecalculateTotals();
        }

        private void RemoveRoom_Click(object sender, RoutedEventArgs e)
        {
            if (RoomsGrid.SelectedItem is QuoteRoom r)
            {
                if (!r.IsSubItem)
                {
                    RemoveSubItems(r.QuoteRoomId);
                }
                _rooms.Remove(r);
                RecalculateTotals();
            }
        }

        private void AddSubItem_Click(object sender, RoutedEventArgs e)
        {
            if (RoomsGrid.SelectedItem is not QuoteRoom selected)
            {
                MessageBox.Show("Select a room first to add a sub-item.", "No room selected");
                return;
            }

            var parentRoom = selected.IsSubItem
                ? _rooms.FirstOrDefault(r => r.QuoteRoomId == selected.ParentRoomId)
                : selected;

            if (parentRoom == null)
            {
                MessageBox.Show("Select a parent room row to add sub-items.", "Invalid selection");
                return;
            }

            if (parentRoom.QuoteRoomId == Guid.Empty)
                parentRoom.QuoteRoomId = Guid.NewGuid();

            var subItemLabel = GetSelectedSubItemLabel();
            if (string.IsNullOrWhiteSpace(subItemLabel))
            {
                MessageBox.Show("Choose a sub-item type first.", "Missing sub-item");
                return;
            }

            var category = MapSubItemCategory(subItemLabel);
            var size = WindowSizeBox.SelectedItem is ComboBoxItem sizeItem
                ? sizeItem.Content?.ToString() ?? "M"
                : "M";

            if (!int.TryParse(SubItemCountBox.Text, out var count))
                count = 1;
            count = Math.Max(1, count);

            var includeInside = WindowInsideCheck.IsChecked == true;
            var includeOutside = WindowOutsideCheck.IsChecked == true;
            var windowSideSelection = category == "Window"
                ? GetWindowSideSelection(includeInside, includeOutside)
                : "Excluded";

            for (var i = 0; i < count; i++)
            {
                var subItem = new QuoteRoom
                {
                    QuoteRoomId = Guid.NewGuid(),
                    QuoteId = _currentQuote?.QuoteId ?? Guid.Empty,
                    ParentRoomId = parentRoom.QuoteRoomId,
                    RoomType = subItemLabel,
                    ItemCategory = category,
                    IsSubItem = true,
                    IncludedInQuote = true,
                    Size = category == "Window" ? size : "M",
                    Complexity = GetSubItemDefaultComplexity(category),
                    WindowSideSelection = windowSideSelection,
                };

                InsertSubItemAfterParent(parentRoom, subItem);
            }

            RecalculateTotals();
        }

        private void InsertSubItemAfterParent(QuoteRoom parentRoom, QuoteRoom subItem)
        {
            var parentIndex = _rooms.IndexOf(parentRoom);
            if (parentIndex < 0)
            {
                _rooms.Add(subItem);
                return;
            }

            var insertIndex = parentIndex + 1;
            while (insertIndex < _rooms.Count && _rooms[insertIndex].ParentRoomId == parentRoom.QuoteRoomId)
            {
                insertIndex++;
            }
            _rooms.Insert(insertIndex, subItem);
        }

        private void RemoveSubItems(Guid parentRoomId)
        {
            for (var i = _rooms.Count - 1; i >= 0; i--)
            {
                if (_rooms[i].ParentRoomId == parentRoomId)
                    _rooms.RemoveAt(i);
            }
        }

        private string GetSelectedSubItemLabel()
        {
            return SubItemTypeBox.SelectedItem is ComboBoxItem item
                ? item.Content?.ToString() ?? ""
                : "";
        }

        private string MapSubItemCategory(string label)
        {
            return label switch
            {
                "Full Glass Shower" => "FullGlassShower",
                "Pebble Stone Floor" => "PebbleStoneFloor",
                "Fridge" => "Fridge",
                "Oven" => "Oven",
                "Ceiling Fan" => "CeilingFan",
                "Standard Window - 2 panes" => "Window",
                _ => ""
            };
        }

        private string GetWindowSideSelection(bool includeInside, bool includeOutside)
        {
            if (includeInside && includeOutside)
                return "Inside & Outside";
            if (includeInside)
                return "Inside";
            if (includeOutside)
                return "Outside";
            return "Excluded";
        }

        private int GetSubItemDefaultComplexity(string category)
        {
            if (_currentServiceTypePricing == null)
                return 1;

            return category switch
            {
                "FullGlassShower" => _currentServiceTypePricing.FullGlassShowerComplexity,
                "PebbleStoneFloor" => _currentServiceTypePricing.PebbleStoneFloorComplexity,
                "Fridge" => _currentServiceTypePricing.FridgeComplexity,
                "Oven" => _currentServiceTypePricing.OvenComplexity,
                "CeilingFan" => _currentServiceTypePricing.CeilingFanComplexity,
                "Window" => _currentServiceTypePricing.WindowComplexity,
                _ => 1
            };
        }

        private void PetsYesCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (PetsYesCheck.IsChecked == true)
            {
                PetsNoCheck.IsChecked = false;
                PetsGrid.Visibility = Visibility.Visible;
                PetsButtonsPanel.Visibility = Visibility.Visible;
            }
            else if (PetsNoCheck.IsChecked == false)
            {
                PetsNoCheck.IsChecked = true;
            }
            RecalculateTotals();
        }

        private void PetsNoCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (PetsNoCheck.IsChecked == true)
            {
                PetsYesCheck.IsChecked = false;
                _pets.Clear();
                PetsGrid.Visibility = Visibility.Collapsed;
                PetsButtonsPanel.Visibility = Visibility.Collapsed;
            }
            RecalculateTotals();
        }

        private void OccupantsYesCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (OccupantsYesCheck.IsChecked == true)
            {
                OccupantsNoCheck.IsChecked = false;
                OccupantsGrid.Visibility = Visibility.Visible;
                OccupantsButtonsPanel.Visibility = Visibility.Visible;
            }
            else if (OccupantsNoCheck.IsChecked == false)
            {
                OccupantsNoCheck.IsChecked = true;
            }
        }

        private void OccupantsNoCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (OccupantsNoCheck.IsChecked == true)
            {
                OccupantsYesCheck.IsChecked = false;
                _occupants.Clear();
                OccupantsGrid.Visibility = Visibility.Collapsed;
                OccupantsButtonsPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void AddPet_Click(object sender, RoutedEventArgs e)
        {
            _pets.Add(new QuotePet());
        }

        private void RemovePet_Click(object sender, RoutedEventArgs e)
        {
            if (PetsGrid.SelectedItem is QuotePet pet)
                _pets.Remove(pet);
        }

        private void AddOccupant_Click(object sender, RoutedEventArgs e)
        {
            _occupants.Add(new QuoteOccupant());
        }

        private void RemoveOccupant_Click(object sender, RoutedEventArgs e)
        {
            if (OccupantsGrid.SelectedItem is QuoteOccupant occupant)
                _occupants.Remove(occupant);
        }

        private void PaymentMethodBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = PaymentMethodBox.SelectedItem as ComboBoxItem;
            var isOther = selected?.Content?.ToString() == "Other";
            PaymentMethodOtherBox.Visibility = isOther ? Visibility.Visible : Visibility.Collapsed;
            if (!isOther)
                PaymentMethodOtherBox.Text = "";
            RecalculateTotals();
        }

        private void RoomsGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }


        private void QuoteInputs_Changed(object sender, RoutedEventArgs e) => RecalculateTotals();
        private void QuoteInputs_Changed(object sender, TextChangedEventArgs e) => RecalculateTotals();

        private void QuoteDetails_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoadingQuote)
                return;

            UpdateQuoteNameIfAuto();
            LoadServiceTypePricing(GetSelectedServiceType());
            RecalculateTotals();
        }

        private void QuoteNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressQuoteNameTracking)
                return;

            _quoteNameManuallyEdited = !string.IsNullOrWhiteSpace(QuoteNameBox.Text);
            if (_currentQuote != null)
                _currentQuote.QuoteName = QuoteNameBox.Text?.Trim() ?? "";
        }

        private void RoomsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Let edit commit, then recalc
            Dispatcher.InvokeAsync(RecalculateTotals);
        }

        private void RecalculateTotals()
        {
            if (_isLoadingQuote)
                return;

            if (_selectedClient == null || _currentQuote == null)
            {
                ClearTotals();
                return;
            }

            ApplyServiceTypePricingRules();

            if (!decimal.TryParse(LaborRateBox.Text, out var laborRate)) laborRate = 50m;
            if (!decimal.TryParse(TaxRateBox.Text, out var taxRate)) taxRate = 0.08m;
            if (!decimal.TryParse(CcFeeRateBox.Text, out var ccRate)) ccRate = 0.03m;

            var pets = _pets.Count;

            _currentQuote.LaborRate = laborRate;
            _currentQuote.TaxRate = taxRate;
            _currentQuote.CreditCardFeeRate = ccRate;
            _currentQuote.CreditCard = CreditCardCheck.IsChecked == true;
            _currentQuote.PetsCount = pets;
            _currentQuote.ServiceType = ServiceTypeBox.SelectedItem is ComboBoxItem serviceTypeItem
                ? serviceTypeItem.Content?.ToString() ?? ""
                : "";
            _currentQuote.ServiceFrequency = ServiceFrequencyBox.SelectedItem is ComboBoxItem frequencyItem
                ? frequencyItem.Content?.ToString() ?? ""
                : "";
            _currentQuote.LastProfessionalCleaning = LastProfessionalCleaningBox.SelectedItem is ComboBoxItem cleaningItem
                ? cleaningItem.Content?.ToString() ?? ""
                : "";
            _currentQuote.EntryInstructions = EntryInstructionsBox.Text?.Trim() ?? "";
            _currentQuote.PaymentMethod = PaymentMethodBox.SelectedItem is ComboBoxItem paymentItem
                ? paymentItem.Content?.ToString() ?? ""
                : "";
            _currentQuote.PaymentMethodOther = PaymentMethodOtherBox.Text?.Trim() ?? "";
            _currentQuote.FeedbackDiscussed = FeedbackDiscussedCheck.IsChecked == true;
            _currentQuote.Notes = QuoteNotesBox.Text?.Trim() ?? "";
            _currentQuote.QuoteDate = QuoteDatePicker.SelectedDate ?? DateTime.Today;
            if (decimal.TryParse(TotalSqFtBox.Text, out var totalSqFt))
                _currentQuote.TotalSqFt = totalSqFt;
            else
                _currentQuote.TotalSqFt = 0m;
            _currentQuote.UseTotalSqFtOverride = SqFtOverrideCheck.IsChecked == true;

            UpdateQuoteNameIfAuto();
            _currentQuote.QuoteName = QuoteNameBox.Text?.Trim() ?? "";

            // Ensure the quote has an ID (new quotes)
            if (_currentQuote.QuoteId == Guid.Empty)
                _currentQuote.QuoteId = Guid.NewGuid();

            _currentQuote.Rooms.Clear();

            foreach (var r in _rooms)
            {
                // Ensure every room has its own unique PK
                if (r.QuoteRoomId == Guid.Empty)
                    r.QuoteRoomId = Guid.NewGuid();

                // Keep the FK consistent
                r.QuoteId = _currentQuote.QuoteId;

                _currentQuote.Rooms.Add(r);
            }

            _currentQuote.Pets = new System.Collections.Generic.List<QuotePet>(_pets);
            _currentQuote.Occupants = new System.Collections.Generic.List<QuoteOccupant>(_occupants);

            var totals = _pricing.CalculateTotals(_currentQuote);
            UpdateRoomAmounts(laborRate);

            HoursText = $"Hours: {totals.TotalLaborHours}";
            SubtotalText = $"Subtotal: {totals.Subtotal:C}";
            CcFeeText = $"CC Fee: {totals.CreditCardFee:C}";
            TaxText = $"Tax: {totals.Tax:C}";
            TotalText = $"Total: {totals.Total:C}";

            UpdateEstimatedSqFt();
            UpdateQuoteFormCalculator();
        }

        private void QuotesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedQuoteListItem = QuotesList.SelectedItem as QuoteListItem;
        }

        private void LoadQuote_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedQuoteListItem == null)
            {
                MessageBox.Show("Select a quote to load.");
                return;
            }

            var q = _quoteRepo.GetById(_selectedQuoteListItem.QuoteId);
            if (q == null)
            {
                MessageBox.Show("Could not load that quote.");
                LoadQuotesForSelectedClient();
                return;
            }

            _isLoadingQuote = true;
            try
            {
                _currentQuote = q;

                LaborRateBox.Text = q.LaborRate.ToString();
                TaxRateBox.Text = q.TaxRate.ToString();
                CcFeeRateBox.Text = q.CreditCardFeeRate.ToString();
                CreditCardCheck.IsChecked = q.CreditCard;
                SetSelectedCombo(ServiceTypeBox, q.ServiceType);
                SetSelectedCombo(ServiceFrequencyBox, q.ServiceFrequency);
                SetSelectedCombo(LastProfessionalCleaningBox, q.LastProfessionalCleaning);
                QuoteDatePicker.SelectedDate = q.QuoteDate;
                TotalSqFtBox.Text = q.TotalSqFt.ToString();
                SqFtOverrideCheck.IsChecked = q.UseTotalSqFtOverride;
                EntryInstructionsBox.Text = q.EntryInstructions ?? "";
                SetSelectedCombo(PaymentMethodBox, q.PaymentMethod);
                PaymentMethodOtherBox.Text = q.PaymentMethodOther ?? "";
                FeedbackDiscussedCheck.IsChecked = q.FeedbackDiscussed;
                QuoteNotesBox.Text = q.Notes ?? "";

                _suppressQuoteNameTracking = true;
                QuoteNameBox.Text = q.QuoteName ?? "";
                _suppressQuoteNameTracking = false;
                _quoteNameManuallyEdited = !string.IsNullOrWhiteSpace(q.QuoteName);
                if (!_quoteNameManuallyEdited)
                    UpdateQuoteNameIfAuto();

                _rooms.Clear();
                foreach (var r in q.Rooms)
                    _rooms.Add(r);

                _pets.Clear();
                if (q.Pets != null)
                {
                    foreach (var pet in q.Pets)
                        _pets.Add(pet);
                }

                _occupants.Clear();
                if (q.Occupants != null)
                {
                    foreach (var occupant in q.Occupants)
                        _occupants.Add(occupant);
                }

                PetsYesCheck.IsChecked = _pets.Count > 0;
                PetsNoCheck.IsChecked = _pets.Count == 0;
                PetsGrid.Visibility = _pets.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                PetsButtonsPanel.Visibility = _pets.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

                OccupantsYesCheck.IsChecked = _occupants.Count > 0;
                OccupantsNoCheck.IsChecked = _occupants.Count == 0;
                OccupantsGrid.Visibility = _occupants.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                OccupantsButtonsPanel.Visibility = _occupants.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

                LoadServiceTypePricing(string.IsNullOrWhiteSpace(q.ServiceType) ? GetSelectedServiceType() : q.ServiceType);
                ShowQuotePanel();

                HoursText = $"Hours: {q.TotalLaborHours}";
                SubtotalText = $"Subtotal: {q.Subtotal:C}";
                CcFeeText = $"CC Fee: {q.CreditCardFee:C}";
                TaxText = $"Tax: {q.Tax:C}";
                TotalText = $"Total: {q.Total:C}";
                UpdateEstimatedSqFt();
                UpdateQuoteFormCalculator();

                StatusText.Text = $"Loaded quote from {q.QuoteDate:MM/dd/yyyy}";
            }
            finally
            {
                _isLoadingQuote = false;
            }

            RecalculateTotals();
        }


        private void SaveQuote_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient == null)
            {
                MessageBox.Show("Select a client first.", "No client selected");
                return;
            }
            if (_currentQuote == null)
            {
                MessageBox.Show("Start a new quote or load an existing quote first.", "No quote selected");
                return;
            }
            if (_rooms.Count == 0)
            {
                MessageBox.Show("Add at least one room.", "No rooms");
                return;
            }

            try
            {
                RecalculateTotals();

                var totals = _pricing.CalculateTotals(_currentQuote);
                _currentQuote.TotalLaborHours = totals.TotalLaborHours;
                _currentQuote.Subtotal = totals.Subtotal;
                _currentQuote.CreditCardFee = totals.CreditCardFee;
                _currentQuote.Tax = totals.Tax;
                _currentQuote.Total = totals.Total;

                _currentQuote.ClientId = _selectedClient.ClientId;

                if (_currentQuote.QuoteDate == default)
                    _currentQuote.QuoteDate = DateTime.Today;

                if (string.IsNullOrWhiteSpace(_currentQuote.Status))
                    _currentQuote.Status = "Draft";

                if (string.IsNullOrWhiteSpace(_currentQuote.QuoteName))
                {
                    _quoteNameManuallyEdited = false;
                    UpdateQuoteNameIfAuto();
                    _currentQuote.QuoteName = QuoteNameBox.Text?.Trim() ?? "";
                }

                _quoteRepo.Save(_currentQuote);


                LoadQuotesForSelectedClient(hideQuotePanel: false); // refresh list so you see changes

                // Re-select the quote we just saved so the UI reflects the edit
                foreach (var item in (IEnumerable)QuotesList.ItemsSource)
                {
                    if (item is QuoteListItem qi && qi.QuoteId == _currentQuote.QuoteId)
                    {
                        QuotesList.SelectedItem = qi;
                        break;
                    }
                }


                MessageBox.Show("Quote saved.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                StatusText.Text = $"Quote saved for {_selectedClient.DisplayName}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Save Quote failed");
            }
        }

        private void DeleteQuote_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedQuoteListItem == null)
            {
                MessageBox.Show("Select a quote to delete.");
                return;
            }

            var result = MessageBox.Show(
                "Delete this quote? This cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            _quoteRepo.Delete(_selectedQuoteListItem.QuoteId);
            _selectedQuoteListItem = null;
            QuotesList.SelectedItem = null;

            LoadQuotesForSelectedClient();
            HideQuotePanel();
            StatusText.Text = "Quote deleted.";
        }

        private void UpdateQuoteNameIfAuto()
        {
            if (_selectedClient == null || _currentQuote == null)
                return;

            if (_quoteNameManuallyEdited && !string.IsNullOrWhiteSpace(QuoteNameBox.Text))
                return;

            var generated = GenerateQuoteName();

            _suppressQuoteNameTracking = true;
            QuoteNameBox.Text = generated;
            _suppressQuoteNameTracking = false;

            _currentQuote.QuoteName = generated;
        }

        private string GenerateQuoteName()
        {
            var date = _currentQuote?.QuoteDate == default ? DateTime.Today : _currentQuote.QuoteDate;
            var displayName = _selectedClient?.DisplayName?.Trim() ?? "";
            var lastName = displayName;

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                var parts = displayName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                lastName = parts.Length > 0 ? parts[^1] : displayName;
            }

            var serviceType = ServiceTypeBox.SelectedItem is ComboBoxItem item
                ? item.Content?.ToString() ?? ""
                : "";

            return $"{date:MM/dd/yyyy} | {lastName} | {serviceType}";
        }

        private void SetSelectedCombo(ComboBox comboBox, string value)
        {
            if (comboBox == null)
                return;

            foreach (var item in comboBox.Items)
            {
                if (item is ComboBoxItem comboItem && comboItem.Content?.ToString() == value)
                {
                    comboBox.SelectedItem = comboItem;
                    return;
                }
            }
        }

        private void LoadServiceTypePricing(string serviceType)
        {
            if (string.IsNullOrWhiteSpace(serviceType))
                return;

            _currentServiceTypePricing = _serviceTypePricingRepo.GetOrCreate(serviceType);
            _suppressServiceTypeSettingsChange = true;

            SqFtPerLaborHourBox.Text = _currentServiceTypePricing.SqFtPerLaborHour.ToString();
            SizeSmallSqFtBox.Text = _currentServiceTypePricing.SizeSmallSqFt.ToString();
            SizeMediumSqFtBox.Text = _currentServiceTypePricing.SizeMediumSqFt.ToString();
            SizeLargeSqFtBox.Text = _currentServiceTypePricing.SizeLargeSqFt.ToString();
            Complexity1MultiplierBox.Text = _currentServiceTypePricing.Complexity1Multiplier.ToString();
            Complexity2MultiplierBox.Text = _currentServiceTypePricing.Complexity2Multiplier.ToString();
            Complexity3MultiplierBox.Text = _currentServiceTypePricing.Complexity3Multiplier.ToString();
            FullGlassShowerHoursBox.Text = _currentServiceTypePricing.FullGlassShowerHoursEach.ToString();
            PebbleStoneFloorHoursBox.Text = _currentServiceTypePricing.PebbleStoneFloorHoursEach.ToString();
            FridgeHoursBox.Text = _currentServiceTypePricing.FridgeHoursEach.ToString();
            OvenHoursBox.Text = _currentServiceTypePricing.OvenHoursEach.ToString();

            _suppressServiceTypeSettingsChange = false;
            ApplyServiceTypePricingRules();
            RecalculateTotals();
        }

        private void ServiceTypeSettings_Changed(object sender, TextChangedEventArgs e)
        {
            if (_suppressServiceTypeSettingsChange || _currentServiceTypePricing == null)
                return;

            UpdateServiceTypePricingFromInputs(_currentServiceTypePricing);
            _serviceTypePricingRepo.Upsert(_currentServiceTypePricing);
            ApplyServiceTypePricingRules();
            RecalculateTotals();
        }

        private void UpdateServiceTypePricingFromInputs(ServiceTypePricing pricing)
        {
            if (pricing == null)
                return;

            if (decimal.TryParse(SqFtPerLaborHourBox.Text, out var sqftPerHour))
                pricing.SqFtPerLaborHour = sqftPerHour;
            if (decimal.TryParse(SizeSmallSqFtBox.Text, out var sizeSmall))
                pricing.SizeSmallSqFt = sizeSmall;
            if (decimal.TryParse(SizeMediumSqFtBox.Text, out var sizeMedium))
                pricing.SizeMediumSqFt = sizeMedium;
            if (decimal.TryParse(SizeLargeSqFtBox.Text, out var sizeLarge))
                pricing.SizeLargeSqFt = sizeLarge;
            if (decimal.TryParse(Complexity1MultiplierBox.Text, out var mult1))
                pricing.Complexity1Multiplier = mult1;
            if (decimal.TryParse(Complexity2MultiplierBox.Text, out var mult2))
                pricing.Complexity2Multiplier = mult2;
            if (decimal.TryParse(Complexity3MultiplierBox.Text, out var mult3))
                pricing.Complexity3Multiplier = mult3;
            if (decimal.TryParse(FullGlassShowerHoursBox.Text, out var glassHours))
                pricing.FullGlassShowerHoursEach = glassHours;
            if (decimal.TryParse(PebbleStoneFloorHoursBox.Text, out var pebbleHours))
                pricing.PebbleStoneFloorHoursEach = pebbleHours;
            if (decimal.TryParse(FridgeHoursBox.Text, out var fridgeHours))
                pricing.FridgeHoursEach = fridgeHours;
            if (decimal.TryParse(OvenHoursBox.Text, out var ovenHours))
                pricing.OvenHoursEach = ovenHours;
        }

        private void UpdateRoomAmounts(decimal laborRate)
        {
            foreach (var room in _rooms)
            {
                room.RoomAmount = room.RoomLaborHours * laborRate;
            }
        }

        private void UpdateEstimatedSqFt()
        {
            if (_currentServiceTypePricing == null)
            {
                EstimatedSqFtText = "";
                return;
            }

            decimal total = 0m;
            foreach (var room in _rooms)
            {
                if (room.IsSubItem)
                    continue;

                var size = (room.Size ?? "").Trim().ToUpperInvariant();
                total += size switch
                {
                    "S" => _currentServiceTypePricing.SizeSmallSqFt,
                    "L" => _currentServiceTypePricing.SizeLargeSqFt,
                    _ => _currentServiceTypePricing.SizeMediumSqFt
                };
            }

            EstimatedSqFtText = $"{total:N0}";
        }

        private void UpdateQuoteFormCalculator()
        {
            if (_currentServiceTypePricing == null || _currentQuote == null)
            {
                SqFtCalcText = "";
                FirstCleanText = "";
                DeepCleanText = "";
                MaintenanceText = "";
                OneTimeDeepCleanText = "";
                WindowInsideText = "";
                WindowOutsideText = "";
                MinimumWarningText = "";
                return;
            }

            var estimatedSqFt = GetEstimatedSqFtValue();
            var effectiveSqFt = _currentQuote.UseTotalSqFtOverride ? _currentQuote.TotalSqFt : estimatedSqFt;
            SqFtCalcText = $"Sq Ft: {effectiveSqFt:N0}";

            var firstCleanTotal = effectiveSqFt * _currentServiceTypePricing.FirstCleanRate;
            var deepCleanTotal = effectiveSqFt * _currentServiceTypePricing.DeepCleanRate;
            var maintenanceTotal = effectiveSqFt * _currentServiceTypePricing.MaintenanceRate;
            var oneTimeDeepTotal = effectiveSqFt * _currentServiceTypePricing.OneTimeDeepCleanRate;

            FirstCleanText = $"First Clean ({_currentServiceTypePricing.FirstCleanRate:0.##}/sq ft): {firstCleanTotal:C} (min {_currentServiceTypePricing.FirstCleanMinimum:C})";
            DeepCleanText = $"Deep Clean ({_currentServiceTypePricing.DeepCleanRate:0.##}/sq ft): {deepCleanTotal:C} (min {_currentServiceTypePricing.DeepCleanMinimum:C})";
            MaintenanceText = $"Maintenance ({_currentServiceTypePricing.MaintenanceRate:0.##}/sq ft): {maintenanceTotal:C} (min {_currentServiceTypePricing.MaintenanceMinimum:C})";
            OneTimeDeepCleanText = $"One Time Deep Clean ({_currentServiceTypePricing.OneTimeDeepCleanRate:0.##}/sq ft): {oneTimeDeepTotal:C} (min {_currentServiceTypePricing.OneTimeDeepCleanMinimum:C})";

            var windowInsideCount = CountWindows(side: "inside");
            var windowOutsideCount = CountWindows(side: "outside");
            var windowInsideTotal = windowInsideCount * _currentServiceTypePricing.WindowInsideRate;
            var windowOutsideTotal = windowOutsideCount * _currentServiceTypePricing.WindowOutsideRate;
            WindowInsideText = $"Windows inside ({windowInsideCount} @ {_currentServiceTypePricing.WindowInsideRate:C}): {windowInsideTotal:C}";
            WindowOutsideText = $"Windows outside ({windowOutsideCount} @ {_currentServiceTypePricing.WindowOutsideRate:C}): {windowOutsideTotal:C}";

            var minNotMet =
                firstCleanTotal < _currentServiceTypePricing.FirstCleanMinimum ||
                deepCleanTotal < _currentServiceTypePricing.DeepCleanMinimum ||
                maintenanceTotal < _currentServiceTypePricing.MaintenanceMinimum ||
                oneTimeDeepTotal < _currentServiceTypePricing.OneTimeDeepCleanMinimum;

            MinimumWarningText = minNotMet
                ? "Minimum not met for quote form pricing."
                : "";
        }

        private decimal GetEstimatedSqFtValue()
        {
            if (_currentServiceTypePricing == null)
                return 0m;

            decimal total = 0m;
            foreach (var room in _rooms)
            {
                if (room.IsSubItem)
                    continue;

                var size = (room.Size ?? "").Trim().ToUpperInvariant();
                total += size switch
                {
                    "S" => _currentServiceTypePricing.SizeSmallSqFt,
                    "L" => _currentServiceTypePricing.SizeLargeSqFt,
                    _ => _currentServiceTypePricing.SizeMediumSqFt
                };
            }

            return total;
        }

        private int CountWindows(string side)
        {
            var count = 0;
            foreach (var room in _rooms)
            {
                if (!room.IsSubItem || room.ItemCategory != "Window" || !room.IncludedInQuote)
                    continue;

                var inside = room.WindowInside;
                var outside = room.WindowOutside;
                if (!inside && !outside && side == "inside")
                {
                    count++;
                    continue;
                }

                if (side == "inside" && inside)
                    count++;
                else if (side == "outside" && outside)
                    count++;
            }

            return count;
        }

        private QuoteRoom BuildDefaultRoom()
        {
            return new QuoteRoom
            {
                RoomType = "Bedroom",
                Size = GetDefaultRoomSize(),
                Complexity = GetDefaultRoomComplexity(),
                Level = GetDefaultRoomLevel()
            };
        }

        private string GetDefaultRoomLevel()
        {
            return DefaultRoomLevelBox.SelectedItem as string ?? "";
        }

        private string GetDefaultRoomSize()
        {
            return DefaultRoomSizeBox.SelectedItem as string ?? "M";
        }

        private int GetDefaultRoomComplexity()
        {
            if (DefaultRoomComplexityBox.SelectedItem is int complexity)
                return complexity;

            if (DefaultRoomComplexityBox.SelectedItem is string value &&
                int.TryParse(value, out var parsed))
            {
                return parsed;
            }

            return 1;
        }

        private string BuildClientAddressLine(Client client)
        {
            if (client == null)
                return "";

            var line2 = string.IsNullOrWhiteSpace(client.AddressLine2) ? "" : $" {client.AddressLine2}";
            return $"{client.AddressLine1}{line2}, {client.City}, {client.State} {client.Zip}".Trim();
        }

        private void ApplyServiceTypePricingRules()
        {
            _pricing = new PricingService(PricingRules.FromServiceTypePricing(_currentServiceTypePricing));
        }

        private string GetSelectedServiceType()
        {
            return ServiceTypeBox.SelectedItem is ComboBoxItem serviceTypeItem
                ? serviceTypeItem.Content?.ToString() ?? ""
                : "";
        }

        private void ShowQuotePanel()
        {
            QuotePanelVisibility = Visibility.Visible;
        }

        private void HideQuotePanel()
        {
            QuotePanelVisibility = Visibility.Collapsed;
            _currentQuote = null;
            _rooms.Clear();
            _pets.Clear();
            _occupants.Clear();
            ClearTotals();
        }

        private void ClearTotals()
        {
            HoursText = "";
            SubtotalText = "";
            CcFeeText = "";
            TaxText = "";
            TotalText = "";
            MinimumWarningText = "";
            EstimatedSqFtText = "";
            SqFtCalcText = "";
            FirstCleanText = "";
            DeepCleanText = "";
            MaintenanceText = "";
            OneTimeDeepCleanText = "";
            WindowInsideText = "";
            WindowOutsideText = "";
        }

    }
}
