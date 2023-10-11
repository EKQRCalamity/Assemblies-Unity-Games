using UnityEngine;
using UnityEngine.UI;

public class FText : Text
{
	public string format = "#.##;-#.##;0";

	public string vectorLeftBrace;

	public string vectorRightBrace;

	public string vectorDelimiter;

	public void Float(float value)
	{
		text = value.ToString(format);
	}

	public void FloatRound(float value)
	{
		text = Mathf.RoundToInt(value).ToString(format);
	}

	public void FloatFloor(float value)
	{
		text = Mathf.FloorToInt(value).ToString(format);
	}

	public void FloatCeiling(float value)
	{
		text = Mathf.CeilToInt(value).ToString(format);
	}

	public void Vector2(Vector2 value)
	{
		text = vectorLeftBrace + value.x.ToString(format) + vectorDelimiter + value.y.ToString(format) + vectorRightBrace;
	}

	public void Vector2Round(Vector2 value)
	{
		text = vectorLeftBrace + Mathf.RoundToInt(value.x).ToString(format) + vectorDelimiter + Mathf.RoundToInt(value.y).ToString(format) + vectorRightBrace;
	}

	public void Vector2Floor(Vector2 value)
	{
		text = vectorLeftBrace + Mathf.FloorToInt(value.x).ToString(format) + vectorDelimiter + Mathf.FloorToInt(value.y).ToString(format) + vectorRightBrace;
	}

	public void Vector2Ceiling(Vector2 value)
	{
		text = vectorLeftBrace + Mathf.CeilToInt(value.x).ToString(format) + vectorDelimiter + Mathf.CeilToInt(value.y).ToString(format) + vectorRightBrace;
	}
}
