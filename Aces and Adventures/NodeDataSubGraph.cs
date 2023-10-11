using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField("Sub Graph", 0u, null, null, null, null, null, null, false, null, 5, false, null, order = 2000u)]
[YouTubeVideo("Sub Graph Node Tutorial", "_Jtft637DlU", -1, -1)]
public sealed class NodeDataSubGraph : ANodeDataSubGraph
{
	[ProtoMember(1)]
	private NodeGraph _subGraph;

	public override NodeDataIconType iconType => NodeDataIconType.SubGraph;

	public override NodeGraph subGraph
	{
		get
		{
			NodeGraph nodeGraph = _subGraph;
			if (nodeGraph == null)
			{
				NodeGraph obj = new NodeGraph
				{
					name = name,
					parentNode = this
				};
				NodeGraph nodeGraph2 = obj;
				_subGraph = obj;
				nodeGraph = nodeGraph2;
			}
			return nodeGraph;
		}
	}

	public NodeDataSubGraph()
	{
	}

	public NodeDataSubGraph(NodeGraph subGraph)
	{
		_subGraph = subGraph;
	}

	public NodeDataSubGraph(NodeGraph owningGraph, IEnumerable<NodeData> nodesToPutIntoSubGraph)
	{
		using PoolKeepItemHashSetHandle<NodeData> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet(nodesToPutIntoSubGraph);
		using PoolStructHashSetHandle<uint> poolStructHashSetHandle = Pools.UseStructHashSet<uint>();
		using PoolKeepItemHashSetHandle<NodeDataConnection> poolKeepItemHashSetHandle2 = Pools.UseKeepItemHashSet<NodeDataConnection>();
		using PoolStructHashSetHandle<Couple<uint, Type>> poolStructHashSetHandle2 = Pools.UseStructHashSet<Couple<uint, Type>>();
		using PoolStructHashSetHandle<Couple<uint, Type>> poolStructHashSetHandle3 = Pools.UseStructHashSet<Couple<uint, Type>>();
		name = ((poolKeepItemHashSetHandle.Count > 0) ? poolKeepItemHashSetHandle.value.First().name : "Created Sub Graph");
		base.position = poolKeepItemHashSetHandle.value.Select((NodeData node) => node.position).Centroid();
		foreach (NodeData item in poolKeepItemHashSetHandle.value)
		{
			poolStructHashSetHandle.Add(item);
		}
		foreach (NodeDataConnection item2 in poolKeepItemHashSetHandle.value.SelectMany((NodeData node) => node.GetConnectionsEnumerable()).Distinct())
		{
			bool num = !poolStructHashSetHandle.Contains(item2.outputNodeId);
			if (num)
			{
				poolStructHashSetHandle2.Add(new Couple<uint, Type>(item2.outputNodeId, item2.outputType));
			}
			bool flag = !poolStructHashSetHandle.Contains(item2.inputNodeId);
			if (flag)
			{
				poolStructHashSetHandle3.Add(new Couple<uint, Type>(item2.inputNodeId, item2.outputType));
			}
			if (num || flag)
			{
				item2.graph.RemoveConnection(item2);
			}
			else
			{
				poolKeepItemHashSetHandle2.Add(item2);
			}
		}
		foreach (NodeData item3 in poolKeepItemHashSetHandle.value)
		{
			item3.graph.RemoveNode(item3);
			item3.position -= base.position;
			subGraph.AddNode(item3, item3.id);
		}
		foreach (NodeDataConnection item4 in poolKeepItemHashSetHandle2.value)
		{
			subGraph.AddConnection(item4, item4.id);
		}
		owningGraph.AddNode(this);
		foreach (Couple<uint, Type> item5 in poolStructHashSetHandle2.value)
		{
			owningGraph.AddConnection(new NodeDataConnection(item5, this, item5));
		}
		foreach (Couple<uint, Type> item6 in poolStructHashSetHandle3.value)
		{
			owningGraph.AddConnection(new NodeDataConnection(this, item6));
		}
	}
}
