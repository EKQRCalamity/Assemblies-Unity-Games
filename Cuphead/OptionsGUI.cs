using System;
using System.Collections.Generic;
using Rewired.UI.ControlMapper;
using UnityEngine;
using UnityEngine.UI;

public class OptionsGUI : AbstractMonoBehaviour
{
	[Serializable]
	public struct LanguageTranslation
	{
		[SerializeField]
		public Localization.Languages language;

		[SerializeField]
		public string translation;
	}

	public enum State
	{
		MainOptions,
		Visual,
		Audio,
		Controls,
		Language
	}

	private enum VisualOptions
	{
		Resolution,
		Display,
		VSync,
		Align,
		Brightness,
		ChromaticAberration,
		TitleScreen,
		Filter
	}

	private enum AudioOptions
	{
		MasterVol,
		SFXVol,
		MusicVol,
		Vintage
	}

	private enum LanguageOptions
	{
		Language
	}

	[Serializable]
	public class Button
	{
		public Text text;

		public LocalizationHelper localizationHelper;

		public string[] options;

		public int selection;

		public bool wrap = true;

		public void updateSelection(int index)
		{
			selection = index;
			if (localizationHelper == null)
			{
				text.text = options[index];
			}
			else
			{
				localizationHelper.ApplyTranslation(Localization.Find(options[index]));
			}
		}

		public void incrementSelection()
		{
			if (wrap || selection < options.Length - 1)
			{
				updateSelection((selection + 1) % options.Length);
			}
		}

		public void decrementSelection()
		{
			if (wrap || selection > 0)
			{
				updateSelection((selection != 0) ? (selection - 1) : (options.Length - 1));
			}
		}
	}

	private const float BRIGHTNESS_MAX = 1f;

	private const float VOLUME_MIN = -48f;

	private const float CHROMATIC_ABERRATION_MIN = 0.5f;

	private const float CHROMATIC_ABERRATION_MAX = 1.5f;

	private const float VOLUME_NONE = -80f;

	[SerializeField]
	private GameObject mainObject;

	[SerializeField]
	private GameObject visualObject;

	[SerializeField]
	private GameObject audioObject;

	[SerializeField]
	private GameObject languageObject;

	[SerializeField]
	private Button[] mainObjectButtons;

	[SerializeField]
	private GameObject[] PcOnlyObjects;

	[SerializeField]
	private GameObject[] playStation4HideObjects;

	[SerializeField]
	private GameObject[] dlcHideObjects;

	[SerializeField]
	private GameObject[] FilterUnlockedOnlyObjects;

	[SerializeField]
	private GameObject bigCard;

	[SerializeField]
	private GameObject bigNoise;

	[SerializeField]
	private Button[] visualObjectButtons;

	[SerializeField]
	private Button[] audioObjectButtons;

	[SerializeField]
	private Button[] languageObjectButtons;

	[SerializeField]
	private LanguageTranslation[] languageTranslations;

	[SerializeField]
	private LocalizationHelper[] elementsToTranslate;

	[SerializeField]
	private float[] audioCenterPositions;

	[SerializeField]
	private float[] visualCenterPositions;

	[SerializeField]
	private CustomLanguageLayout[] customPositionning;

	private List<Button> currentItems;

	private List<BlurGamma.Filter> unlockedFilters;

	private bool isConsole;

	private bool showAlignOption;

	private bool showTitleScreenOption;

	private string[] slider = new string[11]
	{
		"|----------", "-|---------", "--|--------", "---|-------", "----|------", "-----|-----", "------|----", "-------|---", "--------|--", "---------|-",
		"----------|"
	};

	private CanvasGroup canvasGroup;

	private AbstractPauseGUI pauseMenu;

	private float _selectionTimer;

	private const float _SELECTION_TIME = 0.15f;

	private int _verticalSelection;

	private CupheadInput.AnyPlayerInput input;

	private int lastIndex;

	private List<Resolution> resolutions;

	private float initialAudioCenter;

	private float initialVisualCenter;

	private float initialAudioBackCenter;

	private float initialVisualBackCenter;

	private bool savePlayerData;

	public State state { get; private set; }

	public static Color COLOR_SELECTED { get; private set; }

	public static Color COLOR_INACTIVE { get; private set; }

	public bool optionMenuOpen { get; private set; }

	public bool inputEnabled { get; private set; }

