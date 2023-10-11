using System.Collections.Generic;
using UnityEngine;

public class PolygonColliderData
{
	private List<Vector2[]> _paths;

	private Rect bounds;

	public int pathCount { get; private set; }

	public PolygonColliderData(PolygonCollider2D poly)
	{
		pathCount = poly.pathCount;
		_paths = new List<Vector2[]>(pathCount);
		for (int i = 0; i < pathCount; i++)
		{
			_paths.Add(poly.GetPath(i));
		}
		bounds = poly.GetBoundingRect();
	}

	public Vector2[] GetPath(int index)
	{
		return _paths[index];
	}

	public Rect GetBoundingRect()
	{
		return bounds;
	}
}
