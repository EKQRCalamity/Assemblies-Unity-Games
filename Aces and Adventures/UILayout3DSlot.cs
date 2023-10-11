using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UILayout3DSlot : MonoBehaviour
{
	public enum State : byte
	{
		Rest,
		PointerOver,
		PointerDown,
		Drag
	}

	[Flags]
	public enum StateFlags
	{
		Rest = 1,
		PointerOver = 2,
		PointerDown = 4,
		Drag = 8
	}

	public struct Snapshot
	{
		public readonly TransformData slotTransformData;

		public readonly Transform slottedTransform;

		public readonly TransformTarget velocity;

		public Snapshot(UILayout3DSlot slot)
		{
			slotTransformData = new TransformData(slot.slot);
			slottedTransform = slot.slottedTransform;
			velocity = slot._velocity;
		}
	}

	[Serializable]
	public struct NoiseData
	{
		[EnumFlags]
		public StateFlags applyInState;

		[Header("Frequency")]
		public float frequencyMultiplier;

		public Vector3 frequency;

		[Header("Range")]
		public float rangeMultiplier;

		public Vector3 rangeMin;

		public Vector3 rangeMax;

		[Header("Transition")]
		[Range(1f, 20f)]
		public float transitionEaseSpeed;

		private Vector3? _timeOffset;

		private float _elapsedTime;

		private float _magnitudeMultiplier;

		private Vector3 timeOffset
		{
			get
			{
				Vector3? vector = _timeOffset;
				if (!vector.HasValue)
				{
					Vector3? vector2 = (_timeOffset = new Vector3(_Random.Next(2048), _Random.Next(2048), _Random.Next(2048)));
					return vector2.Value;
				}
				return vector.GetValueOrDefault();
			}
		}

		private bool _ShouldApply(State state)
		{
			return EnumUtil.HasFlagConvert(applyInState, state);
		}

		private float _GetValue(int axis, Vector2 noiseDirection)
		{
			Vector2 vector = noiseDirection * frequency[axis] * _elapsedTime;
			return Mathf.Lerp(rangeMin[axis], rangeMax[axis], Mathf.PerlinNoise(timeOffset[axis] + vector.x, timeOffset[axis] + vector.y)) * _magnitudeMultiplier;
		}

		private Vector3 _GetValue()
		{
			if (_magnitudeMultiplier != 0f)
			{
				return new Vector3(_GetValue(0, Vector2.right), _GetValue(1, Vector2.up), _GetValue(2, Vector2.left));
			}
			return Vector3.zero;
		}

		private void _CommonApply(State state, float deltaTime)
		{
			_elapsedTime += deltaTime * frequencyMultiplier;
			MathUtil.Ease(ref _magnitudeMultiplier, _ShouldApply(state) ? rangeMultiplier : 0f, transitionEaseSpeed, deltaTime);
		}

		public void ApplyPosition(State state, Transform transform, float deltaTime)
		{
			_CommonApply(state, deltaTime);
			transform.localPosition = _GetValue();
		}

		public void ApplyRotation(State state, Transform transform, float deltaTime)
		{
			_CommonApply(state, deltaTime);
			transform.localRotation = Quaternion.Euler(_GetValue());
		}

		public void ApplyScale(State state, Transform transform, float deltaTime)
		{
			_CommonApply(state, deltaTime);
			transform.localScale = _GetValue() + Vector3.one;
		}

		public void Reset()
		{
			applyInState = (StateFlags)0;
			frequencyMultiplier = 1f;
			frequency = Vector3.one;
			rangeMultiplier = 1f;
			rangeMin = -Vector3.one;
			rangeMax = Vector3.one;
			transitionEaseSpeed = 5f;
			_timeOffset = null;
			_elapsedTime = 0f;
			_magnitudeMultiplier = 0f;
		}
	}

	private static System.Random _random;

	[SerializeField]
	[Header("Transforms====================================================================================================", order = -1)]
	protected Transform _parentOverride;

	[SerializeField]
	protected Transform _rest;

	[SerializeField]
	protected Transform _into;

	[SerializeField]
	protected Transform _outOf;

	[SerializeField]
	protected Transform _pointerOver;

	[SerializeField]
	protected Transform _pointerDown;

	[SerializeField]
	protected Transform _drag;

	[Header("ANIMATION=====================================================================================================================", order = -1)]
	public bool useScaledTime;

	public bool animateOnlyWhileDragging;

	[Header("Position Spring")]
	[Range(1f, 1000f)]
	public float positionSpringConstant = 100f;

	[Range(1f, 100f)]
	public float positionSpringDampening = 10f;

	[Header("Rotation Spring")]
	[Range(1f, 1000f)]
	public float rotationSpringConstant = 100f;

	[Range(1f, 100f)]
	public float rotationSpringDampening = 10f;

	[Header("Scale Spring")]
	[Range(1f, 1000f)]
	public float scaleSpringConstant = 100f;

	[Range(1f, 100f)]
	public float scaleSpringDampening = 10f;

	[Header("Animation Complete Thresholds")]
	[Range(0.01f, 1f)]
	public float animationCompletePositionDelta = 0.01f;

	[Range(0.1f, 30f)]
	public float animationCompleteRotationDelta = 1f;

	[Range(0.01f, 1f)]
	public float animationCompleteScaleDelta = 0.01f;

	[Header("Noise")]
	public NoiseData positionNoise;

	public NoiseData rotationNoise;

	public NoiseData scaleNoise;

	[Header("DRAGGING======================================================================================================================", order = -1)]
	[SerializeField]
	protected bool _dragEnabled = true;

	public PlaneAxes dragPlaneAxes = PlaneAxes.XZ;

	[Range(1f, 400f)]
	public float dragSpringConstant = 200f;

	[Range(1f, 100f)]
	public float dragSpringDampening = 15f;

	[Range(1f, 100f)]
	[Header("Dynamic Drag Rotation")]
	public float dragRotationSpeedMax = 25f;

	[Range(0f, 1f)]
	public float dragRotationRatioMax = 1f;

	[Range(0f, 100f)]
	[Header("Drag State Transitioning")]
	public float dragTransitionRadius = 1f;

	[Range(0.1f, 10f)]
	public float dragTransitionCurvePower = 1f;

	[Header("EVENTS========================================================================================================================", order = -1)]
	public bool destroyOnAnimateOutComplete = true;

	public bool addPointerOverColliderScaler = true;

	[Range(1.05f, 3f)]
	public float pointerOverColliderScale = 1.75f;

	public Collider onDropCollider;

	[SerializeField]
	protected GameObjectEvent _OnSlottedObjectChanged;

	[SerializeField]
	protected UILayout3DTransferEvent _OnSlottedObjectTransferredToExternalLayout;

	[SerializeField]
	protected UILayout3DTransferEvent _OnObjectTransferredFromExternalLayout;

	[SerializeField]
	protected GameObjectEvent _OnObjectDroppedWithNoReceiver;

	[SerializeField]
	protected GameObjectEvent _OnObjectBeginAnimateOut;

	[SerializeField]
	protected GameObjectEvent _OnObjectReachedRest;

	[SerializeField]
	protected GameObjectEvent _OnObjectPointerOver;

	[SerializeField]
	protected GameObjectEvent _OnObjectPointerDown;

	[SerializeField]
	protected PointerEvent _OnObjectPointerClick;

	[SerializeField]
	protected GameObjectEvent _OnObjectBeginDrag;

	[SerializeField]
	protected GameObjectEvent _OnObjectEndDrag;

	private StateFlags _stateFlags = StateFlags.Rest;

	private State? _previousState;

	private Transform _slot;

	private Transform _target;

	private Transform _dragTarget;

	private Transform _animateOut;

	private TransformTarget _velocity;

	private Quaternion _targetOriginalRotation;

	private TransformTarget _animateOutVelocity;

	private Quaternion _animateOutTargetOriginalRotation;

	private GameObject _registeredGameObject;

	private bool _hasReachedRest;

	private int _frameOfCreation;

	private static System.Random _Random => _random ?? (_random = new System.Random());

	protected Transform _parent
	{
		get
		{
			if (!_parentOverride)
			{
				return base.transform;
			}
			return _parentOverride;
		}
	}

	public Transform parentOverride
	{
		get
		{
			return _parentOverride;
		}
		set
		{
			_parentOverride = value;
		}
	}

	public Transform rest
	{
		get
		{
			if (!_rest)
			{
				return _parent;
			}
			return _rest;
		}
	}

	public Transform into
	{
		get
		{
			if (!_into)
			{
				return rest;
			}
			return _into;
		}
	}

	public Transform outOf
	{
		get
		{
			if (!_outOf)
			{
				return into;
			}
			return _outOf;
		}
	}

	public Transform pointerOver
	{
		get
		{
			if (!_pointerOver)
			{
				return rest;
			}
			return _pointerOver;
		}
	}

	public Transform pointerDown
	{
		get
		{
			if (!_pointerDown)
			{
				return pointerOver;
			}
			return _pointerDown;
		}
	}

	public Transform drag
	{
		get
		{
			if (!_drag)
			{
				return pointerOver;
			}
			return _drag;
		}
		set
		{
			_drag = value;
		}
	}

	public Transform slot
	{
		get
		{
			if (!_slot)
			{
				return _slot = new GameObject("Slot").transform.SetParentAndReturn(_parent, worldPositionStays: false);
			}
			return _slot;
		}
	}

	protected Transform target
	{
		get
		{
			if (!_target)
			{
				return _target = new GameObject("Target").transform.SetParentAndReturn(_parent, worldPositionStays: false);
			}
			return _target;
		}
	}

	protected Transform dragTarget
	{
		get
		{
			if (!_dragTarget)
			{
				return _dragTarget = new GameObject("Drag Target").transform.SetParentAndReturn(_parent, worldPositionStays: false);
			}
			return _dragTarget;
		}
	}

	protected Transform animateOut
	{
		get
		{
			if (!_animateOut)
			{
				return _animateOut = new GameObject("Animate Out").transform.SetParentAndReturn(_parent, worldPositionStays: false);
			}
			return _animateOut;
		}
	}

	public State state
	{
		get
		{
			State state = EnumUtil<StateFlags>.ConvertFromFlag<State>(EnumUtil.MaxActiveFlag(_stateFlags));
			if (_previousState != state)
			{
				_previousState = state;
				_ResetTarget();
				if (animateOnlyWhileDragging)
				{
					_SignalRestReached();
				}
			}
			return state;
		}
	}

	public Transform slottedTransform
	{
		get
		{
			if (slot.childCount <= 0)
			{
				return null;
			}
			return slot.GetChild(0);
		}
	}

	public Transform animatingOutTransform
	{
		get
		{
			if (animateOut.childCount <= 0)
			{
				return null;
			}
			return animateOut.GetChild(0);
		}
	}

	public bool isOpen => slot.childCount == 0;

	public float speed => _velocity.position.magnitude;

	public GameObjectEvent OnSlottedObjectChanged => _OnSlottedObjectChanged ?? (_OnSlottedObjectChanged = new GameObjectEvent());

	public UILayout3DTransferEvent OnSlottedObjectTransferredToExternalLayout => _OnSlottedObjectTransferredToExternalLayout ?? (_OnSlottedObjectTransferredToExternalLayout = new UILayout3DTransferEvent());

	public UILayout3DTransferEvent OnObjectTransferredFromExternalLayout => _OnObjectTransferredFromExternalLayout ?? (_OnObjectTransferredFromExternalLayout = new UILayout3DTransferEvent());

	public GameObjectEvent OnObjectDroppedWithNoReceiver => _OnObjectDroppedWithNoReceiver ?? (_OnObjectDroppedWithNoReceiver = new GameObjectEvent());

	public GameObjectEvent OnObjectBeginAnimateOut => _OnObjectBeginAnimateOut ?? (_OnObjectBeginAnimateOut = new GameObjectEvent());

	public GameObjectEvent OnObjectReachedRest => _OnObjectReachedRest ?? (_OnObjectReachedRest = new GameObjectEvent());

	public GameObjectEvent OnObjectPointerOver => _OnObjectPointerOver ?? (_OnObjectPointerOver = new GameObjectEvent());

	public GameObjectEvent OnObjectPointerDown => _OnObjectPointerDown ?? (_OnObjectPointerDown = new GameObjectEvent());

	public PointerEvent OnObjectPointerClick => _OnObjectPointerClick ?? (_OnObjectPointerClick = new PointerEvent());

	public GameObjectEvent OnObjectBeginDrag => _OnObjectBeginDrag ?? (_OnObjectBeginDrag = new GameObjectEvent());

	public GameObjectEvent OnObjectEndDrag => _OnObjectEndDrag ?? (_OnObjectEndDrag = new GameObjectEvent());

	protected GameObject registeredGameObject
	{
		get
		{
			return _registeredGameObject;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _registeredGameObject, value))
			{
				OnSlottedObjectChanged.Invoke(_registeredGameObject);
			}
		}
	}

	public AUILayout3D layout => GetComponentInParent<AUILayout3D>();

	public bool dragEnabled
	{
		get
		{
			return _dragEnabled;
		}
		set
		{
			SetDragEnabled(value);
		}
	}

	protected bool _isDragging => state == State.Drag;

	protected float? _dragSpringConstantOverride
	{
		get
		{
			if (!_isDragging)
			{
				return null;
			}
			return dragSpringConstant;
		}
	}

	protected float? _dragSpringDampeningOverride
	{
		get
		{
			if (!_isDragging)
			{
				return null;
			}
			return dragSpringDampening;
		}
	}

	public Collider constrainDragWithinCollider { get; set; }

	private Transform _staticTargetTransform => state switch
	{
		State.Rest => rest, 
		State.PointerOver => pointerOver, 
		State.PointerDown => pointerDown, 
		State.Drag => dragTarget, 
		_ => throw new ArgumentOutOfRangeException(), 
	};

	private Transform _dynamicTargetTransform
	{
		get
		{
			switch (state)
			{
			case State.Rest:
			case State.PointerOver:
			case State.PointerDown:
				return target;
			case State.Drag:
				return dragTarget;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	private static event Action<bool> OnIsDraggingChanged;

	private void _RegisterOnDropCollider()
	{
		if ((bool)onDropCollider)
		{
			onDropCollider.gameObject.GetOrAddComponent<PointerDrop3D>().OnDropped.AddListener(_OnDrop);
		}
	}

	private void _OnDrop(PointerEventData eventData)
	{
		SimulateOnDrop(eventData.pointerDrag);
	}

	private void _RegisterObject(GameObject gameObject)
	{
		if (!(registeredGameObject == gameObject))
		{
			if ((bool)registeredGameObject)
			{
				_UnregisterObject(registeredGameObject);
			}
			registeredGameObject = gameObject;
			PointerOver3D orAddComponent = gameObject.GetOrAddComponent<PointerOver3D>();
			if (addPointerOverColliderScaler)
			{
				gameObject.GetOrAddComponent<PointerOver3DColliderScaler>().scale = pointerOverColliderScale;
			}
			orAddComponent.OnEnter.AddListenerUnique(_OnPointerEnter);
			orAddComponent.OnExit.AddListenerUnique(_OnPointerExit);
			PointerClick3D orAddComponent2 = gameObject.GetOrAddComponent<PointerClick3D>();
			orAddComponent2.OnDown.AddListenerUnique(_OnPointerDown);
			orAddComponent2.OnUp.AddListenerUnique(_OnPointerUp);
			orAddComponent2.OnClick.AddListenerUnique(_OnPointerClick);
			PointerDrag3D orAddComponent3 = gameObject.GetOrAddComponent<PointerDrag3D>();
			orAddComponent3.OnInitialize.AddListenerUnique(_OnInitializeDrag);
			orAddComponent3.OnBegin.AddListenerUnique(_OnBeginDrag);
			orAddComponent3.OnDragged.AddListenerUnique(_OnDrag);
			orAddComponent3.OnEnd.AddListenerUnique(_OnEndDrag);
			_ResetTarget();
		}
	}

	private void _UnregisterObject(GameObject gameObject)
	{
		if (!(registeredGameObject != gameObject))
		{
			registeredGameObject = null;
			if (EnumUtil.HasFlag(_stateFlags, StateFlags.PointerOver))
			{
				_OnPointerExit(null);
			}
			if (EnumUtil.HasFlag(_stateFlags, StateFlags.PointerDown))
			{
				_OnPointerUp(null);
			}
			if (EnumUtil.HasFlag(_stateFlags, StateFlags.Drag))
			{
				_OnEndDrag(null);
			}
			PointerOver3D component = gameObject.GetComponent<PointerOver3D>();
			if ((bool)component)
			{
				component.OnEnter.RemoveListener(_OnPointerEnter);
				component.OnExit.RemoveListener(_OnPointerExit);
			}
			PointerClick3D component2 = gameObject.GetComponent<PointerClick3D>();
			if ((bool)component2)
			{
				component2.OnDown.RemoveListener(_OnPointerDown);
				component2.OnUp.RemoveListener(_OnPointerUp);
				component2.OnClick.RemoveListener(_OnPointerClick);
			}
			PointerDrag3D component3 = gameObject.GetComponent<PointerDrag3D>();
			if ((bool)component3)
			{
				component3.OnInitialize.RemoveListener(_OnInitializeDrag);
				component3.OnBegin.RemoveListener(_OnBeginDrag);
				component3.OnDragged.RemoveListener(_OnDrag);
				component3.OnEnd.RemoveListener(_OnEndDrag);
			}
		}
	}

	private void _OnPointerEnter(PointerEventData eventData)
	{
		if (!eventData.dragging || !(eventData.pointerDrag != eventData.pointerEnter))
		{
			if (!animateOnlyWhileDragging)
			{
				EnumUtil.Add(ref _stateFlags, StateFlags.PointerOver);
			}
			OnObjectPointerOver.Invoke(eventData.pointerEnter);
		}
	}

	private void _OnPointerExit(PointerEventData eventData)
	{
		EnumUtil.Subtract(ref _stateFlags, StateFlags.PointerOver);
	}

	private void _OnPointerDown(PointerEventData eventData)
	{
		if (!animateOnlyWhileDragging)
		{
			EnumUtil.Add(ref _stateFlags, StateFlags.PointerDown);
		}
		OnObjectPointerDown.Invoke(slottedTransform.gameObject);
	}

	private void _OnPointerUp(PointerEventData eventData)
	{
		EnumUtil.Subtract(ref _stateFlags, StateFlags.PointerDown);
	}

	private void _OnPointerClick(PointerEventData eventData)
	{
		OnObjectPointerClick.Invoke(eventData);
	}

	private void _OnInitializeDrag(PointerEventData eventData)
	{
		if (!dragEnabled || eventData.button != 0)
		{
			eventData.pointerDrag = null;
		}
	}

	private void _OnBeginDrag(PointerEventData eventData)
	{
		EnumUtil.Add(ref _stateFlags, StateFlags.Drag);
		if ((bool)slottedTransform)
		{
			Collider[] componentsInChildren = slottedTransform.GetComponentsInChildren<Collider>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			OnObjectBeginDrag.Invoke(slottedTransform.gameObject);
			if (UILayout3DSlot.OnIsDraggingChanged != null)
			{
				UILayout3DSlot.OnIsDraggingChanged(obj: true);
			}
		}
	}

	private void _UpdateDrag(Camera eventCamera)
	{
		dragTarget.position = drag.GetPlane(dragPlaneAxes).ClosestPointOnPlane(eventCamera.ScreenPointToRay(Input.mousePosition));
		if ((bool)constrainDragWithinCollider && !constrainDragWithinCollider.ContainsPoint(dragTarget.position))
		{
			dragTarget.position = drag.GetPlane(dragPlaneAxes).ClosestPointOnPlane(constrainDragWithinCollider.ClosestPoint(dragTarget.position));
		}
		dragTarget.rotation = drag.rotation;
	}

	private void _OnDrag(PointerEventData eventData)
	{
		_UpdateDrag(eventData.pressEventCamera);
	}

	private void _OnEndDrag(PointerEventData eventData)
	{
		EnumUtil.Subtract(ref _stateFlags, StateFlags.Drag);
		if ((bool)slottedTransform)
		{
			Collider[] componentsInChildren = slottedTransform.GetComponentsInChildren<Collider>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
		}
		if (eventData != null && (bool)eventData.pointerDrag && (!eventData.pointerCurrentRaycast.gameObject || !eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<UILayout3DSlot>()))
		{
			OnObjectDroppedWithNoReceiver.Invoke(eventData.pointerDrag);
		}
		else if ((bool)slottedTransform)
		{
			OnObjectEndDrag.Invoke(slottedTransform.gameObject);
		}
		if (UILayout3DSlot.OnIsDraggingChanged != null)
		{
			UILayout3DSlot.OnIsDraggingChanged(obj: false);
		}
	}

	private void _OnIsDraggingChanged(bool isDragging)
	{
		if ((bool)onDropCollider)
		{
			onDropCollider.gameObject.SetActive(dragEnabled && isDragging);
		}
	}

	private void _SignalRestReached(bool force = false)
	{
		if ((bool)slottedTransform && (force || (!_hasReachedRest && state == State.Rest)))
		{
			_hasReachedRest = true;
			if (_frameOfCreation != Time.frameCount)
			{
				OnObjectReachedRest.Invoke(slottedTransform.gameObject);
			}
		}
	}

	private void _ResetTarget()
	{
		_ResetRotation(ref _velocity, ref _targetOriginalRotation);
		target.CopyFrom(_staticTargetTransform);
		_hasReachedRest = false;
	}

	private void _ResetRotation(ref TransformTarget velocity, ref Quaternion targetOriginalRotation)
	{
		velocity = velocity.ResetRotationData();
		targetOriginalRotation = slot.localRotation;
	}

	private Transform _UnslotObject()
	{
		Transform transform = slottedTransform;
		if (!transform)
		{
			return null;
		}
		if ((bool)animatingOutTransform)
		{
			_OnAnimateOutComplete(animatingOutTransform);
		}
		_animateOutVelocity = _velocity;
		animateOut.CopyFrom(slot);
		_UnregisterObject(transform.gameObject);
		transform.SetParent(animateOut, worldPositionStays: true);
		_ResetRotation(ref _animateOutVelocity, ref _animateOutTargetOriginalRotation);
		OnObjectBeginAnimateOut.Invoke(transform.gameObject);
		return transform;
	}

	private void _SlotObject(GameObject gameObjectToSlot)
	{
		gameObjectToSlot.transform.SetParent(slot, worldPositionStays: false);
		_RegisterObject(gameObjectToSlot);
		_hasReachedRest = false;
		if (animateOnlyWhileDragging)
		{
			_SignalRestReached();
		}
	}

	private void _OnAnimateOutComplete(Transform finishedTransform)
	{
		finishedTransform.SetParent(null, worldPositionStays: true);
		if (destroyOnAnimateOutComplete)
		{
			UnityEngine.Object.Destroy(finishedTransform.gameObject);
		}
		else
		{
			finishedTransform.gameObject.SetActive(value: false);
		}
	}

	private bool _AnimateTransform(Transform transformToAnimate, ref TransformTarget velocity, Quaternion targetOriginalRotation, Transform animateTowards, float? positionSpringConstantOverride = null, float? positionSpringDampeningOverride = null)
	{
		float deltaTime = GameUtil.GetDeltaTime(useScaledTime);
		TransformTarget current = new TransformTarget(transformToAnimate);
		TransformTarget other = new TransformTarget(animateTowards);
		other.originalRotation = targetOriginalRotation;
		TransformTarget.SpringToTarget(current, other, ref velocity, positionSpringConstantOverride ?? positionSpringConstant, positionSpringDampeningOverride ?? positionSpringDampening, rotationSpringConstant, rotationSpringDampening, scaleSpringConstant, scaleSpringDampening, deltaTime).SetTransformValues(transformToAnimate);
		if (!(transformToAnimate.localScale.Max() <= 0f))
		{
			return current.IsRoughlyEqual(other, 1f, animationCompletePositionDelta, animationCompleteScaleDelta, animationCompleteRotationDelta);
		}
		return true;
	}

	private void Awake()
	{
		_RegisterOnDropCollider();
		_frameOfCreation = Time.frameCount;
	}

	private void OnEnable()
	{
		OnIsDraggingChanged += _OnIsDraggingChanged;
	}

	private void OnDisable()
	{
		OnIsDraggingChanged -= _OnIsDraggingChanged;
	}

	private void Update()
	{
		if (!_isDragging && !animateOnlyWhileDragging && dragTransitionRadius > 0f)
		{
			Transform staticTargetTransform = _staticTargetTransform;
			Vector3 vector = drag.position - staticTargetTransform.position;
			AxisType axisClosestToDirection = staticTargetTransform.GetAxisClosestToDirection(vector);
			float magnitude = (staticTargetTransform.GetPlane(axisClosestToDirection.GetPlaneAxes()).ClosestPointOnPlane(slot.position) - staticTargetTransform.position).magnitude;
			target.position = staticTargetTransform.position + vector * Mathf.Pow(Mathf.Clamp01(magnitude / dragTransitionRadius), dragTransitionCurvePower);
		}
		if (!animateOnlyWhileDragging || state == State.Drag)
		{
			if (_AnimateTransform(slot, ref _velocity, _targetOriginalRotation, _dynamicTargetTransform, _dragSpringConstantOverride, _dragSpringDampeningOverride))
			{
				_SignalRestReached();
			}
			if ((bool)slottedTransform)
			{
				float deltaTime = GameUtil.GetDeltaTime(useScaledTime);
				positionNoise.ApplyPosition(state, slottedTransform, deltaTime);
				rotationNoise.ApplyRotation(state, slottedTransform, deltaTime);
				scaleNoise.ApplyScale(state, slottedTransform, deltaTime);
			}
		}
		if ((bool)animatingOutTransform && _AnimateTransform(animateOut, ref _animateOutVelocity, _animateOutTargetOriginalRotation, outOf))
		{
			_OnAnimateOutComplete(animatingOutTransform);
		}
		if (_isDragging)
		{
			if (animateOnlyWhileDragging)
			{
				_UpdateDrag(CameraManager.Instance.mainCamera);
			}
			Vector3 axis = drag.GetAxis(dragPlaneAxes.NormalAxis());
			Vector3 vector2 = Vector3.ProjectOnPlane(_velocity.position, axis);
			float magnitude2 = vector2.magnitude;
			if (magnitude2 > 0f)
			{
				slot.rotation *= Quaternion.Lerp(Quaternion.identity, Quaternion.FromToRotation(axis, vector2 / magnitude2), Mathf.Clamp01(magnitude2 / dragRotationSpeedMax) * dragRotationRatioMax);
			}
		}
		_OnIsDraggingChanged(InputManager.EventSystem.IsPointerDragging());
	}

	public UILayout3DSlot CopyFrom(UILayout3DSlot copyFrom, bool copyPosition = true, bool copyRotation = true, bool copyScale = true)
	{
		_rest.CopyFromLocal(copyFrom._rest, copyPosition, copyRotation, copyScale);
		_into.CopyFromLocal(copyFrom._into, copyPosition, copyRotation, copyScale);
		_outOf.CopyFromLocal(copyFrom._outOf, copyPosition, copyRotation, copyScale);
		_pointerOver.CopyFromLocal(copyFrom._pointerOver, copyPosition, copyRotation, copyScale);
		_pointerDown.CopyFromLocal(copyFrom._pointerDown, copyPosition, copyRotation, copyScale);
		_drag.CopyFromLocal(copyFrom._drag, copyPosition, copyRotation, copyScale);
		if ((bool)onDropCollider && (bool)copyFrom.onDropCollider)
		{
			onDropCollider.transform.CopyFromLocal(copyFrom.onDropCollider.transform);
		}
		useScaledTime = copyFrom.useScaledTime;
		positionSpringConstant = copyFrom.positionSpringConstant;
		positionSpringDampening = copyFrom.positionSpringDampening;
		rotationSpringConstant = copyFrom.rotationSpringConstant;
		rotationSpringDampening = copyFrom.rotationSpringDampening;
		scaleSpringConstant = copyFrom.scaleSpringConstant;
		scaleSpringDampening = copyFrom.scaleSpringDampening;
		animationCompletePositionDelta = copyFrom.animationCompletePositionDelta;
		animationCompleteRotationDelta = copyFrom.animationCompleteRotationDelta;
		animationCompleteScaleDelta = copyFrom.animationCompleteScaleDelta;
		positionNoise = copyFrom.positionNoise;
		rotationNoise = copyFrom.rotationNoise;
		scaleNoise = copyFrom.scaleNoise;
		dragPlaneAxes = copyFrom.dragPlaneAxes;
		dragSpringConstant = copyFrom.dragSpringConstant;
		dragSpringDampening = copyFrom.dragSpringDampening;
		dragRotationSpeedMax = copyFrom.dragRotationSpeedMax;
		dragRotationRatioMax = copyFrom.dragRotationRatioMax;
		dragTransitionRadius = copyFrom.dragTransitionRadius;
		dragTransitionCurvePower = copyFrom.dragTransitionCurvePower;
		destroyOnAnimateOutComplete = copyFrom.destroyOnAnimateOutComplete;
		addPointerOverColliderScaler = copyFrom.addPointerOverColliderScaler;
		pointerOverColliderScale = copyFrom.pointerOverColliderScale;
		return this;
	}

	private void _SetObject(Snapshot snapShot)
	{
		SetObject(snapShot.slottedTransform.gameObject, snapShot.slotTransformData, snapShot.velocity);
	}

	public void SetObject(GameObject gameObjectToSetIntoSlot, TransformData? initialTransformData = null, TransformTarget? initialVelocity = null)
	{
		_UnslotObject();
		if (!gameObjectToSetIntoSlot)
		{
			return;
		}
		slot.localScale = Vector3.one;
		_SlotObject(gameObjectToSetIntoSlot);
		if (!animateOnlyWhileDragging)
		{
			if (initialTransformData.HasValue)
			{
				initialTransformData.Value.SetValues(slot);
			}
			else
			{
				slot.transform.CopyFrom(into);
			}
		}
		_velocity = initialVelocity.GetValueOrDefault();
		_ResetRotation(ref _velocity, ref _targetOriginalRotation);
	}

	public void SetObjectImmediate(GameObject gameObjectToSetIntoSlot)
	{
		SetObject(gameObjectToSetIntoSlot, new TransformData(rest.transform), default(TransformTarget));
	}

	public void Clear()
	{
		_UnslotObject();
		if ((bool)animatingOutTransform)
		{
			_OnAnimateOutComplete(animatingOutTransform);
		}
	}

	public void SimulateOnDrop(GameObject pointerDrag)
	{
		if (!pointerDrag)
		{
			return;
		}
		UILayout3DSlot componentInParent = pointerDrag.GetComponentInParent<UILayout3DSlot>();
		if (!componentInParent)
		{
			return;
		}
		componentInParent = ((componentInParent.slottedTransform == pointerDrag.transform) ? componentInParent : null);
		if (this == componentInParent)
		{
			return;
		}
		AUILayout3D aUILayout3D = layout;
		AUILayout3D aUILayout3D2 = (componentInParent ? componentInParent.layout : null);
		Snapshot? snapshot = (slottedTransform ? new Snapshot?(new Snapshot(this)) : null);
		Snapshot? snapshot2 = (componentInParent ? new Snapshot?(new Snapshot(componentInParent)) : null);
		if (snapshot2.HasValue)
		{
			if (snapshot.HasValue)
			{
				componentInParent._SetObject(snapshot.Value);
			}
			else
			{
				componentInParent.SetObject(null);
			}
			_SetObject(snapshot2.Value);
		}
		else
		{
			SetObject(pointerDrag, pointerDrag.transform);
		}
		if (!(aUILayout3D != aUILayout3D2))
		{
			return;
		}
		if (snapshot.HasValue)
		{
			OnSlottedObjectTransferredToExternalLayout.Invoke(snapshot.Value.slottedTransform.gameObject, aUILayout3D2);
		}
		if (snapshot2.HasValue)
		{
			OnObjectTransferredFromExternalLayout.Invoke(snapshot2.Value.slottedTransform.gameObject, aUILayout3D2);
		}
		if ((bool)componentInParent)
		{
			if (snapshot.HasValue)
			{
				componentInParent.OnObjectTransferredFromExternalLayout.Invoke(snapshot.Value.slottedTransform.gameObject, aUILayout3D);
			}
			componentInParent.OnSlottedObjectTransferredToExternalLayout.Invoke(snapshot2.Value.slottedTransform.gameObject, aUILayout3D);
		}
	}

	public void SetDragEnabled(bool isEnabled)
	{
		if (SetPropertyUtility.SetStruct(ref _dragEnabled, isEnabled) && !_dragEnabled && (bool)onDropCollider)
		{
			onDropCollider.gameObject.SetActive(value: false);
		}
	}
}
