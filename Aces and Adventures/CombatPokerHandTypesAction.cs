using ProtoBuf;

[ProtoContract]
[UIField("Combat Poker Hand Types", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class CombatPokerHandTypesAction : ACombatantAction
{
	[ProtoMember(1)]
	[UIField(tooltip = "Which type of combat to change available poker hand types for.")]
	[UIHorizontalLayout("Top")]
	private CombatPokerHandTypes _combatType;

	[ProtoMember(2)]
	[UIField(tooltip = "Whether to add to available poker hands, or subtract.")]
	[UIHorizontalLayout("Top")]
	private EnumFlagFunction _function;

	[ProtoMember(3)]
	[UIField(tooltip = "Poker hand types that will be added or subtracted.", maxCount = 0)]
	private PokerHandTypes _pokerHands;

	protected override bool _canTick => false;

	protected override void _Apply(ActionContext context, ACombatant combatant)
	{
		combatant.combat[_combatType].AddModification(_function, _pokerHands);
	}

	protected override void _Unapply(ActionContext context, ACombatant combatant)
	{
		combatant.combat[_combatType].RemoveModification(_function, _pokerHands);
	}

	protected override string _ToStringUnique()
	{
		return EnumUtil.FriendlyName(_function) + " " + EnumUtil.FriendlyName(_pokerHands) + " " + ((_function == EnumFlagFunction.Add) ? "To" : "From") + " " + EnumUtil.FriendlyName(_combatType) + " to";
	}
}
