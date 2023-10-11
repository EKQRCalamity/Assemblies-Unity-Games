using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(NodeGraphView))]
public class NodeGraphContextMenu : AContextMenu
{
	public class CopyData
	{
		public Dictionary<uint, NodeData> nodes = new Dictionary<uint, NodeData>();

		public Dictionary<uint, NodeDataConnection> connections = new Dictionary<uint, NodeDataConnection>();

		public Dictionary<uint, NodeData> copyOfNodes => ProtoUtil.Clone(nodes);

		public Dictionary<uint, NodeDataConnection> copyOfConnections => ProtoUtil.Clone(connections);

		public void Clear()
		{
			nodes.Clear();
			connections.Clear();
		}

		public CopyData SetData(IEnumerable<NodeData> nodesToCopy, IEnumerable<NodeDataConnection> connectionsToCopy)
		{
			Clear();
			foreach (NodeData item in nodesToCopy)
			{
				nodes.Add(item, ProtoUtil.Clone(item));
			}
			foreach (NodeDataConnection item2 in connectionsToCopy)
			{
				if (nodes.ContainsKey(item2.outputNodeId) && nodes.ContainsKey(item2.inputNodeId))
				{
					connections.Add(item2, ProtoUtil.Clone(item2));
				}
			}
			return this;
		}

		public static implicit operator bool(CopyData copyData)
		{
			if (copyData != null)
			{
				return copyData.nodes.Count > 0;
			}
			return false;
		}
	}

	private const string CAT_MAIN = "Main";

	private const string ADD_NODE = "Add Node/";

	private const string CAT_MODIFY = "Modify";

	private const string CAT_SERIALIZE = "Serialize";

	private const string CAT_HISTORY = "History";

	private const string CAT_DESTROY = "Destroy";

	private const string VIEW = "View/";

	private static readonly string PRESET = "Presets/";

	private static float? _MinPasteOffset;

	public int autoLayoutRelaxIterations = 20;

	public int autoLayoutSpringIterations = 100;

	public float autoLayoutHorizontalSpacing = 240f;

	public float autoLayoutVerticalSpacing = 60f;

	public float autoLayoutSpringConstant = 100f;

	public float autoLayoutSpringDampening = 10f;

	private NodeGraphView _graphView;

	private CopyData _copyData;

	private NodeData _copiedValue;

	private static float MinPasteOffset
	{
		get
		{
			float? minPasteOffset = _MinPasteOffset;
			if (!minPasteOffset.HasValue)
			{
				float? num = (_MinPasteOffset = NodeDataView.Size.Min() * 0.5f);
				return num.Value;
			}
			return minPasteOffset.GetValueOrDefault();
		}
	}

	private NodeGraphView graphView => this.CacheComponent(ref _graphView);

	private CopyData copyData => _copyData ?? (_copyData = new CopyData());

	private NodeGraph _graph => graphView.graph;

	private bool _AddReferencedNodeInContext(ContextMenuContext context)
	{
		return DataRef<NodeGraphRef>.Search(mustBeCommitted: false).Any();
	}

	private bool _CreateSubGraphFromNodesInContext(ContextMenuContext context)
	{
		return context.GetSelection<NodeDataView>().Count > 1;
	}

	private bool _FlattenSubGraphsInContext(ContextMenuContext context)
	{
		return context.GetSelection<NodeDataView>().Any((NodeDataView nodeDataView) => nodeDataView.node.IsSubGraph() && nodeDataView.node.canBeFlattened);
	}

	private bool _AutoConnectInContext(ContextMenuContext context)
	{
		return context.GetSelection<NodeDataView>().Count > 1;
	}

	private bool _RouteConnectionsInContext(ContextMenuContext context)
	{
		if (context.GetSelection<NodeDataView>().Count == 0)
		{
			return context.GetSelection<NodeDataConnectionView>().Count > 0;
		}
		return false;
	}

	private bool _RouteConnectionsThroughNodeInContext(ContextMenuContext context)
	{
		if (context.GetSelection<NodeDataConnectionView>().Count > 0 && context.GetSelection<NodeDataView>().Count > 0)
		{
			return context.GetSelection<NodeDataView>().All((NodeDataView view) => (bool)view.node && view.node.connectionIds.Count == 0);
		}
		return false;
	}

	private bool _DeleteInContext(ContextMenuContext context)
	{
		return context.selection.Count > 0;
	}

	private bool _RemoveConnectionsInContext(ContextMenuContext context)
	{
		if (context.GetSelection<NodeDataConnectionView>().Count > 0)
		{
			return context.GetSelection<NodeDataView>().Count == 0;
		}
		return false;
	}

