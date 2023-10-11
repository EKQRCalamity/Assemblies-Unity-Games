using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UndoHistory))]
public class NodeGraphView : MonoBehaviour
{
	private static GameObject _NodeGraphButtonBlueprint;

	private static Dictionary<NodeGraph, NodeGraphView> _ViewMap;

	[Header("Layers")]
	public UIGridSnapper contentGrid;

	public RectTransform connectionContainer;

	public RectTransform nodeContainer;

	public RectTransform nodeGraphButtonContainer;

	[SerializeField]
	protected UnityEvent _onContextDirty;

	[SerializeField]
	protected BoolEvent _onIsReferenceGraphChange;

	[SerializeField]
	protected BoolEvent _onHasNodeChange;

	[SerializeField]
	protected StringEvent _onContextTipChange;

	private NodeGraph _graph;

	private NodeGraph _rootGraph;

	private Dictionary<uint, NodeDataView> _nodes;

	private Dictionary<uint, NodeDataConnectionView> _connections;

	private Stack<NodeGraph> _viewGraphStack;

	private UndoHistory _undoHistory;

	private Dictionary<NodeGraph, Vector4> _lastGraphPositions;

	private bool _isDirty;

	private SlideRect _slideRect;

	private SelectableGroup _selectableGroup;

	private static GameObject NodeGraphButtonBlueprint
	{
		get
		{
			if (!_NodeGraphButtonBlueprint)
			{
				return _NodeGraphButtonBlueprint = Resources.Load<GameObject>("UI/NodeGraph/NodeGraphButton");
			}
			return _NodeGraphButtonBlueprint;
		}
	}

	private static Dictionary<NodeGraph, NodeGraphView> ViewMap => _ViewMap ?? (_ViewMap = new Dictionary<NodeGraph, NodeGraphView>());

	public NodeGraph graph
	{
		get
		{
			NodeGraph nodeGraph = _graph;
			if (nodeGraph == null)
			{
				NodeGraph obj = new NodeGraph
				{
					name = "Base Graph"
				};
				NodeGraph nodeGraph2 = obj;
				graph = obj;
				nodeGraph = nodeGraph2;
			}
			return nodeGraph;
		}
		set
		{
			_OnGraphChange(value);
		}
	}

	public NodeData parentNode
	{
		get
		{
			if (_graph == null)
			{
				return null;
			}
			return _graph.parentNode;
		}
		set
		{
			if (_graph == null || !NodeGraph.ParentNodeEqualityComparer.Default.Equals(_graph.parentNode, value))
			{
				if (_graph != null && _graph.parentNode != null && (value != null || !_graph.isRootGraph))
				{
					_graph.parentNode.OnCloseSubGraphView(value);
				}
				if (value != null)
				{
					value = value.OnOpenSubGraphView((_graph != null) ? _graph.parentNode : null);
				}
				graph = ((value != null) ? value.subGraph : ((_graph != null) ? _graph.rootGraph : null));
			}
		}
	}

	protected NodeGraph rootGraph
	{
		get
		{
			return _rootGraph ?? graph.rootGraph;
		}
		set
		{
			_OnRootGraphChange(value);
		}
	}

	protected Dictionary<uint, NodeDataView> nodes => _nodes ?? (_nodes = new Dictionary<uint, NodeDataView>());

	protected Dictionary<uint, NodeDataConnectionView> connections => _connections ?? (_connections = new Dictionary<uint, NodeDataConnectionView>());

	protected Stack<NodeGraph> viewGraphStack => _viewGraphStack ?? (_viewGraphStack = new Stack<NodeGraph>());

	public UnityEvent onContextDirty => _onContextDirty ?? (_onContextDirty = new UnityEvent());

	public BoolEvent onIsReferenceGraphChange => _onIsReferenceGraphChange ?? (_onIsReferenceGraphChange = new BoolEvent());

