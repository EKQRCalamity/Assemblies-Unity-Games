using UnityEngine;

public class IntRelay : MonoBehaviour
{
	public IntEvent onValueChange;

	public FloatEvent onValueChangeFloat;

	public void SetValue(int value)
	{
		onValueChange?.Invoke(value);
		onValueChangeFloat?.Invoke(value);
	}
}
