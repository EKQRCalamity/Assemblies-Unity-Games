using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField("Process Combat Damage", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class ProcessCombatDamageAction : ACombatantAction
{
	[ProtoMember(1)]
	[DefaultValue(CombatTypes.Attack)]
	[UIField(tooltip = "The types of combat that will be processed for target of this action.")]
	[UIHorizontalLayout("Top")]
	private CombatTypes _combatTypes = CombatTypes.Attack;

	[ProtoMember(2)]
	[UIField(tooltip = "Optionally specify combat result required to process damage. (Relative to target of this action)")]
	[UIHorizontalLayout("Top")]
	private AttackResultType? _combatResult;

	[ProtoMember(3)]
	[UIField(tooltip = "How the combat damage will be affected by <i>Damage Adjustment</i>.")]
	[UIHorizontalLayout("Top")]
	private ProcessDamageFunction _function;

	[ProtoMember(4)]
	[UIField(tooltip = "The amount of change in combat damage.\n<i>The target of this action is considered the owner and their opponent is considered the target.</i>")]
	[UIDeepValueChange]
	private DynamicNumber _damageAdjustment;

	[ProtoMember(5, OverwriteList = true)]
	[UIField(tooltip = "Conditions that must hold true for the target of this action in order to process damage.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<Condition.Combatant> _mainCombatantConditions;

	[ProtoMember(6, OverwriteList = true)]
	[UIField(tooltip = "Conditions that must hold true for combat hand of the target of this action in order to process damage.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<ResourceHandFilter> _mainCombatHandFilters;

	[ProtoMember(7, OverwriteList = true)]
	[UIField(tooltip = "Conditions that must hold true for the opponent of the target of this action in order to process damage.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<Condition.Combatant> _opponentCombatantConditions;

	[ProtoMember(8, OverwriteList = true)]
	[UIField(tooltip = "Conditions that must hold true for the target of this action's opponent's combat hand.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<ResourceHandFilter> _opponentCombatHandFilters;

	[ProtoMember(9)]
	[UIField(validateOnChange = true, tooltip = "Ignore successful attacks that were triggered via an attack damage action from an ability.")]
	private bool _mustBeRealCombat;

	[ProtoMember(10, OverwriteList = true)]
	[UIField(tooltip = "Conditions that must hold true for the action which is dealing damage in order to process damage.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	[UIHideIf("_hideDamageActionConditions")]
	private List<Filter> _damageActionConditions;

	private AppliedAction _appliedAction;

	private ActionContext _context => _appliedAction.context;

	protected override bool _canTick => false;

	protected override bool _shouldPlayTickMedia => false;

	public override bool processesDamage => true;

	private bool _hideDamageActionConditions => _mustBeRealCombat;

	private void _OnProcessCombatDamage(ActiveCombat combat)
	{
		if ((_mustBeRealCombat && combat.createdByAction) || (!_damageActionConditions.IsNullOrEmpty() && !combat.createdByAction))
		{
			return;
		}
		ACombatant processCombatFor = _context.GetTarget<ACombatant>();
		CombatType? combatType = combat.GetCombatType(processCombatFor);
		if (!combatType.HasValue || !EnumUtil.HasFlagConvert(_combatTypes, combatType.Value))
		{
			return;
		}
		ACombatant opponent = combat.GetOpponent(processCombatFor);
		ActionContext context = _context.SetActorAndTarget(processCombatFor, opponent);
		if ((_combatResult.HasValue && combat.GetResultFor(processCombatFor) != _combatResult.Value) || (_mainCombatantConditions != null && !_mainCombatantConditions.All((Condition.Combatant c) => c.IsValid(context.SetTarget(processCombatFor)))) || (_opponentCombatantConditions != null && !_opponentCombatantConditions.All((Condition.Combatant c) => c.IsValid(context))) || (_mainCombatHandFilters != null && !_mainCombatHandFilters.All((ResourceHandFilter f) => f.IsValidHand(processCombatFor.resourceDeck.GetCards(combat.GetPile(processCombatFor))))) || (_opponentCombatHandFilters != null && !_opponentCombatHandFilters.All((ResourceHandFilter f) => f.IsValidHand(opponent.resourceDeck.GetCards(combat.GetPile(opponent))))))
		{
			return;
		}
		AAction action = combat.action;
		if (action != null && !_damageActionConditions.All(context, action))
		{
			return;
		}
		int value = _damageAdjustment.GetValue(context);
		ActiveCombat.DamageData damageData = combat.damageData;
		switch (_function)
		{
		case ProcessDamageFunction.Add:
			combat.damage += value;
			break;
		case ProcessDamageFunction.Subtract:
			combat.damage -= value;
			break;
		case ProcessDamageFunction.Set:
			combat.damage = value;
			break;
		case ProcessDamageFunction.Multiply:
			if (value != 0)
			{
				combat.damageMultiplier *= value;
			}
			else
			{
				combat.zeroDamage = true;
			}
			break;
		case ProcessDamageFunction.Divide:
			combat.damageDenominator *= value;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (context.gameState.reactToProcessDamage && combat.resultIsFinal && (bool)base.tickMedia && damageData != combat.damageData && (combatType == CombatType.Attack || combat.result == AttackResultType.Success || !combat.attacker.statuses.safeAttack))
		{
			context.gameState.stack.Push(new GameStepActionTickMedia(this, context));
		}
		else if (!context.gameState.reactToProcessDamage && damageData != combat.damageData)
		{
			context.gameState.view.SignalAbilityProcessingDamage(_appliedAction.context.ability);
		}
	}

	protected override void _Register(AppliedAction appliedAction)
	{
		_appliedAction = appliedAction;
		_context.gameState.onProcessCombatDamage += _OnProcessCombatDamage;
	}

	protected override void _Unregister(AppliedAction appliedAction)
	{
		_context.gameState.onProcessCombatDamage -= _OnProcessCombatDamage;
	}

	public override IEnumerable<AbilityKeyword> GetKeywords(AbilityData abilityData)
	{
		foreach (AbilityKeyword keyword in base.GetKeywords(abilityData))
		{
			yield return keyword;
		}
		if (_combatTypes == EnumUtil<CombatTypes>.AllFlags)
		{
			yield return AbilityKeyword.CombatDamageModifier;
			if (_mustBeRealCombat)
			{
				yield return AbilityKeyword.StandardCombat;
			}
		}
		else
		{
			AbilityKeyword type = (((_combatTypes == CombatTypes.Attack) ^ (_combatResult == AttackResultType.Failure)) ? AbilityKeyword.AttackDamageModifier : AbilityKeyword.DefenseDamageModifier);
			yield return type;
			if (_mustBeRealCombat && type == AbilityKeyword.AttackDamageModifier)
			{
				yield return AbilityKeyword.StandardAttack;
			}
		}
		foreach (AbilityKeyword keyword2 in _mainCombatantConditions.GetKeywords())
		{
			yield return keyword2;
		}
		foreach (AbilityKeyword keyword3 in _opponentCombatantConditions.GetKeywords())
		{
			yield return keyword3;
		}
	}

	protected override string _ToStringUnique()
	{
		return (_damageActionConditions.IsNullOrEmpty() ? "" : ("<size=66%>If action is " + _damageActionConditions.ToStringSmart(" & ") + "</size> ")) + ((!_mainCombatantConditions.IsNullOrEmpty()) ? ("<size=66%>" + _mainCombatantConditions.ToStringSmart(" & ") + " </size>") : "") + ((!_mainCombatHandFilters.IsNullOrEmpty()) ? (((!_mainCombatantConditions.IsNullOrEmpty()) ? "<size=66%>& " : "<size=66%>") + _mainCombatHandFilters.ToStringSmart(" & ") + " </size>") : "") + string.Format("{0}{1} {2}{3}{4} damage ", _function.Sign(), _damageAdjustment, _combatResult.PastTense().SpaceIfNotEmpty(), _mustBeRealCombat ? "<b>Real</b> " : "", (_combatTypes != EnumUtil<CombatTypes>.AllFlags) ? EnumUtil.FriendlyName(_combatTypes) : "Combat") + "against opponent" + ((!_opponentCombatantConditions.IsNullOrEmpty()) ? (" <size=66%>" + _opponentCombatantConditions.ToStringSmart(" & ") + "</size>") : "") + ((!_opponentCombatHandFilters.IsNullOrEmpty()) ? (((!_opponentCombatantConditions.IsNullOrEmpty()) ? "<size=66%> & " : " <size=66%>") + _opponentCombatHandFilters.ToStringSmart(" & ") + "</size>") : "") + " for";
	}
}
