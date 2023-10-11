using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField("Process Ability Damage", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class ProcessAbilityDamageAction : ACombatantAction
{
	[ProtoContract(EnumPassthru = true)]
	public enum DamageType
	{
		Dealt,
		Received
	}

	[ProtoMember(1)]
	[UIField]
	[UIHorizontalLayout("Top")]
	private DamageType _processDamageBeing;

	[ProtoMember(2)]
	[UIField(tooltip = "How the ability damage will be affected by <i>Damage Adjustment</i>.")]
	[UIHorizontalLayout("Top")]
	private ProcessDamageFunction _function;

	[ProtoMember(3)]
	[UIField(tooltip = "The amount of change in ability damage.\n<i>The target of this action is considered the owner and their opponent is considered the target.</i>")]
	[UIDeepValueChange]
	private DynamicNumber _damageAdjustment;

	[ProtoMember(4, OverwriteList = true)]
	[UIField(tooltip = "Conditions that must hold true for the action which is dealing damage in order to process damage.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<Filter> _damageActionConditions;

	[ProtoMember(7, OverwriteList = true)]
	[UIField(tooltip = "Conditions that must hold true for ability that was used in order to trigger.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<Condition.AAbility> _abilityConditions;

	[ProtoMember(5, OverwriteList = true)]
	[UIField(tooltip = "Conditions that must hold true for the target of this action in order to process damage.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<Condition.Combatant> _mainCombatantConditions;

	[ProtoMember(6, OverwriteList = true)]
	[UIField(tooltip = "Conditions that must hold true for the opponent of the target of this action in order to process damage.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<Condition.Combatant> _opponentCombatantConditions;

	private AppliedAction _appliedAction;

	private ActionContext _context => _appliedAction.context;

	private bool _increasesDamage
	{
		get
		{
			if (_function != 0 || _damageAdjustment.constantValue < 0)
			{
				if (_function == ProcessDamageFunction.Multiply)
				{
					return _damageAdjustment.constantValue != 0;
				}
				return false;
			}
			return true;
		}
	}

	protected override bool _canTick => false;

	protected override bool _shouldPlayTickMedia => false;

	public override bool processesDamage => true;

	private void _OnProcessAbilityDamage(ActionContext damageContext, AAction damageAction, ref int damage, ref int damageMultiplier, ref int damageDenominator)
	{
		ActionContext context = _context;
		ActionContext actionContext = damageContext;
		Ability ability = damageContext.ability;
		if ((ability != null && ability.itemType.IsCondition() && _increasesDamage) || context.target != damageContext.GetTarget<ACombatant>((ActionContextTarget)_processDamageBeing) || !_damageActionConditions.All(damageContext, damageAction) || !_abilityConditions.All(damageContext.SetTarget(damageContext.ability), !_increasesDamage) || !_mainCombatantConditions.All(context))
		{
			return;
		}
		damageContext = damageContext.SetActorAndTarget(context.target as AEntity, damageContext.GetTarget<ACombatant>((ActionContextTarget)(1 - _processDamageBeing)));
		if (_opponentCombatantConditions.All(damageContext))
		{
			int value = _damageAdjustment.GetValue(damageContext);
			Int3 @int = new Int3(damage, damageMultiplier, damageDenominator);
			switch (_function)
			{
			case ProcessDamageFunction.Add:
				damage += value;
				break;
			case ProcessDamageFunction.Subtract:
				damage -= value;
				break;
			case ProcessDamageFunction.Set:
				damage = value;
				break;
			case ProcessDamageFunction.Multiply:
				damageMultiplier *= value;
				break;
			case ProcessDamageFunction.Divide:
				damageDenominator *= value;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			Int3 int2 = new Int3(damage, damageMultiplier, damageDenominator);
			if (damageContext.gameState.reactToProcessDamage && (bool)base.tickMedia && @int != int2)
			{
				damageContext.gameState.stack.Push(new GameStepActionTickMedia(this, actionContext.SetAbility(context.ability)));
			}
			else if (!damageContext.gameState.reactToProcessDamage && @int != int2)
			{
				damageContext.gameState.view.SignalAbilityProcessingDamage(_appliedAction.context.ability);
			}
		}
	}

	protected override void _Register(AppliedAction appliedAction)
	{
		_appliedAction = appliedAction;
		_context.gameState.onProcessAbilityDamage += _OnProcessAbilityDamage;
	}

	protected override void _Unregister(AppliedAction appliedAction)
	{
		_context.gameState.onProcessAbilityDamage -= _OnProcessAbilityDamage;
	}

	public override IEnumerable<AbilityKeyword> GetKeywords(AbilityData abilityData)
	{
		foreach (AbilityKeyword keyword in base.GetKeywords(abilityData))
		{
			yield return keyword;
		}
		yield return AbilityKeyword.AbilityDamage;
		foreach (AbilityKeyword keyword2 in _abilityConditions.GetKeywords())
		{
			yield return keyword2;
		}
		foreach (AbilityKeyword keyword3 in _mainCombatantConditions.GetKeywords())
		{
			yield return keyword3;
		}
		foreach (AbilityKeyword keyword4 in _opponentCombatantConditions.GetKeywords())
		{
			yield return keyword4;
		}
	}

	protected override string _ToStringUnique()
	{
		return ((!_mainCombatantConditions.IsNullOrEmpty()) ? ("<size=66%>" + _mainCombatantConditions.ToStringSmart(" & ") + " </size>") : "") + ((!_damageActionConditions.IsNullOrEmpty()) ? (((!_mainCombatantConditions.IsNullOrEmpty()) ? "<size=66%>& " : "<size=66%>") + "If action is " + _damageActionConditions.ToStringSmart(" & ") + " </size>") : "") + $"{_function.Sign()}{_damageAdjustment} {_abilityConditions.ToStringSmart().SizeIfNotEmpty().SpaceIfNotEmpty()}Ability damage " + ((_processDamageBeing == DamageType.Dealt) ? "dealt to " : "received from ") + "opponent" + ((!_opponentCombatantConditions.IsNullOrEmpty()) ? (" <size=66%>" + _opponentCombatantConditions.ToStringSmart(" & ") + "</size>") : "") + " for";
	}
}
