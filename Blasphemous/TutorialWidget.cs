using System;
using System.Collections.Generic;
using Framework.Managers;
using I2.Loc;
using Rewired;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialWidget : SerializedMonoBehaviour
{
	[Serializable]
	private class LocalizationText
	{
		public TextMeshProUGUI mesh;

		public LocalizedString text;
	}

	[BoxGroup("Common", true, false, 0)]
	[SerializeField]
	private CanvasGroup rootCanvas;

	[BoxGroup("Menu", true, false, 0)]
	[SerializeField]
	private GameObject buttonBackMenu;

	[BoxGroup("Menu", true, false, 0)]
	[SerializeField]
	private GameObject buttonNavLeft;

	[BoxGroup("Menu", true, false, 0)]
	[SerializeField]
	private GameObject buttonNavRight;

	[BoxGroup("Menu", true, false, 0)]
	[SerializeField]
	private Text counter;

	[BoxGroup("Menu", true, false, 0)]
	[SerializeField]
	private float alphaMenu = 1f;

	[BoxGroup("InGame", true, false, 0)]
	[SerializeField]
	private GameObject buttonBackInGame;

	[BoxGroup("InGame", true, false, 0)]
	[SerializeField]
	private float alphaInGame = 0.8f;

	[BoxGroup("Text", true, false, 0)]
	[SerializeField]
	private List<LocalizationText> texts = new List<LocalizationText>();

	[BoxGroup("Debug", true, false, 0)]
	[ValueDropdown("MyLanguages")]
	public string Language;

	[BoxGroup("Debug", true, false, 0)]
	[SerializeField]
	private ControllerType debugController;

	[BoxGroup("Debug", true, false, 0)]
	[SerializeField]
	private JoystickType debugJoystick;

	private bool InMenu;

	private bool IsSwowing;

	private Player rewired;

	private JoystickType currentJoystick;

	private ControllerType currentController;

	public bool WantToExit { get; private set; }

	private IList<ValueDropdownItem<string>> MyLanguages()
	{
		ValueDropdownList<string> valueDropdownList = new ValueDropdownList<string>();
		string[] array = I2.Loc.LocalizationManager.GetAllLanguages().ToArray();
		Array.Sort(array);
		string[] array2 = array;
		foreach (string text in array2)
		{
			valueDropdownList.Add(text, text);
		}
		return valueDropdownList;
	}

	[BoxGroup("Debug", true, false, 0)]
	[Button("Check menu", ButtonSizes.Small)]
	private void CheckMenu()
	{
		I2.Loc.LocalizationManager.CurrentLanguage = Language;
		currentController = debugController;
		currentJoystick = debugJoystick;
		ShowInMenu(3, 10);
	}

	[BoxGroup("Debug", true, false, 0)]
	[Button("Check InGame", ButtonSizes.Small)]
	private void CheckIngame()
	{
		I2.Loc.LocalizationManager.CurrentLanguage = Language;
		currentController = debugController;
		currentJoystick = debugJoystick;
		ShowInGame();
	}

	public void ShowInMenu(int current, int total)
	{
		Show(inMenu: true, current, total, catchInput: false);
	}

	public void ShowInGame()
	{
		Show(inMenu: false, 0, 0);
	}

	private void Update()
	{
		if (!IsSwowing)
		{
			return;
		}
		if (rewired == null)
		{
			rewired = ReInput.players.GetPlayer(0);
		}
		if (rewired.GetButtonDown(51))
		{
			Hide();
			return;
		}
		JoystickType activeJoystickModel = Core.Input.ActiveJoystickModel;
		ControllerType activeControllerType = Core.Input.ActiveControllerType;
		if (activeJoystickModel != currentJoystick || activeControllerType != currentController)
		{
			UpdateInput();
			UpdateTextPro();
		}
	}

	public void Hide()
	{
		IsSwowing = false;
		WantToExit = true;
	}

	private void Show(bool inMenu, int current, int total, bool catchInput = true)
	{
		WantToExit = false;
		IsSwowing = true;
		rootCanvas.alpha = ((!inMenu) ? alphaInGame : alphaMenu);
		SetInMenu(inMenu);
		if (inMenu)
		{
			counter.text = current + " / " + total;
		}
		if (catchInput)
		{
			UpdateInput();
		}
		UpdateTextPro();
	}

	private void SetInMenu(bool inmenu)
	{
		InMenu = inmenu;
		buttonBackMenu.SetActive(inmenu);
		buttonNavLeft.SetActive(inmenu);
		buttonNavRight.SetActive(inmenu);
		counter.gameObject.SetActive(inmenu);
		buttonBackInGame.SetActive(!inmenu);
	}

	private void UpdateTextPro()
	{
		foreach (LocalizationText text in texts)
		{
			string localizedText = text.text;
			text.mesh.text = Framework.Managers.LocalizationManager.ParseMeshPro(localizedText, text.text.mTerm, text.mesh);
		}
	}

	private void UpdateInput()
	{
		currentJoystick = Core.Input.ActiveJoystickModel;
		currentController = Core.Input.ActiveControllerType;
	}
}
