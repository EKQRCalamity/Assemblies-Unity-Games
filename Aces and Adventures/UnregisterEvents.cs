using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class UnregisterEvents
{
	private static class UnregisterEventsMethodCache<T>
	{
		public static readonly Action<T> UnregisterEventsMethod;

		static UnregisterEventsMethodCache()
		{
			Type typeFromHandle = typeof(T);
			HashSet<string> eventNameHash = (from eInfo in typeFromHandle.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
				select eInfo.Name).ToHash();
			IEnumerable<FieldInfo> source = from f in typeFromHandle.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
				where f.GetUnderlyingType().IsSubclassOf(typeof(Delegate)) && ((f.IsPublic && !f.IsInitOnly) || eventNameHash.Contains(f.Name))
				select f;
			Action<T, Delegate>[] fieldSetters = source.Select((FieldInfo info) => info.GetValueSetter<T, Delegate>()).ToArray();
			UnregisterEventsMethod = delegate(T obj)
			{
				for (int i = 0; i < fieldSetters.Length; i++)
				{
					fieldSetters[i](obj, null);
				}
			};
		}
	}

	public static void Unregister<T>(T obj)
	{
		UnregisterEventsMethodCache<T>.UnregisterEventsMethod(obj);
	}
}
