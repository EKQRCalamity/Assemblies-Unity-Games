using System.Collections.Generic;
using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Penitent.Attack;
using UnityEngine;

namespace Tools.Items;

public class ItemTemporalEffect : ObjectEffect
{
	public enum PenitentEffects
	{
		StopFervourRecolection,
		Invulnerable,
		RedAttack,
		Level2Attack,
		StopGuiltDrop,
		DisableUnEquipSword
	}

	[SerializeField]
	private List<PenitentEffects> effects = new List<PenitentEffects>();

	private PenitentSword.AttackColor previousColor;

	private int previousAttackLevel;

	public List<PenitentEffects> Effects => effects;

	public bool ContainsEffect(PenitentEffects effect)
	{
		return effects.Contains(effect);
	}

	protected override bool OnApplyEffect()
	{
		if (effects.Contains(PenitentEffects.StopFervourRecolection))
		{
			Core.Logic.Penitent.obtainsFervour = false;
		}
		if (effects.Contains(PenitentEffects.Invulnerable))
		{
			Core.Logic.Penitent.Status.Invulnerable = true;
		}
		if (effects.Contains(PenitentEffects.Level2Attack))
		{
			previousAttackLevel = Core.Logic.Penitent.PenitentAttack.CurrentLevel;
			Core.Logic.Penitent.PenitentAttack.CurrentLevel = 2;
		}
		if (effects.Contains(PenitentEffects.RedAttack))
		{
			previousColor = Core.Logic.Penitent.PenitentAttack.AttackColor;
			Core.Logic.Penitent.PenitentAttack.AttackColor = PenitentSword.AttackColor.Red;
		}
		if (effects.Contains(PenitentEffects.StopGuiltDrop))
		{
			Core.Logic.Penitent.GuiltDrop = false;
		}
		if (effects.Contains(PenitentEffects.DisableUnEquipSword))
		{
			Core.Logic.Penitent.AllowEquipSwords = false;
		}
		ShowDebug("ON");
		return true;
	}

	protected override void OnRemoveEffect()
	{
		if (effects.Contains(PenitentEffects.StopFervourRecolection))
		{
			Core.Logic.Penitent.obtainsFervour = true;
		}
		if (effects.Contains(PenitentEffects.Invulnerable))
		{
			Core.Logic.Penitent.Status.Invulnerable = false;
		}
		if (effects.Contains(PenitentEffects.Level2Attack))
		{
			Core.Logic.Penitent.PenitentAttack.CurrentLevel = previousAttackLevel;
		}
		if (effects.Contains(PenitentEffects.RedAttack))
		{
			Core.Logic.Penitent.PenitentAttack.AttackColor = previousColor;
		}
		if (effects.Contains(PenitentEffects.StopGuiltDrop))
		{
			Core.Logic.Penitent.GuiltDrop = true;
		}
		if (effects.Contains(PenitentEffects.DisableUnEquipSword))
		{
			Core.Logic.Penitent.AllowEquipSwords = true;
		}
		ShowDebug("OFF");
	}

	private void ShowDebug(string status)
	{
		string text = string.Empty;
		foreach (PenitentEffects effect in effects)
		{
			if (text != string.Empty)
			{
				text += ", ";
			}
			text += effect;
		}
		if (base.name != string.Empty)
		{
			Debug.Log("Effects " + status + "  " + text);
		}
	}
}
