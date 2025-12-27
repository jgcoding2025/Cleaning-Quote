using Cleaning_Quote.Data;
using Cleaning_Quote.Models;
using Cleaning_Quote.Services;
using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Cleaning_Quote
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Repositories / selection
        private readonly ClientRepository _clientRepo = new ClientRepository();
        private Client _selectedClient;

        // Quotes
        private readonly QuoteRepository _quoteRepo = new QuoteRepository();
        private readonly PricingService _pricing = new PricingService(PricingRules.Default());
        private Quote _currentQuote;
        private readonly System.Collections.ObjectModel.ObservableCollection<QuoteRoom> _rooms =
            new System.Collections.ObjectModel.ObservableCollection<QuoteRoom>();
        
        // Quote list selection
        private QuoteListItem _selectedQuoteListItem;

        private bool _suppressQuoteNameTracking = false;
        private bool _quoteNameManuallyEdited = false;

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

        public string HoursText { get => _hoursText; set { _hoursText = value; OnPropertyChanged(); } }
        public string SubtotalText { get => _subtotalText; set { _subtotalText = value; OnPropertyChanged(); } }
        public string CcFeeText { get => _ccFeeText; set { _ccFeeText = value; OnPropertyChanged(); } }
        public string TaxText { get => _taxText; set { _taxText = value; OnPropertyChanged(); } }
        public string TotalText { get => _totalText; set { _totalText = value; OnPropertyChanged(); } }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Load client list on startup
            LoadClients();

            RoomsGrid.ItemsSource = _rooms;
            InitQuoteInputsDefaults();

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

        private void LoadQuotesForSelectedClient()
        {
            if (_selectedClient == null)
            {
                QuotesList.ItemsSource = null;
                return;
            }

            QuotesList.ItemsSource = _quoteRepo.GetForClient(_selectedClient.ClientId);
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

            _suppressDirtyTracking = false;
            _isDirty = false;

            SaveButton.Content = "Update";
            StatusText.Text = $"Editing: {_selectedClient.DisplayName}";

            LoadQuotesForSelectedClient();

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

            ClientsList.SelectedItem = null;
            _isDirty = false;
            SaveButton.Content = "Save";
            StatusText.Text = "New client ready.";

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
            PetsBox.Text = "0";
            HouseholdSizeBox.Text = "2";
            SmokingCheck.IsChecked = false;
            ServiceTypeBox.SelectedIndex = 0;
            ServiceFrequencyBox.SelectedIndex = 0;
            LastProfessionalCleaningBox.SelectedIndex = 0;
            QuoteNameBox.Text = "";
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
            _rooms.Add(new QuoteRoom { RoomType = "Bedroom", Size = "M", Complexity = 1 });

            _quoteNameManuallyEdited = false;
            UpdateQuoteNameIfAuto();

            RecalculateTotals();
            StatusText.Text = "New quote started.";
        }

        private void AddRoom_Click(object sender, RoutedEventArgs e)
        {
            _rooms.Add(new QuoteRoom { RoomType = "Bedroom", Size = "M", Complexity = 1 });
            RecalculateTotals();
        }

        private void RemoveRoom_Click(object sender, RoutedEventArgs e)
        {
            if (RoomsGrid.SelectedItem is QuoteRoom r)
            {
                _rooms.Remove(r);
                RecalculateTotals();
            }
        }

        private void RoomsGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }


        private void QuoteInputs_Changed(object sender, RoutedEventArgs e) => RecalculateTotals();
        private void QuoteInputs_Changed(object sender, TextChangedEventArgs e) => RecalculateTotals();

        private void QuoteDetails_Changed(object sender, RoutedEventArgs e)
        {
            UpdateQuoteNameIfAuto();
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
            if (_selectedClient == null)
                return;

            if (_currentQuote == null)
                _currentQuote = new Quote { ClientId = _selectedClient.ClientId, QuoteDate = DateTime.Today };

            if (!decimal.TryParse(LaborRateBox.Text, out var laborRate)) laborRate = 50m;
            if (!decimal.TryParse(TaxRateBox.Text, out var taxRate)) taxRate = 0.08m;
            if (!decimal.TryParse(CcFeeRateBox.Text, out var ccRate)) ccRate = 0.03m;

            if (!int.TryParse(PetsBox.Text, out var pets)) pets = 0;
            if (!int.TryParse(HouseholdSizeBox.Text, out var hh)) hh = 2;

            _currentQuote.LaborRate = laborRate;
            _currentQuote.TaxRate = taxRate;
            _currentQuote.CreditCardFeeRate = ccRate;
            _currentQuote.CreditCard = CreditCardCheck.IsChecked == true;
            _currentQuote.PetsCount = pets;
            _currentQuote.HouseholdSize = hh;
            _currentQuote.SmokingInside = SmokingCheck.IsChecked == true;
            _currentQuote.ServiceType = ServiceTypeBox.SelectedItem is ComboBoxItem serviceTypeItem
                ? serviceTypeItem.Content?.ToString() ?? ""
                : "";
            _currentQuote.ServiceFrequency = ServiceFrequencyBox.SelectedItem is ComboBoxItem frequencyItem
                ? frequencyItem.Content?.ToString() ?? ""
                : "";
            _currentQuote.LastProfessionalCleaning = LastProfessionalCleaningBox.SelectedItem is ComboBoxItem cleaningItem
                ? cleaningItem.Content?.ToString() ?? ""
                : "";

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


            var totals = _pricing.CalculateTotals(_currentQuote);

            HoursText = $"Hours: {totals.TotalLaborHours}";
            SubtotalText = $"Subtotal: {totals.Subtotal:C}";
            CcFeeText = $"CC Fee: {totals.CreditCardFee:C}";
            TaxText = $"Tax: {totals.Tax:C}";
            TotalText = $"Total: {totals.Total:C}";
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

            _currentQuote = q;

            LaborRateBox.Text = q.LaborRate.ToString();
            TaxRateBox.Text = q.TaxRate.ToString();
            CcFeeRateBox.Text = q.CreditCardFeeRate.ToString();
            CreditCardCheck.IsChecked = q.CreditCard;
            PetsBox.Text = q.PetsCount.ToString();
            HouseholdSizeBox.Text = q.HouseholdSize.ToString();
            SmokingCheck.IsChecked = q.SmokingInside;
            SetSelectedCombo(ServiceTypeBox, q.ServiceType);
            SetSelectedCombo(ServiceFrequencyBox, q.ServiceFrequency);
            SetSelectedCombo(LastProfessionalCleaningBox, q.LastProfessionalCleaning);

            _suppressQuoteNameTracking = true;
            QuoteNameBox.Text = q.QuoteName ?? "";
            _suppressQuoteNameTracking = false;
            _quoteNameManuallyEdited = !string.IsNullOrWhiteSpace(q.QuoteName);
            if (!_quoteNameManuallyEdited)
                UpdateQuoteNameIfAuto();

            _rooms.Clear();
            foreach (var r in q.Rooms)
                _rooms.Add(r);

            HoursText = $"Hours: {q.TotalLaborHours}";
            SubtotalText = $"Subtotal: {q.Subtotal:C}";
            CcFeeText = $"CC Fee: {q.CreditCardFee:C}";
            TaxText = $"Tax: {q.Tax:C}";
            TotalText = $"Total: {q.Total:C}";

            StatusText.Text = $"Loaded quote from {q.QuoteDate:MM/dd/yyyy}";
        }


        private void SaveQuote_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient == null)
            {
                MessageBox.Show("Select a client first.", "No client selected");
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


                LoadQuotesForSelectedClient(); // refresh list so you see changes

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

    }
}
