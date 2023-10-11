using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

public static class ConstructorCacheType
{
	private static readonly Dictionary<Type, Func<object>> _ConstructorsByType = new Dictionary<Type, Func<object>>();

	private static readonly Dictionary<Type, object> _Defaults = new Dictionary<Type, object>();

	public static object Construct(Type typeToConstruct)
	{
		if (!_ConstructorsByType.ContainsKey(typeToConstruct))
		{
			_ConstructorsByType.Add(typeToConstruct, Expression.Lambda<Func<object>>(Expression.New(typeToConstruct.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], new ParameterModifier[0])), Array.Empty<ParameterExpression>()).Compile());
		}
		return _ConstructorsByType[typeToConstruct]();
	}

	public static T Construct<T>(Type typeToConstruct)
	{
		return (T)Construct(typeToConstruct);
	}

	public static object GetDefault(Type type)
	{
		if (!_Defaults.ContainsKey(type))
		{
			_Defaults.Add(type, Construct(type));
		}
		return _Defaults[type];
	}

	public static T GetDefault<T>(Type type)
	{
		return (T)GetDefault(type);
	}
}
