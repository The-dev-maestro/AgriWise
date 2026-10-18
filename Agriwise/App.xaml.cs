namespace Agriwise;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		// Démarre sur l'Onboarding / Splash Screen
		// Le flux utilisateur est : Onboarding -> Authentification (Login) -> Dashboard (AppShell)
		MainPage = new NavigationPage(new Views.OnboardingPage());
	}
}
