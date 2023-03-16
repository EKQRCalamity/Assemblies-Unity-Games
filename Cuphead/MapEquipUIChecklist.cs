using System.Collections.Generic;
using UnityEngine;

public class MapEquipUIChecklist : AbstractMapEquipUICardSide
{
	private readonly string[] worldPaths = new string[4] { "equip_checklist_world_1", "equip_checklist_world_2", "equip_checklist_world_3", "equip_checklist_finale" };

	private readonly Levels[] world1Levels = new Levels[7]
	{
		Levels.Veggies,
		Levels.Slime,
		Levels.FlyingBlimp,
		Levels.Flower,
		Levels.Frogs,
		Levels.Platforming_Level_1_1,
		Levels.Platforming_Level_1_2
	};

	private readonly Levels[] world2Levels = new Levels[7]
	{
		Levels.Baroness,
		Levels.Clown,
		Levels.FlyingGenie,
		Levels.Dragon,
		Levels.FlyingBird,
		Levels.Platforming_Level_2_1,
		Levels.Platforming_Level_2_2
	};

	private readonly Levels[] world3Levels = new Levels[9]
	{
		Levels.Bee,
		Levels.Pirate,
		Levels.SallyStagePlay,
		Levels.Mouse,
		Levels.Robot,
		Levels.FlyingMermaid,
		Levels.Train,
		Levels.Platforming_Level_3_1,
		Levels.Platforming_Level_3_2
	};

	private readonly Levels[] finaleLevels = new Levels[11]
	{
		Levels.DicePalaceBooze,
		Levels.DicePalaceChips,
		Levels.DicePalaceCigar,
		Levels.DicePalaceDomino,
		Levels.DicePalaceEightBall,
		Levels.DicePalaceFlyingHorse,
		Levels.DicePalaceFlyingMemory,
		Levels.DicePalaceRabbit,
		Levels.DicePalaceRoulette,
		Levels.DicePalaceMain,
		Levels.Devil
	};

	private readonly Levels[] DLClevels = new Levels[6]
	{
		Levels.OldMan,
		Levels.RumRunners,
		Levels.Airplane,
		Levels.SnowCult,
		Levels.FlyingCowboy,
		Levels.Saltbaker
	};

	private readonly string[] worldNames = new string[5] { "CheckListWorld1", "CheckListWorld2", "CheckListWorld3", "CheckListFinale", "ChecklistDLC" };

	[Header("Headers")]
	[SerializeField]
	private GameObject worldTop;

	[SerializeField]
	private GameObject finaleTop;

	[SerializeField]
	private GameObject localizedTop;

	[SerializeField]
	private GameObject worldTopLocalized;

	[SerializeField]
	private GameObject worldTopDLCLocalized;

	[SerializeField]
	private GameObject finaleTopLocalized;

	[Header("Cursors")]
	[SerializeField]
	private MapEquipUICursor cursor;

	[Header("Icons")]
	[SerializeField]
	private MapEquipUICardChecklistIcon[] worldSelectionIcons;

	[Header("Bosses + Platforming items")]
	[SerializeField]
	private List<MapEquipUIChecklistItem> checklistItems;

	[SerializeField]
	private List<MapEquipUIChecklistItem> finaleItems;

	[SerializeField]
	private GameObject finaleGrid;

	[SerializeField]
	private GameObject rightArrow;

	[SerializeField]
	private GameObject leftArrow;

	private int index;

	private int lastIndex;

	private int DLCIndex;

	private bool selectedFinale;

	private bool showDLCMenu;

	private bool skippedOver;

	private bool editorShowDLCChecklist;

	private Color darkText;

	private Color lightText;

	private Color disabledText;

	private int selectableLength;

