using System.Collections.Generic;
using Framework.Managers;
using Rewired;
using UnityEngine;

namespace Tools.UI;

public class InputEnableObjects : MonoBehaviour
{
	public List<GameObject> keyboardControls = new List<GameObject>();

	public List<GameObject> padControls = new List<GameObject>();

	private void Start()
	{
		Core.Input.JoystickPressed += RefreshLists;
		Core.Input.KeyboardPressed += RefreshLists;
		RefreshLists();
	}

	private void OnDestroy()
	{
		Core.Input.JoystickPressed -= RefreshLists;
		Core.Input.KeyboardPressed -= RefreshLists;
	}

	private void RefreshLists()
	{
		bool IsPad = Core.Input.ActiveControllerType == ControllerType.Joystick;
		keyboardControls.ForEach(delegate(GameObject p)
		{
			p.SetActive(!IsPad);
		});
		padControls.ForEach(delegate(GameObject p)
		{
			p.SetActive(IsPad);
		});
	}
}
