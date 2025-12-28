namespace CleaningQuote.Api.Models;

public record QuoteSummary(
    Guid QuoteId,
    string QuoteName,
    DateTime QuoteDate,
    decimal Total,
    string Status);

public class QuoteDetail
{
    public Guid QuoteId { get; set; }
    public Guid ClientId { get; set; }
    public DateTime QuoteDate { get; set; }
    public string QuoteName { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string ServiceFrequency { get; set; } = string.Empty;
    public string LastProfessionalCleaning { get; set; } = string.Empty;
    public decimal TotalSqFt { get; set; }
    public bool UseTotalSqFtOverride { get; set; }
    public string EntryInstructions { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentMethodOther { get; set; } = string.Empty;
    public bool FeedbackDiscussed { get; set; }
    public string Status { get; set; } = "Draft";
    public decimal LaborRate { get; set; }
    public decimal TaxRate { get; set; }
    public decimal CreditCardFeeRate { get; set; }
    public bool CreditCard { get; set; }
    public int PetsCount { get; set; }
    public int HouseholdSize { get; set; }
    public bool SmokingInside { get; set; }
    public decimal TotalLaborHours { get; set; }
    public decimal Subtotal { get; set; }
    public decimal CreditCardFee { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<QuoteRoomDetail> Rooms { get; set; } = new();
    public List<QuotePetDetail> Pets { get; set; } = new();
    public List<QuoteOccupantDetail> Occupants { get; set; } = new();
}

public class QuoteRoomDetail
{
    public Guid QuoteRoomId { get; set; }
    public Guid QuoteId { get; set; }
    public Guid? ParentRoomId { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public string Size { get; set; } = "M";
    public int Complexity { get; set; }
    public string Level { get; set; } = string.Empty;
    public string ItemCategory { get; set; } = string.Empty;
    public bool IsSubItem { get; set; }
    public bool IncludedInQuote { get; set; } = true;
    public decimal RoomLaborHours { get; set; }
    public decimal RoomAmount { get; set; }
    public string RoomNotes { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class QuotePetDetail
{
    public Guid QuotePetId { get; set; }
    public Guid QuoteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class QuoteOccupantDetail
{
    public Guid QuoteOccupantId { get; set; }
    public Guid QuoteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
