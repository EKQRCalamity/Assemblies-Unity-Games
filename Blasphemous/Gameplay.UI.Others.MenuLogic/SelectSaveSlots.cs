using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Managers;
using Framework.Map;
using Gameplay.UI.Others.Buttons;
using Gameplay.UI.Widgets;
using I2.Loc;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class SelectSaveSlots : SerializedMonoBehaviour
{
	[Serializable]
	public class PenitenceData
	{
		public string id;

		public Sprite InProgress;

		public Sprite Completed;

		public Sprite Missing;
	}

	public enum SlotsModes
	{
		Normal,
		BossRush
	}

	[SerializeField]
	[BoxGroup("Elements", true, false, 0)]
	private GameObject buttonNewGame;

	[SerializeField]
	[BoxGroup("Elements", true, false, 0)]
	private GameObject buttonContinue;

	[SerializeField]
	[BoxGroup("Elements", true, false, 0)]
	private GameObject ConfirmationUpgradeRoot;

	[SerializeField]
	[BoxGroup("Elements", true, false, 0)]
	private GameObject ConfirmationUpgradePlusRoot;

	[SerializeField]
	[BoxGroup("Elements", true, false, 0)]
	private GameObject ConfirmationDeleteRoot;

	[SerializeField]
	[BoxGroup("Elements", true, false, 0)]
	private List<EventsButton> AllSlots;

	[SerializeField]
	[BoxGroup("Config", true, false, 0)]
	private float timetoShowCorruptedMessage = 2f;

	[SerializeField]
	[BoxGroup("Penitence", true, false, 0)]
	private List<PenitenceData> PenitencesConfig = new List<PenitenceData>();

	[SerializeField]
	[BoxGroup("Slots", true, false, 0)]
	private List<SaveSlot> slots = new List<SaveSlot>();

	private List<GameObject> options = new List<GameObject>();

	private NewMainMenu parentMainMenu;

	private SlotsModes CurrentSlotsMode;

	public ShowHideUIItem corruptedSaveMessage;

	public bool IsShowing { get; private set; }

	public bool IsConfirming { get; private set; }

	public bool IsConfirmingUpgrade { get; private set; }

	public int SelectedSlot { get; private set; }

	public bool MustConvertToNewgamePlus { get; private set; }

	public bool CanLoadSelectedSlot
	{
		get
		{
			if (SelectedSlot >= 0)
			{
				return slots[SelectedSlot].CanLoad;
			}
			return false;
		}
	}

	private void Update()
	{
		if (ReInput.players.playerCount <= 0 || ((bool)corruptedSaveMessage && corruptedSaveMessage.IsShowing) || CurrentSlotsMode == SlotsModes.BossRush)
		{
			return;
		}
		Player player = ReInput.players.GetPlayer(0);
		if (player.GetButtonDown(51))
		{
			if (IsConfirming)
			{
				OnConfirmationCancel();
			}
			else if (IsShowing)
			{
				parentMainMenu.PlaySoundOnBack();
				SelectedSlot = -1;
				IsShowing = false;
			}
		}
		if (IsShowing && !IsConfirming && SelectedSlot >= 0 && player.GetButtonDown(52) && !slots[SelectedSlot].IsEmpty)
		{
			SetConfirming(IsUpgrade: false, 0);
		}
		if (IsShowing && !IsConfirming && SelectedSlot >= 0 && player.GetButtonDown(61) && !slots[SelectedSlot].IsEmpty && slots[SelectedSlot].CanLoad && slots[SelectedSlot].CanConvertToNewGamePlus)
		{
			SetConfirming(IsUpgrade: true, slots[SelectedSlot].NewGamePlusUpgrades);
		}
	}

	public void Clear()
	{
		IsShowing = false;
		IsConfirming = false;
		MustConvertToNewgamePlus = false;
		SelectedSlot = -1;
		SetAccept(isEmpty: false);
		for (int i = 0; i < slots.Count; i++)
		{
			SaveSlot saveSlot = slots[i];
			saveSlot.SetNumber(i + 1);
			saveSlot.SetData(string.Empty, string.Empty, 0, canLoad: false, isPlus: false, canConvert: false, 0, CurrentSlotsMode);
			saveSlot.SetPenitenceConfig(PenitencesConfig);
		}
	}

	public void SetAllData(NewMainMenu mainMenu, SlotsModes mode)
	{
		CurrentSlotsMode = mode;
		IsConfirming = false;
		MustConvertToNewgamePlus = false;
		parentMainMenu = mainMenu;
		IsShowing = true;
		SelectedSlot = -1;
		int num = 0;
		bool flag = false;
		if ((bool)ConfirmationUpgradeRoot)
		{
			ConfirmationUpgradeRoot.transform.parent.gameObject.SetActive(value: true);
		}
		foreach (SaveSlot slot in slots)
		{
			string zoneName = string.Empty;
			string info = string.Empty;
			float num2 = 0f;
			PersistentManager.PublicSlotData slotData = Core.Persistence.GetSlotData(num);
			bool flag2 = slotData != null && (!slotData.persistence.Corrupted || slotData.persistence.HasBackup);
			bool isPlus = false;
			bool canConvert = false;
			int newGamePlusUpgrades = 0;
			if (flag2)
			{
				int num3 = (int)(slotData.persistence.Time / 3600f);
				int num4 = (int)(slotData.persistence.Time % 3600f / 60f);
				string text = string.Empty;
				if (num3 > 0)
				{
					text = num3 + "h ";
				}
				text = text + num4 + "m";
				ZoneKey sceneKey = new ZoneKey(slotData.persistence.CurrentDomain, slotData.persistence.CurrentZone, string.Empty);
				zoneName = Core.NewMapManager.GetZoneName(sceneKey);
				float num5 = Mathf.Min(slotData.persistence.Percent, 150f);
				info = Core.Localization.GetValueWithParams(ScriptLocalization.UI_Slot.TEXT_SLOT_INFO, new Dictionary<string, string>
				{
					{ "playtime", text },
					{
						"completed",
						num5.ToString("0.##")
					}
				});
				num2 = slotData.persistence.Purge;
				isPlus = slotData.persistence.IsNewGamePlus;
				canConvert = slotData.persistence.CanConvertToNewGamePlus;
				newGamePlusUpgrades = slotData.persistence.NewGamePlusUpgrades;
				slot.SetPenitenceData(slotData.penitence);
			}
			if (slotData != null && slotData.persistence.Corrupted)
			{
				flag = true;
			}
			slot.SetData(zoneName, info, (int)num2, flag2, isPlus, canConvert, newGamePlusUpgrades, CurrentSlotsMode);
			num++;
		}
		if (flag)
		{
			StartCoroutine(ShowCorruptedMenssage());
		}
		else
		{
			OnSelectedSlots(0);
		}
	}

	public void OnSelectedSlots(int idxSlot)
	{
		if (IsShowing && (!corruptedSaveMessage || !corruptedSaveMessage.IsShowing))
		{
			if (SelectedSlot >= 0 && SelectedSlot != idxSlot)
			{
				slots[SelectedSlot].SetSelected(selected: false);
			}
			SelectedSlot = idxSlot;
			if (SelectedSlot >= 0 && SelectedSlot < slots.Count)
			{
				slots[SelectedSlot].SetSelected(selected: true);
				SetAccept(slots[SelectedSlot].IsEmpty);
			}
		}
	}

	public void OnAcceptSlots(int idxSlot)
	{
		if (IsShowing && (!corruptedSaveMessage || !corruptedSaveMessage.IsShowing) && (CurrentSlotsMode != SlotsModes.BossRush || !slots[idxSlot].IsEmpty))
		{
			SelectedSlot = idxSlot;
			IsShowing = false;
		}
	}

	public void OnConfirmationDelete()
	{
		IsConfirming = false;
		if (IsConfirmingUpgrade)
		{
			MustConvertToNewgamePlus = true;
			IsShowing = false;
		}
		else
		{
			Core.Persistence.DeleteSaveGame(SelectedSlot);
			SetAllData(parentMainMenu, SlotsModes.Normal);
		}
		parentMainMenu.SetNormalModeFromConfirmation();
		parentMainMenu.PlaySoundOnBack();
	}

	public void OnConfirmationCancel()
	{
		IsConfirming = false;
		parentMainMenu.SetNormalModeFromConfirmation();
		parentMainMenu.PlaySoundOnBack();
	}

	private void SetAccept(bool isEmpty)
	{
		if ((bool)buttonNewGame)
		{
			buttonNewGame.SetActive(CurrentSlotsMode == SlotsModes.Normal && isEmpty);
		}
		if ((bool)buttonContinue)
		{
			buttonContinue.SetActive(CurrentSlotsMode == SlotsModes.Normal && !isEmpty);
		}
	}

	public IEnumerator ShowCorruptedMenssage()
	{
		AllSlots.ForEach(delegate(EventsButton x)
		{
			Navigation navigation = x.navigation;
			navigation.mode = Navigation.Mode.None;
			x.navigation = navigation;
		});
		corruptedSaveMessage.Show();
		yield return new WaitForSecondsRealtime(timetoShowCorruptedMessage);
		corruptedSaveMessage.Hide();
		corruptedSaveMessage.OnHidden += OnCorruptedSaveMessageHidden;
	}

	private void OnCorruptedSaveMessageHidden(ShowHideUIItem item)
	{
		corruptedSaveMessage.OnHidden -= OnCorruptedSaveMessageHidden;
		AllSlots.ForEach(delegate(EventsButton x)
		{
			Navigation navigation = x.navigation;
			navigation.mode = Navigation.Mode.Explicit;
			x.navigation = navigation;
		});
		OnSelectedSlots(0);
	}

	private void SetConfirming(bool IsUpgrade, int NumberOfNewGamePlusUpgrades)
	{
		IsConfirming = true;
		IsConfirmingUpgrade = IsUpgrade;
		ConfirmationUpgradeRoot.SetActive(IsUpgrade && NumberOfNewGamePlusUpgrades == 0);
		ConfirmationUpgradePlusRoot.SetActive(IsUpgrade && NumberOfNewGamePlusUpgrades > 0);
		ConfirmationDeleteRoot.SetActive(!IsUpgrade);
		parentMainMenu.SetConfirmationDeleteFromSlot();
	}
}
