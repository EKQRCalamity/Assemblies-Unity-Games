using System.Collections.Generic;
using System.Linq;

public static class NodeGraphExtensions
{
	public static bool IsSubGraph(this NodeData nodeData)
	{
		return nodeData?.isSubGraph ?? false;
	}

	public static NodeGraphFlags GetNodeGraphFlagsSafe(this NodeGraph nodeGraph, NodeDataFlagType flagType)
	{
		return nodeGraph?.GetNodeGraphFlags(flagType);
	}

	public static NodeGraphFlags GetNodeGraphFlagsSafe(this NodeGraph nodeGraph, ref NodeDataFlagType flagType)
	{
		if (nodeGraph == null)
		{
			return null;
		}
		return nodeGraph.GetNodeGraphFlags(flagType) ?? nodeGraph.GetNodeGraphFlags(flagType = ((flagType == NodeDataFlagType.Standard) ? NodeDataFlagType.Persisted : NodeDataFlagType.Standard));
	}

	public static int GetFlagValueSafe(this StringIntFlags flags, string flag)
	{
		return flags?.GetFlagValue(flag) ?? 0;
	}

	public static bool CheckFlagSafe(this StringIntFlags flags, string flag, int value, FlagCheckType checkType)
	{
		return flags?.CheckFlag(flag, value, checkType) ?? false;
	}

	public static void AdjustFlagValueSafe(this StringIntFlags flags, string flag, int value, FlagSetType setType, bool signalChange = true)
	{
		flags?.AdjustFlagValue(flag, value, setType, signalChange);
	}

	public static void AddToFlagValueSafe(this StringIntFlags flags, string flag, int delta, bool signalChange = true)
	{
		flags?.AddToFlagValue(flag, delta, signalChange);
	}

	public static void SetFlagValueSafe(this StringIntFlags flags, string flag, int setValue, bool signalChange = true)
	{
		flags?.SetFlagValue(flag, setValue, signalChange);
	}

	public static NodeDataFlagType? GetFlagTypeSafe(this NodeGraphFlags graphFlags)
	{
		return graphFlags?.flagType;
	}

	public static bool ShouldSerialize(this NodeGraphFlags graphFlags)
	{
		if (graphFlags != null)
		{
			return graphFlags.count > 0;
		}
		return false;
	}

	public static NodeData.Type ToNodeOutput(this bool b)
	{
		if (!b)
		{
			return NodeData.Type.False;
		}
		return NodeData.Type.True;
	}

	public static string[] ToStringArray(this NodeDataTags tags)
	{
		if (tags.IsNullOrEmpty())
		{
			return null;
		}
		return tags.tags.Select((NodeDataTag t) => t.tag).ToArray();
	}

	public static PoolKeepItemDictionaryHandle<uint, int> MinimizeBranchDepths(this PoolKeepItemDictionaryHandle<uint, int> branchDepths, IEnumerable<NodeData> nodesInOrder)
	{
		PoolKeepItemDictionaryHandle<uint, int> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<uint, int>();
		int num = 0;
		using (branchDepths)
		{
			using PoolKeepItemDictionaryHandle<int, int> poolKeepItemDictionaryHandle2 = Pools.UseKeepItemDictionary<int, int>();
			foreach (NodeData item in nodesInOrder)
			{
				if (!branchDepths.ContainsKey(item))
				{
					continue;
				}
				int num2 = branchDepths[item];
				int num3 = num2 - num;
				if (num3 > 1)
				{
					if (!poolKeepItemDictionaryHandle2.ContainsKey(num))
					{
						poolKeepItemDictionaryHandle2.Add(num, 0);
					}
					poolKeepItemDictionaryHandle2[num] += 1 - num3;
				}
				foreach (int item2 in poolKeepItemDictionaryHandle2.value.EnumerateKeysSafe())
				{
					if (num2 <= item2)
					{
						poolKeepItemDictionaryHandle2.Remove(item2);
					}
				}
				int num4 = 0;
				foreach (int value in poolKeepItemDictionaryHandle2.value.Values)
				{
					num4 += value;
				}
				poolKeepItemDictionaryHandle[item] = num2 + num4;
				num = num2;
			}
			return poolKeepItemDictionaryHandle;
		}
	}

