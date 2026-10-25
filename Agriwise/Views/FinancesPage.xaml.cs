using Agriwise.Models;
using Agriwise.Services;

namespace Agriwise.Views;

public partial class FinancesPage : ContentPage
{
    private FarmFinancialSummary? _summary;

    public FinancesPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshFinancesAsync();
    }

    private async Task RefreshFinancesAsync()
    {
        await DataService.Current.InitializeAsync();
        _summary = await DataService.Current.GetFinancialSummaryAsync();

        if (_summary.RankedParcels.Count > 0)
        {
            var pB = _summary.RankedParcels.FirstOrDefault(p => p.ParcelId == "parcel-b") ?? _summary.RankedParcels[0];
            NetMarginValueLabel.Text = $"+{pB.NetMarginFCFA:N0}".Replace(",", " ");
            RoiBadgeLabel.Text = $"ROI +{pB.RoiPercent}%";
            RevenuesValueLabel.Text = $"{pB.RevenueEstimatedFCFA:N0} FCFA".Replace(",", " ");
            ExpensesValueLabel.Text = $"{pB.ExpensesTotalFCFA:N0} FCFA".Replace(",", " ");
            DirectSavingsLabel.Text = $"+{pB.DirectSavingsDAR:N0} FCFA".Replace(",", " ");
        }

        // Global Comparatif KPIs
        TotalNetMarginValueLabel.Text = $"+{_summary.TotalProjectedNetMarginFCFA:N0}".Replace(",", " ");
        TotalRoiBadgeLabel.Text = $"+{_summary.OverallRoiPercent}% ROI";
        TotalRevenuesValueLabel.Text = $"{_summary.TotalRevenuesFCFA:N0} F".Replace(",", " ");
        TotalExpensesValueLabel.Text = $"{_summary.TotalExpensesFCFA:N0} F".Replace(",", " ");
        TotalSavingsValueLabel.Text = $"+{_summary.TotalSavingsAntiOverdoseFCFA:N0} FCFA économisés avec AgriWise".Replace(",", " ");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//DashboardPage");
    }

    private void OnShowBilanTab(object sender, EventArgs e)
    {
        BilanParcelleLayout.IsVisible = true;
        ComparatifLayout.IsVisible = false;

        TabBilanBorder.BackgroundColor = Color.FromArgb("#013A13");
        TabBilanLabel.TextColor = Colors.White;
        TabBilanLabel.FontFamily = "OpenSansSemibold";

        TabComparatifBorder.BackgroundColor = Colors.Transparent;
        TabComparatifLabel.TextColor = Color.FromArgb("#71796F");
        TabComparatifLabel.FontFamily = null;
    }

    private void OnShowComparatifTab(object sender, EventArgs e)
    {
        BilanParcelleLayout.IsVisible = false;
        ComparatifLayout.IsVisible = true;

        TabComparatifBorder.BackgroundColor = Color.FromArgb("#013A13");
        TabComparatifLabel.TextColor = Colors.White;
        TabComparatifLabel.FontFamily = "OpenSansSemibold";

        TabBilanBorder.BackgroundColor = Colors.Transparent;
        TabBilanLabel.TextColor = Color.FromArgb("#71796F");
        TabBilanLabel.FontFamily = null;
    }

    private async void OnShareClicked(object sender, EventArgs e)
    {
        if (_summary == null) return;

        try
        {
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Text = $"AgriWise Bilan Financier (Foumbot 2026) : Bénéfice Net Projeté +{_summary.TotalProjectedNetMarginFCFA:N0} FCFA (+{_summary.OverallRoiPercent}% ROI). " +
                       $"Chiffre d'Affaires : {_summary.TotalRevenuesFCFA:N0} FCFA, Charges réelles : {_summary.TotalExpensesFCFA:N0} FCFA. " +
                       $"Économies surdosage intrants : +{_summary.TotalSavingsAntiOverdoseFCFA:N0} FCFA grâce aux protocoles DAR.",
                Title = "Bilan Financier AgriWise"
            });
        }
        catch
        {
            await DisplayAlert("Rapport Financier", "Bilan prêt à être partagé en PDF ou sur WhatsApp.", "OK");
        }
    }

    private async void OnAddTransactionClicked(object sender, EventArgs e)
    {
        var action = await DisplayActionSheet(
            "Enregistrer une transaction",
            "Annuler",
            null,
            "Dépense (Intrant / Main d'œuvre / Traitement)",
            "Vente Récolte (Chiffre d'affaires)"
        );

        if (string.IsNullOrWhiteSpace(action) || action == "Annuler") return;

        bool isExpense = action.StartsWith("Dépense");

        string amountStr = await DisplayPromptAsync("Montant en FCFA", "Entrez le montant :", "Suivant", "Annuler", "Ex: 25000", keyboard: Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(amountStr)) return;

        if (!int.TryParse(amountStr, out int amount) || amount <= 0)
        {
            await DisplayAlert("Erreur", "Veuillez entrer un montant valide.", "OK");
            return;
        }

        string desc = await DisplayPromptAsync("Motif", "Description de l'opération :", "Enregistrer", "Annuler", isExpense ? "Ex: Achat fongicide ou sarclage" : "Ex: Vente de 20 cageots au marché");
        if (string.IsNullOrWhiteSpace(desc)) desc = isExpense ? "Dépense exploitation" : "Vente récolte";

        await DataService.Current.AddTransactionAsync("parcel-b", amount, desc, isExpense);
        await RefreshFinancesAsync();

        await DisplayAlert("Enregistré !", $"La transaction de {amount:N0} FCFA a été ajoutée avec succès au carnet de caisse.".Replace(",", " "), "OK");
    }
}

