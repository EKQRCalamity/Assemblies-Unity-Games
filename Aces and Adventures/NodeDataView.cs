using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SelectableItem), typeof(PointerDrop3D))]
public class NodeDataView : MonoBehaviour, IInspectable, IShowCanDrag
{
	private static Dictionary<NodeDataSpriteType, Sprite> _NodeDataSprites;

	private static Dictionary<NodeDataIconType, Sprite> _NodeDataIcons;

	private static GameObject _Blueprint;

	private static Vector2? _Size;

	[Header("Events====================================================================================================================")]
	[SerializeField]
	protected StringEvent _OnNameChange;

	[SerializeField]
	protected SpriteEvent _OnSpriteChange;

	[SerializeField]
	protected SpriteEvent _OnIconChange;

	[SerializeField]
	protected ColorEvent _OnTintChange;

	public RectTransform inputNubContainer;

	public RectTransform outputNubContainer;

	private SelectableItem _selectable;

	private PointerDrop3D _pointerDrop;

	private NodeData _node;

	private Vector3 _dragOffset;

	private List<NodeDataIONubView> _ioNubs = new List<NodeDataIONubView>();

	private List<NodeDataConnectionView> _highlightedConnections = new List<NodeDataConnectionView>();

	private static Dictionary<NodeDataSpriteType, Sprite> NodeDataSprites => _NodeDataSprites ?? (_NodeDataSprites = ReflectionUtil.CreateEnumResourceMap<NodeDataSpriteType, Sprite>("UI/NodeGraph/NodeData/NodeDataSpriteType"));

	private static Dictionary<NodeDataIconType, Sprite> NodeDataIcons => _NodeDataIcons ?? (_NodeDataIcons = ReflectionUtil.CreateEnumResourceMap<NodeDataIconType, Sprite>("UI/NodeGraph/NodeData/NodeDataIconType"));

	private static GameObject Blueprint
	{
		get
		{
			if (!_Blueprint)
			{
				return _Blueprint = Resources.Load<GameObject>("UI/NodeGraph/NodeData/NodeDataView");
			}
			return _Blueprint;
		}
	}

	public static Vector2 Size
	{
		get
		{
			Vector2? size = _Size;
			if (!size.HasValue)
			{
				Vector2? vector = (_Size = (Blueprint.transform as RectTransform).rect.size);
				return vector.Value;
			}
			return size.GetValueOrDefault();
		}
	}

	public StringEvent OnNameChange => _OnNameChange ?? (_OnNameChange = new StringEvent());

	public SpriteEvent OnSpriteChange => _OnSpriteChange ?? (_OnSpriteChange = new SpriteEvent());

	public SpriteEvent OnIconChange => _OnIconChange ?? (_OnIconChange = new SpriteEvent());

	public ColorEvent OnTintChange => _OnTintChange ?? (_OnTintChange = new ColorEvent());

	public SelectableItem selectable => this.CacheComponent(ref _selectable);

	public PointerDrop3D pointerDrop => this.CacheComponent(ref _pointerDrop);

