namespace Agriwise.Models;

public class ParcelFinancialStat
{
    public string ParcelId { get; set; } = string.Empty;
    public string ParcelName { get; set; } = string.Empty;
    public string Crop { get; set; } = string.Empty;
    public double AreaHa { get; set; }
    public int RevenueEstimatedFCFA { get; set; }
    public int ExpensesTotalFCFA { get; set; }
    public int NetMarginFCFA => RevenueEstimatedFCFA - ExpensesTotalFCFA;
    public int RoiPercent => ExpensesTotalFCFA > 0 ? (int)((NetMarginFCFA / (double)ExpensesTotalFCFA) * 100) : 0;
    public int MarginPerHaFCFA => AreaHa > 0 ? (int)(NetMarginFCFA / AreaHa) : 0;
    public int ChemicalInputPercent { get; set; }
    public int DirectSavingsDAR { get; set; }
    public int Rank { get; set; }
}

public class FarmFinancialSummary
{
    public int TotalProjectedNetMarginFCFA { get; set; }
    public int TotalRevenuesFCFA { get; set; }
    public int TotalExpensesFCFA { get; set; }
    public int OverallRoiPercent { get; set; }
    public int TotalSavingsAntiOverdoseFCFA { get; set; }
    public List<ParcelFinancialStat> RankedParcels { get; set; } = new();
}
