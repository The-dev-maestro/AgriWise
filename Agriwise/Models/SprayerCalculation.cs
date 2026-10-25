namespace Agriwise.Models;

public class SprayerCalculationRequest
{
    public string ParcelId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int SprayerCapacityLitres { get; set; } = 15; // 10, 15, 16, 20
    public string InfestationLevel { get; set; } = "Curatif"; // Preventif, Curatif, Severe
}

public class SprayerCalculationResult
{
    public double NetDosePerSprayer { get; set; }
    public string Unit { get; set; } = "g";
    public string MeasuringGuide { get; set; } = string.Empty;
    public int SprayersCountTotal { get; set; }
    public double TotalSprayVolumeLitres { get; set; }
    public int EstimatedSavingsFCFA { get; set; }
    public int DarDays { get; set; }
    public DateTime EarliestHarvestDate { get; set; }
    public bool IsDarCritical { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ParcelName { get; set; } = string.Empty;
    public List<string> PreparationSteps { get; set; } = new();
}