	private bool _DeleteConnectionsInContext(ContextMenuContext context)
	{
		if (context.GetSelection<NodeDataConnectionView>().Count > 0)
		{
			return context.GetSelection<NodeDataView>().Count > 0;
		}
		return false;
	}

	private bool _RemoveInContext(ContextMenuContext context)
	{
		return context.GetSelection<NodeDataView>().Count > 0;
	}

	private bool _CopyInContext(ContextMenuContext context)
	{
		return context.GetSelection<NodeDataView>().Count > 0;
	}

	private bool _PasteInContext(ContextMenuContext context)
	{
		if ((bool)copyData)
		{
			return _graph.AreValidNodes(copyData.nodes.Values.GetNodesDeep(stopAtCannotBeFlattened: true));
		}
		return false;
	}

	private bool _PasteOverInContext(ContextMenuContext context)
	{
		if (context.GetSelection<NodeDataView>().Count == 1 && copyData.nodes.Count == 1 && copyData.connections.Count == 0)
		{
			return _PasteInContext(context);
		}
		return false;
	}

	private bool _CopyValuesInContext(ContextMenuContext context)
	{
		return context.GetSelection<NodeDataView>().Count == 1;
	}

	private bool _PasteValuesInContext(ContextMenuContext context)
	{
		if (_copiedValue != null)
		{
			return context.GetSelection<NodeDataView>().Any((NodeDataView nodeView) => nodeView.node != null && nodeView.node.GetType() == _copiedValue.GetType());
		}
		return false;
	}

	private bool _UndoInContext(ContextMenuContext context)
	{
		return graphView.undoHistory.HasUndo();
	}

	private bool _RedoInContext(ContextMenuContext context)
	{
		return graphView.undoHistory.HasRedo();
	}

	private bool _CreateNewReferencedNodeInContext(ContextMenuContext context)
	{
		if (context.GetSelection<NodeDataView>().Count == 1)
		{
			return context.GetSelection<NodeDataView>()[0].node is NodeDataSubGraph;
		}
		return false;
	}

	private bool _BreakReferenceInContext(ContextMenuContext context)
	{
		if (context.GetSelection<NodeDataView>().Count == 1)
		{
			return context.GetSelection<NodeDataView>()[0].node is NodeDataRef;
		}
		return false;
	}

	private bool _RevertSubGraphRefChangesInContext(ContextMenuContext context)
	{
		if (_BreakReferenceInContext(context))
		{
			return ((NodeDataRef)context.GetSelection<NodeDataView>()[0].node).canRevertChanges;
		}
		return false;
	}

	private bool _SaveGameDataInContext(ContextMenuContext context)
	{
		return DataRefControl.GetControl(_graph.rootGraph.owningContentRef).GetDataRef().SaveCanBeOverriden();
	}

	private bool _GenerateGraphInContext(ContextMenuContext context)
	{
		if (context.GetSelection<NodeDataView>().Count == 1 && (bool)context.GetSelection<NodeDataView>()[0].node)
		{
			return context.GetSelection<NodeDataView>()[0].node.isSubGraph;
		}
		return false;
	}

	private bool _SoloNodeSelectInContext<T>(ContextMenuContext context, Func<T, bool> validNode = null) where T : NodeData
	{
		if (context.selection.Count == 1 && context.GetSelection<NodeDataView>().Count == 1 && context.GetSelection<NodeDataView>()[0].node is T)
		{
			return validNode?.Invoke(context.GetSelection<NodeDataView>()[0].node as T) ?? true;
		}
		return false;
	}

	private bool _GoToWarpExitInContext(ContextMenuContext context)
	{
		return _SoloNodeSelectInContext(context, (NodeDataWarpEnter n) => !n.returnToLastWarpEnter);
	}

	private bool _GoToWarpEnterInContext(ContextMenuContext context)
	{
		return _SoloNodeSelectInContext<NodeDataWarpExit>(context);
	}

	private bool _GoToFlagSetInContext(ContextMenuContext context)
	{
		return _SoloNodeSelectInContext<ANodeDataFlagCheck>(context);
	}

	private bool _GoToFlagCheckInContext(ContextMenuContext context)
	{
		return _SoloNodeSelectInContext<ANodeDataFlagSet>(context);
	}

	private bool _GoToEventTriggerInContext(ContextMenuContext context)
	{
		return _SoloNodeSelectInContext<NodeDataEventWarpExit>(context);
	}

