using System.Collections.Generic;
using UnityEngine;

public class ResolutionStringEqualityComparer : IEqualityComparer<string>
{
	public static readonly ResolutionStringEqualityComparer Default = new ResolutionStringEqualityComparer();

	public bool Equals(string x, string y)
	{
		return default(Resolution).FromString(x).EqualResolution(default(Resolution).FromString(y));
	}

	public int GetHashCode(string obj)
	{
		Resolution resolution = default(Resolution).FromString(obj);
		return resolution.width ^ (resolution.height << 16);
	}
}
