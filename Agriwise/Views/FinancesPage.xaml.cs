namespace Agriwise.Views;

public partial class FinancesPage : ContentPage
{
    public FinancesPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//DashboardPage");
    }

    private async void OnSwitchParcelClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Parcelle A — Cacao", "Chiffre d'Affaires estimé : 1 200 000 FCFA\nDépenses : 340 000 FCFA\nBénéfice projeté : +860 000 FCFA", "OK");
    }

    private async void OnShareClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Export Bilan", "Le bilan financier de votre campagne a été exporté en PDF dans vos documents.", "OK");
    }

    private async void OnAddTransactionClicked(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync("Nouvelle Dépense / Vente", "Entrez le montant en FCFA :", "Enregistrer", "Annuler", "Ex: 15000", keyboard: Keyboard.Numeric);
        if (!string.IsNullOrWhiteSpace(result))
        {
            await DisplayAlert("Enregistré !", $"La transaction de {result} FCFA a été ajoutée au carnet de caisse.", "OK");
        }
    }
}
