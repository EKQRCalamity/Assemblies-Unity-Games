using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PointerDrag3D), typeof(PointerScroll3D))]
public class DragPlane3D : MonoBehaviour
{
	public Transform contentContainer;

	public Transform contentBoundsReference;

	public RectTransform minZoomViewRect;

	public RectTransform maxZoomViewRect;

	[SerializeField]
	[Range(0f, 1f)]
	protected float _zoom = 0.5f;

	[Range(0.01f, 1f)]
	public float zoomSpeed = 0.25f;

	public Vector2Event onDragBeyondThreshold;

	private Vector3 _dragOffset;

	private bool _isDragging;

	private float _previousZoom;

	public float zoom
	{
		get
		{
			return _zoom;
		}
		set
		{
			_previousZoom = _zoom;
			if (SetPropertyUtility.SetStruct(ref _zoom, Mathf.Clamp01(value)))
			{
				_OnZoomChanged();
			}
		}
	}

	public Rect3D zoomRect => _GetZoomRect();

	public Transform contentReference
	{
		get
		{
			if (!contentBoundsReference)
			{
				return contentContainer;
			}
			return contentBoundsReference;
		}
	}

	public Vector3 contentPositionAtBeginDrag { get; private set; }

	public bool isDragging => _isDragging;

	private Rect3D _GetZoomRect(float? zoomAmount = null)
	{
		return new Rect3D(minZoomViewRect).Lerp(new Rect3D(maxZoomViewRect), zoomAmount ?? zoom);
	}

	private Vector3? _RaycastMouseOnZoomPlane(float? zoomAmount = null)
	{
		return _GetZoomRect(zoomAmount ?? zoom).ToPlane().Raycast(Camera.main.ScreenPointToRay(Input.mousePosition));
	}

	private void _OnBeginDrag(PointerEventData eventData)
	{
		if (eventData != null)
		{
			_isDragging = true;
		}
		if (_isDragging)
		{
			contentPositionAtBeginDrag = contentContainer.position;
		}
		_dragOffset = Vector3.ProjectOnPlane(contentContainer.position - (_RaycastMouseOnZoomPlane() ?? eventData?.pointerPressRaycast.worldPosition ?? contentContainer.position), zoomRect.normal);
	}

	private void _OnDrag(PointerEventData eventData)
	{
		Vector3? vector = _RaycastMouseOnZoomPlane();
		if (vector.HasValue)
		{
			Vector3 valueOrDefault = vector.GetValueOrDefault();
			contentContainer.position = valueOrDefault + _dragOffset;
		}
	}

	private void _OnEndDrag(PointerEventData eventData)
	{
		if (eventData != null)
		{
			_isDragging = false;
		}
		Rect3D rect3D = zoomRect.CreateCoplanarRectFromPoints(contentReference.gameObject.GetComponentsInChildrenPooled<Renderer>().AsEnumerable().SelectMany((Renderer c) => c.bounds.GetCorners()));
		if (contentBoundsReference != null)
		{
			rect3D = rect3D.Translate(contentContainer.position - rect3D.center);
		}
		Vector3 vector = rect3D.FitIntoView(zoomRect).bottomLeft - rect3D.bottomLeft;
		contentContainer.position += vector;
		if (eventData != null && eventData.dragging && vector.sqrMagnitude > 0f)
		{
			onDragBeyondThreshold?.Invoke(rect3D.GetLerpAmount(rect3D.bottomLeft - vector));
		}
	}

	private void _OnScrollDelta(Vector2 delta)
	{
		if (!_isDragging)
		{
			zoom += delta.y * zoomSpeed;
		}
	}

	private void _OnZoomChanged()
	{
		Vector3? vector = _RaycastMouseOnZoomPlane(_previousZoom);
		_OnBeginDrag(null);
		_OnDrag(null);
		_OnEndDrag(null);
		Vector3? vector2 = _RaycastMouseOnZoomPlane();
		if (vector.HasValue && vector2.HasValue)
		{
			contentContainer.position += Vector3.ProjectOnPlane(vector2.Value - vector.Value, zoomRect.normal);
		}
	}

	private void Awake()
	{
		PointerDrag3D component = GetComponent<PointerDrag3D>();
		component.OnBegin.AddListener(_OnBeginDrag);
		component.OnDragged.AddListener(_OnDrag);
		component.OnEnd.AddListener(_OnEndDrag);
		GetComponent<PointerScroll3D>().onScrollDelta.AddListener(_OnScrollDelta);
		contentContainer.position = _GetZoomRect(zoom).center;
		_previousZoom = zoom;
	}

	public void FocusOnPosition(Transform content, Vector3 position)
	{
		contentContainer.position = zoomRect.center + minZoomViewRect.TransformDirection(content.InverseTransformDirection(content.position - position));
	}

	public void SetContentPosition(Vector3 position)
	{
		contentContainer.position = zoomRect.GetPointOnPlaneClosestTo(position);
		_OnEndDrag(null);
	}
}
