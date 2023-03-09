using System;
using UnityEngine;

public class GeometryUtils
{
	public enum Axis
	{
		X,
		Y,
		Z
	}

	public static Vector3[] GetCircle(Vector3 center, float radius, Axis axis = Axis.Y, int resolution = 128)
	{
		Vector3[] array = new Vector3[resolution];
		float num = (float)Math.PI * 2f / (float)resolution;
		for (int i = 0; i < resolution; i++)
		{
			float f = num * (float)i;
			float x = radius * Mathf.Cos(f);
			float y = radius * Mathf.Sin(f);
			ref Vector3 reference = ref array[i];
			reference = new Vector3(x, y, 0f);
		}
		Quaternion quaternion = axis switch
		{
			Axis.X => Quaternion.AngleAxis(90f, Vector3.up), 
			Axis.Y => Quaternion.AngleAxis(90f, Vector3.right), 
			_ => Quaternion.AngleAxis(0f, Vector3.up), 
		};
		for (int j = 0; j < array.Length; j++)
		{
			ref Vector3 reference2 = ref array[j];
			reference2 = quaternion * array[j] + center;
		}
		return array;
	}
}
