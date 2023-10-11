using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField("Transfer Ability Card", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Ability Card")]
public class TransferAbilityCardAction : AAbilityAction
{
	[ProtoMember(1)]
	[UIField]
	[DefaultValue(Ability.Pile.Hand)]
	private Ability.Pile _pile = Ability.Pile.Hand;

	protected override void _Tick(ActionContext context, Ability ability)
	{
		ability.deck.Transfer(ability, _pile);
	}

	protected override string _ToStringUnique()
	{
		return "Transfer";
	}

	protected override string _ToStringAfterTarget()
	{
		return " to " + EnumUtil.FriendlyName(_pile);
	}
}
