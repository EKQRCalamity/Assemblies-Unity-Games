using UnityEngine;

public class FloatRemapper : MonoBehaviour
{
	public Vector2 inputRange = new Vector2(0f, 1f);

	public Vector2 outputRange = new Vector2(0f, 1f);

	public FloatEvent OnValueChanged;

	public void Input(float input)
	{
		OnValueChanged.Invoke(MathUtil.RemapUnclamped(input, inputRange, outputRange));
	}

	public void InputClamped(float input)
	{
		OnValueChanged.Invoke(MathUtil.Remap(input, inputRange, outputRange));
	}

	public void Input(bool input)
	{
		Input(input ? inputRange.y : inputRange.x);
	}
}
