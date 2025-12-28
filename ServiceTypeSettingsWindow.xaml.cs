using Cleaning_Quote.Data;
using Cleaning_Quote.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Cleaning_Quote
{
    public partial class ServiceTypeSettingsWindow : Window
    {
        private readonly ServiceTypePricingRepository _serviceTypePricingRepo = new ServiceTypePricingRepository();
        private ServiceTypePricing _currentPricing;
        private bool _suppressLoad;
        private readonly ServiceCatalog _serviceCatalog;
        private readonly ObservableCollection<ServiceTypeStandard> _serviceTypeStandards;

        public ServiceTypeSettingsWindow(string selectedServiceType)
        {
            InitializeComponent();
            _serviceCatalog = ServiceCatalogStore.Load();
            _serviceTypeStandards = new ObservableCollection<ServiceTypeStandard>(ServiceTypeStandardsStore.Load(_serviceCatalog));
            ServiceTypeStandardsGrid.ItemsSource = _serviceTypeStandards;
            ServiceTypeBox.ItemsSource = _serviceCatalog.ServiceTypes;
            DefaultSubItemTypeBox.ItemsSource = _serviceCatalog.SubItems;
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
            PopulateInputs(_currentPricing);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ServiceTypeStandardsGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            ServiceTypeStandardsGrid.CommitEdit(DataGridEditingUnit.Row, true);
            ServiceTypeStandardsStore.Save(_serviceTypeStandards);

            if (_currentPricing == null)
            {
                Close();
                return;
            }

            UpdateServiceTypePricingFromInputs(_currentPricing);
            _serviceTypePricingRepo.Upsert(_currentPricing);
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ResetToDefault_Click(object sender, RoutedEventArgs e)
        {
            var serviceType = GetSelectedServiceType();
            if (string.IsNullOrWhiteSpace(serviceType))
                return;

            _currentPricing = ServiceTypePricing.Default(serviceType);
            PopulateInputs(_currentPricing);
        }

        private void UpdateServiceTypePricingFromInputs(ServiceTypePricing pricing)
        {
            var multiplier = GetSelectedServiceTypeMultiplier();
            pricing.DefaultRoomType = DefaultRoomTypeBox.SelectedItem as string ?? pricing.DefaultRoomType;
            pricing.DefaultRoomLevel = DefaultRoomLevelBox.SelectedItem as string ?? pricing.DefaultRoomLevel;
            pricing.DefaultRoomSize = DefaultRoomSizeBox.SelectedItem as string ?? pricing.DefaultRoomSize;
            if (DefaultRoomComplexityBox.SelectedItem is int roomComplexity)
                pricing.DefaultRoomComplexity = roomComplexity;
            pricing.DefaultSubItemType = DefaultSubItemTypeBox.SelectedItem as string ?? pricing.DefaultSubItemType;

            if (decimal.TryParse(SizeSmallMultiplierBox.Text, out var sizeSmallMultiplier))
                pricing.SizeSmallMultiplier = sizeSmallMultiplier;
            if (decimal.TryParse(SizeMediumMultiplierBox.Text, out var sizeMediumMultiplier))
                pricing.SizeMediumMultiplier = sizeMediumMultiplier;
            if (decimal.TryParse(SizeLargeMultiplierBox.Text, out var sizeLargeMultiplier))
                pricing.SizeLargeMultiplier = sizeLargeMultiplier;
            pricing.SizeSmallDefinition = SizeSmallDefinitionBox.Text?.Trim() ?? "";
            pricing.SizeMediumDefinition = SizeMediumDefinitionBox.Text?.Trim() ?? "";
            pricing.SizeLargeDefinition = SizeLargeDefinitionBox.Text?.Trim() ?? "";
            if (decimal.TryParse(Complexity1MultiplierBox.Text, out var mult1))
                pricing.Complexity1Multiplier = mult1;
            if (decimal.TryParse(Complexity2MultiplierBox.Text, out var mult2))
                pricing.Complexity2Multiplier = mult2;
            if (decimal.TryParse(Complexity3MultiplierBox.Text, out var mult3))
                pricing.Complexity3Multiplier = mult3;
            pricing.Complexity1Definition = Complexity1DefinitionBox.Text?.Trim() ?? "";
            pricing.Complexity2Definition = Complexity2DefinitionBox.Text?.Trim() ?? "";
            pricing.Complexity3Definition = Complexity3DefinitionBox.Text?.Trim() ?? "";

            if (TryParseScaledMinutes(RoomBathroomFullMinutesBox.Text, multiplier, out var roomBathroomFullMinutes))
                pricing.RoomBathroomFullMinutes = roomBathroomFullMinutes;
            if (TryParseScaledMinutes(RoomBathroomHalfMinutesBox.Text, multiplier, out var roomBathroomHalfMinutes))
                pricing.RoomBathroomHalfMinutes = roomBathroomHalfMinutes;
            if (TryParseScaledMinutes(RoomBathroomMasterMinutesBox.Text, multiplier, out var roomBathroomMasterMinutes))
                pricing.RoomBathroomMasterMinutes = roomBathroomMasterMinutes;
            if (TryParseScaledMinutes(RoomBedroomMinutesBox.Text, multiplier, out var roomBedroomMinutes))
                pricing.RoomBedroomMinutes = roomBedroomMinutes;
            if (TryParseScaledMinutes(RoomBedroomMasterMinutesBox.Text, multiplier, out var roomBedroomMasterMinutes))
                pricing.RoomBedroomMasterMinutes = roomBedroomMasterMinutes;
            if (TryParseScaledMinutes(RoomDiningRoomMinutesBox.Text, multiplier, out var roomDiningRoomMinutes))
                pricing.RoomDiningRoomMinutes = roomDiningRoomMinutes;
            if (TryParseScaledMinutes(RoomEntryMinutesBox.Text, multiplier, out var roomEntryMinutes))
                pricing.RoomEntryMinutes = roomEntryMinutes;
            if (TryParseScaledMinutes(RoomFamilyRoomMinutesBox.Text, multiplier, out var roomFamilyRoomMinutes))
                pricing.RoomFamilyRoomMinutes = roomFamilyRoomMinutes;
            if (TryParseScaledMinutes(RoomHallwayMinutesBox.Text, multiplier, out var roomHallwayMinutes))
                pricing.RoomHallwayMinutes = roomHallwayMinutes;
            if (TryParseScaledMinutes(RoomKitchenMinutesBox.Text, multiplier, out var roomKitchenMinutes))
                pricing.RoomKitchenMinutes = roomKitchenMinutes;
            if (TryParseScaledMinutes(RoomLaundryMinutesBox.Text, multiplier, out var roomLaundryMinutes))
                pricing.RoomLaundryMinutes = roomLaundryMinutes;
            if (TryParseScaledMinutes(RoomLivingRoomMinutesBox.Text, multiplier, out var roomLivingRoomMinutes))
                pricing.RoomLivingRoomMinutes = roomLivingRoomMinutes;
            if (TryParseScaledMinutes(RoomOfficeMinutesBox.Text, multiplier, out var roomOfficeMinutes))
                pricing.RoomOfficeMinutes = roomOfficeMinutes;
            if (TryParseScaledMinutes(SubItemCeilingFanMinutesBox.Text, multiplier, out var subItemCeilingFanMinutes))
                pricing.SubItemCeilingFanMinutes = subItemCeilingFanMinutes;
            if (TryParseScaledMinutes(SubItemFridgeMinutesBox.Text, multiplier, out var subItemFridgeMinutes))
                pricing.SubItemFridgeMinutes = subItemFridgeMinutes;
            if (TryParseScaledMinutes(SubItemMirrorMinutesBox.Text, multiplier, out var subItemMirrorMinutes))
                pricing.SubItemMirrorMinutes = subItemMirrorMinutes;
            if (TryParseScaledMinutes(SubItemOvenMinutesBox.Text, multiplier, out var subItemOvenMinutes))
                pricing.SubItemOvenMinutes = subItemOvenMinutes;
            if (TryParseScaledMinutes(SubItemShowerNoGlassMinutesBox.Text, multiplier, out var subItemShowerNoGlassMinutes))
                pricing.SubItemShowerNoGlassMinutes = subItemShowerNoGlassMinutes;
            if (TryParseScaledMinutes(SubItemShowerNoStoneMinutesBox.Text, multiplier, out var subItemShowerNoStoneMinutes))
                pricing.SubItemShowerNoStoneMinutes = subItemShowerNoStoneMinutes;
            if (TryParseScaledMinutes(SubItemSinkDiscountMinutesBox.Text, multiplier, out var subItemSinkDiscountMinutes))
                pricing.SubItemSinkDiscountMinutes = subItemSinkDiscountMinutes;
            if (TryParseScaledMinutes(SubItemStoveTopGasMinutesBox.Text, multiplier, out var subItemStoveTopGasMinutes))
                pricing.SubItemStoveTopGasMinutes = subItemStoveTopGasMinutes;
            if (TryParseScaledMinutes(SubItemTubMinutesBox.Text, multiplier, out var subItemTubMinutes))
                pricing.SubItemTubMinutes = subItemTubMinutes;
            if (TryParseScaledMinutes(SubItemWindowInsideFirstMinutesBox.Text, multiplier, out var subItemWindowInsideFirstMinutes))
                pricing.SubItemWindowInsideFirstMinutes = subItemWindowInsideFirstMinutes;
            if (TryParseScaledMinutes(SubItemWindowOutsideFirstMinutesBox.Text, multiplier, out var subItemWindowOutsideFirstMinutes))
                pricing.SubItemWindowOutsideFirstMinutes = subItemWindowOutsideFirstMinutes;
            if (TryParseScaledMinutes(SubItemWindowInsideSecondMinutesBox.Text, multiplier, out var subItemWindowInsideSecondMinutes))
                pricing.SubItemWindowInsideSecondMinutes = subItemWindowInsideSecondMinutes;
            if (TryParseScaledMinutes(SubItemWindowOutsideSecondMinutesBox.Text, multiplier, out var subItemWindowOutsideSecondMinutes))
                pricing.SubItemWindowOutsideSecondMinutes = subItemWindowOutsideSecondMinutes;
            if (TryParseScaledMinutes(SubItemWindowTrackMinutesBox.Text, multiplier, out var subItemWindowTrackMinutes))
                pricing.SubItemWindowTrackMinutes = subItemWindowTrackMinutes;
            if (TryParseScaledMinutes(SubItemWindowStandardMinutesBox.Text, multiplier, out var subItemWindowStandardMinutes))
                pricing.SubItemWindowStandardMinutes = subItemWindowStandardMinutes;
        }

        private void PopulateInputs(ServiceTypePricing pricing)
        {
            if (pricing == null)
                return;

            _suppressLoad = true;
            var multiplier = GetSelectedServiceTypeMultiplier();
            DefaultRoomTypeBox.SelectedItem = pricing.DefaultRoomType;
            DefaultRoomLevelBox.SelectedItem = pricing.DefaultRoomLevel;
            DefaultRoomSizeBox.SelectedItem = pricing.DefaultRoomSize;
            DefaultRoomComplexityBox.SelectedItem = pricing.DefaultRoomComplexity;
            DefaultSubItemTypeBox.SelectedItem = pricing.DefaultSubItemType;
            SizeSmallMultiplierBox.Text = pricing.SizeSmallMultiplier.ToString();
            SizeMediumMultiplierBox.Text = pricing.SizeMediumMultiplier.ToString();
            SizeLargeMultiplierBox.Text = pricing.SizeLargeMultiplier.ToString();
            SizeSmallDefinitionBox.Text = pricing.SizeSmallDefinition ?? "";
            SizeMediumDefinitionBox.Text = pricing.SizeMediumDefinition ?? "";
            SizeLargeDefinitionBox.Text = pricing.SizeLargeDefinition ?? "";
            Complexity1MultiplierBox.Text = pricing.Complexity1Multiplier.ToString();
            Complexity2MultiplierBox.Text = pricing.Complexity2Multiplier.ToString();
            Complexity3MultiplierBox.Text = pricing.Complexity3Multiplier.ToString();
            Complexity1DefinitionBox.Text = pricing.Complexity1Definition ?? "";
            Complexity2DefinitionBox.Text = pricing.Complexity2Definition ?? "";
            Complexity3DefinitionBox.Text = pricing.Complexity3Definition ?? "";
            RoomBathroomFullMinutesBox.Text = FormatScaledMinutes(pricing.RoomBathroomFullMinutes, multiplier);
            RoomBathroomHalfMinutesBox.Text = FormatScaledMinutes(pricing.RoomBathroomHalfMinutes, multiplier);
            RoomBathroomMasterMinutesBox.Text = FormatScaledMinutes(pricing.RoomBathroomMasterMinutes, multiplier);
            RoomBedroomMinutesBox.Text = FormatScaledMinutes(pricing.RoomBedroomMinutes, multiplier);
            RoomBedroomMasterMinutesBox.Text = FormatScaledMinutes(pricing.RoomBedroomMasterMinutes, multiplier);
            RoomDiningRoomMinutesBox.Text = FormatScaledMinutes(pricing.RoomDiningRoomMinutes, multiplier);
            RoomEntryMinutesBox.Text = FormatScaledMinutes(pricing.RoomEntryMinutes, multiplier);
            RoomFamilyRoomMinutesBox.Text = FormatScaledMinutes(pricing.RoomFamilyRoomMinutes, multiplier);
            RoomHallwayMinutesBox.Text = FormatScaledMinutes(pricing.RoomHallwayMinutes, multiplier);
            RoomKitchenMinutesBox.Text = FormatScaledMinutes(pricing.RoomKitchenMinutes, multiplier);
            RoomLaundryMinutesBox.Text = FormatScaledMinutes(pricing.RoomLaundryMinutes, multiplier);
            RoomLivingRoomMinutesBox.Text = FormatScaledMinutes(pricing.RoomLivingRoomMinutes, multiplier);
            RoomOfficeMinutesBox.Text = FormatScaledMinutes(pricing.RoomOfficeMinutes, multiplier);
            SubItemCeilingFanMinutesBox.Text = FormatScaledMinutes(pricing.SubItemCeilingFanMinutes, multiplier);
            SubItemFridgeMinutesBox.Text = FormatScaledMinutes(pricing.SubItemFridgeMinutes, multiplier);
            SubItemMirrorMinutesBox.Text = FormatScaledMinutes(pricing.SubItemMirrorMinutes, multiplier);
            SubItemOvenMinutesBox.Text = FormatScaledMinutes(pricing.SubItemOvenMinutes, multiplier);
            SubItemShowerNoGlassMinutesBox.Text = FormatScaledMinutes(pricing.SubItemShowerNoGlassMinutes, multiplier);
            SubItemShowerNoStoneMinutesBox.Text = FormatScaledMinutes(pricing.SubItemShowerNoStoneMinutes, multiplier);
            SubItemSinkDiscountMinutesBox.Text = FormatScaledMinutes(pricing.SubItemSinkDiscountMinutes, multiplier);
            SubItemStoveTopGasMinutesBox.Text = FormatScaledMinutes(pricing.SubItemStoveTopGasMinutes, multiplier);
            SubItemTubMinutesBox.Text = FormatScaledMinutes(pricing.SubItemTubMinutes, multiplier);
            SubItemWindowInsideFirstMinutesBox.Text = FormatScaledMinutes(pricing.SubItemWindowInsideFirstMinutes, multiplier);
            SubItemWindowOutsideFirstMinutesBox.Text = FormatScaledMinutes(pricing.SubItemWindowOutsideFirstMinutes, multiplier);
            SubItemWindowInsideSecondMinutesBox.Text = FormatScaledMinutes(pricing.SubItemWindowInsideSecondMinutes, multiplier);
            SubItemWindowOutsideSecondMinutesBox.Text = FormatScaledMinutes(pricing.SubItemWindowOutsideSecondMinutes, multiplier);
            SubItemWindowTrackMinutesBox.Text = FormatScaledMinutes(pricing.SubItemWindowTrackMinutes, multiplier);
            SubItemWindowStandardMinutesBox.Text = FormatScaledMinutes(pricing.SubItemWindowStandardMinutes, multiplier);
            _suppressLoad = false;
        }

        private string GetSelectedServiceType()
        {
            return ServiceTypeBox.SelectedItem as string ?? "";
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

                if (item is string stringItem && stringItem == value)
                {
                    comboBox.SelectedItem = stringItem;
                    return;
                }
            }
        }

        private decimal GetSelectedServiceTypeMultiplier()
        {
            var serviceType = GetSelectedServiceType();
            var standard = _serviceTypeStandards.FirstOrDefault(item =>
                string.Equals(item.ServiceType, serviceType, StringComparison.OrdinalIgnoreCase));
            var multiplier = standard?.Multiplier ?? 1m;
            return multiplier <= 0m ? 1m : multiplier;
        }

        private static string FormatScaledMinutes(int minutes, decimal multiplier)
        {
            var adjusted = minutes * (multiplier <= 0m ? 1m : multiplier);
            var rounded = (int)Math.Round(adjusted, MidpointRounding.AwayFromZero);
            return rounded.ToString();
        }

        private static bool TryParseScaledMinutes(string text, decimal multiplier, out int baseMinutes)
        {
            baseMinutes = 0;
            if (!decimal.TryParse(text, out var displayedMinutes))
                return false;

            var safeMultiplier = multiplier <= 0m ? 1m : multiplier;
            var baseValue = displayedMinutes / safeMultiplier;
            baseMinutes = (int)Math.Round(baseValue, MidpointRounding.AwayFromZero);
            return true;
        }
    }
}
