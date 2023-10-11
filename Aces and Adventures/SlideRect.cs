using UnityEngine;
using UnityEngine.EventSystems;

public class SlideRect : MonoBehaviour, IInitializePotentialDragHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
	public bool useScaledTime;

	public RectTransform viewport;

	public RectTransform content;

	[Range(0f, 1000f)]
	public float sizeOfContentRectThatMustBeInViewport;

	[Header("Drag")]
	[EnumFlags]
	public PointerInputButtonFlags dragButtons = PointerInputButtonFlags.Left;

	public PointerEventBubbleType otherDragButtonsBubbleType = PointerEventBubbleType.Hierarchy;

	[Range(0f, 100f)]
	public float easeBackIntoViewSpeed = 5f;

	[Range(0f, 1f)]
	public float dragVelocityMultiplier;

	[Header("Zoom")]
	public bool zoomEnabled;

	[Range(0.1f, 1f)]
	public float minZoomScale = 0.5f;

	[Range(1f, 10f)]
	public float maxZoomScale = 2f;

	[Range(0.01f, 1f)]
	public float zoomStepSize = 0.1f;

	[Range(0f, 1f)]
	public float zoomButtonPressedGracePeriod = 0.2f;

	[Header("Events")]
	public bool signalUVEvents;

	[SerializeField]
	protected UVCoordsEvent _onViewportUVCoordsChange;

	private Vector3? _dragOffset;

	private Vector3 _dragVelocity;

	private float _timeOfLastDrag;

	private Camera _canvasCamera;

	public bool isDragging => _dragOffset.HasValue;

	public bool isValid
	{
		get
		{
			if ((bool)viewport && (bool)content)
			{
				return canvasCamera;
			}
			return false;
		}
	}

	public Camera canvasCamera
	{
		get
		{
			if (!_canvasCamera)
			{
				return _canvasCamera = GetComponentInParent<Canvas>().worldCamera;
			}
			return _canvasCamera;
		}
	}

	protected Vector3 _contentPosition
	{
		get
		{
			return content.position;
		}
		set
		{
			if (value != _contentPosition)
			{
				_SetContentPosition(value);
			}
		}
	}

	protected float _contentScale
	{
		get
		{
			return content.localScale.x;
		}
		set
		{
			content.localScale = Vector3.one * Mathf.Clamp(value, minZoomScale, maxZoomScale);
		}
	}

	public UVCoordsEvent onViewportUVCoordsChange => _onViewportUVCoordsChange ?? (_onViewportUVCoordsChange = new UVCoordsEvent());

	public Vector2 zoomScaleRange
	{
		get
		{
			return new Vector2(minZoomScale, maxZoomScale);
		}
		set
		{
			minZoomScale = value.x;
			maxZoomScale = value.y;
			_contentScale = _contentScale;
		}
	}

	public UVCoords viewPortUVCoords
	{
		get
		{
			return content.GetWorldRect3D().GetUVCoords(viewport.GetWorldRect3D());
		}
		set
		{
			content.SetWorldCornersPreserveScale(viewport.GetWorldRect3D().GetUVReferenceRect(value));
		}
	}

	private bool _IsValid(PointerEventData eventData)
	{
		if (isValid)
		{
			return EnumUtil.HasFlagConvert(dragButtons, eventData.button);
		}
		return false;
	}

	private void _ApplyDragFriction(float time)
	{
		_dragVelocity *= MathUtil.FrictionSubjectToTimeSmooth(dragVelocityMultiplier * dragVelocityMultiplier * dragVelocityMultiplier, time);
	}

	private void _ApplyDragVelocity()
	{
		if (!(dragVelocityMultiplier <= 0f))
		{
			float deltaTime = GameUtil.GetDeltaTime(useScaledTime);
			_ApplyDragFriction(deltaTime);
			_contentPosition += _dragVelocity * deltaTime;
		}
	}

	private void _SetContentPosition(Vector3 position)
	{
		content.position = position;
		if (signalUVEvents && isValid)
		{
			onViewportUVCoordsChange.Invoke(viewPortUVCoords);
		}
	}

	private void OnDisable()
	{
		_dragOffset = null;
		_canvasCamera = null;
	}

	private void LateUpdate()
	{
		if (isDragging || !isValid)
		{
			return;
		}
		_ApplyDragVelocity();
		Rect3D rect3D = viewport.GetWorldRect3D();
		Rect3D worldRect3D = content.GetWorldRect3D();
		if (sizeOfContentRectThatMustBeInViewport > 0f)
		{
			rect3D = rect3D.Pad(Vector2.Max(Vector2.zero, (worldRect3D.size - Vector2.one * sizeOfContentRectThatMustBeInViewport * content.lossyScale.Average()) * 2f));
		}
		Rect container = rect3D.WorldToViewportRect(canvasCamera).Project2D();
		Vector2 v = worldRect3D.WorldToViewportRect(canvasCamera).Project2D().ContainmentOffset(container);
		if (!(v.sqrMagnitude <= 0f))
		{
			Vector3 vector = (canvasCamera.cameraToWorldMatrix * canvasCamera.projectionMatrix.inverse).MultiplyVector(v.Unproject(AxisType.Z)) * Mathf.Abs(canvasCamera.worldToCameraMatrix.MultiplyPoint(viewport.position).z) * 2f;
			Vector3 v2 = Vector3.zero;
			if (easeBackIntoViewSpeed > 0f)
			{
				vector = MathUtil.EaseV3(ref v2, vector, easeBackIntoViewSpeed, GameUtil.GetDeltaTime(useScaledTime));
			}
			_contentPosition += vector;
		}
	}

	public void SetContentScale(float scale)
	{
		content.localScale = Vector3.one * scale;
	}

	public void SetContentPosition(Vector3 worldPosition)
	{
		_dragVelocity = Vector3.zero;
		_SetContentPosition(worldPosition);
	}

	public void SetContentPosition(Vector3? worldPosition)
	{
		if (worldPosition.HasValue)
		{
			SetContentPosition(worldPosition.Value);
		}
		else
		{
			CenterContent();
		}
	}

	public void CenterContent()
	{
		SetContentPosition(viewport.GetWorldRect3D().center + (content.GetWorldRect3D().center - content.position));
	}

	public void CenterContentAtPosition(Vector3? worldPositionOfContentToCenter)
	{
		if (worldPositionOfContentToCenter.HasValue)
		{
			SetContentPosition(viewport.GetWorldRect3D().center + (content.position - worldPositionOfContentToCenter.Value));
		}
		else
		{
			CenterContent();
		}
	}

	public void OnInitializePotentialDrag(PointerEventData eventData)
	{
		if (!_IsValid(eventData))
		{
			otherDragButtonsBubbleType.BubblePointerDrag(this, eventData, ExecuteEvents.initializePotentialDrag);
		}
		else
		{
			_dragVelocity = Vector3.zero;
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (_IsValid(eventData))
		{
			_dragOffset = _contentPosition - viewport.GetPlane(PlaneAxes.XY).ClosestPointOnPlane(eventData.pressEventCamera.ScreenPointToRay(Input.mousePosition));
		}
		else
		{
			otherDragButtonsBubbleType.BubblePointerDrag(this, eventData, ExecuteEvents.beginDragHandler);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (isValid)
		{
			Vector3 contentPosition = _contentPosition;
			_contentPosition = viewport.GetPlane(PlaneAxes.XY).ClosestPointOnPlane(eventData.pressEventCamera.ScreenPointToRay(Input.mousePosition)) + (_dragOffset ?? Vector3.zero);
			_dragVelocity = (_contentPosition - contentPosition) / GameUtil.GetDeltaTime(useScaledTime).InsureNonZero();
			_timeOfLastDrag = GameUtil.GetTime(useScaledTime);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		_dragOffset = null;
		if (dragVelocityMultiplier <= 0f)
		{
			_dragVelocity = Vector3.zero;
		}
		else
		{
			_ApplyDragFriction(GameUtil.GetTime(useScaledTime) - _timeOfLastDrag);
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (!zoomEnabled || !isValid || isDragging)
		{
			return;
		}
		foreach (KeyState item in InputManager.I[dragButtons])
		{
			if (item.timeSincePressedOrReleased < zoomButtonPressedGracePeriod)
			{
				return;
			}
		}
		Vector3 vector = content.GetPlane(PlaneAxes.XY).ClosestPointOnPlane(canvasCamera.ScreenPointToRay(Input.mousePosition));
		Vector2 lerpAmount = content.GetWorldRect3D().GetLerpAmount(vector);
		content.localScale = Vector3.one * Mathf.Clamp(content.transform.localScale.x + eventData.scrollDelta.y * zoomStepSize, minZoomScale, maxZoomScale);
		_contentPosition += vector - content.GetWorldRect3D().Lerp(lerpAmount);
	}
}
