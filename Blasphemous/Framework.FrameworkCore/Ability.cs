using System.Collections.Generic;
using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.FrameworkCore;

public class Ability : MonoBehaviour
{
	private float timeCount;

	private bool fixedCastScheduled;

	[FoldoutGroup("Fervour Consumption", true, 0)]
	public bool ConsumeFervour;

	[FoldoutGroup("Fervour Consumption", true, 0)]
	[ShowIf("ConsumeFervour", true)]
	public Sword SwordHeart;

	[FoldoutGroup("Fervour Consumption", true, 0)]
	[ShowIf("ConsumeFervour", true)]
	public float MinFervourConsumption = 5f;

	[FoldoutGroup("Fervour Consumption", true, 0)]
	[Range(0f, 100f)]
	[ShowIf("ConsumeFervour", true)]
	public float ReducePercentageByHeart = 5f;

	public MonoBehaviour[] DisableAbilities;

	[SerializeField]
	[FoldoutGroup("Information", true, 0)]
	[TextArea]
	[ReadOnly]
	private string abilityDescription;

	[SerializeField]
	[FoldoutGroup("Information", true, 0)]
	[ReadOnly]
	private string castInformation;

	[SerializeField]
	[FoldoutGroup("Generic Settings", true, 0)]
	private float cooldown;

	[SerializeField]
	[FoldoutGroup("Generic Settings", true, 0)]
	private float castTime;

	[SerializeField]
	[FoldoutGroup("Generic Settings", true, 0)]
	private float castEnergyCost;

	[SerializeField]
	[FoldoutGroup("Generic Settings", true, 0)]
	private string triggerCode;

	[SerializeField]
	[FoldoutGroup("Skills Settings", true, 0)]
	private bool useUnlocableSkill;

	[SerializeField]
	[FoldoutGroup("Skills Settings", true, 0)]
	[ShowIf("useUnlocableSkill", true)]
	[ValueDropdown("SkillsValues")]
	private List<string> unlocableSkill = new List<string>();

	[TutorialId]
	public string TutorialId;

	public Entity EntityOwner { get; private set; }

	protected Animator Animator { get; private set; }

	public string LastUnlockedSkillId { get; protected set; }

	public Player Rewired { get; private set; }

	public bool IsUsingAbility { get; protected set; }

	protected virtual float FervorConsumption
	{
		get
		{
			float num = MinFervourConsumption;
			if ((bool)SwordHeart && Core.InventoryManager.IsSwordEquipped(SwordHeart.id))
			{
				float num2 = MinFervourConsumption * ReducePercentageByHeart / 100f;
				num -= num2;
			}
			return num;
		}
	}

	public bool ReadyToUse => timeCount <= 0f;

	public bool Casting => castTime > 0f;

	protected bool HasEnoughFervour
	{
		get
		{
			if (!ConsumeFervour)
			{
				return true;
			}
			return EntityOwner.Stats.Fervour.Current >= FervorConsumption;
		}
	}

	protected float CastingTime => castTime;

	private IList<string> SkillsValues()
	{
		return null;
	}

	protected UnlockableSkill GetLastUnlockedSkill()
	{
		UnlockableSkill result = null;
		foreach (string item in unlocableSkill)
		{
			if (Core.SkillManager.IsSkillUnlocked(item))
			{
				result = Core.SkillManager.GetSkill(item);
				continue;
			}
			return result;
		}
		return result;
	}

	protected bool CanExecuteSkilledAbility()
	{
		return !useUnlocableSkill || GetLastUnlockedSkill() != null;
	}

	protected void SetCooldown(float newCooldown)
	{
		timeCount = newCooldown;
		cooldown = newCooldown;
	}

	protected virtual void OnAwake()
	{
	}

	protected virtual void OnStart()
	{
		IsUsingAbility = false;
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void OnFixedUpdate()
	{
	}

	protected virtual void OnDamageReceived()
	{
	}

	protected virtual void OnCastStart()
	{
		if (ConsumeFervour)
		{
			EntityOwner.Stats.Fervour.Current -= FervorConsumption;
		}
		IsUsingAbility = true;
	}

	protected virtual void OnCastEnd(float castingTime)
	{
		ToggleAbilities(toggle: true);
		IsUsingAbility = false;
	}

	protected virtual void OnFixedCastStart()
	{
	}

	protected virtual void OnDead()
	{
	}

	protected virtual bool Condition()
	{
		return true;
	}

	protected virtual string Description()
	{
		return abilityDescription;
	}

	protected virtual string CastInformation()
	{
		return castInformation;
	}

	protected virtual void OnAbilityGizmosSelected()
	{
	}

	private void Reset()
	{
		abilityDescription = Description();
	}

	private void OnDrawGizmosSelected()
	{
		OnAbilityGizmosSelected();
	}

	private void Awake()
	{
		castTime = 0f;
		ReloadOwner();
		OnAwake();
	}

	private void Start()
	{
		Rewired = ReInput.players.GetPlayer(0);
		castTime = 0f;
		if ((bool)EntityOwner)
		{
			OnStart();
		}
	}

	private void Update()
	{
		if ((bool)EntityOwner)
		{
			UpdateInput();
			UpdateCooldown();
			OnUpdate();
		}
	}

	private void FixedUpdate()
	{
		OnFixedUpdate();
		if (fixedCastScheduled)
		{
			fixedCastScheduled = false;
			OnFixedCastStart();
		}
	}

	private void UpdateCooldown()
	{
		if (timeCount > 0f)
		{
			timeCount -= Time.deltaTime;
		}
		else if (timeCount < 0f)
		{
			timeCount = 0f;
			OnCooldownFinished();
		}
	}

	private void UpdateInput()
	{
		if ((bool)EntityOwner && Rewired != null && EntityOwner.CompareTag("Penitent"))
		{
			if (Rewired.GetButtonDown(triggerCode))
			{
				Cast();
			}
			if (Rewired.GetButtonUp(triggerCode))
			{
				StopCast();
			}
		}
	}

	public void Cast()
	{
		if (!EntityOwner)
		{
			castInformation = "INVALID OWNER";
			return;
		}
		if (Casting)
		{
			castInformation = "ALREADY CASTING";
			return;
		}
		if (!ReadyToUse)
		{
			castInformation = "ABILITY NOT READY";
			return;
		}
		if (EntityOwner.Status.Dead)
		{
			castInformation = "ENTITY DEAD";
			return;
		}
		if (EntityOwner.Status.AbilitiesDisabled)
		{
			castInformation = "ABILITIES DISABLED";
			return;
		}
		if (!Condition())
		{
			castInformation = "CONDITION NOT MET";
			return;
		}
		castInformation = "SUCCESSFULLY EXECUTED";
		castTime += Time.deltaTime;
		StartCooldown();
		OnCastStart();
		fixedCastScheduled = true;
		ToggleAbilities(toggle: false);
	}

	public void StopCast()
	{
		if (Casting)
		{
			OnCastEnd(castTime);
			castTime = 0f;
		}
	}

	private void StartCooldown()
	{
		timeCount = cooldown;
	}

	private void ReloadOwner()
	{
		EntityOwner = GetComponentInParent<Entity>();
		if (EntityOwner != null)
		{
			Animator = EntityOwner.GetComponentInChildren<Animator>();
		}
	}

	protected virtual void OnCooldownFinished()
	{
	}

	protected virtual void ToggleAbilities(bool toggle)
	{
		MonoBehaviour[] disableAbilities = DisableAbilities;
		foreach (MonoBehaviour monoBehaviour in disableAbilities)
		{
			monoBehaviour.enabled = toggle;
		}
	}
}
