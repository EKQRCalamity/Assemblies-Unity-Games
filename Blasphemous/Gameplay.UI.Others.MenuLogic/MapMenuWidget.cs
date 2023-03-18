using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using FullSerializer;
using Gameplay.UI.Others.Buttons;
using Gameplay.UI.Widgets;
using I2.Loc;
using Rewired;
using Sirenix.OdinInspector;
using Tools;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class MapMenuWidget : SerializedMonoBehaviour
{
	private enum STATE
	{
		STATE_OFF,
		STATE_MAP,
		STATE_LEYEND,
		STATE_OPTIONS
	}

	private enum MENU
	{
		OPTIONS,
		GAME,
		ACCESSIBILITY,
		VIDEO,
		AUDIO,
		TUTORIAL,
		CONTROLS
	}

	public enum GAME_OPTIONS
	{
		AUDIOLANGUAGE,
		TEXTLANGUAGE,
		ENABLEHOWTOPLAY,
		CONTROLSREMAP
	}

	public enum ACCESSIBILITY_OPTIONS
	{
		RUMBLEENABLED,
		SHAKEENABLED
	}

	public enum VIDEO_OPTIONS
	{
		VSYNC,
		WINDOWMODE,
		BRIGHTNES,
		RESOLUTION,
		FILTERING,
		RESOLUTIONMODE
	}

	public enum SCALING_STRATEGY
	{
		PIXEL_PERFECT,
		SCALE
	}

	public enum AUDIO_OPTIONS
	{
		MASTERVOLUME,
		EFFECTSVOLUME,
		MUSICVOLUME,
		VOICEOVERVOLUME
	}

	[Serializable]
	private class RangePixels
	{
		public int cellMin;

		public int cellMax;

		public int value;
	}

	[SerializeField]
	[BoxGroup("Options", true, false, 0)]
	private Dictionary<MENU, Transform> optionsRoot = new Dictionary<MENU, Transform>();

	[SerializeField]
	[BoxGroup("Options", true, false, 0)]
	private Color optionNormalColor = new Color(0.972549f, 76f / 85f, 0.78039217f);

	[SerializeField]
	[BoxGroup("Options", true, false, 0)]
	private Color optionHighligterColor = new Color(0.80784315f, 72f / 85f, 0.49803922f);

	[SerializeField]
	[BoxGroup("Options", true, false, 0)]
	private Transform languageRoot;

	[SerializeField]
	[BoxGroup("Options", true, false, 0)]
	private Text acceptOrApplyButton;

	[SerializeField]
	[BoxGroup("Options Game", true, false, 0)]
	private Dictionary<GAME_OPTIONS, SelectableOption> gameElements;

	[SerializeField]
	[BoxGroup("Options Accessibility", true, false, 0)]
	private Dictionary<ACCESSIBILITY_OPTIONS, GameObject> accessibilityElements;

	[SerializeField]
	[BoxGroup("Options Video", true, false, 0)]
	private Dictionary<VIDEO_OPTIONS, GameObject> videoElements;

	[SerializeField]
	[BoxGroup("Options Video", true, false, 0)]
	private EventsButton vsyncButton;

	[SerializeField]
	[BoxGroup("Options Video", true, false, 0)]
	private EventsButton resolutionButton;

	[SerializeField]
	[BoxGroup("Options Video", true, false, 0)]
	private EventsButton resolutionModeButton;

	[SerializeField]
	[BoxGroup("Options Video", true, false, 0)]
	private GameObject resolutionModeSelectionElement;

	[SerializeField]
	[BoxGroup("Options Video", true, false, 0)]
	private int minBrightness = 20;

	[SerializeField]
	[BoxGroup("Options Video", true, false, 0)]
	private int maxBrightness = 100;

	[SerializeField]
	[BoxGroup("Options Video", true, false, 0)]
	private int stepBrightness = 5;

	[SerializeField]
	[BoxGroup("Options Audio", true, false, 0)]
	private Dictionary<AUDIO_OPTIONS, GameObject> audioElements;

	[SerializeField]
	[BoxGroup("Options Audio", true, false, 0)]
	private int minVolume;

	[SerializeField]
	[BoxGroup("Options Audio", true, false, 0)]
	private int maxVolume = 100;

	[SerializeField]
	[BoxGroup("Options Audio", true, false, 0)]
	private int stepVolume = 10;

	[BoxGroup("Header", true, false, 0)]
	[SerializeField]
	private Text domainCaption;

	[BoxGroup("Header", true, false, 0)]
	[SerializeField]
	private Text zoneCaption;

	[BoxGroup("Navigation Buttons", true, false, 0)]
	[SerializeField]
	private GameObject navigationButtonsRoot;

	[BoxGroup("Map", true, false, 0)]
	[SerializeField]
	private Transform mapBigLayout;

	[BoxGroup("Map", true, false, 0)]
	[SerializeField]
	private float scrollSpeedX = 1000f;

	[BoxGroup("Map", true, false, 0)]
	[SerializeField]
	private float scrollSpeedY = 400f;

	[BoxGroup("Map", true, false, 0)]
	[SerializeField]
	private Vector2 scrollLimitsMin;

	[BoxGroup("Map", true, false, 0)]
	[SerializeField]
	private Vector2 scrollLimitsMax;

	[BoxGroup("Map", true, false, 0)]
	[SerializeField]
	private Color mapElementColor = new Color(1f, 1f, 1f, 0.4f);

	[BoxGroup("Map", true, false, 0)]
	[SerializeField]
	private Color mapPlayerColor = new Color(1f, 1f, 1f, 0.6f);

	[BoxGroup("Map", true, false, 0)]
	[SerializeField]
	private Color mapNotSelected = new Color(0.7f, 0.7f, 0.7f, 1f);

	[BoxGroup("Map", true, false, 0)]
	[SerializeField]
	private Color mapSelected = new Color(1f, 1f, 1f, 1f);

	[BoxGroup("Map Sprites", true, false, 0)]
	[SerializeField]
	private Sprite playerSprite;

	[BoxGroup("Map Sprites", true, false, 0)]
	[SerializeField]
	private Sprite mapGateVertical;

	[BoxGroup("Map Sprites", true, false, 0)]
	[SerializeField]
	private Sprite mapDoorOpen;

	[BoxGroup("Map Sprites", true, false, 0)]
	[SerializeField]
	private Sprite mapDoorClose;

	[BoxGroup("Map Sprites", true, false, 0)]
	[SerializeField]
	private Sprite mapPrieDieuLight;

	[BoxGroup("Map Sprites", true, false, 0)]
	[SerializeField]
	private Sprite mapPrieDieuOff;

	[BoxGroup("Map Sprites", true, false, 0)]
	[SerializeField]
	private Sprite mapMeaCulpa;

	[BoxGroup("Map Sprites", true, false, 0)]
	[SerializeField]
	private Sprite mapTravel;

	[BoxGroup("Map Sprites", true, false, 0)]
	[SerializeField]
	private Sprite mapGuiltDrop;

	[BoxGroup("MapAutomatic", true, false, 0)]
	[SerializeField]
	private float mapEpsilonSqrDistance = 30f;

	[BoxGroup("MapAutomatic", true, false, 0)]
	[SerializeField]
	private AnimationCurve automaticFactor;

	[SerializeField]
	[BoxGroup("Sounds", true, false, 0)]
	private string soundBack = "event:/SFX/UI/ChangeTab";

	[BoxGroup("Options", true, false, 0)]
	[SerializeField]
	private EventsButton beforeToTutorialMenuOption;

	[BoxGroup("Options", true, false, 0)]
	[SerializeField]
	private EventsButton TutorialMenuOption;

	[BoxGroup("Options", true, false, 0)]
	[SerializeField]
	private EventsButton ExitToMainMenuOption;

	[BoxGroup("Options", true, false, 0)]
	[SerializeField]
	private EventsButton ResumeGameOption;

	[BoxGroup("Options", true, false, 0)]
	[SerializeField]
	private EventsButton afterToResumeGameOption;

	[SerializeField]
	[BoxGroup("DisableMap", true, false, 0)]
	private List<GameObject> elementsWhenEnabled;

	[SerializeField]
	[BoxGroup("DisableMap", true, false, 0)]
	private List<GameObject> elementsWhenDisabled;

	public ControlsMenuScreen controlsMenuScreen;

	private Animator animator;

	private GameObject optionLastSelected;

	private STATE _currentState;

	private MENU currentMenu;

	private Player rewired;

	private float mapPosX;

	private float mapPosY;

	private RectTransform playerSpriteInMap;

	private RectTransform mapRoot;

	private RectTransform allmapRoot;

	private RectTransform elementsRoot;

	private bool automaticScroll;

	private float prevVerticalAxis;

	private float prevHorizontalAxis;

	private float timeInAutomatic;

	private Dictionary<string, Image> mapPieces = new Dictionary<string, Image>();

	private Image lastSelectedMapPiece;

	private float lastHorizontalInOptions;

	private bool initialOptions;

	public const float VirtualFrameWidth = 640f;

	public const float VirtualFrameHeight = 360f;

	private List<Resolution> resolutions = new List<Resolution>();

	private Resolution savedResolution;

	private int currentTextLanguageIndex;

	private bool currentEnableHowToPlay;

	private bool currentEnableRumble;

	private bool currentEnableShake;

	private bool currentVsync;

	private bool currentFullScreen;

	private int currentBrightness;

	private int currentResolution = -1;

	private AnisotropicFiltering currentFilter = AnisotropicFiltering.Enable;

	private SCALING_STRATEGY currentScalingStrategy;

	private int currentMasterVolume;

	private int currentEffectsVolume;

	private int currentMusicVolume;

	private int currentVoiceoverVolume;

	private int nativeHeightRes;

	private int nativeWidthRes;

	private GAME_OPTIONS optionLastGameSelected;

	private ACCESSIBILITY_OPTIONS optionLastAccessibilitySelected;

	private VIDEO_OPTIONS optionLastVideoSelected;

	private AUDIO_OPTIONS optionLastAudioSelected;

	private bool mapEnabled = true;

	[BoxGroup("Cell fix", true, false, 0)]
	[SerializeField]
	private int CELL_WIDTH = 18;

	[BoxGroup("Cell fix", true, false, 0)]
	[SerializeField]
	private int CELL_HEIGHT = 10;

	[BoxGroup("Cell fix", true, false, 0)]
	[SerializeField]
	private float FACTOR_X = 1f;

	[BoxGroup("Cell fix", true, false, 0)]
	[SerializeField]
	private float FACTOR_Y = 1f;

	[BoxGroup("Cell fix", true, false, 0)]
	[SerializeField]
	private List<RangePixels> cellPixelFix = new List<RangePixels>();

	private int currentTutorial;

	private Dictionary<string, GameObject> tutorialInstances = new Dictionary<string, GameObject>();

	private List<string> tutorialOrder = new List<string>();

	private const string OPTIONS_SETTINGS_FILE = "/options_settings.json";

	private STATE currentState
	{
		get
		{
			return _currentState;
		}
		set
		{
			_currentState = value;
			if ((bool)animator)
			{
				animator.SetInteger("STATUS", (int)currentState);
			}
		}
	}

	public bool currentlyActive => currentState != STATE.STATE_OFF;

	public int appliedResolutionIndex { get; private set; }

	private void Awake()
	{
		currentState = STATE.STATE_OFF;
		animator = GetComponent<Animator>();
		mapPosX = 0f;
		mapPosY = 0f;
		currentMenu = MENU.OPTIONS;
		nativeHeightRes = Display.main.systemHeight;
		nativeWidthRes = Display.main.systemWidth;
		CreateMap();
		ResetMenus();
	}

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => Core.ready);
		ReadOptionsFromFile();
	}

	private void OnDestroy()
	{
		Core.Logic.ResumeGame();
	}

	private void Update()
	{
		if (rewired == null)
		{
			Player player = ReInput.players.GetPlayer(0);
			if (player == null)
			{
				return;
			}
			rewired = player;
		}
		float axisRaw = rewired.GetAxisRaw(48);
		bool flag = axisRaw < 0.3f && (double)axisRaw >= -0.3;
		if (currentState == STATE.STATE_OPTIONS)
		{
			switch (currentMenu)
			{
			case MENU.GAME:
				if (flag && lastHorizontalInOptions != 0f)
				{
					UpdateInputGameOptions(lastHorizontalInOptions < 0f);
					lastHorizontalInOptions = 0f;
				}
				else if (!flag && axisRaw < 0f)
				{
					lastHorizontalInOptions = -1f;
				}
				else if (!flag && axisRaw > 0f)
				{
					lastHorizontalInOptions = 1f;
				}
				break;
			case MENU.ACCESSIBILITY:
				if (flag && lastHorizontalInOptions != 0f)
				{
					UpdateInputAccessibilityOptions();
					lastHorizontalInOptions = 0f;
				}
				else if (!flag && axisRaw < 0f)
				{
					lastHorizontalInOptions = -1f;
				}
				else if (!flag && axisRaw > 0f)
				{
					lastHorizontalInOptions = 1f;
				}
				break;
			case MENU.VIDEO:
				if (flag && lastHorizontalInOptions != 0f)
				{
					UpdateInputVideoOptions(lastHorizontalInOptions < 0f);
					lastHorizontalInOptions = 0f;
				}
				else if (!flag && axisRaw < 0f)
				{
					lastHorizontalInOptions = -1f;
				}
				else if (!flag && axisRaw > 0f)
				{
					lastHorizontalInOptions = 1f;
				}
				break;
			case MENU.AUDIO:
				if (flag && lastHorizontalInOptions != 0f)
				{
					UpdateInputAudioOptions(lastHorizontalInOptions < 0f);
					lastHorizontalInOptions = 0f;
				}
				else if (!flag && axisRaw < 0f)
				{
					lastHorizontalInOptions = -1f;
				}
				else if (!flag && axisRaw > 0f)
				{
					lastHorizontalInOptions = 1f;
				}
				break;
			}
		}
		if (currentState != STATE.STATE_MAP || !mapEnabled)
		{
			return;
		}
		float num = rewired.GetAxisRaw(48);
		float num2 = rewired.GetAxisRaw(49);
		bool flag2 = false;
		if (rewired.GetButtonDown(52))
		{
			automaticScroll = true;
			flag2 = true;
			timeInAutomatic = 0f;
		}
		else if (automaticScroll)
		{
			automaticScroll = num == 0f && num2 == 0f;
			timeInAutomatic += Time.unscaledDeltaTime;
		}
		if (automaticScroll)
		{
			Vector2 vector = new Vector2(playerSpriteInMap.localPosition.x, playerSpriteInMap.localPosition.y);
			Vector2 vector2 = new Vector2(mapPosX, mapPosY);
			Vector2 vector3 = vector2 - vector;
			if (vector3.sqrMagnitude <= mapEpsilonSqrDistance)
			{
				automaticScroll = false;
				SetMapFocusOnPosition(vector.x, vector.y);
			}
			else
			{
				vector3.Normalize();
				vector3 *= automaticFactor.Evaluate(timeInAutomatic);
				num = 0f - vector3.x;
				num2 = 0f - vector3.y;
				if (!flag2 && ((prevVerticalAxis > 0f && num2 <= 0f) || (prevVerticalAxis < 0f && num2 >= 0f)))
				{
					SetMapFocusOnPosition(mapPosX, vector.y);
				}
				if (!flag2 && ((prevHorizontalAxis > 0f && num <= 0f) || (prevHorizontalAxis < 0f && num >= 0f)))
				{
					SetMapFocusOnPosition(vector.x, mapPosY);
				}
				prevVerticalAxis = num2;
				prevHorizontalAxis = num;
			}
		}
		if (num != 0f || num2 != 0f)
		{
			mapPosX += num * scrollSpeedX * Time.unscaledDeltaTime;
			mapPosY += num2 * scrollSpeedY * Time.unscaledDeltaTime;
			mapPosX = Mathf.Clamp(mapPosX, scrollLimitsMin.x, scrollLimitsMax.x);
			mapPosY = Mathf.Clamp(mapPosY, scrollLimitsMin.y, scrollLimitsMax.y);
			SetMapFocusOnPosition(mapPosX, mapPosY);
		}
		UpdateDomainAndZoneName();
	}

	private void UpdateDomainAndZoneName()
	{
		if (lastSelectedMapPiece != null)
		{
			lastSelectedMapPiece.color = mapNotSelected;
			lastSelectedMapPiece = null;
		}
		MapManager.DataMapReveal nearestZone = Core.MapManager.GetNearestZone(new Vector2(mapPosX, mapPosY));
		string text = string.Empty;
		string text2 = string.Empty;
		if (nearestZone != null && (nearestZone.updatedAnyTime || nearestZone.updated))
		{
			text2 = Core.MapManager.GetDomainName(nearestZone.domain);
			text = Core.MapManager.GetZoneName(nearestZone.domain, nearestZone.zone);
			string key = nearestZone.domain + "_" + nearestZone.zone;
			if (mapPieces.ContainsKey(key))
			{
				lastSelectedMapPiece = mapPieces[key];
				lastSelectedMapPiece.color = mapSelected;
			}
		}
		domainCaption.text = text2;
		zoneCaption.text = text;
	}

	public void ShowOptionsOutsideMap()
	{
		initialOptions = true;
		UpdateMenuButtons();
		Core.Input.SetBlocker("INGAME_MENU", blocking: true);
		Core.Logic.SetState(LogicStates.Unresponsive);
		currentState = STATE.STATE_OPTIONS;
		Core.Logic.PauseGame();
		ShowMenu(MENU.OPTIONS);
	}

	public void Show(bool b)
	{
		if (SceneManager.GetActiveScene().name == "MainMenu" || FadeWidget.instance.Fading || (b && Core.Input.InputBlocked && !Core.Input.HasBlocker("PLAYER_LOGIC")))
		{
			return;
		}
		initialOptions = false;
		UpdateMenuButtons();
		Core.Input.SetBlocker("INGAME_MENU", b);
		if (b)
		{
			UpdateTutorials();
			Core.Logic.SetState(LogicStates.Unresponsive);
			currentState = STATE.STATE_MAP;
			Core.Logic.PauseGame();
			CreateMap();
			MapManager.DataMapReveal currentZone = Core.MapManager.GetCurrentZone();
			mapEnabled = currentZone != null;
			playerSpriteInMap.gameObject.SetActive(mapEnabled);
			elementsWhenEnabled.ForEach(delegate(GameObject p)
			{
				p.SetActive(mapEnabled);
			});
			elementsWhenDisabled.ForEach(delegate(GameObject p)
			{
				p.SetActive(!mapEnabled);
			});
			if (mapEnabled)
			{
				if (currentZone != null)
				{
					Rect rect = playerSprite.rect;
					Vector3 pos = currentZone.WorldToTexture(Core.Logic.Penitent.transform.position + Core.MapManager.playerMapOffset);
					playerSpriteInMap.localPosition = SnapToMapCell(pos, changeX: true);
				}
				SetMapFocusOnPosition(playerSpriteInMap.localPosition.x, playerSpriteInMap.localPosition.y);
				UpdateZoneInfo();
			}
			else
			{
				domainCaption.text = string.Empty;
				zoneCaption.text = string.Empty;
			}
		}
		else
		{
			EventSystem.current.SetSelectedGameObject(null);
			ResetMenus();
			Core.Logic.ResumeGame();
			Core.Logic.SetState(LogicStates.Playing);
			currentState = STATE.STATE_OFF;
		}
	}

	public void GoBack()
	{
		switch (currentState)
		{
		case STATE.STATE_MAP:
			Show(b: false);
			break;
		case STATE.STATE_LEYEND:
			currentState = STATE.STATE_MAP;
			break;
		case STATE.STATE_OPTIONS:
			if (currentMenu != 0)
			{
				EventSystem.current.SetSelectedGameObject(null);
				if (currentMenu == MENU.CONTROLS)
				{
					ShowMenu(MENU.GAME);
				}
				else
				{
					ShowMenu(MENU.OPTIONS);
				}
			}
			else
			{
				acceptOrApplyButton.text = ScriptLocalization.UI_Map.LABEL_BUTTON_ACCEPT;
				if (initialOptions)
				{
					Show(b: false);
				}
				else
				{
					currentState = STATE.STATE_MAP;
				}
			}
			break;
		}
		if (soundBack != string.Empty)
		{
			Core.Audio.PlayOneShot(soundBack);
		}
	}

	public bool IsInControlRemapScreen()
	{
		return currentState == STATE.STATE_OPTIONS && currentMenu == MENU.CONTROLS;
	}

	public void ShowOrHideLeyend()
	{
		if (mapEnabled && (currentState == STATE.STATE_MAP || currentState == STATE.STATE_LEYEND))
		{
			currentState = ((currentState != STATE.STATE_MAP) ? STATE.STATE_MAP : STATE.STATE_LEYEND);
			if (soundBack != string.Empty)
			{
				Core.Audio.PlayOneShot(soundBack);
			}
		}
	}

	public void ShowOptions()
	{
		if (currentState != STATE.STATE_OPTIONS)
		{
			currentState = STATE.STATE_OPTIONS;
			ShowMenu(MENU.OPTIONS);
		}
	}

	public void ShowControlsRemap()
	{
		if (currentState == STATE.STATE_OPTIONS)
		{
			ShowMenu(MENU.CONTROLS);
		}
	}

	public void SelectPreviousTutorial()
	{
		if (currentState == STATE.STATE_OPTIONS && currentMenu == MENU.TUTORIAL)
		{
			currentTutorial--;
			if (currentTutorial < 0)
			{
				currentTutorial = tutorialOrder.Count - 1;
			}
			ShowCurrentTutorial();
		}
	}

	public void SelectNextTutorial()
	{
		if (currentState == STATE.STATE_OPTIONS && currentMenu == MENU.TUTORIAL)
		{
			currentTutorial++;
			if (currentTutorial >= tutorialOrder.Count)
			{
				currentTutorial = 0;
			}
			ShowCurrentTutorial();
		}
	}

	public void Option_OnSelect(GameObject item)
	{
		if ((bool)optionLastSelected)
		{
			SetOptionSelected(optionLastSelected, selected: false);
		}
		optionLastSelected = item;
		SetOptionSelected(item, selected: true);
	}

	public void Option_SelectGame(int idx)
	{
		SetOptionGameSelected(optionLastGameSelected, selected: false);
		optionLastGameSelected = (GAME_OPTIONS)idx;
		SetOptionGameSelected((GAME_OPTIONS)idx, selected: true);
	}

	public void Option_SelectAccessibility(int idx)
	{
		SetOptionAccessibilitySelected(optionLastAccessibilitySelected, selected: false);
		optionLastAccessibilitySelected = (ACCESSIBILITY_OPTIONS)idx;
		SetOptionAccessibilitySelected((ACCESSIBILITY_OPTIONS)idx, selected: true);
	}

	public void Option_SelectVideo(int idx)
	{
		SetOptionVideoSelected(optionLastVideoSelected, selected: false);
		optionLastVideoSelected = (VIDEO_OPTIONS)idx;
		SetOptionVideoSelected((VIDEO_OPTIONS)idx, selected: true);
	}

	public void Option_SelectAudio(int idx)
	{
		SetOptionAudioSelected(optionLastAudioSelected, selected: false);
		optionLastAudioSelected = (AUDIO_OPTIONS)idx;
		SetOptionAudioSelected((AUDIO_OPTIONS)idx, selected: true);
	}

	private void UpdateInputGameOptions(bool left)
	{
		int num = 1;
		if (left)
		{
			num = -1;
		}
		switch (optionLastGameSelected)
		{
		case GAME_OPTIONS.TEXTLANGUAGE:
			currentTextLanguageIndex += num;
			if (currentTextLanguageIndex >= Core.Localization.GetAllEnabledLanguagesNames().Count)
			{
				currentTextLanguageIndex = 0;
			}
			else if (currentTextLanguageIndex < 0)
			{
				currentTextLanguageIndex = Core.Localization.GetAllEnabledLanguagesNames().Count - 1;
			}
			break;
		case GAME_OPTIONS.ENABLEHOWTOPLAY:
			currentEnableHowToPlay = !currentEnableHowToPlay;
			break;
		}
		ShowGameValues();
	}

	private void UpdateInputAccessibilityOptions()
	{
		switch (optionLastAccessibilitySelected)
		{
		case ACCESSIBILITY_OPTIONS.RUMBLEENABLED:
			currentEnableRumble = !currentEnableRumble;
			break;
		case ACCESSIBILITY_OPTIONS.SHAKEENABLED:
			currentEnableShake = !currentEnableShake;
			break;
		}
		ShowAccessibilityValues();
	}

	private void UpdateInputVideoOptions(bool left)
	{
		int num = 1;
		if (left)
		{
			num = -1;
		}
		switch (optionLastVideoSelected)
		{
		case VIDEO_OPTIONS.BRIGHTNES:
			num *= stepBrightness;
			currentBrightness = Mathf.Clamp(currentBrightness + num, minBrightness, maxBrightness);
			break;
		case VIDEO_OPTIONS.FILTERING:
		{
			int length = Enum.GetValues(typeof(AnisotropicFiltering)).Length;
			int num2 = (int)(currentFilter + num);
			if (num2 < 0)
			{
				num2 = length - 1;
			}
			else if (num2 >= length)
			{
				num2 = 0;
			}
			currentFilter = (AnisotropicFiltering)num2;
			break;
		}
		case VIDEO_OPTIONS.RESOLUTION:
		{
			currentResolution += num;
			if (currentResolution < 0)
			{
				currentResolution = resolutions.Count - 1;
			}
			else if (currentResolution >= resolutions.Count)
			{
				currentResolution = 0;
			}
			Resolution resolution = resolutions[currentResolution];
			if (Core.Screen.ResolutionRequireStrategyScale(resolution.width, resolution.height))
			{
				videoElements[VIDEO_OPTIONS.RESOLUTIONMODE].SetActive(value: true);
				resolutionModeSelectionElement.SetActive(value: true);
				videoElements[VIDEO_OPTIONS.RESOLUTIONMODE].GetComponentInChildren<Text>(includeInactive: true).text = ((currentScalingStrategy != 0) ? ScriptLocalization.UI_Map.LABEL_MENU_VIDEO_SCALE : ScriptLocalization.UI_Map.LABEL_MENU_VIDEO_PIXELPERFECT);
				LinkButtonsVertical(resolutionButton, resolutionModeButton, firstAndLast: false);
				LinkButtonsVertical(resolutionModeButton, vsyncButton, firstAndLast: false);
			}
			else
			{
				videoElements[VIDEO_OPTIONS.RESOLUTIONMODE].SetActive(value: false);
				resolutionModeSelectionElement.SetActive(value: false);
				LinkButtonsVertical(resolutionButton, vsyncButton, firstAndLast: false);
			}
			break;
		}
		case VIDEO_OPTIONS.VSYNC:
			currentVsync = !currentVsync;
			break;
		case VIDEO_OPTIONS.WINDOWMODE:
			currentFullScreen = !currentFullScreen;
			break;
		case VIDEO_OPTIONS.RESOLUTIONMODE:
			if (currentScalingStrategy == SCALING_STRATEGY.PIXEL_PERFECT)
			{
				currentScalingStrategy = SCALING_STRATEGY.SCALE;
			}
			else
			{
				currentScalingStrategy = SCALING_STRATEGY.PIXEL_PERFECT;
			}
			break;
		}
		ShowVideoValues();
	}

	private void UpdateInputAudioOptions(bool left)
	{
		int num = 1;
		if (left)
		{
			num = -1;
		}
		switch (optionLastAudioSelected)
		{
		case AUDIO_OPTIONS.MASTERVOLUME:
			num *= stepVolume;
			currentMasterVolume = Mathf.Clamp(currentMasterVolume + num, minVolume, maxVolume);
			break;
		case AUDIO_OPTIONS.EFFECTSVOLUME:
			num *= stepVolume;
			currentEffectsVolume = Mathf.Clamp(currentEffectsVolume + num, minVolume, maxVolume);
			break;
		case AUDIO_OPTIONS.MUSICVOLUME:
			num *= stepVolume;
			currentMusicVolume = Mathf.Clamp(currentMusicVolume + num, minVolume, maxVolume);
			break;
		case AUDIO_OPTIONS.VOICEOVERVOLUME:
			num *= stepVolume;
			currentVoiceoverVolume = Mathf.Clamp(currentVoiceoverVolume + num, minVolume, maxVolume);
			break;
		}
		ShowAudioValues();
	}

	public void Option_Resume()
	{
		if (currentState == STATE.STATE_OPTIONS)
		{
			StartCoroutine(Option_ResumeSecure());
		}
	}

	private IEnumerator Option_ResumeSecure()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		Show(b: false);
	}

	public void Option_MenuGame()
	{
		if (currentState == STATE.STATE_OPTIONS)
		{
			ShowMenu(MENU.GAME);
		}
	}

	public void Option_MenuAccessibility()
	{
		if (currentState == STATE.STATE_OPTIONS)
		{
			ShowMenu(MENU.ACCESSIBILITY);
		}
	}

	public void Option_MenuVideo()
	{
		if (currentState == STATE.STATE_OPTIONS)
		{
			ShowMenu(MENU.VIDEO);
		}
	}

	public void Option_MenuAudio()
	{
		if (currentState == STATE.STATE_OPTIONS)
		{
			ShowMenu(MENU.AUDIO);
		}
	}

	public void Option_Tutorial()
	{
		if (currentState == STATE.STATE_OPTIONS && tutorialOrder.Count > 0)
		{
			ShowMenu(MENU.TUTORIAL);
		}
	}

	public void Option_MainMenu()
	{
		if (currentState == STATE.STATE_OPTIONS)
		{
			Core.Logic.ResumeGame();
			ResetMenus();
			Show(b: false);
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("Scene", SceneManager.GetActiveScene().name);
			Analytics.CustomEvent("QUIT_GAME", dictionary);
			Core.Logic.LoadMenuScene();
		}
	}

	public void Option_Reset()
	{
		if (currentState == STATE.STATE_OPTIONS)
		{
			Core.Logic.ResumeGame();
			ResetMenus();
			Show(b: false);
			Core.Logic.Penitent.KillInstanteneously();
		}
	}

	public void Option_Exit()
	{
		Option_MainMenu();
	}

	public void Option_AcceptGameOptions()
	{
		int currentLanguageIndex = Core.Localization.GetCurrentLanguageIndex();
		if (currentLanguageIndex != currentTextLanguageIndex)
		{
			StartCoroutine(ChangeTextLanguageAndGoBack());
			return;
		}
		Core.TutorialManager.TutorialsEnabled = currentEnableHowToPlay;
		GetCurrentAccessibilitySettings();
		GetCurrentVideoSettings();
		GetCurrentAudioSettings();
		WriteOptionsToFile();
	}

	private IEnumerator ChangeTextLanguageAndGoBack()
	{
		Core.Localization.SetLanguageByIdx(currentTextLanguageIndex);
		Core.TutorialManager.TutorialsEnabled = currentEnableHowToPlay;
		yield return new WaitForEndOfFrame();
		GetCurrentAccessibilitySettings();
		GetCurrentVideoSettings();
		GetCurrentAudioSettings();
		WriteOptionsToFile();
		ShowGameValues();
	}

	public void Option_AcceptAccesibilityOptions()
	{
		SingletonSerialized<RumbleSystem>.Instance.RumblesEnabled = currentEnableRumble;
		Core.Logic.CameraManager.ProCamera2DShake.enabled = currentEnableShake;
		GetCurrentGameSettings();
		GetCurrentVideoSettings();
		GetCurrentAudioSettings();
		WriteOptionsToFile();
	}

	public void ApplyVideoOptionsFromFile()
	{
		if (appliedResolutionIndex >= 0 && appliedResolutionIndex < resolutions.Count)
		{
			Resolution resolution = resolutions[appliedResolutionIndex];
			UnityEngine.Screen.SetResolution(resolution.width, resolution.height, currentFullScreen);
			Core.Screen.FitScreenCamera();
			UnityEngine.Screen.fullScreen = currentFullScreen;
			QualitySettings.vSyncCount = (currentVsync ? 1 : 0);
			float num = Mathf.Clamp01((float)currentBrightness / 255f);
			RenderSettings.ambientLight = new Color(num, num, num);
			QualitySettings.anisotropicFiltering = currentFilter;
		}
	}

	public void Option_AcceptVideoOptions()
	{
		Resolution resolution = resolutions[currentResolution];
		UnityEngine.Screen.SetResolution(resolution.width, resolution.height, currentFullScreen);
		Core.Screen.FitScreenCamera();
		appliedResolutionIndex = currentResolution;
		QualitySettings.vSyncCount = (currentVsync ? 1 : 0);
		float num = Mathf.Clamp01((float)currentBrightness / 255f);
		RenderSettings.ambientLight = new Color(num, num, num);
		QualitySettings.anisotropicFiltering = currentFilter;
		GetCurrentGameSettings();
		GetCurrentAccessibilitySettings();
		GetCurrentAudioSettings();
		WriteOptionsToFile();
	}

	public void Option_AcceptAudioOptions()
	{
		Core.Audio.MasterVolume = (float)currentMasterVolume / 100f;
		Core.Audio.SetSfxVolume((float)currentEffectsVolume / 100f);
		Core.Audio.SetMusicVolume((float)currentMusicVolume / 100f);
		Core.Audio.SetVoiceoverVolume((float)currentVoiceoverVolume / 100f);
		GetCurrentGameSettings();
		GetCurrentAccessibilitySettings();
		GetCurrentVideoSettings();
		WriteOptionsToFile();
	}

	public void Option_AcceptControlsOptions()
	{
		Core.ControlRemapManager.WriteControlsSettingsToFile();
		GoBack();
	}

	private Vector3 SnapToMapCell(Vector3 pos, bool changeX)
	{
		float x = pos.x;
		if (changeX)
		{
			x = Mathf.Round(pos.x * FACTOR_X / (float)CELL_WIDTH) * (float)CELL_WIDTH;
		}
		float num = Mathf.Round(pos.y * FACTOR_Y / (float)CELL_HEIGHT) * (float)CELL_HEIGHT;
		foreach (RangePixels item in cellPixelFix)
		{
			if (num >= (float)item.cellMin && num <= (float)item.cellMax)
			{
				num += (float)item.value;
				break;
			}
		}
		return new Vector3(x, num, pos.z);
	}

	private void UpdateZoneInfo()
	{
		domainCaption.text = Core.MapManager.GetCurrentDomainName();
		zoneCaption.text = Core.MapManager.GetCurrentZoneName();
	}

	private void UpdateMenuButtons()
	{
		bool flag = !initialOptions && Core.TutorialManager.AnyTutorialIsUnlocked();
		TutorialMenuOption.transform.parent.gameObject.SetActive(flag);
		ExitToMainMenuOption.transform.parent.gameObject.SetActive(!initialOptions);
		ResumeGameOption.transform.parent.gameObject.SetActive(!initialOptions);
		if (initialOptions)
		{
			if (flag)
			{
				LinkButtonsVertical(beforeToTutorialMenuOption, TutorialMenuOption, firstAndLast: false);
				LinkButtonsVertical(TutorialMenuOption, afterToResumeGameOption, firstAndLast: false);
			}
			else
			{
				LinkButtonsVertical(beforeToTutorialMenuOption, afterToResumeGameOption, firstAndLast: false);
			}
			return;
		}
		if (flag)
		{
			LinkButtonsVertical(beforeToTutorialMenuOption, TutorialMenuOption, firstAndLast: false);
			LinkButtonsVertical(TutorialMenuOption, ExitToMainMenuOption, firstAndLast: false);
		}
		else
		{
			LinkButtonsVertical(beforeToTutorialMenuOption, ExitToMainMenuOption, firstAndLast: false);
		}
		LinkButtonsVertical(ExitToMainMenuOption, ResumeGameOption, firstAndLast: false);
		LinkButtonsVertical(ResumeGameOption, afterToResumeGameOption, firstAndLast: false);
	}

	private void ShowMenu(MENU menu)
	{
		currentMenu = menu;
		foreach (KeyValuePair<MENU, Transform> item in optionsRoot)
		{
			if (item.Value != null)
			{
				CanvasGroup component = item.Value.gameObject.GetComponent<CanvasGroup>();
				component.alpha = ((item.Key != currentMenu) ? 0f : 1f);
				component.interactable = item.Key == currentMenu;
			}
		}
		navigationButtonsRoot.SetActive(value: true);
		acceptOrApplyButton.text = ScriptLocalization.UI_Map.LABEL_BUTTON_APPLY;
		switch (currentMenu)
		{
		case MENU.TUTORIAL:
			currentTutorial = 0;
			ShowCurrentTutorial();
			break;
		case MENU.GAME:
			foreach (KeyValuePair<GAME_OPTIONS, SelectableOption> gameElement in gameElements)
			{
				SetOptionGameSelected(gameElement.Key, selected: false);
			}
			GetCurrentGameSettings();
			ShowGameValues();
			break;
		case MENU.ACCESSIBILITY:
			foreach (KeyValuePair<GAME_OPTIONS, SelectableOption> gameElement2 in gameElements)
			{
				SetOptionGameSelected(gameElement2.Key, selected: false);
			}
			GetCurrentAccessibilitySettings();
			ShowAccessibilityValues();
			break;
		case MENU.VIDEO:
			foreach (KeyValuePair<VIDEO_OPTIONS, GameObject> videoElement in videoElements)
			{
				SetOptionVideoSelected(videoElement.Key, selected: false);
			}
			GetCurrentVideoSettings();
			currentResolution = appliedResolutionIndex;
			ShowVideoValues();
			break;
		case MENU.AUDIO:
			foreach (KeyValuePair<AUDIO_OPTIONS, GameObject> audioElement in audioElements)
			{
				SetOptionAudioSelected(audioElement.Key, selected: false);
			}
			GetCurrentAudioSettings();
			ShowAudioValues();
			break;
		case MENU.CONTROLS:
			foreach (KeyValuePair<MENU, Transform> item2 in optionsRoot)
			{
				if (item2.Value != null)
				{
					CanvasGroup component2 = item2.Value.gameObject.GetComponent<CanvasGroup>();
					component2.alpha = 0f;
					component2.interactable = false;
				}
			}
			controlsMenuScreen.Open();
			break;
		case MENU.OPTIONS:
			acceptOrApplyButton.text = ScriptLocalization.UI_Map.LABEL_BUTTON_ACCEPT;
			break;
		}
		Transform transform = optionsRoot[currentMenu];
		if (transform == null)
		{
			return;
		}
		transform = transform.Find("Selection");
		if (!(transform == null))
		{
			lastHorizontalInOptions = 0f;
			for (int i = 0; i < transform.childCount; i++)
			{
				SetOptionSelected(transform.GetChild(i).gameObject, i == 0);
			}
			optionLastSelected = transform.GetChild(0).gameObject;
			EventSystem.current.SetSelectedGameObject(optionLastSelected.GetComponentInChildren<Text>(includeInactive: true).gameObject);
		}
	}

	public void GoBackFromControlsRemapScreen()
	{
		GoBack();
	}

	private void GetCurrentGameSettings()
	{
		currentTextLanguageIndex = Core.Localization.GetCurrentLanguageIndex();
		currentEnableHowToPlay = Core.TutorialManager.TutorialsEnabled;
	}

	private void GetCurrentAccessibilitySettings()
	{
		currentEnableRumble = SingletonSerialized<RumbleSystem>.Instance.RumblesEnabled;
		currentEnableShake = Core.Logic.CameraManager.ProCamera2DShake.enabled;
	}

	private void GetCurrentVideoSettings()
	{
		InitializeSupportedResolutions();
		currentVsync = QualitySettings.vSyncCount > 0;
		currentFullScreen = UnityEngine.Screen.fullScreen;
		currentBrightness = (int)(RenderSettings.ambientLight.r * 255f);
		currentBrightness = Mathf.Clamp(currentBrightness, minBrightness, maxBrightness);
		currentFilter = QualitySettings.anisotropicFiltering;
	}

	private void InitializeSupportedResolutions()
	{
		resolutions.Clear();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		Resolution[] validResolutions = GetValidResolutions(UnityEngine.Screen.resolutions);
		for (int i = 0; i < validResolutions.Length; i++)
		{
			Resolution item = validResolutions[i];
			if (i <= 0 || item.width != validResolutions[i - 1].width || item.height != validResolutions[i - 1].height)
			{
				resolutions.Add(item);
				if (currentResolution == -1 && item.width == UnityEngine.Screen.currentResolution.width && item.height == UnityEngine.Screen.currentResolution.height)
				{
					currentResolution = num;
				}
				if (item.width > num2)
				{
					num2 = item.width;
					num3 = num;
				}
				num++;
			}
		}
		if (currentResolution == -1)
		{
			currentResolution = num3;
		}
	}

	private Resolution[] GetValidResolutions(Resolution[] res)
	{
		return res.Where((Resolution currentRes) => !((float)currentRes.height < 360f) && !((float)currentRes.width < 640f)).ToArray();
	}

	private Resolution GetFitResolution(Resolution[] unityAvailableResolutions, int savedScreenResIndex)
	{
		Resolution resolution = resolutions[0];
		List<Resolution> list = unityAvailableResolutions.ToList();
		if (savedScreenResIndex < resolutions.Count)
		{
			resolution = resolutions[savedScreenResIndex];
		}
		if (list.Contains(resolution))
		{
			return resolution;
		}
		Resolution resolution2 = UnityEngine.Screen.currentResolution;
		if (list.Contains(resolution2))
		{
			return resolution2;
		}
		List<Resolution> list2 = (from x in resolutions.ToList()
			orderby x.height descending
			select x).ToList();
		foreach (Resolution item in list2)
		{
			if (list.Contains(item))
			{
				return item;
			}
		}
		return resolution;
	}

	private void GetCurrentAudioSettings()
	{
		currentMasterVolume = (int)(Core.Audio.MasterVolume * 100f);
		currentEffectsVolume = (int)(Core.Audio.GetSfxVolume() * 100f);
		currentMusicVolume = (int)(Core.Audio.GetMusicVolume() * 100f);
		currentVoiceoverVolume = (int)(Core.Audio.GetVoiceoverVolume() * 100f);
	}

	private void SetOptionSelected(GameObject option, bool selected)
	{
		option.GetComponentInChildren<Text>(includeInactive: true).color = ((!selected) ? optionNormalColor : optionHighligterColor);
		option.GetComponentInChildren<Image>(includeInactive: true).gameObject.SetActive(selected);
	}

	private void SetOptionGameSelected(GAME_OPTIONS option, bool selected)
	{
		if (option != 0)
		{
			gameElements[option].selectionTransform.SetActive(selected);
		}
		gameElements[option].highlightableText.color = ((!selected) ? optionNormalColor : optionHighligterColor);
	}

	private void SetOptionAccessibilitySelected(ACCESSIBILITY_OPTIONS option, bool selected)
	{
		accessibilityElements[option].transform.Find("Selection").gameObject.SetActive(selected);
		accessibilityElements[option].GetComponentInChildren<Text>(includeInactive: true).color = ((!selected) ? optionNormalColor : optionHighligterColor);
	}

	private void SetOptionVideoSelected(VIDEO_OPTIONS option, bool selected)
	{
		videoElements[option].transform.Find("Selection").gameObject.SetActive(selected);
		if (option != VIDEO_OPTIONS.BRIGHTNES)
		{
			videoElements[option].GetComponentInChildren<Text>(includeInactive: true).color = ((!selected) ? optionNormalColor : optionHighligterColor);
		}
	}

	private void SetOptionAudioSelected(AUDIO_OPTIONS option, bool selected)
	{
		audioElements[option].transform.Find("Selection").gameObject.SetActive(selected);
	}

	private void ShowGameValues()
	{
		string key = "UI_Map/LABEL_MENU_" + Core.Localization.GetLanguageCodeByIndex(1).ToUpper() + "_LANGUAGENAME";
		gameElements[GAME_OPTIONS.AUDIOLANGUAGE].highlightableText.text = Core.Localization.Get(key);
		string languageNameByIndex = Core.Localization.GetLanguageNameByIndex(currentTextLanguageIndex);
		string term = "UI_Map/LABEL_MENU_LANGUAGENAME";
		string overrideLanguage = languageNameByIndex;
		string text = ScriptLocalization.Get(term, FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, overrideLanguage);
		gameElements[GAME_OPTIONS.TEXTLANGUAGE].highlightableText.text = text;
		gameElements[GAME_OPTIONS.TEXTLANGUAGE].highlightableText.font = Core.Localization.GetFontByLanguageName(languageNameByIndex);
		gameElements[GAME_OPTIONS.ENABLEHOWTOPLAY].highlightableText.text = ((!currentEnableHowToPlay) ? ScriptLocalization.UI.DISABLED_TEXT : ScriptLocalization.UI.ENABLED_TEXT);
	}

	private void ShowAccessibilityValues()
	{
		accessibilityElements[ACCESSIBILITY_OPTIONS.RUMBLEENABLED].GetComponentInChildren<Text>(includeInactive: true).text = ((!currentEnableRumble) ? ScriptLocalization.UI.DISABLED_TEXT : ScriptLocalization.UI.ENABLED_TEXT);
		accessibilityElements[ACCESSIBILITY_OPTIONS.SHAKEENABLED].GetComponentInChildren<Text>(includeInactive: true).text = ((!currentEnableShake) ? ScriptLocalization.UI.DISABLED_TEXT : ScriptLocalization.UI.ENABLED_TEXT);
	}

	private void ShowVideoValues()
	{
		if (resolutions.Count == 0)
		{
			InitializeSupportedResolutions();
		}
		Resolution resolution = resolutions[currentResolution];
		Debug.Log(" show videos 2 " + resolution.width + "x" + resolution.height);
		videoElements[VIDEO_OPTIONS.RESOLUTION].GetComponentInChildren<Text>(includeInactive: true).text = resolution.width + "x" + resolution.height;
		videoElements[VIDEO_OPTIONS.WINDOWMODE].GetComponentInChildren<Text>(includeInactive: true).text = ((!currentFullScreen) ? ScriptLocalization.UI_Map.LABEL_MENU_VIDEO_WINDOWED : ScriptLocalization.UI_Map.LABEL_MENU_VIDEO_FULLSCREEN);
		videoElements[VIDEO_OPTIONS.VSYNC].GetComponentInChildren<Text>(includeInactive: true).text = ((!currentVsync) ? ScriptLocalization.UI.DISABLED_TEXT : ScriptLocalization.UI.ENABLED_TEXT);
		videoElements[VIDEO_OPTIONS.FILTERING].GetComponentInChildren<Text>(includeInactive: true).text = currentFilter.ToString();
		float currentValue = ((float)currentBrightness - (float)minBrightness) / ((float)maxBrightness - (float)minBrightness);
		videoElements[VIDEO_OPTIONS.BRIGHTNES].GetComponentInChildren<CustomScrollBar>(includeInactive: true).CurrentValue = currentValue;
		if (Core.Screen.ResolutionRequireStrategyScale(resolution.width, resolution.height))
		{
			videoElements[VIDEO_OPTIONS.RESOLUTIONMODE].SetActive(value: true);
			resolutionModeSelectionElement.SetActive(value: true);
			videoElements[VIDEO_OPTIONS.RESOLUTIONMODE].GetComponentInChildren<Text>(includeInactive: true).text = ((currentScalingStrategy != 0) ? ScriptLocalization.UI_Map.LABEL_MENU_VIDEO_SCALE : ScriptLocalization.UI_Map.LABEL_MENU_VIDEO_PIXELPERFECT);
			LinkButtonsVertical(resolutionButton, resolutionModeButton, firstAndLast: false);
			LinkButtonsVertical(resolutionModeButton, vsyncButton, firstAndLast: false);
		}
		else
		{
			videoElements[VIDEO_OPTIONS.RESOLUTIONMODE].SetActive(value: false);
			resolutionModeSelectionElement.SetActive(value: false);
			LinkButtonsVertical(resolutionButton, vsyncButton, firstAndLast: false);
		}
	}

	public SCALING_STRATEGY GetScalingStrategy()
	{
		return currentScalingStrategy;
	}

	private void ShowAudioValues()
	{
		float currentValue = ((float)currentMasterVolume - (float)minVolume) / ((float)maxVolume - (float)minVolume);
		audioElements[AUDIO_OPTIONS.MASTERVOLUME].GetComponentInChildren<CustomScrollBar>(includeInactive: true).CurrentValue = currentValue;
		float currentValue2 = ((float)currentEffectsVolume - (float)minVolume) / ((float)maxVolume - (float)minVolume);
		audioElements[AUDIO_OPTIONS.EFFECTSVOLUME].GetComponentInChildren<CustomScrollBar>(includeInactive: true).CurrentValue = currentValue2;
		float currentValue3 = ((float)currentMusicVolume - (float)minVolume) / ((float)maxVolume - (float)minVolume);
		audioElements[AUDIO_OPTIONS.MUSICVOLUME].GetComponentInChildren<CustomScrollBar>(includeInactive: true).CurrentValue = currentValue3;
		float currentValue4 = ((float)currentVoiceoverVolume - (float)minVolume) / ((float)maxVolume - (float)minVolume);
		audioElements[AUDIO_OPTIONS.VOICEOVERVOLUME].GetComponentInChildren<CustomScrollBar>(includeInactive: true).CurrentValue = currentValue4;
	}

	private void DeleteAllChildren(Transform parent)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in parent)
		{
			list.Add(item.gameObject);
		}
		foreach (GameObject item2 in list)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(item2);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(item2);
			}
		}
	}

	private void CreateMap()
	{
		mapPieces.Clear();
		lastSelectedMapPiece = null;
		DeleteAllChildren(mapBigLayout);
		Core.MapManager.GetCurrentZone()?.UpdateElementsStatus();
		mapRoot = CreateUIMapImage(mapBigLayout, "ZONES", Vector3.zero, null);
		allmapRoot = CreateUIMapImage(mapBigLayout, "REVEAL", Vector3.zero, null);
		playerSpriteInMap = CreateUIMapImage(mapBigLayout, "PLAYER", Vector3.zero, playerSprite);
		playerSpriteInMap.GetComponent<Image>().color = mapPlayerColor;
		elementsRoot = CreateUIMapImage(mapBigLayout, "ELEMENTS", Vector3.zero, null);
		foreach (string zones in Core.MapManager.GetZonesList())
		{
			MapManager.DataMapReveal zone = Core.MapManager.GetZone(zones);
			RectTransform rectTransform = CreateUIMapImage(mapRoot, "Map_" + zones, zone.GetWorldPosition(), zone.mask);
			mapPieces[zones] = rectTransform.gameObject.GetComponent<Image>();
			mapPieces[zones].color = mapNotSelected;
			UpdateZoneElements(zone);
		}
		allmapRoot.gameObject.SetActive(value: false);
	}

	private void UpdateZoneElements(MapManager.DataMapReveal zone)
	{
		foreach (KeyValuePair<string, MapManager.ElementsRevealed> element in zone.elements)
		{
			Sprite sprite = null;
			bool changeX = true;
			switch (element.Value.element)
			{
			case MapManager.MapElementType.Door:
				sprite = ((!element.Value.activatedOrOpen) ? mapDoorClose : mapDoorOpen);
				break;
			case MapManager.MapElementType.Gate:
				sprite = ((!element.Value.activatedOrOpen) ? mapGateVertical : null);
				changeX = false;
				break;
			case MapManager.MapElementType.Reclinatory:
				sprite = ((!element.Value.activatedOrOpen) ? mapPrieDieuOff : mapPrieDieuLight);
				break;
			case MapManager.MapElementType.MeaCulpa:
				sprite = mapMeaCulpa;
				break;
			case MapManager.MapElementType.Teleport:
				sprite = mapTravel;
				break;
			}
			if (sprite != null)
			{
				Vector2 vector = SnapToMapCell(zone.WorldToTexture(element.Value.pos), changeX);
				RectTransform rectTransform = CreateUIMapImage(elementsRoot, element.Key, vector, sprite);
				rectTransform.GetComponent<Image>().color = mapElementColor;
			}
		}
		if (!(mapGuiltDrop != null))
		{
			return;
		}
		foreach (GuiltManager.GuiltDrop allDrop in Core.GuiltManager.GetAllDrops())
		{
			RectTransform rectTransform2 = CreateUIMapImage(elementsRoot, allDrop.id, zone.WorldToTexture(allDrop.position), mapGuiltDrop);
			rectTransform2.GetComponent<Image>().color = mapElementColor;
		}
	}

	private RectTransform CreateUIMapImage(Transform parent, string name, Vector3 position, Sprite sprite, bool addMask = false)
	{
		GameObject gameObject = new GameObject(name, typeof(RectTransform));
		gameObject.transform.SetParent(parent);
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.localRotation = Quaternion.identity;
		rectTransform.localScale = Vector3.one;
		Vector3 localPosition = new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), Mathf.Round(position.z));
		rectTransform.localPosition = localPosition;
		if ((bool)sprite)
		{
			rectTransform.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
			Image image = gameObject.AddComponent<Image>();
			image.sprite = sprite;
			if (addMask)
			{
				gameObject.AddComponent<Mask>();
			}
		}
		else
		{
			rectTransform.sizeDelta = new Vector2(1f, 1f);
		}
		return rectTransform;
	}

	private void SetMapFocusOnPosition(float posX, float posY)
	{
		mapPosX = Mathf.Round(posX);
		mapPosY = Mathf.Round(posY);
		if (mapPosX % 2f != 0f)
		{
			mapPosX += 1f;
		}
		if (mapPosY % 2f != 0f)
		{
			mapPosY += 1f;
		}
		mapBigLayout.localPosition = new Vector3(0f - mapPosX, 0f - mapPosY, 0f);
	}

	private void ResetMenus()
	{
		foreach (KeyValuePair<MENU, Transform> item in optionsRoot)
		{
			if (item.Value != null)
			{
				item.Value.gameObject.SetActive(value: true);
				CanvasGroup component = item.Value.gameObject.GetComponent<CanvasGroup>();
				component.alpha = 0f;
				component.interactable = false;
			}
		}
		int num = 0;
		List<string> allEnabledLanguagesNames = Core.Localization.GetAllEnabledLanguagesNames();
		EventsButton first = null;
		EventsButton second = null;
		EventsButton eventsButton = null;
		foreach (Transform item2 in languageRoot)
		{
			EventsButton component2 = item2.GetComponent<EventsButton>();
			if (num == 0)
			{
				first = component2;
			}
			if (num < allEnabledLanguagesNames.Count)
			{
				item2.gameObject.SetActive(value: true);
				item2.GetComponentInChildren<Text>().text = allEnabledLanguagesNames[num];
				second = component2;
				if (eventsButton != null)
				{
					LinkButtonsVertical(eventsButton, component2, firstAndLast: false);
				}
				eventsButton = component2;
			}
			else
			{
				item2.gameObject.SetActive(value: false);
			}
			num++;
		}
		LinkButtonsVertical(first, second, firstAndLast: true);
	}

	private void LinkButtonsVertical(EventsButton first, EventsButton second, bool firstAndLast)
	{
		Navigation navigation = first.navigation;
		Navigation navigation2 = second.navigation;
		if (firstAndLast)
		{
			navigation.selectOnUp = second;
			navigation2.selectOnDown = first;
		}
		else
		{
			navigation.selectOnDown = second;
			navigation2.selectOnUp = first;
		}
		first.navigation = navigation;
		second.navigation = navigation2;
	}

	private void LinkButtonsHorizontal(EventsButton first, EventsButton second)
	{
		Navigation navigation = first.navigation;
		Navigation navigation2 = second.navigation;
		navigation.selectOnRight = second;
		navigation2.selectOnLeft = first;
		first.navigation = navigation;
		second.navigation = navigation2;
	}

	private void UpdateTutorials()
	{
		tutorialOrder.Clear();
		foreach (Tutorial unlockedTutorial in Core.TutorialManager.GetUnlockedTutorials())
		{
			tutorialOrder.Add(unlockedTutorial.id);
			if (!tutorialInstances.ContainsKey(unlockedTutorial.id))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(unlockedTutorial.prefab, Vector3.zero, Quaternion.identity, optionsRoot[MENU.TUTORIAL]);
				gameObject.transform.localPosition = Vector3.zero;
				tutorialInstances[unlockedTutorial.id] = gameObject;
			}
		}
	}

	private void ShowCurrentTutorial()
	{
		foreach (KeyValuePair<string, GameObject> tutorialInstance in tutorialInstances)
		{
			tutorialInstance.Value.SetActive(value: false);
		}
		GameObject gameObject = tutorialInstances[tutorialOrder[currentTutorial]];
		gameObject.SetActive(value: true);
		gameObject.GetComponent<TutorialWidget>().ShowInMenu(currentTutorial + 1, tutorialOrder.Count);
	}

	private void WriteOptionsToFile()
	{
		try
		{
			string pathOptionsSettings = GetPathOptionsSettings();
			if (!File.Exists(pathOptionsSettings))
			{
				File.CreateText(pathOptionsSettings).Dispose();
			}
			fsData fsData = fsData.CreateDictionary();
			fsData.AsDictionary["textLanguageIndex"] = new fsData(currentTextLanguageIndex);
			fsData.AsDictionary["enableTips"] = new fsData(currentEnableHowToPlay);
			fsData.AsDictionary["enableControllerRumble"] = new fsData(currentEnableRumble);
			fsData.AsDictionary["enableCameraRumble"] = new fsData(currentEnableShake);
			fsData.AsDictionary["enableVsync"] = new fsData(currentVsync);
			fsData.AsDictionary["enableFullScreen"] = new fsData(currentFullScreen);
			fsData.AsDictionary["screenBrightness"] = new fsData(currentBrightness);
			fsData.AsDictionary["screenResolution"] = new fsData(currentResolution);
			fsData.AsDictionary["anisotropicFiltering"] = new fsData(Enum.GetName(typeof(AnisotropicFiltering), currentFilter));
			fsData.AsDictionary["resolutionMode"] = new fsData(Enum.GetName(typeof(SCALING_STRATEGY), currentScalingStrategy));
			fsData.AsDictionary["masterVolume"] = new fsData(currentMasterVolume);
			fsData.AsDictionary["effectsVolume"] = new fsData(currentEffectsVolume);
			fsData.AsDictionary["musicVolume"] = new fsData(currentMusicVolume);
			fsData.AsDictionary["voiceVolume"] = new fsData(currentVoiceoverVolume);
			string encryptedData = fsJsonPrinter.CompressedJson(fsData);
			FileTools.SaveSecure(pathOptionsSettings, encryptedData);
		}
		catch (IOException ex)
		{
			Debug.LogError(ex.Message + ex.StackTrace);
		}
	}

	private void ReadOptionsFromFile()
	{
		string pathOptionsSettings = GetPathOptionsSettings();
		if (!File.Exists(pathOptionsSettings))
		{
			File.CreateText(pathOptionsSettings);
			return;
		}
		string input = File.ReadAllText(pathOptionsSettings);
		fsData data;
		fsResult fsResult = fsJsonParser.Parse(input, out data);
		if (fsResult.Failed && !fsResult.FormattedMessages.Equals("No input"))
		{
			Debug.LogError("ReadOptionsFromFile: parsing error: " + fsResult.FormattedMessages);
		}
		else if (data != null)
		{
			Dictionary<string, fsData> asDictionary = data.AsDictionary;
			Core.Localization.SetLanguageByIdx((int)asDictionary["textLanguageIndex"].AsInt64);
			Core.TutorialManager.TutorialsEnabled = asDictionary["enableTips"].AsBool;
			SingletonSerialized<RumbleSystem>.Instance.RumblesEnabled = asDictionary["enableControllerRumble"].AsBool;
			Core.Logic.CameraManager.ProCamera2DShake.enabled = asDictionary["enableCameraRumble"].AsBool;
			currentVsync = asDictionary["enableVsync"].AsBool;
			QualitySettings.vSyncCount = (currentVsync ? 1 : 0);
			float num = Mathf.Clamp01((float)(int)asDictionary["screenBrightness"].AsInt64 / 255f);
			RenderSettings.ambientLight = new Color(num, num, num);
			int savedScreenResIndex = (int)asDictionary["screenResolution"].AsInt64;
			if (resolutions.Count == 0)
			{
				InitializeSupportedResolutions();
			}
			Resolution resolution = (savedResolution = GetFitResolution(GetValidResolutions(UnityEngine.Screen.resolutions), savedScreenResIndex));
			appliedResolutionIndex = savedScreenResIndex;
			currentFullScreen = asDictionary["enableFullScreen"].AsBool;
			UnityEngine.Screen.SetResolution(resolution.width, resolution.height, currentFullScreen);
			QualitySettings.anisotropicFiltering = (AnisotropicFiltering)Enum.Parse(typeof(AnisotropicFiltering), asDictionary["anisotropicFiltering"].AsString);
			currentScalingStrategy = (SCALING_STRATEGY)Enum.Parse(typeof(SCALING_STRATEGY), asDictionary["resolutionMode"].AsString);
			ApplyVideoOptionsFromFile();
			Core.Audio.MasterVolume = (float)(int)asDictionary["masterVolume"].AsInt64 / 100f;
			Core.Audio.SetSfxVolume((float)(int)asDictionary["effectsVolume"].AsInt64 / 100f);
			Core.Audio.SetMusicVolume((float)(int)asDictionary["musicVolume"].AsInt64 / 100f);
			Core.Audio.SetVoiceoverVolume((float)(int)asDictionary["voiceVolume"].AsInt64 / 100f);
		}
	}

	private string GetPathOptionsSettings()
	{
		return PersistentManager.GetPathAppSettings("/options_settings.json");
	}
}
