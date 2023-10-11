using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Node", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
[UIDeepValidate]
[ProtoInclude(1000, typeof(ANodeDataSubGraph))]
[ProtoInclude(1001, typeof(NodeDataRandom))]
[ProtoInclude(1002, typeof(ANodeDataFlagSet))]
[ProtoInclude(1003, typeof(ANodeDataFlagCheck))]
[ProtoInclude(1004, typeof(NodeDataRouter))]
[ProtoInclude(1005, typeof(NodeDataCounter))]
[ProtoInclude(1006, typeof(ANodeDataWarp))]
[ProtoInclude(1007, typeof(NodeDataSetTagMap))]
[ProtoInclude(1008, typeof(NodeDataComment))]
public abstract class NodeData : IComparable<NodeData>
{
	public enum Type : byte
	{
		True,
		False
	}

	public enum RandomizationType : byte
	{
		OneTime,
		Dynamic,
		DynamicEvenWhenNotExecuted
	}

	public const uint NULL_ID = 0u;

	private static readonly Type[] DEFAULT_OUTPUT = new Type[1];

	protected static readonly Type[] BOOL_OUTPUT = new Type[2]
	{
		Type.True,
		Type.False
	};

	protected const string PATH_ADVANCED = "Advanced/";

	protected const string PATH_FLAGS = "Advanced/Flags/";

	protected const string PATH_AUDIO = "Advanced/Audio/";

	protected const string PATH_WARP = "Advanced/Warp/";

	protected const string PATH_COMBAT = "Advanced/Combat/";

	protected const string CAT_DATA = "Data";

	protected const string CAT_ADVANCED = "Advanced";

	protected const string CAT_DEV = "Developer Only";

	protected const int MAX_NAME_LENGTH = 30;

	private static TextBuilder _Builder;

	[ProtoMember(2)]
	private string _name;

	[ProtoMember(4, OverwriteList = true)]
	private List<uint> _connectionIds;

	[ProtoMember(5)]
	private Type? _completionType;

	protected NodeGraph _graph;

	private static TextBuilder Builder => _Builder ?? (_Builder = new TextBuilder(clearOnToString: true));

	protected static Func<NodeData, bool> _ProcessStopBranchingAt { get; private set; }

	protected static Func<NodeData, bool> _ProcessIsValidBranch { get; private set; }

	[ProtoMember(1)]
	public uint id { get; set; }

