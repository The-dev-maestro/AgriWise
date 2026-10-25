namespace Agriwise.Views;

public partial class OnboardingPage : ContentPage
{
    public OnboardingPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage());
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage());
    }

    private void OnDirectDashboardClicked(object sender, EventArgs e)
    {
        if (Application.Current != null)
        {
            Application.Current.MainPage = new AppShell();
        }
    }
}
