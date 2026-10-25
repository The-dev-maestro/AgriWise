using Agriwise.Services;

namespace Agriwise.Views;

public partial class LoginPage : ContentPage
{
    private readonly List<string> _selectedCrops = new() { "Tomate", "Maïs" };

    public LoginPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        // Pre-fill remembered phone if any
        if (!string.IsNullOrEmpty(AuthService.Current.CurrentPhoneNumber))
        {
            PhoneEntry.Text = AuthService.Current.CurrentPhoneNumber;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnAccessParcelsClicked(object sender, EventArgs e)
    {
        string phone = string.IsNullOrWhiteSpace(PhoneEntry.Text) ? "670 12 34 56" : PhoneEntry.Text.Trim();
        string region = RegionPicker.SelectedItem?.ToString() ?? "Région de l'Ouest — Bafoussam / Foumbot";
        string pin = string.IsNullOrWhiteSpace(PinEntry.Text) ? "1234" : PinEntry.Text.Trim();

        await AuthService.Current.LoginWithPinAsync(phone, region, pin, _selectedCrops);

        // Transition seamlessly to the main 4-tab shell
        if (Application.Current != null)
        {
            Application.Current.MainPage = new AppShell();
        }
    }

    private async void OnSmsCodeClicked(object sender, EventArgs e)
    {
        string phone = string.IsNullOrWhiteSpace(PhoneEntry.Text) ? "670 12 34 56" : PhoneEntry.Text.Trim();
        await DisplayAlert("Activation SMS", $"Un code d'activation sécurisé a été transmis par SMS au (+237) {phone}.", "Continuer");

        string region = RegionPicker.SelectedItem?.ToString() ?? "Région de l'Ouest — Bafoussam / Foumbot";
        await AuthService.Current.LoginWithPinAsync(phone, region, "1234", _selectedCrops);

        if (Application.Current != null)
        {
            Application.Current.MainPage = new AppShell();
        }
    }
}