	[UIField(order = 1u, dynamicInitMethod = "_InitName")]
	[UIHideIf("_hideNameInInspector")]
	[UICategory("Data")]
	public virtual string name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
			if (isSubGraph)
			{
				subGraph.name = value;
			}
		}
	}

	[ProtoMember(3)]
	public Vector2 position { get; set; }

	public virtual NodeGraph graph
	{
		get
		{
			return _graph;
		}
		set
		{
			_graph = value;
			if (!isSubGraph || isRecursivelyNested)
			{
				return;
			}
			subGraph.parentNode = this;
			foreach (NodeData node in subGraph.GetNodes())
			{
				node.graph = subGraph;
			}
		}
	}

	public virtual string searchName => name;

	public List<uint> connectionIds
	{
		get
		{
			return _connectionIds ?? (_connectionIds = new List<uint>());
		}
		set
		{
			_connectionIds = value;
		}
	}

	public int numberOfInputs
	{
		get
		{
			using PoolKeepItemListHandle<NodeDataConnection> poolKeepItemListHandle = GetInputConnections();
			return poolKeepItemListHandle.Count;
		}
	}

	public int numberOfOutputs
	{
		get
		{
			using PoolKeepItemListHandle<NodeDataConnection> poolKeepItemListHandle = GetOutputConnections();
			return poolKeepItemListHandle.Count;
		}
	}

	public bool hasInput
	{
		get
		{
			foreach (uint connectionId in connectionIds)
			{
				if (graph.GetConnection(connectionId).inputNodeId == id)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool hasOutput
	{
		get
		{
			foreach (uint connectionId in connectionIds)
			{
				if (graph.GetConnection(connectionId).outputNodeId == id)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool inGraph
	{
		get
		{
			if (graph != null)
			{
				return graph.HasNode(id);
			}
			return false;
		}
	}

	public System.Random random => graph.random;

	public virtual bool useGlobalId => false;

	public virtual Guid globalId
	{
		get
		{
			return Guid.Empty;
		}
		set
		{
		}
	}

	public bool generated => graph.generated;

	public virtual IEnumerable<Couple<string, NodeDataFlagType>> usedFlags
	{
		get
		{
			yield break;
		}
	}

	public bool beingEdited => NodeGraphView.GetView(graph);

	public virtual Type[] output => DEFAULT_OUTPUT;

	public virtual NodeDataSpriteType spriteType => NodeDataSpriteType.Default;

	public virtual NodeDataIconType iconType => NodeDataIconType.SubGraph;

	public virtual bool isHidden
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public virtual bool isExecutionStoppingPoint => false;

	public virtual bool shouldCheckForBranchLinearization => true;

	public virtual string contextPath => "";

	public virtual string contextCategory => null;

	public virtual HotKey? contextHotKey => null;

	public virtual bool hideFromContext => false;

	protected virtual bool _hideNameInInspector => false;

	public virtual UICategorySet[] uiCategorySets => UICategorySet.Defaults;

	public virtual Type activeOutputType => _completionType.GetValueOrDefault();

	public Type? completionType
	{
		get
		{
			return _completionType;
		}
		set
		{
			_completionType = value;
		}
	}

	public virtual NodeGraph subGraph => null;

	public virtual bool isSubGraph => subGraph != null;

	public virtual bool isRecursivelyNested => false;

	public virtual bool canBeFlattened => isSubGraph;

	public virtual bool canFlattenChildren => false;

	public virtual bool canProcessRandomNodes => canBeFlattened;

	public virtual RandomizationType? randomization => null;

	protected virtual int? _maxNameLength => null;

	public bool isCurrentlyLeaf => !HasOutput(activeOutputType);

	protected virtual bool _nameSpecified => _name != null;

	private PoolKeepItemListHandle<NodeDataConnection> _GetConnections(bool includeInput = true, bool includeOutput = true, Type? outputType = null)
	{
		PoolKeepItemListHandle<NodeDataConnection> poolKeepItemListHandle = Pools.UseKeepItemList<NodeDataConnection>();
		foreach (uint connectionId in connectionIds)
		{
			if (graph.HasConnection(connectionId))
			{
				NodeDataConnection connection = graph.GetConnection(connectionId);
				if ((!outputType.HasValue || connection.outputType == outputType) && ((includeInput && connection.inputNodeId == (uint)this) || (includeOutput && connection.outputNodeId == (uint)this)))
				{
					poolKeepItemListHandle.Add(connection);
				}
			}
		}
		return poolKeepItemListHandle;
	}

	protected void _OnOutputTypeChange()
	{
		NodeGraphView view = NodeGraphView.GetView(graph);
		view.GetNode(this).selectable.SetSelected(isSelected: false);
		view.undoHistory.SuppressEntries(this);
		using (PoolKeepItemListHandle<NodeDataConnection> poolKeepItemListHandle = GetConnections())
		{
			graph.RemoveNode(this);
			graph.AddNode(this, id);
			foreach (NodeDataConnection item in poolKeepItemListHandle.value)
			{
				if (item.inputNodeId == id || output.Contains(item.outputType))
				{
					graph.AddConnection(item, item.id);
				}
			}
		}
		view.undoHistory.AllowEntries(this);
		view.GetNode(this).selectable.SetSelected(isSelected: true);
	}

	public virtual NodeData OnOpenSubGraphView(NodeData previousParentNode)
	{
		NodeDataRef owningReferenceNode = graph.owningReferenceNode;
		if (owningReferenceNode == null || (previousParentNode != null && NodeGraph.ParentNodeEqualityComparer.Default.Equals(previousParentNode.graph.owningReferenceNode, owningReferenceNode)))
		{
			return this;
		}
		owningReferenceNode.OnOpenSubGraphView(previousParentNode);
		return owningReferenceNode.subGraph.GetNodeFromIdPath(GetIdPathRelativeToOwningReference()) ?? this;
	}

	public virtual void OnCloseSubGraphView(NodeData nextParentNode)
	{
		NodeDataRef owningReferenceNode = graph.owningReferenceNode;
		if (owningReferenceNode != null && (nextParentNode == null || !NodeGraph.ParentNodeEqualityComparer.Default.Equals(nextParentNode.graph.owningReferenceNode, owningReferenceNode)))
		{
			owningReferenceNode.OnCloseSubGraphView(nextParentNode);
		}
	}

	public virtual void SetGraphOnDeserialize(NodeGraph graphToSet)
	{
		graph = graphToSet;
	}

	public virtual void PrepareForSave()
	{
	}

	public virtual void OnPreFlatten()
	{
	}

	public virtual void OnGenerate()
	{
	}

	public virtual void Preprocess()
	{
	}

	public virtual void UndoPreprocess()
	{
	}

	public virtual void Process()
	{
	}

	public virtual void UndoProcess()
	{
	}

	public virtual void Execute()
	{
	}

	public virtual bool HasScreenPlayText(ScreenplayBuilder screenplayBuilder)
	{
		return false;
	}

	public virtual ScreenplayBuilder AppendScreenPlayText(ScreenplayBuilder screenplayBuilder)
	{
		return screenplayBuilder;
	}

	public virtual HtmlBuilder AppendScreenPlayName(HtmlBuilder htmlBuilder, bool underline = true, bool bold = true)
	{
		if (bold)
		{
			htmlBuilder.Bold();
		}
		if (underline)
		{
			htmlBuilder.Underline();
		}
		htmlBuilder.Append(name.ToHtmlText());
		if (underline)
		{
			htmlBuilder.EndUnderline();
		}
		if (bold)
		{
			htmlBuilder.EndBold();
		}
		htmlBuilder.Small().Append(" (").Append(GetType().GetUILabel())
			.Append(")")
			.EndSmall()
			.Append(": ");
		return htmlBuilder;
	}

	protected virtual void _GetSearchText(TextBuilder builder)
	{
	}

	public NodeData SetData(string name, Vector2 position)
	{
		this.name = name;
		this.position = position;
		return this;
	}

	public bool AddConnection(NodeDataConnection connection)
	{
		return connectionIds.AddUnique(connection);
	}

	public bool RemoveConnection(NodeDataConnection connection)
	{
		return connectionIds.Remove(connection);
	}

	public void RemoveAllConnections()
	{
		foreach (NodeDataConnection connection in GetConnections())
		{
			graph.RemoveConnection(connection);
		}
	}

	public void RemoveOutputConnections(Type? type = null, bool deepRemove = false)
	{
		if (!deepRemove)
		{
			foreach (NodeDataConnection outputConnection in GetOutputConnections(type))
			{
				graph.RemoveConnection(outputConnection);
			}
			return;
		}
		foreach (NodeDataConnection outputConnection2 in GetOutputConnections(type))
		{
			graph.RemoveConnectionDeep(outputConnection2);
		}
	}

	public void RemoveOutputConnectionTo(uint nodeId, Type? type = null)
	{
		foreach (NodeDataConnection outputConnection in GetOutputConnections(type))
		{
			if (outputConnection.inputNodeId == nodeId)
			{
				graph.RemoveConnection(outputConnection);
			}
		}
	}

	public void ClearConnectionIds()
	{
		connectionIds.Clear();
	}

	public void SortConnections()
	{
		using PoolKeepItemListHandle<NodeDataConnection> poolKeepItemListHandle = GetConnections();
		poolKeepItemListHandle.value.Sort();
		connectionIds.Clear();
		foreach (NodeDataConnection item in poolKeepItemListHandle.value)
		{
			connectionIds.Add(item);
		}
	}

	public PoolKeepItemListHandle<NodeDataConnection> GetConnections()
	{
		return _GetConnections();
	}

	public IEnumerable<NodeDataConnection> GetConnectionsEnumerable()
	{
		foreach (NodeDataConnection item in _GetConnections())
		{
			yield return item;
		}
	}

	public PoolKeepItemListHandle<NodeDataConnection> GetInputConnections(Type? outputType = null)
	{
		return _GetConnections(includeInput: true, includeOutput: false, outputType);
	}

	public PoolKeepItemListHandle<NodeDataConnection> GetOutputConnections(Type? outputType = null)
	{
		return _GetConnections(includeInput: false, includeOutput: true, outputType);
	}

	public NodeDataConnection GetConnectionTo(NodeData node, IOType io, Type outputType)
	{
		foreach (NodeDataConnection item in _GetConnections(io == IOType.Input, io == IOType.Output, outputType))
		{
			if (item.outputNodeId == (uint)node || item.inputNodeId == (uint)node)
			{
				return item;
			}
		}
		return null;
	}

	public bool HasOutput(Type outputType, bool checkIfConnectionExists = false)
	{
		foreach (uint connectionId in connectionIds)
		{
			if (!checkIfConnectionExists || graph.HasConnection(connectionId))
			{
				NodeDataConnection connection = graph.GetConnection(connectionId);
				if (connection.outputNodeId == id && connection.outputType == outputType)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool ConnectedTo(uint nodeDataId, bool includeInput = true, bool includeOutput = true, Type? outputType = null)
	{
		foreach (NodeDataConnection item in _GetConnections(includeInput, includeOutput, outputType))
		{
			if (item.Contains(nodeDataId))
			{
				return true;
			}
		}
		return false;
	}

	public void CopyOutputConnectionsFromTypeToType(Type copyFrom, Type copyTo)
	{
		if (copyFrom == copyTo || !output.Contains(copyTo))
		{
			return;
		}
		foreach (NodeDataConnection outputConnection in GetOutputConnections(copyFrom))
		{
			graph.AddConnection(new NodeDataConnection(outputConnection.outputNodeId, outputConnection.inputNodeId, copyTo));
		}
	}

	public NodeData CopyConnectionsFrom(NodeData copyFrom, bool copyInput, bool copyOutput)
	{
		if (copyInput)
		{
			foreach (NodeDataConnection inputConnection in copyFrom.GetInputConnections())
			{
				graph.AddConnection(new NodeDataConnection(inputConnection.outputNode, this, inputConnection.outputType));
			}
		}
		if (copyOutput)
		{
			foreach (NodeDataConnection outputConnection in copyFrom.GetOutputConnections())
			{
				if (output.Contains(outputConnection.outputType))
				{
					graph.AddConnection(new NodeDataConnection(this, outputConnection.inputNodeId, outputConnection.outputType));
				}
			}
			return this;
		}
		return this;
	}

	public bool SharesAllConnectionsWith(NodeData otherNode, IOType? io = null, Type? outputType = null)
	{
		using PoolKeepItemListHandle<NodeDataConnection> poolKeepItemListHandle = _GetConnections(io != IOType.Output, io != IOType.Input, outputType);
		using PoolKeepItemListHandle<NodeDataConnection> poolKeepItemListHandle2 = otherNode._GetConnections(io != IOType.Output, io != IOType.Input, outputType);
		if (poolKeepItemListHandle.Count != poolKeepItemListHandle2.Count)
		{
			return false;
		}
		foreach (NodeDataConnection item in poolKeepItemListHandle.value)
		{
			bool flag = false;
			foreach (NodeDataConnection item2 in poolKeepItemListHandle2.value)
			{
				if (item.outputType == item2.outputType && ((item.inputNodeId == item2.inputNodeId && item.inputNodeId != (uint)this && item.inputNodeId != (uint)otherNode) || (item.outputNodeId == item2.outputNodeId && item.outputNodeId != (uint)this && item.outputNodeId != (uint)otherNode)))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public bool HasInput(bool checkIfConnectionExists = false)
	{
		foreach (uint connectionId in connectionIds)
		{
			if ((!checkIfConnectionExists || graph.HasConnection(connectionId)) && graph.GetConnection(connectionId).inputNodeId == id)
			{
				return true;
			}
		}
		return false;
	}

	public PoolKeepItemListHandle<NodeData> GetInputNodes(Type? outputType = null)
	{
		PoolKeepItemListHandle<NodeData> poolKeepItemListHandle = Pools.UseKeepItemList<NodeData>();
		foreach (NodeDataConnection inputConnection in GetInputConnections(outputType))
		{
			poolKeepItemListHandle.Add(inputConnection.outputNode);
		}
		return poolKeepItemListHandle;
	}

	public PoolKeepItemHashSetHandle<NodeData> GetInputNodesDeep(Dictionary<NodeData, int> nodeDepths = null, PoolKeepItemHashSetHandle<NodeData> inputNodesDeep = null)
	{
		bool flag = inputNodesDeep == null;
		inputNodesDeep = inputNodesDeep ?? Pools.UseKeepItemHashSet<NodeData>();
		foreach (NodeData inputNode in GetInputNodes())
		{
			if (inputNodesDeep.Add(inputNode) && (flag || nodeDepths == null || nodeDepths[inputNode] < nodeDepths[this]))
			{
				inputNode.GetInputNodesDeep(nodeDepths, inputNodesDeep);
			}
		}
		return inputNodesDeep;
	}

	public IEnumerable<Couple<NodeData, int>> GetInputNodesByDepth(bool includeSelf = false)
	{
		using PoolKeepItemListHandle<NodeData> nodesAtDepth = GetInputNodes();
		using PoolKeepItemHashSetHandle<NodeData> nodesAtPreviousDepth = Pools.UseKeepItemHashSet<NodeData>();
		using PoolKeepItemHashSetHandle<NodeData> alreadyReturned = Pools.UseKeepItemHashSet<NodeData>();
		if (includeSelf && alreadyReturned.Add(this))
		{
			yield return new Couple<NodeData, int>(this, 0);
		}
		int depth = -1;
		while (nodesAtDepth.value.Count > 0)
		{
			foreach (NodeData item in nodesAtDepth.value)
			{
				if (alreadyReturned.Add(item))
				{
					yield return new Couple<NodeData, int>(item, depth);
				}
			}
			foreach (NodeData item2 in nodesAtDepth.value)
			{
				foreach (NodeData inputNode in item2.GetInputNodes())
				{
					if (!alreadyReturned.Contains(inputNode))
					{
						nodesAtPreviousDepth.value.Add(inputNode);
					}
				}
			}
			nodesAtDepth.value.Clear();
			foreach (NodeData item3 in nodesAtPreviousDepth.value)
			{
				nodesAtDepth.value.Add(item3);
			}
			nodesAtPreviousDepth.value.Clear();
			int num = depth - 1;
			depth = num;
		}
	}

	public bool IsInputOf(NodeData other)
	{
		using PoolKeepItemHashSetHandle<NodeData> poolKeepItemHashSetHandle = other.GetInputNodesDeep();
		return poolKeepItemHashSetHandle.Contains(this);
	}

	private PoolKeepItemListHandle<PoolKeepItemListHandle<NodeData>> _GetInputBranches(PoolKeepItemListHandle<PoolKeepItemListHandle<NodeData>> branches = null, PoolKeepItemHashSetHandle<uint> checkedNodes = null)
	{
		branches = branches ?? Pools.UseKeepItemList<PoolKeepItemListHandle<NodeData>>().Add(Pools.UseKeepItemList(this));
		checkedNodes = checkedNodes ?? Pools.UseKeepItemHashSet<uint>();
		if (checkedNodes.Add(this))
		{
			using (PoolStructHashSetHandle<UInt2> poolStructHashSetHandle = Pools.UseStructHashSet<UInt2>())
			{
				for (int num = branches.Count - 1; num >= 0; num--)
				{
					foreach (NodeData inputNode in GetInputNodes())
					{
						if (branches[num].value.LastRef() == this && poolStructHashSetHandle.Add(new UInt2(this, inputNode)))
						{
							branches.Add(Pools.UseKeepItemList(branches[num].value).Add(inputNode));
							inputNode._GetInputBranches(branches, checkedNodes);
						}
					}
				}
				return branches;
			}
		}
		return branches;
	}

	public PoolKeepItemListHandle<PoolKeepItemListHandle<NodeData>> GetInputBranches()
	{
		using PoolKeepItemHashSetHandle<uint> checkedNodes = Pools.UseKeepItemHashSet<uint>();
		PoolKeepItemListHandle<PoolKeepItemListHandle<NodeData>> poolKeepItemListHandle = _GetInputBranches(null, checkedNodes);
		for (int num = poolKeepItemListHandle.Count - 1; num >= 0; num--)
		{
			if (poolKeepItemListHandle[num].value.LastRef().hasInput)
			{
				Pools.Repool(poolKeepItemListHandle[num]);
				poolKeepItemListHandle.RemoveAt(num);
			}
		}
		return poolKeepItemListHandle;
	}

	public PoolKeepItemListHandle<NodeData> GetOutputNodes(Type? outputType = null)
	{
		PoolKeepItemListHandle<NodeData> poolKeepItemListHandle = Pools.UseKeepItemList<NodeData>();
		foreach (NodeDataConnection outputConnection in GetOutputConnections(outputType))
		{
			poolKeepItemListHandle.Add(outputConnection.inputNode);
		}
		return poolKeepItemListHandle;
	}

	public PoolKeepItemHashSetHandle<NodeData> GetOutputNodesDeep(Type? outputType = null, PoolKeepItemHashSetHandle<NodeData> outputNodesDeep = null)
	{
		outputNodesDeep = outputNodesDeep ?? Pools.UseKeepItemHashSet<NodeData>();
		foreach (NodeData outputNode in GetOutputNodes(outputType))
		{
			if (outputNodesDeep.Add(outputNode))
			{
				outputNode.GetOutputNodesDeep(null, outputNodesDeep);
			}
		}
		return outputNodesDeep;
	}

	public IEnumerable<Couple<NodeData, int>> GetOutputNodesByDepth(Type? outputType = null, bool includeSelf = false)
	{
		using PoolKeepItemListHandle<NodeData> nodesAtDepth = GetOutputNodes(outputType);
		using PoolKeepItemHashSetHandle<NodeData> nodesAtNextDepth = Pools.UseKeepItemHashSet<NodeData>();
		using PoolKeepItemHashSetHandle<NodeData> alreadyReturned = Pools.UseKeepItemHashSet<NodeData>();
		if (includeSelf && alreadyReturned.Add(this))
		{
			yield return new Couple<NodeData, int>(this, 0);
		}
		int depth = 1;
		while (nodesAtDepth.value.Count > 0)
		{
			foreach (NodeData item in nodesAtDepth.value)
			{
				if (alreadyReturned.Add(item))
				{
					yield return new Couple<NodeData, int>(item, depth);
				}
			}
			foreach (NodeData item2 in nodesAtDepth.value)
			{
				foreach (NodeData outputNode in item2.GetOutputNodes())
				{
					if (!alreadyReturned.Contains(outputNode))
					{
						nodesAtNextDepth.value.Add(outputNode);
					}
				}
			}
			nodesAtDepth.value.Clear();
			foreach (NodeData item3 in nodesAtNextDepth.value)
			{
				nodesAtDepth.value.Add(item3);
			}
			nodesAtNextDepth.value.Clear();
			int num = depth + 1;
			depth = num;
		}
	}

	public bool IsLeaf(Func<NodeData, bool> validOutputNode = null, Type? outputType = null)
	{
		if (validOutputNode == null)
		{
			return !hasOutput;
		}
		foreach (NodeData item in GetOutputNodesDeep(outputType))
		{
			if (validOutputNode(item))
			{
				return false;
			}
		}
		return true;
	}

	private PoolDictionaryValuesHandle<NodeData, PoolListHandle<PoolKeepItemListHandle<NodeData>>> _GetOutputBranches(Func<NodeData, bool> stopBranchingAt, bool preprocess, PoolDictionaryValuesHandle<NodeData, PoolListHandle<PoolKeepItemListHandle<NodeData>>> outputBranches = null, PoolKeepItemListHandle<NodeData> currentBranch = null, Dictionary<NodeData, int> nodeDepths = null, HashSet<NodeData> inputNodes = null, bool hasRecursedOutput = false)
	{
		outputBranches = outputBranches ?? Pools.UseDictionaryValues<NodeData, PoolListHandle<PoolKeepItemListHandle<NodeData>>>();
		currentBranch = (currentBranch ? Pools.UseKeepItemList(currentBranch.value).Add(this) : Pools.UseKeepItemList<NodeData>());
		if (preprocess)
		{
			Preprocess();
		}
		foreach (NodeData outputNode in GetOutputNodes(activeOutputType))
		{
			bool hasRecursedOutput2 = hasRecursedOutput;
			if (nodeDepths != null && (hasRecursedOutput || (hasRecursedOutput2 = nodeDepths[outputNode] < nodeDepths[this])) && (inputNodes == null || !inputNodes.Contains(outputNode)))
			{
				continue;
			}
			if (stopBranchingAt(outputNode))
			{
				if (!outputBranches.ContainsKey(outputNode))
				{
					outputBranches.Add(outputNode, Pools.UseList<PoolKeepItemListHandle<NodeData>>());
				}
				outputBranches[outputNode].Add(Pools.UseKeepItemList(currentBranch.value));
			}
			else if (!currentBranch.value.Contains(outputNode))
			{
				outputNode._GetOutputBranches(stopBranchingAt, preprocess, outputBranches, currentBranch, nodeDepths, inputNodes, hasRecursedOutput2);
			}
		}
		if (preprocess)
		{
			UndoPreprocess();
		}
		Pools.Repool(currentBranch);
		return outputBranches;
	}

	private PoolDictionaryValuesHandle<NodeData, PoolKeepItemHashSetHandle<NodeData>> GetOutputBranches(Func<NodeData, bool> stopBranchingAt = null, bool checkSelf = false, bool preprocess = false, Dictionary<NodeData, int> nodeDepths = null, HashSet<NodeData> inputNodes = null)
	{
		PoolDictionaryValuesHandle<NodeData, PoolKeepItemHashSetHandle<NodeData>> poolDictionaryValuesHandle = Pools.UseDictionaryValues<NodeData, PoolKeepItemHashSetHandle<NodeData>>();
		stopBranchingAt = stopBranchingAt ?? ((Func<NodeData, bool>)((NodeData node) => !node.hasOutput));
		if (checkSelf && stopBranchingAt(this))
		{
			return poolDictionaryValuesHandle.Add(this, Pools.UseKeepItemHashSet<NodeData>());
		}
		foreach (KeyValuePair<NodeData, PoolListHandle<PoolKeepItemListHandle<NodeData>>> item in _GetOutputBranches(stopBranchingAt, preprocess, null, null, nodeDepths, inputNodes))
		{
			poolDictionaryValuesHandle.Add(item.Key, Pools.UseKeepItemHashSet(((IEnumerable<PoolKeepItemListHandle<NodeData>>)item.Value.value).Select((Func<PoolKeepItemListHandle<NodeData>, IEnumerable<NodeData>>)((PoolKeepItemListHandle<NodeData> h) => h.value)).Interweave()));
		}
		return poolDictionaryValuesHandle;
	}

	private PoolKeepItemHashSetHandle<NodeData> _GetProcessedOutputBranch(HashSet<NodeData> nodesToProcess, NodeData targetLeafNode, bool checkSelf, ref bool validBranch, bool doProcesses = true, PoolKeepItemHashSetHandle<NodeData> processedNodes = null, PoolKeepItemHashSetHandle<NodeData> traversed = null)
	{
		processedNodes = processedNodes ?? Pools.UseKeepItemHashSet<NodeData>();
		traversed = traversed ?? Pools.UseKeepItemHashSet<NodeData>();
		if (checkSelf && this == targetLeafNode)
		{
			validBranch = true;
		}
		using PoolKeepItemListHandle<NodeData> poolKeepItemListHandle = GetOutputNodes(activeOutputType);
		foreach (NodeData item in poolKeepItemListHandle.value)
		{
			if (item == targetLeafNode)
			{
				validBranch = true;
			}
			else if (nodesToProcess.Contains(item) && processedNodes.Add(item) && doProcesses)
			{
				item.Process();
			}
		}
		foreach (NodeData item2 in poolKeepItemListHandle.value)
		{
			if (nodesToProcess.Contains(item2) && traversed.Add(item2))
			{
				item2._GetProcessedOutputBranch(nodesToProcess, targetLeafNode, checkSelf: false, ref validBranch, doProcesses, processedNodes, traversed);
			}
		}
		return processedNodes;
	}

	public PoolDictionaryValuesHandle<NodeData, PoolKeepItemHashSetHandle<NodeData>> GetProcessedOutputBranches(Func<NodeData, bool> stopBranchingAt = null, bool checkSelf = false, bool doProcesses = true, UndoProcessType undoProcesses = UndoProcessType.All, bool processLeaves = false, Func<NodeData, bool> isValidBranch = null, HashSet<NodeData> execute = null, ProcessRecursionType recursion = ProcessRecursionType.PreventInfiniteRecursion, bool delayExecute = false)
	{
		_ProcessStopBranchingAt = stopBranchingAt;
		_ProcessIsValidBranch = isValidBranch;
		PoolDictionaryValuesHandle<NodeData, PoolKeepItemHashSetHandle<NodeData>> poolDictionaryValuesHandle = Pools.UseDictionaryValues<NodeData, PoolKeepItemHashSetHandle<NodeData>>();
		PoolKeepItemDictionaryHandle<NodeData, int> item = ((recursion == ProcessRecursionType.PreventInfiniteRecursion) ? null : graph.GetNodeMaxInputDepths());
		PoolKeepItemHashSetHandle<NodeData> item2 = ((recursion == ProcessRecursionType.RecurseInputOnly) ? GetInputNodesDeep(item) : null);
		foreach (KeyValuePair<NodeData, PoolKeepItemHashSetHandle<NodeData>> outputBranch in GetOutputBranches(stopBranchingAt, checkSelf, doProcesses, item, item2))
		{
			bool validBranch = false;
			PoolKeepItemHashSetHandle<NodeData> poolKeepItemHashSetHandle = _GetProcessedOutputBranch(outputBranch.Value.value, outputBranch.Key, checkSelf, ref validBranch, doProcesses);
			validBranch = validBranch && (isValidBranch?.Invoke(outputBranch.Key) ?? true);
			if (processLeaves && doProcesses)
			{
				outputBranch.Key.Process();
			}
			if (processLeaves && undoProcesses.ShouldUndo(validBranch))
			{
				outputBranch.Key.UndoProcess();
			}
			if (undoProcesses.ShouldUndo(validBranch))
			{
				foreach (NodeData item3 in poolKeepItemHashSetHandle.value.Reverse())
				{
					item3.UndoProcess();
				}
			}
			if (validBranch && execute != null)
			{
				foreach (NodeData item4 in poolKeepItemHashSetHandle.value)
				{
					if (execute.Add(item4) && !delayExecute)
					{
						item4.Execute();
					}
				}
				if (processLeaves && execute.Add(outputBranch.Key) && !delayExecute)
				{
					outputBranch.Key.Execute();
				}
			}
			if (validBranch)
			{
				poolDictionaryValuesHandle.Add(outputBranch.Key, poolKeepItemHashSetHandle);
			}
			else
			{
				Pools.Repool(poolKeepItemHashSetHandle);
			}
		}
		Pools.Repool(ref item2);
		Pools.Repool(ref item);
		if (execute != null && poolDictionaryValuesHandle.Count > 0)
		{
			graph.SignalProcessExecuted(poolDictionaryValuesHandle);
		}
		return poolDictionaryValuesHandle;
	}

	public int GetInputDepthMin()
	{
		int num = 0;
		using PoolKeepItemListHandle<NodeData> poolKeepItemListHandle = Pools.UseKeepItemList(this);
		using PoolKeepItemListHandle<NodeData> poolKeepItemListHandle3 = Pools.UseKeepItemList<NodeData>();
		while (true)
		{
			foreach (NodeData item in poolKeepItemListHandle.value)
			{
				using PoolKeepItemListHandle<NodeDataConnection> poolKeepItemListHandle2 = item.GetInputConnections();
				if (poolKeepItemListHandle2.Count == 0)
				{
					return num;
				}
				foreach (NodeDataConnection item2 in poolKeepItemListHandle2)
				{
					poolKeepItemListHandle3.Add(item2.outputNode);
				}
			}
			poolKeepItemListHandle.value.ClearAndCopyFrom(poolKeepItemListHandle3);
			poolKeepItemListHandle3.value.Clear();
			num++;
		}
	}

	public int GetInputDepthMax()
	{
		int num = 0;
		using PoolKeepItemListHandle<NodeData> poolKeepItemListHandle = Pools.UseKeepItemList(this);
		using PoolKeepItemListHandle<NodeData> poolKeepItemListHandle2 = Pools.UseKeepItemList<NodeData>();
		while (true)
		{
			foreach (NodeData item in poolKeepItemListHandle.value)
			{
				foreach (NodeDataConnection inputConnection in item.GetInputConnections())
				{
					poolKeepItemListHandle2.Add(inputConnection.outputNode);
				}
			}
			if (poolKeepItemListHandle2.Count == 0)
			{
				break;
			}
			poolKeepItemListHandle.value.ClearAndCopyFrom(poolKeepItemListHandle2);
			poolKeepItemListHandle2.value.Clear();
			num++;
		}
		return num;
	}

	public IEnumerable<uint> GetIdPath()
	{
		NodeData current = this;
		NodeData parentNode;
		do
		{
			yield return current.id;
			current = (parentNode = current.graph.parentNode);
		}
		while (parentNode != null);
	}

	public IEnumerable<uint> GetIdPathRelativeToOwningReference()
	{
		NodeData current = this;
		NodeData parentNode;
		do
		{
			yield return current.id;
			current = (parentNode = current.graph.parentNode);
		}
		while (parentNode != null && !(current is NodeDataRef));
	}

	public NodeData SetCompletion(Type? completion)
	{
		completionType = completion;
		return this;
	}

	public void RemoveNodeAndPatchConnections()
	{
		bool flag = beingEdited;
		foreach (NodeDataConnection inputConnection in GetInputConnections())
		{
			foreach (NodeDataConnection outputConnection in GetOutputConnections())
			{
				if (!flag || inputConnection.outputNodeId != outputConnection.inputNodeId)
				{
					graph.AddConnection(new NodeDataConnection(inputConnection.outputNodeId, outputConnection.inputNodeId, inputConnection.outputType));
				}
			}
		}
		graph.RemoveNode(this);
	}

	public NodeData ReplaceWithNewNode(NodeData newNode)
	{
		NodeGraph nodeGraph = graph;
		newNode.id = id;
		if (newNode.name.IsNullOrEmpty())
		{
			newNode.name = name;
		}
		newNode.globalId = globalId;
		newNode.position = position;
		newNode.completionType = completionType;
		using PoolKeepItemListHandle<NodeDataConnection> poolKeepItemListHandle = GetConnections();
		nodeGraph.RemoveNode(this);
		newNode.ClearConnectionIds();
		nodeGraph.AddNode(newNode, newNode.id);
		foreach (NodeDataConnection item in poolKeepItemListHandle.value)
		{
			nodeGraph.AddConnection((item.outputNodeId == (uint)newNode && !newNode.output.Contains(item.outputType)) ? ProtoUtil.Clone(item).SetOutputType(newNode.output[0]) : item, item.id);
		}
		return newNode;
	}

	public NodeData ClearForPreset()
	{
		id = 0u;
		globalId = Guid.Empty;
		ClearConnectionIds();
		position = Vector2.zero;
		completionType = null;
		return this;
	}

	public void Flatten(bool recursivelyFlatten = false)
	{
		if (!canBeFlattened)
		{
			if (!canFlattenChildren)
			{
				return;
			}
			subGraph.Prepend(new NodeDataRouter());
			{
				foreach (NodeData node in subGraph.GetNodes())
				{
					node.Flatten(recursivelyFlatten);
				}
				return;
			}
		}
		if (subGraph.nodeCount == 0)
		{
			RemoveNodeAndPatchConnections();
			return;
		}
		using PoolKeepItemListHandle<NodeData> poolKeepItemListHandle = subGraph.GetNodes();
		using PoolKeepItemListHandle<NodeData> poolKeepItemListHandle4 = subGraph.GetRootNodes();
		using PoolStructListHandle<Couple<NodeData, Type>> poolStructListHandle = subGraph.GetLeafNodes();
		using PoolKeepItemListHandle<NodeDataConnection> poolKeepItemListHandle3 = subGraph.GetConnections();
		using PoolKeepItemDictionaryHandle<uint, uint> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<uint, uint>();
		using PoolKeepItemListHandle<NodeData> poolKeepItemListHandle2 = Pools.UseKeepItemList<NodeData>();
		Vector2 vector = poolKeepItemListHandle.value.Select((NodeData node) => node.position).Centroid();
		foreach (NodeData item in poolKeepItemListHandle.value)
		{
			subGraph.RemoveNode(item);
		}
		foreach (NodeData item2 in poolKeepItemListHandle.value)
		{
			item2.position = position + (item2.position - vector);
			poolKeepItemDictionaryHandle.Add(item2, graph.AddNode(item2, item2.id));
			if (recursivelyFlatten && item2.isSubGraph)
			{
				poolKeepItemListHandle2.Add(item2);
			}
		}
		foreach (NodeDataConnection item3 in poolKeepItemListHandle3)
		{
			item3.outputNodeId = poolKeepItemDictionaryHandle[item3.outputNodeId];
			item3.inputNodeId = poolKeepItemDictionaryHandle[item3.inputNodeId];
			graph.AddConnection(item3, item3.id);
		}
		foreach (NodeDataConnection connection2 in GetConnections())
		{
			if (connection2.inputNodeId == id)
			{
				NodeData outputNode = connection2.outputNode;
				int num = outputNode.connectionIds.IndexOf(connection2);
				foreach (NodeData item4 in poolKeepItemListHandle4.value)
				{
					NodeDataConnection connection = new NodeDataConnection(connection2.outputNodeId, item4, connection2.outputType);
					graph.AddConnection(connection);
					outputNode.connectionIds.RemoveAndInstertIndex(outputNode.connectionIds.Count - 1, num++);
				}
				continue;
			}
			foreach (Couple<NodeData, Type> item5 in poolStructListHandle.value)
			{
				graph.AddConnection(new NodeDataConnection(item5.a, connection2.inputNodeId, item5.b));
			}
		}
		foreach (NodeData item6 in poolKeepItemListHandle.value)
		{
			item6.SortConnections();
		}
		graph.RemoveNode(this);
		foreach (NodeData item7 in poolKeepItemListHandle2.value)
		{
			item7.Flatten(recursivelyFlatten);
		}
	}

	public bool ShouldEvaluateSubGraph(bool stopAtCannotBeFlattened = false)
	{
		if (isSubGraph && !isRecursivelyNested)
		{
			if (stopAtCannotBeFlattened)
			{
				return canBeFlattened;
			}
			return true;
		}
		return false;
	}

	public void ValidateInHierarchy()
	{
		for (NodeGraph parentGraph = graph; parentGraph != null; parentGraph = parentGraph.parentGraph)
		{
			parentGraph.ValidateNode(this);
		}
	}

	public string GetNameWithPath(int maxPathCount = 0, bool useSearchName = false, int sizePercent = 66)
	{
		return string.Format("{0}<size={1}%> [{2}]</size>", useSearchName ? searchName : name, sizePercent, (graph != null) ? graph.GetPathString(maxPathCount) : "");
	}

	public string GetSearchText(bool includeType = true)
	{
		Builder.Append(searchName).Space();
		if (includeType)
		{
			Builder.Append(GetType().GetUILabel()).Space();
		}
		_GetSearchText(Builder);
		return Builder.TrimEnd().ToString().RemoveRichText();
	}

	public int CompareTo(NodeData other)
	{
		return MathUtil.Compare(id, other.id);
	}

	public static implicit operator uint(NodeData node)
	{
		return node?.id ?? 0;
	}

	public static implicit operator bool(NodeData node)
	{
		if (node != null)
		{
			return node.id != 0;
		}
		return false;
	}

	public override string ToString()
	{
		return name;
	}

	protected void _InitName(UIFieldAttribute uiField)
	{
		if (_maxNameLength.HasValue)
		{
			uiField.max = _maxNameLength.Value;
		}
	}
}
