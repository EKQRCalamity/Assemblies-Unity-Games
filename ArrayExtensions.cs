using UnityEngine;

public static class ArrayExtensions
{
	public static T GetRandom<T>(this T[] array)
	{
		return array[Random.Range(0, array.Length)];
	}

	public static T GetLast<T>(this T[] array)
	{
		return array[array.Length - 1];
	}
}
