using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class Log
{
	public enum Level
	{
		Text,
		Warning,
		Error
	}

	[Flags]
	public enum LevelFlags
	{
		Text = 1,
		Warning = 2,
		Error = 4
	}

	private static StringBuilder _Builder = new StringBuilder();

	private static Stack<int> _LogStartIndices = new Stack<int>();

	public static void BeginLog()
	{
		_LogStartIndices.Push(_Builder.Length);
	}

	public static void Entry(Level level, string text, bool appendToUserLog)
	{
		if (text.IsNullOrEmpty())
		{
			return;
		}
		switch (level)
		{
		case Level.Text:
			Debug.Log(text);
			break;
		case Level.Warning:
			Debug.LogWarning(text);
			break;
		case Level.Error:
			Debug.LogError(text);
			break;
		}
		if (_LogStartIndices.Count != 0 && appendToUserLog)
		{
			if (_Builder.Length > 0)
			{
				_Builder.Append("\n");
			}
			_Builder.Append("<color=").Append(level.ColorString()).Append(">");
			_Builder.Append("• ").Append(text);
			_Builder.Append("</color>");
		}
	}

	public static void Text(string text, bool appendToUserLog = true)
	{
		Entry(Level.Text, text, appendToUserLog);
	}

	public static void Warning(string warning, bool appendToUserLog = true)
	{
		Entry(Level.Warning, warning, appendToUserLog);
	}

	public static void Error(string error, bool appendToUserLog = true)
	{
		Entry(Level.Error, error, appendToUserLog);
	}

	public static string EndLog()
	{
		if (_LogStartIndices.Count == 0)
		{
			return "";
		}
		int num = _LogStartIndices.Pop();
		string result = _Builder.ToString(num, _Builder.Length - num);
		if (_LogStartIndices.Count == 0)
		{
			_Builder.Clear();
		}
		return result;
	}

	private static string ColorString(this Level level)
	{
		return level switch
		{
			Level.Warning => "#FFFF00F0", 
			Level.Text => "#000000F0", 
			_ => "#FF0000F0", 
		};
	}

	public static bool LogText(this LevelFlags flags)
	{
		return (flags & LevelFlags.Text) != 0;
	}

	public static bool LogWarning(this LevelFlags flags)
	{
		return (flags & LevelFlags.Warning) != 0;
	}

	public static bool LogError(this LevelFlags flags)
	{
		return (flags & LevelFlags.Error) != 0;
	}
}
