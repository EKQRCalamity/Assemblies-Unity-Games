using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PointerOver3D), typeof(PointerClick3D), typeof(PointerDrag3D))]
[RequireComponent(typeof(PointerScroll3D))]
public class Dial3D : MonoBehaviour, IShowCanDrag
{
	private static System.Random _random;

	[Header("Transforms")]
	public Transform rotationalAxisTransform;

	[SerializeField]
	protected Transform _radialTransform;

	[SerializeField]
	protected float _spannedDegrees = 180f;

	[SerializeField]
	protected bool _useSpannedDegreesPerValue;

	[SerializeField]
	[Range(0f, 3600f)]
	protected float _maxSpannedDegrees;

	[Header("Values")]
	[SerializeField]
	protected int _desiredValue;

	[SerializeField]
	protected int _minimumValue;

	[SerializeField]
	protected int _maximumValue = 100;

	[Header("Animation")]
	public bool useScaledTime;

	[Range(1f, 1000f)]
	public float rotationalSpringConstant = 200f;

	[Range(0.1f, 100f)]
	public float rotationalSpringDampening = 10f;

	[Range(-1f, 1f)]
	public float onTickAngularVelocityMultiplier = -0.333f;

	[Range(0f, 1f)]
	public float minTimeBetweenInvalidTickCues = 0.1f;

	[Range(-20f, 20f)]
	public float invalidTickVelocityMultiplier = 5f;

	[Header("Input")]
	public bool invertClickDirection;

	public bool invertDragDirection;

	public bool invertScrollDirection;

	public bool wrapValues;

	[Header("Sound")]
	[SerializeField]
	protected Dial3DSoundPack _soundPack;

	[Header("Events")]
	public IntEvent OnValueChanged;

	public IntEvent OnDesiredValueChanged;

	private PointerOver3D _pointerOver;

	private PointerClick3D _pointerClick;

	private PointerDrag3D _pointerDrag;

	private PointerScroll3D _pointerScroll;

	private int _value;

	private int _beginDragValue;

	private int _previousDragDesiredValue;

	private bool _isDirty;

	private Vector3 _beginDragPosition;

	private float _currentAngle;

	private float _angularVelocity;

	private bool _validClick;

	private float _timeOfLastInvalidTick;

	private bool _dragChangedValue;

	private static System.Random _Random => _random ?? (_random = new System.Random());

	public int desiredValue
	{
		get
		{
			return _desiredValue;
		}
		set
		{
			int num = _desiredValue;
			if (SetPropertyUtility.SetStruct(ref _desiredValue, value))
			{
				_SetDirty();
				if (num != _desiredValue)
				{
					OnDesiredValueChanged.Invoke(_desiredValue);
				}
			}
		}
	}