	private int verticalSelection
	{
		get
		{
			return _verticalSelection;
		}
		set
		{
			bool flag = value > _verticalSelection;
			int num = (int)Mathf.Repeat(value, currentItems.Count);
			while (!currentItems[num].text.gameObject.activeSelf)
			{
				num = ((!flag) ? (num - 1) : (num + 1));
				num = (int)Mathf.Repeat(num, currentItems.Count);
			}
			_verticalSelection = num;
			UpdateVerticalSelection();
		}
	}

	public bool justClosed { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		isConsole = PlatformHelper.IsConsole;
		showAlignOption = true;
		showTitleScreenOption = DLCManager.DLCEnabled();
		optionMenuOpen = false;
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		currentItems = new List<Button>(mainObjectButtons);
		resolutions = new List<Resolution>();
		Resolution[] array = Screen.resolutions;
		for (int i = 0; i < array.Length; i++)
		{
			Resolution resolution = array[i];
			Resolution item = default(Resolution);
			item.width = resolution.width;
			item.height = resolution.height;
			item.refreshRate = 60;
			if (!resolutions.Contains(item))
			{
				resolutions.Add(item);
			}
		}
		SetupButtons();
		COLOR_SELECTED = currentItems[0].text.color;
		COLOR_INACTIVE = currentItems[currentItems.Count - 1].text.color;
		initialAudioCenter = audioObject.transform.localPosition.x;
		initialVisualCenter = visualObject.transform.localPosition.x;
		initialAudioBackCenter = audioObjectButtons[4].text.transform.localPosition.x;
		initialVisualBackCenter = visualObjectButtons[7].text.transform.localPosition.x;
	}

	private void Start()
	{
		Localization.OnLanguageChangedEvent += UpdateLanguages;
	}

	private void OnDestroy()
	{
		Localization.OnLanguageChangedEvent -= UpdateLanguages;
	}

	private void UpdateLanguages()
	{
		for (int i = 0; i < audioObjectButtons.Length; i++)
		{
			if (audioObjectButtons[i].localizationHelper != null)
			{
				audioObjectButtons[i].localizationHelper.ApplyTranslation();
			}
		}
		for (int j = 0; j < visualObjectButtons.Length; j++)
		{
			if (visualObjectButtons[j].localizationHelper != null)
			{
				visualObjectButtons[j].localizationHelper.ApplyTranslation();
			}
		}
	}

