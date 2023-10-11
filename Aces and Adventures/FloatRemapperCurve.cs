using UnityEngine;

public class FloatRemapperCurve : MonoBehaviour
{
	public Vector2 inputRange = new Vector2(0f, 1f);

	public Vector2 outputRange = new Vector2(0f, 1f);

	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public FloatEvent onValueChanged;

	public void Input(float input)
	{
		onValueChanged?.Invoke(MathUtil.RemapUnclamped(input, inputRange, outputRange, curve));
	}

	public void InputClamped(float input)
	{
		onValueChanged?.Invoke(MathUtil.Remap(input, inputRange, outputRange, curve));
	}

	public void Input(bool input)
	{
		Input(input ? inputRange.y : inputRange.x);
	}
}
