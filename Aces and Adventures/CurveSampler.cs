using UnityEngine;

public class CurveSampler : MonoBehaviour
{
	public Vector2 inputRange = new Vector2(0f, 1f);

	public Vector2 outputRange = new Vector2(0f, 1f);

	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public FloatEvent OnCurveSampleChange;

	public void Input(float input)
	{
		OnCurveSampleChange.Invoke(Mathf.Lerp(outputRange.x, outputRange.y, curve.Evaluate(Mathf.Clamp01(MathUtil.GetLerpAmount(inputRange.x, inputRange.y, input)))));
	}
}
