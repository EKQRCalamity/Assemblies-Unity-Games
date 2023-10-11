using System.Collections.Generic;
using UnityEngine;

public static class SetPropertyUtility
{
	public static bool SetStruct<T>(ref T currentValue, T newValue, IEqualityComparer<T> equalityComparer = null) where T : struct
	{
		bool result = !(equalityComparer ?? EqualityComparer<T>.Default).Equals(currentValue, newValue);
		currentValue = newValue;
		return result;
	}

	public static bool SetStruct<T>(ref T? currentValue, T? newValue) where T : struct
	{
		bool result = !currentValue.Equals(newValue);
		currentValue = newValue;
		return result;
	}

	public static bool SetStruct(ref bool currentValue, bool newValue)
	{
		bool result = currentValue != newValue;
		currentValue = newValue;
		return result;
	}

	public static bool SetStruct(ref bool? currentValue, bool? newValue)
	{
		bool result = currentValue != newValue;
		currentValue = newValue;
		return result;
	}

	public static bool SetStruct(ref float currentValue, float newValue)
	{
		bool result = currentValue != newValue;
		currentValue = newValue;
		return result;
	}

	public static bool SetStruct(ref float? currentValue, float? newValue)
	{
		bool result = currentValue != newValue;
		currentValue = newValue;
		return result;
	}

	public static bool SetStruct(ref double currentValue, double newValue)
	{
		bool result = currentValue != newValue;
		currentValue = newValue;
		return result;
	}

	public static bool SetStruct(ref long currentValue, long newValue)
	{
		bool result = currentValue != newValue;
		currentValue = newValue;
		return result;
	}

	public static bool SetStruct(ref ulong currentValue, ulong newValue)
	{
		bool result = currentValue != newValue;
		currentValue = newValue;
		return result;
	}

	public static bool SetStruct(ref int currentValue, int newValue)
	{
		bool result = currentValue != newValue;
		currentValue = newValue;
		return result;
	}

	public static bool SetStruct(ref int? currentValue, int? newValue)
	{
		bool result = currentValue != newValue;
		currentValue = newValue;
		return result;
	}

	public static bool SetStruct(ref uint currentValue, uint newValue)
	{
		bool result = currentValue != newValue;
		currentValue = newValue;
		return result;
	}

	public static bool SetStruct(ref short currentValue, short newValue)
	{
		bool result = currentValue != newValue;
		currentValue = newValue;
		return result;
	}

	public static bool SetStruct(ref ushort currentValue, ushort newValue)
	{
		bool result = currentValue != newValue;
		currentValue = newValue;
		return result;
	}

	public static bool SetStruct(ref byte currentValue, byte newValue)
	{
		bool result = currentValue != newValue;
		currentValue = newValue;
		return result;
	}

	public static bool SetStruct(ref sbyte currentValue, sbyte newValue)
	{
		bool result = currentValue != newValue;
		currentValue = newValue;
		return result;
	}

	public static bool SetStruct(ref Vector2 currentValue, Vector2 newValue)
	{
		bool result = currentValue != newValue;
		currentValue = newValue;
		return result;
	}

	public static bool SetStruct(ref Vector3 currentValue, Vector3 newValue)
	{
		bool result = currentValue != newValue;
		currentValue = newValue;
		return result;
	}

	public static bool SetStruct(ref Color32 currentValue, Color32 newValue)
	{
		bool result = !currentValue.EqualTo(newValue);
		currentValue = newValue;
		return result;
	}

	public static bool SetObject<T>(ref T currentValue, T newValue) where T : class
	{
		bool result = !ReflectionUtil.SafeEquals(currentValue, newValue);
		currentValue = newValue;
		return result;
	}

	public static bool SetObjectEQ<T>(ref T currentValue, T newValue, IEqualityComparer<T> equalityComparer = null, bool repoolCurrentValueOnChange = false) where T : class
	{
		bool num = ((currentValue == null || newValue == null) ? (currentValue != newValue) : (!(equalityComparer ?? EqualityComparer<T>.Default).Equals(currentValue, newValue)));
		if (num && repoolCurrentValueOnChange)
		{
			Pools.TryRepool(ref currentValue);
		}
		currentValue = newValue;
		return num;
	}
}