	public BoolEvent onHasNodeChange => _onHasNodeChange ?? (_onHasNodeChange = new BoolEvent());

	public StringEvent onContextTipChange => _onContextTipChange ?? (_onContextTipChange = new StringEvent());

	public UndoHistory undoHistory => this.CacheComponent(ref _undoHistory);

	public Canvas rootCanvas => base.gameObject.GetRootComponent<Canvas>();

	public Transform rootParent => rootCanvas.transform;

	public SlideRect slideRect => this.CacheComponentInChildren(ref _slideRect);

	public SelectableGroup selectableGroup => this.CacheComponentInChildren(ref _selectableGroup);

	private Dictionary<NodeGraph, Vector4> lastGraphPositions => _lastGraphPositions ?? (_lastGraphPositions = new Dictionary<NodeGraph, Vector4>(ReferenceEqualityComparer<NodeGraph>.Default));

	public static NodeGraphView GetView(NodeGraph graph)
	{
		if (!ViewMap.ContainsKey(graph))
		{
			return null;
		}
		return ViewMap[graph];
	}

	public static NodeGraphView GetActiveView()
	{
		return ViewMap.Values.FirstOrDefault((NodeGraphView view) => view.isActiveAndEnabled);
	}

	private void Awake()
	{
		contentGrid.onGridItemPositionUpdate.AddListener(_OnNodeDataViewPositionChange);
		onContextDirty.AddListener(_SetDirty);
	}

	private void Update()
	{
		if (_isDirty && !(_isDirty = false))
		{
			onHasNodeChange.Invoke(graph.nodeCount > 0);
			if (graph.nodeCount == 0)
			{
				onContextTipChange.Invoke("Right Click to Add Node");
			}
			else
			{
				onContextTipChange.Invoke(graph.GetContextTip() ?? "Middle-Mouse Drag to Pan");
			}
		}
	}

	private void OnDisable()
	{
		if (graph != null)
		{
			ViewMap.Remove(graph);
			graph = null;
		}
	}

	private void _SetDirty()
	{
		_isDirty = true;
	}

	private void _OnNodeDataViewPositionChange(Transform nodeDataViewTransform)
	{
		nodeDataViewTransform.GetComponent<NodeDataView>().node.position = nodeDataViewTransform.localPosition.Project(AxisType.Z);
	}

	private Vector4? _GetLastGraphPosition(NodeGraph graphToGetLastPositionOf)
	{
		if (!lastGraphPositions.ContainsKey(graphToGetLastPositionOf))
		{
			return null;
		}
		return lastGraphPositions[graphToGetLastPositionOf];
	}

	private void _OnGraphChange(NodeGraph value)
	{
		if (_graph == value)
		{
			return;
		}
		if (_graph != null && _graph.parentNode != null && value == null)
		{
			_graph.parentNode.OnCloseSubGraphView(null);
		}
		if (undoHistory.acceptingEntries)
		{
			undoHistory.AddEntry(new UndoRecordActiveGraphChange(this, _graph, value));
		}
		undoHistory.SuppressEntries(this);
		if (_graph != null)
		{
			lastGraphPositions[_graph] = slideRect.content.transform.position.ToVector4(slideRect.content.transform.localScale.x);
			_RemoveNodeGraphButtons();
			_graph.SignalDestructionEvents();
			if (value != null && value.IsParentOf(_graph))
			{
				viewGraphStack.Push(_graph);
			}
			ViewMap.Remove(_graph);
		}
		_graph = value;
		if (_graph != null)
		{
			rootGraph = _graph.rootGraph;
			PoolKeepItemDictionaryHandle<NodeData, Vector2> poolKeepItemDictionaryHandle = _graph.SignalConstructionEvents();
			_CreateNodeGraphButtons();
			ViewMap.Add(_graph, this);
			contentGrid.ForceUpdate();
			Vector4? vector = _GetLastGraphPosition(_graph);
			if (vector.HasValue)
			{
				slideRect.SetContentScale(vector.Value.w);
			}
			slideRect.SetContentPosition(vector);
			if (!vector.HasValue)
			{
				slideRect.CenterContentAtPosition(_GetRootNodeCentroid());
			}
			foreach (KeyValuePair<NodeData, Vector2> item in poolKeepItemDictionaryHandle)
			{
				item.Key.position = item.Value;
			}
		}
		else
		{
			rootGraph = null;
		}
		undoHistory.AllowEntries(this);
		onContextDirty.Invoke();
		onIsReferenceGraphChange.Invoke(_graph != null && _graph.isReferencedGraph);
	}

