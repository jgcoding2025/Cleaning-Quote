using Cleaning_Quote.Data;
using Cleaning_Quote.Models;
using System.Windows;
using System.Windows.Controls;

namespace Cleaning_Quote
{
    public partial class ComplexityReferenceWindow : Window
    {
        private readonly ServiceTypePricingRepository _serviceTypePricingRepo = new ServiceTypePricingRepository();
        private bool _suppressLoad;

        public ComplexityReferenceWindow(string selectedServiceType)
        {
            InitializeComponent();
            ServiceTypeBox.SelectedIndex = 0;
            SetSelectedCombo(ServiceTypeBox, selectedServiceType);
            LoadDefinitions(GetSelectedServiceType());
        }

        private void ServiceTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressLoad)
                return;

            LoadDefinitions(GetSelectedServiceType());
        }

        private void LoadDefinitions(string serviceType)
        {
            if (string.IsNullOrWhiteSpace(serviceType))
                return;

            ServiceTypePricing pricing = _serviceTypePricingRepo.GetOrCreate(serviceType);

            _suppressLoad = true;
            Complexity1DefinitionText.Text = FormatDefinition(pricing.Complexity1Definition);
            Complexity2DefinitionText.Text = FormatDefinition(pricing.Complexity2Definition);
            Complexity3DefinitionText.Text = FormatDefinition(pricing.Complexity3Definition);
            _suppressLoad = false;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private string GetSelectedServiceType()
        {
            return ServiceTypeBox.SelectedItem is ComboBoxItem serviceTypeItem
                ? serviceTypeItem.Content?.ToString() ?? ""
                : "";
        }

        private static string FormatDefinition(string definition)
        {
            return string.IsNullOrWhiteSpace(definition)
                ? "No definition set for this service type yet."
                : definition;
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
