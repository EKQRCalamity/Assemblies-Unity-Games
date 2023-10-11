using UnityEngine;

[RequireComponent(typeof(ToggleFloat))]
[DisallowMultipleComponent]
public class ToggleFloatInitializer : MonoBehaviour
{
	public bool initialValue = true;

	private void Awake()
	{
		ToggleFloat component = GetComponent<ToggleFloat>();
		component.toggle = initialValue;
		component.ForceFinish();
	}
}