	private void _OnRootGraphChange(NodeGraph value)
	{
		if (_rootGraph != value)
		{
			if (_rootGraph != null)
			{
				_rootGraph.onNodeAdded -= _OnNodeAdded;
				_rootGraph.onNodeRemoved -= _OnNodeRemoved;
				_rootGraph.onConnectionAdded -= _OnConnectionAdded;
				_rootGraph.onConnectionRemoved -= _OnConnectionRemoved;
				_rootGraph.onNodePositionChanged -= _OnNodePositionChanged;
				_rootGraph.onNodePastedValues -= _OnNodePastedValues;
			}
			_rootGraph = value;
			if (_rootGraph != null)
			{
				_rootGraph.onNodeAdded += _OnNodeAdded;
				_rootGraph.onNodeRemoved += _OnNodeRemoved;
				_rootGraph.onConnectionAdded += _OnConnectionAdded;
				_rootGraph.onConnectionRemoved += _OnConnectionRemoved;
				_rootGraph.onNodePositionChanged += _OnNodePositionChanged;
				_rootGraph.onNodePastedValues += _OnNodePastedValues;
			}
		}
	}

	private void _OnNodeAdded(NodeGraph ownerGraph, NodeData node)
	{
		if (undoHistory.acceptingEntries)
		{
			undoHistory.AddEntry(new UndoRecordNodeData(ownerGraph, node, AUndoRecord.EntryType.Add));
		}
		onContextDirty.Invoke();
		if (ownerGraph == graph)
		{
			nodes.Add(node, contentGrid.AddGridItem(NodeDataView.Create(this, node, nodeContainer)));
		}
	}

	private void _OnNodeRemoved(NodeGraph ownerGraph, NodeData node)
	{
		if (undoHistory.acceptingEntries)
		{
			undoHistory.AddEntry(new UndoRecordNodeData(ownerGraph, node, AUndoRecord.EntryType.Remove));
		}
		onContextDirty.Invoke();
		if (ownerGraph == graph)
		{
			contentGrid.RemoveGridItem(nodes[node]);
			nodes[node].gameObject.SetActive(value: false);
			nodes.Remove(node);
		}
	}

	private void _OnConnectionAdded(NodeGraph ownerGraph, NodeDataConnection connection)
	{
		if (undoHistory.acceptingEntries)
		{
			undoHistory.AddEntry(new UndoRecordNodeDataConnection(ownerGraph, connection, AUndoRecord.EntryType.Add));
		}
		onContextDirty.Invoke();
		if (ownerGraph == graph)
		{
			connections.Add(connection, NodeDataConnectionView.Create(this, connection, connectionContainer));
		}
	}

	private void _OnConnectionRemoved(NodeGraph ownerGraph, NodeDataConnection connection)
	{
		if (undoHistory.acceptingEntries)
		{
			undoHistory.AddEntry(new UndoRecordNodeDataConnection(ownerGraph, connection, AUndoRecord.EntryType.Remove));
		}
		onContextDirty.Invoke();
		if (ownerGraph == graph)
		{
			connections[connection].gameObject.SetActive(value: false);
			connections.Remove(connection);
		}
	}

