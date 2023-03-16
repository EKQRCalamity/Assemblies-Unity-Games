using System.Globalization;

public static class DetectLanguage
{
	public static Localization.Languages GetDefaultLanguage()
	{
		Localization.Languages defaultLanguage = Localization.Languages.English;
		getDefaultLanguage(ref defaultLanguage);
		return defaultLanguage;
	}

	private static void getDefaultLanguage(ref Localization.Languages defaultLanguage)
	{
		CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
		switch (currentUICulture.TwoLetterISOLanguageName)
		{
		case "fr":
			defaultLanguage = Localization.Languages.French;
			break;
		case "de":
			defaultLanguage = Localization.Languages.German;
			break;
		case "it":
			defaultLanguage = Localization.Languages.Italian;
			break;
		case "ja":
			defaultLanguage = Localization.Languages.Japanese;
			break;
		case "zh":
			defaultLanguage = Localization.Languages.SimplifiedChinese;
			break;
		case "ru":
			defaultLanguage = Localization.Languages.Russian;
			break;
		case "es":
			if (currentUICulture.Name == "es-ES" || currentUICulture.Name == "es")
			{
				defaultLanguage = Localization.Languages.SpanishSpain;
			}
			else
			{
				defaultLanguage = Localization.Languages.SpanishAmerica;
			}
			break;
		case "ko":
			defaultLanguage = Localization.Languages.Korean;
			break;
		case "po":
			defaultLanguage = Localization.Languages.Polish;
			break;
		default:
			if (currentUICulture.Name == "pt-BR")
			{
				defaultLanguage = Localization.Languages.PortugueseBrazil;
			}
			else
			{
				defaultLanguage = Localization.Languages.English;
			}
			break;
		}
	}
}
