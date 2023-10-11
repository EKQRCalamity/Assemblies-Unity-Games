using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class StringEventLocalizer : MonoBehaviour
{
	private static readonly Dictionary<StringEvent, Func<string>> _Map;

	private HashSet<StringEvent> _stringEvents = new HashSet<StringEvent>();

	static StringEventLocalizer()
	{
		_Map = new Dictionary<StringEvent, Func<string>>();
		LocalizationSettings.Instance.OnSelectedLocaleChanged += OnLocalChange;
	}

	public static void OnLocalChange(Locale locale)
	{
		foreach (KeyValuePair<StringEvent, Func<string>> item in _Map)
		{
			item.Key?.Invoke(item.Value?.Invoke() ?? "");
		}
	}

	private void OnDisable()
	{
		foreach (StringEvent stringEvent in _stringEvents)
		{
			_Map.Remove(stringEvent);
		}
		_stringEvents.Clear();
	}

	public void SetData(StringEvent stringEvent, Func<string> getString)
	{
		_Map[_stringEvents.AddReturn(stringEvent)] = getString;
	}
}
