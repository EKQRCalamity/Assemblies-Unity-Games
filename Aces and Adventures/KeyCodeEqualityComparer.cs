using System.Collections.Generic;
using UnityEngine;

public class KeyCodeEqualityComparer : IEqualityComparer<KeyCode>
{
	public static KeyCodeEqualityComparer Default = new KeyCodeEqualityComparer();

	public bool Equals(KeyCode x, KeyCode y)
	{
		return x == y;
	}

	public int GetHashCode(KeyCode obj)
	{
		return (int)obj;
	}
}
