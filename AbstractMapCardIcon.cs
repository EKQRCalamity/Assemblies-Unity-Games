using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using RektTransform;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class AbstractMapCardIcon : AbstractMonoBehaviour
{
	private const float FRAME_DELAY = 0.07f;

	private static readonly Weapon[] DLCWeapons = new Weapon[3]
	{
		Weapon.level_weapon_crackshot,
		Weapon.level_weapon_upshot,
		Weapon.level_weapon_wide_shot
	};

	private static readonly Charm[] DLCCharms = new Charm[3]
	{
		Charm.charm_chalice,
		Charm.charm_curse,
		Charm.charm_healer
	};

	private static readonly string DefaultAtlas = "Equip_Icons";

	private static readonly string DLCAtlas = "Equip_Icons_DLC";

	[SerializeField]
	private Image iconImage;

	private Sprite[] icons;

	private Sprite[] normalIcons;

	private Sprite[] greyIcons;

	public void SetIcons(Weapon weapon, bool isGrey)
	{
		string atlasName = DefaultAtlas;
		if (Array.IndexOf(DLCWeapons, weapon) > -1)
		{
			atlasName = DLCAtlas;
			Color color = Color.white;
			if (isGrey)
			{
				color = new Color(1f, 1f, 1f, 0.5f);
			}
			iconImage.color = color;
		}
		setIcons(WeaponProperties.GetIconPath(weapon), isGrey, atlasName);
	}

	public void SetIcons(Super super, bool isGrey)
	{
		setIcons(WeaponProperties.GetIconPath(super), isGrey, DefaultAtlas);
	}

	public void SetIcons(Charm charm, bool isGrey)
	{
		string atlasName = DefaultAtlas;
		if (Array.IndexOf(DLCCharms, charm) > -1)
		{
			atlasName = DLCAtlas;
		}
		setIcons(WeaponProperties.GetIconPath(charm), isGrey, atlasName);
	}

	public void SetIconsManual(string iconPath, bool isGrey, bool isDLC = false)
	{
		setIcons(iconPath, isGrey, (!isDLC) ? DefaultAtlas : DLCAtlas);
	}

	private void setIcons(string iconPath, bool isGrey, string atlasName)
	{
		SpriteAtlas cachedAsset = AssetLoader<SpriteAtlas>.GetCachedAsset(atlasName);
		List<Sprite> list = new List<Sprite>();
		string fileName = Path.GetFileName(iconPath);
		Sprite sprite = getSprite(cachedAsset, fileName);
		if (sprite != null)
		{
			list.Add(sprite);
		}
		for (int i = 1; i < 4; i++)
		{
			string text = "_000";
			string fileName2 = Path.GetFileName(iconPath + text + i);
			Sprite sprite2 = getSprite(cachedAsset, fileName2);
			if (!(sprite2 == null))
			{
				list.Add(sprite2);
			}
		}
		normalIcons = list.ToArray();
		list.Clear();
		if (sprite != null)
		{
			list.Add(sprite);
		}
		for (int j = 1; j < 4; j++)
		{
			string text2 = "_grey_000";
			string fileName3 = Path.GetFileName(iconPath + text2 + j);
			Sprite sprite3 = getSprite(cachedAsset, fileName3);
			if (!(sprite3 == null))
			{
				list.Add(sprite3);
			}
		}
		greyIcons = list.ToArray();
		icons = ((!isGrey) ? normalIcons : greyIcons);
		StopAllCoroutines();
		if (iconPath != WeaponProperties.GetIconPath(Weapon.None))
		{
			StartCoroutine(animate_cr());
		}
		else
		{
			SetIcon(icons[0]);
		}
	}

	public void SetIcons(string iconPath)
	{
		SpriteAtlas cachedAsset = AssetLoader<SpriteAtlas>.GetCachedAsset("Equip_Icons");
		List<Sprite> list = new List<Sprite>();
		string fileName = Path.GetFileName(iconPath);
		Sprite sprite = getSprite(cachedAsset, fileName);
		if (sprite != null)
		{
			list.Add(sprite);
		}
		string fileName2 = Path.GetFileName(iconPath);
		Sprite sprite2 = getSprite(cachedAsset, fileName2);
		list.Add(sprite2);
		icons = list.ToArray();
		SetIcon(sprite2);
	}

	public virtual void SelectIcon()
	{
		if (base.animator != null)
		{
			base.animator.Play("Select");
		}
	}

	public virtual void UnselectIcon()
	{
		if (base.animator != null)
		{
			base.animator.Play("Unselect");
		}
	}

	public virtual void OnLocked()
	{
		if (base.animator != null)
		{
			base.animator.Play("Locked");
		}
	}

	private void SetIcon(Sprite sprite)
	{
		if (!(sprite == null))
		{
			iconImage.sprite = sprite;
			iconImage.rectTransform.SetSize(sprite.rect.width, sprite.rect.height);
		}
	}

	private IEnumerator animate_cr()
	{
		int i = 0;
		WaitForSeconds wait = new WaitForSeconds(0.07f);
		SetIcon((icons != null && icons.Length >= 1) ? icons[0] : null);
		while (true)
		{
			yield return wait;
			if (icons == null || icons.Length < 1)
			{
				SetIcon(null);
				continue;
			}
			i++;
			if (i > icons.Length - 1)
			{
				i = 0;
			}
			SetIcon(icons[i]);
		}
	}

	private Sprite getSprite(SpriteAtlas atlas, string spriteName)
	{
		return atlas.GetSprite(spriteName);
	}
}