	private bool _SearchNodesInContext(ContextMenuContext context)
	{
		return context.GetSelection<NodeDataView>().Count == 0;
	}

	private bool _SearchNodesOfTypeInContext(ContextMenuContext context)
	{
		return context.GetSelection<NodeDataView>().Count == 1;
	}

	private bool _WatchTutorialVideoInContext(ContextMenuContext context)
	{
		if (context.GetSelection<NodeDataView>().Count == 1 && (bool)context.GetSelection<NodeDataView>()[0].node)
		{
			return context.GetSelection<NodeDataView>()[0].node.GetType().HasAttribute<YouTubeVideoAttribute>();
		}
		return false;
	}

	private IEnumerable<ContextMenuAction> _GetAddNodeContextActions()
	{
		foreach (Type item in from t in typeof(NodeData).GetDerivedClasses()
			where t.GetAttribute<UIFieldAttribute>() != null && t.IsConcrete()
			orderby t.GetAttribute<UIFieldAttribute>().order
			select t)
		{
			Type type = item;
			NodeData @default = ConstructorCacheType.GetDefault<NodeData>(item);
			if (!@default.hideFromContext)
			{
				string typeLabel = type.GetUILabel();
				yield return new ContextMenuAction("Add Node/" + @default.contextPath + typeLabel, @default.contextCategory, delegate(ContextMenuContext context)
				{
					_graph.AddNode(ConstructorCacheType.Construct<NodeData>(type).SetData(typeLabel, graphView.contentGrid.WorldToGridPosition(context.mouseWorldPosition)));
				}, @default.contextHotKey, (ContextMenuContext context) => _graph.validNodeDataTypes.Contains(type));
			}
		}
	}

	private void _AddReferencedNode(ContextMenuContext context)
	{
		UIUtil.CreateDataSearchPopup(delegate(DataRef<NodeGraphRef> graphRef)
		{
			_graph.AddNode(new NodeDataRef(graphRef).SetData(graphRef.friendlyName, graphView.contentGrid.WorldToGridPosition(context.mouseWorldPosition)));
		}, _graphView.rootParent, (DataRef<NodeGraphRef> graphRef) => _graph.AreValidNodes(graphRef.data.GetNodesDeep(stopAtCannotBeFlattened: true)), mustBeCommitted: false);
	}

	private void _AddReferencedNodeAsPreset(ContextMenuContext context)
	{
		UIUtil.CreateDataSearchPopup(delegate(DataRef<NodeGraphRef> graphRef)
		{
			NodeData nodeData = new NodeDataRef(graphRef).SetData(graphRef.friendlyName, graphView.contentGrid.WorldToGridPosition(context.mouseWorldPosition));
			_graph.AddNode(nodeData);
			NodeDataSubGraph nodeDataSubGraph = nodeData.ReplaceWithNewNode(new NodeDataSubGraph(ProtoUtil.Clone(nodeData.subGraph))) as NodeDataSubGraph;
			if (nodeDataSubGraph.subGraph.nodeCount == 1)
			{
				nodeDataSubGraph.Flatten();
			}
		}, _graphView.rootParent, (DataRef<NodeGraphRef> graphRef) => _graph.AreValidNodes(graphRef.data.GetNodesDeep(stopAtCannotBeFlattened: true)), mustBeCommitted: false);
	}

	private void _CreateSubGraphFromNodes(ContextMenuContext context)
	{
		new NodeDataSubGraph(_graph, from nodeDataView in context.GetSelection<NodeDataView>()
			select nodeDataView.node);
	}

	private void _FlattenSubGraphs(ContextMenuContext context)
	{
		foreach (NodeDataView item in context.GetSelection<NodeDataView>())
		{
			if (item.node.IsSubGraph())
			{
				item.node.Flatten();
			}
		}
	}

	private void _AutoConnect(ContextMenuContext context)
	{
		float num = NodeDataView.Size.x + 48f;
		using PoolKeepItemListHandle<NodeData> poolKeepItemListHandle = Pools.UseKeepItemList(from nodeDataView in context.GetSelection<NodeDataView>()
			select nodeDataView.node);
		using PoolListHandle<PoolKeepItemListHandle<NodeData>> poolListHandle = Pools.UseList<PoolKeepItemListHandle<NodeData>>();
		float num2 = float.MinValue;
		poolKeepItemListHandle.value.Sort(NodeDataXPositionComparer.Default);
		foreach (NodeData item in poolKeepItemListHandle.value)
		{
			if (Mathf.Abs(item.position.x - num2) >= num)
			{
				poolListHandle.Add(Pools.UseKeepItemList<NodeData>());
			}
			poolListHandle[poolListHandle.Count - 1].Add(item);
			num2 = item.position.x;
		}
		for (int i = 0; i < poolListHandle.Count - 1; i++)
		{
			foreach (Couple<NodeData, NodeData> item2 in poolListHandle[i].value.Pairings(poolListHandle[i + 1].value))
			{
				if (!item2.a.ConnectedTo(item2.b))
				{
					graphView.graph.AddConnection(new NodeDataConnection(item2.a, item2.b));
				}
			}
		}
	}

