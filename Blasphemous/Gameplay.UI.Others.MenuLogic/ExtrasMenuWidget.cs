using System;
using System.Collections.Generic;
using Framework.Achievements;
using Framework.Managers;
using FullSerializer;
using Gameplay.UI.Others.Buttons;
using Gameplay.UI.Widgets;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class ExtrasMenuWidget : SerializedMonoBehaviour
{
	[Serializable]
	public struct SkinSelectorElement
	{
		public string skinKey;

		public GameObject element;
	}

	private enum STATE
	{
		STATE_OFF,
		STATE_EXTRAS
	}

	private enum MENU
	{
		EXTRAS,
		SKINSELECTOR,
		ACHIEVEMENT
	}

	[SerializeField]
	[BoxGroup("Roots", true, false, 0)]
	private Transform ExtrasRoot;

	[SerializeField]
	[BoxGroup("Roots", true, false, 0)]
	private Transform SelectorRoot;

	[SerializeField]
	[BoxGroup("Roots", true, false, 0)]
	private Transform AchievementRoot;

	private Dictionary<MENU, Transform> extrasRoot = new Dictionary<MENU, Transform>();

	[SerializeField]
	[BoxGroup("Extras", true, false, 0)]
	private Color extrasNormalColor = new Color(0.972549f, 76f / 85f, 0.78039217f);

	[SerializeField]
	[BoxGroup("Extras", true, false, 0)]
	private Color extrasHighligterColor = new Color(0.80784315f, 72f / 85f, 0.49803922f);

	[SerializeField]
	[BoxGroup("Sounds", true, false, 0)]
	private string soundBack = "event:/SFX/UI/ChangeTab";

	[SerializeField]
	[BoxGroup("Options Extras", true, false, 0)]
	private EventsButton skinSelectorOption;

	[SerializeField]
	[BoxGroup("Options Extras", true, false, 0)]
	private EventsButton skinSelectorOptionPrevious;

	[SerializeField]
	[BoxGroup("Options Extras", true, false, 0)]
	private EventsButton skinSelectorOptionNext;

	[SerializeField]
	[BoxGroup("Skin Selector Data Elements", true, false, 0)]
	private List<SkinSelectorElement> skinSelectorDataElements;

	[SerializeField]
	[BoxGroup("Skin Selector Selection Elements", true, false, 0)]
	private List<SkinSelectorElement> skinSelectorSelectionElements;

	[SerializeField]
	[BoxGroup("Achievements", true, false, 0)]
	private CustomScrollView AchievementScrollView;

	[SerializeField]
	[BoxGroup("Achievements", true, false, 0)]
	private GameObject AchievementElement;

	private Animator animator;

	private GameObject optionLastSelected;

	private const string SHADER_NAME = "Maikel/MaikelSpriteMasterShader";

	private STATE _currentState;

	private fsSerializer serializer;

	private MENU currentMenu;

	private List<string> allSkins;

	private const string PALETTE_PATH = "Color Palettes/AVAILABLE_COLOR_PALETTES";

	private Player rewired;

	private float lastHorizontalInOptions;

	private float lastVerticalInOptions;

	private string optionLastSkinSelected = "PENITENT_DEFAULT";

	private string currentSkin;

	private Shader colorPaletteShader;

	private Dictionary<string, GameObject> AchivementsCache = new Dictionary<string, GameObject>();

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

	private void Awake()
	{
		extrasRoot.Clear();
		extrasRoot[MENU.ACHIEVEMENT] = AchievementRoot;
		extrasRoot[MENU.EXTRAS] = ExtrasRoot;
		extrasRoot[MENU.SKINSELECTOR] = SelectorRoot;
		serializer = new fsSerializer();
		currentState = STATE.STATE_OFF;
		animator = GetComponent<Animator>();
		currentMenu = MENU.EXTRAS;
		colorPaletteShader = Shader.Find("Maikel/MaikelSpriteMasterShader");
		if (colorPaletteShader == null)
		{
			Debug.LogError("Couldn't find shader: Maikel/MaikelSpriteMasterShader");
		}
		allSkins = Core.ColorPaletteManager.GetAllColorPalettesId();
		ResetMenus();
		CreateAchievements();
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
		float axisRaw2 = rewired.GetAxisRaw(49);
		bool flag = axisRaw < 0.3f && (double)axisRaw >= -0.3 && axisRaw2 < 0.3f && (double)axisRaw2 >= -0.3;
		if (currentState != STATE.STATE_EXTRAS)
		{
			return;
		}
		MENU mENU = currentMenu;
		if (mENU != MENU.SKINSELECTOR)
		{
			return;
		}
		if ((flag && lastHorizontalInOptions != 0f) || (flag && lastVerticalInOptions != 0f))
		{
			UpdateInputSkinSelectorOptions();
			lastHorizontalInOptions = 0f;
			lastVerticalInOptions = 0f;
			return;
		}
		if (!flag && axisRaw < 0f)
		{
			lastHorizontalInOptions = -1f;
		}
		else if (!flag && axisRaw > 0f)
		{
			lastHorizontalInOptions = 1f;
		}
		if (!flag && axisRaw2 < 0f)
		{
			lastVerticalInOptions = -1f;
		}
		else if (!flag && axisRaw2 > 0f)
		{
			lastVerticalInOptions = 1f;
		}
	}

	public void ShowExtras()
	{
		Core.Input.SetBlocker("INGAME_MENU", blocking: true);
		Core.Logic.SetState(LogicStates.Unresponsive);
		currentState = STATE.STATE_EXTRAS;
		ShowMenu(MENU.EXTRAS);
	}

	private void LinkAllButtons()
	{
		List<string> allUnlockedColorPalettesId = Core.ColorPaletteManager.GetAllUnlockedColorPalettesId();
		string text = allSkins[0];
		EventsButton first = skinSelectorSelectionElements.Find((SkinSelectorElement x) => x.skinKey == allSkins[0]).element.GetComponentInChildren<EventsButton>();
		int i;
		for (i = 1; i <= allSkins.Count; i++)
		{
			if (i == allSkins.Count)
			{
				EventsButton componentInChildren = skinSelectorSelectionElements.Find((SkinSelectorElement x) => x.skinKey == allSkins[0]).element.GetComponentInChildren<EventsButton>();
				LinkButtonHorizontal(first, componentInChildren);
			}
			else if (allUnlockedColorPalettesId.Contains(allSkins[i]))
			{
				EventsButton componentInChildren2 = skinSelectorSelectionElements.Find((SkinSelectorElement x) => x.skinKey == allSkins[i]).element.GetComponentInChildren<EventsButton>();
				LinkButtonHorizontal(first, componentInChildren2);
				first = componentInChildren2;
				text = allSkins[i];
			}
		}
	}

	public void Hide()
	{
		if (!FadeWidget.instance.Fading)
		{
			ResetMenus();
			Core.Logic.SetState(LogicStates.Playing);
			currentState = STATE.STATE_OFF;
			Core.Input.SetBlocker("INGAME_MENU", blocking: false);
			UIController.instance.ShowMainMenu(string.Empty);
		}
	}

	public void GoBack()
	{
		STATE sTATE = currentState;
		if (sTATE == STATE.STATE_EXTRAS)
		{
			if (currentMenu != 0)
			{
				EventSystem.current.SetSelectedGameObject(null);
				ShowMenu(MENU.EXTRAS);
			}
			else
			{
				Hide();
			}
		}
		if (soundBack != string.Empty)
		{
			Core.Audio.PlayOneShot(soundBack);
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

	public void Option_OnSelectSkin(int idx)
	{
		string item = allSkins[idx];
		SetOptionSkinSelected(optionLastSkinSelected, selected: false);
		skinSelectorSelectionElements.Find((SkinSelectorElement x) => x.skinKey == optionLastSkinSelected).element.GetComponentInChildren<Text>().enabled = false;
		optionLastSkinSelected = item;
		SetOptionSkinSelected(item, selected: true);
		skinSelectorSelectionElements.Find((SkinSelectorElement x) => x.skinKey == item).element.GetComponentInChildren<Text>().enabled = true;
	}

	public void Option_MenuCredits()
	{
		if (currentState == STATE.STATE_EXTRAS)
		{
			ResetMenus();
			currentState = STATE.STATE_OFF;
			Core.Logic.SetState(LogicStates.Playing);
			Core.Input.SetBlocker("INGAME_MENU", blocking: false);
			EventSystem.current.SetSelectedGameObject(null);
			Core.Audio.Ambient.StopCurrent();
			Core.Logic.LoadCreditsScene();
		}
	}

	public void Option_Achievement()
	{
		if (currentState == STATE.STATE_EXTRAS)
		{
			ShowMenu(MENU.ACHIEVEMENT);
		}
	}

	public void Option_MenuSkinSelector()
	{
		if (currentState == STATE.STATE_EXTRAS)
		{
			ShowMenu(MENU.SKINSELECTOR);
		}
	}

	public void Option_PatchNotes()
	{
		if (currentState == STATE.STATE_EXTRAS)
		{
			GoBack();
			UIController.instance.ShowPatchNotes();
		}
	}

	public void Option_ChooseBackground()
	{
		if (currentState == STATE.STATE_EXTRAS)
		{
			GoBack();
			StartCoroutine(UIController.instance.ShowMainMenuToChooseBackground());
		}
	}

	public void Option_MainMenu()
	{
		if (currentState == STATE.STATE_EXTRAS)
		{
			GoBack();
		}
	}

	private void ShowMenu(MENU menu)
	{
		currentMenu = menu;
		foreach (KeyValuePair<MENU, Transform> item in extrasRoot)
		{
			if (item.Value != null)
			{
				CanvasGroup component = item.Value.gameObject.GetComponent<CanvasGroup>();
				component.alpha = ((item.Key != currentMenu) ? 0f : 1f);
				component.interactable = item.Key == currentMenu;
			}
		}
		Transform transform = extrasRoot[currentMenu];
		if (transform == null)
		{
			return;
		}
		transform = transform.Find("Selection");
		if (transform == null && currentMenu != MENU.ACHIEVEMENT)
		{
			return;
		}
		lastHorizontalInOptions = 0f;
		switch (currentMenu)
		{
		case MENU.SKINSELECTOR:
		{
			foreach (SkinSelectorElement skinSelectorDataElement in skinSelectorDataElements)
			{
				SetOptionSkinSelected(skinSelectorDataElement.skinKey, selected: false);
			}
			currentSkin = Core.ColorPaletteManager.GetCurrentColorPaletteId();
			List<string> allUnlockedColorPalettesId = Core.ColorPaletteManager.GetAllUnlockedColorPalettesId();
			int i;
			for (i = 0; i < allSkins.Count; i++)
			{
				if (Core.ColorPaletteManager.GetColorPaletteById(allSkins[i]) == null)
				{
					Debug.LogError("Color palette " + allSkins[i] + " has no Sprite attached to it in the asset: Color Palettes/AVAILABLE_COLOR_PALETTES");
				}
				else if (allUnlockedColorPalettesId.Contains(allSkins[i]))
				{
					skinSelectorDataElements.Find((SkinSelectorElement x) => x.skinKey == allSkins[i]).element.SetActive(value: true);
					skinSelectorSelectionElements.Find((SkinSelectorElement x) => x.skinKey == allSkins[i]).element.SetActive(value: true);
					Material material = new Material(colorPaletteShader);
					material.SetTexture("_PaletteTex", Core.ColorPaletteManager.GetColorPaletteById(allSkins[i]).texture);
					material.EnableKeyword("PALETTE_ON");
					skinSelectorDataElements.Find((SkinSelectorElement x) => x.skinKey == allSkins[i]).element.GetComponentInChildren<Image>().material = material;
				}
				else
				{
					skinSelectorDataElements.Find((SkinSelectorElement x) => x.skinKey == allSkins[i]).element.SetActive(value: false);
					skinSelectorSelectionElements.Find((SkinSelectorElement x) => x.skinKey == allSkins[i]).element.SetActive(value: false);
				}
			}
			int num = allSkins.IndexOf(currentSkin);
			optionLastSelected = transform.GetChild(num).gameObject;
			for (int k = 0; k < transform.childCount; k++)
			{
				SetOptionSelected(transform.GetChild(k).gameObject, k == num);
			}
			optionLastSkinSelected = currentSkin;
			LinkAllButtons();
			ShowSkinSelectorValues();
			break;
		}
		case MENU.EXTRAS:
		{
			for (int j = 0; j < transform.childCount; j++)
			{
				SetOptionSelected(transform.GetChild(j).gameObject, j == 0);
			}
			if (Core.ColorPaletteManager.GetAllUnlockedColorPalettesId().Count > 1)
			{
				skinSelectorOption.transform.parent.gameObject.SetActive(value: true);
				LinkButtonVertical(skinSelectorOptionPrevious, skinSelectorOption);
				LinkButtonVertical(skinSelectorOption, skinSelectorOptionNext);
			}
			else
			{
				skinSelectorOption.transform.parent.gameObject.SetActive(value: false);
				LinkButtonVertical(skinSelectorOptionPrevious, skinSelectorOptionNext);
			}
			optionLastSelected = transform.GetChild(0).gameObject;
			break;
		}
		case MENU.ACHIEVEMENT:
			CreateAchievements();
			break;
		}
		EventSystem.current.SetSelectedGameObject(optionLastSelected.GetComponentInChildren<Text>(includeInactive: true).gameObject);
	}

	private void SetOptionSelected(GameObject option, bool selected)
	{
		option.GetComponentInChildren<Text>(includeInactive: true).color = ((!selected) ? extrasNormalColor : extrasHighligterColor);
		option.GetComponentInChildren<Image>(includeInactive: true).gameObject.SetActive(selected);
	}

	private void SetOptionSkinSelected(string option, bool selected)
	{
		skinSelectorDataElements.Find((SkinSelectorElement x) => x.skinKey == option).element.GetComponentInChildren<Text>(includeInactive: true).color = ((!selected) ? extrasNormalColor : extrasHighligterColor);
	}

	public void Option_AcceptSkinSelectorOptions()
	{
		Core.ColorPaletteManager.SetCurrentSkinToSkinSettings(currentSkin);
		GoBack();
	}

	private void UpdateInputSkinSelectorOptions()
	{
		currentSkin = optionLastSkinSelected;
		ShowSkinSelectorValues();
	}

	private void ShowSkinSelectorValues()
	{
		int i;
		for (i = 0; i < allSkins.Count; i++)
		{
			Image componentInChildren = skinSelectorDataElements.Find((SkinSelectorElement x) => x.skinKey == allSkins[i]).element.GetComponentInChildren<Image>();
			componentInChildren.enabled = currentSkin == allSkins[i];
			Text componentInChildren2 = skinSelectorSelectionElements.Find((SkinSelectorElement x) => x.skinKey == allSkins[i]).element.GetComponentInChildren<Text>();
			componentInChildren2.enabled = currentSkin == allSkins[i];
		}
	}

	private void ResetMenus()
	{
		foreach (KeyValuePair<MENU, Transform> item in extrasRoot)
		{
			if (item.Value != null)
			{
				item.Value.gameObject.SetActive(value: true);
				CanvasGroup component = item.Value.gameObject.GetComponent<CanvasGroup>();
				component.alpha = 0f;
				component.interactable = false;
			}
		}
	}

	private void LinkButtonHorizontal(EventsButton first, EventsButton second)
	{
		Navigation navigation = first.navigation;
		Navigation navigation2 = second.navigation;
		navigation.selectOnRight = second;
		navigation2.selectOnLeft = first;
		first.navigation = navigation;
		second.navigation = navigation2;
	}

	private void LinkButtonVertical(EventsButton first, EventsButton second)
	{
		Navigation navigation = first.navigation;
		Navigation navigation2 = second.navigation;
		navigation.selectOnDown = second;
		navigation2.selectOnUp = first;
		first.navigation = navigation;
		second.navigation = navigation2;
	}

	private void CreateAchievements()
	{
		foreach (Achievement allAchievement in Core.AchievementsManager.GetAllAchievements())
		{
			if (!AchivementsCache.ContainsKey(allAchievement.Id))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(AchievementElement, Vector3.zero, Quaternion.identity);
				gameObject.transform.SetParent(AchievementScrollView.scrollRect.content);
				RectTransform rectTransform = (RectTransform)gameObject.transform;
				rectTransform.localRotation = Quaternion.identity;
				rectTransform.localScale = Vector3.one;
				rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0f);
				gameObject.SetActive(value: true);
				AchivementsCache[allAchievement.Id] = gameObject;
			}
			AchivementsCache[allAchievement.Id].GetComponent<AchievementElementWidget>().SetData(allAchievement);
		}
		AchievementElement.SetActive(value: false);
		AchievementScrollView.NewContentSetted();
	}
}
