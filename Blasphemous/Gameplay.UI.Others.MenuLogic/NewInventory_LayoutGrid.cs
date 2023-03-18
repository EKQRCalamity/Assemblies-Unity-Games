using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DG.Tweening;
using Framework.Inventory;
using Framework.Managers;
using Gameplay.UI.Others.Buttons;
using Gameplay.UI.Others.UIGameLogic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class NewInventory_LayoutGrid : NewInventory_Layout
{
	[Serializable]
	private class TypeConfiguration
	{
		public GameObject rootLayout;

		public GameObject equipablesRoot;

		public int slots;
	}

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private GameObject gridElementBase;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private NewInventory_Description description;

	[BoxGroup("Controls", true, false, 0)]
	[SerializeField]
	private PlayerPurgePoints purgeControl;

	[SerializeField]
	[BoxGroup("Grid", true, false, 0)]
	private int numGridElements = 72;

	[SerializeField]
	[BoxGroup("Grid", true, false, 0)]
	private int colsInGrid = 8;

	[SerializeField]
	[BoxGroup("Grid", true, false, 0)]
	private int visibleRowsForScroll = 3;

	[SerializeField]
	[BoxGroup("Grid", true, false, 0)]
	private float cellHeightForScroll = 33f;

	[SerializeField]
	[BoxGroup("Grid", true, false, 0)]
	private ScrollRect scrollRect;

	[SerializeField]
	[BoxGroup("Grid", true, false, 0)]
	private FixedScrollBar scrollBar;

	[OdinSerialize]
	[BoxGroup("Type Configuration", true, false, 0)]
	private Dictionary<InventoryManager.ItemType, TypeConfiguration> typeConfiguration;

	[SerializeField]
	[BoxGroup("Sounds", true, false, 0)]
	private string soundElementChange = "event:/SFX/UI/ChangeSelection";

	[SerializeField]
	[BoxGroup("Sounds by type", true, false, 0)]
	private Dictionary<InventoryManager.ItemType, string> equipSounds = new Dictionary<InventoryManager.ItemType, string>
	{
		{
			InventoryManager.ItemType.Bead,
			"event:/SFX/UI/EquipBead"
		},
		{
			InventoryManager.ItemType.Prayer,
			"event:/SFX/UI/EquipPrayer"
		},
		{
			InventoryManager.ItemType.Relic,
			"event:/SFX/UI/EquipItem"
		},
		{
			InventoryManager.ItemType.Sword,
			"event:/SFX/UI/EquipItem"
		}
	};

	[SerializeField]
	[BoxGroup("Sounds by type", true, false, 0)]
	private Dictionary<InventoryManager.ItemType, string> unequipSounds = new Dictionary<InventoryManager.ItemType, string>
	{
		{
			InventoryManager.ItemType.Bead,
			"event:/SFX/UI/UnEquipBead"
		},
		{
			InventoryManager.ItemType.Prayer,
			"event:/SFX/UI/UnequipItem"
		},
		{
			InventoryManager.ItemType.Relic,
			"event:/SFX/UI/UnequipItem"
		},
		{
			InventoryManager.ItemType.Sword,
			"event:/SFX/UI/UnequipItem"
		}
	};

	[BoxGroup("Extras", true, false, 0)]
	public GameObject swordHeartText;

	[BoxGroup("Extras", true, false, 0)]
	public GameObject swordHeartTextLocked;

	private List<NewInventory_GridItem> cachedGridElements;

	private List<NewInventory_GridItem> cachedEquipped;

	private bool objectsCached;

	private int currentSelected = -1;

	private int currentSelectedEquiped = -1;

	private int availableEquipables = 9999;

	private InventoryManager.ItemType currentItemType;

	private bool ignoreSelectSound;

	private TypeConfiguration currentTypeConfiguration;

	private int currentViewScroll;

	private int currentMaxScrolls;

	private BaseInventoryObject pendingEquipableObject;

	private int pendingEquipableSlot;

	private bool WaitingToCloseNotUnequipable;

	private const int MAX_RESPONSES = 2;

	[BoxGroup("NotUnequipable", true, false, 0)]
	[SerializeField]
	private GameObject NotUnequipableRoot;

	[BoxGroup("NotUnequipable", true, false, 0)]
	[SerializeField]
	private Text[] dialogResponse = new Text[2];

	[BoxGroup("NotUnequipable", true, false, 0)]
	[SerializeField]
	private Color optionNormalColor = new Color(0.972549f, 76f / 85f, 0.78039217f);

	[BoxGroup("NotUnequipable", true, false, 0)]
	[SerializeField]
	private Color optionHighligterColor = new Color(0.80784315f, 72f / 85f, 0.49803922f);

	[BoxGroup("NotUnequipable", true, false, 0)]
	[SerializeField]
	private float NotUnequipableFadeTime = 0.3f;

	[BoxGroup("NotUnequipable", true, false, 0)]
	[SerializeField]
	private float NotUnequipableFadeResponseTime = 0.3f;

	[BoxGroup("NotUnequipable", true, false, 0)]
	[SerializeField]
	private float NotUnequipableEndFadeTime = 0.2f;

	[BoxGroup("NotUnequipable", true, false, 0)]
	[SerializeField]
	private float NotUnequipableWaitTime = 1.5f;

	private GameObject[] dialogResponseSelection = new GameObject[2];

	private bool InConfirmationNotUnequipable;

	private GameObject responsesUI;

	private void Awake()
	{
		ignoreSelectSound = false;
		if (objectsCached)
		{
			return;
		}
		cachedEquipped = new List<NewInventory_GridItem>();
		objectsCached = true;
		cachedGridElements = new List<NewInventory_GridItem>();
		if ((bool)gridElementBase && numGridElements > 0)
		{
			for (int i = 0; i < numGridElements; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(gridElementBase);
				gameObject.SetActive(value: true);
				gameObject.name = "Grid_Element" + i;
				gameObject.transform.SetParent(gridElementBase.transform.parent);
				gameObject.transform.localScale = Vector3.one;
				gameObject.transform.localPosition = Vector3.one;
				NewInventory_GridItem component = gameObject.GetComponent<NewInventory_GridItem>();
				component.SetObject(null);
				cachedGridElements.Add(component);
			}
		}
		if ((bool)NotUnequipableRoot)
		{
			NotUnequipableRoot.SetActive(value: false);
			for (int j = 0; j < 2; j++)
			{
				dialogResponseSelection[j] = dialogResponse[j].transform.Find("Img").gameObject;
				dialogResponseSelection[j].SetActive(value: false);
			}
			responsesUI = dialogResponse[0].transform.parent.gameObject;
			responsesUI.GetComponent<CanvasGroup>().alpha = 0f;
			responsesUI.SetActive(value: false);
		}
		InConfirmationNotUnequipable = false;
	}

	public override void RestoreFromLore()
	{
		StartCoroutine(FocusSlotSecure((currentSelected != -1) ? currentSelected : 0));
	}

	public override void RestoreSlotPosition(int slotPosition)
	{
		slotPosition = Mathf.Clamp(slotPosition, 0, cachedGridElements.Count - 1);
		if (cachedGridElements[slotPosition].inventoryObject == null)
		{
			slotPosition = 0;
		}
		StartCoroutine(FocusSlotSecure(slotPosition));
	}

	public override int GetLastSlotSelected()
	{
		currentSelected = Mathf.Clamp(currentSelected, 0, cachedGridElements.Count - 1);
		return (cachedGridElements[currentSelected].inventoryObject != null) ? currentSelected : 0;
	}

	public override void ShowLayout(NewInventoryWidget.TabType tabType, bool editMode)
	{
		currentSelectedEquiped = -1;
		currentViewScroll = 0;
		pendingEquipableObject = null;
		if (scrollRect != null)
		{
			scrollRect.verticalNormalizedPosition = 1f;
			scrollBar.SetScrollbar(0f);
		}
		int num = currentSelected;
		if (cachedGridElements != null && (num < 0 || num >= cachedGridElements.Count))
		{
			num = 0;
		}
		if ((bool)description)
		{
			description.SetObject(null, NewInventoryWidget.EquipAction.None);
		}
		if ((bool)purgeControl)
		{
			bool flag = !Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.BOSS_RUSH);
			purgeControl.transform.parent.gameObject.SetActive(flag);
			if (flag)
			{
				purgeControl.RefreshPoints(inmediate: true);
			}
		}
		switch (tabType)
		{
		case NewInventoryWidget.TabType.Collectables:
			FillGridElements(InventoryManager.ItemType.Collectible, Core.InventoryManager.GetCollectibleItemOwned());
			break;
		case NewInventoryWidget.TabType.Prayers:
			FillGridElements(InventoryManager.ItemType.Prayer, Core.InventoryManager.GetPrayersOwned());
			break;
		case NewInventoryWidget.TabType.Reliquary:
			FillGridElements(InventoryManager.ItemType.Relic, Core.InventoryManager.GetRelicsOwned());
			break;
		case NewInventoryWidget.TabType.Rosary:
			FillGridElements(InventoryManager.ItemType.Bead, Core.InventoryManager.GetRosaryBeadOwned());
			break;
		case NewInventoryWidget.TabType.Sword:
			FillGridElements(InventoryManager.ItemType.Sword, Core.InventoryManager.GetSwordsOwned());
			break;
		case NewInventoryWidget.TabType.Quest:
			FillGridElements(InventoryManager.ItemType.Quest, Core.InventoryManager.GetQuestItemOwned());
			break;
		}
		CacheEquipped();
		UpdateItemsBySwordHeartEquipability(tabType);
		if (scrollBar != null)
		{
			scrollBar.gameObject.SetActive(currentMaxScrolls > 1);
		}
		if ((bool)NotUnequipableRoot)
		{
			NotUnequipableRoot.SetActive(value: false);
		}
		StartCoroutine(FocusSlotSecure(num, ignoreSound: false));
		UpdateEquipped(currentItemType);
		InConfirmationNotUnequipable = false;
	}

	private void UpdateItemsBySwordHeartEquipability(NewInventoryWidget.TabType tabType)
	{
		bool allowEquipSwords = Core.Logic.Penitent.AllowEquipSwords;
		bool flag = tabType == NewInventoryWidget.TabType.Sword;
		bool flag2 = !flag || (UIController.instance.CanEquipSwordHearts && (allowEquipSwords || Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.BOSS_RUSH)));
		if (swordHeartText != null)
		{
			swordHeartText.SetActive(flag && !UIController.instance.CanEquipSwordHearts && allowEquipSwords);
		}
		if (swordHeartTextLocked != null)
		{
			swordHeartTextLocked.SetActive(flag && !allowEquipSwords);
		}
		foreach (NewInventory_GridItem cachedGridElement in cachedGridElements)
		{
			if (flag2)
			{
				cachedGridElement.DeactivateGrayscale();
			}
			else
			{
				cachedGridElement.ActivateGrayscale();
			}
		}
	}

	public override void GetSelectedLoreData(out string caption, out string lore)
	{
		BaseInventoryObject baseInventoryObject = null;
		if (currentSelected >= 0 && cachedGridElements[currentSelected] != null)
		{
			baseInventoryObject = cachedGridElements[currentSelected].inventoryObject;
		}
		if (baseInventoryObject != null && baseInventoryObject.HasLore())
		{
			caption = baseInventoryObject.caption;
			lore = baseInventoryObject.lore;
		}
		else
		{
			caption = string.Empty;
			lore = string.Empty;
		}
	}

	public override bool CanGoBack()
	{
		bool flag = !InConfirmationNotUnequipable;
		if (!flag)
		{
			HideNotUnequipableDialog();
		}
		return flag;
	}

	public override bool CanLore()
	{
		return !InConfirmationNotUnequipable;
	}

	public override int GetItemPosition(BaseInventoryObject item)
	{
		for (int i = 0; i < cachedGridElements.Count; i++)
		{
			if (cachedGridElements[i].inventoryObject == item)
			{
				return i;
			}
		}
		return GetLastSlotSelected();
	}

	public void ActivateGridElement(int slot)
	{
		if (InConfirmationNotUnequipable)
		{
			return;
		}
		BaseInventoryObject inventoryObject = cachedGridElements[slot].inventoryObject;
		if (!inventoryObject)
		{
			return;
		}
		switch (GetInventoryObjectAction(inventoryObject))
		{
		case NewInventoryWidget.EquipAction.Equip:
			if (inventoryObject.WillBlockSwords() && !Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.BOSS_RUSH))
			{
				pendingEquipableObject = inventoryObject;
				pendingEquipableSlot = slot;
				ShowNotUnequipableDialog();
			}
			else
			{
				EquipObject(inventoryObject);
			}
			break;
		case NewInventoryWidget.EquipAction.UnEquip:
			UnEquipObject(inventoryObject);
			break;
		}
		UpdateEquipped(currentItemType);
		SelectGridElement(slot, playSound: false);
	}

	public void SelectGridElement(int slot, bool playSound = true)
	{
		if (InConfirmationNotUnequipable)
		{
			return;
		}
		if (currentSelected != -1 && currentSelected < cachedGridElements.Count)
		{
			cachedGridElements[currentSelected].UpdateSelect(selected: false);
		}
		currentSelected = slot;
		if ((bool)scrollRect && currentMaxScrolls > 1)
		{
			CheckScroll();
		}
		BaseInventoryObject inventoryObject = cachedGridElements[slot].inventoryObject;
		if ((bool)description)
		{
			NewInventoryWidget.EquipAction inventoryObjectAction = GetInventoryObjectAction(inventoryObject);
			description.SetObject(inventoryObject, inventoryObjectAction);
		}
		if (playSound)
		{
			if (ignoreSelectSound)
			{
				ignoreSelectSound = false;
			}
			else
			{
				Core.Audio.PlayOneShot(soundElementChange);
			}
		}
		cachedGridElements[slot].UpdateStatus(p_enabled: true, p_selected: true, IsEquipped(inventoryObject));
		CleanSelectedEquipped();
		if ((bool)inventoryObject)
		{
			UpdateEquipedSelected(inventoryObject.id);
		}
	}

	public void NoEquipableOptionSelected(int response)
	{
		if (!WaitingToCloseNotUnequipable)
		{
			for (int i = 0; i < 2; i++)
			{
				dialogResponseSelection[i].SetActive(i == response);
				Text componentInChildren = dialogResponseSelection[i].transform.parent.GetComponentInChildren<Text>();
				componentInChildren.color = ((i != response) ? optionNormalColor : optionHighligterColor);
			}
		}
	}

	public void NoEquipableResponsePressed(int response)
	{
		if (!WaitingToCloseNotUnequipable)
		{
			if (response == 0)
			{
				EquipObject(pendingEquipableObject);
				UpdateEquipped(currentItemType);
				UpdateItemsBySwordHeartEquipability(NewInventoryWidget.TabType.Sword);
			}
			HideNotUnequipableDialog();
		}
	}

	private void CheckScroll()
	{
		int num = Mathf.FloorToInt((float)currentSelected / (float)colsInGrid) + 1;
		int num2 = Mathf.CeilToInt((float)num / (float)visibleRowsForScroll);
		if (currentViewScroll != num2)
		{
			currentViewScroll = num2;
			float scrollbar = (currentViewScroll - 1) / (currentMaxScrolls - 1);
			scrollBar.SetScrollbar(scrollbar);
			float y = (float)((currentViewScroll - 1) * visibleRowsForScroll) * cellHeightForScroll;
			scrollRect.content.transform.localPosition = new Vector3(scrollRect.content.transform.localPosition.x, y, scrollRect.content.transform.localPosition.z);
		}
	}

	private void CleanSelectedEquipped()
	{
		if (currentSelectedEquiped != -1 && currentSelectedEquiped < cachedEquipped.Count)
		{
			cachedEquipped[currentSelectedEquiped].UpdateStatus(p_enabled: true, p_selected: false, p_equiped: false);
			currentSelectedEquiped = -1;
		}
	}

	private void UpdateItemById(string id)
	{
		for (int i = 0; i < cachedGridElements.Count; i++)
		{
			NewInventory_GridItem newInventory_GridItem = cachedGridElements[i];
			if ((bool)newInventory_GridItem.inventoryObject && newInventory_GridItem.inventoryObject.id == id)
			{
				newInventory_GridItem.UpdateStatus(p_enabled: true, p_selected: false, p_equiped: false);
			}
		}
	}

	private void UpdateEquipedSelected(string id)
	{
		for (int i = 0; i < cachedEquipped.Count; i++)
		{
			NewInventory_GridItem newInventory_GridItem = cachedEquipped[i];
			if ((bool)newInventory_GridItem.inventoryObject && newInventory_GridItem.inventoryObject.id == id)
			{
				newInventory_GridItem.UpdateStatus(p_enabled: true, p_selected: true, p_equiped: true);
				currentSelectedEquiped = i;
			}
		}
	}

	private void UpdateEquipped(InventoryManager.ItemType itemType)
	{
		for (int i = 0; i < cachedEquipped.Count; i++)
		{
			NewInventory_GridItem newInventory_GridItem = cachedEquipped[i];
			BaseInventoryObject @object = null;
			switch (itemType)
			{
			case InventoryManager.ItemType.Bead:
				@object = Core.InventoryManager.GetRosaryBeadInSlot(i);
				break;
			case InventoryManager.ItemType.Prayer:
				@object = Core.InventoryManager.GetPrayerInSlot(i);
				break;
			case InventoryManager.ItemType.Relic:
				@object = Core.InventoryManager.GetRelicInSlot(i);
				break;
			case InventoryManager.ItemType.Sword:
				@object = Core.InventoryManager.GetSwordInSlot(i);
				break;
			}
			newInventory_GridItem.SetObject(@object);
			newInventory_GridItem.UpdateStatus(i < availableEquipables, currentSelectedEquiped == i, currentSelectedEquiped == i);
		}
	}

	private void ShowMaxSlotsForCurrentTabType()
	{
		if (currentTypeConfiguration.slots > 0)
		{
			for (int i = 0; i < cachedGridElements.Count; i++)
			{
				NewInventory_GridItem newInventory_GridItem = cachedGridElements[i];
				bool active = i < currentTypeConfiguration.slots;
				newInventory_GridItem.gameObject.SetActive(active);
			}
		}
	}

	private void FillGridElements<T>(InventoryManager.ItemType type, ReadOnlyCollection<T> list) where T : BaseInventoryObject
	{
		currentItemType = type;
		currentTypeConfiguration = typeConfiguration[currentItemType];
		ShowMaxSlotsForCurrentTabType();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = numGridElements / colsInGrid;
		int[,] array = new int[colsInGrid, num4];
		for (int i = 0; i < cachedGridElements.Count; i++)
		{
			NewInventory_GridItem newInventory_GridItem = cachedGridElements[i];
			T val = (T)null;
			if (num < list.Count)
			{
				val = list[num];
				num++;
			}
			array[num3, num2] = i;
			int elementNumber = i;
			EventsButton button = newInventory_GridItem.Button;
			button.onClick = new EventsButton.ButtonClickedEvent();
			button.onClick.AddListener(delegate
			{
				ActivateGridElement(elementNumber);
			});
			button.onSelected = new EventsButton.ButtonSelectedEvent();
			button.onSelected.AddListener(delegate
			{
				SelectGridElement(elementNumber);
			});
			newInventory_GridItem.Button.interactable = true;
			newInventory_GridItem.SetObject(val);
			newInventory_GridItem.UpdateStatus(base.enabled, p_selected: false, IsEquipped(val));
			num3++;
			if (num3 >= colsInGrid)
			{
				num3 = 0;
				num2++;
			}
		}
		int num5 = Mathf.CeilToInt((float)list.Count / (float)colsInGrid);
		currentMaxScrolls = Mathf.CeilToInt((float)num5 / (float)visibleRowsForScroll);
		if (currentMaxScrolls == 0)
		{
			currentMaxScrolls = 1;
		}
		int num6 = currentMaxScrolls * visibleRowsForScroll;
		for (num3 = 0; num3 < colsInGrid; num3++)
		{
			for (num2 = 0; num2 < num6; num2++)
			{
				int slot = array[num3, num2];
				LinkNavigation(slot, array, num3, num2, num6);
			}
		}
		if (type != InventoryManager.ItemType.Quest)
		{
			LinkLastSlotToLastRowFirstSlot();
		}
	}

	private void LinkLastSlotToLastRowFirstSlot()
	{
		int slots = currentTypeConfiguration.slots;
		int num = colsInGrid * (slots / colsInGrid);
		Navigation navigation = cachedGridElements[num].Button.navigation;
		int num2 = slots - 1;
		Navigation navigation2 = cachedGridElements[num2].Button.navigation;
		navigation.selectOnLeft = GetActiveButton(num2);
		navigation2.selectOnRight = GetActiveButton(num);
		cachedGridElements[num].Button.navigation = navigation;
		cachedGridElements[num2].Button.navigation = navigation2;
	}

	private void LinkNavigation(int slot, int[,] indexgrid, int col, int row, int maxRow)
	{
		Navigation navigation = cachedGridElements[slot].Button.navigation;
		int num = col - 1;
		int num2 = row;
		if (num < 0)
		{
			num = colsInGrid - 1;
		}
		int slot2 = indexgrid[num, num2];
		navigation.selectOnLeft = GetActiveButton(slot2);
		num = col + 1;
		if (num >= colsInGrid)
		{
			num = 0;
		}
		slot2 = indexgrid[num, num2];
		navigation.selectOnRight = GetActiveButton(slot2);
		num = col;
		num2 = row - 1;
		if (num2 < 0)
		{
			num2 = maxRow - 1;
		}
		slot2 = indexgrid[num, num2];
		navigation.selectOnUp = GetActiveButton(slot2);
		num2 = row + 1;
		if (num2 >= maxRow)
		{
			num2 = 0;
		}
		slot2 = indexgrid[num, num2];
		navigation.selectOnDown = GetActiveButton(slot2);
		cachedGridElements[slot].Button.navigation = navigation;
	}

	private EventsButton GetActiveButton(int slot)
	{
		EventsButton eventsButton = cachedGridElements[slot].Button;
		if (!eventsButton.interactable)
		{
			eventsButton = null;
		}
		return eventsButton;
	}

	private void CacheEquipped()
	{
		cachedEquipped.Clear();
		if (typeConfiguration == null)
		{
			return;
		}
		availableEquipables = ((currentItemType != InventoryManager.ItemType.Bead) ? 9999 : Core.InventoryManager.GetRosaryBeadSlots());
		foreach (KeyValuePair<InventoryManager.ItemType, TypeConfiguration> item in typeConfiguration)
		{
			bool flag = item.Key == currentItemType;
			if ((bool)item.Value.rootLayout)
			{
				item.Value.rootLayout.SetActive(flag);
			}
			if (!flag || !item.Value.equipablesRoot)
			{
				continue;
			}
			for (int i = 0; i < item.Value.equipablesRoot.transform.childCount; i++)
			{
				Transform child = item.Value.equipablesRoot.transform.GetChild(i);
				NewInventory_GridItem component = child.GetComponent<NewInventory_GridItem>();
				if ((bool)component)
				{
					cachedEquipped.Add(component);
				}
			}
		}
	}

	private IEnumerator FocusSlotSecure(int slot, bool ignoreSound = true)
	{
		slot = Mathf.Clamp(slot, 0, cachedGridElements.Count - 1);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		int maxSlot = currentMaxScrolls * visibleRowsForScroll * colsInGrid;
		if (slot >= maxSlot)
		{
			int num = Mathf.FloorToInt((float)slot / (float)colsInGrid) + 1;
			int num2 = Mathf.CeilToInt((float)num / (float)visibleRowsForScroll);
			slot -= (num2 - 1) * visibleRowsForScroll * colsInGrid;
		}
		GameObject focusElement = cachedGridElements[slot].Button.gameObject;
		ignoreSelectSound = ignoreSound;
		EventSystem.current.SetSelectedGameObject(focusElement);
		SelectGridElement(slot, playSound: false);
	}

	private void ShowNotUnequipableDialog()
	{
		InConfirmationNotUnequipable = true;
		WaitingToCloseNotUnequipable = false;
		NotUnequipableRoot.SetActive(value: true);
		responsesUI.SetActive(value: true);
		CanvasGroup component = responsesUI.GetComponent<CanvasGroup>();
		component.alpha = 0f;
		CanvasGroup component2 = NotUnequipableRoot.GetComponent<CanvasGroup>();
		component2.alpha = 0f;
		StartCoroutine(ShowFirstSecure());
		DOTween.defaultTimeScaleIndependent = true;
		DOTween.Sequence().Append(component2.DOFade(1f, NotUnequipableFadeTime)).AppendInterval(NotUnequipableWaitTime)
			.Append(component.DOFade(1f, NotUnequipableFadeResponseTime))
			.Play();
	}

	private void HideNotUnequipableDialog()
	{
		WaitingToCloseNotUnequipable = true;
		CanvasGroup component = NotUnequipableRoot.GetComponent<CanvasGroup>();
		DOTween.defaultTimeScaleIndependent = true;
		component.DOFade(0f, NotUnequipableEndFadeTime).OnComplete(delegate
		{
			NotUnequipableRoot.SetActive(value: false);
			InConfirmationNotUnequipable = false;
			WaitingToCloseNotUnequipable = false;
			StartCoroutine(FocusSlotSecure(pendingEquipableSlot));
		});
	}

	public IEnumerator ShowFirstSecure()
	{
		yield return new WaitForFixedUpdate();
		EventSystem.current.SetSelectedGameObject(dialogResponse[1].gameObject);
		yield return new WaitForFixedUpdate();
		EventSystem.current.SetSelectedGameObject(dialogResponse[0].gameObject);
		NoEquipableOptionSelected(0);
	}

	private void EquipObject(BaseInventoryObject obj)
	{
		if (currentItemType == InventoryManager.ItemType.Prayer || currentItemType == InventoryManager.ItemType.Sword)
		{
			switch (currentItemType)
			{
			case InventoryManager.ItemType.Prayer:
			{
				Prayer prayerInSlot = Core.InventoryManager.GetPrayerInSlot(0);
				if (prayerInSlot != null)
				{
					UnEquipObject(prayerInSlot);
					UpdateItemById(prayerInSlot.id);
				}
				Core.InventoryManager.SetPrayerInSlot(0, obj.id);
				break;
			}
			case InventoryManager.ItemType.Sword:
			{
				Sword swordInSlot = Core.InventoryManager.GetSwordInSlot(0);
				if (swordInSlot != null)
				{
					UnEquipObject(swordInSlot);
					UpdateItemById(swordInSlot.id);
				}
				Core.InventoryManager.SetSwordInSlot(0, obj.id);
				break;
			}
			}
		}
		else
		{
			int firstEmptySlot = GetFirstEmptySlot();
			if (firstEmptySlot < 0)
			{
				return;
			}
			switch (currentItemType)
			{
			case InventoryManager.ItemType.Bead:
				Core.InventoryManager.SetRosaryBeadInSlot(firstEmptySlot, obj.id);
				break;
			case InventoryManager.ItemType.Relic:
				Core.InventoryManager.SetRelicInSlot(firstEmptySlot, obj.id);
				break;
			}
		}
		if (equipSounds.ContainsKey(currentItemType))
		{
			string text = equipSounds[currentItemType];
			if (text != string.Empty)
			{
				Core.Audio.PlayOneShot(text);
			}
		}
	}

	private void UnEquipObject(BaseInventoryObject obj)
	{
		if (InConfirmationNotUnequipable)
		{
			return;
		}
		int num = -1;
		switch (currentItemType)
		{
		case InventoryManager.ItemType.Bead:
			num = Core.InventoryManager.GetRosaryBeadSlot((RosaryBead)obj);
			Core.InventoryManager.SetRosaryBeadInSlot(num, (RosaryBead)null);
			break;
		case InventoryManager.ItemType.Relic:
			num = Core.InventoryManager.GetRelicSlot((Relic)obj);
			Core.InventoryManager.SetRelicInSlot(num, (Relic)null);
			break;
		case InventoryManager.ItemType.Prayer:
			num = Core.InventoryManager.GetPrayerInSlot((Prayer)obj);
			Core.InventoryManager.SetPrayerInSlot(num, (Prayer)null);
			break;
		case InventoryManager.ItemType.Sword:
			num = Core.InventoryManager.GetSwordInSlot((Sword)obj);
			Core.InventoryManager.SetSwordInSlot(num, (Sword)null);
			break;
		}
		if (unequipSounds.ContainsKey(currentItemType))
		{
			string text = unequipSounds[currentItemType];
			if (text != string.Empty)
			{
				Core.Audio.PlayOneShot(text);
			}
		}
	}

	private NewInventoryWidget.EquipAction GetInventoryObjectAction(BaseInventoryObject obj)
	{
		NewInventoryWidget.EquipAction result = NewInventoryWidget.EquipAction.None;
		if ((bool)obj)
		{
			if (IsEquipped(obj))
			{
				result = NewInventoryWidget.EquipAction.UnEquip;
			}
			else if (currentItemType == InventoryManager.ItemType.Prayer || currentItemType == InventoryManager.ItemType.Sword)
			{
				result = NewInventoryWidget.EquipAction.Equip;
			}
			else
			{
				int firstEmptySlot = GetFirstEmptySlot();
				if (firstEmptySlot != -1)
				{
					result = NewInventoryWidget.EquipAction.Equip;
				}
			}
		}
		if (obj is Sword && (!UIController.instance.CanEquipSwordHearts || (!Core.Logic.Penitent.AllowEquipSwords && !Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.BOSS_RUSH))))
		{
			result = NewInventoryWidget.EquipAction.None;
		}
		return result;
	}

	private bool IsEquipped(BaseInventoryObject obj)
	{
		bool result = false;
		if ((bool)obj)
		{
			switch (currentItemType)
			{
			case InventoryManager.ItemType.Prayer:
				result = Core.InventoryManager.IsPrayerEquipped(obj.id);
				break;
			case InventoryManager.ItemType.Bead:
				result = Core.InventoryManager.IsRosaryBeadEquipped(obj.id);
				break;
			case InventoryManager.ItemType.Relic:
				result = Core.InventoryManager.IsRelicEquipped(obj.id);
				break;
			case InventoryManager.ItemType.Sword:
				result = Core.InventoryManager.IsSwordEquipped(obj.id);
				break;
			}
		}
		return result;
	}

	private int GetFirstEmptySlot()
	{
		int result = -1;
		int num = 0;
		switch (currentItemType)
		{
		case InventoryManager.ItemType.Bead:
			num = Core.InventoryManager.GetRosaryBeadSlots();
			break;
		case InventoryManager.ItemType.Relic:
			num = 3;
			break;
		case InventoryManager.ItemType.Prayer:
			num = 1;
			break;
		case InventoryManager.ItemType.Sword:
			num = 1;
			break;
		}
		for (int i = 0; i < num; i++)
		{
			BaseInventoryObject baseInventoryObject = null;
			switch (currentItemType)
			{
			case InventoryManager.ItemType.Bead:
				baseInventoryObject = Core.InventoryManager.GetRosaryBeadInSlot(i);
				break;
			case InventoryManager.ItemType.Relic:
				baseInventoryObject = Core.InventoryManager.GetRelicInSlot(i);
				break;
			case InventoryManager.ItemType.Prayer:
				baseInventoryObject = Core.InventoryManager.GetPrayerInSlot(i);
				break;
			case InventoryManager.ItemType.Sword:
				baseInventoryObject = Core.InventoryManager.GetSwordInSlot(i);
				break;
			}
			if (!baseInventoryObject)
			{
				result = i;
				break;
			}
		}
		return result;
	}
}
