using UnityEngine;
using UnityEngine.UI;

public class MapEquipUICardFront : AbstractMapEquipUICardSide
{
	public MapEquipUICardFrontIcon weaponA;

	public MapEquipUICardFrontIcon weaponB;

	public MapEquipUICardFrontIcon super;

	public MapEquipUICardFrontIcon item;

	public MapEquipUICardFrontIcon checklist;

	public bool checkListSelected;

	[Space(10f)]
	public MapEquipUICursor cursor;

	private int index;

	private MapEquipUICardFrontIcon[] icons;

	public Text title;

	public Outline[] outlines;

	private PlayerData.PlayerLoadouts.PlayerLoadout loadout;

	public MapEquipUICard.Slot Slot => (MapEquipUICard.Slot)index;

	private void Update()
	{
		SetCursorPosition(index);
	}

	private void Start()
	{
		Localization.OnLanguageChangedEvent += OnLanguageChanged;
	}

	private void OnDestroy()
	{
		Localization.OnLanguageChangedEvent -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		ChangeSelection(0);
	}

	public override void Init(PlayerId playerID)
	{
		base.Init(playerID);
		icons = new MapEquipUICardFrontIcon[5] { weaponA, weaponB, super, item, checklist };
		checklist.SetIconsManual("Icons/equip_icon_list", isGrey: false);
		checkListSelected = false;
		Refresh();
		ChangeSelection(0);
	}

	public void Refresh()
	{
		loadout = PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID);
		weaponA.SetIcons(loadout.primaryWeapon, isGrey: false);
		weaponB.SetIcons(loadout.secondaryWeapon, isGrey: false);
		super.SetIcons(loadout.super, isGrey: false);
		if (loadout.charm == Charm.charm_curse)
		{
			item.SetIconsManual("Icons/equip_icon_charm_curse_" + (CharmCurse.CalculateLevel(base.playerID) + 1), isGrey: false, isDLC: true);
		}
		else
		{
			item.SetIcons(loadout.charm, isGrey: false);
		}
	}

	public void Unequip()
	{
		if (icons[index] != weaponA)
		{
			icons[index].SetIcons(WeaponProperties.GetIconPath(Weapon.None));
			if (icons[index] == weaponB)
			{
				PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).secondaryWeapon = Weapon.None;
				if (PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).MustNotifySwitchRegularWeapon)
				{
					PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).HasEquippedSecondaryRegularWeapon = false;
					PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).MustNotifySwitchRegularWeapon = false;
				}
			}
			else if (icons[index] == super)
			{
				PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).super = Super.None;
			}
			else if (icons[index] == item)
			{
				if (PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).charm == Charm.charm_chalice)
				{
					PlayerManager.OnChaliceCharmUnequipped(base.playerID);
				}
				PlayerData.Data.Loadouts.GetPlayerLoadout(base.playerID).charm = Charm.None;
			}
			else
			{
				Debug.LogError("Something went wrong");
			}
		}
		else
		{
			AudioManager.Play("menu_locked");
			cursor.OnLocked();
		}
		Refresh();
		ChangeSelection(0);
	}

	public void ChangeSelection(int direction)
	{
		if ((index != icons.Length - 1 && direction != -1) || (index != 0 && direction != 1))
		{
			AudioManager.Play("menu_equipment_move");
		}
		index = Mathf.Clamp(index + direction, 0, icons.Length - 1);
		SetCursorPosition(index);
		checkListSelected = ((index == icons.Length - 1) ? true : false);
		string empty = string.Empty;
		if (icons[index] == weaponA)
		{
			empty = WeaponProperties.GetDisplayName(loadout.primaryWeapon);
			if (empty.ToUpper() == "ERROR")
			{
				empty = Localization.Translate("level_weapon_none_name").text;
			}
			title.text = empty;
		}
		else if (icons[index] == weaponB)
		{
			empty = WeaponProperties.GetDisplayName(loadout.secondaryWeapon);
			if (empty.ToUpper() == "ERROR")
			{
				empty = Localization.Translate("level_weapon_none_name").text;
			}
			title.text = empty;
		}
		else if (icons[index] == super)
		{
			empty = WeaponProperties.GetDisplayName(loadout.super);
			if (empty.ToUpper() == "ERROR")
			{
				empty = Localization.Translate("level_super_none_name").text;
			}
			title.text = empty;
		}
		else if (icons[index] == item)
		{
			empty = ((loadout.charm != Charm.charm_curse) ? WeaponProperties.GetDisplayName(loadout.charm) : ((CharmCurse.CalculateLevel(base.playerID) == -1) ? Localization.Translate("charm_broken_name").text : ((!CharmCurse.IsMaxLevel(base.playerID)) ? Localization.Translate("charm_curse_name").text : Localization.Translate("charm_paladin_name").text)));
			if (empty.ToUpper() == "ERROR")
			{
				empty = Localization.Translate("charm_none_name").text;
			}
			title.text = empty;
		}
		else
		{
			title.text = Localization.Translate("list_name").text;
		}
		title.font = Localization.Instance.fonts[(int)Localization.language][9].font;
		Outline[] array = outlines;
		foreach (Outline outline in array)
		{
			outline.enabled = Localization.language == Localization.Languages.Japanese;
		}
	}

	private void SetCursorPosition(int index)
	{
		if (icons != null && icons.Length > index)
		{
			cursor.SetPosition(icons[index].transform.position);
		}
	}
}