	public void SetupButtons()
	{
		string[] array = new string[resolutions.Count];
		int index = 0;
		for (int i = 0; i < resolutions.Count; i++)
		{
			array[i] = resolutions[i].width + "x" + resolutions[i].height;
			if (Screen.width == resolutions[i].width && Screen.height == resolutions[i].height)
			{
				index = i;
			}
		}
		if (isConsole)
		{
			GameObject[] pcOnlyObjects = PcOnlyObjects;
			foreach (GameObject gameObject in pcOnlyObjects)
			{
				gameObject.SetActive(value: false);
			}
		}
		if (!DLCManager.DLCEnabled())
		{
			GameObject[] array2 = dlcHideObjects;
			foreach (GameObject gameObject2 in array2)
			{
				gameObject2.SetActive(value: false);
			}
		}
		bool active = PlayerData.inGame && (PlayerData.Data.unlockedBlackAndWhite || PlayerData.Data.unlocked2Strip || PlayerData.Data.unlockedChaliceRecolor);
		GameObject[] filterUnlockedOnlyObjects = FilterUnlockedOnlyObjects;
		foreach (GameObject gameObject3 in filterUnlockedOnlyObjects)
		{
			gameObject3.SetActive(active);
		}
		if (!isConsole)
		{
			visualObjectButtons[0].options = array;
			visualObjectButtons[1].options = new string[2] { "OptionMenuDisplayWindowed", "OptionMenuDisplayFullscreen" };
			visualObjectButtons[2].options = new string[2] { "OptionMenuOn", "OptionMenuOff" };
		}
		if (showAlignOption)
		{
			visualObjectButtons[3].options = slider;
			visualObjectButtons[3].wrap = false;
		}
		visualObjectButtons[4].options = slider;
		visualObjectButtons[4].wrap = false;
		visualObjectButtons[5].options = slider;
		visualObjectButtons[5].wrap = false;
		if (showTitleScreenOption)
		{
			visualObjectButtons[6].options = new string[2] { "TitleScreenOptionsMenuOriginal", "TitleScreenOptionsMenuDLC" };
			visualObjectButtons[6].wrap = true;
		}
		List<string> list = new List<string>();
		unlockedFilters = new List<BlurGamma.Filter>();
		unlockedFilters.Add(BlurGamma.Filter.None);
		list.Add("OptionMenuFilterNone");
		if (PlayerData.Data.unlocked2Strip)
		{
			list.Add("OptionMenuFilter2Strip");
			unlockedFilters.Add(BlurGamma.Filter.TwoStrip);
		}
		if (PlayerData.Data.unlockedBlackAndWhite)
		{
			list.Add("OptionMenuFilterBlackWhite");
			unlockedFilters.Add(BlurGamma.Filter.BW);
		}
		if (PlayerData.Data.unlockedChaliceRecolor)
		{
			list.Add("ChaliceCostumeOptionsMenu");
			unlockedFilters.Add(BlurGamma.Filter.Chalice);
		}
		visualObjectButtons[7].options = list.ToArray();
		if (!isConsole)
		{
			visualObjectButtons[0].updateSelection(index);
			visualObjectButtons[1].updateSelection(Screen.fullScreen ? 1 : 0);
			visualObjectButtons[2].updateSelection((QualitySettings.vSyncCount <= 0) ? 1 : 0);
		}
		if (showAlignOption)
		{
			visualObjectButtons[3].updateSelection(floatToSliderIndex(SettingsData.Data.overscan, 0f, 1f));
		}
		visualObjectButtons[4].updateSelection(floatToSliderIndex(SettingsData.Data.Brightness, -1f, 1f));
		visualObjectButtons[5].updateSelection(floatToSliderIndex(SettingsData.Data.chromaticAberration, 0.5f, 1.5f));
		if (showTitleScreenOption)
		{
			visualObjectButtons[6].updateSelection((!SettingsData.Data.forceOriginalTitleScreen) ? 1 : 0);
		}
		visualObjectButtons[7].updateSelection(Mathf.Min((int)SettingsData.Data.filter, list.Count - 1));
		audioObjectButtons[0].options = slider;
		audioObjectButtons[0].wrap = false;
		audioObjectButtons[1].options = slider;
		audioObjectButtons[1].wrap = false;
		audioObjectButtons[2].options = slider;
		audioObjectButtons[2].wrap = false;
		audioObjectButtons[3].options = new string[2] { "OptionMenuOff", "OptionMenuOn" };
		audioObjectButtons[0].updateSelection(floatToSliderIndex(SettingsData.Data.masterVolume, -48f, 0f));
		audioObjectButtons[1].updateSelection(floatToSliderIndex(SettingsData.Data.sFXVolume, -48f, 0f));
		audioObjectButtons[2].updateSelection(floatToSliderIndex(SettingsData.Data.musicVolume, -48f, 0f));
		audioObjectButtons[3].updateSelection(SettingsData.Data.vintageAudioEnabled ? 1 : 0);
		int index2 = 0;
		for (int m = 0; m < languageTranslations.Length; m++)
		{
			if (languageTranslations[m].language == Localization.language)
			{
				index2 = m;
				break;
			}
		}
		string[] array3 = new string[languageTranslations.Length];
		for (int n = 0; n < languageTranslations.Length; n++)
		{
			array3[n] = "Language" + languageTranslations[n].translation;
		}
		languageObjectButtons[0].options = array3;
		languageObjectButtons[0].updateSelection(index2);
	}

	public void ChangeStateCustomLayoutScripts()
	{
		string text = visualObjectButtons[1].text.text;
		string value = Localization.Find(visualObjectButtons[1].options[0]).translation.SanitizedText();
		bool flag = text.Equals(value);
		for (int i = 0; i < customPositionning.Length; i++)
		{
			customPositionning[i].enabled = flag;
		}
	}

	public void Init(bool checkIfDead)
	{
		input = new CupheadInput.AnyPlayerInput(checkIfDead);
	}

