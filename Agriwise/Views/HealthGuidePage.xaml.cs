namespace Agriwise.Views;

public partial class HealthGuidePage : ContentPage
{
    public HealthGuidePage()
    {
        InitializeComponent();
    }

    private async void OnOpenCalculatorClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("CalculatorPage");
    }

    private async void OnEmergencyCallClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Urgence Intoxication", "Appel du SAMU Cameroun (119) ou Centre Anti-poison Yaoundé : +237 222 23 44 55", "Appeler");
    }
}