	public NodeData node
	{
		get
		{
			return _node;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _node, value))
			{
				_OnNodeDataChange();
			}
		}
	}

	public NodeGraphView graphView { get; private set; }

	public string inspectedName
	{
		get
		{
			return node.name;
		}
		set
		{
			OnNameChange.Invoke(value);
			NodeDataRef nodeDataRef = _node as NodeDataRef;
			if (nodeDataRef == null || _node.graph == null || nodeDataRef.nodeGraphRef == null)
			{
				return;
			}
			foreach (NodeDataRef item in from n in _node.graph.GetNodes().AsEnumerable().OfType<NodeDataRef>()
				where n != nodeDataRef && nodeDataRef.nodeGraphRef.Equals(n.nodeGraphRef)
				select n)
			{
				graphView.GetNode(item.id).OnNameChange.Invoke(value);
			}
		}
	}

	public object inspectedValue => node;

	public UICategorySet[] uiCategorySets => node.uiCategorySets;

	public static NodeDataView Create(NodeGraphView graphView, NodeData nodeData, Transform parent = null)
	{
		NodeDataView nodeDataView = Pools.Unpool(Blueprint, parent).GetComponent<NodeDataView>()._SetData(graphView, nodeData);
		nodeDataView.transform.localPosition = nodeData.position.Unproject(AxisType.Z);
		return nodeDataView;
	}

	private void Awake()
	{
		_RegisterPointerEvents();
	}

	private void OnDisable()
	{
		node = null;
	}

	private void _RegisterPointerEvents()
	{
		selectable.onSelectedChange.AddListener(_OnSelectedChange);
		selectable.onDoubleClick.AddListener(_OnDoubleClick);
		selectable.pointerDrag.OnBegin.AddListener(_OnBeginDrag);
		selectable.pointerDrag.OnDragged.AddListener(_OnDrag);
		selectable.pointerDrag.OnEnd.AddListener(_OnEndDrag);
		pointerDrop.OnDropped.AddListener(_OnDrop);
	}

	private void _OnSelectedChange(bool selected)
	{
		if (selected)
		{
			foreach (NodeDataConnection connection in node.GetConnections())
			{
				_highlightedConnections.Add(graphView.GetConnection(connection));
			}
		}
		foreach (NodeDataConnectionView highlightedConnection in _highlightedConnections)
		{
			highlightedConnection.selectable.secondaryHighlights += selected.ToInt(1, -1);
		}
		if (!selected)
		{
			_highlightedConnections.Clear();
		}
	}

	private void _OnDoubleClick(PointerEventData eventData)
	{
		if (node.IsSubGraph())
		{
			graphView.parentNode = node;
		}
	}

	private void _OnBeginDrag(PointerEventData eventData)
	{
		_dragOffset = base.transform.position - graphView.transform.GetPlane(PlaneAxes.XY).ClosestPointOnPlane(eventData.pressEventCamera.ScreenPointToRay(Input.mousePosition));
	}

	private void _OnDrag(PointerEventData eventData)
	{
		base.transform.position = graphView.transform.GetPlane(PlaneAxes.XY).ClosestPointOnPlane(eventData.pressEventCamera.ScreenPointToRay(Input.mousePosition)) + _dragOffset;
	}

	private void _OnEndDrag(PointerEventData eventData)
	{
		graphView.contentGrid.SetDirty();
	}

	private void _OnDrop(PointerEventData eventData)
	{
		NodeDataIONubView component = eventData.pointerDrag.GetComponent<NodeDataIONubView>();
		if (!component)
		{
			return;
		}
		foreach (NodeDataIONubView ioNub in _ioNubs)
		{
			if (component.ConnectWith(ioNub))
			{
				break;
			}
		}
	}

	private NodeDataView _SetData(NodeGraphView graphView, NodeData data)
	{
		this.graphView = graphView;
		node = data;
		return this;
	}

	private void _OnNodeDataChange()
	{
		if (node == null)
		{
			foreach (NodeDataIONubView ioNub in _ioNubs)
			{
				ioNub.gameObject.SetActive(value: false);
			}
			_ioNubs.Clear();
			return;
		}
		OnNameChange.Invoke(node.name);
		if ((bool)NodeDataSprites[node.spriteType])
		{
			OnSpriteChange.Invoke(NodeDataSprites[node.spriteType]);
		}
		if ((bool)NodeDataIcons[node.iconType])
		{
			OnIconChange.Invoke(NodeDataIcons[node.iconType]);
		}
		OnTintChange.Invoke(node.iconType.GetTint().SetAlpha(node.isHidden.ToFloat(0.6f, 1f)));
		_ioNubs.Add(NodeDataIONubView.Create(this, IOType.Input, NodeData.Type.True, inputNubContainer));
		NodeData.Type[] output = node.output;
		foreach (NodeData.Type type in output)
		{
			_ioNubs.Add(NodeDataIONubView.Create(this, IOType.Output, type, outputNubContainer));
		}
	}

	public NodeDataIONubView GetIONub(NodeDataConnection connection)
	{
		IOType? iOType = (((uint)this == connection.outputNodeId) ? new IOType?(IOType.Output) : (((uint)this == connection.inputNodeId) ? new IOType?(IOType.Input) : null));
		if (iOType.HasValue)
		{
			foreach (NodeDataIONubView ioNub in _ioNubs)
			{
				if (ioNub.ioType == iOType.Value && (ioNub.ioType == IOType.Input || ioNub.outputType == connection.outputType))
				{
					return ioNub;
				}
			}
		}
		return null;
	}

	public void Refresh(NodeData newNode = null)
	{
		NodeData objectToRefreshUIFor = _node;
		_node = newNode ?? _node;
		inspectedName = inspectedName;
		UIGeneratorType.Refresh(objectToRefreshUIFor, newNode);
	}

	public static implicit operator NodeData(NodeDataView nodeView)
	{
		return nodeView.node;
	}

	public static implicit operator uint(NodeDataView nodeView)
	{
		return nodeView.node;
	}

	public bool ShouldShowCanDrag()
	{
		return true;
	}
}