	public static bool ShouldSerialize(this NodeDataTags tags)
	{
		if (tags != null)
		{
			return tags.tags.Count > 0;
		}
		return false;
	}

	public static TextBuilder AppendText(this NodeDataTags tags, TextBuilder builder, bool appendSpace = true, bool prependSpace = false, string onEmptyText = null)
	{
		if (tags.ShouldSerialize())
		{
			builder.Space(prependSpace).Append(tags.useMappedTags.ToText("*")).Append("tag[")
				.Append(tags.tags.ToStringSmart())
				.Append("]")
				.Space(appendSpace);
		}
		else if (onEmptyText != null)
		{
			builder.Append(onEmptyText);
		}
		return builder;
	}

	public static TextBuilder AppendSearchText(this NodeDataTags tags, TextBuilder builder)
	{
		if (tags != null)
		{
			foreach (NodeDataTag tag in tags.tags)
			{
				builder.Append(tag).Space();
			}
			return builder;
		}
		return builder;
	}

	public static bool SafeEquals(this NodeDataTags a, NodeDataTags b)
	{
		bool num = a.ShouldSerialize();
		bool flag = b.ShouldSerialize();
		if (!num)
		{
			return !flag;
		}
		if (!flag)
		{
			return false;
		}
		if (a.useMappedTags == b.useMappedTags && a.Count == b.Count)
		{
			return a.tags.ContainsAll(b.tags);
		}
		return false;
	}

	public static bool MatchFoundSafe(this NodeDataTags tags, NodeDataTags otherTags, bool matchIfEmpty = false)
	{
		if (tags == null || otherTags == null)
		{
			return matchIfEmpty;
		}
		return tags.MatchFound(otherTags, matchIfEmpty);
	}

	public static bool IsNullOrEmpty(this NodeDataTags tags)
	{
		if (tags != null)
		{
			return tags.Count == 0;
		}
		return true;
	}

	public static void SetGraphTags(this NodeDataTag tag, NodeGraphTags graphTags)
	{
		if (tag != null)
		{
			tag.graphTags = graphTags;
		}
	}

	public static void SetGraphTags(this NodeDataTagWithEdit tag, NodeGraphTags graphTags)
	{
		if (tag != null)
		{
			tag.graphTags = graphTags;
		}
	}

	public static bool _ShouldSetGraph(this NodeGraph graph)
	{
		if (graph != null)
		{
			return GameState.Instance == null;
		}
		return false;
	}

	public static void SetGraph(this NodeGraph graph, ref NodeDataTags tags)
	{
		if (graph._ShouldSetGraph() || tags != null)
		{
			(tags ?? (tags = new NodeDataTags())).graph = graph;
		}
	}

	public static void SetGraph(this NodeGraph graph, NodeDataTags tags)
	{
		if (tags != null)
		{
			graph.SetGraph(ref tags);
		}
	}

	public static void SetGraphLocal(this NodeGraph graph, NodeDataTag tag)
	{
		if (graph._ShouldSetGraph() || tag != null)
		{
			tag.graphTags = graph.localNodeGraphTags;
		}
	}

	public static void SetGraph(this NodeGraph graph, ref NodeDataFlagWithEdit flag, NodeDataFlagType flagType)
	{
		if (graph._ShouldSetGraph())
		{
			NodeDataFlagWithEdit obj = flag ?? new NodeDataFlagWithEdit();
			NodeDataFlagWithEdit nodeDataFlagWithEdit = obj;
			flag = obj;
			nodeDataFlagWithEdit.nodeGraphFlags = graph.GetNodeGraphFlags(flagType);
		}
	}

	public static void PrepareForSave(this SpecificType specificType)
	{
		specificType?._PrepareForSave();
	}
}
