using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(RayCastTargetTriangle))]
public class PointerOverTriangle : MonoBehaviour, ILayoutIgnorer, IPointerDownHandler, IEventSystemHandler, IPointerExitHandler
{
	private static GameObject _Blueprint;

	[Range(0f, 10f)]
	public float leftRightEdgePadding = 1.44f;

	[Range(0f, 100f)]
	public float topBottomEdgePadding = 10f;

	[Range(0f, 1f)]
	public float mirrorEdgeScale = 0.5f;

	[Range(0f, 1f)]
	public float mouseMovementTime = 0.25f;

	[Range(0f, 2f)]
	public float mouseMovementPadding = 1f;

	private Camera _eventCamera;

	private RectTransform _targetRectTransform;

	private Canvas _targetCanvas;

	private RayCastTargetTriangle _tri;

	private Vector3? _lastMousePosition;

	private float _timeOfLastMouseMovement;

	private static GameObject Blueprint
	{
		get
		{
			if (!_Blueprint)
			{
				return _Blueprint = Resources.Load<GameObject>("UI/ContextMenu/PointerOverTriangle");
			}
			return _Blueprint;
		}
	}

	private RayCastTargetTriangle tri => this.CacheComponent(ref _tri);

	public bool ignoreLayout => true;

	public event Action onMouseDown;

	private void OnDisable()
	{
		_eventCamera = null;
		_targetRectTransform = null;
		_targetCanvas = null;
		_lastMousePosition = null;
	}

	private void OnEnable()
	{
		tri.raycastTarget = true;
	}

	private void Update()
	{
		tri.raycastTarget = tri.raycastTarget || !InputManager.I.MouseInputButtonState(KState.Down);
	}

	private void LateUpdate()
	{
		tri.tri = _CalculateTriangle();
	}

	private Triangle _CalculateTriangle()
	{
		if (!_eventCamera || !_targetRectTransform || !base.transform.parent)
		{
			return default(Triangle);
		}
		Vector3? vector = base.transform.parent.transform.GetPlane(PlaneAxes.XY).Raycast(_eventCamera.ScreenPointToRay(Input.mousePosition));
		if (!vector.HasValue || RectTransformUtility.RectangleContainsScreenPoint(_targetRectTransform, Input.mousePosition.Project(AxisType.Z), _eventCamera))
		{
			return default(Triangle);
		}
		float num = ((_targetCanvas.renderMode == RenderMode.WorldSpace) ? 1f : _targetCanvas.transform.lossyScale.Average());
		Vector3 vector2 = vector.Value * num.Reciprocal().InsureNonZero();
		if (!_lastMousePosition.HasValue || (vector2 - _lastMousePosition.Value).magnitude > mouseMovementPadding)
		{
			_lastMousePosition = vector2;
			_timeOfLastMouseMovement = Time.unscaledTime;
		}
		if (!InputManager.I.MouseInputButtonState(KState.Down) && Time.unscaledTime - _timeOfLastMouseMovement > mouseMovementTime && !RectTransformUtility.RectangleContainsScreenPoint(base.transform.parent as RectTransform, Input.mousePosition.Project(AxisType.Z), _eventCamera))
		{
			return default(Triangle);
		}
		RectTransform.Edge edge;
		LineSegment lineSegment = _targetRectTransform.GetWorldRect3D().GetEdgeClosestTo(vector.Value, out edge).Mirror(vector.Value, mirrorEdgeScale);
		float num2 = ((edge == RectTransform.Edge.Left || edge == RectTransform.Edge.Right) ? leftRightEdgePadding : topBottomEdgePadding);
		return new Triangle(vector.Value, lineSegment.a, lineSegment.b).Scale(Vector3.one * 1000f, vector.Value).Pad(num2 * num);
	}

	public PointerOverTriangle SetData(PointerEventData eventData, RectTransform targetRectTransform)
	{
		base.gameObject.SetActive(value: true);
		_eventCamera = eventData.enterEventCamera;
		_targetRectTransform = targetRectTransform;
		_targetCanvas = targetRectTransform.GetComponentInParent<Canvas>();
		LateUpdate();
		return this;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (this.onMouseDown != null)
		{
			this.onMouseDown();
		}
		ExecuteEvents.ExecuteHierarchy(base.gameObject, eventData, ExecuteEvents.pointerExitHandler);
		eventData.BubbleEvent(this, ExecuteEvents.pointerEnterHandler, useCachedRaycasts: false);
		GameObject pressed = eventData.BubbleEvent(this, ExecuteEvents.pointerDownHandler, useCachedRaycasts: false);
		eventData.PostProcess(delegate(PointerEventData e)
		{
			e.pointerPress = pressed;
		});
		tri.raycastTarget = false;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (eventData.eligibleForClick && Math.Abs(eventData.ClickHeldTime()) < MathUtil.BigEpsilon)
		{
			eventData.PostProcess(delegate(PointerEventData e)
			{
				e.pointerPress = eventData.BubbleEvent(this, ExecuteEvents.pointerDownHandler);
			});
		}
	}
}
