using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Internal;

public static class Debug
{
	public static bool developerConsoleVisible
	{
		get
		{
			return UnityEngine.Debug.developerConsoleVisible;
		}
		set
		{
			UnityEngine.Debug.developerConsoleVisible = value;
		}
	}

	public static bool isDebugBuild => UnityEngine.Debug.isDebugBuild;

	[Conditional("UNITY_ASSERTIONS")]
	public static void Assert(bool condition)
	{
	}

	[Conditional("UNITY_ASSERTIONS")]
	public static void Assert(bool condition, string message)
	{
	}

	[Conditional("UNITY_ASSERTIONS")]
	public static void AssertFormat(bool condition, string format, params object[] args)
	{
	}

	public static void Break()
	{
		UnityEngine.Debug.Break();
	}

	public static void ClearDeveloperConsole()
	{
		UnityEngine.Debug.ClearDeveloperConsole();
	}

	public static void DebugBreak()
	{
		UnityEngine.Debug.DebugBreak();
	}

	public static void DrawLine(Vector3 start, Vector3 end)
	{
		UnityEngine.Debug.DrawLine(start, end);
	}

	public static void DrawLine(Vector3 start, Vector3 end, Color color)
	{
		UnityEngine.Debug.DrawLine(start, end, color);
	}

	public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration)
	{
		UnityEngine.Debug.DrawLine(start, end, color, duration);
	}

	public static void DrawLine(Vector3 start, Vector3 end, [DefaultValue("Color.white")] Color color, [DefaultValue("0.0f")] float duration, [DefaultValue("true")] bool depthTest)
	{
		UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
	}

	public static void DrawRay(Vector3 start, Vector3 dir)
	{
		UnityEngine.Debug.DrawRay(start, dir);
	}

	public static void DrawRay(Vector3 start, Vector3 dir, Color color)
	{
		UnityEngine.Debug.DrawRay(start, dir, color);
	}

	public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration)
	{
		UnityEngine.Debug.DrawRay(start, dir, color, duration);
	}

	public static void DrawRay(Vector3 start, Vector3 dir, [DefaultValue("Color.white")] Color color, [DefaultValue("0.0f")] float duration, [DefaultValue("true")] bool depthTest)
	{
		UnityEngine.Debug.DrawRay(start, dir, color, duration, depthTest);
	}

	public static void LogInfo(object message, UnityEngine.Object context = null)
	{
		UnityEngine.Debug.Log(message, context);
	}

	public static void LogInfoCat(params object[] args)
	{
		UnityEngine.Debug.Log(string.Concat(args));
	}

	[Conditional("VERBOSE")]
	public static void Log(object message, UnityEngine.Object context = null)
	{
		UnityEngine.Debug.Log(message, context);
	}

	[Conditional("VERBOSE")]
	public static void LogCat(params object[] args)
	{
		UnityEngine.Debug.Log(string.Concat(args));
	}

	public static void LogError(object message, UnityEngine.Object context = null)
	{
		UnityEngine.Debug.LogError(message, context);
	}

	public static void LogErrorCat(params object[] args)
	{
		UnityEngine.Debug.LogError(string.Concat(args));
	}

	public static void LogErrorFormat(string format, params object[] args)
	{
		UnityEngine.Debug.LogErrorFormat(format, args);
	}

	public static void LogErrorFormat(UnityEngine.Object context, string format, params object[] args)
	{
		UnityEngine.Debug.LogErrorFormat(context, format, args);
	}

	public static void LogException(Exception exception)
	{
		UnityEngine.Debug.LogException(exception);
	}

	public static void LogException(Exception exception, UnityEngine.Object context)
	{
		UnityEngine.Debug.LogException(exception, context);
	}

	[Conditional("VERBOSE")]
	public static void LogFormat(string format, params object[] args)
	{
		UnityEngine.Debug.LogFormat(format, args);
	}

	[Conditional("VERBOSE")]
	public static void LogFormat(UnityEngine.Object context, string format, params object[] args)
	{
		UnityEngine.Debug.LogFormat(context, format, args);
	}

	[Conditional("VERBOSE")]
	public static void LogWarning(object message, UnityEngine.Object context = null)
	{
		UnityEngine.Debug.LogWarning(message, context);
	}

	[Conditional("VERBOSE")]
	public static void LogWarningCat(params object[] args)
	{
		UnityEngine.Debug.LogWarning(string.Concat(args));
	}

	[Conditional("VERBOSE")]
	public static void LogWarningFormat(string format, params object[] args)
	{
		UnityEngine.Debug.LogWarningFormat(format, args);
	}

	[Conditional("VERBOSE")]
	public static void LogWarningFormat(UnityEngine.Object context, string format, params object[] args)
	{
		UnityEngine.Debug.LogWarningFormat(context, format, args);
	}
}
