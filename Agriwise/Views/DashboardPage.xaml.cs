namespace Agriwise.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
    }

    private async void OnOpenParcelBClicked(object sender, EventArgs e)
    {
        // Navigate to the Journal tab
        await Shell.Current.GoToAsync("//JournalPage");
    }

    private async void OnOpenJournalClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//JournalPage");
    }

    private async void OnOpenCalculatorClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("CalculatorPage");
    }

    private async void OnCallAdvisorClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Conseiller Foumbot", "Contact de permanence : +237 670 11 22 33\nHoraires : 7h - 18h", "Appeler");
    }

    private async void OnNotificationsClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Notifications", "1 Alerte prioritaire : Le Délai Avant Récolte (DAR) sur votre parcelle de tomates court jusqu'au 28 Octobre.", "Compris");
    }
}
