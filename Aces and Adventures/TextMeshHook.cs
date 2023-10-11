using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextMeshHook : MonoBehaviour
{
	public string format = "#.##;-#.##;0";

	public string vectorLeftBrace = "(";

	public string vectorRightBrace = ")";

	public string vectorDelimiter = ", ";

	public string defaultText = "";

	public StringEvent onValueChanged;

	private string _previousText;

	private TextMeshProUGUI _text;

	public BoolEvent OnDefaultTextInputResponse;

	public TextMeshProUGUI text => _text ?? (_text = GetComponent<TextMeshProUGUI>());

	public Color32 color32
	{
		get
		{
			return text.color;
		}
		set
		{
			text.color = value;
		}
	}

	private void Awake()
	{
		vectorLeftBrace = vectorLeftBrace ?? "";
		vectorRightBrace = vectorRightBrace ?? "";
		vectorDelimiter = vectorDelimiter ?? "";
	}

	private void _CheckValueChanged()
	{
		if (_previousText != text.text)
		{
			_previousText = _text.text;
			if (onValueChanged != null)
			{
				onValueChanged.Invoke(_text.text);
			}
		}
	}

	public void SetText(string value)
	{
		text.text = value;
		_CheckValueChanged();
	}

	public void ToString(object obj)
	{
		text.text = defaultText + (obj ?? "NULL");
		_CheckValueChanged();
	}

	public void MaterialLightAngle(float radians)
	{
		text.fontMaterial.SetFloat("_LightAngle", radians);
	}

	public void IsDefaultTextInput(bool input)
	{
		OnDefaultTextInputResponse.Invoke(!input || !defaultText.Equals(text.text));
	}

	public void SetUnderline(bool isUnderlined)
	{
		_SetTextStyleFlag(FontStyles.Underline, isUnderlined);
	}

	public void SetItalic(bool isItalic)
	{
		_SetTextStyleFlag(FontStyles.Italic, isItalic);
	}

	public void SetBold(bool isBold)
	{
		_SetTextStyleFlag(FontStyles.Bold, isBold);
	}

	public void SetSmallCaps(bool isSmallCaps)
	{
		_SetTextStyleFlag(FontStyles.SmallCaps, isSmallCaps);
	}

	private void _SetTextStyleFlag(FontStyles flag, bool isOn)
	{
		text.fontStyle = (isOn ? (text.fontStyle | flag) : (text.fontStyle & ~flag));
	}

	public void SetInt(int value)
	{
		SetText(value.ToString(format));
	}

	public void SetInt2(Int2 value)
	{
		SetText(vectorLeftBrace + value.x.ToString(format) + vectorDelimiter + value.y.ToString(format) + vectorRightBrace);
	}

	public void SetFloat(float value)
	{
		SetText(value.ToString(format));
	}

	public void SetFloatRound(float value)
	{
		SetText(Mathf.RoundToInt(value).ToString(format));
	}

	public void SetFloatFloor(float value)
	{
		SetText(Mathf.FloorToInt(value).ToString(format));
	}

	public void SetFloatCeiling(float value)
	{
		SetText(Mathf.CeilToInt(value).ToString(format));
	}

	public void SetFloatPercentage(float value)
	{
		SetText((value * 100f).ToString(format) + "%");
	}

	public void SetVector2(Vector2 value)
	{
		SetText(vectorLeftBrace + value.x.ToString(format) + vectorDelimiter + value.y.ToString(format) + vectorRightBrace);
	}

	public void SetVector2Round(Vector2 value)
	{
		SetText(vectorLeftBrace + Mathf.RoundToInt(value.x).ToString(format) + vectorDelimiter + Mathf.RoundToInt(value.y).ToString(format) + vectorRightBrace);
	}

	public void SetVector2RoundWithPercentage(Vector2 value)
	{
		SetText(vectorLeftBrace + Mathf.RoundToInt(value.x).ToString(format) + vectorDelimiter + Mathf.RoundToInt(value.y).ToString(format) + vectorRightBrace + " (" + MathUtil.ToPercentIntSigned(value.x / value.y.InsureNonZero()) + "%)");
	}

	public void SetVector2Floor(Vector2 value)
	{
		SetText(vectorLeftBrace + Mathf.FloorToInt(value.x).ToString(format) + vectorDelimiter + Mathf.FloorToInt(value.y).ToString(format) + vectorRightBrace);
	}

	public void SetVector2Ceiling(Vector2 value)
	{
		SetText(vectorLeftBrace + Mathf.CeilToInt(value.x).ToString(format) + vectorDelimiter + Mathf.CeilToInt(value.y).ToString(format) + vectorRightBrace);
	}

	public void SetVector2RangeRound(Vector2 range)
	{
		SetText((range.Range() > 0f) ? (vectorLeftBrace + Mathf.RoundToInt(range.x).ToString(format) + vectorDelimiter + Mathf.RoundToInt(range.y).ToString(format) + vectorRightBrace) : range.x.ToString());
	}

	public void SetLongBraced(long value)
	{
		SetText(vectorLeftBrace + value.ToString(format) + vectorRightBrace);
	}
}
