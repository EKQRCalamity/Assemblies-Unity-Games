using System;
using System.Collections.Generic;
using Framework.Managers;
using I2.Loc;
using Rewired;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockWidget : SerializedMonoBehaviour
{
	[Serializable]
	private class LocalizationText
	{
		public TextMeshProUGUI mesh;

		public LocalizedString text;
	}

	[BoxGroup("References", true, false, 0)]
	[SerializeField]
	private CanvasGroup rootCanvas;

	[BoxGroup("References", true, false, 0)]
	[SerializeField]
	private GameObject buttonBackInGame;

	[BoxGroup("References", true, false, 0)]
	[SerializeField]
	private float alphaInGame = 0.8f;

	[BoxGroup("Skin info", true, false, 0)]
	[SerializeField]
	private Image skinPreviewImage;

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

	private bool IsShowing;

	private Player rewired;

	private JoystickType currentJoystick;

	private ControllerType currentController;

	private string unlockId;

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
	[Button("Check InGame", ButtonSizes.Small)]
	private void CheckIngame()
	{
		I2.Loc.LocalizationManager.CurrentLanguage = Language;
		currentController = debugController;
		currentJoystick = debugJoystick;
		ShowInGame();
	}

	public void ShowInGame()
	{
		Show(0, 0);
	}

	private void Update()
	{
		if (!IsShowing)
		{
			return;
		}
		if (rewired == null)
		{
			rewired = ReInput.players.GetPlayer(0);
		}
		if (rewired.GetButtonDown(51))
		{
			IsShowing = false;
			WantToExit = true;
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

	private void Show(int current, int total, bool catchInput = true)
	{
		WantToExit = false;
		IsShowing = true;
		rootCanvas.alpha = alphaInGame;
		if (catchInput)
		{
			UpdateInput();
		}
		UpdateTextPro();
	}

	private void UpdateTextPro()
	{
		string text = "UI_Extras/UNLOCK_" + unlockId;
		foreach (LocalizationText text2 in texts)
		{
			if (text2.text.mTerm.Equals(text))
			{
				string localizedText = text2.text;
				text2.mesh.text = Framework.Managers.LocalizationManager.ParseMeshPro(localizedText, text);
			}
		}
	}

	private void UpdateInput()
	{
		currentJoystick = Core.Input.ActiveJoystickModel;
		currentController = Core.Input.ActiveControllerType;
	}

	public void Configurate(string unlockId)
	{
		Sprite paletteSpritePreview = Core.ColorPaletteManager.GetPaletteSpritePreview(unlockId);
		skinPreviewImage.sprite = paletteSpritePreview;
		this.unlockId = unlockId;
	}
}
