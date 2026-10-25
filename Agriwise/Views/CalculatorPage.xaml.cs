using Agriwise.Models;
using Agriwise.Services;

namespace Agriwise.Views;

public partial class CalculatorPage : ContentPage
{
    private readonly IDarCalculationService _calcService = new DarCalculationService();
    private List<Parcel> _parcels = new();
    private List<ChemicalProduct> _products = new();
    
    private Parcel? _selectedParcel;
    private ChemicalProduct? _selectedProduct;
    private int _selectedVolume = 15;
    private string _selectedPressure = "Curatif";
    private SprayerCalculationResult? _lastCalculation;

    public CalculatorPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await InitializeDataAsync();
    }

    private async Task InitializeDataAsync()
    {
        await DataService.Current.InitializeAsync();
        _parcels = await DataService.Current.GetParcelsAsync();
        _products = await DataService.Current.GetChemicalProductsAsync();

        if (_parcels.Count == 0 || _products.Count == 0) return;

        // Populate Picker
        ParcelPicker.SelectedIndexChanged -= OnParcelPickerChanged;
        ParcelPicker.ItemsSource = _parcels.Select(p => $"{p.Name} ({p.AreaHa:0.0} ha)").ToList();
        ParcelPicker.SelectedIndex = 0;
        ParcelPicker.SelectedIndexChanged += OnParcelPickerChanged;

        _selectedParcel = _parcels[0];
        _selectedProduct = _products.FirstOrDefault(p => p.Id == "mancozebe-80wp") ?? _products[0];

        UpdateParcelDisplay();
        Recalculate();
    }

    private void OnParcelPickerChanged(object? sender, EventArgs e)
    {
        if (ParcelPicker.SelectedIndex >= 0 && ParcelPicker.SelectedIndex < _parcels.Count)
        {
            _selectedParcel = _parcels[ParcelPicker.SelectedIndex];
            UpdateParcelDisplay();
            Recalculate();
        }
    }

    private void UpdateParcelDisplay()
    {
        if (_selectedParcel == null) return;
        ParcelAreaLabel.Text = $"Superficie : {_selectedParcel.AreaM2:N0} m²".Replace(",", " ");
        ParcelStageLabel.Text = $"Stade : {(_selectedParcel.Crop.Contains("Tomate", StringComparison.OrdinalIgnoreCase) ? "Floraison" : "Plein champ")}";
    }

    #region Product Selection Handlers

    private void OnSelectProdMancozebe(object sender, TappedEventArgs e)
    {
        SelectProductById("mancozebe-80wp");
    }

    private void OnSelectProdLambda(object sender, TappedEventArgs e)
    {
        SelectProductById("lambda-50ec");
    }

    private void OnSelectProdBouillie(object sender, TappedEventArgs e)
    {
        SelectProductById("bouillie-bordelaise");
    }

    private void OnSelectProdNeem(object sender, TappedEventArgs e)
    {
        SelectProductById("bio-neem");
    }

    private void SelectProductById(string id)
    {
        _selectedProduct = _products.FirstOrDefault(p => p.Id == id) ?? _selectedProduct;
        UpdateProductCardsHighlight(id);
        Recalculate();
    }

    private void UpdateProductCardsHighlight(string selectedId)
    {
        // Option 1
        var is1 = selectedId == "mancozebe-80wp";
        BorderProdMancozebe.BackgroundColor = is1 ? Color.FromArgb("#EAF7EE") : Colors.White;
        BorderProdMancozebe.Stroke = is1 ? Color.FromArgb("#013A13") : Color.FromArgb("#E4E2E0");
        BorderProdMancozebe.StrokeThickness = is1 ? 2 : 1;
        BadgeCheckMancozebe.BackgroundColor = is1 ? Color.FromArgb("#013A13") : Colors.Transparent;
        BadgeCheckMancozebe.Content = is1 ? new Image { Source = "ic_check.png", WidthRequest = 12, HeightRequest = 12, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center } : null;

        // Option 2
        var is2 = selectedId == "lambda-50ec";
        BorderProdLambda.BackgroundColor = is2 ? Color.FromArgb("#EAF7EE") : Colors.White;
        BorderProdLambda.Stroke = is2 ? Color.FromArgb("#013A13") : Color.FromArgb("#E4E2E0");
        BorderProdLambda.StrokeThickness = is2 ? 2 : 1;
        BadgeCheckLambda.BackgroundColor = is2 ? Color.FromArgb("#013A13") : Colors.Transparent;
        BadgeCheckLambda.Content = is2 ? new Image { Source = "ic_check.png", WidthRequest = 12, HeightRequest = 12, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center } : null;

        // Option 3
        var is3 = selectedId == "bouillie-bordelaise";
        BorderProdBouillie.BackgroundColor = is3 ? Color.FromArgb("#EAF7EE") : Colors.White;
        BorderProdBouillie.Stroke = is3 ? Color.FromArgb("#013A13") : Color.FromArgb("#E4E2E0");
        BorderProdBouillie.StrokeThickness = is3 ? 2 : 1;
        BadgeCheckBouillie.BackgroundColor = is3 ? Color.FromArgb("#013A13") : Colors.Transparent;
        BadgeCheckBouillie.Content = is3 ? new Image { Source = "ic_check.png", WidthRequest = 12, HeightRequest = 12, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center } : null;

        // Option 4
        var is4 = selectedId == "bio-neem";
        BorderProdNeem.BackgroundColor = is4 ? Color.FromArgb("#EAF7EE") : Colors.White;
        BorderProdNeem.Stroke = is4 ? Color.FromArgb("#013A13") : Color.FromArgb("#E4E2E0");
        BorderProdNeem.StrokeThickness = is4 ? 2 : 1;
        BadgeCheckNeem.BackgroundColor = is4 ? Color.FromArgb("#013A13") : Colors.Transparent;
        BadgeCheckNeem.Content = is4 ? new Image { Source = "ic_check.png", WidthRequest = 12, HeightRequest = 12, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center } : null;
    }

    #endregion

    #region Volume Selection Handlers

    private void OnSelectVolume10(object sender, TappedEventArgs e) => SetVolume(10);
    private void OnSelectVolume15(object sender, TappedEventArgs e) => SetVolume(15);
    private void OnSelectVolume16(object sender, TappedEventArgs e) => SetVolume(16);
    private void OnSelectVolume20(object sender, TappedEventArgs e) => SetVolume(20);

    private void SetVolume(int vol)
    {
        _selectedVolume = vol;
        VolumeSelectedHeaderLabel.Text = $"{vol}L sélectionné";

        HighlightVolume(BorderVol10, LabelVol10, vol == 10);
        HighlightVolume(BorderVol15, LabelVol15, vol == 15);
        HighlightVolume(BorderVol16, LabelVol16, vol == 16);
        HighlightVolume(BorderVol20, LabelVol20, vol == 20);

        Recalculate();
    }

    private void HighlightVolume(Border border, Label label, bool isSelected)
    {
        border.BackgroundColor = isSelected ? Color.FromArgb("#1E5128") : Color.FromArgb("#F5F3F1");
        border.Stroke = isSelected ? Color.FromArgb("#013A13") : Color.FromArgb("#E4E2E0");
        border.StrokeThickness = isSelected ? 2 : 1;
        label.TextColor = isSelected ? Colors.White : Color.FromArgb("#1B1C1B");
    }

    #endregion

    #region Pressure Selection Handlers

    private void OnSelectPressurePreventif(object sender, TappedEventArgs e) => SetPressure("Préventif");
    private void OnSelectPressureCuratif(object sender, TappedEventArgs e) => SetPressure("Curatif");
    private void OnSelectPressureSevere(object sender, TappedEventArgs e) => SetPressure("Sévère");

    private void SetPressure(string pressure)
    {
        _selectedPressure = pressure;

        HighlightPressure(BorderPressPreventif, pressure == "Préventif");
        HighlightPressure(BorderPressCuratif, pressure == "Curatif");
        HighlightPressure(BorderPressSevere, pressure == "Sévère");

        Recalculate();
    }

    private void HighlightPressure(Border border, bool isSelected)
    {
        border.BackgroundColor = isSelected ? Color.FromArgb("#EAF7EE") : Color.FromArgb("#F5F3F1");
        border.Stroke = isSelected ? Color.FromArgb("#013A13") : Color.FromArgb("#E4E2E0");
        border.StrokeThickness = isSelected ? 2 : 1;
    }

    #endregion

    private void Recalculate()
    {
        if (_selectedParcel == null || _selectedProduct == null) return;

        var request = new SprayerCalculationRequest
        {
            ParcelId = _selectedParcel.Id,
            ProductId = _selectedProduct.Id,
            SprayerCapacityLitres = _selectedVolume,
            InfestationLevel = _selectedPressure
        };

        _lastCalculation = _calcService.CalculateDose(_selectedParcel, _selectedProduct, _selectedVolume, _selectedPressure);

        // Update Results UI
        ResultHeaderLabel.Text = $"Dose calculée pour 1 pulvérisateur ({_selectedVolume}L)";
        CalculatedDoseValueLabel.Text = $"{_lastCalculation.NetDosePerSprayer:0.#}";
        CalculatedDoseUnitLabel.Text = _lastCalculation.Unit == "g" ? "grammes" : "millilitres";
        CalculatedMeasuringGuideLabel.Text = _lastCalculation.MeasuringGuide;

        CalculatedParcelVolumeLabel.Text = $"Volume parcelle ({_selectedParcel.AreaHa:0.0} ha)";
        CalculatedSprayersCountLabel.Text = $"{_lastCalculation.SprayersCountTotal} Pulvérisateurs";
        CalculatedTotalBouillieLabel.Text = $"Total : {_lastCalculation.TotalSprayVolumeLitres:0.#}L de bouillie";

        CalculatedSavingsLabel.Text = $"+{_lastCalculation.EstimatedSavingsFCFA:N0} FCFA".Replace(",", " ");
        CalculatedDarBadgeLabel.Text = $"{_lastCalculation.DarDays} JOURS MINIMUM";
        CalculatedEarliestHarvestDateLabel.Text = $"{_lastCalculation.EarliestHarvestDate:dd MMMM yyyy}";

        CalculatedDarNoticeLabel.Text = $"Attention sanitaire : Interdiction formelle de récolter, consommer ou commercialiser {_selectedParcel.Crop} avant le {_lastCalculation.EarliestHarvestDate:dd/MM/yyyy} pour garantir l'absence totale de résidus chimiques.";

        if (_lastCalculation.PreparationSteps.Count >= 4)
        {
            CalculatedStep1DescLabel.Text = _lastCalculation.PreparationSteps[0];
            CalculatedStep2DescLabel.Text = _lastCalculation.PreparationSteps[1];
            CalculatedStep3DescLabel.Text = _lastCalculation.PreparationSteps[2];
            CalculatedStep4DescLabel.Text = _lastCalculation.PreparationSteps[3];
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnValidateClicked(object sender, EventArgs e)
    {
        if (_selectedParcel == null || _selectedProduct == null || _lastCalculation == null)
        {
            await DisplayAlert("Erreur", "Veuillez sélectionner une parcelle et un produit.", "OK");
            return;
        }

        await DataService.Current.RecordSprayerTreatmentAsync(_selectedParcel.Id, _selectedProduct.Id, _lastCalculation);

        var msg = $"L'application de {_lastCalculation.NetDosePerSprayer:0.#}{_lastCalculation.Unit} de {_selectedProduct.Name} sur {_selectedParcel.Name} a été enregistrée avec succès.\n\n" +
                  $"[DÉLAI PHYTOSANITAIRE]\nDélai Avant Récolte (DAR) démarré : {_lastCalculation.DarDays} jours.\n" +
                  $"Récolte autorisée au plus tôt : {_lastCalculation.EarliestHarvestDate:dd MMMM yyyy}.";

        await DisplayAlert("Traitement Consigné", msg, "Voir le Journal");
        await Shell.Current.GoToAsync("//JournalPage");
    }

    private async void OnShareClicked(object sender, EventArgs e)
    {
        if (_lastCalculation == null || _selectedProduct == null || _selectedParcel == null) return;

        try
        {
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Text = $"AgriWise Fiche Dosage : {_lastCalculation.NetDosePerSprayer:0.#}{_lastCalculation.Unit} {_selectedProduct.Name} / pulvérisateur {_selectedVolume}L pour {_selectedParcel.Crop} ({_selectedParcel.Name}, Foumbot). " +
                       $"Volume total : {_lastCalculation.SprayersCountTotal} pulvérisateurs ({_lastCalculation.TotalSprayVolumeLitres}L). " +
                       $"DAR obligatoire : {_lastCalculation.DarDays} jours (Récolte autorisée dès le {_lastCalculation.EarliestHarvestDate:dd/MM/yyyy}). Port des EPI obligatoire !",
                Title = "Partage Fiche de Dosage AgriWise"
            });
        }
        catch
        {
            await DisplayAlert("Fiche Dosage", "Fiche prête à être envoyée via WhatsApp / SMS.", "OK");
        }
    }
}

