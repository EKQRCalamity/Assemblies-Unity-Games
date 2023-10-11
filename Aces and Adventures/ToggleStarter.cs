using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleStarter : MonoBehaviour
{
	public bool onAwake;

	private void Awake()
	{
		if (onAwake)
		{
			Init();
		}
	}

	private void Start()
	{
		if (!onAwake)
		{
			Init();
		}
	}

	private void Init()
	{
		Toggle component = GetComponent<Toggle>();
		component.onValueChanged.Invoke(component.isOn);
	}
}
