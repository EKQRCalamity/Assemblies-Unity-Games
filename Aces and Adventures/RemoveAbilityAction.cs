using ProtoBuf;

[ProtoContract]
[UIField("Remove Ability", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Discards a target ability card without presenting a choice.\n<i>Can be used to discard item cards.</i>", category = "Ability Card")]
public class RemoveAbilityAction : AAbilityAction
{
	protected override void _Tick(ActionContext context, Ability ability)
	{
		ability.Unapply();
		if (ability is ItemCard card)
		{
			context.gameState.adventureDeck.Transfer(card, AdventureCard.Pile.Discard);
		}
		else
		{
			ability.owner.abilityDeck.Discard(ability);
		}
	}

	protected override string _ToStringUnique()
	{
		return "Remove";
	}
}
