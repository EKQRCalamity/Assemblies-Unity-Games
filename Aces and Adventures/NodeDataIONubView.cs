using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PointerDrag3D), typeof(PointerDrop3D))]
public class NodeDataIONubView : MonoBehaviour
{
	private static GameObject _Blueprint;

	private static GameObject _NewConnectionBlueprint;

	[SerializeField]
	private StringEvent _OnTextChange;

	[SerializeField]
	private ColorEvent _OnTintChange;

	private NewNodeConnectionView _newConnection;

	private NodeDataView _nodeView;

	private static GameObject Blueprint
	{
		get
		{
			if (!_Blueprint)
			{
				return _Blueprint = Resources.Load<GameObject>("UI/NodeGraph/NodeData/IO/NodeDataIONubView");
			}
			return _Blueprint;
		}
	}

	private static GameObject NewConnectionBlueprint
	{
		get
		{
			if (!_NewConnectionBlueprint)
			{
				return _NewConnectionBlueprint = Resources.Load<GameObject>("UI/NodeGraph/NodeData/IO/NewNodeConnectionView");
			}
			return _NewConnectionBlueprint;
		}
	}

	public StringEvent OnTextChange => _OnTextChange ?? (_OnTextChange = new StringEvent());

	public ColorEvent OnTintChange => _OnTintChange ?? (_OnTintChange = new ColorEvent());

	public NodeDataView nodeView
	{
		get
		{
			return _nodeView;
		}
		private set
		{
			if (!(_nodeView == value))
			{
				if (_nodeView != null)
				{
					_nodeView.node.graph.rootGraph.onConnectionAdded -= _OnConnectionAdded;
					_nodeView.node.graph.rootGraph.onConnectionRemoved -= _OnConnectionRemoved;
				}
				_nodeView = value;
				if (_nodeView != null)
				{
					_nodeView.node.graph.rootGraph.onConnectionAdded += _OnConnectionAdded;
					_nodeView.node.graph.rootGraph.onConnectionRemoved += _OnConnectionRemoved;
				}
			}
		}
	}

	public uint nodeId
	{
		get
		{
			if (!nodeView || !nodeView.node)
			{
				return 0u;
			}
			return nodeView.node;
		}
	}

	public bool hasConnection
	{
		get
		{
			if (nodeId != 0)
			{
				if (ioType != 0)
				{
					return nodeView.node.HasOutput(outputType, checkIfConnectionExists: true);
				}
				return nodeView.node.HasInput(checkIfConnectionExists: true);
			}
			return false;
		}
	}

	public IOType ioType { get; private set; }

	public NodeData.Type outputType { get; private set; }

	public static NodeDataIONubView Create(NodeDataView nodeDataView, IOType ioType, NodeData.Type type, Transform parent)
	{
		return Pools.Unpool(Blueprint, parent).GetComponent<NodeDataIONubView>()._SetData(nodeDataView, ioType, type);
	}

	private void Awake()
	{
		_RegisterPointerEvents();
	}

	private void OnDisable()
	{
		nodeView = null;
	}

	private void _RegisterPointerEvents()
	{
		PointerDrag3D component = GetComponent<PointerDrag3D>();
		component.OnBegin.AddListener(_OnBeginDrag);
		component.OnDragged.AddListener(_OnDrag);
		component.OnEnd.AddListener(_OnEndDrag);
		GetComponent<PointerDrop3D>().OnDropped.AddListener(_OnDrop);
	}

	private void _OnBeginDrag(PointerEventData eventData)
	{
		if ((bool)_newConnection)
		{
			_newConnection.gameObject.SetActive(value: false);
		}
		_newConnection = Pools.Unpool(NewConnectionBlueprint, nodeView.graphView.transform).GetComponent<NewNodeConnectionView>().SetData(this);
	}

	private void _OnDrag(PointerEventData eventData)
	{
		if ((bool)_newConnection)
		{
			_newConnection.end.transform.position = base.transform.GetPlane(PlaneAxes.XY).ClosestPointOnPlane(eventData.pressEventCamera.ScreenPointToRay(Input.mousePosition));
		}
	}

	private void _OnEndDrag(PointerEventData eventData)
	{
		if ((bool)_newConnection)
		{
			_newConnection.gameObject.SetActive(value: false);
		}
	}

	private void _OnDrop(PointerEventData eventData)
	{
		ConnectWith(eventData.pointerDrag.GetComponent<NodeDataIONubView>());
	}

	private void _UpdateTint()
	{
		if (hasConnection)
		{
			OnTintChange.Invoke(Color.white);
		}
		else
		{
			OnTintChange.Invoke((ioType == IOType.Input) ? new Color(0.6f, 0.6f, 1f, 1f) : outputType.GetNubTint());
		}
	}

	private void _OnConnectionAdded(NodeGraph graph, NodeDataConnection connection)
	{
		if (connection.Contains(nodeId))
		{
			_UpdateTint();
		}
	}

	private void _OnConnectionRemoved(NodeGraph graph, NodeDataConnection connection)
	{
		if (connection.Contains(nodeId))
		{
			_UpdateTint();
		}
	}

	private NodeDataIONubView _SetData(NodeDataView nodeDataView, IOType ioType, NodeData.Type type)
	{
		nodeView = nodeDataView;
		this.ioType = ioType;
		outputType = type;
		OnTextChange.Invoke(outputType.GetText());
		_UpdateTint();
		return this;
	}

	public bool CanConnectWith(NodeDataIONubView ioNub)
	{
		if ((bool)ioNub && ioType != ioNub.ioType)
		{
			return nodeView != ioNub.nodeView;
		}
		return false;
	}

	public bool ConnectWith(NodeDataIONubView ioNub)
	{
		if (!CanConnectWith(ioNub))
		{
			return false;
		}
		NodeDataIONubView nodeDataIONubView = ((ioType == IOType.Output) ? this : ioNub);
		NodeDataIONubView nodeDataIONubView2 = ((ioType == IOType.Input) ? this : ioNub);
		nodeView.node.graph.AddConnection(new NodeDataConnection(nodeDataIONubView.nodeView, nodeDataIONubView2.nodeView, nodeDataIONubView.outputType));
		return true;
	}
}
