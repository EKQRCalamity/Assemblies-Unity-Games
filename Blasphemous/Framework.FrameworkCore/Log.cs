using Framework.Managers;
using Sirenix.Utilities;
using UnityEngine;

namespace Framework.FrameworkCore;

public class Log : GameSystem
{
	private const string MODULE_PREFIX = "[";

	private const string MODULE_SUFFIX = "]";

	public static void Raw(string text)
	{
		UnityEngine.Debug.Log(text);
	}

	public static void Trace(string module, string text, Object context = null)
	{
		string message = "[TRACE]" + CustomFormat(module, text);
		UnityEngine.Debug.Log(message, context);
	}

	public static void Trace(string text, Object context = null)
	{
		string message = "[TRACE]" + CustomFormat(string.Empty, text);
		UnityEngine.Debug.Log(message, context);
	}

	public static void Debug(string module, string text, Object context = null)
	{
		string message = "[DEBUG]" + CustomFormat(module, text);
		UnityEngine.Debug.Log(message, context);
	}

	public static void Debug(string text, Object context = null)
	{
		string message = "[DEBUG]" + CustomFormat(string.Empty, text);
		UnityEngine.Debug.Log(message, context);
	}

	public static void Warning(string module, string text, Object context = null)
	{
		string message = "[WARN]" + CustomFormat(module, text);
		UnityEngine.Debug.LogWarning(message, context);
	}

	public static void Warning(string text, Object context = null)
	{
		string message = "[WARN]" + CustomFormat(string.Empty, text);
		UnityEngine.Debug.LogWarning(message, context);
	}

	public static void Error(string module, string text, Object context = null)
	{
		string message = "[ERROR]" + CustomFormat(module, text);
		UnityEngine.Debug.LogError(message, context);
	}

	public static void Error(string text, GameObject context = null)
	{
		string message = "[ERROR]" + CustomFormat(string.Empty, text);
		UnityEngine.Debug.LogError(message, context);
	}

	private static string CustomFormat(string module, string text)
	{
		if (!module.IsNullOrWhitespace())
		{
			module = "[" + module.ToUpper() + "] ";
			text = text.Replace("<!", "<i><color=blue>");
			text = text.Replace("!>", "</color></i>");
		}
		return module + text;
	}
}
