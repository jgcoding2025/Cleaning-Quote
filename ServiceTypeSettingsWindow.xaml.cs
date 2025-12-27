using Cleaning_Quote.Data;
using Cleaning_Quote.Models;
using System.Windows;
using System.Windows.Controls;

namespace Cleaning_Quote
{
    public partial class ServiceTypeSettingsWindow : Window
    {
        private readonly ServiceTypePricingRepository _serviceTypePricingRepo = new ServiceTypePricingRepository();
        private ServiceTypePricing _currentPricing;
        private bool _suppressLoad;

        public ServiceTypeSettingsWindow(string selectedServiceType)
        {
            InitializeComponent();
            ServiceTypeBox.SelectedIndex = 0;
            SetSelectedCombo(ServiceTypeBox, selectedServiceType);
            LoadServiceTypePricing(GetSelectedServiceType());
        }

        private void ServiceTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressLoad)
                return;

            LoadServiceTypePricing(GetSelectedServiceType());
        }

        private void LoadServiceTypePricing(string serviceType)
        {
            if (string.IsNullOrWhiteSpace(serviceType))
                return;

            _currentPricing = _serviceTypePricingRepo.GetOrCreate(serviceType);

            _suppressLoad = true;
            SqFtPerLaborHourBox.Text = _currentPricing.SqFtPerLaborHour.ToString();
            SizeSmallSqFtBox.Text = _currentPricing.SizeSmallSqFt.ToString();
            SizeMediumSqFtBox.Text = _currentPricing.SizeMediumSqFt.ToString();
            SizeLargeSqFtBox.Text = _currentPricing.SizeLargeSqFt.ToString();
            Complexity1MultiplierBox.Text = _currentPricing.Complexity1Multiplier.ToString();
            Complexity2MultiplierBox.Text = _currentPricing.Complexity2Multiplier.ToString();
            Complexity3MultiplierBox.Text = _currentPricing.Complexity3Multiplier.ToString();
            FullGlassShowerHoursBox.Text = _currentPricing.FullGlassShowerHoursEach.ToString();
            PebbleStoneFloorHoursBox.Text = _currentPricing.PebbleStoneFloorHoursEach.ToString();
            FridgeHoursBox.Text = _currentPricing.FridgeHoursEach.ToString();
            OvenHoursBox.Text = _currentPricing.OvenHoursEach.ToString();
            Complexity1DefinitionBox.Text = _currentPricing.Complexity1Definition ?? "";
            Complexity2DefinitionBox.Text = _currentPricing.Complexity2Definition ?? "";
            Complexity3DefinitionBox.Text = _currentPricing.Complexity3Definition ?? "";
            _suppressLoad = false;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPricing == null)
                return;

            UpdateServiceTypePricingFromInputs(_currentPricing);
            _serviceTypePricingRepo.Upsert(_currentPricing);
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void UpdateServiceTypePricingFromInputs(ServiceTypePricing pricing)
        {
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

            pricing.Complexity1Definition = Complexity1DefinitionBox.Text?.Trim() ?? "";
            pricing.Complexity2Definition = Complexity2DefinitionBox.Text?.Trim() ?? "";
            pricing.Complexity3Definition = Complexity3DefinitionBox.Text?.Trim() ?? "";
        }

        private string GetSelectedServiceType()
        {
            return ServiceTypeBox.SelectedItem is ComboBoxItem serviceTypeItem
                ? serviceTypeItem.Content?.ToString() ?? ""
                : "";
        }

        private void SetSelectedCombo(ComboBox comboBox, string value)
        {
            if (comboBox == null || string.IsNullOrWhiteSpace(value))
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
