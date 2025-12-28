using Cleaning_Quote.Data;
using Cleaning_Quote.Models;
using System.Collections.ObjectModel;
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
            pricing.DefaultRoomType = DefaultRoomTypeBox.SelectedItem as string ?? pricing.DefaultRoomType;
            pricing.DefaultRoomLevel = DefaultRoomLevelBox.SelectedItem as string ?? pricing.DefaultRoomLevel;
            pricing.DefaultRoomSize = DefaultRoomSizeBox.SelectedItem as string ?? pricing.DefaultRoomSize;
            if (DefaultRoomComplexityBox.SelectedItem is int roomComplexity)
                pricing.DefaultRoomComplexity = roomComplexity;
            pricing.DefaultSubItemType = DefaultSubItemTypeBox.SelectedItem as string ?? pricing.DefaultSubItemType;
            pricing.DefaultWindowSize = DefaultWindowSizeBox.SelectedItem as string ?? pricing.DefaultWindowSize;

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

            if (int.TryParse(RoomBathroomFullMinutesBox.Text, out var roomBathroomFullMinutes))
                pricing.RoomBathroomFullMinutes = roomBathroomFullMinutes;
            if (int.TryParse(RoomBathroomHalfMinutesBox.Text, out var roomBathroomHalfMinutes))
                pricing.RoomBathroomHalfMinutes = roomBathroomHalfMinutes;
            if (int.TryParse(RoomBathroomMasterMinutesBox.Text, out var roomBathroomMasterMinutes))
                pricing.RoomBathroomMasterMinutes = roomBathroomMasterMinutes;
            if (int.TryParse(RoomBedroomMinutesBox.Text, out var roomBedroomMinutes))
                pricing.RoomBedroomMinutes = roomBedroomMinutes;
            if (int.TryParse(RoomBedroomMasterMinutesBox.Text, out var roomBedroomMasterMinutes))
                pricing.RoomBedroomMasterMinutes = roomBedroomMasterMinutes;
            if (int.TryParse(RoomDiningRoomMinutesBox.Text, out var roomDiningRoomMinutes))
                pricing.RoomDiningRoomMinutes = roomDiningRoomMinutes;
            if (int.TryParse(RoomEntryMinutesBox.Text, out var roomEntryMinutes))
                pricing.RoomEntryMinutes = roomEntryMinutes;
            if (int.TryParse(RoomFamilyRoomMinutesBox.Text, out var roomFamilyRoomMinutes))
                pricing.RoomFamilyRoomMinutes = roomFamilyRoomMinutes;
            if (int.TryParse(RoomHallwayMinutesBox.Text, out var roomHallwayMinutes))
                pricing.RoomHallwayMinutes = roomHallwayMinutes;
            if (int.TryParse(RoomKitchenMinutesBox.Text, out var roomKitchenMinutes))
                pricing.RoomKitchenMinutes = roomKitchenMinutes;
            if (int.TryParse(RoomLaundryMinutesBox.Text, out var roomLaundryMinutes))
                pricing.RoomLaundryMinutes = roomLaundryMinutes;
            if (int.TryParse(RoomLivingRoomMinutesBox.Text, out var roomLivingRoomMinutes))
                pricing.RoomLivingRoomMinutes = roomLivingRoomMinutes;
            if (int.TryParse(RoomOfficeMinutesBox.Text, out var roomOfficeMinutes))
                pricing.RoomOfficeMinutes = roomOfficeMinutes;
            if (int.TryParse(SubItemCeilingFanMinutesBox.Text, out var subItemCeilingFanMinutes))
            {
                pricing.SubItemCeilingFanMinutes = subItemCeilingFanMinutes;
                pricing.CeilingFanHoursEach = subItemCeilingFanMinutes / 60m;
            }
            if (int.TryParse(SubItemFridgeMinutesBox.Text, out var subItemFridgeMinutes))
            {
                pricing.SubItemFridgeMinutes = subItemFridgeMinutes;
                pricing.FridgeHoursEach = subItemFridgeMinutes / 60m;
            }
            if (int.TryParse(SubItemMirrorMinutesBox.Text, out var subItemMirrorMinutes))
                pricing.SubItemMirrorMinutes = subItemMirrorMinutes;
            if (int.TryParse(SubItemOvenMinutesBox.Text, out var subItemOvenMinutes))
            {
                pricing.SubItemOvenMinutes = subItemOvenMinutes;
                pricing.OvenHoursEach = subItemOvenMinutes / 60m;
            }
            if (int.TryParse(SubItemShowerNoGlassMinutesBox.Text, out var subItemShowerNoGlassMinutes))
                pricing.SubItemShowerNoGlassMinutes = subItemShowerNoGlassMinutes;
            if (int.TryParse(SubItemShowerNoStoneMinutesBox.Text, out var subItemShowerNoStoneMinutes))
                pricing.SubItemShowerNoStoneMinutes = subItemShowerNoStoneMinutes;
            if (int.TryParse(SubItemSinkDiscountMinutesBox.Text, out var subItemSinkDiscountMinutes))
                pricing.SubItemSinkDiscountMinutes = subItemSinkDiscountMinutes;
            if (int.TryParse(SubItemStoveTopGasMinutesBox.Text, out var subItemStoveTopGasMinutes))
                pricing.SubItemStoveTopGasMinutes = subItemStoveTopGasMinutes;
            if (int.TryParse(SubItemTubMinutesBox.Text, out var subItemTubMinutes))
                pricing.SubItemTubMinutes = subItemTubMinutes;
            if (int.TryParse(SubItemWindowInsideFirstMinutesBox.Text, out var subItemWindowInsideFirstMinutes))
                pricing.SubItemWindowInsideFirstMinutes = subItemWindowInsideFirstMinutes;
            if (int.TryParse(SubItemWindowOutsideFirstMinutesBox.Text, out var subItemWindowOutsideFirstMinutes))
                pricing.SubItemWindowOutsideFirstMinutes = subItemWindowOutsideFirstMinutes;
            if (int.TryParse(SubItemWindowInsideSecondMinutesBox.Text, out var subItemWindowInsideSecondMinutes))
                pricing.SubItemWindowInsideSecondMinutes = subItemWindowInsideSecondMinutes;
            if (int.TryParse(SubItemWindowOutsideSecondMinutesBox.Text, out var subItemWindowOutsideSecondMinutes))
                pricing.SubItemWindowOutsideSecondMinutes = subItemWindowOutsideSecondMinutes;
            if (int.TryParse(SubItemWindowTrackMinutesBox.Text, out var subItemWindowTrackMinutes))
                pricing.SubItemWindowTrackMinutes = subItemWindowTrackMinutes;
            if (int.TryParse(SubItemWindowStandardMinutesBox.Text, out var subItemWindowStandardMinutes))
                pricing.SubItemWindowStandardMinutes = subItemWindowStandardMinutes;
        }

        private void PopulateInputs(ServiceTypePricing pricing)
        {
            if (pricing == null)
                return;

            _suppressLoad = true;
            DefaultRoomTypeBox.SelectedItem = pricing.DefaultRoomType;
            DefaultRoomLevelBox.SelectedItem = pricing.DefaultRoomLevel;
            DefaultRoomSizeBox.SelectedItem = pricing.DefaultRoomSize;
            DefaultRoomComplexityBox.SelectedItem = pricing.DefaultRoomComplexity;
            DefaultSubItemTypeBox.SelectedItem = pricing.DefaultSubItemType;
            DefaultWindowSizeBox.SelectedItem = pricing.DefaultWindowSize;
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
            RoomBathroomFullMinutesBox.Text = pricing.RoomBathroomFullMinutes.ToString();
            RoomBathroomHalfMinutesBox.Text = pricing.RoomBathroomHalfMinutes.ToString();
            RoomBathroomMasterMinutesBox.Text = pricing.RoomBathroomMasterMinutes.ToString();
            RoomBedroomMinutesBox.Text = pricing.RoomBedroomMinutes.ToString();
            RoomBedroomMasterMinutesBox.Text = pricing.RoomBedroomMasterMinutes.ToString();
            RoomDiningRoomMinutesBox.Text = pricing.RoomDiningRoomMinutes.ToString();
            RoomEntryMinutesBox.Text = pricing.RoomEntryMinutes.ToString();
            RoomFamilyRoomMinutesBox.Text = pricing.RoomFamilyRoomMinutes.ToString();
            RoomHallwayMinutesBox.Text = pricing.RoomHallwayMinutes.ToString();
            RoomKitchenMinutesBox.Text = pricing.RoomKitchenMinutes.ToString();
            RoomLaundryMinutesBox.Text = pricing.RoomLaundryMinutes.ToString();
            RoomLivingRoomMinutesBox.Text = pricing.RoomLivingRoomMinutes.ToString();
            RoomOfficeMinutesBox.Text = pricing.RoomOfficeMinutes.ToString();
            SubItemCeilingFanMinutesBox.Text = pricing.SubItemCeilingFanMinutes.ToString();
            SubItemFridgeMinutesBox.Text = pricing.SubItemFridgeMinutes.ToString();
            SubItemMirrorMinutesBox.Text = pricing.SubItemMirrorMinutes.ToString();
            SubItemOvenMinutesBox.Text = pricing.SubItemOvenMinutes.ToString();
            SubItemShowerNoGlassMinutesBox.Text = pricing.SubItemShowerNoGlassMinutes.ToString();
            SubItemShowerNoStoneMinutesBox.Text = pricing.SubItemShowerNoStoneMinutes.ToString();
            SubItemSinkDiscountMinutesBox.Text = pricing.SubItemSinkDiscountMinutes.ToString();
            SubItemStoveTopGasMinutesBox.Text = pricing.SubItemStoveTopGasMinutes.ToString();
            SubItemTubMinutesBox.Text = pricing.SubItemTubMinutes.ToString();
            SubItemWindowInsideFirstMinutesBox.Text = pricing.SubItemWindowInsideFirstMinutes.ToString();
            SubItemWindowOutsideFirstMinutesBox.Text = pricing.SubItemWindowOutsideFirstMinutes.ToString();
            SubItemWindowInsideSecondMinutesBox.Text = pricing.SubItemWindowInsideSecondMinutes.ToString();
            SubItemWindowOutsideSecondMinutesBox.Text = pricing.SubItemWindowOutsideSecondMinutes.ToString();
            SubItemWindowTrackMinutesBox.Text = pricing.SubItemWindowTrackMinutes.ToString();
            SubItemWindowStandardMinutesBox.Text = pricing.SubItemWindowStandardMinutes.ToString();
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
    }
}
