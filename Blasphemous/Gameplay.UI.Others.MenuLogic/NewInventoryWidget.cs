using System;
using System.Collections.Generic;
using Framework.Managers;
using Gameplay.UI.Widgets;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class NewInventoryWidget : SerializedMonoBehaviour
{
	public enum TabType
	{
		Rosary,
		Reliquary,
		Quest,
		Sword,
		Prayers,
		Abilities,
		Collectables
	}

	private enum MenuState
	{
		OFF,
		Normal,
		UnlockSkills,
		Lore
	}

	public enum EquipAction
	{
		None,
		Equip,
		UnEquip,
		Decipher
	}

	[Serializable]
	private class TypeConfig
	{
		public NewInventory_Layout layout;

		public string caption = string.Empty;

		[NonSerialized]
		public GameObject cachedButtonOn;

		[NonSerialized]
		public GameObject cachedButtonOff;
	}

	public delegate void TeleportCancelDelegate();

	private const string ANIMATOR_STATE = "STATE";

	[SerializeField]
	[BoxGroup("Common", true, false, 0)]
	private GameObject normalHeader;

	[SerializeField]
	[BoxGroup("Common", true, false, 0)]
	private GameObject unlockHeader;

	[SerializeField]
	[BoxGroup("Tabs", true, false, 0)]
	private Text tabLabel;

	[SerializeField]
	[BoxGroup("Tabs", true, false, 0)]
	private GameObject tabParent;

	[SerializeField]
	[BoxGroup("Tabs", true, false, 0)]
	private string tabPrefix = "Tab_";

	[SerializeField]
	[BoxGroup("Lore", true, false, 0)]
	private Text loreCaption;

	[SerializeField]
	[BoxGroup("Lore", true, false, 0)]
	private Text loreDescription;

	[SerializeField]
	[BoxGroup("Lore", true, false, 0)]
	private CustomScrollView loreScroll;

	[SerializeField]
	[BoxGroup("Sounds", true, false, 0)]
	private string soundChangeTab = "event:/SFX/UI/ChangeTab";

	[SerializeField]
	[BoxGroup("Sounds", true, false, 0)]
	private string soundLore = "event:/SFX/UI/ChangeTab";

	[SerializeField]
	[BoxGroup("Sounds", true, false, 0)]
	private string soundBack = "event:/SFX/UI/ChangeTab";

	[OdinSerialize]
	private Dictionary<TabType, TypeConfig> TypeConfiguration;

	private List<TabType> tabOrder = new List<TabType>
	{
		TabType.Rosary,
		TabType.Reliquary,
		TabType.Quest,
		TabType.Sword,
		TabType.Prayers,
		TabType.Abilities,
		TabType.Collectables
	};

	private MenuState currentMenuState;

	private MenuState previoudMenuState;

	private TabType currentTabType;

	private TabType lastOpenTabType;

	private int lastSlot;

	private NewInventory_Layout currentLayout;

	private Animator animator;

	public bool currentlyActive => currentMenuState != MenuState.OFF;

	public static event TeleportCancelDelegate OnTeleportCancelled;

	private void Awake()
	{
		currentMenuState = MenuState.OFF;
		animator = GetComponent<Animator>();
		foreach (KeyValuePair<TabType, TypeConfig> item in TypeConfiguration)
		{
			if (item.Value.layout != null)
			{
				item.Value.layout.enabled = true;
				item.Value.layout.gameObject.SetActive(value: true);
			}
		}
		int num = 1;
		foreach (TabType value in Enum.GetValues(typeof(TabType)))
		{
			string text = tabPrefix + num;
			Transform transform = tabParent.transform.Find(text);
			TypeConfiguration[value].cachedButtonOn = transform.Find("ButtonOn").gameObject;
			TypeConfiguration[value].cachedButtonOff = transform.Find("ButtonOff").gameObject;
			num++;
		}
	}

	public void Show(bool p_active)
	{
		if (p_active && IsGUILocked())
		{
			return;
		}
		Core.Input.SetBlocker("INVENTORY", p_active);
		if (p_active)
		{
			SetState(MenuState.Normal);
			TabType tabType = lastOpenTabType;
			bool anyLastUsedObjectUntilLastCalled = Core.InventoryManager.AnyLastUsedObjectUntilLastCalled;
			if (anyLastUsedObjectUntilLastCalled)
			{
				switch (Core.InventoryManager.LastAddedObjectType)
				{
				case InventoryManager.ItemType.Bead:
					tabType = TabType.Rosary;
					break;
				case InventoryManager.ItemType.Collectible:
					tabType = TabType.Collectables;
					break;
				case InventoryManager.ItemType.Prayer:
					tabType = TabType.Prayers;
					break;
				case InventoryManager.ItemType.Quest:
					tabType = TabType.Quest;
					break;
				case InventoryManager.ItemType.Relic:
					tabType = TabType.Reliquary;
					break;
				case InventoryManager.ItemType.Sword:
					tabType = TabType.Sword;
					break;
				}
			}
			SelectTab(tabType, playSound: false);
			Core.Logic.PauseGame();
			int slotPosition = currentLayout.GetLastSlotSelected();
			if (anyLastUsedObjectUntilLastCalled)
			{
				slotPosition = currentLayout.GetItemPosition(Core.InventoryManager.LastAddedObject);
			}
			currentLayout.RestoreSlotPosition(slotPosition);
		}
		else
		{
			if (currentLayout != null)
			{
				lastSlot = currentLayout.GetLastSlotSelected();
			}
			if (currentLayout != null && currentLayout is NewInventory_LayoutSkill)
			{
				(currentLayout as NewInventory_LayoutSkill).CancelEditMode();
			}
			SetState(MenuState.OFF);
			lastOpenTabType = currentTabType;
			EventSystem.current.SetSelectedGameObject(null);
			Core.Logic.ResumeGame();
		}
	}

	public void ShowSkills(bool p_active)
	{
		if (!p_active || !IsGUILocked())
		{
			Core.Input.SetBlocker("INVENTORY", p_active);
			if (p_active)
			{
				SetState(MenuState.UnlockSkills);
				SelectTab(TabType.Abilities, playSound: false);
				Core.Logic.PauseGame();
			}
			else
			{
				EventSystem.current.SetSelectedGameObject(null);
				SetState(MenuState.OFF);
				Core.Logic.ResumeGame();
			}
		}
	}

	public void GoBack()
	{
		bool flag = true;
		switch (currentMenuState)
		{
		case MenuState.Normal:
		case MenuState.UnlockSkills:
			if (currentLayout.CanGoBack())
			{
				Show(p_active: false);
			}
			else
			{
				flag = false;
			}
			break;
		case MenuState.Lore:
			SetState(previoudMenuState);
			currentLayout.RestoreFromLore();
			break;
		}
		if (flag && soundBack != string.Empty)
		{
			Core.Audio.PlayOneShot(soundBack);
		}
	}

	public void SetTab(TabType tab)
	{
		lastOpenTabType = tab;
	}

	public void ShowLore()
	{
		if ((currentMenuState != MenuState.Normal && currentMenuState != MenuState.UnlockSkills) || !(currentLayout != null) || !currentLayout.CanLore())
		{
			return;
		}
		string lore = string.Empty;
		string caption = string.Empty;
		currentLayout.GetSelectedLoreData(out caption, out lore);
		if (lore != string.Empty)
		{
			if (soundLore != string.Empty)
			{
				Core.Audio.PlayOneShot(soundLore);
			}
			Core.Metrics.CustomEvent("LORE_BUTTON_PRESSED", string.Empty);
			loreCaption.text = caption;
			loreDescription.text = lore;
			loreScroll.NewContentSetted();
			EventSystem.current.SetSelectedGameObject(null);
			SetState(MenuState.Lore);
		}
	}

	public void SelectNextCategory()
	{
		if (currentMenuState == MenuState.Normal)
		{
			int num = tabOrder.FindIndex((TabType x) => x == currentTabType);
			TabType tabType = tabOrder[(num + 1) % tabOrder.Count];
			lastSlot = 0;
			SelectTab(tabType);
		}
	}

	public void SelectPreviousCategory()
	{
		if (currentMenuState == MenuState.Normal)
		{
			int num = tabOrder.FindIndex((TabType x) => x == currentTabType);
			int num2 = num - 1;
			if (num2 < 0)
			{
				num2 = tabOrder.Count - 1;
			}
			TabType tabType = tabOrder[num2];
			lastSlot = 0;
			SelectTab(tabType);
		}
	}

	private void SetState(MenuState state)
	{
		previoudMenuState = currentMenuState;
		currentMenuState = state;
		animator.SetInteger("STATE", (int)currentMenuState);
		normalHeader.SetActive(currentMenuState == MenuState.Normal || currentMenuState == MenuState.Lore);
		unlockHeader.SetActive(currentMenuState == MenuState.UnlockSkills);
	}

	private void SelectTab(TabType tabType, bool playSound = true)
	{
		foreach (KeyValuePair<TabType, TypeConfig> item in TypeConfiguration)
		{
			if (item.Value.layout != null)
			{
				item.Value.layout.gameObject.SetActive(value: false);
			}
		}
		currentTabType = tabType;
		TypeConfig typeConfig = TypeConfiguration[currentTabType];
		currentLayout = typeConfig.layout;
		currentLayout.gameObject.SetActive(value: true);
		currentLayout.ShowLayout(tabType, currentMenuState == MenuState.UnlockSkills);
		if ((bool)tabLabel && typeConfig.caption != null)
		{
			tabLabel.text = Core.Localization.Get(typeConfig.caption);
		}
		foreach (TabType value in Enum.GetValues(typeof(TabType)))
		{
			bool flag = value == currentTabType;
			TypeConfiguration[value].cachedButtonOn.SetActive(flag);
			TypeConfiguration[value].cachedButtonOff.SetActive(!flag);
		}
		if (playSound)
		{
			Core.Audio.PlayOneShot(soundChangeTab);
		}
		currentLayout.RestoreSlotPosition(lastSlot);
	}

	private bool IsGUILocked()
	{
		return (Core.Input.InputBlocked && !Core.Input.HasBlocker("INTERACTABLE") && !Core.Input.HasBlocker("PLAYER_LOGIC")) || SceneManager.GetActiveScene().name == "MainMenu" || FadeWidget.instance.Fading || UIController.instance.Paused;
	}
}
