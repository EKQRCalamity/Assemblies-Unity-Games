using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderStarter : MonoBehaviour
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
		Slider component = GetComponent<Slider>();
		component.onValueChanged.Invoke(component.value);
	}
}
