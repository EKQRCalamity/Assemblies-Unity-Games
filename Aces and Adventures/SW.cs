using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class SW
{
	private static List<string> taskNames;

	private static Dictionary<string, Stopwatch> timers;

	private static Queue<string> messages;

	static SW()
	{
		taskNames = new List<string>();
		timers = new Dictionary<string, Stopwatch>();
		messages = new Queue<string>();
	}

	[Conditional("TEST_BUILD")]
	[Conditional("UNITY_EDITOR")]
	public static void Begin(string taskName)
	{
		if (!timers.ContainsKey(taskName))
		{
			taskNames.Add(taskName);
			Stopwatch stopwatch = new Stopwatch();
			timers.Add(taskName, stopwatch);
			stopwatch.Start();
		}
		else if (!timers[taskName].IsRunning)
		{
			taskNames.Add(taskName);
			timers[taskName].Start();
		}
	}

	[Conditional("TEST_BUILD")]
	[Conditional("UNITY_EDITOR")]
	public static void Pause(string taskName)
	{
		if (timers.ContainsKey(taskName))
		{
			timers[taskName].Stop();
			taskNames.Remove(taskName);
			return;
		}
		UnityEngine.Debug.LogWarning("Task [" + taskName + "] is not currently running. Please call SW.Begin(" + taskName + ") before calling Pause.");
	}

	[Conditional("TEST_BUILD")]
	[Conditional("UNITY_EDITOR")]
	public static void Pause()
	{
		if (taskNames.Count <= 0)
		{
			UnityEngine.Debug.LogWarning("SW: No tasks currently running, please Start a task before calling Pause.");
		}
	}

	[Conditional("TEST_BUILD")]
	[Conditional("UNITY_EDITOR")]
	public static void End(string taskName, double showMessageTimeThreshold = 0.0, bool appendStackTrace = false)
	{
		if (timers.ContainsKey(taskName))
		{
			Stopwatch stopwatch = timers[taskName];
			stopwatch.Stop();
			if (stopwatch.Elapsed.TotalMilliseconds >= showMessageTimeThreshold)
			{
				messages.Enqueue("Task [" + taskName + "] completed in [" + stopwatch.Elapsed.TotalMilliseconds.ToString("F") + "] milliseconds.\n\n" + (appendStackTrace ? Environment.StackTrace : ""));
			}
			timers.Remove(taskName);
			taskNames.Remove(taskName);
		}
		else
		{
			UnityEngine.Debug.LogWarning("Task [" + taskName + "] is not currently running. Please call SW.Begin(" + taskName + ") before calling End.");
		}
	}

	[Conditional("TEST_BUILD")]
	[Conditional("UNITY_EDITOR")]
	public static void End(double showMessageTimeThreshold = 0.0, bool appendStackTrace = false)
	{
		if (taskNames.Count <= 0)
		{
			UnityEngine.Debug.LogWarning("SW: No tasks currently running, please Begin a task before calling End.");
		}
	}

	public static void Message(string message, bool appendStackTrace = false)
	{
		messages.Enqueue(message + "\n\n" + (appendStackTrace ? Environment.StackTrace : ""));
	}

	private static IEnumerator _Update()
	{
		while (true)
		{
			if (messages.Count > 0)
			{
				UnityEngine.Debug.Log(messages.Dequeue());
			}
			else
			{
				yield return null;
			}
		}
	}
}
