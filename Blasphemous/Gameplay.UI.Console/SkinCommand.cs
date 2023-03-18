using System.Collections.Generic;
using System.Linq;
using Framework.Managers;

namespace Gameplay.UI.Console;

public class SkinCommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "skin")
		{
			ParseSkin(subcommand, listParameters);
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("skin");
		return list;
	}

	private void ParseSkin(string command, List<string> paramList)
	{
		string command2 = "skin " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available SKIN commands:");
				base.Console.Write("list: get all the possible SKIN_IDs.");
				base.Console.Write("listunlocked: get all the unlocked SKIN_IDs.");
				base.Console.Write("get: get the current SKIN_ID.");
				base.Console.Write("set: set the current skin with a given SKIN_ID.");
				base.Console.Write("unlock [SKIN_ID | ALL]: unlocks the skin with SKIN_ID or all of them");
				base.Console.Write("lock [SKIN_ID | ALL]: locks the skin with SKIN_ID or all of them");
			}
			break;
		case "list":
			if (!ValidateParams(command2, 0, paramList))
			{
				break;
			}
			{
				foreach (string item in Core.ColorPaletteManager.GetAllColorPalettesId())
				{
					base.Console.Write("Skin ID: " + item);
				}
				break;
			}
		case "listunlocked":
			if (!ValidateParams(command2, 0, paramList))
			{
				break;
			}
			{
				foreach (string item2 in Core.ColorPaletteManager.GetAllUnlockedColorPalettesId())
				{
					base.Console.Write("Skin UNLOCKED ID: " + item2);
				}
				break;
			}
		case "get":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Current Skin ID: " + Core.ColorPaletteManager.GetCurrentColorPaletteId());
			}
			break;
		case "set":
			if (ValidateParams(command2, 1, paramList))
			{
				string text2 = paramList[0].ToUpper();
				if (Core.ColorPaletteManager.GetAllColorPalettesId().Contains(text2))
				{
					Core.ColorPaletteManager.SetCurrentColorPaletteId(text2);
				}
				else
				{
					base.Console.Write("Skin ID '" + text2 + "' doesn't exist.");
				}
			}
			break;
		case "unlock":
			if (ValidateParams(command2, 1, paramList))
			{
				string text3 = paramList[0].ToUpperInvariant();
				if (text3 == "ALL")
				{
					UnlockAllColorPalettes();
				}
				else if (Core.ColorPaletteManager.GetAllColorPalettesId().Contains(text3))
				{
					Core.ColorPaletteManager.UnlockColorPalette(text3);
					base.Console.Write("Skin with ID: " + text3 + " has been unlocked.");
				}
				else
				{
					base.Console.Write("A skin with ID: " + text3 + " doesn't exist!");
				}
			}
			break;
		case "lock":
			if (ValidateParams(command2, 1, paramList))
			{
				string text = paramList[0].ToUpperInvariant();
				if (text == "ALL")
				{
					LockAllColorPalettes();
				}
				else if (Core.ColorPaletteManager.GetAllColorPalettesId().Contains(text))
				{
					Core.ColorPaletteManager.LockColorPalette(text);
					base.Console.Write("Skin with ID: " + text + " has been locked.");
				}
				else
				{
					base.Console.Write("A skin with ID: " + text + " doesn't exist!");
				}
			}
			break;
		default:
			base.Console.Write("Command unknow, use skin help");
			break;
		}
	}

	private static void LockAllColorPalettes()
	{
		foreach (string item in from palette in Core.ColorPaletteManager.GetAllColorPalettesId()
			where palette != "PENITENT_DEFAULT"
			select palette)
		{
			Core.ColorPaletteManager.LockColorPalette(item);
		}
	}

	private static void UnlockAllColorPalettes()
	{
		string currentColorPaletteId = Core.ColorPaletteManager.GetCurrentColorPaletteId();
		foreach (string item in Core.ColorPaletteManager.GetAllColorPalettesId())
		{
			Core.ColorPaletteManager.UnlockColorPalette(item, showPopup: false);
		}
		Core.ColorPaletteManager.SetCurrentSkinToSkinSettings(currentColorPaletteId);
	}
}
