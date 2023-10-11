using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField("Referenced Graph", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
public class NodeGraphRef : NodeGraph
{
	[ProtoMember(1)]
	private NodeGraphFlags _flagMediaData;

	[ProtoMember(2)]
	private NodeGraphFlags _persistedFlagMediaData;

	private NodeGraphFlags flagMediaData => _flagMediaData ?? (_flagMediaData = new NodeGraphFlags(NodeDataFlagType.Standard));

	private NodeGraphFlags persistedFlagMediaData => _persistedFlagMediaData ?? (_persistedFlagMediaData = new NodeGraphFlags(NodeDataFlagType.Persisted));

	private bool _flagMediaDataSpecified => _flagMediaData.ShouldSerialize();

	private bool _persistedFlagMediaDataSpecified => _persistedFlagMediaData.ShouldSerialize();

	private NodeGraphRef()
	{
	}

	public NodeGraphRef(NodeGraph graph)
	{
		CopyDataFrom(graph);
	}

	private void _RestoreFlagMedia(NodeGraphFlags mediaData, NodeGraphFlags graphFlags)
	{
		graphFlags?.CopyMediaFrom(mediaData);
	}

	public void SaveFlagMedia(NodeGraph ownerGraph)
	{
		HashSet<Couple<string, NodeDataFlagType>> hashSet = new HashSet<Couple<string, NodeDataFlagType>>();
		foreach (NodeData item in GetNodesDeep(stopAtCannotBeFlattened: false, stopAtReferenceNode: true))
		{
			foreach (Couple<string, NodeDataFlagType> usedFlag in item.usedFlags)
			{
				if (hashSet.Add(usedFlag))
				{
					NodeGraphFlags nodeGraphFlags = ownerGraph.GetNodeGraphFlags(usedFlag);
					if (nodeGraphFlags == null)
					{
						return;
					}
					NodeGraphFlags nodeGraphFlags2 = (((NodeDataFlagType)usedFlag == NodeDataFlagType.Standard) ? flagMediaData : persistedFlagMediaData);
					NodeGraphFlags.FlagMedia media = nodeGraphFlags.GetMedia(usedFlag);
					if (!media.isHidden)
					{
						nodeGraphFlags2.SetMedia(usedFlag, ProtoUtil.Clone(media));
					}
				}
			}
		}
	}

	public void RestoreFlagMedia(NodeGraph ownerGraph)
	{
		_RestoreFlagMedia(flagMediaData, ownerGraph.nodeGraphFlags);
		_RestoreFlagMedia(persistedFlagMediaData, ownerGraph.persistedNodeGraphFlags);
	}
}
