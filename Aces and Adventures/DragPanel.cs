using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class DragPanel : MonoBehaviour, IInitializePotentialDragHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IShowCanDrag
{
	private const float CONNECTED_EDGE_THRESHOLD = 0.001f;

	[Range(1f, 100f)]
	public float dragAreaWidth = 10f;

	[Range(0.01f, 1f)]
	public float minDimensionX = 0.1f;

	[Range(0.01f, 1f)]
	public float minDimensionY = 0.1f;

	public PointerEventBubbleType dragBubbleType = PointerEventBubbleType.Hierarchy;

	private Dictionary<RectTransform.Edge, DragPanel> _connectedEdges;

	private Dictionary<RectTransform.Edge, Rect3D> _dragRects;

	private List<RectTransform.Edge> _draggedEdges;

	private int _previousRayCastFilterFrame;

	public RectTransform rectTransform => base.transform as RectTransform;

	public Vector2 minDimensions
	{
		get
		{
			return new Vector2(minDimensionX, minDimensionY);
		}
		set
		{
			minDimensionX = value.x;
			minDimensionY = value.y;
		}
	}

	private Dictionary<RectTransform.Edge, DragPanel> connectedEdges => _connectedEdges ?? (_connectedEdges = new Dictionary<RectTransform.Edge, DragPanel>());

	private Dictionary<RectTransform.Edge, Rect3D> dragRects => _dragRects ?? (_dragRects = new Dictionary<RectTransform.Edge, Rect3D>());

	private List<RectTransform.Edge> draggedEdges => _draggedEdges ?? (_draggedEdges = new List<RectTransform.Edge>());

	private void _CacheConnectedEdges()
	{
		connectedEdges.Clear();
		if (base.transform.parent == null)
		{
			return;
		}
		using PoolKeepItemListHandle<DragPanel> poolKeepItemListHandle = base.transform.parent.gameObject.GetComponentsInImmediateChildrenPooled<DragPanel>();
		using PoolKeepItemDictionaryHandle<DragPanel, Rect3D> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<DragPanel, Rect3D>();
		foreach (DragPanel item in poolKeepItemListHandle.value)
		{
			poolKeepItemDictionaryHandle.Add(item, item.rectTransform.GetWorldRect3D());
		}
		poolKeepItemListHandle.value.Remove(this);
		RectTransform.Edge[] values = EnumUtil<RectTransform.Edge>.Values;
		foreach (RectTransform.Edge edge in values)
		{
			LineSegment lineSegment = poolKeepItemDictionaryHandle[this][edge];
			foreach (DragPanel item2 in poolKeepItemListHandle.value)
			{
				if (lineSegment.DistanceToLineSegment(poolKeepItemDictionaryHandle[item2][edge.OppositeEdge()]) <= 0.001f)
				{
					connectedEdges.Add(edge, item2);
				}
			}
		}
	}

	private void _CalculateDragRectsBasedOnConnectedEdges()
	{
		dragRects.Clear();
		Rect3D worldRect3D = rectTransform.GetWorldRect3D();
		float x = dragAreaWidth * base.transform.parent.lossyScale.Average() * 2f;
		foreach (KeyValuePair<RectTransform.Edge, DragPanel> item in connectedEdges.EnumeratePairsSafe())
		{
			if ((bool)item.Value)
			{
				LineSegment lineSegment = worldRect3D[item.Key];
				dragRects.Add(item.Key, new Rect3D(lineSegment.midpoint, -rectTransform.forward, lineSegment.direction, new Vector2(x, lineSegment.length)));
			}
			else
			{
				connectedEdges.Remove(item.Key);
			}
		}
	}

	private bool _CalculateDraggedEdges(Vector3 worldPosition)
	{
		draggedEdges.Clear();
		foreach (KeyValuePair<RectTransform.Edge, Rect3D> dragRect in dragRects)
		{
			if (dragRect.Value.ContainsProjection(worldPosition))
			{
				draggedEdges.Add(dragRect.Key);
			}
		}
		return draggedEdges.Count > 0;
	}

	private bool _CommonDrag<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> eventFunction) where T : IEventSystemHandler
	{
		if (_CalculateDraggedEdges(eventData.GetWorldPositionOnPlane(base.transform)))
		{
			return true;
		}
		dragBubbleType.BubblePointerDrag(this, eventData, eventFunction);
		return false;
	}

	private void OnDisable()
	{
		connectedEdges.Clear();
		dragRects.Clear();
		draggedEdges.Clear();
	}

	public void OnInitializePotentialDrag(PointerEventData eventData)
	{
		_CacheConnectedEdges();
		_CalculateDragRectsBasedOnConnectedEdges();
		if (_CommonDrag(eventData, ExecuteEvents.initializePotentialDrag))
		{
			eventData.useDragThreshold = false;
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		_CommonDrag(eventData, ExecuteEvents.beginDragHandler);
	}

	public void OnDrag(PointerEventData eventData)
	{
		Vector2 lerpAmount = (base.transform.parent as RectTransform).GetWorldRect3D().GetLerpAmount(eventData.GetWorldPositionOnPlane(base.transform.parent));
		foreach (RectTransform.Edge draggedEdge in draggedEdges)
		{
			draggedEdge.SetNeighboringAnchors(minDimensions, rectTransform, connectedEdges[draggedEdge].minDimensions, connectedEdges[draggedEdge].rectTransform, lerpAmount);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		draggedEdges.Clear();
	}

	public bool ShouldShowCanDrag()
	{
		_CacheConnectedEdges();
		_CalculateDragRectsBasedOnConnectedEdges();
		return _CalculateDraggedEdges(base.transform.GetPlane(PlaneAxes.XY).ClosestPointOnPlane(GetComponentInParent<Canvas>().worldCamera.ScreenPointToRay(Input.mousePosition)));
	}
}
