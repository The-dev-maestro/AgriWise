namespace Agriwise.Models;

public class Parcel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Crop { get; set; } = string.Empty; // Tomate, Cacao, Maïs, Piment
    public string Variety { get; set; } = string.Empty; // Cobra F1, Forastero, CMS 8704
    public double AreaHa { get; set; }
    public double AreaM2 => AreaHa * 10000;
    public string Location { get; set; } = string.Empty;
    public int SoilMoisturePercent { get; set; } = 65;
    public string Status { get; set; } = string.Empty;
    public bool HasActiveDar { get; set; }
    public DateTime? LastTreatmentDate { get; set; }
    public string LastChemicalApplied { get; set; } = string.Empty;
    public int DarDaysTotal { get; set; }
    public DateTime? DarHarvestAuthorizedDate { get; set; }
    public string ColorAccentHex { get; set; } = "#27AE60";

    public int DarDaysRemaining
    {
        get
        {
            if (!HasActiveDar || !DarHarvestAuthorizedDate.HasValue) return 0;
            var remaining = (DarHarvestAuthorizedDate.Value.Date - DateTime.Now.Date).Days;
            return remaining > 0 ? remaining : 0;
        }
    }

    public int DarProgressPercent
    {
        get
        {
            if (!HasActiveDar || DarDaysTotal <= 0 || !LastTreatmentDate.HasValue) return 100;
            var daysElapsed = (DateTime.Now.Date - LastTreatmentDate.Value.Date).Days;
            if (daysElapsed < 0) daysElapsed = 0;
            var percent = (int)((daysElapsed / (double)DarDaysTotal) * 100);
            return Math.Min(100, Math.Max(0, percent));
        }
    }
}