	private void _RouteConnections(ContextMenuContext context)
	{
		NodeData node = _graph.AddNode(new NodeDataRouter().SetData(null, graphView.contentGrid.WorldToGridPosition(context.mouseWorldPosition)));
		foreach (NodeDataConnection item in Pools.UseKeepItemList(from view in context.GetSelection<NodeDataConnectionView>()
			select view.connection))
		{
			_RouteConnectionsCommon(item, node);
		}
	}

	private void _RouteConnectionsThroughNode(ContextMenuContext context)
	{
		foreach (NodeDataConnection item in Pools.UseKeepItemList(from view in context.GetSelection<NodeDataConnectionView>()
			select view.connection))
		{
			foreach (NodeData item2 in Pools.UseKeepItemList(from view in context.GetSelection<NodeDataView>()
				select view.node))
			{
				_RouteConnectionsCommon(item, item2);
			}
		}
	}

	private void _RouteConnectionsCommon(NodeDataConnection connection, NodeData node)
	{
		_graph.RouteConnection(connection, node);
	}

	private void _Delete(ContextMenuContext context)
	{
		foreach (NodeDataConnectionView item in context.GetSelection<NodeDataConnectionView>())
		{
			item.graphView.graph.RemoveConnection(item);
		}
		foreach (NodeDataView item2 in context.GetSelection<NodeDataView>())
		{
			item2.graphView.graph.RemoveNode(item2);
		}
	}

	private void _DeleteConnections(ContextMenuContext context)
	{
		foreach (NodeDataConnectionView item in context.GetSelection<NodeDataConnectionView>())
		{
			item.graphView.graph.RemoveConnection(item);
		}
	}

	private void _Remove(ContextMenuContext context)
	{
		foreach (NodeDataView item in context.GetSelection<NodeDataView>())
		{
			item.node.RemoveNodeAndPatchConnections();
		}
	}

	private void _Copy(ContextMenuContext context)
	{
		copyData.SetData(from nodeView in context.GetSelection<NodeDataView>()
			select nodeView.node, context.GetSelection<NodeDataView>().SelectMany((NodeDataView nodeView) => nodeView.node.GetConnectionsEnumerable()).Distinct());
		_MarkContextDirty();
	}

	private void _Paste(ContextMenuContext context)
	{
		Vector2 vector = graphView.contentGrid.WorldToGridPosition(context.mouseWorldPosition);
		Vector2 vector2 = copyData.nodes.Values.Select((NodeData node) => node.position).Centroid();
		Vector2 vector3 = vector - vector2;
		if (vector3.magnitude < MinPasteOffset)
		{
			vector3 = new Vector2(1f, -1f) * MinPasteOffset;
		}
		foreach (NodeData value in copyData.nodes.Values)
		{
			value.position += vector3;
		}
		Dictionary<uint, NodeData> copyOfNodes = copyData.copyOfNodes;
		Dictionary<uint, NodeDataConnection> copyOfConnections = copyData.copyOfConnections;
		_graph.Paste(copyOfNodes, copyOfConnections);
		graphView.GetNode(copyOfNodes.Values.First()).selectable.group.ClearSelected();
		foreach (NodeData value2 in copyOfNodes.Values)
		{
			graphView.GetNode(value2).selectable.SetSelected(isSelected: true);
		}
		foreach (NodeDataConnection value3 in copyOfConnections.Values)
		{
			graphView.GetConnection(value3).selectable.SetSelected(isSelected: true);
		}
	}

	private void _PasteOver(ContextMenuContext context)
	{
		graphView.GetNode(context.GetSelection<NodeDataView>()[0].node.ReplaceWithNewNode(ProtoUtil.Clone(copyData.nodes.Values.First()))).selectable.SetSelected(isSelected: true);
	}

	private void _Duplicate(ContextMenuContext context)
	{
		_Copy(context);
		_Paste(context);
	}

