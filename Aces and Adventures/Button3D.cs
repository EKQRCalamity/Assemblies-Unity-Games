using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PointerOver3D), typeof(PointerClick3D))]
public class Button3D : MonoBehaviour
{
	public enum State : byte
	{
		Rest,
		PointerOver,
		PointerDown,
		Click
	}

	[Flags]
	public enum StateFlags : byte
	{
		Rest = 1,
		PointerOver = 2,
		PointerDown = 4,
		Click = 8
	}

	private static System.Random _random;

	[Header("Transforms====================================================================================================================", order = -1)]
	[SerializeField]
	protected Transform _rest;

	[SerializeField]
	protected Transform _over;

	[SerializeField]
	protected Transform _down;

	[SerializeField]
	protected Transform _click;

	[Header("ANIMATION=====================================================================================================================", order = -1)]
	public bool useScaledTime;

	[Range(0.01f, 100f)]
	public float clickFinishedThresholdMultiplier = 100f;

	[Range(0f, 10f)]
	public float stopAnimationRestTime;

	[Header("Position Spring")]
	[Range(1f, 2000f)]
	public float positionSpringConstant = 1500f;

	[Range(1f, 100f)]
	public float positionSpringDampening = 25f;

	[Header("Rotation Spring")]
	[Range(1f, 2000f)]
	public float rotationSpringConstant = 1500f;

	[Range(1f, 100f)]
	public float rotationSpringDampening = 25f;

	[Header("Scale Spring")]
	[Range(1f, 2000f)]
	public float scaleSpringConstant = 1500f;

	[Range(1f, 100f)]
	public float scaleSpringDampening = 25f;

	[Header("SOUNDS========================================================================================================================", order = -1)]
	[SerializeField]
	protected Button3DSoundPack _soundPack;

	[Header("EVENTS========================================================================================================================", order = -1)]
	[SerializeField]
	[Range(0f, 1f)]
	protected float _pointerOverColliderScalePadding = 0.25f;

	[SerializeField]
	protected Vector3 _pointerOverColliderPerAxisScale = Vector3.one;

	[SerializeField]
	protected UnityEvent _OnClick;

	[SerializeField]
	protected UnityEvent _OnDown;

	[SerializeField]
	protected UnityEvent _OnUp;

	[SerializeField]
	protected UnityEvent _OnEnter;

	[SerializeField]
	protected UnityEvent _OnExit;

	private PointerOver3D _pointerOver3D;

	private PointerClick3D _pointerClick3D;

	private StateFlags _stateFlags = StateFlags.Rest;

	private State? _previousState;

	private TransformTarget _velocity;

	private Quaternion _targetOriginalRotation;

	private PointerOver3DColliderScaler _pointerOver3DColliderScaler;

	private float _elapsedRestTime;

	private Transform _slot;

	private static System.Random _Random => _random ?? (_random = new System.Random());

	protected Transform slot => _slot;

	private Transform over
	{
		get
		{
			if (!_over)
			{
				return _over = _rest;
			}
			return _over;
		}
	}

	private Transform down
	{
		get
		{
			if (!_down)
			{
				return _down = _rest;
			}
			return _down;
		}
	}

	private Transform click
	{
		get
		{
			if (!_click)
			{
				return _click = down;
			}
			return _click;
		}
	}

	protected Button3DSoundPack soundPack
	{
		get
		{
			return _soundPack ?? (_soundPack = ScriptableObject.CreateInstance<Button3DSoundPack>());
		}
		set
		{
			_soundPack = value;
		}
	}

	private UnityEvent OnClick => _OnClick ?? (_OnClick = new UnityEvent());

	private UnityEvent OnDown => _OnDown ?? (_OnDown = new UnityEvent());

	private UnityEvent OnUp => _OnUp ?? (_OnUp = new UnityEvent());

	private UnityEvent OnEnter => _OnEnter ?? (_OnEnter = new UnityEvent());

	private UnityEvent OnExit => _OnExit ?? (_OnExit = new UnityEvent());

	public StateFlags stateFlags
	{
		get
		{
			return _stateFlags;
		}
		private set
		{
			if (SetPropertyUtility.SetStruct(ref _stateFlags, value))
			{
				_elapsedRestTime = 0f;
			}
		}
	}

	private State _state
	{
		get
		{
			State state = EnumUtil<StateFlags>.ConvertFromFlag<State>(EnumUtil.MaxActiveFlag(stateFlags));
			if (_previousState == state)
			{
				return state;
			}
			if (_previousState == State.Click)
			{
				soundPack.onClickFinished.Play(slot, _Random, soundPack.mixerGroup);
			}
			_velocity = _velocity.ResetRotationData();
			_targetOriginalRotation = slot.localRotation;
			_previousState = state;
			return state;
		}
	}

	private Transform _targetTransform => _state switch
	{
		State.Rest => _rest, 
		State.PointerOver => over, 
		State.PointerDown => down, 
		State.Click => click, 
		_ => throw new ArgumentOutOfRangeException(), 
	};

	private PointerOver3DColliderScaler pointerOver3DColliderScaler
	{
		get
		{
			if (!_pointerOver3DColliderScaler)
			{
				return _pointerOver3DColliderScaler = this.GetOrAddComponent<PointerOver3DColliderScaler>();
			}
			return _pointerOver3DColliderScaler;
		}
	}

	protected PointerOver3D pointerOver3D => this.CacheComponent(ref _pointerOver3D);

	protected PointerClick3D pointerClick3D => this.CacheComponent(ref _pointerClick3D);

