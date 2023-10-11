using System;
using UnityEngine;

[Serializable]
public class NamedFloatEvent
{
	public string name;

	[Range(-180f, 180f)]
	public float defaultValue = 1f;

	public FloatEvent floatEvent;

	public StringFloatEvent stringFloatEvent;

	public void SetValue(float value)
	{
		floatEvent?.Invoke(value);
		stringFloatEvent?.Invoke(name, value);
	}
}
