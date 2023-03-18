using System;
using Framework.Managers;
using I2.Loc;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class InputScheme : MonoBehaviour
{
	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private Image keyboardSpanish;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private Image keyboardEnglish;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private Image gamepadSpanish;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private Image gamepadEnglish;

	private void Start()
	{
		Core.Input.KeyboardPressed += Refresh;
		Core.Input.JoystickPressed += Refresh;
		I2.Loc.LocalizationManager.OnLocalizeEvent += Refresh;
	}

	private void Refresh()
	{
		string currentLanguage = I2.Loc.LocalizationManager.CurrentLanguage;
		if (currentLanguage.Equals("Spanish") && Core.Input.ActiveControllerType == ControllerType.Keyboard)
		{
			SetActiveScheme(keyboardSpanish);
		}
		else if (currentLanguage.Equals("English") && Core.Input.ActiveControllerType == ControllerType.Keyboard)
		{
			SetActiveScheme(keyboardEnglish);
		}
		else if (currentLanguage.Equals("Spanish") && Core.Input.ActiveControllerType == ControllerType.Joystick)
		{
			SetActiveScheme(gamepadSpanish);
		}
		else if (currentLanguage.Equals("English") && Core.Input.ActiveControllerType == ControllerType.Joystick)
		{
			SetActiveScheme(gamepadEnglish);
		}
	}

	private void SetActiveScheme(Image image)
	{
		try
		{
			keyboardSpanish.enabled = false;
			keyboardEnglish.enabled = false;
			gamepadSpanish.enabled = false;
			gamepadEnglish.enabled = false;
			image.enabled = true;
		}
		catch (Exception ex)
		{
			Debug.LogError("Main menu input scheme has no image." + ex.ToString());
		}
	}

	private void OnDestroy()
	{
		Core.Input.KeyboardPressed -= Refresh;
		Core.Input.JoystickPressed -= Refresh;
		I2.Loc.LocalizationManager.OnLocalizeEvent -= Refresh;
	}
}
