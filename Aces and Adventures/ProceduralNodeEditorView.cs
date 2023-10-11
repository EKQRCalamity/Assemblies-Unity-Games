using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PointerDrag3D), typeof(PointerDrop3D), typeof(BubblePointerDrag3D))]
public class ProceduralNodeEditorView : ProceduralNodeView
{
	private void _OnDragInitialize(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Middle)
		{
			eventData.dragging = false;
		}
	}

	private void _OnDragged(PointerEventData eventData)
	{
		if (eventData.button != 0)
		{
			return;
		}
		Vector2 position = base.node.position;
		base.node.position = _graphView.ToVector2Position(_graphView.ToLocalPosition(eventData.pointerCurrentRaycast.worldPosition));
		if (!InputManager.I[KeyModifiers.Alt])
		{
			base.node.position = _graphView.SnapNodePositionToGrid(base.node.position);
		}
		Vector2 vector = base.node.position - position;
		if (base.graph.IsSelectedNode(base.node))
		{
			foreach (ProceduralNode selectedNode in base.graph.GetSelectedNodes())
			{
				if (selectedNode.id != base._nodeId)
				{
					selectedNode.position += vector;
				}
			}
		}
		_graphView.UpdateNodePositions();
		_graphView.SetDirty();
	}

	private void _OnPointerDrop(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			ProceduralNodeView componentInParent = eventData.pointerDrag.GetComponentInParent<ProceduralNodeView>();
			if ((object)componentInParent != null)
			{
				base.graph.CreateConnection(componentInParent.node.id, base.node.id);
			}
		}
	}

	private void _OnNodeDeleted(ProceduralNode deletedNode)
	{
		if (deletedNode.id == base._nodeId)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	protected override void Start()
	{
		base.Start();
		GetComponent<BubblePointerDrag3D>().buttonsToBubble = PointerInputButtonFlags.Middle;
		GetComponent<PointerDrag3D>().OnInitialize.AddListener(_OnDragInitialize);
		GetComponent<PointerDrag3D>().OnDragged.AddListener(_OnDragged);
		GetComponent<PointerDrop3D>().OnDropped.AddListener(_OnPointerDrop);
	}

	protected override void OnDisable()
	{
		if (base.graph != null)
		{
			base.graph.onNodeDeleted -= _OnNodeDeleted;
		}
		base.OnDisable();
	}

	public override ProceduralNodeView SetData(ProceduralMapView graphToSet, ProceduralNode nodeToSet)
	{
		base.SetData(graphToSet, nodeToSet);
		base.graph.onNodeDeleted += _OnNodeDeleted;
		return this;
	}
}
