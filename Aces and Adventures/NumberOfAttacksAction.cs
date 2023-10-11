using ProtoBuf;

[ProtoContract]
[UIField("Number Of Attacks", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant", tooltip = "Add or subtract to the number of attacks that can be taken by a combatant for a given turn.")]
public class NumberOfAttacksAction : ACombatantAction
{
	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide, tooltip = "How many additional attacks should be added to the combatant for a given turn.")]
	private DynamicNumber _adjustment;

	protected override void _Tick(ActionContext context, ACombatant combatant)
	{
		combatant.numberOfAttacks.value += _adjustment.GetValue(context);
	}

	protected override string _ToStringUnique()
	{
		return string.Format("{0} <b>{1}</b> this turn for", _adjustment, "Attack Action".Pluralize(_adjustment?.constantValue ?? 2));
	}
}
