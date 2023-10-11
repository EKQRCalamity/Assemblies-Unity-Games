using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class ObjectReferenceEqualityComparator : IEqualityComparer<object>
{
	public static readonly IEqualityComparer<object> Default = new ObjectReferenceEqualityComparator();

	bool IEqualityComparer<object>.Equals(object x, object y)
	{
		return x == y;
	}

	int IEqualityComparer<object>.GetHashCode(object obj)
	{
		return RuntimeHelpers.GetHashCode(obj);
	}
}