	private void _CopyValues(ContextMenuContext context)
	{
		_copiedValue = ProtoUtil.Clone(context.GetSelection<NodeDataView>()[0].node);
	}

	private void _PasteValues(ContextMenuContext context)
	{
		NodeGraph.PasteValues(_copiedValue, from nodeView in context.GetSelection<NodeDataView>()
			select nodeView.node);
		if (context.GetSelection<NodeDataView>().Count == 1)
		{
			graphView.selectableGroup.RefreshSelection();
		}
	}

	private void _Undo(ContextMenuContext context)
	{
		graphView.undoHistory.Undo();
	}

	private void _Redo(ContextMenuContext context)
	{
		graphView.undoHistory.Redo();
	}

	private void _SelectAll(ContextMenuContext context)
	{
		graphView.GetComponentInChildren<SelectableGroup>().SelectAll();
	}

	private void _CreateNewReferencedNode(ContextMenuContext context)
	{
		NodeDataSubGraph obj = context.GetSelection<NodeDataView>()[0].node as NodeDataSubGraph;
		obj.ReplaceWithNewNode(new NodeDataRef(obj.subGraph));
	}

	private void _BreakReference(ContextMenuContext context)
	{
		NodeDataRef obj = context.GetSelection<NodeDataView>()[0].node as NodeDataRef;
		obj.ReplaceWithNewNode(new NodeDataSubGraph(ProtoUtil.Clone(obj.subGraph)));
	}

	private void _EditSubGraphReferenceTags(ContextMenuContext context)
	{
		NodeDataRef nodeDataRef = (NodeDataRef)context.GetSelection<NodeDataView>()[0].node;
		UIUtil.CreateContentRefTagsPopup(nodeDataRef.nodeGraphRef, _graphView.transform, nodeDataRef.subGraph, delegate
		{
			nodeDataRef.nodeGraphRef.SaveFromUIWithoutValidation(nodeDataRef.subGraph);
		});
	}

	private void _RevertSubGraphRefChanges(ContextMenuContext context)
	{
		graphView.GetNode(((NodeDataRef)context.GetSelection<NodeDataView>()[0].node).RevertChanges()).Refresh();
	}

	private void _SaveGameData(ContextMenuContext context)
	{
		_graphView.PrepareForSave();
		DataRefControl.GetControl(_graph.rootGraph.owningContentRef).OnSaveChanges(_graphView.rootParent);
	}

