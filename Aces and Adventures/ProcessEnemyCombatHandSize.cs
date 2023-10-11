using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField("Process Enemy Combat Hand Size", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant", tooltip = "Allows changing the number of cards an enemy draws while doing combat.\n<i>Should only be applied on enemies.</i>")]
public class ProcessEnemyCombatHandSize : ACombatantAction
{
	[ProtoMember(1)]
	[UIField]
	private CombatType _hand;

	[ProtoMember(2)]
	[UIField(tooltip = "How the enemy's combat hand size will be affected by <i>Adjustment</i>.")]
	[UIHorizontalLayout("Top")]
	[DefaultValue(ProcessDamageFunction.Set)]
	private ProcessDamageFunction _function = ProcessDamageFunction.Set;

	[ProtoMember(3)]
	[UIField(tooltip = "The amount of change in enemy's combat hand size.\n<i>The attacker is considered the owner, and the defender the target.</i>")]
	[UIDeepValueChange]
	private DynamicNumber _adjustment;

	[ProtoMember(4, OverwriteList = true)]
	[UIField(tooltip = "Conditions that must hold true for the attacker in order to process enemy's combat hand size.\n<i>Attacker is considered the target, and defender the owner.</i>")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<Condition.Combatant> _attackerConditions;

	[ProtoMember(5, OverwriteList = true)]
	[UIField(tooltip = "Conditions that must hold true for the defender in order to process enemy's combat hand size.\n<i>Defender is considered the target, and attacker the owner.</i>")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<Condition.Combatant> _defenderConditions;

	private AppliedAction _appliedAction;

	protected override bool _canTick => false;

	private void _OnProcessEnemyCombatHandSize(ACombatant attacker, ACombatant defender, ref int effectiveStat, bool shouldTriggerMedia)
	{
		if (((_hand == CombatType.Attack) ? attacker : defender) != _appliedAction.context.target)
		{
			return;
		}
		ActionContext context = new ActionContext(attacker, _appliedAction.context.ability, defender);
		context.SetCapturedValue(effectiveStat);
		if (_attackerConditions.All(context.SetActorAndTarget(defender, attacker)) && _defenderConditions.All(context.SetActorAndTarget(attacker, defender)))
		{
			int value = _adjustment.GetValue(context);
			int num = effectiveStat;
			switch (_function)
			{
			case ProcessDamageFunction.Set:
				effectiveStat = value;
				break;
			case ProcessDamageFunction.Add:
				effectiveStat += value;
				break;
			case ProcessDamageFunction.Subtract:
				effectiveStat -= value;
				break;
			case ProcessDamageFunction.Multiply:
				effectiveStat *= value;
				break;
			}
			if (shouldTriggerMedia && (bool)base.tickMedia && num != effectiveStat)
			{
				context.gameState.stack.Push(new GameStepActionTickMedia(this, context));
			}
		}
	}

	protected override void _Register(AppliedAction appliedAction)
	{
		(_appliedAction = appliedAction).context.gameState.onProcessEnemyCombatStat += _OnProcessEnemyCombatHandSize;
	}

	protected override void _Unregister(AppliedAction appliedAction)
	{
		appliedAction.context.gameState.onProcessEnemyCombatStat -= _OnProcessEnemyCombatHandSize;
	}

	protected override string _ToStringUnique()
	{
		return (_attackerConditions.IsNullOrEmpty() ? "" : ("<size=66%>(Attacker: " + _attackerConditions.ToStringSmart(" & ") + ") </size>")) + (_defenderConditions.IsNullOrEmpty() ? "" : ("<size=66%>(Defender: " + _defenderConditions.ToStringSmart(" & ") + ") </size>")) + $"{EnumUtil.FriendlyName(_hand)} Hand Size {_function.Sign()} {_adjustment} for";
	}
}
