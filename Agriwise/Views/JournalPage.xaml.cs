namespace Agriwise.Views;

public partial class JournalPage : ContentPage
{
    public JournalPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//DashboardPage");
    }

    private async void OnAddWorkClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("CalculatorPage");
    }

    private async void OnOpenFinancesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//FinancesPage");
    }
}
