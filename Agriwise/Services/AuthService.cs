namespace Agriwise.Services;

public interface IAuthService
{
    bool IsLoggedIn { get; }
    string CurrentPhoneNumber { get; }
    string CurrentRegion { get; }
    List<string> SelectedCrops { get; }
    Task<bool> LoginWithPinAsync(string phone, string region, string pin, List<string> crops);
    Task LogoutAsync();
}

public class AuthService : IAuthService
{
    private static AuthService? _current;
    public static AuthService Current => _current ??= new AuthService();

    public bool IsLoggedIn => Preferences.Default.Get("auth_logged_in", false);
    public string CurrentPhoneNumber => Preferences.Default.Get("auth_phone", "670 12 34 56");
    public string CurrentRegion => Preferences.Default.Get("auth_region", "Région de l'Ouest — Bafoussam / Foumbot");
    public List<string> SelectedCrops
    {
        get
        {
            var raw = Preferences.Default.Get("auth_crops", "Tomate,Maïs");
            return raw.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }

    public Task<bool> LoginWithPinAsync(string phone, string region, string pin, List<string> crops)
    {
        Preferences.Default.Set("auth_logged_in", true);
        Preferences.Default.Set("auth_phone", phone);
        Preferences.Default.Set("auth_region", region);
        Preferences.Default.Set("auth_pin", pin);
        Preferences.Default.Set("auth_crops", string.Join(",", crops));
        return Task.FromResult(true);
    }

    public Task LogoutAsync()
    {
        Preferences.Default.Remove("auth_logged_in");
        return Task.CompletedTask;
    }
}
