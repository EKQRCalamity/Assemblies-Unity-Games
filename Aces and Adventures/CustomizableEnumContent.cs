using System;
using System.Collections.Generic;
using UnityEngine;

public class CustomizableEnumContent : MonoBehaviour
{
	[Header("Primary Color")]
	public bool hasPrimaryColor = true;

	[ColorUsage(true, true)]
	public Color defaultPrimaryColor = Color.white;

	public ColorEvent OnPrimaryColorChange;

	[Header("Secondary Color")]
	public bool hasSecondaryColor;

	[ColorUsage(true, true)]
	public Color defaultSecondaryColor = Color.white;

	public ColorEvent OnSecondaryColorChange;

	[Header("Custom Events")]
	public List<NamedFloatEvent> customFloatEvents;

	public List<NamedColorEvent> customColorEvents;

	public bool hasCustomFloats => !customFloatEvents.IsNullOrEmpty();

	public bool hasCustomColors => !customColorEvents.IsNullOrEmpty();

	private void OnEnable()
	{
		SetPrimaryColor(defaultPrimaryColor);
		SetSecondaryColor(defaultSecondaryColor);
	}

	public void SetPrimaryColor(Color color)
	{
		if (hasPrimaryColor)
		{
			OnPrimaryColorChange.Invoke(color);
		}
	}

	public void SetSecondaryColor(Color color)
	{
		if (hasSecondaryColor)
		{
			OnSecondaryColorChange.Invoke(color);
		}
	}

	public void SetFloat(string eventName, float value)
	{
		if (customFloatEvents.IsNullOrEmpty())
		{
			return;
		}
		foreach (NamedFloatEvent customFloatEvent in customFloatEvents)
		{
			if (customFloatEvent.name == eventName)
			{
				customFloatEvent.SetValue(value);
				break;
			}
		}
	}

	public void SetCustomFloatsToDefaultValues()
	{
		foreach (NamedFloatEvent customFloatEvent in customFloatEvents)
		{
			customFloatEvent.SetValue(customFloatEvent.defaultValue);
		}
	}

	public void SetCustomFloatValues(Dictionary<string, RangeF> customFloatValues, System.Random random)
	{
		if (!customFloatValues.IsNullOrEmpty())
		{
			foreach (NamedFloatEvent customFloatEvent in customFloatEvents)
			{
				customFloatEvent.SetValue(customFloatValues.ContainsKey(customFloatEvent.name) ? random.Range(customFloatValues[customFloatEvent.name]) : customFloatEvent.defaultValue);
			}
			return;
		}
		SetCustomFloatsToDefaultValues();
	}

	public void SetCustomColorsToDefaultValues()
	{
		foreach (NamedColorEvent customColorEvent in customColorEvents)
		{
			customColorEvent.SetValue(customColorEvent.defaultValue);
		}
	}

	public void SetCustomColorValues(Dictionary<string, OptionalTints> customColorValues, System.Random random)
	{
		if (!customColorValues.IsNullOrEmpty())
		{
			foreach (NamedColorEvent customColorEvent in customColorEvents)
			{
				customColorEvent.SetValue(customColorValues.ContainsKey(customColorEvent.name) ? (customColorValues[customColorEvent.name].GetTint(random) ?? customColorEvent.defaultValue) : customColorEvent.defaultValue);
			}
			return;
		}
		SetCustomColorsToDefaultValues();
	}
}
