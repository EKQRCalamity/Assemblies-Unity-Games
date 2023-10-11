using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class ProfileStartupLocaleSelector : IStartupLocaleSelector
{
	public Locale GetStartupLocale(ILocalesProvider availableLocales)
	{
		if (!LaunchManager.InGame)
		{
			return null;
		}
		return ProfileManager.prefs.localeOverride;
	}
}
