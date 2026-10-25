namespace Agriwise.Models;

public class CultureWork
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ParcelId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Phytosanitaire, Fertilisation, Entretien, Récolte
    public DateTime Date { get; set; } = DateTime.Now;
    public string Product { get; set; } = string.Empty;
    public string DoseApplied { get; set; } = string.Empty;
    public int CostFCFA { get; set; }
    public bool IsDoseCompliant { get; set; } = true;
    public string AudioNoteText { get; set; } = string.Empty;
    public string DarBadgeText { get; set; } = string.Empty;
    public string CategoryColorHex { get; set; } = "#1A6D0E";
}
