using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using Tools.Level.Actionables;
using UnityEngine;

namespace Framework.Inventory;

[RequireComponent(typeof(BaseInventoryObject))]
public class ObjectEffect : MonoBehaviour
{
	public enum EffectType
	{
		OnEquip,
		OnUse,
		OnHitEnemy,
		OnInitialization,
		OnUpdate,
		OnHitReceived,
		OnceOnTimer,
		OnBreakBreakable,
		OnKillEnemy,
		OnAdquisition,
		OnPenitentDead,
		OnAbilityCast
	}

	public enum ConditionType
	{
		WhenLifeUnderPercent,
		WhenExecutionDone,
		WhenHeavyAttackDone,
		WhenDamageReceived,
		WhenNoFlasksLeft
	}

	[Serializable]
	public class Condition
	{
		public ConditionType type;

		[ShowIf("type", ConditionType.WhenLifeUnderPercent, true)]
		public float value;
	}

	[BoxGroup("Type Dependant", true, false, 0)]
	public EffectType effectType;

	[BoxGroup("Type Dependant", true, false, 0)]
	[ValueDropdown("abilityNames")]
	[ShowIf("VisibleAbilityNames", true)]
	public string abilityName;

	private const string abilityNamePrefix = "Gameplay.GameControllers.Penitent.Abilities.";

	[BoxGroup("Type Dependant", true, false, 0)]
	[ShowIf("VisibleTimeLimit", true)]
	public bool LimitTime;

	[BoxGroup("Type Dependant", true, false, 0)]
	[ShowIf("LimitTime", true)]
	public float EffectTime;

	[BoxGroup("Type Dependant", true, false, 0)]
	[ShowIf("LimitTime", true)]
	public bool UsePrayerDurationAddition;

	[BoxGroup("Type Dependant", true, false, 0)]
	[ShowIf("NotVisibleContinuous", true)]
	public bool TriggerOnlyOnce = true;

	[BoxGroup("Type Dependant", true, false, 0)]
	[ShowIf("HitPrayer", true)]
	public bool OnlyWhenUsingPrayer;

	[BoxGroup("Type Dependant", true, false, 0)]
	[ShowIf("HitPrayer", true)]
	[MinValue(0.0)]
	[MaxValue(100.0)]
	public int percentToExecute = 100;

	[BoxGroup("Type Dependant", true, false, 0)]
	[ShowIf("VisibleContinuous", true)]
	public float PingTime = 0.4f;

	[BoxGroup("Type Dependant", true, false, 0)]
	[ShowIf("VisibleOnceOnTimer", true)]
	public float TimeToWait = 3f;

	[BoxGroup("Type Dependant", true, false, 0)]
	[ShowIf("VisibleContinuous", true)]
	public bool UseWhenCastingPrayer = true;

	[BoxGroup("Applying Conditions", true, false, 0)]
	public List<Condition> Conditions = new List<Condition>();

	[BoxGroup("Stopping Conditions", true, false, 0)]
	[ShowIf("VisibleContinuous", true)]
	public List<Condition> StoppingConditions = new List<Condition>();

	[BoxGroup("SFX", true, false, 0)]
	[EventRef]
	public string ActivationFxSound = string.Empty;

	protected BaseInventoryObject InvObj;

	protected bool IsWatingConditions;

	protected Hit currentHit;

	protected BreakableObject breakableObject;

	private float timeLeft;

	private Ability ability;

	private bool abilityIsBeingCasted;

	private bool abilityWasCasted;

	private float prevFrameHealth;