	public int value
	{
		get
		{
			return _value;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _value, Mathf.Clamp(value, minimumValue, maximumValue)))
			{
				_OnValueChanged();
			}
		}
	}

	public int minimumValue
	{
		get
		{
			return _minimumValue;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _minimumValue, value))
			{
				_SetDirty();
			}
		}
	}

	public int maximumValue
	{
		get
		{
			return _maximumValue;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _maximumValue, value))
			{
				_SetDirty();
			}
		}
	}

	public Int2 range
	{
		get
		{
			return new Int2(minimumValue, maximumValue);
		}
		set
		{
			minimumValue = value.x;
			maximumValue = value.y;
		}
	}

	public float desiredRatio => MathUtil.GetLerpAmount(minimumValue, maximumValue, desiredValue);

	public Transform radialTransform
	{
		get
		{
			return _radialTransform;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _radialTransform, value))
			{
				_SetDirty();
			}
		}
	}

	public float spannedRadians => spannedDegrees * (MathF.PI / 180f);

	public float spannedDegrees
	{
		get
		{
			if (!useSpannedDegreesPerValue)
			{
				return _spannedDegrees;
			}
			float result = _spannedDegrees * (float)Mathf.Max(1, numberOfNotches - 1);
			if (maxSpannedDegrees > 0f)
			{
				result = Mathf.Clamp(result, 0f - maxSpannedDegrees, maxSpannedDegrees);
			}
			return result;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _spannedDegrees, value))
			{
				_SetDirty();
			}
		}
	}

	public bool useSpannedDegreesPerValue
	{
		get
		{
			return _useSpannedDegreesPerValue;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _useSpannedDegreesPerValue, value))
			{
				_SetDirty();
			}
		}
	}

	public float maxSpannedDegrees
	{
		get
		{
			return _maxSpannedDegrees;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _maxSpannedDegrees, Mathf.Max(0f, value)))
			{
				_SetDirty();
			}
		}
	}

	public float radius => (radialTransform.position - rotationalAxisTransform.position).magnitude;

	public int numberOfNotches => Mathf.Max(1, maximumValue - minimumValue + 1);

	public float notchChordDistance => Math.Abs(spannedRadians / (float)numberOfNotches * radius).InsureNonZero();

	public float notchAngle => spannedDegrees / (float)Math.Max(1, numberOfNotches - 1);

	public PointerOver3D pointerOver => this.CacheComponent(ref _pointerOver);

	public PointerClick3D pointerClick => this.CacheComponent(ref _pointerClick);

	public PointerDrag3D pointerDrag => this.CacheComponent(ref _pointerDrag);

	public PointerScroll3D pointerScroll => this.CacheComponentSafe(ref _pointerScroll);

	public Dial3DSoundPack soundPack => _soundPack ?? (_soundPack = ScriptableObject.CreateInstance<Dial3DSoundPack>());

	private void _SetDirty()
	{
		_isDirty = true;
		MathUtil.InsureMinToMax(ref _minimumValue, ref _maximumValue);
		_desiredValue = (wrapValues ? _desiredValue : Mathf.Clamp(desiredValue, minimumValue, maximumValue));
	}

	private void _OnValueChanged()
	{
		_angularVelocity *= onTickAngularVelocityMultiplier;
		soundPack.onTick.PlaySafe(base.transform, _Random, soundPack.mixerGroup);
		OnValueChanged.Invoke(value);
	}

	private Vector3 _GetRotationalUpAxis(Camera eventCamera)
	{
		return Vector3.Cross(eventCamera.transform.forward, rotationalAxisTransform.forward).normalized * Mathf.Sign(spannedDegrees);
	}

	private PoolStructListHandle<int> _GetValueNotchesInAngleSpan(float startAngle, float endAngle)
	{
		PoolStructListHandle<int> poolStructListHandle = Pools.UseStructList<int>();
		bool num = Math.Sign(endAngle - startAngle) == Math.Sign(spannedDegrees);
		float f = startAngle / notchAngle;
		float f2 = endAngle / notchAngle;
		int num2 = (num ? Mathf.FloorToInt(f) : Mathf.CeilToInt(f));
		int num3 = (num ? Mathf.FloorToInt(f2) : Mathf.CeilToInt(f2));
		if (num2 == num3)
		{
			return poolStructListHandle;
		}
		int num4 = Math.Sign(num3 - num2);
		for (int i = num2 + num4; i != num3 + num4; i += num4)
		{
			poolStructListHandle.Add(i + minimumValue);
		}
		return poolStructListHandle;
	}

	private void _SignalInvalidTick(int valueOffset, bool ignoreTimeRestriction = false)
	{
		if (ignoreTimeRestriction || !(GameUtil.GetTime(useScaledTime) - _timeOfLastInvalidTick < minTimeBetweenInvalidTickCues))
		{
			soundPack.onInvalidTick.PlaySafe(base.transform, _Random, soundPack.mixerGroup);
			_isDirty = true;
			_angularVelocity -= (float)(Math.Sign(valueOffset) * Math.Sign(spannedDegrees)) * notchAngle * invalidTickVelocityMultiplier;
			_timeOfLastInvalidTick = GameUtil.GetTime(useScaledTime);
		}
	}

	private void _RegisterEvents()
	{
		pointerOver.OnEnter.AddListener(_OnPointerEnter);
		pointerOver.OnExit.AddListener(_OnPointerExit);
		pointerClick.OnDown.AddListener(_OnPointerDown);
		pointerClick.OnUp.AddListener(_OnPointerUp);
		pointerClick.OnClick.AddListener(_OnPointerClick);
		pointerDrag.OnInitialize.AddListener(_OnInitializePotentialDrag);
		pointerDrag.OnBegin.AddListener(_OnBeginDrag);
		pointerDrag.OnDragged.AddListener(_OnDrag);
		pointerDrag.OnEnd.AddListener(_OnEndDrag);
		pointerScroll.onScroll.AddListener(_OnPointerScroll);
	}

	private void _OnPointerEnter(PointerEventData eventData)
	{
		soundPack.onPointerEnter.PlaySafe(base.transform, _Random, soundPack.mixerGroup);
	}

	private void _OnPointerExit(PointerEventData eventData)
	{
	}

	private void _OnPointerDown(PointerEventData eventData)
	{
		_validClick = true;
		_beginDragPosition = eventData.pointerCurrentRaycast.worldPosition;
	}

	private void _OnPointerUp(PointerEventData eventData)
	{
	}

	private void _OnPointerClick(PointerEventData eventData)
	{
		if (_validClick)
		{
			soundPack.onClick.PlaySafe(base.transform, _Random, soundPack.mixerGroup);
			Camera pressEventCamera = eventData.pressEventCamera;
			Vector3 vector = new Plane(pressEventCamera.transform.forward, rotationalAxisTransform.position).ClosestPointOnPlane(pressEventCamera.ScreenPointToRay(Input.mousePosition));
			_TickValue(Math.Sign(Vector3.Dot(_GetRotationalUpAxis(pressEventCamera), vector - rotationalAxisTransform.position)) * invertClickDirection.ToInt(-1, 1));
		}
	}

	private void _TickValue(int valueOffset)
	{
		int num = desiredValue;
		desiredValue += valueOffset;
		if (desiredValue == num)
		{
			_SignalInvalidTick(valueOffset, ignoreTimeRestriction: true);
		}
	}

	private void _OnInitializePotentialDrag(PointerEventData eventData)
	{
		if (eventData.button != 0)
		{
			eventData.pointerDrag = null;
		}
	}

	private void _OnBeginDrag(PointerEventData eventData)
	{
		_validClick = false;
		_beginDragValue = desiredValue;
		soundPack.onBeginDrag.PlaySafe(base.transform, _Random, soundPack.mixerGroup);
		_dragChangedValue = false;
	}

	private void _OnDrag(PointerEventData eventData)
	{
		Camera pressEventCamera = eventData.pressEventCamera;
		Vector3 vector = new Plane(pressEventCamera.transform.forward, _beginDragPosition).ClosestPointOnPlane(pressEventCamera.ScreenPointToRay(Input.mousePosition));
		int num = desiredValue;
		int num2 = _beginDragValue + (int)(Vector3.Dot(_GetRotationalUpAxis(pressEventCamera), vector - _beginDragPosition) / notchChordDistance) * invertDragDirection.ToInt(-1, 1);
		_dragChangedValue |= _beginDragValue != num2;
		desiredValue = num2;
		if (num == desiredValue && desiredValue == value && Math.Abs(num2 - desiredValue) > Math.Abs(_previousDragDesiredValue - desiredValue))
		{
			_SignalInvalidTick(num2 - _previousDragDesiredValue);
		}
		_previousDragDesiredValue = num2;
	}

	private void _OnEndDrag(PointerEventData eventData)
	{
		soundPack.onEndDrag.PlaySafe(base.transform, _Random, soundPack.mixerGroup);
		if (!_dragChangedValue)
		{
			_validClick = eventData.pointerCurrentRaycast.gameObject == base.gameObject;
			_OnPointerClick(eventData);
		}
	}

	private void _OnPointerScroll(PointerEventData eventData)
	{
		Camera enterEventCamera = eventData.enterEventCamera;
		_TickValue(Math.Sign(Vector3.Dot(_GetRotationalUpAxis(enterEventCamera), (enterEventCamera.projectionMatrix * enterEventCamera.worldToCameraMatrix).inverse.MultiplyVector(eventData.scrollDelta.Unproject(AxisType.Z)))).InsureNonZero() * invertScrollDirection.ToInt(-1, 1));
	}

	private void Awake()
	{
		_RegisterEvents();
		Transform parent = base.gameObject.CreateParentGameObject().transform;
		rotationalAxisTransform.SetParent(parent, worldPositionStays: true);
		radialTransform.SetParent(parent, worldPositionStays: true);
		_SetDirty();
	}

	private void Update()
	{
		if (!_isDirty)
		{
			return;
		}
		float deltaTime = GameUtil.GetDeltaTime(useScaledTime);
		float currentAngle = _currentAngle;
		float num = spannedDegrees * desiredRatio;
		MathUtil.Spring(ref _currentAngle, ref _angularVelocity, num, rotationalSpringConstant, rotationalSpringDampening, deltaTime);
		foreach (int item in _GetValueNotchesInAngleSpan(currentAngle, _currentAngle))
		{
			value = (wrapValues ? MathUtil.Wrap(item, 0, minimumValue, maximumValue + 1) : item);
		}
		base.transform.rotation = Quaternion.AngleAxis(_currentAngle, rotationalAxisTransform.forward) * base.transform.parent.rotation;
		_isDirty = value != desiredValue || Math.Abs(_currentAngle - num) > 0.01f || _angularVelocity > 0.01f;
	}

	private void OnDisable()
	{
		SetInputEnabled(setEnabled: false);
	}

	private void OnEnable()
	{
		SetInputEnabled(setEnabled: true);
	}

	public void SetInputEnabled(bool setEnabled)
	{
		pointerOver.enabled = setEnabled;
		pointerClick.enabled = setEnabled;
		pointerDrag.enabled = setEnabled;
		pointerScroll.enabled = setEnabled;
	}

	public void Increment()
	{
		_TickValue(1);
	}

	public void Decrement()
	{
		_TickValue(-1);
	}

	public bool ShouldShowCanDrag()
	{
		return true;
	}
}
