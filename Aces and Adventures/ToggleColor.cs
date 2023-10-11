using UnityEngine;

public class ToggleColor : MonoBehaviour
{
	[ColorUsage(true, true)]
	public Color OnColor = Color.white;

	[ColorUsage(true, true)]
	public Color OffColor = Color.black;

	private bool? _isOn;

	public ColorEvent OnColorChange;

	public void SetIsOn(bool isOn)
	{
		if (_isOn != isOn)
		{
			_isOn = isOn;
			OnColorChange.Invoke(isOn ? OnColor : OffColor);
		}
	}

	public void SetIsOn(int isOn)
	{
		SetIsOn(isOn > 0);
	}
}
