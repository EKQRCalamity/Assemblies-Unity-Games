using System;
using System.Collections.Generic;
using UnityEngine;

public class PolyEdge : IQuadTreeObject<PolyEdge>
{
	public Vector2 a;

	public Vector2 b;

	protected List<Ushort2> partitionIndices;

	public PolyEdge(Vector2 a, Vector2 b)
	{
		this.a = a;
		this.b = b;
		partitionIndices = new List<Ushort2>();
	}

	public void AddToQuadTree(QuadTree<PolyEdge> tree)
	{
		Vector2 dir = default(Vector2);
		dir.x = b.x - a.x;
		dir.y = b.y - a.y;
		float num = (float)Math.Sqrt(dir.x * dir.x + dir.y * dir.y);
		dir.x /= num;
		dir.y /= num;
		tree.Add(this, a, dir, num, 0f);
	}

	public void AddToPartition(ushort x, ushort y)
	{
		partitionIndices.Add(new Ushort2(x, y));
	}

	public void RemoveFromTree(QuadTree<PolyEdge> tree)
	{
		for (int i = 0; i < partitionIndices.Count; i++)
		{
			Ushort2 @ushort = partitionIndices[i];
			tree.Partitions[@ushort.x, @ushort.y].Remove(this);
		}
		partitionIndices.Clear();
	}
}
