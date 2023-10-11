using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Slider))]
public class SliderHook : MonoBehaviour, IDragHandler, IEventSystemHandler, IBeginDragHandler
{
	private float _lastMin = float.MaxValue;

	private float _lastMax = float.MinValue;

	private float _initialValue = float.MinValue;

	private bool? _lastIsWholeNumber;

	private Slider _slider;

	private RectTransform _rt;

	public FloatEvent OnValueDragged;

	public FloatEvent OnInitialValue;

	public FloatEvent OnMinChange;

	public FloatEvent OnMaxChange;

	public InputFieldContentEvent OnIsWholeNumberChange;

	public Slider slider => _slider ?? (_slider = GetComponent<Slider>());

	public RectTransform rt => _rt ?? (_rt = base.transform as RectTransform);

	private bool isVertical
	{
		get
		{
			if (slider.direction != Slider.Direction.BottomToTop)
			{
				return slider.direction == Slider.Direction.TopToBottom;
			}
			return true;
		}
	}

	private bool reverse
	{
		get
		{
			if (slider.direction != Slider.Direction.RightToLeft)
			{
				return slider.direction == Slider.Direction.TopToBottom;
			}
			return true;
		}
	}

	private void Awake()
	{
		_slider = GetComponent<Slider>();
	}

	private void Update()
	{
		if (!_lastIsWholeNumber.HasValue || _lastIsWholeNumber.Value != _slider.wholeNumbers)
		{
			OnIsWholeNumberChange.Invoke(_slider.wholeNumbers ? InputField.ContentType.IntegerNumber : InputField.ContentType.DecimalNumber);
			_lastIsWholeNumber = _slider.wholeNumbers;
		}
		if (_lastMin != _slider.minValue)
		{
			OnMinChange.SInvoke(ref _lastMin, _slider.minValue);
		}
		if (_lastMax != _slider.maxValue)
		{
			OnMaxChange.SInvoke(ref _lastMax, _slider.maxValue);
		}
		if (_initialValue == float.MinValue)
		{
			OnInitialValue.Invoke(_slider.value);
			_initialValue = _slider.value;
		}
	}

	public void ValueFromString(string value)
	{
		if (!value.IsNullOrEmpty())
		{
			slider.value = slider.value.TryParse(value);
		}
	}

	public void ValueFromObject(object value)
	{
		if (value != null)
		{
			ValueFromString(value.ToString());
		}
	}

	public void SetRange(Vector2 range)
	{
		slider.minValue = range.x;
		slider.maxValue = range.y;
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(base.transform as RectTransform, eventData.position, eventData.pressEventCamera, out var localPoint))
		{
			localPoint -= rt.rect.position;
			float num = ((!isVertical) ? Mathf.Clamp01(localPoint.x / rt.rect.width) : Mathf.Clamp01(localPoint.y / rt.rect.height));
			if (reverse)
			{
				num = 1f - num;
			}
			float arg = Mathf.Lerp(slider.minValue, slider.maxValue, num);
			OnValueDragged.Invoke(arg);
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		eventData.useDragThreshold = false;
	}
}
