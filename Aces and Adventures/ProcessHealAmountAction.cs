using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField("Process Heal Amount", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class ProcessHealAmountAction : ACombatantAction
{
	[ProtoMember(1)]
	[UIField]
	[UIHorizontalLayout("Top")]
	[DefaultValue(ProcessAbilityDamageAction.DamageType.Received)]
	private ProcessAbilityDamageAction.DamageType _processHealingBeing = ProcessAbilityDamageAction.DamageType.Received;

	[ProtoMember(2)]
	[UIField(tooltip = "How the heal amount will be affected by <i>Heal Adjustment</i>.")]
	[UIHorizontalLayout("Top")]
	private ProcessDamageFunction _function;

	[ProtoMember(3)]
	[UIField(tooltip = "The amount of change in heal amount.\n<i>The target of this action is considered the owner and the target being healed is considered the target.</i>")]
	[UIDeepValueChange]
	private DynamicNumber _healAdjustment;

	[ProtoMember(4)]
	[UIField(tooltip = "Conditions that must hold true for the action which is healing in order to process heal amount.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<Filter> _healActionConditions;

	[ProtoMember(5, OverwriteList = true)]
	[UIField(tooltip = "Conditions that must hold true for the target of this action in order to process heal amount.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<Condition.Combatant> _mainCombatantConditions;

	[ProtoMember(6, OverwriteList = true)]
	[UIField(tooltip = "Conditions that must hold true for the target being healed in order to process heal amount.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<Condition.Combatant> _targetCombatantConditions;

	private AppliedAction _appliedAction;

	private ActionContext _context => _appliedAction.context;

	protected override bool _canTick => false;

	protected override bool _shouldPlayTickMedia => false;

	private void _OnProcessHealAmount(ActionContext healContext, AAction healAction, ref int heal, ref int healMultiplier, ref int healDenominator, ref AEntity actorOverride)
	{
		ActionContext appliedContext = _context;
		ActionContext actionContext = healContext;
		if (appliedContext.target != healContext.GetTarget<ACombatant>((ActionContextTarget)_processHealingBeing) || (_healActionConditions != null && !_healActionConditions.All((Filter c) => c.IsValid(healContext, healAction))) || (_mainCombatantConditions != null && !_mainCombatantConditions.All((Condition.Combatant c) => c.IsValid(appliedContext))))
		{
			return;
		}
		healContext = healContext.SetActorAndTarget(appliedContext.target as AEntity, healContext.GetTarget<ACombatant>((ActionContextTarget)(1 - _processHealingBeing)));
		if (_targetCombatantConditions == null || _targetCombatantConditions.All((Condition.Combatant c) => c.IsValid(healContext)))
		{
			int value = _healAdjustment.GetValue(healContext);
			Int3 @int = new Int3(heal, healMultiplier, healDenominator);
			switch (_function)
			{
			case ProcessDamageFunction.Add:
				heal += value;
				break;
			case ProcessDamageFunction.Subtract:
				heal -= value;
				break;
			case ProcessDamageFunction.Set:
				heal = value;
				break;
			case ProcessDamageFunction.Multiply:
				healMultiplier *= value;
				break;
			case ProcessDamageFunction.Divide:
				healDenominator *= value;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (@int.y > 0 && healMultiplier < 0 && actorOverride == null)
			{
				actorOverride = _context.actor;
			}
			if (healContext.gameState.reactToProcessHeal && (bool)base.tickMedia && @int != new Int3(heal, healMultiplier, healDenominator))
			{
				healContext.gameState.stack.Push(new GameStepActionTickMedia(this, actionContext.SetAbility(appliedContext.ability)));
			}
		}
	}

	protected override void _Register(AppliedAction appliedAction)
	{
		_appliedAction = appliedAction;
		_context.gameState.onProcessHealAmount += _OnProcessHealAmount;
	}

	protected override void _Unregister(AppliedAction appliedAction)
	{
		_context.gameState.onProcessHealAmount -= _OnProcessHealAmount;
	}

	public override IEnumerable<AbilityKeyword> GetKeywords(AbilityData abilityData)
	{
		foreach (AbilityKeyword keyword in base.GetKeywords(abilityData))
		{
			yield return keyword;
		}
		if (_function == ProcessDamageFunction.Multiply && _healAdjustment.constantValue == 0)
		{
			yield return AbilityKeyword.PreventHealing;
		}
	}

	protected override string _ToStringUnique()
	{
		return ((!_mainCombatantConditions.IsNullOrEmpty()) ? ("<size=66%>" + _mainCombatantConditions.ToStringSmart(" & ") + " </size>") : "") + ((!_healActionConditions.IsNullOrEmpty()) ? (((!_mainCombatantConditions.IsNullOrEmpty()) ? "<size=66%>& " : "<size=66%>") + "If action is " + _healActionConditions.ToStringSmart(" & ") + " </size>") : "") + $"{_function.Sign()}{_healAdjustment} Healing " + ((_processHealingBeing == ProcessAbilityDamageAction.DamageType.Dealt) ? "dealt" : "received") + ((!_targetCombatantConditions.IsNullOrEmpty()) ? (" <size=66%>for " + _targetCombatantConditions.ToStringSmart(" & ") + " target</size>") : "") + " for";
	}
}
