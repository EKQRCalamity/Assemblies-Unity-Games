using UnityEngine;

public class Vector2Relay : MonoBehaviour
{
	public bool useScaledTime = true;

	public bool clearOnDisable;

	public bool setToZeroOnDisable;

	[Range(0f, 5f)]
	public float responseTimeX;

	[Range(0f, 5f)]
	public float responseTimeY;

	[Range(0.001f, 5f)]
	public float animationTimeX;

	[Range(0.001f, 5f)]
	public float animationTimeY;

	private Vector2 _value;

	private FloatRelayer _xRelay;

	private FloatRelayer _yRelay;

	public FloatEvent OnXChange;

	public IntEvent OnXChangeInt;

	public FloatEvent OnYChange;

	public IntEvent OnYChangeInt;

	public Vector2Event OnValueChange;

	public FloatEvent OnRatioChange;

	protected FloatRelayer xRelay => _xRelay ?? (_xRelay = new FloatRelayer());

	protected FloatRelayer yRelay => _yRelay ?? (_yRelay = new FloatRelayer());

	public float xValue => xRelay.value ?? _value.x;

	public float yValue => yRelay.value ?? _value.y;

	private void Update()
	{
		bool flag = false;
		Vector2 vector = new Vector2(xValue, yValue);
		float deltaTime = GameUtil.GetDeltaTime(useScaledTime);
		if (xRelay.Update(deltaTime))
		{
			flag = true;
			OnXChange.Invoke(xValue);
			if (Mathf.RoundToInt(vector.x) != Mathf.RoundToInt(xValue))
			{
				OnXChangeInt.Invoke(Mathf.RoundToInt(xValue));
			}
		}
		if (yRelay.Update(deltaTime))
		{
			flag = true;
			OnYChange.Invoke(yValue);
			if (Mathf.RoundToInt(vector.y) != Mathf.RoundToInt(yValue))
			{
				OnYChangeInt.Invoke(Mathf.RoundToInt(yValue));
			}
		}
		if (flag)
		{
			Vector2 vector2 = new Vector2(xValue, yValue);
			OnValueChange.Invoke(vector2);
			OnRatioChange.Invoke(Mathf.Clamp01(vector2.Ratio()));
		}
	}

	private void OnDisable()
	{
		if ((bool)this && clearOnDisable)
		{
			xRelay.Clear();
			yRelay.Clear();
			if (setToZeroOnDisable)
			{
				_value = Vector2.zero;
			}
		}
	}

	public void ChangeXValue(float x)
	{
		xRelay.Add(_value.x, x, animationTimeX, responseTimeX);
		_value.x = x;
	}

	public void ChangeYValue(float y)
	{
		yRelay.Add(_value.y, y, animationTimeY, responseTimeY);
		_value.y = y;
	}

	public void ChangeValue(Vector2 value)
	{
		ChangeXValue(value.x);
		ChangeYValue(value.y);
	}

	public void SetXVlaue(float x)
	{
		xRelay.Clear();
		xRelay.Add(x, x, 0f, 0f, 0);
		_value.x = x;
		Update();
	}

	public void SetYValue(float y)
	{
		yRelay.Clear();
		yRelay.Add(y, y, 0f, 0f, 0);
		_value.y = y;
		Update();
	}

	public void SetValue(Vector2 value)
	{
		float x = value.x;
		float y = value.y;
		xRelay.Clear();
		xRelay.Add(x, x, 0f, 0f, 0);
		_value.x = x;
		yRelay.Clear();
		yRelay.Add(y, y, 0f, 0f, 0);
		_value.y = y;
		Update();
	}
}
