using System;
using System.Collections.Generic;

public class TagMap
{
	private readonly Dictionary<object, Dictionary<Type, object>> _map = new Dictionary<object, Dictionary<Type, object>>();

	static TagMap()
	{
		Pools.CreatePoolDictionary<Type, object>();
	}

	public T AddTag<T>(object owner, T tag)
	{
		object obj2 = ((_map.GetValueOrDefault(owner) ?? (_map[owner] = Pools.Unpool<Dictionary<Type, object>>()))[typeof(T)] = tag);
		return (T)obj2;
	}

	public T AddHashTag<T>(object owner, T tag)
	{
		(GetTag<PoolKeepItemHashSetHandle<T>>(owner) ?? AddTag(owner, Pools.UseKeepItemHashSet<T>())).Add(tag);
		return tag;
	}

	public bool RemoveTag(object owner, Type type)
	{
		Dictionary<Type, object> valueOrDefault = _map.GetValueOrDefault(owner);
		if (valueOrDefault != null && valueOrDefault.ContainsKey(type))
		{
			object obj = valueOrDefault[type];
			if (obj != null && obj is IDisposable)
			{
				Pools.TryRepoolObject(obj);
			}
			if (valueOrDefault.Remove(type) && valueOrDefault.Count == 0 && _map.Remove(owner))
			{
				Pools.Repool(valueOrDefault);
			}
			return true;
		}
		return false;
	}

	public bool RemoveTag<T>(object owner)
	{
		return RemoveTag(owner, typeof(T));
	}

	public void ClearTags(object owner)
	{
		Dictionary<Type, object> valueOrDefault = _map.GetValueOrDefault(owner);
		if (valueOrDefault == null)
		{
			return;
		}
		foreach (Type item in valueOrDefault.EnumerateKeysSafe())
		{
			RemoveTag(owner, item);
		}
	}

	public T GetTag<T>(object owner)
	{
		return (T)(_map.GetValueOrDefault(owner)?.GetValueOrDefault(typeof(T)));
	}

	public bool HasHashTag<T>(object owner, T tag)
	{
		return GetTag<PoolKeepItemHashSetHandle<T>>(owner)?.Contains(tag) ?? false;
	}
}
