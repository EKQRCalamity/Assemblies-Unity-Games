using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;

namespace Gameplay.UI.Console;

public class LoadLevel : ConsoleCommand
{
	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		switch (command)
		{
		case "load":
		{
			LevelManager.OnLevelLoaded += OnLevelLoaded;
			string levelName = subcommand.ToUpper();
			Core.LevelManager.ChangeLevel(levelName, startFromEditor: false, useFade: true, forceDeactivate: true);
			break;
		}
		case "loadmenu":
			Core.Logic.LoadMenuScene(useFade: false);
			break;
		case "loadnoui":
			UIController.instance.HideAllNotInGameUI();
			Core.SpawnManager.PrepareForCommandSpawn(subcommand.ToUpper());
			Core.LevelManager.ChangeLevel(subcommand.ToUpper(), startFromEditor: false, useFade: true, forceDeactivate: true);
			break;
		}
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		Core.UI.NavigationUI.Show(b: true);
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("load");
		list.Add("loadmenu");
		list.Add("loadnoui");
		return list;
	}

	public override bool HasLowerParameters()
	{
		return false;
	}
}
