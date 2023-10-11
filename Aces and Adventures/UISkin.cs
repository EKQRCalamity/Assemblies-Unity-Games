using UnityEngine;

public abstract class UISkin : ScriptableObject
{
	protected T _SetValue<T>(T current, T newValue, ref bool changed)
	{
		if (current != null && current.Equals(newValue))
		{
			return current;
		}
		changed = true;
		return newValue;
	}

	public abstract bool ApplyTo(Object obj);
}
