using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIPositionAngle : Slider2D, IPointerClickHandler, IEventSystemHandler, ILayoutElement
{
	[Header("Angle")]
	public RectTransform angleIndicatorRect;

	public RectTransform angleLine;

	public RectTransform orthogonalAngleIndicatorRect;

	public RectTransform orthogonalAngleLine;

	[Range(0f, 180f)]
	public float angleDegreeStep;

	[Range(0.01f, 1f)]
	public float angleNudgeDegrees = 0.025f;

	[SerializeField]
	[Range(0f, 1.5f)]
	protected float _angleIndicatorOffsetScaler = 0.4f;

	[Header("Angle Events")]
	public FloatEvent OnAngleChanged;

	public BoolEvent OnFlipOrthogonalAxisChanged;

	public PositionAngleEvent OnPositionAngleChange;

	[Header("Layout")]
	[Range(1f, 3f)]
	public float preferredSizeScaler = 2f;

	[SerializeField]
	[HideInInspector]
	protected float _angle;

	[SerializeField]
	[HideInInspector]
	protected bool _flipOrthogonalAxis;

	private bool _hasNudgeFocus;

	public float angleIndicatorOffset
	{
		get
		{
			return _angleIndicatorOffsetScaler;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _angleIndicatorOffsetScaler, value))
			{
				_UpdateKnob();
			}
		}
	}

	public float angle
	{
		get
		{
			return _angle;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _angle, value))
			{
				_OnOrientChanged();
			}
		}
	}

	public bool flipOrthogonalAxis
	{
		get
		{
			return _flipOrthogonalAxis;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _flipOrthogonalAxis, value))
			{
				_OnFlipOrthogonalAxisChanged();
			}
		}
	}

	public PositionAngle positionAngle
	{
		get
		{
			return new PositionAngle(value, angle, flipOrthogonalAxis);
		}
		set
		{
			this.value = value.position;
			angle = value.angle;
			flipOrthogonalAxis = value.flipOrthogonalAxis;
		}
	}

	private bool _angleEnabled => angleIndicatorRect;

	protected override bool _useDragThreshold => _angleEnabled;

	protected override bool _setPositionOnPointerDown => !_angleEnabled;

	protected override bool _nudgeValue
	{
		get
		{
			if (_angleEnabled)
			{
				return !_hasNudgeFocus;
			}
			return true;
		}
	}

	public override Vector2 value
	{
		get
		{
			return base.value;
		}
		set
		{
			Vector2 vector = this.value;
			base.value = value;
			if (this.value != vector)
			{
				OnPositionAngleChange.Invoke(positionAngle);
			}
		}
	}

	public float minWidth
	{
		get
		{
			return 0f;
		}
		private set
		{
		}
	}

	public float preferredWidth
	{
		get
		{
			if (!(base.image != null))
			{
				return 0f;
			}
			return base.image.preferredWidth * preferredSizeScaler;
		}
		private set
		{
		}
	}

	public float flexibleWidth
	{
		get
		{
			return 0f;
		}
		private set
		{
		}
	}

	public float minHeight
	{
		get
		{
			return 0f;
		}
		private set
		{
		}
	}

	public float preferredHeight
	{
		get
		{
			if (!(base.image != null))
			{
				return 0f;
			}
			return base.image.preferredHeight * preferredSizeScaler;
		}
		private set
		{
		}
	}

	public float flexibleHeight
	{
		get
		{
			return 0f;
		}
		private set
		{
		}
	}

	public int layoutPriority
	{
		get
		{
			if (!base.image)
			{
				return -1;
			}
			return 1;
		}
		private set
		{
		}
	}

	private void _UpdateOrthogonalIndicator()
	{
		if ((bool)orthogonalAngleIndicatorRect)
		{
			orthogonalAngleIndicatorRect.localRotation = Quaternion.Euler(0f, 0f, angle * 57.29578f + (float)(flipOrthogonalAxis ? 90 : (-90)));
		}
	}

	private void _OnOrientChanged()
	{
		if (_angleEnabled)
		{
			angleIndicatorRect.localRotation = Quaternion.Euler(0f, 0f, angle * 57.29578f);
			_UpdateOrthogonalIndicator();
			OnAngleChanged.Invoke(angle);
			OnPositionAngleChange.Invoke(positionAngle);
			_UpdateKnob();
		}
	}

	private void _OnFlipOrthogonalAxisChanged()
	{
		_UpdateOrthogonalIndicator();
		OnFlipOrthogonalAxisChanged.Invoke(flipOrthogonalAxis);
		OnPositionAngleChange.Invoke(positionAngle);
		_UpdateKnob();
	}

	private void _NudgeAngle(int direction)
	{
		if (direction != 0)
		{
			angle += (float)direction * angleNudgeDegrees * (MathF.PI / 180f);
		}
	}

	private void _SetSpriteSafe(Sprite sprite)
	{
		if ((bool)this && (bool)base.image)
		{
			base.image.sprite = sprite;
		}
	}

	protected override void Start()
	{
		base.Start();
		_OnOrientChanged();
	}

	protected override void Update()
	{
		base.Update();
		if (_angleEnabled && !_nudgeValue && _isPointerInside)
		{
			int num = 0;
			if (InputManager.I[KeyCode.LeftArrow][KState.Clicked])
			{
				num++;
			}
			if (InputManager.I[KeyCode.RightArrow][KState.Clicked])
			{
				num--;
			}
			_NudgeAngle(num);
		}
	}

	private void LateUpdate()
	{
		_UpdateKnob();
	}

	protected override void _CommonDrag(PointerEventData eventData, bool isPointerDown = false)
	{
		base._CommonDrag(eventData, isPointerDown);
		_hasNudgeFocus = false;
	}

	protected override void _UpdateKnob()
	{
		base._UpdateKnob();
		if (!_angleEnabled)
		{
			return;
		}
		angleIndicatorRect.position = knob.position;
		angleIndicatorRect.localPosition += angleIndicatorRect.localRotation.Right() * (knob.rect.size * angleIndicatorOffset * MathUtil.OneOverSqrtTwo).magnitude;
		if ((bool)angleLine)
		{
			angleLine.localPosition = angleIndicatorRect.localPosition;
			angleLine.localRotation = angleIndicatorRect.localRotation;
			angleLine.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MathUtil.SqrtTwo * sliderArea.rect.size.AbsMax());
		}
		if ((bool)orthogonalAngleIndicatorRect)
		{
			orthogonalAngleIndicatorRect.position = knob.position;
			orthogonalAngleIndicatorRect.localPosition += orthogonalAngleIndicatorRect.localRotation.Right() * (knob.rect.size * angleIndicatorOffset * MathUtil.OneOverSqrtTwo).magnitude;
			if ((bool)orthogonalAngleLine)
			{
				orthogonalAngleLine.localPosition = orthogonalAngleIndicatorRect.localPosition;
				orthogonalAngleLine.localRotation = orthogonalAngleIndicatorRect.localRotation;
				orthogonalAngleLine.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MathUtil.SqrtTwo * sliderArea.rect.size.AbsMax());
			}
		}
	}

	public void SetImageRef(ImageRef imageRef)
	{
		if (imageRef.IsValid())
		{
			ImageCategoryType category = imageRef.category;
			if (category > ImageCategoryType.Adventure)
			{
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!eventData.IsClick())
		{
			_CommonDrag(eventData);
		}
		else if (!eventData.dragging)
		{
			_hasNudgeFocus = true;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(knob.parent as RectTransform, eventData.position, eventData.pressEventCamera, out var localPoint);
			Vector2 normalized = (localPoint - knob.localPosition.Project(AxisType.Z)).normalized;
			float num = Mathf.Atan2(normalized.y, normalized.x);
			if (_SnapModifierActive())
			{
				num = MathUtil.RoundToNearestMultipleOf(num, MathUtil.PIOver36);
			}
			if (angleDegreeStep > 0f)
			{
				num = MathUtil.RoundToNearestMultipleOf(num, angleDegreeStep * (MathF.PI / 180f));
			}
			angle = num;
		}
	}

	public void CalculateLayoutInputHorizontal()
	{
	}

	public void CalculateLayoutInputVertical()
	{
	}
}
