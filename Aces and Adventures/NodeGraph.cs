using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[ProtoInclude(1000, typeof(NodeGraphRef))]
public class NodeGraph : IDataContent
{
	public struct AutoLayoutSpring
	{
		public NodeData pivotNode;

		public NodeData pushedNode;

		public Vector2 springOffset;

		public SpringType springType;

		public AutoLayoutSpring(NodeData pivotNode, NodeData pushedNode, Vector2 springOffset, SpringType springType = SpringType.Standard)
		{
			this.pivotNode = pivotNode;
			this.pushedNode = pushedNode;
			this.springOffset = springOffset;
			this.springType = springType;
		}
	}

	public class ParentNodeEqualityComparer : IEqualityComparer<NodeData>
	{
		public static readonly ParentNodeEqualityComparer Default = new ParentNodeEqualityComparer();

		public bool Equals(NodeData x, NodeData y)
		{
			if (x == y)
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}
			return x.GetIdPath().SequenceEqual(y.GetIdPath());
		}

		public int GetHashCode(NodeData obj)
		{
			return obj.GetHashCode();
		}
	}

	private static HashSet<Type> NULL_VALID_TYPES = new HashSet<Type>();

	protected static bool DEBUG_WARPS = false;

	private static TextBuilder _Builder;

	[ProtoMember(1, OverwriteList = true)]
	private Dictionary<uint, NodeData> _nodes;

	[ProtoMember(2, OverwriteList = true)]
	private Dictionary<uint, NodeDataConnection> _connections;

	[ProtoMember(3)]
	private string _name;

	[ProtoMember(4)]
	private uint _lastNodeId;

	[ProtoMember(5)]
	private uint _lastConnectionId;

	[ProtoMember(6, OverwriteList = true)]
	private List<uint> _warpEnterIds;

	[ProtoMember(7)]
	private bool _generated;

	[ProtoMember(8)]
	private uint _generatedRootNode;

	[ProtoMember(15)]
	private string _tags;

	private HashSet<uint> _registeredEventIds;

	private System.Random _random;

	private static TextBuilder Builder => _Builder ?? (_Builder = new TextBuilder(clearOnToString: true));

	public string name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	public NodeData parentNode { get; set; }

	public NodeGraph parentGraph
	{
		get
		{
			if (parentNode == null)
			{
				return null;
			}
			return parentNode.graph;
		}
	}

	protected virtual StringIntFlags _flags
	{
		get
		{
			return null;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public StringIntFlags flags
	{
		get
		{
			StringIntFlags stringIntFlags = _flags;
			if (stringIntFlags == null)
			{
				if (parentGraph == null)
				{
					return null;
				}
				stringIntFlags = parentGraph.flags;
			}
			return stringIntFlags;
		}
		set
		{
			_flags = value;
		}
	}

	protected virtual StringIntFlags _localFlags => null;

	public StringIntFlags localFlags
	{
		get
		{
			StringIntFlags stringIntFlags = _localFlags;
			if (stringIntFlags == null)
			{
				if (parentGraph == null)
				{
					return null;
				}
				stringIntFlags = parentGraph.localFlags;
			}
			return stringIntFlags;
		}
	}

	protected virtual StringIntFlags _persistedFlags
	{
		get
		{
			return null;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public StringIntFlags persistedFlags
	{
		get
		{
			StringIntFlags stringIntFlags = _persistedFlags;
			if (stringIntFlags == null)
			{
				if (parentGraph == null)
				{
					return null;
				}
				stringIntFlags = parentGraph.persistedFlags;
			}
			return stringIntFlags;
		}
		set
		{
			_persistedFlags = value;
		}
	}

	protected virtual NodeGraphFlags _nodeGraphFlags => null;

	public NodeGraphFlags nodeGraphFlags
	{
		get
		{
			NodeGraphFlags obj = _nodeGraphFlags;
			if (obj == null)
			{
				if (parentGraph == null)
				{
					return null;
				}
				obj = parentGraph.nodeGraphFlags;
			}
			return obj;
		}
	}

	protected virtual NodeGraphFlags _persistedNodeGraphFlags => null;

	public NodeGraphFlags persistedNodeGraphFlags
	{
		get
		{
			NodeGraphFlags obj = _persistedNodeGraphFlags;
			if (obj == null)
			{
				if (parentGraph == null)
				{
					return null;
				}
				obj = parentGraph.persistedNodeGraphFlags;
			}
			return obj;
		}
	}

	protected virtual NodeGraphTags _nodeGraphTags => null;

	public NodeGraphTags nodeGraphTags
	{
		get
		{
			NodeGraphTags obj = _nodeGraphTags;
			if (obj == null)
			{
				if (parentGraph == null)
				{
					return null;
				}
				obj = parentGraph.nodeGraphTags;
			}
			return obj;
		}
	}

	protected virtual NodeGraphTags _localNodeGraphTags => null;

	public NodeGraphTags localNodeGraphTags
	{
		get
		{
			NodeGraphTags obj = _localNodeGraphTags;
			if (obj == null)
			{
				if (parentGraph == null)
				{
					return null;
				}
				obj = parentGraph.localNodeGraphTags;
			}
			return obj;
		}
	}

	protected virtual NodeGraphTagMap _nodeGraphTagMap => null;

	public NodeGraphTagMap nodeGraphTagMap
	{
		get
		{
			NodeGraphTagMap obj = _nodeGraphTagMap;
			if (obj == null)
			{
				if (parentGraph == null)
				{
					return null;
				}
				obj = parentGraph.nodeGraphTagMap;
			}
			return obj;
		}
	}

	public virtual bool processGlobalIdsOnGenerate => false;

	public System.Random random
	{
		get
		{
			return rootGraph._random ?? (rootGraph._random = new System.Random());
		}
		private set
		{
			rootGraph._random = value;
		}
	}

	protected List<uint> warpEnterIds => _warpEnterIds ?? (_warpEnterIds = new List<uint>());

	protected virtual Dictionary<string, List<uint>> mappedWarpExits => null;

	public HashSet<uint> registeredEventIds => _registeredEventIds ?? (_registeredEventIds = new HashSet<uint>());

	public bool generated
	{
		get
		{
			if (!_generated)
			{
				if (parentGraph != null)
				{
					return parentGraph.generated;
				}
				return false;
			}
			return true;
		}
	}

	public int nodeCount => nodes.Count;

	protected Dictionary<uint, NodeData> nodes => _nodes ?? (_nodes = new Dictionary<uint, NodeData>());

	protected Dictionary<uint, NodeDataConnection> connections => _connections ?? (_connections = new Dictionary<uint, NodeDataConnection>());

	public bool isRootGraph => parentGraph == null;

	public NodeGraph rootGraph
	{
		get
		{
			NodeGraph nodeGraph = this;
			while (nodeGraph.parentGraph != null)
			{
				nodeGraph = nodeGraph.parentGraph;
			}
			return nodeGraph;
		}
	}

	public int depth
	{
		get
		{
			int num = 0;
			NodeGraph nodeGraph = this;
			while (nodeGraph.parentGraph != null)
			{
				nodeGraph = nodeGraph.parentGraph;
				num++;
			}
			return num;
		}
	}

	public virtual HashSet<Type> validNodeDataTypes
	{
		get
		{
			if (parentGraph == null)
			{
				return NULL_VALID_TYPES;
			}
			return parentGraph.validNodeDataTypes;
		}
	}

	public NodeDataRef owningReferenceNode
	{
		get
		{
			object obj = parentNode as NodeDataRef;
			if (obj == null)
			{
				if (parentNode == null)
				{
					return null;
				}
				obj = parentNode.graph.owningReferenceNode;
			}
			return (NodeDataRef)obj;
		}
	}

	public bool isReferencedGraph => owningReferenceNode != null;

	public virtual ContentRef owningContentRef
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string tags
	{
		get
		{
			return _tags;
		}
		set
		{
			_tags = value;
		}
	}

	public event Action<Dictionary<NodeData, PoolKeepItemHashSetHandle<NodeData>>> onProcessExecuted;

	public event Action<NodeGraph, NodeData> onNodeAdded;

	public event Action<NodeGraph, NodeData> onNodeRemoved;

	public event Action<NodeGraph, NodeDataConnection> onConnectionAdded;

	public event Action<NodeGraph, NodeDataConnection> onConnectionRemoved;

	public event Action<NodeGraph, NodeData, Vector2> onNodePositionChanged;

	public event Action<NodeGraph, NodeData, NodeData, byte[]> onNodePastedValues;

	public static void PasteValues(NodeData copiedValue, IEnumerable<NodeData> nodesToPasteOver)
	{
		foreach (NodeData item in nodesToPasteOver)
		{
			PasteValues(copiedValue, item);
		}
	}

	public static void PasteValues(NodeData copyFrom, NodeData nodePastedOver, bool createNodePastedOverData = true)
	{
		if (copyFrom != null && !(copyFrom.GetType() != nodePastedOver.GetType()))
		{
			byte[] nodePastedOverData = (createNodePastedOverData ? ProtoUtil.ToByteArray(nodePastedOver) : null);
			NodeGraph graph = nodePastedOver.graph;
			uint id = nodePastedOver.id;
			Guid globalId = nodePastedOver.globalId;
			Vector2 position = nodePastedOver.position;
			List<uint> connectionIds = nodePastedOver.connectionIds;
			NodeData nodeData2 = (graph.nodes[id] = ProtoUtil.Clone(copyFrom));
			nodePastedOver = nodeData2;
			nodePastedOver.graph = graph;
			nodePastedOver.id = id;
			nodePastedOver.globalId = globalId;
			nodePastedOver.position = position;
			nodePastedOver.connectionIds = connectionIds;
			graph._SignalNodePastedValues(copyFrom, nodePastedOver, nodePastedOverData);
		}
	}

	private void _SignalNodeAdded(NodeData nodeData)
	{
		if (rootGraph.onNodeAdded != null)
		{
			rootGraph.onNodeAdded(this, nodeData);
		}
	}

	private void _SignalNodeRemoved(NodeData nodeData)
	{
		if (rootGraph.onNodeRemoved != null)
		{
			rootGraph.onNodeRemoved(this, nodeData);
		}
	}

	private void _SignalConnectionAdded(NodeDataConnection connection)
	{
		if (rootGraph.onConnectionAdded != null)
		{
			rootGraph.onConnectionAdded(this, connection);
		}
	}

	private void _SignalConnectionRemoved(NodeDataConnection connection)
	{
		if (rootGraph.onConnectionRemoved != null)
		{
			rootGraph.onConnectionRemoved(this, connection);
		}
	}

	private void _SignalNodePositionChanged(NodeData nodeData, Vector2 previousPosition)
	{
		if (rootGraph.onNodePositionChanged != null)
		{
			rootGraph.onNodePositionChanged(this, nodeData, previousPosition);
		}
	}

	private void _SignalNodePastedValues(NodeData copiedValue, NodeData nodePastedOver, byte[] nodePastedOverData)
	{
		if (rootGraph.onNodePastedValues != null)
		{
			rootGraph.onNodePastedValues(this, copiedValue, nodePastedOver, nodePastedOverData);
		}
	}

	private void _RegisterGlobalId(NodeData nodeData)
	{
		if (nodeData.useGlobalId && nodeData.globalId == Guid.Empty)
		{
			nodeData.globalId = Guid.NewGuid();
		}
	}

	public virtual void ValidateNode(NodeData nodeData)
	{
	}

	public NodeData AddNode(NodeData nodeData, uint? forcedId = null)
	{
		if (nodeData.id != 0 && !forcedId.HasValue)
		{
			throw new ArgumentException("Cannot add NodeData which already has registered Id. [" + nodeData?.ToString() + "]");
		}
		nodeData.graph = this;
		nodeData.id = ((forcedId.HasValue && !nodes.ContainsKey(forcedId.Value)) ? forcedId.Value : nodes.GetUniqueId(ref _lastNodeId, 0u));
		nodes.Add(nodeData, nodeData);
		_RegisterGlobalId(nodeData);
		nodeData.ValidateInHierarchy();
		_SignalNodeAdded(nodeData);
		return nodeData;
	}

	public bool HasNode(uint nodeId)
	{
		return nodes.ContainsKey(nodeId);
	}

	public NodeData GetNode(uint nodeId)
	{
		return nodes[nodeId];
	}

	public T GetNode<T>(uint nodeId) where T : NodeData
	{
		return nodes[nodeId] as T;
	}

	public NodeData GetNodeFromIdPath(IEnumerable<uint> idPath)
	{
		NodeGraph nodeGraph = this;
		using PoolKeepItemStackHandle<uint> poolKeepItemStackHandle = Pools.UseKeepItemStack(idPath);
		while (poolKeepItemStackHandle.Count > 0)
		{
			uint nodeId = poolKeepItemStackHandle.Pop();
			if (!nodeGraph.HasNode(nodeId))
			{
				return null;
			}
			NodeData node = nodeGraph.GetNode(nodeId);
			if (poolKeepItemStackHandle.Count == 0)
			{
				return node;
			}
			if (!node.isSubGraph)
			{
				return null;
			}
			nodeGraph = node.subGraph;
		}
		return null;
	}

	public T GetNodeByGlobalId<T>(Guid globalId) where T : NodeData
	{
		foreach (NodeData value in nodes.Values)
		{
			if (value.globalId == globalId)
			{
				return value as T;
			}
		}
		return null;
	}

	public T GetNodeByGlobalIdDeep<T>(Guid globalId, bool stopAtCannotBeFlattened = false, bool stopAtReferenceNode = false) where T : NodeData
	{
		return GetNodesDeep(stopAtCannotBeFlattened, stopAtReferenceNode).FirstOrDefault((NodeData n) => n.globalId == globalId) as T;
	}

	public bool RemoveNode(NodeData nodeData)
	{
		if (!nodes.ContainsKey(nodeData))
		{
			return false;
		}
		nodeData.RemoveAllConnections();
		_SignalNodeRemoved(nodeData);
		return nodes.Remove(nodeData);
	}

	public bool RemoveNodeById(uint id)
	{
		return RemoveNode(GetNode(id));
	}

	public void RemoveInvalidNodeTypes()
	{
		HashSet<Type> hashSet = validNodeDataTypes;
		foreach (NodeData item in nodes.EnumerateValuesSafe())
		{
			if (!hashSet.Contains(item.GetType()))
			{
				item.RemoveNodeAndPatchConnections();
			}
		}
	}

	public T Prepend<T>(T nodeData) where T : NodeData
	{
		if (nodeData.inGraph)
		{
			nodeData.graph.RemoveNode(nodeData);
		}
		using PoolKeepItemListHandle<NodeData> poolKeepItemListHandle = GetRootNodes();
		AddNode(nodeData);
		foreach (NodeData item in poolKeepItemListHandle.value)
		{
			AddConnection(new NodeDataConnection((NodeData)nodeData, item));
		}
		return nodeData;
	}

	public void Prepend(NodeGraph graph)
	{
		Prepend(new NodeDataSubGraph(graph)).Flatten();
	}

	public bool AddConnection(NodeDataConnection connection, uint? forcedId = null)
	{
		if (connection.id != 0 && !forcedId.HasValue)
		{
			throw new ArgumentException("Cannot add NodeDataConnection which already has registered Id. [" + connection?.ToString() + "]");
		}
		connection.graph = this;
		if (!connection.CanBeAdded())
		{
			return false;
		}
		connection.id = ((forcedId.HasValue && !connections.ContainsKey(forcedId.Value)) ? forcedId.Value : connections.GetUniqueId(ref _lastConnectionId, 0u));
		connections.Add(connection, connection);
		connection.AddToNodeConnections();
		_SignalConnectionAdded(connection);
		return true;
	}

	public bool HasConnection(uint connectionId)
	{
		return connections.ContainsKey(connectionId);
	}

	public NodeDataConnection GetConnection(uint connectionId)
	{
		return connections[connectionId];
	}

	public bool RemoveConnection(NodeDataConnection connection)
	{
		if (!connections.ContainsKey(connection))
		{
			return false;
		}
		connection.RemoveFromNodeConnections();
		_SignalConnectionRemoved(connection);
		return connections.Remove(connection);
	}

	public void RouteConnection(NodeDataConnection connection, NodeData nodeToRouteThrough)
	{
		AddConnection(new NodeDataConnection(connection.outputNode, nodeToRouteThrough, connection.outputType));
		AddConnection(new NodeDataConnection(nodeToRouteThrough, connection.inputNode));
		RemoveConnection(connection);
	}

	public PoolKeepItemDictionaryHandle<NodeData, Vector2> SignalConstructionEvents()
	{
		PoolKeepItemDictionaryHandle<NodeData, Vector2> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<NodeData, Vector2>();
		foreach (NodeData value in nodes.Values)
		{
			poolKeepItemDictionaryHandle.Add(value, value.position);
			_SignalNodeAdded(value);
		}
		foreach (NodeDataConnection value2 in connections.Values)
		{
			_SignalConnectionAdded(value2);
		}
		return poolKeepItemDictionaryHandle;
	}

	public void SignalDestructionEvents()
	{
		foreach (NodeDataConnection value in connections.Values)
		{
			_SignalConnectionRemoved(value);
		}
		foreach (NodeData value2 in nodes.Values)
		{
			_SignalNodeRemoved(value2);
		}
	}

	public void SignalProcessExecuted(Dictionary<NodeData, PoolKeepItemHashSetHandle<NodeData>> executedBranches)
	{
		if (this.onProcessExecuted != null)
		{
			this.onProcessExecuted(executedBranches);
		}
	}

	public bool IsValidNode(NodeData nodeData)
	{
		return validNodeDataTypes.Contains(nodeData.GetType());
	}

	public bool AreValidNodes(IEnumerable<NodeData> nodeData)
	{
		HashSet<Type> hashSet = validNodeDataTypes;
		foreach (NodeData nodeDatum in nodeData)
		{
			if (!hashSet.Contains(nodeDatum.GetType()))
			{
				return false;
			}
		}
		return true;
	}

	public void Paste(Dictionary<uint, NodeData> pastedNodes, Dictionary<uint, NodeDataConnection> pastedConnections)
	{
		using PoolKeepItemDictionaryHandle<uint, uint> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<uint, uint>();
		foreach (KeyValuePair<uint, NodeData> pastedNode in pastedNodes)
		{
			NodeData value = pastedNode.Value;
			value.id = 0u;
			value.globalId = Guid.Empty;
			value.ClearConnectionIds();
			AddNode(value);
			poolKeepItemDictionaryHandle.Add(pastedNode.Key, value);
		}
		foreach (KeyValuePair<uint, NodeDataConnection> pastedConnection in pastedConnections)
		{
			NodeDataConnection value2 = pastedConnection.Value;
			value2.outputNodeId = poolKeepItemDictionaryHandle[value2.outputNodeId];
			value2.inputNodeId = poolKeepItemDictionaryHandle[value2.inputNodeId];
			value2.id = 0u;
			AddConnection(value2);
		}
	}

	private bool _ValidOutputNodeByDepth(Dictionary<NodeData, int> nodeDepths, NodeData node, NodeData outputNode)
	{
		if (nodeDepths.ContainsKey(outputNode))
		{
			return !outputNode.IsInputOf(node);
		}
		return true;
	}

	private void _FlattenGraph()
	{
		foreach (NodeData item in nodes.EnumerateValuesSafe())
		{
			if (item.isSubGraph)
			{
				item.Flatten(recursivelyFlatten: true);
			}
		}
	}

	private void _ExecuteRandomNodes()
	{
		using (PoolKeepItemDictionaryHandle<NodeData, int> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<NodeData, int>())
		{
			using PoolKeepItemListHandle<NodeData> poolKeepItemListHandle = GetRootNodes();
			using PoolKeepItemHashSetHandle<NodeData> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<NodeData>();
			int num = 0;
			while (poolKeepItemListHandle.value.Count > 0)
			{
				foreach (NodeData item in poolKeepItemListHandle.value)
				{
					poolKeepItemDictionaryHandle[item] = num;
				}
				foreach (NodeData item2 in poolKeepItemListHandle.value)
				{
					foreach (NodeData outputNode in item2.GetOutputNodes())
					{
						if (_ValidOutputNodeByDepth(poolKeepItemDictionaryHandle, item2, outputNode))
						{
							poolKeepItemHashSetHandle.value.Add(outputNode);
						}
					}
				}
				bool flag = false;
				foreach (NodeData item3 in poolKeepItemListHandle.value)
				{
					if (item3 is NodeDataRandom && (flag = true))
					{
						(item3 as NodeDataRandom).ExecuteRandomNode();
					}
				}
				if (flag)
				{
					foreach (NodeData item4 in poolKeepItemHashSetHandle.value.EnumerateSafe())
					{
						if (!nodes.ContainsKey(item4))
						{
							poolKeepItemHashSetHandle.value.Remove(item4);
						}
					}
				}
				poolKeepItemListHandle.value.Clear();
				foreach (NodeData item5 in poolKeepItemHashSetHandle.value)
				{
					poolKeepItemListHandle.value.Add(item5);
				}
				poolKeepItemHashSetHandle.value.Clear();
				num++;
			}
		}
		foreach (NodeData item6 in nodes.EnumerateValuesSafe())
		{
			if (!item6.isSubGraph || !item6.canProcessRandomNodes)
			{
				continue;
			}
			if (!item6.isRecursivelyNested)
			{
				if (item6 is NodeDataRef)
				{
					item6.subGraph.RemoveInvalidNodeTypes();
				}
				item6.subGraph._ExecuteRandomNodes();
			}
			else
			{
				item6.RemoveNodeAndPatchConnections();
			}
		}
	}

	private void _ProcessGlobalIds()
	{
		PoolKeepItemDictionaryHandle<NodeData, int> nodeInputDepths = GetNodeMaxInputDepths();
		try
		{
			using PoolKeepItemHashSetHandle<Guid> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<Guid>();
			foreach (NodeData item in from n in nodeInputDepths.value.Keys
				where n.useGlobalId
				orderby n.id, nodeInputDepths[n]
				select n)
			{
				while (!poolKeepItemHashSetHandle.Add(item.globalId))
				{
					item.globalId = item.globalId.Increment();
				}
			}
		}
		finally
		{
			if (nodeInputDepths != null)
			{
				((IDisposable)nodeInputDepths).Dispose();
			}
		}
	}

	private void _DoOnPreFlattenNodeLogic()
	{
		foreach (NodeData item in Pools.UseKeepItemList(GetNodesDeep()))
		{
			item.OnPreFlatten();
		}
	}

	private void _DoOnGenerateNodeLogic()
	{
		foreach (NodeData item in Pools.UseKeepItemList(GetNodesDeep()))
		{
			item.OnGenerate();
		}
	}

	public NodeGraph GenerateGraph(int? seed = null)
	{
		if (_generated)
		{
			return this;
		}
		if (seed.HasValue)
		{
			random = new System.Random(seed.Value);
		}
		_ExecuteRandomNodes();
		Prepend(new NodeDataRouter());
		_DoOnPreFlattenNodeLogic();
		_FlattenGraph();
		if (processGlobalIdsOnGenerate)
		{
			_ProcessGlobalIds();
		}
		_DoOnGenerateNodeLogic();
		LinearizeProcessedBranches();
		_generated = true;
		return this;
	}

	public void RemoveNodeDeep(uint nodeId)
	{
		if (!nodes.ContainsKey(nodeId))
		{
			return;
		}
		NodeData nodeData = nodes[nodeId];
		foreach (NodeDataConnection outputConnection in nodeData.GetOutputConnections())
		{
			RemoveConnectionDeep(outputConnection);
		}
		RemoveNode(nodeData);
	}

	public void RemoveConnectionDeep(uint connectionId)
	{
		if (connections.ContainsKey(connectionId))
		{
			NodeDataConnection nodeDataConnection = connections[connectionId];
			NodeData inputNode = nodeDataConnection.inputNode;
			RemoveConnection(nodeDataConnection);
			if (!inputNode.hasInput)
			{
				RemoveNodeDeep(inputNode);
			}
		}
	}

	public void CopyDataFrom(NodeGraph other)
	{
		foreach (NodeData node in GetNodes())
		{
			RemoveNode(node);
		}
		_nodes = ProtoUtil.Clone(other._nodes);
		_connections = ProtoUtil.Clone(other._connections);
		_name = other._name;
		_lastNodeId = other._lastNodeId;
		_lastConnectionId = other._lastConnectionId;
		ProtoAfterDeserialization();
	}

	public void LinearizeProcessedBranches(bool deep = true)
	{
		IEnumerable<NodeData> enumerable;
		if (!deep)
		{
			IEnumerable<NodeData> values = nodes.Values;
			enumerable = values;
		}
		else
		{
			enumerable = GetNodesDeep();
		}
		foreach (NodeData item in enumerable)
		{
			if (!item.shouldCheckForBranchLinearization)
			{
				continue;
			}
			NodeData.Type[] output = item.output;
			foreach (NodeData.Type type in output)
			{
				using PoolKeepItemListHandle<NodeData> poolKeepItemListHandle = item.GetOutputNodes(type);
				if (poolKeepItemListHandle.Count < 2)
				{
					continue;
				}
				foreach (NodeData item2 in poolKeepItemListHandle.value)
				{
					if (!item2.isExecutionStoppingPoint && !item2.HasOutput(NodeData.Type.False))
					{
						continue;
					}
					goto end_IL_005b;
				}
				int num = 1;
				while (true)
				{
					if (num < poolKeepItemListHandle.Count)
					{
						if (!poolKeepItemListHandle[0].SharesAllConnectionsWith(poolKeepItemListHandle[num]))
						{
							break;
						}
						num++;
						continue;
					}
					using (PoolKeepItemListHandle<NodeData> poolKeepItemListHandle2 = poolKeepItemListHandle[0].GetInputNodes(type))
					{
						using PoolKeepItemListHandle<NodeData> poolKeepItemListHandle3 = poolKeepItemListHandle[0].GetOutputNodes(NodeData.Type.True);
						foreach (NodeData item3 in poolKeepItemListHandle.value)
						{
							foreach (NodeDataConnection inputConnection in item3.GetInputConnections(type))
							{
								item.graph.RemoveConnection(inputConnection);
							}
							foreach (NodeDataConnection outputConnection in item3.GetOutputConnections(NodeData.Type.True))
							{
								item.graph.RemoveConnection(outputConnection);
							}
						}
						foreach (NodeData item4 in poolKeepItemListHandle2.value)
						{
							item.graph.AddConnection(new NodeDataConnection(item4, poolKeepItemListHandle[0], type));
						}
						for (int j = 1; j < poolKeepItemListHandle.Count; j++)
						{
							item.graph.AddConnection(new NodeDataConnection(poolKeepItemListHandle[j - 1], poolKeepItemListHandle[j]));
						}
						foreach (NodeData item5 in poolKeepItemListHandle3.value)
						{
							item.graph.AddConnection(new NodeDataConnection(poolKeepItemListHandle.value.Last(), item5));
						}
					}
					break;
				}
				end_IL_005b:;
			}
		}
	}

	public virtual string GetContextTip()
	{
		return null;
	}

	public void AddWarpEnterId(uint id)
	{
		_ = DEBUG_WARPS;
		warpEnterIds.Add(id);
	}

	public void RemoveLastWarpEnterId()
	{
		_ = DEBUG_WARPS;
		warpEnterIds.RemoveLast();
	}

	public NodeData GetLastWarpEnterNode()
	{
		NodeData result = ((warpEnterIds.Count > 0) ? GetNode(warpEnterIds.Last()) : null);
		_ = DEBUG_WARPS;
		return result;
	}

	public void AddWarpExitMap(NodeDataWarpExit warpExit)
	{
		if (mappedWarpExits == null)
		{
			return;
		}
		foreach (NodeDataTag tag in warpExit.identifiers.tags)
		{
			if (!mappedWarpExits.ContainsKey(tag))
			{
				mappedWarpExits.Add(tag, new List<uint>());
			}
			mappedWarpExits[tag].Add(warpExit);
		}
	}

	public NodeData GetMappedWarpExit(HashSet<string> nodeDataTags)
	{
		if (mappedWarpExits == null)
		{
			return null;
		}
		if (nodeDataTags.Count != 1)
		{
			return null;
		}
		string key = nodeDataTags.First();
		if (!mappedWarpExits.ContainsKey(key) || mappedWarpExits[key].Count != 1)
		{
			return null;
		}
		return GetNode(mappedWarpExits[key][0]);
	}

	public bool HasNode<T>() where T : NodeData
	{
		foreach (NodeData value in nodes.Values)
		{
			if (value is T)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasNodeDeep<T>(Func<T, bool> validNode = null) where T : NodeData
	{
		foreach (NodeData item in GetNodesDeep())
		{
			if (item is T && (validNode == null || validNode((T)item)))
			{
				return true;
			}
		}
		return false;
	}

	public int Count<T>() where T : NodeData
	{
		int num = 0;
		foreach (NodeData value in nodes.Values)
		{
			if (value is T)
			{
				num++;
			}
		}
		return num;
	}

	public PoolKeepItemListHandle<NodeData> GetNodes()
	{
		return Pools.UseKeepItemList(nodes.Values.AsEnumerable());
	}

	public IEnumerable<NodeData> GetNodesDeep(bool stopAtCannotBeFlattened = false, bool stopAtReferenceNode = false)
	{
		foreach (NodeData node in nodes.Values)
		{
			yield return node;
			if (!node.ShouldEvaluateSubGraph(stopAtCannotBeFlattened) || (stopAtReferenceNode && node is NodeDataRef))
			{
				continue;
			}
			foreach (NodeData item in node.subGraph.GetNodesDeep(stopAtCannotBeFlattened, stopAtReferenceNode))
			{
				yield return item;
			}
		}
	}

	public IEnumerable<T> SearchNodes<T>(Func<T, bool> validNode, bool fromCannotBeFlattenedRootGraph = false, bool stopAtCannotBeFlattened = false, bool stopAtReferenceNode = false) where T : NodeData
	{
		NodeGraph nodeGraph = this;
		if (fromCannotBeFlattenedRootGraph)
		{
			while ((bool)nodeGraph.parentNode && nodeGraph.parentNode.canBeFlattened)
			{
				nodeGraph = nodeGraph.parentGraph;
			}
		}
		foreach (T item in nodeGraph.GetNodesDeep(stopAtCannotBeFlattened, stopAtReferenceNode).OfType<T>())
		{
			if (validNode(item))
			{
				yield return item;
			}
		}
	}

	public void FreezeGeneratedRootNode()
	{
		_generatedRootNode = GetRootNodes().AsEnumerable().FirstOrDefault();
	}

	public PoolKeepItemListHandle<NodeData> GetRootNodes(bool sort = true)
	{
		if (_generatedRootNode != 0)
		{
			return Pools.UseKeepItemList(nodes[_generatedRootNode]);
		}
		PoolKeepItemListHandle<NodeData> poolKeepItemListHandle = Pools.UseKeepItemList<NodeData>();
		foreach (NodeData value in nodes.Values)
		{
			if (!value.hasInput)
			{
				poolKeepItemListHandle.Add(value);
			}
		}
		if (poolKeepItemListHandle.Count == 0 && nodes.Count > 0)
		{
			NodeData nodeData = nodes.Values.MinBy((NodeData node) => node.position.x);
			foreach (NodeData value2 in nodes.Values)
			{
				if (Mathf.Abs(value2.position.x - nodeData.position.x) <= NodeDataView.Size.x)
				{
					poolKeepItemListHandle.Add(value2);
				}
			}
		}
		if (sort)
		{
			poolKeepItemListHandle.value.Sort(NodeDataYPositionComparer.Default);
		}
		return poolKeepItemListHandle;
	}

	public PoolStructListHandle<Couple<NodeData, NodeData.Type>> GetLeafNodes()
	{
		PoolStructListHandle<Couple<NodeData, NodeData.Type>> poolStructListHandle = Pools.UseStructList<Couple<NodeData, NodeData.Type>>();
		foreach (NodeData value in nodes.Values)
		{
			NodeData.Type[] output = value.output;
			foreach (NodeData.Type type in output)
			{
				if (!value.HasOutput(type))
				{
					poolStructListHandle.Add(new Couple<NodeData, NodeData.Type>(value, type));
				}
			}
		}
		if (poolStructListHandle.Count == 0 && nodes.Count > 0)
		{
			NodeData nodeData = nodes.Values.MaxBy((NodeData node) => node.position.x);
			{
				foreach (NodeData value2 in nodes.Values)
				{
					if (Mathf.Abs(value2.position.x - nodeData.position.x) <= NodeDataView.Size.x)
					{
						NodeData.Type[] output = value2.output;
						foreach (NodeData.Type b in output)
						{
							poolStructListHandle.Add(new Couple<NodeData, NodeData.Type>(value2, b));
						}
					}
				}
				return poolStructListHandle;
			}
		}
		return poolStructListHandle;
	}

	public PoolKeepItemDictionaryHandle<NodeData, int> GetNodeMaxInputDepths()
	{
		PoolKeepItemDictionaryHandle<NodeData, int> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<NodeData, int>();
		using PoolKeepItemListHandle<NodeData> poolKeepItemListHandle = GetRootNodes();
		using PoolKeepItemHashSetHandle<NodeData> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<NodeData>();
		int num = 0;
		while (poolKeepItemListHandle.value.Count > 0)
		{
			foreach (NodeData item in poolKeepItemListHandle.value)
			{
				poolKeepItemDictionaryHandle[item] = num;
			}
			foreach (NodeData item2 in poolKeepItemListHandle.value)
			{
				foreach (NodeData outputNode in item2.GetOutputNodes())
				{
					if (_ValidOutputNodeByDepth(poolKeepItemDictionaryHandle, item2, outputNode))
					{
						poolKeepItemHashSetHandle.value.Add(outputNode);
					}
				}
			}
			poolKeepItemListHandle.value.Clear();
			foreach (NodeData item3 in poolKeepItemHashSetHandle.value)
			{
				poolKeepItemListHandle.value.Add(item3);
			}
			poolKeepItemHashSetHandle.value.Clear();
			num++;
		}
		return poolKeepItemDictionaryHandle;
	}

	public PoolKeepItemHashSetHandle<NodeData> GetNodesDepthFirst()
	{
		NodeGraph nodeGraph = ProtoUtil.Clone(this);
		NodeDataRouter nodeDataRouter = nodeGraph.Prepend(new NodeDataRouter());
		nodeGraph._FlattenGraph();
		return nodeDataRouter.GetOutputNodesDeep();
	}

	public PoolKeepItemDictionaryHandle<uint, int> GetNodeBranchDepths(Func<NodeData, bool> validNode, out PoolKeepItemHashSetHandle<uint> branchNodes)
	{
		PoolKeepItemDictionaryHandle<uint, int> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<uint, int>();
		NodeGraph nodeGraph = ProtoUtil.Clone(this);
		foreach (NodeData item in nodeGraph.nodes.EnumerateValuesSafe())
		{
			if (!validNode(item))
			{
				item.RemoveNodeAndPatchConnections();
			}
		}
		branchNodes = Pools.UseKeepItemHashSet<uint>();
		foreach (NodeData value in nodeGraph.nodes.Values)
		{
			using PoolKeepItemListHandle<NodeData> poolKeepItemListHandle = value.GetOutputNodes();
			if (poolKeepItemListHandle.Count <= 1)
			{
				continue;
			}
			foreach (NodeData item2 in poolKeepItemListHandle.value)
			{
				branchNodes.Add(item2);
			}
		}
		foreach (NodeData value2 in nodeGraph.nodes.Values)
		{
			int num = 0;
			foreach (NodeData item3 in value2.GetInputNodesDeep())
			{
				if (branchNodes.Contains(item3))
				{
					num++;
				}
			}
			poolKeepItemDictionaryHandle[value2] = num;
		}
		return poolKeepItemDictionaryHandle;
	}

	public StringIntFlags GetFlags(NodeDataFlagType type)
	{
		if (type != 0)
		{
			return persistedFlags;
		}
		return flags;
	}

	public NodeGraphFlags GetNodeGraphFlags(NodeDataFlagType type)
	{
		if (type != 0)
		{
			return persistedNodeGraphFlags;
		}
		return nodeGraphFlags;
	}

	public PoolKeepItemListHandle<NodeDataEventWarpExit> GetRegisteredEventNodes(NodeDataFlag flag)
	{
		PoolKeepItemListHandle<NodeDataEventWarpExit> poolKeepItemListHandle = Pools.UseKeepItemList<NodeDataEventWarpExit>();
		foreach (uint registeredEventId in registeredEventIds)
		{
			NodeDataEventWarpExit node = GetNode<NodeDataEventWarpExit>(registeredEventId);
			if (node.flag.flag == flag)
			{
				poolKeepItemListHandle.Add(node);
			}
		}
		return poolKeepItemListHandle;
	}

	public PoolKeepItemListHandle<NodeDataConnection> GetConnections()
	{
		return Pools.UseKeepItemList(connections.Values.AsEnumerable());
	}

	public void SortConnections()
	{
		foreach (NodeData value in nodes.Values)
		{
			value.SortConnections();
		}
	}

	public PoolKeepItemListHandle<NodeData> ProcessBranchBetweenNodes<T>(NodeData startNode, T endNode, Action<string, int> onFlagDelta = null, Action<string, int> onPersistedFlagDelta = null, Func<NodeData, bool> stopBranchingAt = null, bool execute = true, UndoProcessType? undoProcesses = null, bool delayExecute = false, Action<string, string> onTagDelta = null) where T : NodeData
	{
		stopBranchingAt = stopBranchingAt ?? ((Func<NodeData, bool>)((NodeData n) => n is T));
		Func<NodeData, bool> func = ((endNode == null) ? ((Func<NodeData, bool>)((NodeData n) => n.isCurrentlyLeaf)) : ((Func<NodeData, bool>)((NodeData n) => n == endNode)));
		using PoolKeepItemDictionaryHandle<string, int> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary(flags.flags);
		using PoolKeepItemDictionaryHandle<string, int> poolKeepItemDictionaryHandle2 = Pools.UseKeepItemDictionary(persistedFlags.flags);
		using PoolKeepItemDictionaryHandle<string, string> poolKeepItemDictionaryHandle3 = Pools.UseKeepItemDictionary(nodeGraphTagMap.map);
		using PoolKeepItemHashSetHandle<NodeData> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<NodeData>();
		using PoolListHandle<PoolDictionaryValuesHandle<NodeData, PoolKeepItemHashSetHandle<NodeData>>> poolListHandle = Pools.UseList<PoolDictionaryValuesHandle<NodeData, PoolKeepItemHashSetHandle<NodeData>>>();
		bool flag = endNode == null;
		foreach (NodeData item in (startNode != null) ? Pools.UseKeepItemList(startNode) : GetRootNodes())
		{
			Func<NodeData, bool> stopBranchingAt2 = stopBranchingAt;
			Func<NodeData, bool> isValidBranch = func;
			poolListHandle.Add(item.GetProcessedOutputBranches(stopBranchingAt2, startNode == null, doProcesses: true, UndoProcessType.InvalidOnly, flag, isValidBranch, execute ? poolKeepItemHashSetHandle : null, ProcessRecursionType.PreventInfiniteRecursion, delayExecute));
		}
		if (onFlagDelta != null)
		{
			foreach (KeyValuePair<string, int> delta in flags.flags.GetDeltas(poolKeepItemDictionaryHandle))
			{
				if (delta.Value != 0)
				{
					onFlagDelta(delta.Key, delta.Value);
				}
			}
		}
		if (onPersistedFlagDelta != null)
		{
			foreach (KeyValuePair<string, int> delta2 in persistedFlags.flags.GetDeltas(poolKeepItemDictionaryHandle2))
			{
				if (delta2.Value != 0)
				{
					onPersistedFlagDelta(delta2.Key, delta2.Value);
				}
			}
		}
		if (onTagDelta != null)
		{
			foreach (KeyValuePair<string, string> delta3 in nodeGraphTagMap.map.GetDeltas(poolKeepItemDictionaryHandle3))
			{
				onTagDelta(delta3.Key, delta3.Value);
			}
		}
		if (undoProcesses == UndoProcessType.All)
		{
			foreach (PoolDictionaryValuesHandle<NodeData, PoolKeepItemHashSetHandle<NodeData>> item2 in poolListHandle.value.AsEnumerable().Reverse())
			{
				foreach (KeyValuePair<NodeData, PoolKeepItemHashSetHandle<NodeData>> item3 in item2.value.AsEnumerable().Reverse())
				{
					if (flag)
					{
						item3.Key.UndoProcess();
					}
					foreach (NodeData item4 in item3.Value.value.Reverse())
					{
						item4.UndoProcess();
					}
				}
			}
		}
		return delayExecute ? Pools.UseKeepItemList(poolKeepItemHashSetHandle.value) : null;
	}

	public bool IsParentOf(NodeGraph graph)
	{
		NodeData nodeData = graph.parentNode;
		while (nodeData != null)
		{
			if (ParentNodeEqualityComparer.Default.Equals(nodeData = nodeData.graph.parentNode, parentNode))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsChildOf(NodeGraph graph)
	{
		if (graph.IsParentOf(this))
		{
			return graph.GetNodesDeep().Contains((NodeData node) => node.isSubGraph && ParentNodeEqualityComparer.Default.Equals(node, parentNode));
		}
		return false;
	}

	public IEnumerable<NodeGraph> GetPath(int maxPathDepth = 0, Func<NodeGraph, bool> validGraph = null)
	{
		Func<NodeGraph, bool> valid = null;
		if (validGraph != null)
		{
			valid = (NodeGraph graph) => graph != null && validGraph(graph);
		}
		return CollectionUtil.TakeWhile(this, (NodeGraph graph) => graph.parentGraph, includeInitialValue: true, valid).Take((maxPathDepth > 0) ? maxPathDepth : int.MaxValue).Reverse();
	}

	public string GetPathString(int maxPathDepth = 0, bool prependEllipsis = false)
	{
		foreach (NodeGraph item in GetPath(maxPathDepth))
		{
			Builder.Append(item.name).Append("/");
		}
		if (Builder.length > 0)
		{
			Builder.RemoveFromEnd(1);
		}
		if (prependEllipsis && maxPathDepth > 0 && maxPathDepth < depth + 1)
		{
			Builder.Prepend("…");
		}
		return Builder;
	}

	public NodeGraph FindParent(Func<NodeGraph, bool> validParent)
	{
		NodeGraph nodeGraph = this;
		while ((bool)nodeGraph.parentNode && !validParent(nodeGraph))
		{
			nodeGraph = nodeGraph.parentGraph;
		}
		return nodeGraph;
	}

	public T FindParent<T>() where T : NodeGraph
	{
		NodeGraph nodeGraph = this;
		while ((bool)nodeGraph.parentNode && !(nodeGraph is T))
		{
			nodeGraph = nodeGraph.parentGraph;
		}
		return nodeGraph as T;
	}

	public NodeGraph SetParentNode(NodeData parent)
	{
		parentNode = parent;
		foreach (NodeData node in GetNodes())
		{
			node.graph = this;
		}
		return this;
	}

	public void PrepareDataForSave()
	{
		foreach (NodeData value in nodes.Values)
		{
			value.PrepareForSave();
			if (value.isSubGraph && !value.isRecursivelyNested)
			{
				value.subGraph.PrepareDataForSave();
			}
		}
		SortConnections();
	}

	private void _AutoLayout(List<AutoLayoutSpring> springData, Dictionary<NodeData, Vector2> nodeVelocities, float springConstant, float springDampening, float iterationTimeStep)
	{
		foreach (AutoLayoutSpring springDatum in springData)
		{
			Vector2 position = springDatum.pushedNode.position;
			Vector2 velocity = nodeVelocities[springDatum.pushedNode];
			springDatum.pushedNode.position = springDatum.springType.Spring(ref position, ref velocity, springDatum.pivotNode.position, springDatum.pivotNode.position + springDatum.springOffset, springConstant, springDampening, iterationTimeStep);
			nodeVelocities[springDatum.pushedNode] = velocity;
		}
	}

	public void AutoLayout(float horizontalSpacing = 240f, float verticalSpacing = 60f, int verticalSpringIterations = 10, int springIterations = 100, float springConstant = 100f, float springDampening = 10f, float iterationTimeStep = 0.02f)
	{
		if (nodeCount == 0)
		{
			return;
		}
		SortConnections();
		using PoolKeepItemListHandle<NodeData> poolKeepItemListHandle = Pools.UseKeepItemList(nodes.Values.AsEnumerable());
		using PoolStructListHandle<AutoLayoutSpring> poolStructListHandle2 = Pools.UseStructList<AutoLayoutSpring>();
		using PoolKeepItemDictionaryHandle<NodeData, int> poolKeepItemDictionaryHandle3 = GetNodeMaxInputDepths();
		using PoolKeepItemDictionaryHandle<NodeData, Vector2> poolKeepItemDictionaryHandle2 = Pools.UseKeepItemDictionary<NodeData, Vector2>();
		using PoolDictionaryValuesHandle<int, PoolKeepItemListHandle<NodeData>> poolDictionaryValuesHandle = Pools.UseDictionaryValues<int, PoolKeepItemListHandle<NodeData>>();
		using PoolKeepItemDictionaryHandle<NodeData, Vector2> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<NodeData, Vector2>();
		foreach (NodeData item in poolKeepItemListHandle.value)
		{
			poolKeepItemDictionaryHandle.Add(item, item.position);
			poolKeepItemDictionaryHandle2.Add(item, Vector2.zero);
			int key = poolKeepItemDictionaryHandle3[item];
			if (!poolDictionaryValuesHandle.ContainsKey(key))
			{
				poolDictionaryValuesHandle.Add(key, Pools.UseKeepItemList<NodeData>());
			}
			poolDictionaryValuesHandle[key].Add(item);
		}
		foreach (KeyValuePair<int, PoolKeepItemListHandle<NodeData>> item2 in poolDictionaryValuesHandle.value.OrderBy((KeyValuePair<int, PoolKeepItemListHandle<NodeData>> pair) => pair.Key))
		{
			PoolStructListHandle<AutoLayoutSpring> poolStructListHandle = Pools.UseStructList<AutoLayoutSpring>();
			int key2 = item2.Key;
			List<NodeData> value = item2.Value.value;
			PoolKeepItemDictionaryHandle<NodeData, Vector2> parentMaxYMap = Pools.UseKeepItemDictionary<NodeData, Vector2>();
			try
			{
				foreach (NodeData item3 in value)
				{
					using PoolKeepItemListHandle<NodeData> poolKeepItemListHandle2 = item3.GetInputNodes();
					parentMaxYMap.Add(item3, new Vector2(poolKeepItemListHandle2.value.AverageSafe((NodeData n) => n.position.y, item3.position.y), item3.position.y));
				}
				value.Sort((NodeData a, NodeData b) => Vector2XThenYComparer.Descending.Compare(parentMaxYMap[a], parentMaxYMap[b]));
			}
			finally
			{
				if (parentMaxYMap != null)
				{
					((IDisposable)parentMaxYMap).Dispose();
				}
			}
			SpringType springType = ((key2 > 0) ? SpringType.PushOnly : SpringType.Standard);
			for (int i = 0; i < verticalSpringIterations; i++)
			{
				for (int j = 0; j < value.Count - 1; j++)
				{
					poolStructListHandle.Add(new AutoLayoutSpring(value[j], value[j + 1], new Vector2(0f, 0f - verticalSpacing), springType));
					poolStructListHandle.Add(new AutoLayoutSpring(value[j + 1], value[j], new Vector2(0f, verticalSpacing), springType));
				}
			}
			foreach (NodeData item4 in value)
			{
				using PoolKeepItemListHandle<NodeData> poolKeepItemListHandle3 = item4.GetOutputNodes();
				for (int num = poolKeepItemListHandle3.Count - 1; num >= 0; num--)
				{
					if (poolKeepItemDictionaryHandle3[poolKeepItemListHandle3[num]] <= key2)
					{
						poolKeepItemListHandle3.RemoveAt(num);
					}
				}
				if (poolKeepItemListHandle3.Count == 0)
				{
					continue;
				}
				float num2 = (float)(poolKeepItemListHandle3.Count - 1) * 0.5f * verticalSpacing;
				foreach (NodeData item5 in poolKeepItemListHandle3)
				{
					poolStructListHandle.Add(new AutoLayoutSpring(item4, item5, new Vector2(horizontalSpacing * (float)(poolKeepItemDictionaryHandle3[item5] - key2), num2)));
					num2 -= verticalSpacing;
				}
			}
			for (int k = 0; k < springIterations; k++)
			{
				_AutoLayout(poolStructListHandle, poolKeepItemDictionaryHandle2, springConstant, springDampening, iterationTimeStep);
			}
			foreach (AutoLayoutSpring item6 in poolStructListHandle)
			{
				poolStructListHandle2.Add(item6);
			}
		}
		for (int l = 0; l < springIterations; l++)
		{
			_AutoLayout(poolStructListHandle2, poolKeepItemDictionaryHandle2, springConstant, springDampening, iterationTimeStep);
		}
		foreach (NodeData item7 in poolKeepItemListHandle.value)
		{
			_SignalNodePositionChanged(item7, poolKeepItemDictionaryHandle[item7]);
		}
	}

	public string GetTitle()
	{
		return name;
	}

	public string GetAutomatedDescription()
	{
		return null;
	}

	public List<string> GetAutomatedTags()
	{
		return null;
	}

	public Texture2D GetPreviewImage()
	{
		return null;
	}

	public string GetSaveErrorMessage()
	{
		return null;
	}

	public void OnLoadValidation()
	{
	}

	public override string ToString()
	{
		return name;
	}

	[ProtoAfterDeserialization]
	private void ProtoAfterDeserialization()
	{
		foreach (NodeData value in nodes.Values)
		{
			value.SetGraphOnDeserialize(this);
		}
		foreach (NodeDataConnection value2 in connections.Values)
		{
			value2.graph = this;
		}
	}
}
