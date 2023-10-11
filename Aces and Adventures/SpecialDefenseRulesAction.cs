using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField("Special Defense Rules", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class SpecialDefenseRulesAction : ACombatantAction
{
	[ProtoMember(1)]
	[UIField(tooltip = "Optionally define which attack hand types will have special defense rules applied against them.", maxCount = 0)]
	private PokerHandTypes? _attackHandTypes;

	[ProtoMember(2)]
	[UIField(tooltip = "Additional defense hand types that can be used regardless of attack hand size.", maxCount = 0)]
	private PokerHandTypes _defenseHandTypes;

	[ProtoMember(3)]
	[UIField(tooltip = "Which combat type these rules are applied on.")]
	[DefaultValue(CombatType.Defense)]
	private CombatType _combatType = CombatType.Defense;

	private AppliedAction _appliedAction;

	private ActionContext _context => _appliedAction.context;

	protected override bool _canTick => false;

	private void _OnProcessDefenseRules(ACombatant attacker, ACombatant defender, PokerHand attackHand, ref PokerHandTypes defenseHands)
	{
		if (((_combatType == CombatType.Defense) ? defender : attacker) == _context.target && (!_attackHandTypes.HasValue || EnumUtil.HasFlagConvert(_attackHandTypes.Value, attackHand.type)) && defenseHands != EnumUtil.Add(ref defenseHands, _defenseHandTypes) && (bool)base.tickMedia)
		{
			_context.gameState.stack.Push(new GameStepActionTickMedia(this, _context));
		}
	}

	protected override void _Register(AppliedAction appliedAction)
	{
		_appliedAction = appliedAction;
		_context.gameState.onProcessDefenseRules += _OnProcessDefenseRules;
	}

	protected override void _Unregister(AppliedAction appliedAction)
	{
		_context.gameState.onProcessDefenseRules -= _OnProcessDefenseRules;
	}

	protected override string _ToStringUnique()
	{
		return "Allow " + ((_combatType == CombatType.Defense) ? "defending" : "opponent to defend") + " with " + EnumUtil.FriendlyNameFlagRanges(_defenseHandTypes).SizeIfNotEmpty() + " Hand(s) against " + (_attackHandTypes.HasValue ? EnumUtil.FriendlyNameFlagRanges(_attackHandTypes.Value).SizeIfNotEmpty() : "All") + " Hand(s) for";
	}
}
