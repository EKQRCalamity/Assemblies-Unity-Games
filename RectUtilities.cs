using System;
using UnityEngine;

public static class RectUtilities
{
	public static Rect AdjustSize(this Rect rect, float left, float right, float top, float bottom)
	{
		rect.xMin += left;
		rect.xMax += right;
		rect.yMin += top;
		rect.yMax += bottom;
		return rect;
	}

	public static Rect SliceLeft(ref Rect rect, float amount)
	{
		Rect result = new Rect(rect);
		result.xMax = result.xMin + amount;
		rect.xMin += amount;
		return result;
	}

	public static Rect SliceRight(ref Rect rect, float amount)
	{
		Rect result = new Rect(rect);
		result.xMin = result.xMax - amount;
		rect.xMax -= amount;
		return result;
	}

	public static Rect SliceTop(ref Rect rect, float amount)
	{
		Rect result = new Rect(rect);
		result.yMax = result.yMin + amount;
		rect.yMin += amount;
		return result;
	}

	public static Rect SliceBottom(ref Rect rect, float amount)
	{
		Rect result = new Rect(rect);
		result.yMin = result.yMax - amount;
		rect.yMax -= amount;
		return result;
	}

	public static Rect[] SplitVertical(Rect rect, int numberOfGeneratedRects)
	{
		float[] array = new float[numberOfGeneratedRects];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = 1f;
		}
		return SplitVertical(rect, array);
	}

	public static Rect[] SplitVertical(Rect rect, params float[] weights)
	{
		float totalWeight = 0f;
		Array.ForEach(weights, delegate(float weight)
		{
			totalWeight += weight;
		});
		Rect[] array = new Rect[weights.Length];
		float height = rect.height;
		for (int i = 0; i < weights.Length - 1; i++)
		{
			ref Rect reference = ref array[i];
			reference = SliceTop(ref rect, Mathf.Floor(height * weights[i] / totalWeight));
		}
		array[array.Length - 1] = rect;
		return array;
	}

	public static Rect[] SplitHorizontal(Rect rect, int numberOfGeneratedRects)
	{
		float[] array = new float[numberOfGeneratedRects];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = 1f;
		}
		return SplitHorizontal(rect, array);
	}

	public static Rect[] SplitHorizontal(Rect rect, params float[] weights)
	{
		float totalWeight = 0f;
		Array.ForEach(weights, delegate(float weight)
		{
			totalWeight += weight;
		});
		Rect[] array = new Rect[weights.Length];
		float width = rect.width;
		for (int i = 0; i < weights.Length - 1; i++)
		{
			ref Rect reference = ref array[i];
			reference = SliceLeft(ref rect, Mathf.Floor(width * weights[i] / totalWeight));
		}
		array[array.Length - 1] = rect;
		return array;
	}
}
