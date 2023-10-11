using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class RangeSlider : Selectable, IDragHandler, IEventSystemHandler, IInitializePotentialDragHandler, ICanvasElement, IUpdateSelectedHandler
{
	public enum KnobType : byte
	{
		None,
		Min,
		Translate,
		Max
	}

	public static readonly float MIN_RANGE_DISTANCE = MathUtil.BigEpsilon;

	protected RectTransform _rt;

	protected RectTransform _inputRect;

	[Space(12f)]
	[SerializeField]
	protected Slider.Direction _direction;

	protected RectTransform _knobsContainer;

	[Header("Knobs=====================================================================================================")]
	[SerializeField]
	protected RectTransform _minKnob;

	[SerializeField]
	protected RectTransform _translateKnob;

	[SerializeField]
	protected RectTransform _maxKnob;

	protected KnobType _dragKnobType;

	protected float _translateDragOffset;

	protected RectTransform _fillsContainer;

	[Header("Fills=====================================================================================================")]
	[SerializeField]
	protected RectTransform _minFill;

	protected Image _minFillImage;

	[SerializeField]
	protected RectTransform _currentRangeFill;

	protected Image _currentFillImage;

	[SerializeField]
	protected RectTransform _maxFill;

	protected Image _maxFillImage;

	[Header("Extrema===================================================================================================")]
	[SerializeField]
	protected float _desiredMinRange;

	[SerializeField]
	protected float _desiredMaxRange = 1f;

	[SerializeField]
	[HideInInspectorIf("hideMinRange", true)]
	protected float _minRange;

	private float _minRangePrevious = float.MaxValue;

	[SerializeField]
	[HideInInspectorIf("hideMaxRange", true)]
	protected float _maxRange;

	private float _maxRangePrevious = float.MinValue;

	[SerializeField]
	protected float _desiredMinRangeClamp = float.MinValue;

	[SerializeField]
	[HideInInspectorIf("hideMinRangeClamp", true)]
	protected float _minRangeClamp;

	private float _minRangeClampPrevious = float.MinValue;

	[SerializeField]
	protected float _desiredMaxRangeClamp = float.MaxValue;

	[SerializeField]
	[HideInInspectorIf("hideMaxRangeClamp", true)]
	protected float _maxRangeClamp;

	[SerializeField]
	protected float _desiredMinRangeMaxClamp = float.MaxValue;

	[SerializeField]
	[HideInInspectorIf("hideMinRangeMaxClamp", true)]
	protected float _minRangeMaxClamp;

	[SerializeField]
	protected float _desiredMaxRangeMinClamp = float.MinValue;

	[SerializeField]
	[HideInInspectorIf("hideMaxRangeMinClamp", true)]
	protected float _maxRangeMinClamp;

	[SerializeField]
	protected float _desiredMinDistance;

	[SerializeField]
	protected float _desiredMaxDistance;

	[SerializeField]
	[HideInInspectorIf("hideMinDistance", true)]
	protected float _minDistance;

	private float _minDistancePrevious = float.MaxValue;

	[SerializeField]
	[HideInInspectorIf("hideMaxDistance", true)]
	protected float _maxDistance;

	[Header("Stepping==================================================================================================")]
	[SerializeField]
	protected float _stepSize = 0.01f;

	[SerializeField]
	protected bool _integerSteps;

	private bool? _previousIntegerSteps;

	[SerializeField]
	[Range(1f, 100f)]
	protected int _stepsPerNudge = 1;

	protected int previousNudgeDir;

	[Header("Snapping==================================================================================================")]
	[Range(0f, 1f)]
	[SerializeField]
	protected float _snapDistance;

	public float desiredSnapValue;

	[Header("Current Values============================================================================================")]
	[SerializeField]
	protected float _min = 0.25f;

	private float _minPrevious = float.MaxValue;

	[SerializeField]
	protected float _max = 0.75f;

	private float _maxPrevious = float.MinValue;

	[Header("Float Events==============================================================================================")]
	public FloatEvent onMinChanged;

	public FloatEvent onMaxChanged;

	public Vector2Event onCurrentRangeChanged;

	[Header("Int Events================================================================================================")]
	public BoolEvent onIntegerStepsChanged;

	public IntEvent onMinChangedInt;

	public IntEvent onMaxChangedInt;

	public Int2Event onCurrentRangeChangedInt;

	[Header("Range Events==============================================================================================")]
	public RangeFEvent onRangeFChanged;

	public RangeIntEvent onRangeIntChanged;

	public RangeByteEvent onRangeByteChanged;

	protected DrivenRectTransformTracker _tracker;

	private int _axis
	{
		get
		{
			if (isVertical)
			{
				return 1;
			}
			return 0;
		}
	}

	private int _freeAxis
	{
		get
		{
			if (isVertical)
			{
				return 0;
			}
			return 1;
		}
	}

	public float rangeExtremaDistance => _maxRange - _minRange;

	public float currentRangeDistance => _max - _min;

	public Slider.Direction direction
	{
		get
		{
			return _direction;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _direction, value))
			{
				_UpdateTransforms();
			}
		}
	}

	public bool isVertical
	{
		get
		{
			if (_direction != Slider.Direction.BottomToTop)
			{
				return _direction == Slider.Direction.TopToBottom;
			}
			return true;
		}
	}

	public bool reverse
	{
		get
		{
			if (_direction != Slider.Direction.RightToLeft)
			{
				return _direction == Slider.Direction.TopToBottom;
			}
			return true;
		}
	}

	public RectTransform minKnob
	{
		get
		{
			return _minKnob;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _minKnob, value))
			{
				_UpdateCachedReferences();
				_UpdateTransforms();
			}
		}
	}

	public RectTransform translateKnob
	{
		get
		{
			return _translateKnob;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _translateKnob, value))
			{
				_UpdateCachedReferences();
				_UpdateTransforms();
			}
		}
	}

	public RectTransform maxKnob
	{
		get
		{
			return _maxKnob;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _maxKnob, value))
			{
				_UpdateCachedReferences();
				_UpdateTransforms();
			}
		}
	}

	public RectTransform minFill
	{
		get
		{
			return _minFill;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _minFill, value))
			{
				_UpdateCachedReferences();
				_UpdateTransforms();
			}
		}
	}

	public RectTransform currentRangeFill
	{
		get
		{
			return _currentRangeFill;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _currentRangeFill, value))
			{
				_UpdateCachedReferences();
				_UpdateTransforms();
			}
		}
	}

	public RectTransform maxFill
	{
		get
		{
			return _maxFill;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _maxFill, value))
			{
				_UpdateCachedReferences();
				_UpdateTransforms();
			}
		}
	}

	public float minRange
	{
		get
		{
			return _desiredMinRange;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _desiredMinRange, value))
			{
				_UpdateRanges();
			}
		}
	}

	public int minRangeInt
	{
		get
		{
			return Mathf.RoundToInt(_desiredMinRange);
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _desiredMinRange, Mathf.RoundToInt(value)))
			{
				_UpdateRanges();
			}
		}
	}

	public float maxRange
	{
		get
		{
			return _desiredMaxRange;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _desiredMaxRange, value))
			{
				_UpdateRanges();
			}
		}
	}

	public int maxRangeInt
	{
		get
		{
			return Mathf.RoundToInt(_desiredMaxRange);
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _desiredMaxRange, Mathf.RoundToInt(value)))
			{
				_UpdateRanges();
			}
		}
	}

	public float minRangeClamp
	{
		get
		{
			return _desiredMinRangeClamp;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _desiredMinRangeClamp, value))
			{
				_UpdateRanges();
			}
		}
	}

	public float minRangeClampNormalized
	{
		get
		{
			return MathUtil.GetLerpAmount(_minRange, _maxRange, _minRangeClamp);
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _desiredMinRangeClamp, Mathf.Lerp(_minRange, _maxRange, value)))
			{
				_UpdateRanges();
			}
		}
	}

	public float maxRangeClamp
	{
		get
		{
			return _desiredMaxRangeClamp;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _desiredMaxRangeClamp, value))
			{
				_UpdateRanges();
			}
		}
	}

	public float maxRangeClampNormalized
	{
		get
		{
			return MathUtil.GetLerpAmount(_minRange, _maxRange, _maxRangeClamp);
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _desiredMaxRangeClamp, Mathf.Lerp(_minRange, _maxRange, value)))
			{
				_UpdateRanges();
			}
		}
	}

	public float minRangeMaxClamp
	{
		get
		{
			return _desiredMinRangeMaxClamp;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _desiredMinRangeMaxClamp, value))
			{
				_UpdateRanges();
			}
		}
	}

	public float minRangeMaxClampNormalized
	{
		get
		{
			return MathUtil.GetLerpAmount(_minRange, _maxRange, _minRangeMaxClamp);
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _desiredMinRangeMaxClamp, Mathf.Lerp(_minRange, _maxRange, value)))
			{
				_UpdateRanges();
			}
		}
	}

	public float maxRangeMinClamp
	{
		get
		{
			return _desiredMaxRangeMinClamp;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _desiredMaxRangeMinClamp, value))
			{
				_UpdateRanges();
			}
		}
	}

	public float maxRangeMinClampNormalized
	{
		get
		{
			return MathUtil.GetLerpAmount(_minRange, _maxRange, _maxRangeMinClamp);
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _desiredMaxRangeMinClamp, Mathf.Lerp(_minRange, _maxRange, value)))
			{
				_UpdateRanges();
			}
		}
	}

	public Vector2 rangeExtrema
	{
		get
		{
			return new Vector2(_desiredMinRange, _desiredMaxRange);
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _desiredMinRange, value.x) | SetPropertyUtility.SetStruct(ref _desiredMaxRange, value.y))
			{
				_UpdateRanges();
			}
		}
	}

	public float minDistance
	{
		get
		{
			return _desiredMinDistance;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _desiredMinDistance, value))
			{
				_UpdateRanges();
			}
		}
	}

	public int minDistanceInt
	{
		get
		{
			return Mathf.RoundToInt(_desiredMinDistance);
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _desiredMinDistance, Mathf.RoundToInt(value)))
			{
				_UpdateRanges();
			}
		}
	}

	public float maxDistance
	{
		get
		{
			return _desiredMaxDistance;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _desiredMaxDistance, value))
			{
				_UpdateRanges();
			}
		}
	}

	public int maxDistanceInt
	{
		get
		{
			return Mathf.RoundToInt(_desiredMaxDistance);
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _desiredMaxDistance, Mathf.RoundToInt(value)))
			{
				_UpdateRanges();
			}
		}
	}

	public float stepSize
	{
		get
		{
			return _stepSize;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _stepSize, value))
			{
				_UpdateRanges();
			}
		}
	}

	public int stepSizeInt
	{
		get
		{
			return Mathf.RoundToInt(_stepSize);
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _stepSize, Mathf.RoundToInt(value)))
			{
				_UpdateRanges();
			}
		}
	}

	public bool integerSteps
	{
		get
		{
			return _integerSteps;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _integerSteps, value))
			{
				_UpdateRanges();
			}
		}
	}

	public float effectiveStepSize
	{
		get
		{
			float num = (_integerSteps ? ((float)Math.Max(1, Mathf.RoundToInt(_stepSize))) : _stepSize);
			if (!(num > 0f))
			{
				return MIN_RANGE_DISTANCE;
			}
			return num;
		}
	}

	public int stepsPerNudge
	{
		get
		{
			return _stepsPerNudge;
		}
		set
		{
			_stepsPerNudge = Math.Max(1, value);
		}
	}

	public bool snapEnabled
	{
		get
		{
			if (_snapDistance > 0f && desiredSnapValue >= _minRange)
			{
				return desiredSnapValue <= _maxRange;
			}
			return false;
		}
	}

	public float snapDistance
	{
		get
		{
			return _snapDistance;
		}
		set
		{
			_snapDistance = Mathf.Clamp01(value);
		}
	}

	public float snapValue
	{
		get
		{
			return MathUtil.RoundToNearestMultipleOf(Mathf.Clamp(desiredSnapValue, _minRange, _maxRange), effectiveStepSize);
		}
		set
		{
			desiredSnapValue = value;
		}
	}

	public float snapValueNormalized
	{
		get
		{
			return MathUtil.GetLerpAmount(_minRange, _maxRange, snapValue);
		}
		set
		{
			desiredSnapValue = Mathf.Lerp(_minRange, _maxRange, value);
		}
	}

	public float min
	{
		get
		{
			return _min;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _min, value))
			{
				_UpdateRanges();
			}
		}
	}

	public int minInt
	{
		get
		{
			return Mathf.RoundToInt(_min);
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _min, Mathf.RoundToInt(value)))
			{
				_UpdateRanges();
			}
		}
	}

	public float max
	{
		get
		{
			return _max;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _max, value))
			{
				_UpdateRanges();
			}
		}
	}

	public int maxInt
	{
		get
		{
			return Mathf.RoundToInt(_max);
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _max, Mathf.RoundToInt(value)))
			{
				_UpdateRanges();
			}
		}
	}

	public Vector2 currentRange
	{
		get
		{
			return new Vector2(_min, _max);
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _min, value.x) | SetPropertyUtility.SetStruct(ref _max, value.y))
			{
				_UpdateRanges();
			}
		}
	}

	public float normalizedMin
	{
		get
		{
			return MathUtil.GetLerpAmount(_minRange, _maxRange, _min);
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _min, Mathf.Lerp(_minRange, _maxRange, value)))
			{
				_UpdateRanges();
			}
		}
	}

	public float normalizedMax
	{
		get
		{
			return MathUtil.GetLerpAmount(_minRange, _maxRange, _max);
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _max, Mathf.Lerp(_minRange, _maxRange, value)))
			{
				_UpdateRanges();
			}
		}
	}

	public float normalizedMid
	{
		get
		{
			return (normalizedMin + normalizedMax) * 0.5f;
		}
		set
		{
			float num = normalizedMin;
			float num2 = normalizedMax;
			float num3 = (num + num2) * 0.5f;
			float num4 = value - num3;
			if (num2 + num4 > 1f)
			{
				num4 = 1f - num2;
			}
			else if (num + num4 < 0f)
			{
				num4 = 0f - num;
			}
			if (num4 != 0f)
			{
				num += num4;
				num2 += num4;
				_min = Mathf.Lerp(_minRange, _maxRange, num);
				_max = Mathf.Lerp(_minRange, _maxRange, num2);
				_UpdateRanges();
			}
		}
	}

	public float normalizedMidClamped
	{
		get
		{
			return (normalizedMin + normalizedMax) * 0.5f;
		}
		set
		{
			float num = normalizedMin;
			float num2 = normalizedMax;
			float num3 = (num + num2) * 0.5f;
			float num4 = value - num3;
			float num5 = minRangeClampNormalized;
			float num6 = maxRangeClampNormalized;
			if (num2 + num4 > num6)
			{
				num4 = num6 - num2;
			}
			else if (num + num4 < num5)
			{
				num4 = num5 - num;
			}
			if (num4 != 0f)
			{
				num += num4;
				num2 += num4;
				_min = Mathf.Lerp(_minRange, _maxRange, num);
				_max = Mathf.Lerp(_minRange, _maxRange, num2);
				_UpdateRanges();
			}
		}
	}

	public RangeF rangeF
	{
		get
		{
			return new RangeF(_min, _max, _minRange, _maxRange, _minDistance, _maxDistance);
		}
		set
		{
			_min = value.min;
			_max = value.max;
			_desiredMinRange = value.minRange;
			_desiredMaxRange = value.maxRange;
			_desiredMinDistance = value.minDistance;
			_desiredMaxDistance = value.maxDistance;
			_integerSteps = false;
			_UpdateRanges();
		}
	}

	public RangeInt rangeInt
	{
		get
		{
			return new RangeInt(minInt, maxInt, minRangeInt, maxRangeInt, minDistanceInt, maxDistanceInt);
		}
		set
		{
			_min = value.min;
			_max = value.max;
			_desiredMinRange = value.minRange;
			_desiredMaxRange = value.maxRange;
			_desiredMinDistance = value.minDistance;
			_desiredMaxDistance = value.maxDistance;
			_integerSteps = true;
			_UpdateRanges();
		}
	}

	public RangeByte rangeByte
	{
		get
		{
			return new RangeByte((byte)minInt, (byte)maxInt, (byte)minRangeInt, (byte)maxRangeInt, (byte)minDistanceInt, (byte)maxDistanceInt);
		}
		set
		{
			_min = (int)value.min;
			_max = (int)value.max;
			_desiredMinRange = (int)value.minRange;
			_desiredMaxRange = (int)value.maxRange;
			_desiredMinDistance = (int)value.minDistance;
			_desiredMaxDistance = (int)value.maxDistance;
			_integerSteps = true;
			_UpdateRanges();
		}
	}

	private void _UpdateRanges()
	{
		if (_previousIntegerSteps != _integerSteps)
		{
			onIntegerStepsChanged.Invoke(_integerSteps);
		}
		_previousIntegerSteps = _integerSteps;
		MathUtil.InsureMinToMax(_minPrevious, ref _min, ref _max);
		_minRange = _desiredMinRange;
		_maxRange = _desiredMaxRange;
		MathUtil.InsureMinToMax(_minRangePrevious, ref _minRange, ref _maxRange);
		MathUtil.InsureWithinDistances(ref _minRange, _minRangePrevious, ref _maxRange, _maxRangePrevious, (!_integerSteps) ? MIN_RANGE_DISTANCE : 1f);
		float multipleOf = effectiveStepSize;
		_minRange = MathUtil.FloorToNearestMultipleOf(_minRange, multipleOf);
		_maxRange = MathUtil.CeilingToNearestMultipleOf(_maxRange, multipleOf);
		_minRangeClamp = _desiredMinRangeClamp;
		_maxRangeClamp = _desiredMaxRangeClamp;
		_minRangeMaxClamp = _desiredMinRangeMaxClamp;
		_maxRangeMinClamp = _desiredMaxRangeMinClamp;
		MathUtil.InsureMinToMax(_minRangeClampPrevious, ref _minRangeClamp, ref _maxRangeClamp);
		_minRangeClamp = Math.Max(_minRangeClamp, _minRange);
		_maxRangeClamp = Math.Min(_maxRangeClamp, _maxRange);
		_minRangeMaxClamp = Math.Min(_minRangeMaxClamp, _maxRange);
		_maxRangeMinClamp = Math.Max(_maxRangeMinClamp, _minRange);
		_minRangeClamp = MathUtil.FloorToNearestMultipleOf(_minRangeClamp, multipleOf);
		_maxRangeClamp = MathUtil.CeilingToNearestMultipleOf(_maxRangeClamp, multipleOf);
		_minRangeMaxClamp = MathUtil.CeilingToNearestMultipleOf(_minRangeMaxClamp, multipleOf);
		_maxRangeMinClamp = MathUtil.FloorToNearestMultipleOf(_maxRangeMinClamp, multipleOf);
		float num = rangeExtremaDistance;
		_minDistance = Math.Max(0f, Math.Min(_desiredMinDistance, num));
		_maxDistance = ((_desiredMaxDistance > 0f) ? Math.Min(_desiredMaxDistance, num) : num);
		MathUtil.InsureMinToMax(_minDistancePrevious, ref _minDistance, ref _maxDistance);
		_minDistance = Math.Min(num, MathUtil.CeilingToNearestMultipleOf(_minDistance, multipleOf));
		_maxDistance = Math.Max(_minDistance, MathUtil.FloorToNearestMultipleOf(_maxDistance, multipleOf));
		_min = Math.Min(Mathf.Clamp(_min, _minRangeClamp, _maxRangeClamp), _minRangeMaxClamp);
		_max = Math.Max(Mathf.Clamp(_max, _minRangeClamp, _maxRangeClamp), _maxRangeMinClamp);
		MathUtil.InsureWithinDistances(ref _min, _minPrevious, ref _max, _maxPrevious, _minDistance, _maxDistance, _minRange, _maxRange);
		if (_integerSteps)
		{
			int multipleOf2 = (int)effectiveStepSize;
			_min = MathUtil.RoundToNearestMultipleOfInt(_min, multipleOf2);
			_max = MathUtil.RoundToNearestMultipleOfInt(_max, multipleOf2);
		}
		else if (_stepSize > 0f)
		{
			_min = MathUtil.RoundToNearestMultipleOf(_min, multipleOf);
			_max = MathUtil.RoundToNearestMultipleOf(_max, multipleOf);
		}
		_min = Mathf.Clamp(_min, _minRange, _maxRange);
		_max = Mathf.Clamp(_max, _minRange, _maxRange);
		bool flag = false;
		if (_minPrevious != _min)
		{
			onMinChanged.Invoke(_min);
			flag = true;
			if (_integerSteps)
			{
				onMinChangedInt.Invoke(minInt);
			}
		}
		if (_maxPrevious != _max)
		{
			onMaxChanged.Invoke(_max);
			flag = true;
			if (_integerSteps)
			{
				onMaxChangedInt.Invoke(maxInt);
			}
		}
		if (flag)
		{
			onCurrentRangeChanged.Invoke(new Vector2(_min, _max));
			if (_integerSteps)
			{
				onCurrentRangeChangedInt.Invoke(new Int2(minInt, maxInt));
			}
		}
		bool flag2 = _minRangePrevious != _minRange || _maxRangePrevious != _maxRange;
		if (flag || flag2)
		{
			if (_integerSteps)
			{
				onRangeIntChanged.Invoke(rangeInt);
				onRangeByteChanged.Invoke(rangeByte);
			}
			else
			{
				onRangeFChanged.Invoke(rangeF);
			}
			_UpdateTransforms();
		}
		_minRangePrevious = _minRange;
		_maxRangePrevious = _maxRange;
		_minRangeClampPrevious = _minRangeClamp;
		_minDistancePrevious = _minDistance;
		_minPrevious = _min;
		_maxPrevious = _max;
	}

	private void _UpdateCachedReferences()
	{
		_rt = base.transform as RectTransform;
		_minFillImage = ((_minFill == null) ? null : _minFill.GetComponent<Image>());
		_currentFillImage = ((_currentRangeFill == null) ? null : _currentRangeFill.GetComponent<Image>());
		_maxFillImage = ((_maxFill == null) ? null : _maxFill.GetComponent<Image>());
		_knobsContainer = ((_minKnob != null) ? _minKnob.parent : ((_translateKnob != null) ? _translateKnob.parent : ((_maxKnob != null) ? _maxKnob.parent : base.transform))) as RectTransform;
		_fillsContainer = ((_minFill != null) ? _minFill.parent : ((_currentRangeFill != null) ? _currentRangeFill.parent : ((_maxFill != null) ? _maxFill.parent : base.transform))) as RectTransform;
		_inputRect = ((_knobsContainer != _rt) ? _knobsContainer : ((_fillsContainer != _rt) ? _fillsContainer : _rt));
	}

	private void _TrackerCommon(RectTransform rectToTrack)
	{
		if (!isVertical)
		{
			_tracker.Add(this, rectToTrack, DrivenTransformProperties.AnchorMinX);
			_tracker.Add(this, rectToTrack, DrivenTransformProperties.AnchorMaxX);
		}
		else
		{
			_tracker.Add(this, rectToTrack, DrivenTransformProperties.AnchorMinY);
			_tracker.Add(this, rectToTrack, DrivenTransformProperties.AnchorMaxY);
		}
	}

	private void _AnchorsCommon(RectTransform rectTransform, out Vector2 anchorMin, out Vector2 anchorMax)
	{
		anchorMin = Vector2.zero;
		anchorMin[_freeAxis] = rectTransform.anchorMin[_freeAxis];
		anchorMax = Vector2.one;
		anchorMax[_freeAxis] = rectTransform.anchorMax[_freeAxis];
	}

	private void _UpdateFill(RectTransform fillRect, Image fillImage, float minAnchor, float maxAnchor)
	{
		_TrackerCommon(fillRect);
		_AnchorsCommon(fillRect, out var anchorMin, out var anchorMax);
		if (fillImage != null && fillImage.type == Image.Type.Filled)
		{
			fillImage.fillAmount = minAnchor;
		}
		else
		{
			anchorMin[_axis] = minAnchor;
			anchorMax[_axis] = maxAnchor;
		}
		fillRect.anchorMin = anchorMin;
		fillRect.anchorMax = anchorMax;
	}

	private void _UpdateKnob(RectTransform knobRect, float minAnchor, float maxAnchor)
	{
		_TrackerCommon(knobRect);
		_AnchorsCommon(knobRect, out var anchorMin, out var anchorMax);
		anchorMin[_axis] = minAnchor;
		anchorMax[_axis] = maxAnchor;
		knobRect.anchorMin = anchorMin;
		knobRect.anchorMax = anchorMax;
	}

	private void _UpdateTransforms()
	{
		_tracker.Clear();
		float a = normalizedMin;
		float b = normalizedMax;
		if (reverse)
		{
			a = 1f - a;
			b = 1f - b;
			MathUtil.Swap(ref a, ref b);
		}
		float num = (a + b) * 0.5f;
		if (_minKnob != null)
		{
			_UpdateKnob(_minKnob, a, a);
		}
		if (_translateKnob != null)
		{
			_UpdateKnob(_translateKnob, (a + num) * 0.5f, (num + b) * 0.5f);
		}
		if (_maxKnob != null)
		{
			_UpdateKnob(_maxKnob, b, b);
		}
		if (_minFill != null)
		{
			_UpdateFill(_minFill, _minFillImage, 0f, a);
		}
		if (_currentRangeFill != null)
		{
			_UpdateFill(_currentRangeFill, _currentFillImage, a, b);
		}
		if (_maxFill != null)
		{
			_UpdateFill(_maxFill, _maxFillImage, b, 1f);
		}
	}

	private KnobType _CommonPointerResponse(PointerEventData eventData, KnobType? knobOverride = null, bool isDrag = false)
	{
		KnobType result = KnobType.None;
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_inputRect, eventData.position, eventData.pressEventCamera, out var localPoint))
		{
			return result;
		}
		localPoint -= _inputRect.rect.position;
		float num = ((!isVertical) ? Mathf.Clamp01(localPoint.x / _inputRect.rect.width) : Mathf.Clamp01(localPoint.y / _inputRect.rect.height));
		if (reverse)
		{
			num = 1f - num;
		}
		float num2 = normalizedMin;
		float num3 = normalizedMax;
		float num4 = (num2 + num3) * 0.5f;
		float num5 = Math.Abs(num2 - num);
		float num6 = num4 - num;
		float num7 = Math.Abs(num6);
		float num8 = Math.Abs(num3 - num);
		if (num2 == num3)
		{
			num8 += num6;
			num5 -= num6;
		}
		knobOverride = knobOverride.GetValueOrDefault();
		if (isDrag && snapEnabled)
		{
			float num9 = snapValueNormalized - ((knobOverride.Value != KnobType.Translate) ? 0f : _translateDragOffset);
			if (Math.Abs(num - num9) <= _snapDistance)
			{
				num = num9;
			}
		}
		if (!isDrag)
		{
			_translateDragOffset = num4 - num;
		}
		if (knobOverride.Value == KnobType.Min || (knobOverride.Value == KnobType.None && num5 < num7))
		{
			normalizedMin = num;
			return KnobType.Min;
		}
		if (knobOverride.Value == KnobType.Max || (knobOverride.Value == KnobType.None && num8 < num7))
		{
			normalizedMax = num;
			return KnobType.Max;
		}
		if (isDrag)
		{
			normalizedMidClamped = num + _translateDragOffset;
		}
		return KnobType.Translate;
	}

	public void SetMin(string value)
	{
		if (!value.IsNullOrEmpty())
		{
			if (integerSteps)
			{
				minInt = minInt.TryParse(value);
			}
			else
			{
				min = min.TryParse(value);
			}
		}
	}

	public void SetMax(string value)
	{
		if (!value.IsNullOrEmpty())
		{
			if (integerSteps)
			{
				maxInt = maxInt.TryParse(value);
			}
			else
			{
				max = max.TryParse(value);
			}
		}
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		_UpdateTransforms();
	}

	protected override void Awake()
	{
		base.Awake();
		_UpdateCachedReferences();
		_UpdateRanges();
		_UpdateTransforms();
	}

	public override Selectable FindSelectableOnLeft()
	{
		if (base.navigation.mode == Navigation.Mode.Automatic)
		{
			return null;
		}
		return base.FindSelectableOnLeft();
	}

	public override Selectable FindSelectableOnRight()
	{
		if (base.navigation.mode == Navigation.Mode.Automatic)
		{
			return null;
		}
		return base.FindSelectableOnRight();
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		_CommonPointerResponse(eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
		_CommonPointerResponse(eventData, (_dragKnobType != 0) ? new KnobType?(_dragKnobType) : null, isDrag: true);
	}

	public void OnInitializePotentialDrag(PointerEventData eventData)
	{
		eventData.useDragThreshold = false;
		_dragKnobType = _CommonPointerResponse(eventData);
	}

	public void GraphicUpdateComplete()
	{
	}

	public void LayoutComplete()
	{
	}

	public void Rebuild(CanvasUpdate executing)
	{
	}

	public void OnUpdateSelected(BaseEventData eventData)
	{
		if (_dragKnobType == KnobType.None || !EventSystem.current.IsPointerOverGameObject())
		{
			return;
		}
		int num = Math.Sign(Input.GetAxisRaw("Horizontal"));
		if (num == 0 || num == previousNudgeDir)
		{
			previousNudgeDir = num;
			return;
		}
		previousNudgeDir = num;
		float num2 = (float)(num * _stepsPerNudge) * effectiveStepSize;
		switch (_dragKnobType)
		{
		case KnobType.Min:
			min += num2;
			break;
		case KnobType.Translate:
		{
			float num3 = num2 / rangeExtremaDistance;
			normalizedMidClamped += num3;
			break;
		}
		case KnobType.Max:
			max += num2;
			break;
		}
	}

	[SpecialName]
	Transform ICanvasElement.get_transform()
	{
		return base.transform;
	}
}