	private void _GenerateGraph(ContextMenuContext context)
	{
		UIUtil.CreatePopup("Generate Graph (DEBUG)", UIUtil.CreateMessageBox("This process CANNOT be undone and is for DEBUGGING purposes only. Please Make sure to SAVE before Generating Graph."), null, parent: _graphView.rootParent, buttons: new string[2] { "Generate Graph", "Cancel" }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string button)
		{
			if (button == "Generate Graph")
			{
				context.GetSelection<NodeDataView>()[0].node.subGraph.GenerateGraph();
			}
		});
	}

	private void _DoNodeSearch<T>(string title, NodeGraph graph, Func<T, bool> validNode, Func<T, UIListItemData> toUIListItem, Action<List<T>> onSearchResults = null, bool fromCannotBeFlattenedRootGraph = true, bool stopAtCannotBeFlattened = true) where T : NodeData
	{
		using PoolKeepItemListHandle<T> poolKeepItemListHandle = Pools.UseKeepItemList(graph.SearchNodes(validNode, fromCannotBeFlattenedRootGraph, stopAtCannotBeFlattened));
		onSearchResults?.Invoke(poolKeepItemListHandle);
		if (poolKeepItemListHandle.Count == 0)
		{
			return;
		}
		if (poolKeepItemListHandle.Count == 1)
		{
			graphView.ViewNode(poolKeepItemListHandle[0]);
			return;
		}
		string title2 = "Search: " + title;
		GameObject mainContent = UIUtil.CreateUIList(poolKeepItemListHandle.value.ToList(), delegate(T selected)
		{
			graphView.ViewNode(selected);
		}, null, null, toUIListItem);
		Transform rootParent = _graphView.rootParent;
		Color? rayCastBlockerColor = new Color(1f, 1f, 1f, 0.2f);
		Vector2? pivot = new Vector2(0f, 0.5f);
		Vector2? center = new Vector2(1f, 0.333f);
		UIUtil.CreatePopup(title2, mainContent, null, null, null, center, pivot, null, true, true, null, null, rayCastBlockerColor, rootParent, null, null);
	}

	private void _GoToWarpExit(ContextMenuContext context)
	{
		NodeDataWarpEnter warpEnterNode = context.GetSelection<NodeDataView>()[0].node as NodeDataWarpEnter;
		Dictionary<NodeGraph, List<NodeDataWarpExit>> warpExitsByGraph = null;
		_DoNodeSearch("Warp Exit [" + warpEnterNode.identifiers.name + "]", warpEnterNode.graph, (NodeDataWarpExit warpExit) => warpEnterNode.identifiers.MatchFound(warpExit.identifiers, matchIfEmpty: true), (NodeDataWarpExit warpExit) => new UIListItemData($"[{warpExit.graph.GetPathString(2)}] ({warpExitsByGraph[warpExit.graph].IndexOf(warpExit) + 1})", warpExit), delegate(List<NodeDataWarpExit> warpExits)
		{
			warpExitsByGraph = (from n in warpExits
				group n by n.graph).ToDictionary((IGrouping<NodeGraph, NodeDataWarpExit> g) => g.Key, (IGrouping<NodeGraph, NodeDataWarpExit> g) => g.ToList());
		});
	}

	private void _GoToWarpEnter(ContextMenuContext context)
	{
		NodeDataWarpExit warpExitNode = context.GetSelection<NodeDataView>()[0].node as NodeDataWarpExit;
		Dictionary<NodeGraph, List<NodeDataWarpEnter>> warpEntersByGraph = null;
		_DoNodeSearch("Warp Enter [" + warpExitNode.identifiers.name + "]", warpExitNode.graph, (NodeDataWarpEnter warpEnter) => !warpEnter.returnToLastWarpEnter && warpExitNode.identifiers.MatchFound(warpEnter.identifiers, matchIfEmpty: true), (NodeDataWarpEnter warpEnter) => new UIListItemData($"[{warpEnter.graph.GetPathString(2)}] ({warpEntersByGraph[warpEnter.graph].IndexOf(warpEnter) + 1})", warpEnter), delegate(List<NodeDataWarpEnter> warpEnters)
		{
			warpEntersByGraph = (from n in warpEnters
				group n by n.graph).ToDictionary((IGrouping<NodeGraph, NodeDataWarpEnter> g) => g.Key, (IGrouping<NodeGraph, NodeDataWarpEnter> g) => g.ToList());
		});
	}

	private void _GoToFlagSet(ContextMenuContext context)
	{
		ANodeDataFlagCheck flagCheck = context.GetSelection<NodeDataView>()[0].node as ANodeDataFlagCheck;
		NodeGraph graph = ((flagCheck.flagType == NodeDataFlagType.Persisted) ? flagCheck.graph.rootGraph : flagCheck.graph.FindParent((NodeGraph g) => !g.parentNode.canBeFlattened));
		_DoNodeSearch("Flag Set [" + flagCheck.flagName + "]", graph, (ANodeDataFlagSet flagSet) => flagSet.flagType == flagCheck.flagType && flagSet.flagName == flagCheck.flagName, (ANodeDataFlagSet flagSet) => new UIListItemData(flagSet.GetNameWithPath(2), flagSet), null, fromCannotBeFlattenedRootGraph: false, stopAtCannotBeFlattened: false);
	}

	private void _GoToFlagCheck(ContextMenuContext context)
	{
		ANodeDataFlagSet flagSet = context.GetSelection<NodeDataView>()[0].node as ANodeDataFlagSet;
		NodeGraph graph = ((flagSet.flagType == NodeDataFlagType.Persisted) ? flagSet.graph.rootGraph : flagSet.graph.FindParent((NodeGraph g) => !g.parentNode.canBeFlattened));
		_DoNodeSearch("Flag Check [" + flagSet.flagName + "]", graph, (ANodeDataFlagCheck flagCheck) => flagCheck.flagType == flagSet.flagType && flagCheck.flagName == flagSet.flagName, (ANodeDataFlagCheck flagCheck) => new UIListItemData(flagCheck.GetNameWithPath(2), flagCheck), null, fromCannotBeFlattenedRootGraph: false, stopAtCannotBeFlattened: false);
	}

	private void _GoToEventTrigger(ContextMenuContext context)
	{
		NodeDataEventWarpExit nodeDataEventWarpExit = context.GetSelection<NodeDataView>()[0].node as NodeDataEventWarpExit;
		_DoNodeSearch("Event Trigger [" + nodeDataEventWarpExit.flagName + "]", nodeDataEventWarpExit.graph, nodeDataEventWarpExit.CanBeTriggeredBy, (ANodeDataFlagSet flagSet) => new UIListItemData(flagSet.GetNameWithPath(2), flagSet));
	}

	private void _SearchNodesCommon<T>() where T : NodeData
	{
		bool prependType = typeof(T) == typeof(NodeData);
		NodeGraph nodeGraph = _graph.FindParent((NodeGraph g) => !g.parentNode.canBeFlattened);
		_DoNodeSearch(((!prependType) ? (typeof(T).GetUILabel() + " Nodes In ") : "Nodes In ") + nodeGraph.GetType().GetUILabel(), nodeGraph, (T n) => true, (T n) => new UIListItemData((prependType ? $"<size=66%>[{n.GetType().GetUILabel()}] </size>" : "") + n.GetNameWithPath(2, useSearchName: true), n, null, n.GetSearchText(prependType)), null, fromCannotBeFlattenedRootGraph: false, stopAtCannotBeFlattened: false);
	}

	private void _SearchNodes(ContextMenuContext context)
	{
		_SearchNodesCommon<NodeData>();
	}

	private void _SearchNodesOfType(ContextMenuContext context)
	{
		this.InvokeGenericMethod("_SearchNodesCommon", context.GetSelection<NodeDataView>()[0].node.GetType());
	}

	private void _WatchTutorialVideo(ContextMenuContext context)
	{
		context.GetSelection<NodeDataView>()[0].node.GetType().GetAttribute<YouTubeVideoAttribute>().WatchVideo(_graphView.transform);
	}

	private ContextMenuAction _CreateGoToAction(string name, Action<ContextMenuContext> action, Func<ContextMenuContext, bool> inContext)
	{
		return new ContextMenuAction("View/Go To Corresponding " + name + "…", "Navigation", action, new HotKey((KeyModifiers)0, KeyCode.G), inContext);
	}

	private void Awake()
	{
		graphView.onContextDirty.AddListener(base._MarkContextDirty);
	}

	protected override IEnumerable<ContextMenuAction> _GetActions()
	{
		foreach (ContextMenuAction item in _GetAddNodeContextActions())
		{
			yield return item;
		}
		yield return new ContextMenuAction("Add Node/Add Preset Node…", null, _AddReferencedNodeAsPreset, new HotKey(KeyModifiers.Shift, KeyCode.P), _AddReferencedNodeInContext);
		yield return new ContextMenuAction("Add Node/Add Sub Graph Reference…", null, _AddReferencedNode, new HotKey(KeyModifiers.Shift, KeyCode.O), _AddReferencedNodeInContext);
		yield return new ContextMenuAction("To Sub Graph", "Main", _CreateSubGraphFromNodes, new HotKey(KeyModifiers.Shift, KeyCode.G), _CreateSubGraphFromNodesInContext);
		yield return new ContextMenuAction("Flatten Sub Graph", "Main", _FlattenSubGraphs, new HotKey(KeyModifiers.Shift, KeyCode.F), _FlattenSubGraphsInContext);
		yield return new ContextMenuAction("Break Sub Graph Reference", "Main", _BreakReference, null, _BreakReferenceInContext);
		yield return new ContextMenuAction("Edit Sub Graph Reference Tags", "Main", _EditSubGraphReferenceTags, null, _BreakReferenceInContext);
		yield return new ContextMenuAction("Auto Connect Nodes", "Connections", _AutoConnect, new HotKey(KeyModifiers.Shift, KeyCode.X), _AutoConnectInContext);
		yield return new ContextMenuAction("Route Connections", "Connections", _RouteConnections, new HotKey(KeyModifiers.Shift, KeyCode.F), _RouteConnectionsInContext);
		yield return new ContextMenuAction("Route Connections Through Node", "Connections", _RouteConnectionsThroughNode, new HotKey(KeyModifiers.Shift | KeyModifiers.Control, KeyCode.R), _RouteConnectionsThroughNodeInContext);
		yield return new ContextMenuAction("Copy", "Serialize", _Copy, new HotKey(KeyModifiers.Control, KeyCode.C), _CopyInContext);
		yield return new ContextMenuAction("Paste", "Serialize", _Paste, new HotKey(KeyModifiers.Control, KeyCode.V), _PasteInContext);
		yield return new ContextMenuAction("Paste Replace", "Serialize", _PasteOver, new HotKey(KeyModifiers.Shift, KeyCode.V), _PasteOverInContext);
		yield return new ContextMenuAction("Duplicate", "Serialize", _Duplicate, new HotKey(KeyModifiers.Control, KeyCode.D), _CopyInContext);
		yield return new ContextMenuAction("Copy Values", "Serialize", _CopyValues, null, _CopyValuesInContext);
		yield return new ContextMenuAction("Paste Values", "Serialize", _PasteValues, null, _PasteValuesInContext);
		yield return new ContextMenuAction("Undo", "History", _Undo, new HotKey(KeyModifiers.Shift, KeyCode.Z), _UndoInContext);
		yield return new ContextMenuAction("Redo", "History", _Redo, new HotKey(KeyModifiers.Shift, KeyCode.Y), _RedoInContext);
		yield return new ContextMenuAction("Delete", "Destroy", _Delete, new HotKey((KeyModifiers)0, KeyCode.Delete), _DeleteInContext);
		yield return new ContextMenuAction("Remove Nodes Only", "Destroy", _Remove, new HotKey((KeyModifiers)0, KeyCode.Backspace), _RemoveInContext);
		yield return new ContextMenuAction("Remove Connections", "Destroy", _Delete, new HotKey((KeyModifiers)0, KeyCode.Backspace), _RemoveConnectionsInContext);
		yield return new ContextMenuAction("Remove Connections Only", "Destroy", _DeleteConnections, new HotKey(KeyModifiers.Shift, KeyCode.Backspace), _DeleteConnectionsInContext);
		yield return new ContextMenuAction("View/Auto Layout", "Automation", delegate
		{
			_graph.AutoLayout(autoLayoutHorizontalSpacing, autoLayoutVerticalSpacing, autoLayoutRelaxIterations, autoLayoutSpringIterations, autoLayoutSpringConstant, autoLayoutSpringDampening);
		}, new HotKey(KeyModifiers.Shift, KeyCode.L), (ContextMenuContext context) => graphView.graph.nodeCount > 0);
		yield return new ContextMenuAction("View/Parent Graph", "Navigation", delegate
		{
			graphView.ViewParentGraph();
		}, new HotKey((KeyModifiers)0, KeyCode.Mouse3), (ContextMenuContext context) => graphView.HasParentGraph());
		yield return new ContextMenuAction("View/Recent Child Graph", "Navigation", delegate
		{
			graphView.ViewRecentChildGraph();
		}, new HotKey((KeyModifiers)0, KeyCode.Mouse4), (ContextMenuContext context) => graphView.HasRecentChildGraph());
		yield return _CreateGoToAction("Warp Exit", _GoToWarpExit, _GoToWarpExitInContext);
		yield return _CreateGoToAction("Warp Enter", _GoToWarpEnter, _GoToWarpEnterInContext);
		yield return _CreateGoToAction("Flag Set", _GoToFlagSet, _GoToFlagSetInContext);
		yield return _CreateGoToAction("Flag Check", _GoToFlagCheck, _GoToFlagCheckInContext);
		yield return _CreateGoToAction("Event Trigger", _GoToEventTrigger, _GoToEventTriggerInContext);
		yield return new ContextMenuAction("View/Search Nodes", "Navigation", _SearchNodes, new HotKey((KeyModifiers)0, KeyCode.T), _SearchNodesInContext);
		yield return new ContextMenuAction("View/Search Nodes Of Type", "Navigation", _SearchNodesOfType, new HotKey((KeyModifiers)0, KeyCode.T), _SearchNodesOfTypeInContext);
		yield return new ContextMenuAction("View/Center View On Selection", "View", delegate
		{
			graphView.slideRect.CenterContentAtPosition(graphView.selectableGroup.GetSelectedCentroidWorldSpace());
		}, new HotKey((KeyModifiers)0, KeyCode.V));
		yield return new ContextMenuAction("Select All", "Automation", _SelectAll, new HotKey(KeyModifiers.Shift, KeyCode.A));
		yield return new ContextMenuAction(PRESET + "Create Referenced Sub Graph", null, _CreateNewReferencedNode, null, _CreateNewReferencedNodeInContext);
		yield return new ContextMenuAction(PRESET + "Revert Referenced Sub Graph Changes", null, _RevertSubGraphRefChanges, null, _RevertSubGraphRefChangesInContext);
		yield return new ContextMenuAction("Save Game Data…", "Other", _SaveGameData, new HotKey((KeyModifiers)0, KeyCode.F1), _SaveGameDataInContext);
		yield return new ContextMenuAction("Watch Tutorial For Node…", "Help", _WatchTutorialVideo, new HotKey(KeyCode.F9), _WatchTutorialVideoInContext);
	}
}
