using UnityEngine;

[RequireComponent(typeof(UIBezierSpline), typeof(UIBezierSplineConnector), typeof(SelectableItem))]
public class NodeDataConnectionView : MonoBehaviour, IInspectable
{
	private static GameObject _Blueprint;

	[SerializeField]
	protected IntEvent _onOutputTypeChange;

	private NodeDataConnection _connection;

	private UIBezierSpline _spline;

	private UIBezierSplineConnector _splineConnector;

	private SelectableItem _selectable;

	protected static GameObject Blueprint
	{
		get
		{
			if (!_Blueprint)
			{
				return _Blueprint = Resources.Load<GameObject>("UI/NodeGraph/NodeData/IO/NodeDataConnectionView");
			}
			return _Blueprint;
		}
	}

	public IntEvent onOutputTypeChange => _onOutputTypeChange ?? (_onOutputTypeChange = new IntEvent());

	public NodeGraphView graphView { get; private set; }

	public NodeDataConnection connection
	{
		get
		{
			return _connection;
		}
		private set
		{
			if (SetPropertyUtility.SetObject(ref _connection, value))
			{
				_OnConnectionChange();
			}
		}
	}

	private UIBezierSpline spline => this.CacheComponent(ref _spline);

	private UIBezierSplineConnector connector => this.CacheComponent(ref _splineConnector);

	public SelectableItem selectable => this.CacheComponent(ref _selectable);

	public string inspectedName
	{
		get
		{
			return "Connection";
		}
		set
		{
		}
	}

	public object inspectedValue => connection;

	public UICategorySet[] uiCategorySets => UICategorySet.Defaults;

	public static NodeDataConnectionView Create(NodeGraphView graphView, NodeDataConnection connection, Transform parent = null)
	{
		return Pools.Unpool(Blueprint, parent).GetComponent<NodeDataConnectionView>()._SetData(graphView, connection);
	}

	private void OnDisable()
	{
		_connection = null;
	}

	private NodeDataConnectionView _SetData(NodeGraphView graphView, NodeDataConnection connection)
	{
		this.graphView = graphView;
		this.connection = connection;
		onOutputTypeChange.Invoke((int)connection.outputType);
		return this;
	}

	private void _OnConnectionChange()
	{
		connector.start.SetData(graphView.GetNode(connection.outputNodeId).GetIONub(this).transform);
		connector.end.SetData(graphView.GetNode(connection.inputNodeId).GetIONub(this).transform, AxisType.X, negateAxis: true);
	}

	public static implicit operator NodeDataConnection(NodeDataConnectionView nodeConnectionView)
	{
		return nodeConnectionView.connection;
	}
}
