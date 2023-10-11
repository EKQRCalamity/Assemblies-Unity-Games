using ProtoBuf;

[ProtoContract]
[UIField("Number Of Hero Abilities", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Player", tooltip = "Add or subtract to the number of hero abilities that can be taken by the player for a given turn.")]
public class NumberOfHeroAbilitiesAction : APlayerAction
{
	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide, tooltip = "How many additional hero abilities should be added to the player for a given turn.")]
	private DynamicNumber _adjustment;

	protected override void _Tick(ActionContext context, Player player)
	{
		player.numberOfHeroAbilities.value += _adjustment.GetValue(context);
	}

	protected override string _ToStringUnique()
	{
		return string.Format("+{0} <b>{1}</b> this turn for", _adjustment, "Hero Ability".Pluralize(_adjustment?.constantValue ?? 2));
	}
}
