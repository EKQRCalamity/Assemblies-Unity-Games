using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class StringLocalizer : MonoBehaviour
{
	public Action updateString;

	private void _OnLocaleChange(Locale locale)
	{
		updateString?.Invoke();
	}

	private void Awake()
	{
		LocalizationSettings.Instance.OnSelectedLocaleChanged += _OnLocaleChange;
	}

	private void OnDestroy()
	{
		LocalizationSettings.Instance.OnSelectedLocaleChanged -= _OnLocaleChange;
	}
}
