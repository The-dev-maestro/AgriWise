namespace Agriwise.Models;

public class ChemicalProduct
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Fongicide, Insecticide, Herbicide, Engrais bio
    public string ActiveSubstance { get; set; } = string.Empty;
    public double BaseDosePer15L { get; set; } // Grams or Milliliters
    public string Unit { get; set; } = "g"; // "g" or "ml"
    public int DarDays { get; set; }
    public string ToxicityClass { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#E67E22";
    public bool BeeWarning { get; set; }
    public string PpeRequired { get; set; } = "Gants + Masque + Bottes";
    public string TargetPests { get; set; } = string.Empty;
    public string TargetCrops { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public bool IsCertifiedMinader { get; set; } = true;
}