	private void _OnNodePositionChanged(NodeGraph ownerGraph, NodeData node, Vector2 previousPosition)
	{
		if (ownerGraph == graph)
		{
			GetNode(node).transform.localPosition = node.position.Unproject(AxisType.Z);
			contentGrid.SetDirty();
		}
	}

	private void _OnNodePastedValues(NodeGraph ownerGraph, NodeData copiedValue, NodeData nodePastedOver, byte[] nodePastedOverData)
	{
		if (undoHistory.acceptingEntries)
		{
			undoHistory.AddEntry(new UndoRecordNodeDataPastedValue(copiedValue, nodePastedOver, nodePastedOverData));
		}
		onContextDirty.Invoke();
		GetNode(nodePastedOver).Refresh(nodePastedOver);
	}

	private void _CreateNodeGraphButtons()
	{
		using PoolKeepItemStackHandle<NodeGraph> poolKeepItemStackHandle = Pools.UseKeepItemStack<NodeGraph>();
		for (NodeGraph parentGraph = graph; parentGraph != null; parentGraph = parentGraph.parentGraph)
		{
			poolKeepItemStackHandle.Push(parentGraph);
		}
		foreach (NodeGraph item in poolKeepItemStackHandle.value)
		{
			NodeData closureParentNode = item.parentNode;
			GameObject gameObject = Pools.Unpool(NodeGraphButtonBlueprint, nodeGraphButtonContainer);
			gameObject.GetComponentInChildren<TextMeshProUGUI>().text = item.name;
			if (!NodeGraph.ParentNodeEqualityComparer.Default.Equals(closureParentNode, parentNode))
			{
				gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate
				{
					parentNode = closureParentNode;
				});
			}
		}
	}

	private void _RemoveNodeGraphButtons()
	{
		foreach (Button item in nodeGraphButtonContainer.gameObject.GetComponentsInChildrenPooled<Button>())
		{
			item.onClick.RemoveAllListeners();
			item.gameObject.SetActive(value: false);
		}
	}

	private Vector3? _GetRootNodeCentroid()
	{
		return (from n in _graph.GetRootNodes().AsEnumerable()
			select GetNode(n).transform.position).CentroidNullable();
	}

	public NodeDataView GetNode(uint nodeId)
	{
		return nodes[nodeId];
	}

	public NodeDataConnectionView GetConnection(uint connectionId)
	{
		return connections[connectionId];
	}

	public bool HasParentGraph()
	{
		return graph.parentGraph != null;
	}

	public void ViewParentGraph()
	{
		if (HasParentGraph())
		{
			parentNode = graph.parentGraph.parentNode;
		}
	}

	public bool HasRecentChildGraph()
	{
		return viewGraphStack.Count > 0;
	}

	public void ViewRecentChildGraph()
	{
		while (viewGraphStack.Count > 0)
		{
			NodeGraph nodeGraph = viewGraphStack.Pop();
			if (nodeGraph.IsChildOf(graph))
			{
				parentNode = nodeGraph.parentNode;
				break;
			}
		}
	}

	public void ViewNode(NodeData node)
	{
		if (node != null && node.graph != null)
		{
			if (node.graph.parentNode != null)
			{
				parentNode = node.graph.parentNode;
			}
			else
			{
				graph = node.graph;
			}
			selectableGroup.ClearSelected();
			GetNode(node).selectable.SetSelected(isSelected: true);
			slideRect.CenterContentAtPosition(GetNode(node).transform.position);
		}
	}

	public void ViewGraph(NodeGraph graph)
	{
		if (graph != null)
		{
			if (graph.parentNode != null)
			{
				parentNode = graph.parentNode;
			}
			else
			{
				this.graph = graph;
			}
		}
	}

	public void PrepareForSave()
	{
		if (_graph != null && _graph.parentNode != null)
		{
			_graph.parentNode.OnCloseSubGraphView(null);
			_graph.parentNode.OnOpenSubGraphView(null);
		}
	}
}
