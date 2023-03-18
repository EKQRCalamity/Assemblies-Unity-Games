using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Actionables;

public class ActionableSwitch : MonoBehaviour, IActionable
{
	public bool isOn;

	public bool Locked { get; set; }

	public event Action<ActionableSwitch> OnSwitchUsed;

	[Button(ButtonSizes.Small)]
	public void Use()
	{
		ChangeState(!isOn);
	}

	public void ChangeState(bool turnOn)
	{
		isOn = turnOn;
		Debug.Log("SWITCH USED");
		if (this.OnSwitchUsed != null)
		{
			this.OnSwitchUsed(this);
		}
	}
}