	private List<string> abilityNames => (from myType in Assembly.GetExecutingAssembly().GetTypes()
		where myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Ability))
		select myType into t
		select t.Name).ToList();

	public bool IsApplied { get; private set; }

	public void Awake()
	{
		IsApplied = false;
		IsWatingConditions = false;
		InvObj = GetComponent<BaseInventoryObject>();
		if (effectType == EffectType.OnEquip && !InvObj.IsEquipable())
		{
			ShowError("For OnEquip event you need an equipable object");
		}
		else if (effectType == EffectType.OnUse)
		{
			Prayer component = GetComponent<Prayer>();
			if (!component)
			{
				ShowError("Only Prayers can use OnUse type");
			}
		}
		OnAwake();
	}

	public void Start()
	{
		OnStart();
	}

	private void Update()
	{
		if (!IsApplied)
		{
			return;
		}
		if (effectType == EffectType.OnAbilityCast)
		{
			if (!ability)
			{
				return;
			}
			if (ability.IsUsingAbility && !abilityIsBeingCasted)
			{
				abilityIsBeingCasted = true;
				abilityWasCasted = true;
				if (TriggerOnlyOnce)
				{
					ApplyEffect();
				}
			}
			else if (!ability.IsUsingAbility && abilityIsBeingCasted)
			{
				abilityIsBeingCasted = false;
				if (TriggerOnlyOnce && !LimitTime)
				{
					RemoveEffect();
				}
			}
		}
		OnUpdate();
		if (timeLeft <= 0f || (!UseWhenCastingPrayer && Core.Logic.Penitent.PrayerCast.IsUsingAbility) || (effectType == EffectType.OnAbilityCast && TriggerOnlyOnce && !LimitTime) || (effectType == EffectType.OnAbilityCast && !abilityWasCasted))
		{
			return;
		}
		if (CheckAnyStoppingCondition())
		{
			if (!IsTimeBasedEvent())
			{
				RemoveEffect();
			}
			if (effectType == EffectType.OnAbilityCast)
			{
				abilityWasCasted = false;
			}
			return;
		}
		timeLeft -= Time.deltaTime;
		if (timeLeft <= 0f)
		{
			timeLeft = 0f;
			if (IsTimeBasedEvent())
			{
				ApplyEffect();
			}
			else
			{
				RemoveEffect();
			}
		}
	}

	private void OnDestroy()
	{
		OnDispose();
	}

	private void OnAddInventoryObject()
	{
		if (effectType == EffectType.OnAdquisition && !IsApplied)
		{
			ApplyEffect();
		}
		if (effectType == EffectType.OnInitialization)
		{
			ApplyEffect();
		}
		if (!InvObj.IsEquipable() && IsTimeBasedEvent())
		{
			ApplyEffect();
		}
	}

	private void OnRemoveInventoryObject()
	{
		if (IsApplied && effectType == EffectType.OnInitialization)
		{
			RemoveEffect();
		}
		if (!InvObj.IsEquipable() && IsTimeBasedEvent())
		{
			RemoveEffect();
		}
	}

	private void OnEquipInventoryObject()
	{
		if (effectType == EffectType.OnAbilityCast)
		{
			IsApplied = true;
			if (!TriggerOnlyOnce)
			{
				timeLeft = PingTime;
			}
			Type type = Type.GetType("Gameplay.GameControllers.Penitent.Abilities." + abilityName);
			ability = (Ability)Core.Logic.Penitent.GetComponentInChildren(type, includeInactive: true);
		}
		else if (effectType == EffectType.OnEquip || IsTimeBasedEvent())
		{
			ApplyEffect();
		}
	}

	private void OnUnEquipInventoryObject()
	{
		if (!IsApplied)
		{
			return;
		}
		if (effectType == EffectType.OnAbilityCast)
		{
			IsApplied = false;
			abilityWasCasted = false;
			timeLeft = 0f;
			if (!IsWatingConditions)
			{
				OnRemoveEffect();
			}
		}
		else if (effectType == EffectType.OnEquip || IsTimeBasedEvent())
		{
			RemoveEffect();
		}
	}

	private void OnUseInventoryObject()
	{
		if (effectType == EffectType.OnUse)
		{
			if (IsApplied)
			{
				RemoveEffect();
			}
			ApplyEffect();
		}
	}

	private void OnHitEnemy(Hit hit)
	{
		if (effectType == EffectType.OnHitEnemy && (Core.Logic.Penitent.PrayerCast.IsUsingAbility || !OnlyWhenUsingPrayer))
		{
			if (IsApplied)
			{
				RemoveEffect();
			}
			currentHit = hit;
			ApplyEffect();
		}
	}

	private void OnKillEnemy(Enemy e)
	{
		if (effectType == EffectType.OnKillEnemy && (Core.Logic.Penitent.PrayerCast.IsUsingAbility || !OnlyWhenUsingPrayer))
		{
			if (IsApplied && CheckAllApplyingConditions())
			{
				RemoveEffect();
			}
			ApplyEffect();
		}
	}

	private void OnHitReceived(Hit hit)
	{
		if (effectType == EffectType.OnHitReceived && (Core.Logic.Penitent.PrayerCast.IsUsingAbility || !OnlyWhenUsingPrayer))
		{
			if (IsApplied)
			{
				RemoveEffect();
			}
			currentHit = hit;
			ApplyEffect();
		}
	}

	private void OnPenitentHealthChanged(float life)
	{
		CheckNewEventForConditions();
	}

	private void OnNumberOfCurrentFlasksChanged(float numOfFlasks)
	{
		CheckNewEventForConditions();
	}

	private void OnBreakBreakable(BreakableObject breakable)
	{
		if (effectType == EffectType.OnBreakBreakable && (Core.Logic.Penitent.PrayerCast.IsUsingAbility || !OnlyWhenUsingPrayer))
		{
			if (IsApplied)
			{
				RemoveEffect();
			}
			breakableObject = breakable;
			ApplyEffect();
		}
	}

	private void OnPenitentDead()
	{
		if (effectType == EffectType.OnPenitentDead)
		{
			ApplyEffect();
		}
	}

	protected virtual void OnAwake()
	{
	}

	protected virtual void OnStart()
	{
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual bool OnApplyEffect()
	{
		return false;
	}

	protected virtual void OnRemoveEffect()
	{
	}

	protected virtual void OnDispose()
	{
	}

	protected void ShowError(string error)
	{
		Log.Error("Item Effect", base.gameObject.name + ": " + error);
	}

	private void ApplyEffect()
	{
		if (IsApplied && effectType != EffectType.OnInitialization && effectType != EffectType.OnAbilityCast && !IsTimeBasedEvent() && CheckAllApplyingConditions())
		{
			ShowError("Applied without ending last, ignoring");
			return;
		}
		if (!IsApplied && effectType == EffectType.OnceOnTimer)
		{
			IsApplied = true;
			timeLeft = TimeToWait;
		}
		else
		{
			timeLeft = 0f;
			bool flag = true;
			if (CanBeTriggeredMultiple() && percentToExecute < 100)
			{
				flag = UnityEngine.Random.Range(0, 100) <= percentToExecute;
			}
			if (flag)
			{
				if (CheckAllApplyingConditions())
				{
					IsWatingConditions = false;
					IsApplied = OnApplyEffect();
					if (IsApplied && ActivationFxSound != string.Empty)
					{
						PlayFxSound(ActivationFxSound);
					}
				}
				else
				{
					IsWatingConditions = true;
				}
			}
		}
		if (CanBeTriggeredMultiple() && !LimitTime)
		{
			IsApplied = false;
		}
		if (IsApplied && (effectType == EffectType.OnUpdate || !TriggerOnlyOnce))
		{
			timeLeft = PingTime;
		}
		else if (IsApplied && LimitTime)
		{
			timeLeft = ((!UsePrayerDurationAddition) ? EffectTime : (EffectTime + Core.Logic.Penitent.Stats.PrayerDurationAddition.Final));
		}
	}

	private void PlayFxSound(string eventId)
	{
		if (!string.IsNullOrEmpty(eventId))
		{
			Core.Audio.PlaySfx(eventId);
		}
	}

	public void RemoveEffect(bool force = false)
	{
		if (!IsApplied && !IsWatingConditions && !force)
		{
			ShowError("Remove without apply");
			return;
		}
		if (effectType != EffectType.OnAbilityCast)
		{
			IsApplied = false;
		}
		timeLeft = 0f;
		if (force || !IsWatingConditions)
		{
			OnRemoveEffect();
		}
		IsWatingConditions = false;
	}

	private bool CheckAllApplyingConditions()
	{
		bool flag = true;
		foreach (Condition condition in Conditions)
		{
			switch (condition.type)
			{
			case ConditionType.WhenLifeUnderPercent:
			{
				float num = Core.Logic.Penitent.Stats.Life.Current / Core.Logic.Penitent.Stats.Life.CurrentMax * 100f;
				flag = num <= condition.value;
				break;
			}
			case ConditionType.WhenHeavyAttackDone:
				flag = Core.Logic.Penitent.IsAttackCharged;
				break;
			case ConditionType.WhenExecutionDone:
				flag = Core.Logic.Penitent.IsOnExecution;
				break;
			case ConditionType.WhenDamageReceived:
				flag = Core.Logic.Penitent.Stats.Life.Current < prevFrameHealth;
				prevFrameHealth = Core.Logic.Penitent.Stats.Life.Current;
				break;
			case ConditionType.WhenNoFlasksLeft:
				flag = Core.Logic.Penitent.Stats.Flask.Current <= 0f;
				break;
			}
			if (!flag)
			{
				return flag;
			}
		}
		return flag;
	}

	private bool CheckAnyStoppingCondition()
	{
		bool flag = false;
		foreach (Condition stoppingCondition in StoppingConditions)
		{
			switch (stoppingCondition.type)
			{
			case ConditionType.WhenLifeUnderPercent:
			{
				float num = Core.Logic.Penitent.Stats.Life.Current / Core.Logic.Penitent.Stats.Life.CurrentMax * 100f;
				flag = num <= stoppingCondition.value;
				break;
			}
			case ConditionType.WhenHeavyAttackDone:
				flag = Core.Logic.Penitent.IsAttackCharged;
				break;
			case ConditionType.WhenExecutionDone:
				flag = Core.Logic.Penitent.IsOnExecution;
				break;
			case ConditionType.WhenDamageReceived:
				flag = Core.Logic.Penitent.Stats.Life.Current < prevFrameHealth;
				prevFrameHealth = Core.Logic.Penitent.Stats.Life.Current;
				break;
			case ConditionType.WhenNoFlasksLeft:
				flag = Core.Logic.Penitent.Stats.Flask.Current <= 0f;
				break;
			}
			if (flag)
			{
				return flag;
			}
		}
		return flag;
	}

	private void CheckNewEventForConditions()
	{
		if (IsApplied && !CheckAllApplyingConditions())
		{
			RemoveEffect();
			IsWatingConditions = true;
		}
		else if (IsWatingConditions && CheckAllApplyingConditions())
		{
			IsWatingConditions = false;
			IsApplied = OnApplyEffect();
		}
	}

	private bool VisibleAbilityNames()
	{
		return effectType == EffectType.OnAbilityCast;
	}

	private bool VisibleTimeLimit()
	{
		return !IsTimeBasedEvent() || effectType == EffectType.OnUse;
	}

	private bool VisibleContinuous()
	{
		return effectType == EffectType.OnUpdate || !TriggerOnlyOnce;
	}

	private bool NotVisibleContinuous()
	{
		return effectType != EffectType.OnUpdate;
	}

	private bool VisibleOnceOnTimer()
	{
		return effectType == EffectType.OnceOnTimer;
	}

	private bool HitPrayer()
	{
		return effectType == EffectType.OnHitReceived || effectType == EffectType.OnHitEnemy || effectType == EffectType.OnBreakBreakable || effectType == EffectType.OnKillEnemy;
	}

	private bool IsTimeBasedEvent()
	{
		return effectType == EffectType.OnUpdate || effectType == EffectType.OnceOnTimer || !TriggerOnlyOnce;
	}

	private bool CanBeTriggeredMultiple()
	{
		return effectType == EffectType.OnHitEnemy || effectType == EffectType.OnKillEnemy || effectType == EffectType.OnHitReceived || effectType == EffectType.OnBreakBreakable;
	}
}