	protected virtual void _RegisterEvents()
	{
		SetInputEnabled(setEnabled: true);
		pointerOver3D.OnEnter.AddListener(_OnEnterHandler);
		pointerOver3D.OnExit.AddListener(_OnExitHandler);
		pointerClick3D.OnDown.AddListener(_OnDownHandler);
		pointerClick3D.OnUp.AddListener(_OnUpHandler);
		pointerClick3D.OnClick.AddListener(_OnClickHandler);
	}

	protected virtual void _UnregisterEvents()
	{
		pointerOver3D.OnEnter.RemoveListener(_OnEnterHandler);
		pointerOver3D.OnExit.RemoveListener(_OnExitHandler);
		pointerClick3D.OnDown.RemoveListener(_OnDownHandler);
		pointerClick3D.OnUp.RemoveListener(_OnUpHandler);
		pointerClick3D.OnClick.RemoveListener(_OnClickHandler);
		SetInputEnabled(setEnabled: false);
	}

	private void _OnEnterHandler(PointerEventData eventData)
	{
		stateFlags = EnumUtil.Add(stateFlags, StateFlags.PointerOver);
		OnEnter.Invoke();
		soundPack.onEnter.Play(slot, _Random, soundPack.mixerGroup);
	}

	private void _OnExitHandler(PointerEventData eventData)
	{
		stateFlags = EnumUtil.Subtract(stateFlags, StateFlags.PointerOver);
		OnExit.Invoke();
		soundPack.onExit.Play(slot, _Random, soundPack.mixerGroup);
	}

	private void _OnDownHandler(PointerEventData eventData)
	{
		stateFlags = EnumUtil.Add(stateFlags, StateFlags.PointerDown);
		OnDown.Invoke();
		soundPack.onDown.Play(slot, _Random, soundPack.mixerGroup);
	}

	private void _OnUpHandler(PointerEventData eventData)
	{
		stateFlags = EnumUtil.Subtract(stateFlags, StateFlags.PointerDown);
		OnUp.Invoke();
		soundPack.onUp.Play(slot, _Random, soundPack.mixerGroup);
	}

	private void _OnClickHandler(PointerEventData eventData)
	{
		stateFlags = EnumUtil.Add(stateFlags, StateFlags.Click);
		OnClick.Invoke();
		soundPack.onClick.Play(slot, _Random, soundPack.mixerGroup);
	}

	private void _UpdatePointerOverColliderScale()
	{
		if (!(_pointerOverColliderScalePadding <= 0f) || !(_pointerOverColliderPerAxisScale == Vector3.one) || (bool)_pointerOver3DColliderScaler)
		{
			pointerOver3DColliderScaler.scale = 1f + _pointerOverColliderScalePadding;
			pointerOver3DColliderScaler.perAxisScale = _pointerOverColliderPerAxisScale;
		}
	}

	protected bool _AnimateTransform(Transform transformToAnimate, ref TransformTarget velocity, Quaternion targetOriginalRotation, Transform animateTowards)
	{
		float deltaTime = GameUtil.GetDeltaTime(useScaledTime);
		TransformTarget current = new TransformTarget(transformToAnimate);
		TransformTarget transformTarget = new TransformTarget(animateTowards);
		transformTarget.originalRotation = targetOriginalRotation;
		TransformTarget transformTarget2 = transformTarget;
		TransformTarget.SpringToTarget(current, transformTarget2, ref velocity, positionSpringConstant, positionSpringDampening, rotationSpringConstant, rotationSpringDampening, scaleSpringConstant, scaleSpringDampening, deltaTime).SetTransformValues(transformToAnimate);
		bool flag = current.IsRoughlyEqual(transformTarget2, clickFinishedThresholdMultiplier, 0.01f);
		if (stopAnimationRestTime > 0f)
		{
			_elapsedRestTime = (flag ? (_elapsedRestTime + GameUtil.GetDeltaTime(useScaledTime)) : 0f);
		}
		return flag;
	}

	protected virtual void Awake()
	{
		GameObject gameObject = base.gameObject.CreateParentGameObject();
		if (!_rest)
		{
			GameObject gameObject2 = new GameObject("Rest");
			gameObject2.transform.CopyFrom(gameObject.transform);
			gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: true);
			_rest = gameObject2.transform;
		}
		GameObject gameObject3 = new GameObject("Slot");
		_slot = gameObject3.transform;
		_slot.SetParent(gameObject.transform, worldPositionStays: true);
		slot.CopyFrom(base.transform);
		base.transform.SetParent(slot, worldPositionStays: true);
		_rest.SetParent(gameObject.transform, worldPositionStays: true);
		over.SetParent(gameObject.transform, worldPositionStays: true);
		down.SetParent(gameObject.transform, worldPositionStays: true);
		click.SetParent(gameObject.transform, worldPositionStays: true);
		_UpdatePointerOverColliderScale();
	}

	private void OnEnable()
	{
		_RegisterEvents();
	}

	private void OnDisable()
	{
		_UnregisterEvents();
	}

	protected virtual void LateUpdate()
	{
		if (!(_elapsedRestTime > stopAnimationRestTime))
		{
			Transform targetTransform = _targetTransform;
			if (_AnimateTransform(slot, ref _velocity, _targetOriginalRotation, targetTransform) && EnumUtil.HasFlag(stateFlags, StateFlags.Click))
			{
				stateFlags = EnumUtil.Subtract(stateFlags, StateFlags.Click);
			}
		}
	}

	public void SetInputEnabled(bool setEnabled)
	{
		pointerOver3D.enabled = setEnabled;
		pointerClick3D.enabled = setEnabled;
	}

	public void SimulateClick()
	{
		OnClick.Invoke();
	}
}
