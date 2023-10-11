using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slider2D : Selectable, IDragHandler, IEventSystemHandler, IInitializePotentialDragHandler
{
	private const float DRAG_LOCK_THRESHOLD = 0.05f;

	private static readonly Vector2 UNINITIALIZED_VALUE = new Vector2(float.MinValue, float.MinValue);

	[Header("Transforms")]
	public RectTransform sliderArea;

	public RectTransform knob;

	public RectTransform horizontalIndicator;

	public RectTransform verticalIndicator;

	[Header("Origin")]
	public Image.OriginHorizontal horizontalOrigin;

	public Image.OriginVertical verticalOrigin;

	[Header("Initial")]
	public Vector2 initialValue = Vector2.zero;

	[Header("Range")]
	public Vector2 min = Vector2.zero;

	public Vector2 max = Vector2.one;

	[Header("Stepping")]
	public Vector2 stepSize = Vector2.zero;

	[Range(0.00025f, 0.1f)]
	public float nudgeRatio = MathUtil.OneTwoFiftyFifth;

	private Vector2 _value = UNINITIALIZED_VALUE;

	private Vector2 _valueRightBeforePointerDown;

	private Vector2 _valueAtPointerDown;

	private bool? _horizontalDrag;

	protected bool _isPointerInside;

	[Header("Events")]
	public Vector2Event OnValueChange;

	public FloatEvent OnXChange;

	public FloatEvent OnYChange;

	protected virtual bool _useDragThreshold => false;

	protected virtual bool _setPositionOnPointerDown => true;

	protected virtual bool _nudgeValue => true;

	private bool _valueIsUninitialized => _value == UNINITIALIZED_VALUE;

	public virtual Vector2 value
	{
		get
		{
			return _value;
		}
		set
		{
			Vector2 vector = value.Step(stepSize);
			vector = value.Clamp(min, max);
			if (!(_value == vector))
			{
				Vector2 vector2 = _value;
				_value = vector;
				if (vector2.x != _value.x)
				{
					OnXChange.Invoke(_value.x);
				}
				if (vector2.y != _value.y)
				{
					OnYChange.Invoke(_value.y);
				}
				OnValueChange.Invoke(_value);
				_UpdateKnob();
			}
		}
	}

	public float x
	{
		get
		{
			return _value.x;
		}
		set
		{
			this.value = new Vector2(value, _value.y);
		}
	}

	public float y
	{
		get
		{
			return _value.y;
		}
		set
		{
			this.value = new Vector2(_value.x, value);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (_valueIsUninitialized)
		{
			_value = initialValue;
		}
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		if (_valueIsUninitialized)
		{
			_value = initialValue;
		}
		_UpdateKnob();
	}

	protected virtual void Update()
	{
		if (_nudgeValue && _isPointerInside)
		{
			Vector2 zero = Vector2.zero;
			if (InputManager.I[KeyCode.UpArrow][KState.Clicked])
			{
				zero += Vector2.up;
			}
			if (InputManager.I[KeyCode.DownArrow][KState.Clicked])
			{
				zero += Vector2.down;
			}
			if (InputManager.I[KeyCode.LeftArrow][KState.Clicked])
			{
				zero += Vector2.left;
			}
			if (InputManager.I[KeyCode.RightArrow][KState.Clicked])
			{
				zero += Vector2.right;
			}
			_NudgeValue(zero);
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		_horizontalDrag = null;
		_valueRightBeforePointerDown = _value;
		if (_setPositionOnPointerDown)
		{
			_CommonDrag(eventData, isPointerDown: true);
		}
		_valueAtPointerDown = _value;
		if (_LockModifierActive())
		{
			value = _valueRightBeforePointerDown;
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		_isPointerInside = true;
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		_isPointerInside = false;
	}

	protected virtual void _UpdateKnob()
	{
		if ((sliderArea != null) & (knob != null))
		{
			Vector2 lerpAmounts = min.GetLerpAmounts(max, _value);
			lerpAmounts = _CorrectLerpForOrigins(lerpAmounts);
			knob.anchorMin = lerpAmounts;
			knob.anchorMax = lerpAmounts;
			if ((bool)horizontalIndicator)
			{
				horizontalIndicator.anchorMin = new Vector2(0f, lerpAmounts.y);
				horizontalIndicator.anchorMax = new Vector2(1f, lerpAmounts.y);
			}
			if ((bool)verticalIndicator)
			{
				verticalIndicator.anchorMin = new Vector2(lerpAmounts.x, 0f);
				verticalIndicator.anchorMax = new Vector2(lerpAmounts.x, 1f);
			}
		}
	}

	private Vector2 _CorrectLerpForOrigins(Vector2 lerp)
	{
		if (horizontalOrigin != 0)
		{
			lerp.x = 1f - lerp.x;
		}
		if (verticalOrigin != 0)
		{
			lerp.y = 1f - lerp.y;
		}
		return lerp;
	}

	private Vector2 _RectLerpFromValue(Vector2 value)
	{
		Vector2 lerpAmounts = min.GetLerpAmounts(max, value);
		return _CorrectLerpForOrigins(lerpAmounts);
	}

	protected virtual void _CommonDrag(PointerEventData eventData, bool isPointerDown = false)
	{
		if (!(sliderArea != null) || !(sliderArea.rect.size.Min() > 0f) || !RectTransformUtility.ScreenPointToLocalPointInRectangle(sliderArea, eventData.position, eventData.pressEventCamera, out var localPoint))
		{
			return;
		}
		Vector2 vector = sliderArea.rect.GetLerpAmounts(localPoint).Clamp01();
		Vector2 amount = min.Lerp(max, _CorrectLerpForOrigins(vector));
		if (!isPointerDown && _LockModifierActive())
		{
			if (!_horizontalDrag.HasValue)
			{
				Vector2 v = vector - _RectLerpFromValue(_valueAtPointerDown);
				if (v.AbsMax() >= 0.05f)
				{
					_horizontalDrag = Math.Abs(v.x) > Math.Abs(v.y);
				}
			}
			if (_horizontalDrag.HasValue)
			{
				if (_horizontalDrag.Value)
				{
					amount.y = _valueRightBeforePointerDown.y;
				}
				else
				{
					amount.x = _valueRightBeforePointerDown.x;
				}
			}
			else
			{
				amount = _valueRightBeforePointerDown;
			}
		}
		if (!isPointerDown && _SnapModifierActive())
		{
			Vector2 vector2 = min.Lerp(max, 0.5f);
			Vector2 vector3 = min.GetLerpAmounts(max, amount) - new Vector2(0.5f, 0.5f);
			float num = 0.03f;
			if (Mathf.Abs(vector3.x) < num)
			{
				amount.x = vector2.x;
			}
			if (Mathf.Abs(vector3.y) < num)
			{
				amount.y = vector2.y;
			}
		}
		value = amount;
	}

	private void _NudgeValue(Vector2 direction)
	{
		if (!(direction == Vector2.zero))
		{
			value += (max - min).Multiply(_CorrectLerpForOrigins(direction)) * nudgeRatio;
		}
	}

	private bool _LockModifierActive()
	{
		if (!InputManager.I[KeyCode.LeftShift][KState.Down])
		{
			return InputManager.I[KeyCode.RightShift][KState.Down];
		}
		return true;
	}

	protected bool _SnapModifierActive()
	{
		if (!InputManager.I[KeyCode.LeftControl][KState.Down])
		{
			return InputManager.I[KeyCode.RightControl][KState.Down];
		}
		return true;
	}

	public void OnInitializePotentialDrag(PointerEventData eventData)
	{
		eventData.useDragThreshold = _useDragThreshold;
	}

	public void OnDrag(PointerEventData eventData)
	{
		_CommonDrag(eventData);
	}
}