	private void Update()
	{
		justClosed = false;
		if (!inputEnabled)
		{
			return;
		}
		if (state == State.Controls)
		{
			if (!Cuphead.Current.controlMapper.isOpen)
			{
				state = State.MainOptions;
				canvasGroup.alpha = 1f;
				ToggleSubMenu(State.MainOptions);
				PlayerManager.ControlsChanged();
			}
		}
		else if (GetButtonDown(CupheadButton.Pause) || GetButtonDown(CupheadButton.Cancel))
		{
			if (state == State.MainOptions)
			{
				MenuSelectSound();
				HideMainOptionMenu();
			}
			else
			{
				MenuSelectSound();
				ToMainOptions();
			}
		}
		else if (GetButtonDown(CupheadButton.Accept))
		{
			switch (state)
			{
			case State.MainOptions:
				OptionSelect();
				break;
			case State.Visual:
				VisualSelect();
				break;
			case State.Audio:
				AudioSelect();
				break;
			case State.Language:
				LanguageSelect();
				break;
			case State.Controls:
				break;
			}
		}
		else if (_selectionTimer >= 0.15f)
		{
			if (GetButton(CupheadButton.MenuUp))
			{
				MenuMoveSound();
				verticalSelection--;
			}
			if (GetButton(CupheadButton.MenuDown))
			{
				MenuMoveSound();
				verticalSelection++;
			}
			if (GetButton(CupheadButton.MenuRight) && currentItems[verticalSelection].options.Length > 0)
			{
				currentItems[verticalSelection].incrementSelection();
				UpdateHorizontalSelection();
			}
			if (GetButton(CupheadButton.MenuLeft) && currentItems[verticalSelection].options.Length > 0)
			{
				currentItems[verticalSelection].decrementSelection();
				UpdateHorizontalSelection();
			}
		}
		else
		{
			_selectionTimer += Time.deltaTime;
		}
	}

	private void UpdateVerticalSelection()
	{
		_selectionTimer = 0f;
		if (state == State.Controls)
		{
			return;
		}
		if (state == State.Visual && isConsole && showAlignOption && _verticalSelection < 3)
		{
			_verticalSelection = 3;
		}
		if (state == State.Visual && isConsole && !showAlignOption && _verticalSelection < 4)
		{
			_verticalSelection = 4;
		}
		for (int i = 0; i < currentItems.Count; i++)
		{
			Button button = currentItems[i];
			if (i == verticalSelection)
			{
				button.text.color = COLOR_SELECTED;
			}
			else
			{
				button.text.color = COLOR_INACTIVE;
			}
		}
	}

	private void UpdateHorizontalSelection()
	{
		_selectionTimer = 0f;
		for (int i = 0; i < currentItems.Count; i++)
		{
			Button button = currentItems[i];
			if (i == verticalSelection && currentItems[i].options.Length > 0)
			{
				switch (state)
				{
				case State.Audio:
					AudioHorizontalSelect(currentItems[i]);
					break;
				case State.Visual:
					VisualHorizontalSelect(currentItems[i]);
					break;
				case State.Language:
					LanguageHorizontalSelect(currentItems[i]);
					break;
				}
			}
		}
	}

	public void ShowMainOptionMenu()
	{
		state = State.MainOptions;
		ToggleSubMenu(state);
		optionMenuOpen = true;
		verticalSelection = 0;
		canvasGroup.alpha = 1f;
		FrameDelayedCallback(Interactable, 1);
		UpdateVerticalSelection();
	}

	public void HideMainOptionMenu()
	{
		SettingsData.Save();
		if (PlatformHelper.IsConsole)
		{
			SettingsData.SaveToCloud();
		}
		if (savePlayerData)
		{
			PlayerData.SaveCurrentFile();
		}
		savePlayerData = false;
		verticalSelection = 0;
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		inputEnabled = false;
		optionMenuOpen = false;
		justClosed = true;
	}

	private void Interactable()
	{
		verticalSelection = 0;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
		inputEnabled = true;
	}

	private void OptionSelect()
	{
		MenuSelectSound();
		switch (verticalSelection)
		{
		case 0:
			ToAudio();
			break;
		case 1:
			ToVisual();
			break;
		case 2:
			ToControls();
			break;
		case 3:
			ToLanguage();
			break;
		case 4:
			ToPauseMenu();
			break;
		}
	}

	protected void MenuSelectSound()
	{
		AudioManager.Play("level_menu_select");
	}

	protected void MenuMoveSound()
	{
		AudioManager.Play("level_menu_move");
	}

