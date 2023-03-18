using System.Collections.Generic;
using Framework.Managers;
using Framework.Util.CompletionCommandHelper;
using UnityEngine;

namespace Gameplay.UI.Console;

public class CompletionCommand : ConsoleCommand
{
	private const string COMMAND = "completion";

	private CompletionAssets gameData;

	private const string RESOURCE_PATH = "Game Completion Data/Completion Data";

	public override void Start()
	{
		gameData = Resources.Load<CompletionAssets>("Game Completion Data/Completion Data");
	}

	public override string GetName()
	{
		return "completion";
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "completion")
		{
			ParseCompletion(subcommand, listParameters);
		}
	}

	private void ParseCompletion(string command, List<string> paramList)
	{
		switch (command.ToUpperInvariant())
		{
		case "GET":
		case "SHOW":
			ShowCurrentCompletion();
			break;
		case "BASE":
			RunUnlock(isNGPlus: false);
			break;
		case "NG+":
			RunUnlock(isNGPlus: true);
			break;
		default:
			ShowHelp();
			break;
		}
	}

	private float ShowCurrentCompletion()
	{
		float percentCompleted = Core.Persistence.PercentCompleted;
		base.Console.WriteFormat("Current completion: {0:00.000}%", percentCompleted);
		return percentCompleted;
	}

	private void RunUnlock(bool isNGPlus)
	{
		string text = ((!isNGPlus) ? "base" : "NG+");
		float num = ShowCurrentCompletion();
		base.Console.WriteFormat("Unlocking {0} game items...", text);
		Run(gameData.UnlockBaseGame());
		if (isNGPlus)
		{
			Run(gameData.UnlockNGPlus());
		}
		base.Console.Write("... DONE");
		float num2 = ShowCurrentCompletion();
		base.Console.WriteFormat("TOTAL % added: {0:00.000}%", num2 - num);
	}

	private void Run(IEnumerable<string> enumerable)
	{
		foreach (string item in enumerable)
		{
			base.Console.Write(item);
			ShowCurrentCompletion();
		}
	}

	private void ShowHelp()
	{
		base.Console.WriteFormat("Available {0} commands:", "completion");
		base.Console.WriteFormat("{0} get: Shows current completion %", "completion");
		base.Console.WriteFormat("{0} base: Unlocks 100% base game", "completion");
		base.Console.WriteFormat("{0} ng+: Unlocks 150%: 100% base game + 50% NG+", "completion");
	}
}
