using System;
using Sirenix.Serialization;
using UnityEngine;

namespace Framework.Map;

[Serializable]
public class CellKey
{
	[OdinSerialize]
	public int X { get; private set; }

	[OdinSerialize]
	public int Y { get; private set; }

	public CellKey(CellKey other)
	{
		X = other.X;
		Y = other.Y;
	}

	public CellKey(int x, int y)
	{
		X = x;
		Y = y;
	}

	public override int GetHashCode()
	{
		string text = X + "|" + Y;
		return text.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as CellKey);
	}

	public bool Equals(CellKey obj)
	{
		return obj != null && obj.X == X && obj.Y == Y;
	}

	public Vector2Int GetVector2()
	{
		return new Vector2Int(X, Y);
	}

	public override string ToString()
	{
		return $"({X}, {Y})";
	}
}