	public override void Init(PlayerId playerID)
	{
		base.Init(playerID);
		darkText = new Color(0.2f, 0.188f, 0.188f);
		lightText = new Color(0.827f, 0.765f, 0.702f);
		disabledText = new Color(0.537f, 0.498f, 0.463f);
		selectableLength = worldSelectionIcons.Length;
		for (int i = 0; i < worldSelectionIcons.Length; i++)
		{
			worldSelectionIcons[i].SetIcons("Icons/" + worldPaths[i] + "_dark");
			worldSelectionIcons[i].SetTextColor(darkText);
		}
		worldSelectionIcons[index].SetIcons("Icons/" + worldPaths[index] + "_light");
		worldSelectionIcons[index].SetTextColor(lightText);
		if (!PlayerData.Data.CheckLevelsCompleted(Level.world1BossLevels))
		{
			worldSelectionIcons[worldSelectionIcons.Length - 1].SetTextColor(disabledText);
			worldSelectionIcons[worldSelectionIcons.Length - 2].SetTextColor(disabledText);
			worldSelectionIcons[worldSelectionIcons.Length - 3].SetTextColor(disabledText);
			selectableLength -= 3;
		}
		else if (!PlayerData.Data.CheckLevelsCompleted(Level.world2BossLevels))
		{
			worldSelectionIcons[worldSelectionIcons.Length - 1].SetTextColor(disabledText);
			worldSelectionIcons[worldSelectionIcons.Length - 2].SetTextColor(disabledText);
			selectableLength -= 2;
		}
		else if (!PlayerData.Data.CheckLevelsCompleted(Level.world3BossLevels))
		{
			worldSelectionIcons[worldSelectionIcons.Length - 1].SetTextColor(disabledText);
			selectableLength--;
		}
		UpdateList();
	}

	private void SetArrow(bool showRight)
	{
		rightArrow.SetActive(showRight);
		leftArrow.SetActive(!showRight);
	}

	public void ChangeSelection(int direction)
	{
		index = Mathf.Clamp(index + direction, 0, selectableLength - 1);
		bool flag = false;
		skippedOver = false;
		if (showDLCMenu)
		{
			if (selectableLength < worldSelectionIcons.Length)
			{
				flag = true;
				if (DLCIndex == worldNames.Length - 1 && direction < 0)
				{
					DLCIndex = selectableLength - 1;
					skippedOver = true;
					SetArrow(showRight: true);
				}
				else if (DLCIndex + direction > selectableLength - 1)
				{
					DLCIndex = worldNames.Length - 1;
					skippedOver = true;
					SetArrow(showRight: false);
				}
				else if (DLCIndex + direction < 0)
				{
					DLCIndex = 0;
					SetArrow(showRight: true);
				}
				else
				{
					DLCIndex += direction;
				}
			}
			else if (DLCIndex + direction < 0)
			{
				DLCIndex = 0;
				SetArrow(showRight: true);
			}
			else if (DLCIndex + direction > worldNames.Length - 1)
			{
				DLCIndex = worldNames.Length - 1;
				SetArrow(showRight: false);
			}
			else
			{
				DLCIndex += direction;
			}
			ChangeDLCMenu(index, lastIndex);
		}
		if (flag)
		{
			int num = ((DLCIndex != worldNames.Length - 1) ? index : (worldSelectionIcons.Length - 1));
			SetCursorPosition(num, openingChecklist: false);
		}
		else
		{
			SetCursorPosition(index, openingChecklist: false);
		}
	}

	public void SetCursorPosition(int index, bool openingChecklist)
	{
		if (openingChecklist)
		{
			showDLCMenu = (DLCManager.DLCEnabled() && PlayerData.Data.GetMapData(Scenes.scene_map_world_DLC).sessionStarted) || editorShowDLCChecklist;
			if (index >= worldNames.Length - 1)
			{
				DLCIndex = index;
				index = worldSelectionIcons.Length - 1;
			}
			else
			{
				DLCIndex = index;
			}
		}
		this.index = index;
		if (lastIndex != index)
		{
			worldSelectionIcons[index].SetIcons("Icons/" + worldPaths[index] + "_light");
			worldSelectionIcons[index].SetTextColor(new Color(0.827f, 0.765f, 0.702f));
			if (!skippedOver)
			{
				worldSelectionIcons[lastIndex].SetIcons("Icons/" + worldPaths[lastIndex] + "_dark");
				worldSelectionIcons[lastIndex].SetTextColor(new Color(0.2f, 0.188f, 0.188f));
			}
			AudioManager.Play("menu_equipment_move");
			lastIndex = index;
		}
		if (showDLCMenu)
		{
			if (openingChecklist)
			{
				skippedOver = true;
				if (DLCIndex < worldNames.Length - 1)
				{
					SetArrow(showRight: true);
				}
				ChangeDLCMenu(index, lastIndex);
			}
			if (DLCIndex == 0)
			{
				SetArrow(showRight: true);
			}
			else if (DLCIndex == worldNames.Length - 1)
			{
				SetArrow(showRight: false);
			}
		}
		cursor.SetPosition(worldSelectionIcons[index].transform.position);
		UpdateList();
	}

