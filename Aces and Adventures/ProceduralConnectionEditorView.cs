using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PointerClick3D))]
public class ProceduralConnectionEditorView : ProceduralConnectionView
{
	private Vector4 _endPoints;

	private void _OnRightClick(PointerEventData eventData)
	{
		if (InputManager.I[KeyModifiers.Alt])
		{
			base.graph.DeleteConnection(base._startNodeId, base._endNodeId);
		}
	}

	private void _OnConnectionDeleted(ProceduralNode start, ProceduralNode end)
	{
		if (start.id == base._startNodeId && end.id == base._endNodeId)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void Start()
	{
		GetComponent<PointerClick3D>().OnRightClick.AddListener(_OnRightClick);
	}

	private void LateUpdate()
	{
		if (SetPropertyUtility.SetStruct(ref _endPoints, base.startNode.position.ToVector4(base.endNode.position)))
		{
			_UpdatePosition();
		}
	}

	protected override void OnDisable()
	{
		if (base.graph != null)
		{
			base.graph.onConnectionDeleted -= _OnConnectionDeleted;
		}
		base.OnDisable();
	}

	public override ProceduralConnectionView SetData(ProceduralMapView graphView, ProceduralNode startNode, ProceduralNode endNode)
	{
		base.SetData(graphView, startNode, endNode);
		base.graph.onConnectionDeleted += _OnConnectionDeleted;
		return this;
	}
}
