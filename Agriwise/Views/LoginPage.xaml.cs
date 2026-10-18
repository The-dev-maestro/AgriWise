namespace Agriwise.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnAccessParcelsClicked(object sender, EventArgs e)
    {
        // Transition seamlessly to the main 4-tab shell
        Application.Current.MainPage = new AppShell();
    }

    private async void OnSmsCodeClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Code SMS", "Un code d'activation sécurisé a été transmis par SMS au numéro renseigné.", "Continuer");
        Application.Current.MainPage = new AppShell();
    }
}
