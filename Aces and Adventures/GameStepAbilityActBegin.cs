public class GameStepAbilityActBegin : GameStep
{
	public Ability ability { get; }

	public GameStepAbilityActBegin(Ability ability)
	{
		this.ability = ability;
	}

	public override void Start()
	{
		if (ability.isTrait)
		{
			return;
		}
		ability.view.RequestGlow(ability, Colors.USED, GlowTags.Persistent);
		if (ability.cost.usesCards && ability.owner.resourceDeck.Count(ResourceCard.Pile.ActivationHand) == 0)
		{
			foreach (ResourceCard activationCard in ability.GetActivationCards())
			{
				ability.owner.resourceDeck.Transfer(activationCard, ResourceCard.Pile.ActivationHand);
			}
		}
		ability.owner.abilityDeck.layout.TransferWithSpecialTransitions(ability, Ability.Pile.ActivationHand, null, ability.cost.usesCards, ability.cost.usesCards);
		ability.cost.WildIntoCost(ability.owner.resourceDeck.GetCards(ResourceCard.Pile.ActivationHand));
		(base.group as GameStepGroupAbilityAct)?.SetUsesCards(ability.owner.resourceDeck.Any(ResourceCard.Pile.ActivationHand));
	}
}
