using System;
using System.Reflection;
using UnityEngine;

namespace AmplifyImpostors;

public static class SpriteUtilityEx
{
	private static Type type;

	public static Type Type
	{
		get
		{
			if (!(type == null))
			{
				return type;
			}
			return type = Type.GetType("UnityEditor.Sprites.SpriteUtility, UnityEditor");
		}
	}

	public static void GenerateOutline(Texture2D texture, Rect rect, float detail, byte alphaTolerance, bool holeDetection, out Vector2[][] paths)
	{
		Vector2[][] array = new Vector2[0][];
		object[] array2 = new object[6] { texture, rect, detail, alphaTolerance, holeDetection, array };
		Type.GetMethod("GenerateOutline", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, array2);
		paths = (Vector2[][])array2[5];
	}
}
