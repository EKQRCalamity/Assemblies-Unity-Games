using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class IdUtil
{
	private static readonly Dictionary<Type, Action<object>> _CachedReleaseIdsMethods = new Dictionary<Type, Action<object>>();

	public static Id<T> ToId<T>(this Idable idable) where T : class, Idable
	{
		return new Id<T>(idable.id, idable.tableId);
	}

	public static void ClearId<T>(this Idable<T> idable, ushort tableId) where T : class
	{
		if (idable.tableId == tableId)
		{
			idable.id = 0;
			idable.tableId = 0;
		}
	}

	public static void ReleaseIds<T>(T idsContainer) where T : class
	{
		Type type = idsContainer.GetType();
		if (!_CachedReleaseIdsMethods.ContainsKey(type))
		{
			IEnumerable<Func<T, Ids>> first = from info in idsContainer.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				where info.PropertyType.Is<Ids>()
				select info.GetValueGetter<T, Ids>();
			IEnumerable<Func<T, Ids>> second = from info in idsContainer.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				where info.FieldType.Is<Ids>()
				select info.GetValueGetter<T, Ids>();
			Func<T, Ids>[] idGetters = first.Concat(second).ToArray();
			IEnumerable<Action<T, Ids>> first2 = from info in idsContainer.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				where info.PropertyType.Is<Ids>()
				select info.GetValueSetter<T, Ids>();
			IEnumerable<Action<T, Ids>> second2 = from info in idsContainer.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				where info.FieldType.Is<Ids>()
				select info.GetValueSetter<T, Ids>();
			Action<T, Ids>[] idSetters = first2.Concat(second2).ToArray();
			_CachedReleaseIdsMethods.Add(type, delegate(object obj)
			{
				for (int i = 0; i < idGetters.Length; i++)
				{
					idGetters[i](obj as T).SignalBeforeRelease();
				}
				for (int j = 0; j < idGetters.Length; j++)
				{
					idGetters[j](obj as T).Release();
				}
				for (int k = 0; k < idSetters.Length; k++)
				{
					idSetters[k](obj as T, null);
				}
			});
		}
		_CachedReleaseIdsMethods[type](idsContainer);
	}
}
