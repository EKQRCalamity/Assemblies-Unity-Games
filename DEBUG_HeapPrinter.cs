using System;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

public class DEBUG_HeapPrinter : MonoBehaviour
{
	private class CircularCounter
	{
		private long[] values;

		private int currentIndex;

		public CircularCounter(int size)
		{
			values = new long[size];
		}

		public void Add(long value)
		{
			values[currentIndex] = value;
			currentIndex++;
			if (currentIndex >= values.Length)
			{
				currentIndex = 0;
			}
		}

		public float Average()
		{
			long num = 0L;
			for (int i = 0; i < values.Length; i++)
			{
				num += values[i];
			}
			return (float)num / (float)values.Length;
		}
	}

	private static readonly Vector2 Size = new Vector2(250f, 70f);

	private static readonly float HighlightTime = 3f;

	private static readonly int SmallFontSize = 24;

	private static readonly int LargeFontSize = 50;

	private static readonly int CounterSize = 30;

	private bool styleInitialized;

	private GUIStyle style;

	private long previousMemory = long.MaxValue;

	private float highlightTimer = float.MaxValue;

	private StringBuilder builder = new StringBuilder(100);

	private CircularCounter counter = new CircularCounter(CounterSize);

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnGUI()
	{
		if (!styleInitialized)
		{
			styleInitialized = true;
			style = new GUIStyle(GUI.skin.box);
			style.alignment = TextAnchor.MiddleRight;
			style.fontStyle = FontStyle.Bold;
			style.richText = true;
			style.fontSize = 24;
		}
		long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
		long value = totalMemory - previousMemory;
		counter.Add(value);
		if (previousMemory > totalMemory)
		{
			highlightTimer = 0f;
		}
		Vector2 size = Size;
		float num = size.x;
		Vector2 size2 = Size;
		float num2 = size2.y;
		string value2 = string.Empty;
		if (highlightTimer < HighlightTime)
		{
			highlightTimer += Time.unscaledDeltaTime;
			style.fontSize = LargeFontSize;
			builder.Append("<color=red>");
			value2 = "</color>";
			num *= 2f;
			num2 *= 2f;
		}
		else
		{
			style.fontSize = SmallFontSize;
		}
		long value3 = totalMemory / 1024;
		long value4 = Profiler.GetMonoHeapSizeLong() / 1024;
		builder.Append(value3);
		builder.Append(" / ");
		builder.Append(value4);
		builder.Append("\n");
		builder.Append((counter.Average() / 1024f).ToString("F2"));
		builder.Append("kb / frame");
		builder.Append(value2);
		GUI.Box(new Rect((float)Screen.width - num, (float)Screen.height - num2, num, num2), builder.ToString(), style);
		builder.Length = 0;
		previousMemory = totalMemory;
	}
}
