using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField("Process Ignore Shields", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class ProcessIgnoreShieldsAction : ACombatantAction
{
	[ProtoMember(1)]
	[DefaultValue(CombatTypes.Attack)]
	[UIField(tooltip = "The types of combat that will be processed for target of this action.")]
	[UIHorizontalLayout("Top")]
	private CombatTypes _combatTypes = CombatTypes.Attack;

	[ProtoMember(2)]
	[UIField(tooltip = "Optionally specify combat result required to ignore shields. (Relative to target of this action)")]
	[UIHorizontalLayout("Top")]
	[DefaultValue(AttackResultType.Success)]
	private AttackResultType? _combatResult = AttackResultType.Success;

	[ProtoMember(3, OverwriteList = true)]
	[UIField(tooltip = "Conditions that must hold true for the target of this action in order to ignore shields.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<Condition.Combatant> _mainCombatantConditions;

	[ProtoMember(4, OverwriteList = true)]
	[UIField(tooltip = "Conditions that must hold true for combat hand of the target of this action in order to ignore shields.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<ResourceHandFilter> _mainCombatHandFilters;

	[ProtoMember(5, OverwriteList = true)]
	[UIField(tooltip = "Conditions that must hold true for the opponent of the target of this action in order to ignore shields.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<Condition.Combatant> _opponentCombatantConditions;

	[ProtoMember(6, OverwriteList = true)]
	[UIField(tooltip = "Conditions that must hold true for the target of this action's opponent's combat hand.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<ResourceHandFilter> _opponentCombatHandFilters;

	[ProtoMember(7)]
	[UIField(validateOnChange = true, tooltip = "Ignore successful attacks that were triggered via an attack damage action from an ability.")]
	private bool _mustBeRealCombat;

	[ProtoMember(8, OverwriteList = true)]
	[UIField(tooltip = "Conditions that must hold true for the action which is dealing damage in order to ignore shields.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	[UIHideIf("_hideDamageActionConditions")]
	private List<Filter> _damageActionConditions;

	private AppliedAction _appliedAction;

	private ActionContext _context => _appliedAction.context;

	protected override bool _canTick => false;

	private bool _hideDamageActionConditions => _mustBeRealCombat;

	private void _OnShouldIgnoreShields(ActionContext context, AAction abilityAction, int damage, DamageSource source, ref bool? ignoreShields)
	{
		ActiveCombat combat = context.gameState.activeCombat;
		if (combat == null || (_mustBeRealCombat && combat.createdByAction) || (!_damageActionConditions.IsNullOrEmpty() && !combat.createdByAction))
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
		context = _context.SetActorAndTarget(processCombatFor, opponent);
		if ((_combatResult.HasValue && combat.GetResultFor(processCombatFor) != _combatResult.Value) || (_mainCombatantConditions != null && !_mainCombatantConditions.All((Condition.Combatant c) => c.IsValid(context.SetTarget(processCombatFor)))) || (_opponentCombatantConditions != null && !_opponentCombatantConditions.All((Condition.Combatant c) => c.IsValid(context))) || (_mainCombatHandFilters != null && !_mainCombatHandFilters.All((ResourceHandFilter f) => f.IsValidHand(processCombatFor.resourceDeck.GetCards(combat.GetPile(processCombatFor))))) || (_opponentCombatHandFilters != null && !_opponentCombatHandFilters.All((ResourceHandFilter f) => f.IsValidHand(opponent.resourceDeck.GetCards(combat.GetPile(opponent))))))
		{
			return;
		}
		AAction action = combat.action;
		if (action == null || _damageActionConditions.All(context, action))
		{
			bool? flag = ignoreShields;
			ignoreShields = true;
			if ((bool)base.tickMedia && flag != ignoreShields)
			{
				context.gameState.stack.Push(new GameStepActionTickMedia(this, context));
			}
		}
	}

	protected override void _Register(AppliedAction appliedAction)
	{
		_appliedAction = appliedAction;
		_context.gameState.onShouldIgnoreShields += _OnShouldIgnoreShields;
	}

	protected override void _Unregister(AppliedAction appliedAction)
	{
		_context.gameState.onShouldIgnoreShields -= _OnShouldIgnoreShields;
	}

	protected override string _ToStringUnique()
	{
		return (_damageActionConditions.IsNullOrEmpty() ? "" : ("<size=66%>If action is " + _damageActionConditions.ToStringSmart(" & ") + "</size> ")) + ((!_mainCombatantConditions.IsNullOrEmpty()) ? ("<size=66%>" + _mainCombatantConditions.ToStringSmart(" & ") + " </size>") : "") + ((!_mainCombatHandFilters.IsNullOrEmpty()) ? (((!_mainCombatantConditions.IsNullOrEmpty()) ? "<size=66%>& " : "<size=66%>") + _mainCombatHandFilters.ToStringSmart(" & ") + " </size>") : "") + _combatResult.PastTense().SpaceIfNotEmpty() + (_mustBeRealCombat ? "<b>Real</b> " : "") + ((_combatTypes != EnumUtil<CombatTypes>.AllFlags) ? EnumUtil.FriendlyName(_combatTypes) : "Combat") + "s Ignore Shields against opponent" + ((!_opponentCombatantConditions.IsNullOrEmpty()) ? (" <size=66%>" + _opponentCombatantConditions.ToStringSmart(" & ") + "</size>") : "") + ((!_opponentCombatHandFilters.IsNullOrEmpty()) ? (((!_opponentCombatantConditions.IsNullOrEmpty()) ? "<size=66%> & " : " <size=66%>") + _opponentCombatHandFilters.ToStringSmart(" & ") + "</size>") : "") + " for";
	}
}
