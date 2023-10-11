using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class InputFieldHook : MonoBehaviour
{
	public string floatFormat = "#.##;-#.##;0";

	public bool remapValues;

	public bool remapToIntegers = true;

	public Vector2 remapRange = new Vector2(0f, 255f);

	private float _previousValue;

	private float _minValue = float.MinValue;

	private float _maxValue = float.MaxValue;

	private Selectable _inputField;

	public FloatEvent OnEndEditFloat;

	private Selectable inputField
	{
		get
		{
			if ((bool)_inputField)
			{
				return _inputField;
			}
			_inputField = GetComponent<TMP_InputField>();
			if ((bool)_inputField)
			{
				((TMP_InputField)_inputField).onEndEdit.AddListener(OnEndEdit);
			}
			else
			{
				((InputField)(_inputField = GetComponent<InputField>())).onEndEdit.AddListener(OnEndEdit);
			}
			return _inputField;
		}
	}

	public float minValue
	{
		get
		{
			return _minValue;
		}
		set
		{
			_minValue = value;
		}
	}

	public float maxValue
	{
		get
		{
			return _maxValue;
		}
		set
		{
			_maxValue = value;
		}
	}

	private void _SetText(string text)
	{
		if (inputField is TMP_InputField)
		{
			((TMP_InputField)inputField).text = text;
		}
		else
		{
			((InputField)inputField).text = text;
		}
	}

	private bool _IsNumeric()
	{
		if (!(inputField is TMP_InputField))
		{
			return ((InputField)inputField).IsNumeric();
		}
		return ((TMP_InputField)inputField).IsNumeric();
	}

	public void Float(float value)
	{
		value = _Map(value);
		_previousValue = value;
		_SetText(value.ToString(floatFormat));
	}

	private void OnEndEdit(string value)
	{
		if (!_IsNumeric())
		{
			return;
		}
		if (!value.IsNullOrEmpty())
		{
			bool success;
			float num = Mathf.Clamp(_InverseMap(_previousValue.TryParse(value, out success)), minValue, maxValue);
			if (success)
			{
				Float(num);
			}
			else
			{
				_SetText(num.ToString(floatFormat));
			}
			OnEndEditFloat.Invoke(num);
		}
		else
		{
			Float(_InverseMap(_previousValue));
		}
	}

	public void IsInteger(bool isInteger)
	{
		SetContentType(isInteger ? InputField.ContentType.IntegerNumber : InputField.ContentType.DecimalNumber);
	}

	private float _Map(float value)
	{
		if (!remapValues)
		{
			return value;
		}
		value = Mathf.Lerp(remapRange.x, remapRange.y, MathUtil.GetLerpAmount(minValue, maxValue, value));
		if (remapToIntegers)
		{
			value = Mathf.RoundToInt(value);
		}
		return value;
	}

	private float _InverseMap(float value)
	{
		if (!remapValues)
		{
			return value;
		}
		value = Mathf.Lerp(minValue, maxValue, MathUtil.GetLerpAmount(remapRange.x, remapRange.y, value));
		return value;
	}

	public void SetContentType(InputField.ContentType contentType)
	{
		if (inputField is TMP_InputField)
		{
			((TMP_InputField)inputField).contentType = ((contentType == InputField.ContentType.IntegerNumber) ? TMP_InputField.ContentType.IntegerNumber : TMP_InputField.ContentType.DecimalNumber);
		}
		else
		{
			((InputField)inputField).contentType = contentType;
		}
	}
}
