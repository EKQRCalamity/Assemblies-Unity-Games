using Framework.FrameworkCore;
using Framework.FrameworkCore.Attributes.Logic;
using Framework.Managers;
using Framework.Penitences;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.Inventory;

public class ObjectEffect_Stat : ObjectEffect
{
	public enum EffectMode
	{
		Bonus,
		Current
	}

	public enum ValueType
	{
		Value,
		BasedOnCurrentStat,
		BasedOnMaxStat
	}

	[BoxGroup("Bonus", true, false, 0)]
	[ShowIf("CheckOnHit", true)]
	public bool UseHitAsBaseValue;

	[BoxGroup("Bonus", true, false, 0)]
	public EffectMode effectMode;

	[BoxGroup("Bonus", true, false, 0)]
	public EntityStats.StatsTypes statType;

	[BoxGroup("Bonus", true, false, 0)]
	public ValueType valueType;

	[HideIf("IsValueType", true)]
	[BoxGroup("Bonus", true, false, 0)]
	public EntityStats.StatsTypes statValueType;

	[BoxGroup("Bonus", true, false, 0)]
	public float value;

	[BoxGroup("Bonus", true, false, 0)]
	public float multiplier = 1f;

	private RawBonus bonus;

	private float currentValue;

	private bool CheckOnHit()
	{
		return effectType == EffectType.OnHitEnemy || effectType == EffectType.OnHitReceived;
	}

	private bool IsValueType()
	{
		return valueType == ValueType.Value;
	}

	protected override void OnAwake()
	{
		if (effectType == EffectType.OnUse && !LimitTime && effectMode == EffectMode.Bonus)
		{
			ShowError("OnUse effect bonus without limittime");
		}
		if (effectType == EffectType.OnHitEnemy && !LimitTime && effectMode == EffectMode.Bonus)
		{
			ShowError("OnHitEnemy effect bonus without limittime");
		}
		if (effectType == EffectType.OnKillEnemy && !LimitTime && effectMode == EffectMode.Bonus)
		{
			ShowError("OnKillEnemy effect bonus without limittime");
		}
		if (effectType == EffectType.OnHitReceived && !LimitTime && effectMode == EffectMode.Bonus)
		{
			ShowError("OnHitReceived effect bonus without limittime");
		}
		if (effectType == EffectType.OnBreakBreakable && !LimitTime && effectMode == EffectMode.Bonus)
		{
			ShowError("OnBreakBreakable effect bonus without limittime");
		}
	}

	protected override bool OnApplyEffect()
	{
		if (AddsLife() && Core.PenitenceManager.GetCurrentPenitence() is PenitencePE02)
		{
			AddTemporalRegenFactor();
			return true;
		}
		EntityStats stats = Core.Logic.Penitent.Stats;
		Attribute byType = stats.GetByType(statType);
		currentValue = value;
		if (CheckOnHit() && UseHitAsBaseValue)
		{
			currentValue += currentHit.DamageAmount;
			Debug.Log("**** PRAYER WITH HIT Damage:" + currentHit.DamageAmount.ToString() + "  Final base:" + currentValue.ToString() + "  Multiplier:" + multiplier);
		}
		if (valueType == ValueType.BasedOnCurrentStat || valueType == ValueType.BasedOnMaxStat)
		{
			VariableAttribute variableAttribute = (VariableAttribute)stats.GetByType(statValueType);
			if (variableAttribute != null)
			{
				if (valueType == ValueType.BasedOnCurrentStat)
				{
					currentValue += variableAttribute.Current;
				}
				else
				{
					currentValue += variableAttribute.CurrentMax;
				}
			}
			else
			{
				ShowError("Variable attr " + statValueType.ToString() + " not found when BasedOnStat");
			}
		}
		if (effectMode == EffectMode.Bonus)
		{
			bonus = new RawBonus(currentValue, multiplier);
			byType.AddRawBonus(bonus);
		}
		else
		{
			currentValue *= multiplier;
			if (byType.IsVariable())
			{
				VariableAttribute variableAttribute2 = (VariableAttribute)byType;
				if (variableAttribute2 != null)
				{
					variableAttribute2.Current += currentValue;
				}
			}
			else
			{
				ShowError("Effect setted as Current needs a variable stat, STAT:" + statType.ToString() + "  OBJECT:" + base.gameObject.name);
			}
		}
		return true;
	}

	protected override void OnRemoveEffect()
	{
		EntityStats stats = Core.Logic.Penitent.Stats;
		Attribute byType = stats.GetByType(statType);
		if (effectMode == EffectMode.Bonus)
		{
			byType.RemoveRawBonus(bonus);
		}
		else
		{
			((VariableAttribute)byType).Current -= currentValue;
		}
	}

	private bool AddsLife()
	{
		return statType == EntityStats.StatsTypes.Life && effectMode == EffectMode.Current && value > 1f;
	}

	private void AddTemporalRegenFactor()
	{
		Core.PenitenceManager.AddFlasksPassiveHealthRegen(value * 0.1f);
		LevelManager.OnLevelLoaded += OnLevelLoaded;
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		Core.PenitenceManager.AddFlasksPassiveHealthRegen(value * -0.1f);
	}
}
