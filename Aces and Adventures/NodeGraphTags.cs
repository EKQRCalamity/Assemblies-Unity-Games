using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField]
public class NodeGraphTags
{
	[ProtoMember(1)]
	[UIField(maxCount = 1000, collapse = UICollapseType.Open)]
	[UIFieldCollectionItem(max = 32, defaultValue = "New Tag", readOnly = true)]
	private HashSet<string> _tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	public HashSet<string> tags => _tags ?? (_tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase));

	public Dictionary<string, string> GetUIListData(HashSet<NodeDataTag> exclude, NodeDataTag forceInclude)
	{
		PoolKeepItemHashSetHandle<string> hash = Pools.UseKeepItemHashSet<string>();
		try
		{
			if (exclude != null)
			{
				foreach (NodeDataTag item in exclude)
				{
					hash.Add(item);
				}
			}
			if (forceInclude != null)
			{
				hash.Remove(forceInclude);
			}
			return (from t in tags
				where !hash.Contains(t)
				select t into s
				orderby s
				select s).ToDictionary((string p) => p, (string p) => p, StringComparer.OrdinalIgnoreCase);
		}
		finally
		{
			if (hash != null)
			{
				((IDisposable)hash).Dispose();
			}
		}
	}
}
