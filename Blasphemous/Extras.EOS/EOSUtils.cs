using System;
using System.Linq;

namespace Extras.EOS;

public static class EOSUtils
{
	public static T GetBestValue<T>(T fallbackValue, string argName)
	{
		string commandLineArgValue = GetCommandLineArgValue(argName);
		commandLineArgValue = commandLineArgValue.Trim();
		if (fallbackValue.GetType().IsEnum && !string.IsNullOrEmpty(commandLineArgValue) && int.TryParse(commandLineArgValue, out var result) && Enum.IsDefined(fallbackValue.GetType(), result))
		{
			return (T)(object)result;
		}
		return fallbackValue;
	}

	public static string GetBestValue(string fallbackValue, string argName)
	{
		string commandLineArgValue = GetCommandLineArgValue(argName);
		commandLineArgValue = commandLineArgValue.Trim();
		if (!string.IsNullOrEmpty(commandLineArgValue))
		{
			return commandLineArgValue;
		}
		return fallbackValue;
	}

	public static string GetCommandLineArgValue(string argName)
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].StartsWith(argName))
			{
				string[] array = commandLineArgs[i].Split('=');
				if (array.Length > 1)
				{
					return array[1];
				}
			}
		}
		return string.Empty;
	}

	public static bool HasCommandLineArg(string argName)
	{
		return Environment.GetCommandLineArgs().Contains(argName);
	}
}
