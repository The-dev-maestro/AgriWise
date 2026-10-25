using System.Text.Json;
using Agriwise.Models;

namespace Agriwise.Services;

public interface IDataService
{
    Task InitializeAsync();
    Task<List<Parcel>> GetParcelsAsync();
    Task<Parcel?> GetParcelByIdAsync(string id);
    Task SaveParcelAsync(Parcel parcel);
    Task<List<CultureWork>> GetCultureWorksAsync(string parcelId);
    Task AddCultureWorkAsync(CultureWork work);
    Task<List<ChemicalProduct>> GetChemicalProductsAsync();
    Task<ChemicalProduct?> GetChemicalProductByIdAsync(string id);
    Task RecordSprayerTreatmentAsync(string parcelId, string productId, SprayerCalculationResult calc);
    Task<FarmFinancialSummary> GetFinancialSummaryAsync();
    Task AddTransactionAsync(string parcelId, int amountFCFA, string description, bool isExpense);
}

public class DataService : IDataService
{
    private static DataService? _current;
    public static DataService Current => _current ??= new DataService();

    private readonly string _dbFilePath;
    private List<Parcel> _parcels = new();
    private List<CultureWork> _works = new();
    private List<ChemicalProduct> _products = new();
    private bool _isInitialized;

    public DataService()
    {
        _dbFilePath = Path.Combine(FileSystem.AppDataDirectory, "agriwise_database.json");
    }

    private class DatabaseEnvelope
    {
        public List<Parcel> Parcels { get; set; } = new();
        public List<CultureWork> Works { get; set; } = new();
        public List<ChemicalProduct> Products { get; set; } = new();
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            if (File.Exists(_dbFilePath))
            {
                var json = await File.ReadAllTextAsync(_dbFilePath);
                var envelope = JsonSerializer.Deserialize<DatabaseEnvelope>(json);
                if (envelope != null && envelope.Parcels.Count > 0)
                {
                    _parcels = envelope.Parcels;
                    _works = envelope.Works;
                    _products = envelope.Products;
                    _isInitialized = true;
                    return;
                }
            }
        }
        catch
        {
            // Fallback to seed on parse error
        }

