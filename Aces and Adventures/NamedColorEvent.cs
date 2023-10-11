using System;
using UnityEngine;

[Serializable]
public class NamedColorEvent
{
	public string name;

	[ColorUsage(true, true)]
	public Color defaultValue = Color.white;

	public ColorEvent colorEvent;

	public StringColorEvent stringColorEvent;

	public void SetValue(Color value)
	{
		colorEvent?.Invoke(value);
		stringColorEvent?.Invoke(name, value);
	}
}
