using System.Collections.Generic;
using Framework.Managers;

namespace Gameplay.UI.Console;

public class AudioCommand : ConsoleCommand
{
	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "audio")
		{
			ParseAudio(subcommand, listParameters);
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("audio");
		return list;
	}

	private void ParseAudio(string command, List<string> paramList)
	{
		float resultValue = 0f;
		string command2 = "audio " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available AUDIO commands:");
				base.Console.Write("audio list: List all volumes");
				base.Console.Write("audio master VALUE: Change the master volumen, 0-1");
				base.Console.Write("audio sfx VALUE: Change the sfx volumen, 0-1");
				base.Console.Write("audio music VALUE: Change the music volumen, 0-1");
				base.Console.Write("audio voiceover VALUE: Change the voiceover volumen, 0-1");
			}
			break;
		case "master":
			if (ValidateParams(command2, 1, paramList) && ValidateParam(paramList[0], out resultValue, 0f, 1f))
			{
				Core.Audio.MasterVolume = resultValue;
				base.Console.Write("Master volume setted");
			}
			break;
		case "sfx":
			if (ValidateParams(command2, 1, paramList) && ValidateParam(paramList[0], out resultValue, 0f, 1f))
			{
				Core.Audio.SetSfxVolume(resultValue);
				base.Console.Write("Sfx volume setted");
			}
			break;
		case "music":
			if (ValidateParams(command2, 1, paramList) && ValidateParam(paramList[0], out resultValue, 0f, 1f))
			{
				Core.Audio.SetMusicVolume(resultValue);
				base.Console.Write("Music volume setted");
			}
			break;
		case "voiceover":
			if (ValidateParams(command2, 1, paramList) && ValidateParam(paramList[0], out resultValue, 0f, 1f))
			{
				Core.Audio.SetVoiceoverVolume(resultValue);
				base.Console.Write("Voiceover volume setted");
			}
			break;
		case "list":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Master volume: " + Core.Audio.MasterVolume);
				base.Console.Write("Sfx volume: " + Core.Audio.GetSfxVolume());
				base.Console.Write("Music volume: " + Core.Audio.GetMusicVolume());
				base.Console.Write("Voiceover volume: " + Core.Audio.GetVoiceoverVolume());
			}
			break;
		default:
			base.Console.Write("Command unknow, use audio help");
			break;
		}
	}
}
