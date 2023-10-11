using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class NodeGraphTagMap
{
	[ProtoMember(1)]
	private Dictionary<string, string> _map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	public Dictionary<string, string> map
	{
		get
		{
			return _map ?? (_map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
		}
		set
		{
			_map = value;
		}
	}

	public string this[string key]
	{
		get
		{
			if (!map.ContainsKey(key))
			{
				return null;
			}
			return map[key];
		}
		set
		{
			if (value != null)
			{
				map[key] = value;
			}
			else
			{
				map.Remove(key);
			}
		}
	}

	public bool CheckEqualTo(string key, string value)
	{
		return StringComparer.OrdinalIgnoreCase.Equals(this[key], value);
	}

	public bool CheckEqualTo(NodeDataTag key, NodeDataTag value)
	{
		if ((bool)key && (bool)value)
		{
			return CheckEqualTo(key.tag, value.tag);
		}
		return false;
	}

	public PoolKeepItemHashSetHandle<string> GetMappedTags(HashSet<NodeDataTag> tags)
	{
		PoolKeepItemHashSetHandle<string> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<string>();
		foreach (NodeDataTag tag in tags)
		{
			poolKeepItemHashSetHandle.Add(map.ContainsKey(tag) ? map[tag] : ((string)tag));
		}
		return poolKeepItemHashSetHandle;
	}
}
