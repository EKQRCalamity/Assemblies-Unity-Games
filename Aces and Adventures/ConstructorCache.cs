using System;
using System.Linq.Expressions;
using System.Reflection;

public static class ConstructorCache<T>
{
	public static readonly Func<T> Constructor;

	static ConstructorCache()
	{
		Constructor = Expression.Lambda<Func<T>>(Expression.New(typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], new ParameterModifier[0])), Array.Empty<ParameterExpression>()).Compile();
	}
}
