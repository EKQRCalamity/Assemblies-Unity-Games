using System.Collections.Generic;
using I2.Loc;

namespace Gameplay.UI.Console;

public class LanguageCommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		switch (subcommand)
		{
		case "help":
			base.Console.Write("Available LANGUAGE commands:");
			base.Console.Write("language list: List all available languages");
			base.Console.Write("language current: Show the current language");
			base.Console.Write("language set LANGUAGE_CODE: Sets the language");
			break;
		case "list":
			base.Console.Write("The available languages are:");
			{
				foreach (string item in LocalizationManager.GetAllLanguagesCode())
				{
					string languageFromCode2 = LocalizationManager.GetLanguageFromCode(item);
					base.Console.Write(item + ": " + languageFromCode2);
				}
				break;
			}
		case "current":
			base.Console.Write("The current language is " + LocalizationManager.CurrentLanguage + " with code " + LocalizationManager.CurrentLanguageCode);
			break;
		case "set":
			if (ValidateParams(subcommand, 1, listParameters))
			{
				string languageFromCode = LocalizationManager.GetLanguageFromCode(listParameters[0]);
				string supportedLanguage = LocalizationManager.GetSupportedLanguage(languageFromCode);
				if (string.IsNullOrEmpty(supportedLanguage))
				{
					base.Console.Write("language set: Language code " + listParameters[0] + " not supported, use language list");
					break;
				}
				LocalizationManager.CurrentLanguage = supportedLanguage;
				base.Console.Write("Language setted");
			}
			break;
		default:
			base.Console.Write("Command unknow, use language help");
			break;
		}
	}

	public override string GetName()
	{
		return "language";
	}
}
