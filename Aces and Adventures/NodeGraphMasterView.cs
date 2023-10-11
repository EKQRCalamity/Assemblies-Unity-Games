using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Canvas))]
public class NodeGraphMasterView : MonoBehaviour
{
	private static GameObject _Blueprint;

	[SerializeField]
	protected StringEvent _onNameChange;

	[SerializeField]
	protected UnityEvent _onClose;

	private Canvas _canvas;

	private NodeGraphView _graphView;

	public static GameObject Blueprint
	{
		get
		{
			if (!_Blueprint)
			{
				return _Blueprint = Resources.Load<GameObject>("UI/NodeGraph/NodeGraphMasterView");
			}
			return _Blueprint;
		}
	}

	public StringEvent onNameChange => _onNameChange ?? (_onNameChange = new StringEvent());

	public UnityEvent onClose => _onClose ?? (_onClose = new UnityEvent());

	public Canvas canvas => this.CacheComponent(ref _canvas);

	public NodeGraphView graphView => this.CacheComponentInChildren(ref _graphView);

	private void OnDisable()
	{
		onClose.Invoke();
		if ((bool)graphView)
		{
			graphView.graph = null;
		}
	}

	public NodeGraphMasterView SetData(string name, NodeGraph nodeGraph, Camera canvasCamera, int canvasSortingOrder)
	{
		onNameChange.Invoke(name);
		graphView.graph = nodeGraph;
		canvas.worldCamera = canvasCamera;
		canvas.sortingOrder = canvasSortingOrder;
		return this;
	}
}
