using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
{
	public static readonly ReferenceEqualityComparer<T> Default = new ReferenceEqualityComparer<T>();

	bool IEqualityComparer<T>.Equals(T x, T y)
	{
		return x == y;
	}

	int IEqualityComparer<T>.GetHashCode(T obj)
	{
		return RuntimeHelpers.GetHashCode(obj);
	}
}
