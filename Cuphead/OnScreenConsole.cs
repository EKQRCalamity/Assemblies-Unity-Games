using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class OnScreenConsole : MonoBehaviour
{
	private class OnScreenConsoleLogger : ILogHandler
	{
		private static readonly int QueueSize = 15;

		public Queue<string> logQueue = new Queue<string>(QueueSize);

		private ILogHandler defaultLogHandler = UnityEngine.Debug.unityLogger.logHandler;

		public OnScreenConsoleLogger()
		{
			UnityEngine.Debug.unityLogger.logHandler = this;
		}

		public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
		{
			addToQueue($"[{logType}, {Time.frameCount}] {string.Format(format, args)}");
			defaultLogHandler.LogFormat(logType, context, format, args);
		}

		public void LogException(Exception exception, UnityEngine.Object context)
		{
			defaultLogHandler.LogException(exception, context);
		}

		private void addToQueue(string value)
		{
			if (logQueue.Count == QueueSize)
			{
				logQueue.Dequeue();
			}
			logQueue.Enqueue(value);
		}
	}

	private static readonly Vector2 Size = new Vector2(0.5f, 0.4f);

	private static readonly int MaximumStringLength = 500;

	private OnScreenConsoleLogger logger;

	private GUIStyle style;

	private StringBuilder builder = new StringBuilder();

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		logger = new OnScreenConsoleLogger();
	}

	private void OnGUI()
	{
		if (style == null)
		{
			style = new GUIStyle(GUI.skin.GetStyle("Box"));
			style.alignment = TextAnchor.LowerLeft;
			style.wordWrap = true;
		}
		foreach (string item in logger.logQueue)
		{
			string value = item;
			if (item.Length > MaximumStringLength)
			{
				value = item.Substring(0, MaximumStringLength);
			}
			builder.AppendLine(value);
		}
		if (builder.Length > 0)
		{
			builder.Length--;
		}
		Vector2 size = Size;
		int num = (int)(size.x * (float)Screen.width);
		Vector2 size2 = Size;
		int num2 = (int)(size2.y * (float)Screen.height);
		GUI.Box(new Rect(Screen.width - num, Screen.height - num2, num, num2), builder.ToString(), style);
		builder.Length = 0;
	}
}
