using System;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Tools.Gameplay;

public class ColliderUtil
{
	private static Collider2D[] result = new Collider2D[10];

	public static Collider2D[] QueryBox(Vector2 position, Vector2 size, float angle, string tag)
	{
		Physics2D.OverlapBoxNonAlloc(position, size, angle, result);
		return result;
	}

	public static Collider2D[] QueryCircle(Vector2 position, float size, string tag)
	{
		Physics2D.OverlapCircleNonAlloc(position, size, result);
		return result;
	}

	public static Entity QueryCircleEntity(Vector2 position, float size, string tag)
	{
		Physics2D.OverlapCircleNonAlloc(position, size, result);
		return Array.Find(result, (Collider2D value) => value.CompareTag(tag)).GetComponent<Entity>();
	}

	public static Entity QueryBoxEntity(Vector2 position, Vector2 size, float angle, string tag)
	{
		Physics2D.OverlapBoxNonAlloc(position, size, angle, result);
		return Array.Find(result, (Collider2D value) => value.CompareTag(tag)).GetComponent<Entity>();
	}

	public static Collider2D[] QueryCollider(Collider2D col, string tag)
	{
		col.GetContacts(result);
		return Array.FindAll(result, (Collider2D value) => !(value == null) && value.CompareTag(tag));
	}
}
