using System.Collections.Generic;
using Framework.Managers;
using Gameplay.UI.Others.MenuLogic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Widgets;

public class SaveSlot : MonoBehaviour
{
	[SerializeField]
	[BoxGroup("Buttons", true, false, 0)]
	private GameObject ButtonDelete;

	[SerializeField]
	[BoxGroup("Buttons", true, false, 0)]
	private GameObject ButtonUpgradeGamePlus;

	[SerializeField]
	[BoxGroup("Text", true, false, 0)]
	private Text SlotNumber;

	[SerializeField]
	[BoxGroup("Text", true, false, 0)]
	private Text ZoneText;

	[SerializeField]
	[BoxGroup("Text", true, false, 0)]
	private Text PurgeText;

	[SerializeField]
	[BoxGroup("Text", true, false, 0)]
	private Text InfoText;

	[SerializeField]
	[BoxGroup("Text", true, false, 0)]
	private Color ColorSelected;

	[SerializeField]
	[BoxGroup("Text", true, false, 0)]
	private Color ColorUnselected;

	[SerializeField]
	[BoxGroup("Text", true, false, 0)]
	private bool ChangeInfoColor = true;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private PenitenceSlot Penitence;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private Image Background;

	[SerializeField]
	[BoxGroup("Backgorunds", true, false, 0)]
	private Sprite EmptyBackgorund;

	[SerializeField]
	[BoxGroup("Backgorunds", true, false, 0)]
	private Sprite NormalSelectedBackgorund;

	[SerializeField]
	[BoxGroup("Backgorunds", true, false, 0)]
	private Sprite NormalUnSelectedBackgorund;

	[SerializeField]
	[BoxGroup("Backgorunds", true, false, 0)]
	private Sprite PlusSelectedBackgorund;

	[SerializeField]
	[BoxGroup("Backgorunds", true, false, 0)]
	private Sprite PlusUnSelectedBackgorund;

	private SelectSaveSlots.SlotsModes CurrentMode;

	public bool IsEmpty { get; private set; }

	public bool CanLoad { get; private set; }

	public bool IsNewGamePlus { get; private set; }

	public bool CanConvertToNewGamePlus { get; private set; }

	public int NewGamePlusUpgrades { get; private set; }

	public void SetNumber(int slot)
	{
		SlotNumber.text = slot.ToString();
	}

	public void SetData(string zoneName, string info, int purge, bool canLoad, bool isPlus, bool canConvert, int newGamePlusUpgrades, SelectSaveSlots.SlotsModes mode)
	{
		CurrentMode = mode;
		CanLoad = canLoad;
		IsEmpty = zoneName == string.Empty;
		IsNewGamePlus = isPlus;
		CanConvertToNewGamePlus = canConvert;
		ZoneText.text = zoneName;
		InfoText.text = info;
		PurgeText.text = purge.ToString();
		Penitence.gameObject.SetActive(!IsEmpty);
		PurgeText.gameObject.SetActive(!IsEmpty);
		NewGamePlusUpgrades = newGamePlusUpgrades;
		SetSelected(selected: false);
	}

	public void SetPenitenceData(PenitenceManager.PenitencePersistenceData data)
	{
		Penitence.UpdateFromSavegameData(data);
	}

	public void SetPenitenceConfig(List<SelectSaveSlots.PenitenceData> data)
	{
		Penitence.SetPenitenceConfig(data);
	}

	public void SetSelected(bool selected)
	{
		Sprite sprite = EmptyBackgorund;
		if (!IsEmpty)
		{
			sprite = ((!IsNewGamePlus) ? ((!selected) ? NormalUnSelectedBackgorund : NormalSelectedBackgorund) : ((!selected) ? PlusUnSelectedBackgorund : PlusSelectedBackgorund));
		}
		Background.sprite = sprite;
		if ((bool)ButtonUpgradeGamePlus)
		{
			ButtonUpgradeGamePlus.SetActive(selected && CanConvertToNewGamePlus && CurrentMode == SelectSaveSlots.SlotsModes.Normal);
		}
		if ((bool)ButtonDelete)
		{
			ButtonDelete.SetActive(selected && !IsEmpty && CurrentMode == SelectSaveSlots.SlotsModes.Normal);
		}
		Color color = ((!selected) ? ColorUnselected : ColorSelected);
		SlotNumber.color = color;
		ZoneText.color = color;
		PurgeText.color = color;
		if (ChangeInfoColor)
		{
			InfoText.color = color;
		}
	}
}
