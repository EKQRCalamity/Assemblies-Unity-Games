using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapEquipUICardBackSelect : AbstractMapEquipUICardSide
{
	private static readonly Weapon[] WEAPONS = new Weapon[9]
	{
		Weapon.level_weapon_peashot,
		Weapon.level_weapon_spreadshot,
		Weapon.level_weapon_homing,
		Weapon.level_weapon_bouncer,
		Weapon.level_weapon_charge,
		Weapon.level_weapon_boomerang,
		Weapon.level_weapon_crackshot,
		Weapon.level_weapon_wide_shot,
		Weapon.level_weapon_upshot
	};

	private static readonly Super[] SUPERS = new Super[3]
	{
		Super.level_super_beam,
		Super.level_super_invincible,
		Super.level_super_ghost
	};

	private static readonly Super[] CHALICESUPERS = new Super[3]
	{
		Super.level_super_chalice_vert_beam,
		Super.level_super_chalice_shield,
		Super.level_super_chalice_iii
	};

	private static readonly Charm[] CHARMS = new Charm[9]
	{
		Charm.charm_health_up_1,
		Charm.charm_super_builder,
		Charm.charm_smoke_dash,
		Charm.charm_parry_plus,
		Charm.charm_health_up_2,
		Charm.charm_parry_attack,
		Charm.charm_chalice,
		Charm.charm_curse,
		Charm.charm_healer
	};

	private static readonly string[] slotLocalesKey = new string[4] { "ShotABackTitle", "ShotBBackTitle", "SuperBackTitle", "CharmBackTitle" };

	public bool lockInput;

	[Header("Text")]
	[SerializeField]
	private LocalizationHelper headerText;

	[SerializeField]
	private Text titleText;

	[SerializeField]
	private Text exText;

	[SerializeField]
	private TMP_Text descriptionText;

	[Header("Cursors")]
	[SerializeField]
	private MapEquipUICursor cursor;

	[SerializeField]
	private MapEquipUICardBackSelectSelectionCursor selectionCursor;

	[Header("Backs")]
	[SerializeField]
	private Image iconsBack;

	[SerializeField]
	private Image superIconsBack;

	[SerializeField]
	private Image DLCIconsBack;

	[Header("Icons")]
	[SerializeField]
	private MapEquipUICardBackSelectIcon[] normalIcons;

	[Header("Super Icons")]
	[SerializeField]
	private MapEquipUICardBackSelectIcon[] superIcons;

	[Header("DLC Icons")]
	[SerializeField]
	private MapEquipUICardBackSelectIcon[] DLCIcons;

	private int index;

	private int lastIndex;

	private MapEquipUICard.Slot slot;

	private MapEquipUICard.Slot lastSlot;

	private MapEquipUICardBackSelectIcon[] selectedIcons;

	private bool noneUnlocked;

	private bool itemSelected;

	public void ChangeSelection(Trilean2 direction)
	{
		index = selectedIcons[index].GetIndexOfNeighbor(direction);
		SetCursorPosition(index);
		UpdateText();
	}

	public void ChangeSlot(int direction)
	{
		cursor.Show();
		int num = (int)slot;
		slot = (MapEquipUICard.Slot)Mathf.Repeat(num + direction, EnumUtils.GetCount<MapEquipUICard.Slot>());
		Setup(slot);
	}

	private void UpdateText()
	{
		bool flag = false;
		switch (slot)
		{
		case MapEquipUICard.Slot.SHOT_A:
		case MapEquipUICard.Slot.SHOT_B:
			flag = PlayerData.Data.IsUnlocked(base.playerID, WEAPONS[index]);
			titleText.text = ((!flag) ? Localization.Translate("EquipItemLocked").text : WeaponProperties.GetDisplayName(WEAPONS[index]).ToUpper());
			exText.text = ((!flag) ? "? ? ? ? ? ? ? ? ?" : WeaponProperties.GetSubtext(WEAPONS[index]));
			descriptionText.text = ((!flag) ? "? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ?" : WeaponProperties.GetDescription(WEAPONS[index]));
			break;
		case MapEquipUICard.Slot.SUPER:
		{
			flag = PlayerData.Data.IsUnlocked(base.playerID, SUPERS[index]);
			PlayerData.PlayerLoadouts.PlayerLoadout playerLoadout = PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID);
			Super super = ((playerLoadout.charm != Charm.charm_chalice) ? SUPERS[index] : CHALICESUPERS[index]);
			titleText.text = ((!flag) ? Localization.Translate("EquipItemLocked").text : WeaponProperties.GetDisplayName(SUPERS[index]).ToUpper());
			exText.text = ((!flag) ? "? ? ? ? ? ? ? ? ?" : WeaponProperties.GetSubtext(super));
			descriptionText.text = ((!flag) ? "? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ?" : WeaponProperties.GetDescription(super));
			break;
		}
		case MapEquipUICard.Slot.CHARM:
			flag = PlayerData.Data.IsUnlocked(base.playerID, CHARMS[index]);
			if (CHARMS[index] == Charm.charm_curse && CharmCurse.IsMaxLevel(base.playerID))
			{
				titleText.text = Localization.Translate("charm_paladin_name").text;
				exText.text = Localization.Translate("charm_paladin_subtext").text;
				descriptionText.text = Localization.Translate("charm_paladin_description").text;
			}
			else if (flag && CHARMS[index] == Charm.charm_curse && (CharmCurse.CalculateLevel(PlayerId.PlayerOne) > -1 || CharmCurse.CalculateLevel(PlayerId.PlayerTwo) > -1))
			{
				titleText.text = Localization.Translate("charm_curse_name").text;
				exText.text = Localization.Translate("charm_curse_subtext").text;
				descriptionText.text = Localization.Translate("charm_curse_description").text;
			}
			else if (flag && CHARMS[index] == Charm.charm_curse)
			{
				titleText.text = Localization.Translate("charm_broken_name").text;
				exText.text = Localization.Translate("charm_broken_subtext").text;
				descriptionText.text = Localization.Translate("charm_broken_description").text;
			}
			else
			{
				titleText.text = ((!flag) ? Localization.Translate("EquipItemLocked").text : WeaponProperties.GetDisplayName(CHARMS[index]).ToUpper());
				exText.text = ((!flag) ? "? ? ? ? ? ? ? ? ?" : WeaponProperties.GetSubtext(CHARMS[index]));
				descriptionText.text = ((!flag) ? "? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ?" : WeaponProperties.GetDescription(CHARMS[index]));
			}
			break;
		}
		titleText.font = Localization.Instance.fonts[(int)Localization.language][9].font;
		if (flag)
		{
			exText.font = Localization.Instance.fonts[(int)Localization.language][10].font;
			descriptionText.font = Localization.Instance.fonts[(int)Localization.language][11].fontAsset;
		}
		else
		{
			exText.font = Localization.Instance.fonts[(int)Localization.language1][10].font;
			descriptionText.font = Localization.Instance.fonts[(int)Localization.language1][11].fontAsset;
		}
	}

	private void SetCursorPosition(int index)
	{
		if (lastIndex != index)
		{
			AudioManager.Play("menu_equipment_move");
			lastIndex = index;
		}
		cursor.SetPosition(selectedIcons[index].transform.position);
		if (!noneUnlocked && itemSelected)
		{
			selectionCursor.Show();
		}
		else
		{
			selectionCursor.Hide();
		}
	}

	public void Setup(MapEquipUICard.Slot slot)
	{
		this.slot = slot;
		headerText.ApplyTranslation(Localization.Find(slotLocalesKey[(int)slot]));
		bool flag = slot == MapEquipUICard.Slot.SUPER;
		bool flag2 = DLCManager.DLCEnabled();
		selectedIcons = (flag ? superIcons : ((!flag2) ? normalIcons : DLCIcons));
		MapEquipUICardBackSelectIcon[] array = superIcons;
		foreach (MapEquipUICardBackSelectIcon mapEquipUICardBackSelectIcon in array)
		{
			mapEquipUICardBackSelectIcon.gameObject.SetActive(flag);
		}
		MapEquipUICardBackSelectIcon[] array2 = normalIcons;
		foreach (MapEquipUICardBackSelectIcon mapEquipUICardBackSelectIcon2 in array2)
		{
			mapEquipUICardBackSelectIcon2.gameObject.SetActive(!flag && !flag2);
		}
		MapEquipUICardBackSelectIcon[] dLCIcons = DLCIcons;
		foreach (MapEquipUICardBackSelectIcon mapEquipUICardBackSelectIcon3 in dLCIcons)
		{
			mapEquipUICardBackSelectIcon3.gameObject.SetActive(!flag && flag2);
		}
		superIconsBack.enabled = flag;
		iconsBack.enabled = !flag && !flag2;
		DLCIconsBack.enabled = !flag && flag2;
		PlayerData.PlayerLoadouts.PlayerLoadout playerLoadout = PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID);
		selectionCursor.Hide();
		noneUnlocked = true;
		index = -1;
		bool isGrey = false;
		itemSelected = false;
		for (int l = 0; l < selectedIcons.Length; l++)
		{
			selectedIcons[l].Index = l;
			string text = "_000" + (l % 8 + 1).ToStringInvariant();
			string text2 = Weapon.None.ToString();
			switch (slot)
			{
			case MapEquipUICard.Slot.SHOT_A:
				if (PlayerData.Data.IsUnlocked(base.playerID, WEAPONS[l]))
				{
					isGrey = ((WEAPONS[l] == playerLoadout.secondaryWeapon) ? true : false);
					noneUnlocked = false;
					selectedIcons[l].SetIcons(WEAPONS[l], isGrey);
				}
				else
				{
					text2 = WeaponProperties.GetIconPath(Weapon.None) + text;
					selectedIcons[l].SetIconsManual(text2, isGrey);
				}
				if (WEAPONS[l] == playerLoadout.primaryWeapon && playerLoadout.primaryWeapon != Weapon.None)
				{
					index = l;
					itemSelected = true;
				}
				break;
			case MapEquipUICard.Slot.SHOT_B:
				if (PlayerData.Data.IsUnlocked(base.playerID, WEAPONS[l]))
				{
					isGrey = ((WEAPONS[l] == playerLoadout.primaryWeapon) ? true : false);
					noneUnlocked = false;
					selectedIcons[l].SetIcons(WEAPONS[l], isGrey);
				}
				else
				{
					text2 = WeaponProperties.GetIconPath(Weapon.None) + text;
					selectedIcons[l].SetIconsManual(text2, isGrey);
				}
				if (WEAPONS[l] == playerLoadout.secondaryWeapon && playerLoadout.secondaryWeapon != Weapon.None)
				{
					index = l;
					itemSelected = true;
				}
				break;
			case MapEquipUICard.Slot.SUPER:
				if (PlayerData.Data.IsUnlocked(base.playerID, SUPERS[l]))
				{
					noneUnlocked = false;
					selectedIcons[l].SetIcons(SUPERS[l], isGrey);
				}
				else
				{
					text2 = WeaponProperties.GetIconPath(Super.None) + text;
					selectedIcons[l].SetIconsManual(text2, isGrey);
				}
				if (playerLoadout.super != Super.None && (SUPERS[l] == playerLoadout.super || (playerLoadout.charm == Charm.charm_chalice && CHALICESUPERS[l] == playerLoadout.super)))
				{
					index = l;
					itemSelected = true;
				}
				break;
			case MapEquipUICard.Slot.CHARM:
				if (PlayerData.Data.IsUnlocked(base.playerID, CHARMS[l]))
				{
					noneUnlocked = false;
					if (CHARMS[l] == Charm.charm_curse)
					{
						selectedIcons[l].SetIconsManual("Icons/equip_icon_charm_curse_" + (CharmCurse.CalculateLevel(base.playerID) + 1), isGrey: false, isDLC: true);
					}
					else
					{
						selectedIcons[l].SetIcons(CHARMS[l], isGrey);
					}
				}
				else
				{
					text2 = WeaponProperties.GetIconPath(Charm.None) + text;
					selectedIcons[l].SetIconsManual(text2, isGrey);
				}
				if (CHARMS[l] == playerLoadout.charm && playerLoadout.charm != Charm.None)
				{
					index = l;
					itemSelected = true;
				}
				break;
			}
			if (index == -1)
			{
				index = 0;
			}
			cursor.SetPosition(selectedIcons[index].transform.position);
			UpdateText();
		}
		selectionCursor.selectedIndex = -1;
		if (!noneUnlocked && itemSelected)
		{
			if (slot != lastSlot)
			{
				selectionCursor.Show();
				selectionCursor.selectedIndex = index;
				selectionCursor.SetPosition(selectedIcons[index].transform.position);
			}
			else
			{
				StartCoroutine(set_selection_cursor());
			}
			cursor.SelectIcon(onSame: true);
		}
		lastSlot = slot;
	}

	private IEnumerator set_selection_cursor()
	{
		StartCoroutine(lock_input_cr());
		while (!selectionCursor.animator.GetCurrentAnimatorStateInfo(0).IsName("Off") && lockInput)
		{
			yield return null;
		}
		selectionCursor.Show();
		selectionCursor.selectedIndex = index;
		selectionCursor.SetPosition(selectedIcons[index].transform.position);
		yield return null;
	}

	private IEnumerator lock_input_cr()
	{
		lockInput = true;
		yield return new WaitForSeconds(0.2f);
		lockInput = false;
	}

	public void Accept()
	{
		AudioManager.Play("menu_equipment_equip");
		PlayerData.PlayerLoadouts.PlayerLoadout playerLoadout = PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID);
		switch (slot)
		{
		case MapEquipUICard.Slot.SHOT_A:
			if (PlayerData.Data.IsUnlocked(base.playerID, WEAPONS[index]))
			{
				Selection();
			}
			break;
		case MapEquipUICard.Slot.SHOT_B:
			if (PlayerData.Data.IsUnlocked(base.playerID, WEAPONS[index]) && (playerLoadout.primaryWeapon != WEAPONS[index] || playerLoadout.secondaryWeapon != Weapon.None))
			{
				Selection();
			}
			break;
		case MapEquipUICard.Slot.SUPER:
			if (PlayerData.Data.IsUnlocked(base.playerID, SUPERS[index]))
			{
				Selection();
			}
			break;
		case MapEquipUICard.Slot.CHARM:
			if (PlayerData.Data.IsUnlocked(base.playerID, CHARMS[index]))
			{
				Selection();
			}
			break;
		}
		switch (slot)
		{
		case MapEquipUICard.Slot.SHOT_A:
			if (WEAPONS[index] == Weapon.None || !PlayerData.Data.IsUnlocked(base.playerID, WEAPONS[index]))
			{
				OnLocked();
				return;
			}
			if (PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).secondaryWeapon == WEAPONS[index])
			{
				PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).secondaryWeapon = PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).primaryWeapon;
			}
			PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).primaryWeapon = WEAPONS[index];
			break;
		case MapEquipUICard.Slot.SHOT_B:
			if (WEAPONS[index] == Weapon.None || !PlayerData.Data.IsUnlocked(base.playerID, WEAPONS[index]) || (playerLoadout.primaryWeapon == WEAPONS[index] && playerLoadout.secondaryWeapon == Weapon.None))
			{
				OnLocked();
				return;
			}
			if (PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).primaryWeapon == WEAPONS[index])
			{
				PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).primaryWeapon = PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).secondaryWeapon;
			}
			PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).secondaryWeapon = WEAPONS[index];
			if (!PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).HasEquippedSecondaryRegularWeapon)
			{
				PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).MustNotifySwitchRegularWeapon = true;
			}
			PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).HasEquippedSecondaryRegularWeapon = true;
			break;
		case MapEquipUICard.Slot.SUPER:
			if (SUPERS[index] == Super.None || !PlayerData.Data.IsUnlocked(base.playerID, SUPERS[index]))
			{
				OnLocked();
				return;
			}
			PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).super = SUPERS[index];
			break;
		case MapEquipUICard.Slot.CHARM:
			if (CHARMS[index] == Charm.None || !PlayerData.Data.IsUnlocked(base.playerID, CHARMS[index]))
			{
				OnLocked();
				return;
			}
			if (CHARMS[index] != Charm.charm_chalice && PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).charm == Charm.charm_chalice)
			{
				PlayerManager.OnChaliceCharmUnequipped(base.playerID);
			}
			PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).charm = CHARMS[index];
			break;
		}
		Setup(slot);
	}

	public void Unequip()
	{
		AudioManager.Play("menu_equipment_equip");
		switch (slot)
		{
		case MapEquipUICard.Slot.SHOT_A:
			OnLocked();
			break;
		case MapEquipUICard.Slot.SHOT_B:
			if (PlayerData.Data.IsUnlocked(base.playerID, WEAPONS[index]) && PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).secondaryWeapon != Weapon.None)
			{
				Deselect();
				PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).secondaryWeapon = Weapon.None;
				if (PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).MustNotifySwitchRegularWeapon)
				{
					PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).HasEquippedSecondaryRegularWeapon = false;
					PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).MustNotifySwitchRegularWeapon = false;
				}
			}
			else
			{
				OnLocked();
			}
			break;
		case MapEquipUICard.Slot.SUPER:
			if (PlayerData.Data.IsUnlocked(base.playerID, SUPERS[index]) && PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).super != Super.None)
			{
				Deselect();
				PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).super = Super.None;
			}
			else
			{
				OnLocked();
			}
			break;
		case MapEquipUICard.Slot.CHARM:
			if (PlayerData.Data.IsUnlocked(base.playerID, CHARMS[index]) && PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).charm != Charm.None)
			{
				if (PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).charm == Charm.charm_chalice)
				{
					PlayerManager.OnChaliceCharmUnequipped(base.playerID);
				}
				Deselect();
				PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).charm = Charm.None;
			}
			else
			{
				OnLocked();
			}
			break;
		}
	}

	private void Selection()
	{
		selectedIcons[index].SelectIcon();
		if (selectionCursor.selectedIndex >= 0)
		{
			selectedIcons[selectionCursor.selectedIndex].UnselectIcon();
		}
		if (cursor.transform.position != selectionCursor.transform.position)
		{
			selectionCursor.selectedIndex = index;
			selectionCursor.Select();
			cursor.SelectIcon(onSame: false);
		}
		else
		{
			cursor.SelectIcon(onSame: true);
		}
	}

	private void Deselect()
	{
		StartCoroutine(remove_selection_cr());
		itemSelected = false;
	}

	private IEnumerator remove_selection_cr()
	{
		selectionCursor.Select();
		yield return selectionCursor.animator.WaitForAnimationToEnd(this, "Select");
		selectionCursor.Hide();
		yield return null;
	}

	private void OnLocked()
	{
		AudioManager.Play("menu_locked");
		selectedIcons[index].OnLocked();
		cursor.OnLocked();
	}
}
