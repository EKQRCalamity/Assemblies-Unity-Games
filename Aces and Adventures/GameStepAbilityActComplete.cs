using System.Linq;

public class GameStepAbilityActComplete : GameStep
{
	public Ability ability { get; }

	public bool interrupted { get; private set; }

	public GameStepAbilityActComplete(Ability ability)
	{
		this.ability = ability;
	}

	protected override void OnFirstEnabled()
	{
		base.state.SignalAbilityAboutToFinish(this);
	}

	public override void Start()
	{
		if (ability.data.type == AbilityData.Type.TriggeredTrait && GetPreviousSteps(GroupType.Context).OfType<GameStepActionTarget>().Any((GameStepActionTarget targetStep) => targetStep.effectedTarget))
		{
			ability.HighlightAbilityName();
		}
		if (ability.isTrait)
		{
			ability.RegisterRefreshTargetEvents();
			return;
		}
		if ((ability.isConsumedOnAct || interrupted) && ability.abilityPile != Ability.Pile.Hand)
		{
			ability.Consume();
		}
		if (ability.isBuff && !interrupted)
		{
			ACombatant target = GetPreviousSteps(GroupType.Context).OfType<GameStepActionTarget>().Reverse().SelectMany((GameStepActionTarget s) => s.targets)
				.OfType<ACombatant>()
				.FirstOrDefault();
			if (target != null && GetPreviousSteps(GroupType.Context).OfType<GameStepActionTarget>().Any((GameStepActionTarget s) => s.effectedTarget && !s.ExcludedTarget(target)) && ability.IsValidBuffTarget(target))
			{
				target.EndApplyBuff(ability);
			}
			else
			{
				ability.Unapply();
				ability.Consume();
			}
		}
		base.state.SignalAbilityUsed(this);
		if (!ability.cost.usesCards)
		{
			GameStepGroupAbilityAct obj = base.group as GameStepGroupAbilityAct;
			if (obj == null || !obj.usesCards)
			{
				goto IL_01b5;
			}
		}
		ability.owner.resourceDeck.TransferPile(ResourceCard.Pile.ActivationHand, ResourceCard.Pile.DiscardPile, clearExitTransitions: true);
		goto IL_01b5;
		IL_01b5:
		if (!interrupted)
		{
			ability.RegisterRefreshTargetEvents();
		}
		if (ability.hasActiveReaction)
		{
			ability.ClearActiveReaction();
		}
	}

	protected override void OnCanceled()
	{
		if (!ability.isTrait)
		{
			ADeckLayout<Ability.Pile, Ability> layout = ability.owner.abilityDeck.layout;
			Ability card = ability;
			Ability.Pile? toPile = ability.actPile;
			bool useSpecialExitTransition = ability.owner.resourceDeck.Any(ResourceCard.Pile.ActivationHand);
			layout.TransferWithSpecialTransitions(card, toPile, null, useSpecialEnterTransition: false, useSpecialExitTransition);
			ability.owner.resourceDeck.TransferPile(ResourceCard.Pile.ActivationHand, ResourceCard.Pile.Hand);
		}
	}

	public void Interrupt()
	{
		if (!(interrupted = true))
		{
			return;
		}
		foreach (GameStep previousStep in GetPreviousSteps())
		{
			previousStep.Cancel();
		}
	}
}
