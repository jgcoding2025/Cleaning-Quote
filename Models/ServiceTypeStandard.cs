namespace Cleaning_Quote.Models
{
    public class ServiceTypeStandard
    {
        public string ServiceType { get; set; } = "";
        public decimal Rate { get; set; }
        public decimal Multiplier { get; set; } = 1m;
    }
}