        SeedInitialData();
        await SaveToDiskAsync();
        _isInitialized = true;
    }

    private void SeedInitialData()
    {
        // 1. Initial Parcels (Foumbot, Ouest Cameroun)
        var parcelB = new Parcel
        {
            Id = "parcel-b",
            Name = "Parcelle B - Tomate Maraîchère",
            Crop = "Tomate",
            Variety = "Cobra F1",
            AreaHa = 0.8,
            Location = "Planche d'irrigation 2 · Foumbot",
            SoilMoisturePercent = 68,
            Status = "En quarantaine DAR (11j)",
            HasActiveDar = true,
            LastTreatmentDate = DateTime.Now.Date.AddDays(-3),
            LastChemicalApplied = "Fongicide Mancozèbe 80 WP",
            DarDaysTotal = 14,
            DarHarvestAuthorizedDate = DateTime.Now.Date.AddDays(11),
            ColorAccentHex = "#E67E22"
        };

        var parcelA = new Parcel
        {
            Id = "parcel-a",
            Name = "Parcelle A - Cacao Hybride",
            Crop = "Cacao",
            Variety = "Forastero",
            AreaHa = 2.5,
            Location = "Secteur Bas-Fond Nord · Foumbot",
            SoilMoisturePercent = 74,
            Status = "Récolte autorisée",
            HasActiveDar = false,
            LastTreatmentDate = DateTime.Now.Date.AddDays(-18),
            LastChemicalApplied = "Fumier organique composté",
            DarDaysTotal = 0,
            DarHarvestAuthorizedDate = null,
            ColorAccentHex = "#27AE60"
        };

        var parcelC = new Parcel
        {
            Id = "parcel-c",
            Name = "Parcelle C - Maïs Hybride",
            Crop = "Maïs",
            Variety = "CMS 8704",
            AreaHa = 1.2,
            Location = "Versant Sud · Foumbot",
            SoilMoisturePercent = 62,
            Status = "Stade floraison",
            HasActiveDar = false,
            LastTreatmentDate = DateTime.Now.Date.AddDays(-25),
            LastChemicalApplied = "Engrais NPK 20-10-10",
            DarDaysTotal = 0,
            DarHarvestAuthorizedDate = null,
            ColorAccentHex = "#013A13"
        };

        _parcels = new List<Parcel> { parcelB, parcelA, parcelC };

        // 2. Initial Operations for Parcel B Journal
        _works = new List<CultureWork>
        {
            new CultureWork
            {
                Id = "w1",
                ParcelId = "parcel-b",
                Title = "Traitement phytosanitaire fongicide",
                Category = "Phytosanitaire",
                Date = DateTime.Now.Date.AddDays(-3),
                Product = "Mancozèbe / Manèbe 80 WP",
                DoseApplied = "150 g / pulvérisateur 15L (4 cuves)",
                CostFCFA = 14000,
                IsDoseCompliant = true,
                DarBadgeText = "DAR 14j",
                CategoryColorHex = "#9C4400",
                AudioNoteText = "Pulvérisation tôt le matin, port du masque et gants respecté."
            },
            new CultureWork
            {
                Id = "w2",
                ParcelId = "parcel-b",
                Title = "Fertilisation minérale de couverture",
                Category = "Fertilisation",
                Date = DateTime.Now.Date.AddDays(-15),
                Product = "Engrais NPK 20-10-10 (50 kg)",
                DoseApplied = "Application en couronne autour des plants",
                CostFCFA = 34500,
                IsDoseCompliant = true,
                DarBadgeText = "Sans DAR",
                CategoryColorHex = "#1A6D0E",
                AudioNoteText = "Enfouissement léger après arrosage régulier."
            },
            new CultureWork
            {
                Id = "w3",
                ParcelId = "parcel-b",
                Title = "Arrosage motopompe & Sarclage manuel",
                Category = "Entretien",
                Date = DateTime.Now.Date.AddDays(-27),
                Product = "Carburant motopompe + 2 ouvriers",
                DoseApplied = "Nettoyage complet des billons",
                CostFCFA = 10000,
                IsDoseCompliant = true,
                DarBadgeText = "Entretien",
                CategoryColorHex = "#71796F",
                AudioNoteText = "Deux journaliers mobilisés sur 0,8 hectare."
            }
        };

        // 3. Certified Chemical Products Catalog (MINADER Cameroun)
        _products = new List<ChemicalProduct>
        {
            new ChemicalProduct
            {
                Id = "prod-mancozeb",
                Name = "Mancozèbe / Manèbe 80 WP",
                Category = "Fongicide",
                ActiveSubstance = "Mancozèbe 80% (Poudre mouillable)",
                BaseDosePer15L = 150.0,
                Unit = "g",
                DarDays = 14,
                ToxicityClass = "Classe III (Modéré)",
                ColorHex = "#E67E22",
                BeeWarning = false,
                PpeRequired = "Masque FFP2 + Gants + Bottes",
                TargetPests = "Mildiou, Alternariose, Rouille",
                TargetCrops = "Tomate, Pomme de terre, Caféier",
                Instructions = "Traiter préventivement dès l'apparition des premières nécroses.",
                IsCertifiedMinader = true
            },
            new ChemicalProduct
            {
                Id = "prod-lambda",
                Name = "Lambda-cyhalothrine 50 EC",
                Category = "Insecticide",
                ActiveSubstance = "Lambda-cyhalothrine 50 g/L (Concentré émulsionnable)",
                BaseDosePer15L = 15.0,
                Unit = "ml",
                DarDays = 7,
                ToxicityClass = "Classe II (Élevée)",
                ColorHex = "#BA1A1A",
                BeeWarning = true,
                PpeRequired = "Combinaison complète + Masque vapeur + Gants nitrile",
                TargetPests = "Chenilles légionnaires, Noctuelles, Foreurs",
                TargetCrops = "Maïs, Tomate, Maraîchage",
                Instructions = "Avertissement abeilles : Pulvériser uniquement au crépuscule.",
                IsCertifiedMinader = true
            },
            new ChemicalProduct
            {
                Id = "prod-cuivre",
                Name = "Bouillie Bordelaise / Cuivre WP",
                Category = "Fongicide",
                ActiveSubstance = "Sulfate de cuivre neutralisé à la chaux (20% Cu)",
                BaseDosePer15L = 120.0,
                Unit = "g",
                DarDays = 3,
                ToxicityClass = "Classe IV (Faible)",
                ColorHex = "#1A6D0E",
                BeeWarning = false,
                PpeRequired = "Gants + Lunettes de protection",
                TargetPests = "Bactériose, Mildiou, Anthracnose",
                TargetCrops = "Tomate, Piment, Agrumes",
                Instructions = "Action fongistatique et bactériostatique de contact.",
                IsCertifiedMinader = true
            },
            new ChemicalProduct
            {
                Id = "prod-bio",
                Name = "Fumier Composté & Purin de Neem",
                Category = "Engrais bio",
                ActiveSubstance = "Matière organique fermentée + Azadirachtine naturelle",
                BaseDosePer15L = 250.0,
                Unit = "ml",
                DarDays = 0,
                ToxicityClass = "Biologique (Nul)",
                ColorHex = "#27AE60",
                BeeWarning = false,
                PpeRequired = "Équipement usuel de jardinage",
                TargetPests = "Pucerons, Acariens, Carence nutritive",
                TargetCrops = "Toutes spéculations vivrières",
                Instructions = "Zéro résidu chimique nocif. Récolte possible le jour même.",
                IsCertifiedMinader = true
            }
        };
    }

    private async Task SaveToDiskAsync()
    {
        try
        {
            var envelope = new DatabaseEnvelope
            {
                Parcels = _parcels,
                Works = _works,
                Products = _products
            };
            var json = JsonSerializer.Serialize(envelope, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_dbFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving Agriwise DB: {ex.Message}");
        }
    }

    public async Task<List<Parcel>> GetParcelsAsync()
    {
        await InitializeAsync();
        return _parcels;
    }

    public async Task<Parcel?> GetParcelByIdAsync(string id)
    {
        await InitializeAsync();
        return _parcels.FirstOrDefault(p => p.Id == id);
    }

    public async Task SaveParcelAsync(Parcel parcel)
    {
        await InitializeAsync();
        var existing = _parcels.FirstOrDefault(p => p.Id == parcel.Id);
        if (existing != null)
        {
            _parcels.Remove(existing);
        }
        _parcels.Add(parcel);
        await SaveToDiskAsync();
    }

    public async Task<List<CultureWork>> GetCultureWorksAsync(string parcelId)
    {
        await InitializeAsync();
        return _works
            .Where(w => string.IsNullOrEmpty(parcelId) || w.ParcelId == parcelId)
            .OrderByDescending(w => w.Date)
            .ToList();
    }

    public async Task AddCultureWorkAsync(CultureWork work)
    {
        await InitializeAsync();
        _works.Insert(0, work);
        await SaveToDiskAsync();
    }

    public async Task<List<ChemicalProduct>> GetChemicalProductsAsync()
    {
        await InitializeAsync();
        return _products;
    }

    public async Task<ChemicalProduct?> GetChemicalProductByIdAsync(string id)
    {
        await InitializeAsync();
        return _products.FirstOrDefault(p => p.Id == id);
    }

    public async Task RecordSprayerTreatmentAsync(string parcelId, string productId, SprayerCalculationResult calc)
    {
        await InitializeAsync();
        var parcel = _parcels.FirstOrDefault(p => p.Id == parcelId);
        if (parcel != null)
        {
            parcel.LastTreatmentDate = DateTime.Now.Date;
            parcel.LastChemicalApplied = calc.ProductName;
            parcel.DarDaysTotal = calc.DarDays;
            parcel.HasActiveDar = calc.DarDays > 0;
            parcel.DarHarvestAuthorizedDate = calc.EarliestHarvestDate;
            parcel.Status = calc.DarDays > 0 ? $"En délai DAR ({calc.DarDays}j)" : "Récolte autorisée";
            parcel.ColorAccentHex = calc.DarDays > 0 ? "#E67E22" : "#27AE60";
        }

        // Add journal record
        var newWork = new CultureWork
        {
            ParcelId = parcelId,
            Title = $"Traitement : {calc.ProductName}",
            Category = "Phytosanitaire",
            Date = DateTime.Now,
            Product = calc.ProductName,
            DoseApplied = $"{calc.NetDosePerSprayer}{calc.Unit}/pulvé ({calc.SprayersCountTotal} cuves de {calc.TotalSprayVolumeLitres}L)",
            CostFCFA = 14000,
            IsDoseCompliant = true,
            DarBadgeText = calc.DarDays > 0 ? $"DAR {calc.DarDays}j" : "Sans DAR",
            CategoryColorHex = calc.DarDays > 0 ? "#9C4400" : "#27AE60",
            AudioNoteText = $"Dosage précis consigné : {calc.NetDosePerSprayer}{calc.Unit} par pulvérisateur 15L."
        };
        _works.Insert(0, newWork);

        await SaveToDiskAsync();
    }

    public async Task AddTransactionAsync(string parcelId, int amountFCFA, string description, bool isExpense)
    {
        await InitializeAsync();
        var work = new CultureWork
        {
            ParcelId = parcelId,
            Title = description,
            Category = isExpense ? "Dépense" : "Vente récolte",
            Date = DateTime.Now,
            Product = description,
            DoseApplied = isExpense ? "Achat intrant / prestation" : "Recette de récolte",
            CostFCFA = amountFCFA,
            IsDoseCompliant = true,
            DarBadgeText = isExpense ? "Dépense" : "Vente",
            CategoryColorHex = isExpense ? "#BA1A1A" : "#27AE60",
            AudioNoteText = $"Transaction enregistrée le {DateTime.Now:dd/MM/yyyy}."
        };
        _works.Insert(0, work);
        await SaveToDiskAsync();
    }

    public async Task<FarmFinancialSummary> GetFinancialSummaryAsync()
    {
        await InitializeAsync();

        var ranked = new List<ParcelFinancialStat>
        {
            new ParcelFinancialStat
            {
                ParcelId = "parcel-b",
                ParcelName = "Parcelle B · Tomate Cobra F1",
                Crop = "Tomate",
                AreaHa = 0.8,
                RevenueEstimatedFCFA = 512000,
                ExpensesTotalFCFA = 226500,
                ChemicalInputPercent = 30,
                DirectSavingsDAR = 48000,
                Rank = 1
            },
            new ParcelFinancialStat
            {
                ParcelId = "parcel-a",
                ParcelName = "Parcelle A · Cacao Hybride",
                Crop = "Cacao",
                AreaHa = 1.5,
                RevenueEstimatedFCFA = 1425000,
                ExpensesTotalFCFA = 550000,
                ChemicalInputPercent = 12,
                DirectSavingsDAR = 42000,
                Rank = 2
            },
            new ParcelFinancialStat
            {
                ParcelId = "parcel-c",
                ParcelName = "Parcelle C · Maïs Jaune CMS 8704",
                Crop = "Maïs",
                AreaHa = 2.0,
                RevenueEstimatedFCFA = 566500,
                ExpensesTotalFCFA = 292000,
                ChemicalInputPercent = 24,
                DirectSavingsDAR = 25000,
                Rank = 3
            }
        };

        int totalRev = ranked.Sum(r => r.RevenueEstimatedFCFA);
        int totalExp = ranked.Sum(r => r.ExpensesTotalFCFA);
        int totalNet = totalRev - totalExp;
        int overallRoi = totalExp > 0 ? (int)((totalNet / (double)totalExp) * 100) : 0;
        int totalSavings = ranked.Sum(r => r.DirectSavingsDAR);

        return new FarmFinancialSummary
        {
            TotalRevenuesFCFA = totalRev,
            TotalExpensesFCFA = totalExp,
            TotalProjectedNetMarginFCFA = totalNet,
            OverallRoiPercent = overallRoi,
            TotalSavingsAntiOverdoseFCFA = totalSavings,
            RankedParcels = ranked
        };
    }
}
