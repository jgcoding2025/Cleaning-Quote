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
            FullGlassShowerComplexityBox.Text = _currentPricing.FullGlassShowerComplexity.ToString();
            PebbleStoneFloorHoursBox.Text = _currentPricing.PebbleStoneFloorHoursEach.ToString();
            PebbleStoneFloorComplexityBox.Text = _currentPricing.PebbleStoneFloorComplexity.ToString();
            FridgeHoursBox.Text = _currentPricing.FridgeHoursEach.ToString();
            FridgeComplexityBox.Text = _currentPricing.FridgeComplexity.ToString();
            OvenHoursBox.Text = _currentPricing.OvenHoursEach.ToString();
            OvenComplexityBox.Text = _currentPricing.OvenComplexity.ToString();
            CeilingFanHoursBox.Text = _currentPricing.CeilingFanHoursEach.ToString();
            CeilingFanComplexityBox.Text = _currentPricing.CeilingFanComplexity.ToString();
            WindowSmallHoursBox.Text = _currentPricing.WindowSmallHoursEach.ToString();
            WindowMediumHoursBox.Text = _currentPricing.WindowMediumHoursEach.ToString();
            WindowLargeHoursBox.Text = _currentPricing.WindowLargeHoursEach.ToString();
            WindowComplexityBox.Text = _currentPricing.WindowComplexity.ToString();
            FirstCleanRateBox.Text = _currentPricing.FirstCleanRate.ToString();
            FirstCleanMinimumBox.Text = _currentPricing.FirstCleanMinimum.ToString();
            DeepCleanRateBox.Text = _currentPricing.DeepCleanRate.ToString();
            DeepCleanMinimumBox.Text = _currentPricing.DeepCleanMinimum.ToString();
            MaintenanceRateBox.Text = _currentPricing.MaintenanceRate.ToString();
            MaintenanceMinimumBox.Text = _currentPricing.MaintenanceMinimum.ToString();
            OneTimeDeepCleanRateBox.Text = _currentPricing.OneTimeDeepCleanRate.ToString();
            OneTimeDeepCleanMinimumBox.Text = _currentPricing.OneTimeDeepCleanMinimum.ToString();
            WindowInsideRateBox.Text = _currentPricing.WindowInsideRate.ToString();
            WindowOutsideRateBox.Text = _currentPricing.WindowOutsideRate.ToString();
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
            if (int.TryParse(FullGlassShowerComplexityBox.Text, out var glassComplexity))
                pricing.FullGlassShowerComplexity = glassComplexity;
            if (decimal.TryParse(PebbleStoneFloorHoursBox.Text, out var pebbleHours))
                pricing.PebbleStoneFloorHoursEach = pebbleHours;
            if (int.TryParse(PebbleStoneFloorComplexityBox.Text, out var pebbleComplexity))
                pricing.PebbleStoneFloorComplexity = pebbleComplexity;
            if (decimal.TryParse(FridgeHoursBox.Text, out var fridgeHours))
                pricing.FridgeHoursEach = fridgeHours;
            if (int.TryParse(FridgeComplexityBox.Text, out var fridgeComplexity))
                pricing.FridgeComplexity = fridgeComplexity;
            if (decimal.TryParse(OvenHoursBox.Text, out var ovenHours))
                pricing.OvenHoursEach = ovenHours;
            if (int.TryParse(OvenComplexityBox.Text, out var ovenComplexity))
                pricing.OvenComplexity = ovenComplexity;
            if (decimal.TryParse(CeilingFanHoursBox.Text, out var ceilingFanHours))
                pricing.CeilingFanHoursEach = ceilingFanHours;
            if (int.TryParse(CeilingFanComplexityBox.Text, out var ceilingFanComplexity))
                pricing.CeilingFanComplexity = ceilingFanComplexity;
            if (decimal.TryParse(WindowSmallHoursBox.Text, out var windowSmallHours))
                pricing.WindowSmallHoursEach = windowSmallHours;
            if (decimal.TryParse(WindowMediumHoursBox.Text, out var windowMediumHours))
                pricing.WindowMediumHoursEach = windowMediumHours;
            if (decimal.TryParse(WindowLargeHoursBox.Text, out var windowLargeHours))
                pricing.WindowLargeHoursEach = windowLargeHours;
            if (int.TryParse(WindowComplexityBox.Text, out var windowComplexity))
                pricing.WindowComplexity = windowComplexity;
            if (decimal.TryParse(FirstCleanRateBox.Text, out var firstCleanRate))
                pricing.FirstCleanRate = firstCleanRate;
            if (decimal.TryParse(FirstCleanMinimumBox.Text, out var firstCleanMinimum))
                pricing.FirstCleanMinimum = firstCleanMinimum;
            if (decimal.TryParse(DeepCleanRateBox.Text, out var deepCleanRate))
                pricing.DeepCleanRate = deepCleanRate;
            if (decimal.TryParse(DeepCleanMinimumBox.Text, out var deepCleanMinimum))
                pricing.DeepCleanMinimum = deepCleanMinimum;
            if (decimal.TryParse(MaintenanceRateBox.Text, out var maintenanceRate))
                pricing.MaintenanceRate = maintenanceRate;
            if (decimal.TryParse(MaintenanceMinimumBox.Text, out var maintenanceMinimum))
                pricing.MaintenanceMinimum = maintenanceMinimum;
            if (decimal.TryParse(OneTimeDeepCleanRateBox.Text, out var oneTimeDeepRate))
                pricing.OneTimeDeepCleanRate = oneTimeDeepRate;
            if (decimal.TryParse(OneTimeDeepCleanMinimumBox.Text, out var oneTimeDeepMinimum))
                pricing.OneTimeDeepCleanMinimum = oneTimeDeepMinimum;
            if (decimal.TryParse(WindowInsideRateBox.Text, out var windowInsideRate))
                pricing.WindowInsideRate = windowInsideRate;
            if (decimal.TryParse(WindowOutsideRateBox.Text, out var windowOutsideRate))
                pricing.WindowOutsideRate = windowOutsideRate;

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
