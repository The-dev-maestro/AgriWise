namespace Agriwise.Views;

public partial class CalculatorPage : ContentPage
{
    public CalculatorPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnValidateClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Traitement Consigné !", "L'application de 150g de Mancozèbe sur Parcelle B a été enregistrée dans votre carnet.\n\n⚠️ Délai Avant Récolte (DAR) démarré : 14 jours.", "Voir le Journal");
        await Shell.Current.GoToAsync("//JournalPage");
    }
}
