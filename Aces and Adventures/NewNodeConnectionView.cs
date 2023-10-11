using UnityEngine;

[RequireComponent(typeof(UIBezierSplineConnector))]
public class NewNodeConnectionView : MonoBehaviour
{
	private UIBezierSplineConnector _connector;

	private NodeDataIONubView _ioNub;

	private Transform _mousePositionTransform;

	private UIBezierSplineConnector connector => this.CacheComponent(ref _connector);

	public NodeDataIONubView ioNub => _ioNub;

	public UIBezierSplineConnector.Node start
	{
		get
		{
			if (ioNub.ioType != IOType.Output)
			{
				return connector.end;
			}
			return connector.start;
		}
	}

	public UIBezierSplineConnector.Node end
	{
		get
		{
			if (ioNub.ioType != IOType.Output)
			{
				return connector.start;
			}
			return connector.end;
		}
	}

	private void Awake()
	{
		_mousePositionTransform = connector.end.transform;
	}

	private void OnDisable()
	{
		_ioNub = null;
	}

	public NewNodeConnectionView SetData(NodeDataIONubView ioNub)
	{
		_ioNub = ioNub;
		start.SetData(ioNub.transform, AxisType.X, this.ioNub.ioType == IOType.Input);
		end.SetData(_mousePositionTransform, AxisType.X, this.ioNub.ioType == IOType.Output);
		return this;
	}
}