	private void ToVisual()
	{
		state = State.Visual;
		CenterVisual();
		if (!isConsole)
		{
			ChangeStateCustomLayoutScripts();
		}
		ToggleSubMenu(state);
	}

	private void ToAudio()
	{
		state = State.Audio;
		CenterAudio();
		ToggleSubMenu(state);
	}

	private void ToLanguage()
	{
		state = State.Language;
		ToggleSubMenu(state);
	}

	private void ToControls()
	{
		state = State.Controls;
		ToggleSubMenu(state);
	}

	private void ToPauseMenu()
	{
		optionMenuOpen = false;
		HideMainOptionMenu();
	}

	private void ToggleSubMenu(State state)
	{
		currentItems.Clear();
		switch (state)
		{
		case State.MainOptions:
			mainObject.SetActive(value: true);
			visualObject.SetActive(value: false);
			audioObject.SetActive(value: false);
			languageObject.SetActive(value: false);
			bigCard.SetActive(value: false);
			bigNoise.SetActive(value: false);
			currentItems.AddRange(mainObjectButtons);
			break;
		case State.Visual:
			mainObject.SetActive(value: false);
			visualObject.SetActive(value: true);
			audioObject.SetActive(value: false);
			bigCard.SetActive(value: true);
			bigNoise.SetActive(value: true);
			currentItems.AddRange(visualObjectButtons);
			break;
		case State.Audio:
			mainObject.SetActive(value: false);
			visualObject.SetActive(value: false);
			audioObject.SetActive(value: true);
			languageObject.SetActive(value: false);
			bigCard.SetActive(value: true);
			bigNoise.SetActive(value: true);
			currentItems.AddRange(audioObjectButtons);
			break;
		case State.Language:
			languageObjectButtons[0].updateSelection((int)Localization.language);
			mainObject.SetActive(value: false);
			audioObject.SetActive(value: false);
			languageObject.SetActive(value: true);
			bigCard.SetActive(value: false);
			bigNoise.SetActive(value: false);
			currentItems.AddRange(languageObjectButtons);
			break;
		case State.Controls:
			mainObject.SetActive(value: false);
			visualObject.SetActive(value: false);
			audioObject.SetActive(value: false);
			languageObject.SetActive(value: false);
			ShowControlMapper();
			break;
		}
		if (state != State.Controls)
		{
			verticalSelection = 0;
			UpdateVerticalSelection();
		}
	}

	private void ShowControlMapper()
	{
		ControlMapper controlMapper = Cuphead.Current.controlMapper;
		Canvas componentInChildren = controlMapper.GetComponentInChildren<Canvas>(includeInactive: true);
		CupheadUICamera cupheadUICamera = UnityEngine.Object.FindObjectOfType<CupheadUICamera>();
		if (cupheadUICamera != null && componentInChildren != null)
		{
			componentInChildren.worldCamera = cupheadUICamera.GetComponent<Camera>();
		}
		controlMapper.showPlayers = true;
		if (PlatformHelper.IsConsole)
		{
			controlMapper.showKeyboard = false;
			controlMapper.showControllerGroupButtons = false;
		}
		controlMapper.showControllerGroupButtons = !PlatformHelper.IsConsole;
		controlMapper.Reset();
		controlMapper.Open();
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
	}

	private void ToMainOptions()
	{
		state = State.MainOptions;
		ToggleSubMenu(state);
	}

	private void VisualHorizontalSelect(Button button)
	{
		switch (verticalSelection)
		{
		case 0:
			MenuSelectSound();
			if (button.selection < resolutions.Count)
			{
				SettingsData.Data.screenWidth = resolutions[button.selection].width;
				SettingsData.Data.screenHeight = resolutions[button.selection].height;
				Screen.SetResolution(SettingsData.Data.screenWidth, SettingsData.Data.screenHeight, Screen.fullScreen, 60);
			}
			break;
		case 1:
			MenuSelectSound();
			SettingsData.Data.fullScreen = button.selection == 1;
			if (!isConsole)
			{
				ChangeStateCustomLayoutScripts();
			}
			Screen.fullScreen = SettingsData.Data.fullScreen;
			break;
		case 2:
			MenuSelectSound();
			SettingsData.Data.vSyncCount = ((button.selection == 0) ? 1 : 0);
			QualitySettings.vSyncCount = SettingsData.Data.vSyncCount;
			break;
		case 3:
			SettingsData.Data.overscan = sliderIndexToFloat(button.selection, 0f, 1f);
			break;
		case 4:
			SettingsData.Data.Brightness = sliderIndexToFloat(button.selection, -1f, 1f);
			break;
		case 5:
			SettingsData.Data.chromaticAberration = sliderIndexToFloat(button.selection, 0.5f, 1.5f);
			break;
		case 6:
			SettingsData.Data.forceOriginalTitleScreen = !SettingsData.Data.forceOriginalTitleScreen;
			break;
		case 7:
			MenuSelectSound();
			PlayerData.Data.filter = unlockedFilters[button.selection];
			EventManager.Instance.Raise(new ChaliceRecolorEvent(unlockedFilters[button.selection] == BlurGamma.Filter.Chalice));
			savePlayerData = true;
			break;
		}
	}

