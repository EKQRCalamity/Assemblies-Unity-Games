using System;
using System.Linq.Expressions;

public static class CachedCastToExpression<S, T>
{
	internal static readonly Func<S, T> cast = Get();

	private static Func<S, T> Get()
	{
		return ((Expression<Func<S, T>>)((S p) => (T)p)).Compile();
	}
}
