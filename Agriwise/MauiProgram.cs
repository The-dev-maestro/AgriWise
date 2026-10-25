using Microsoft.Extensions.Logging;

namespace Agriwise;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Enregistrement des services Backend & Logique Métier AgriWise
		builder.Services.AddSingleton<Agriwise.Services.IDataService>(Agriwise.Services.DataService.Current);
		builder.Services.AddSingleton<Agriwise.Services.IDarCalculationService, Agriwise.Services.DarCalculationService>();
		builder.Services.AddSingleton<Agriwise.Services.IAuthService>(Agriwise.Services.AuthService.Current);

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