	private void VisualSelect()
	{
		AudioManager.Play("level_menu_select");
		int num = verticalSelection;
		if (num == 8)
		{
			ToMainOptions();
		}
	}

	private void LanguageSelect()
	{
		AudioManager.Play("level_menu_select");
		int num = verticalSelection;
		if (num == 1)
		{
			ToMainOptions();
		}
	}

	private void LanguageHorizontalSelect(Button button)
	{
		if (verticalSelection == 0)
		{
			Localization.language = languageTranslations[button.selection].language;
			button.updateSelection(button.selection);
			for (int i = 0; i < elementsToTranslate.Length; i++)
			{
				elementsToTranslate[i].ApplyTranslation();
			}
		}
	}

	private void AudioHorizontalSelect(Button button)
	{
		switch (verticalSelection)
		{
		case 0:
			AudioManager.masterVolume = ((button.selection > 0) ? sliderIndexToFloat(button.selection, -48f, 0f) : (-80f));
			SettingsData.Data.masterVolume = AudioManager.masterVolume;
			break;
		case 1:
			AudioManager.sfxOptionsVolume = ((button.selection > 0) ? sliderIndexToFloat(button.selection, -48f, 0f) : (-80f));
			SettingsData.Data.sFXVolume = AudioManager.sfxOptionsVolume;
			break;
		case 2:
			AudioManager.bgmOptionsVolume = ((button.selection > 0) ? sliderIndexToFloat(button.selection, -48f, 0f) : (-80f));
			SettingsData.Data.musicVolume = AudioManager.bgmOptionsVolume;
			break;
		case 3:
			MenuSelectSound();
			if (button.selection == 0)
			{
				PlayerData.Data.vintageAudioEnabled = false;
			}
			else if (button.options[button.selection] == button.options[1])
			{
				PlayerData.Data.vintageAudioEnabled = true;
			}
			savePlayerData = true;
			break;
		}
	}

	private void MasterVolume(string option)
	{
	}

	private void AudioSelect()
	{
		AudioManager.Play("level_menu_select");
		int num = verticalSelection;
		if (num == 4)
		{
			ToMainOptions();
		}
	}

	protected bool GetButtonDown(CupheadButton button)
	{
		if (input.GetButtonDown(button))
		{
			return true;
		}
		return false;
	}

	protected bool GetButton(CupheadButton button)
	{
		if (input.GetButton(button))
		{
			return true;
		}
		return false;
	}

	private float sliderIndexToFloat(int index, float min, float max)
	{
		if (index != lastIndex)
		{
			MenuSelectSound();
		}
		lastIndex = index;
		return (float)index / (float)(slider.Length - 1) * (max - min) + min;
	}

	private int floatToSliderIndex(float value, float min, float max)
	{
		int num = Mathf.RoundToInt((value - min) / (max - min) * (float)(slider.Length - 1));
		if (num > slider.Length - 1)
		{
			num = slider.Length - 1;
		}
		if (num < 0)
		{
			num = 0;
		}
		return num;
	}

	private void CenterAudio()
	{
		float num = audioCenterPositions[(int)Localization.language];
		audioObject.transform.SetLocalPosition(initialAudioCenter + num);
		audioObjectButtons[4].text.transform.SetLocalPosition(initialAudioBackCenter - num);
	}

	private void CenterVisual()
	{
		float num = visualCenterPositions[(int)Localization.language];
		visualObject.transform.SetLocalPosition(initialVisualCenter + num);
		visualObjectButtons[7].text.transform.SetLocalPosition(initialVisualBackCenter - num);
	}
}