	private void ChangeDLCMenu(int index, int lastIndex)
	{
		if ((index != lastIndex || (index > 0 && index < selectableLength - 1)) && !skippedOver)
		{
			return;
		}
		int num = ((DLCIndex == worldNames.Length - 1) ? 1 : 0);
		int num2 = 0;
		bool flag = index >= worldSelectionIcons.Length - 1;
		int num3 = 0;
		for (int i = 0; i < worldSelectionIcons.Length; i++)
		{
			TranslationElement translationElement = Localization.Find(worldNames[num].ToString());
			worldSelectionIcons[num2].iconText.text = translationElement.translation.text;
			num3 = ((DLCIndex == worldNames.Length - 1) ? 1 : 0);
			if (i > selectableLength - 1 - num3 && (DLCIndex != worldNames.Length - 1 || i != worldSelectionIcons.Length - 1))
			{
				worldSelectionIcons[i].SetTextColor(disabledText);
			}
			num = (num + 1) % worldNames.Length;
			num2 = (num2 + 1) % worldSelectionIcons.Length;
		}
	}

	private void UpdateList()
	{
		List<Levels> list = new List<Levels>();
		List<string> list2 = new List<string>();
		list.Clear();
		list2.Clear();
		for (int i = 0; i < checklistItems.Count; i++)
		{
			checklistItems[i].gameObject.SetActive(value: false);
			checklistItems[i].ClearDescription(selectedFinale);
		}
		for (int j = 0; j < finaleItems.Count; j++)
		{
			finaleItems[j].gameObject.SetActive(value: false);
			if (finaleItems[j].checkMark != null)
			{
				finaleItems[j].checkMark.enabled = false;
				finaleItems[j].ClearDescription(selectedFinale);
			}
		}
		bool flag = false;
		switch ((!showDLCMenu) ? index : DLCIndex)
		{
		case 0:
			list.AddRange(world1Levels);
			selectedFinale = false;
			break;
		case 1:
			list.AddRange(world2Levels);
			selectedFinale = false;
			break;
		case 2:
			list.AddRange(world3Levels);
			selectedFinale = false;
			break;
		case 3:
			list.AddRange(finaleLevels);
			selectedFinale = true;
			break;
		case 4:
			list.AddRange(DLClevels);
			selectedFinale = false;
			flag = true;
			break;
		}
		foreach (Levels item in list)
		{
			list2.Add(Level.GetLevelName(item).Replace("\\n", " "));
		}
		worldTop.SetActive(value: false);
		finaleTop.SetActive(value: false);
		localizedTop.SetActive(value: true);
		worldTopLocalized.SetActive(!selectedFinale && !flag);
		finaleTopLocalized.SetActive(selectedFinale);
		worldTopDLCLocalized.SetActive(flag);
		finaleGrid.SetActive(selectedFinale);
		bool played = PlayerData.Data.GetLevelData(Levels.Saltbaker).played;
		for (int k = 0; k < list2.Count; k++)
		{
			if (flag)
			{
				if (k != list2.Count - 1 || (k == list2.Count - 1 && played))
				{
					checklistItems[k].gameObject.SetActive(value: true);
					checklistItems[k].EnableCheckbox((k < list2.Count - 1) ? true : false);
					checklistItems[k].SetDescription(list[k], list2[k], selectedFinale);
				}
			}
			else if (!selectedFinale)
			{
				checklistItems[k].gameObject.SetActive(value: true);
				checklistItems[k].EnableCheckbox((k < list2.Count - 2) ? true : false);
				checklistItems[k].SetDescription(list[k], list2[k], selectedFinale);
			}
			else
			{
				finaleItems[k].gameObject.SetActive(value: true);
				finaleItems[k].SetDescription(list[k], list2[k], selectedFinale);
			}
		}
	}
}
