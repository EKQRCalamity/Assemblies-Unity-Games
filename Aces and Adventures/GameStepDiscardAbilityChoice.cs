using System.Linq;
using UnityEngine.Localization;

public class GameStepDiscardAbilityChoice : AGameStepDiscardChoice
{
	protected override int countInSelectHand => base.abilityDeck.Count(Ability.Pile.Hand);

	public GameStepDiscardAbilityChoice(int count, DiscardReason reason, bool removeDiscardedFromDeck = false)
		: base(count, reason, removeDiscardedFromDeck)
	{
	}

	private void _OnAbilityClick(Ability.Pile pile, Ability card)
	{
		if (pile == Ability.Pile.Hand)
		{
			if (_removeDiscardedFromDeck)
			{
				base.state.exileDeck.Transfer(card, ExilePile.ClearGameState);
				_OnAbilityExit(pile, card);
			}
			else
			{
				base.abilityDeck.Discard(card);
			}
			int num = base.count - 1;
			base.count = num;
		}
	}

	protected override LocalizedString _LogCountMessage(DiscardCount discardCount)
	{
		return discardCount.LocalizeAbility();
	}

	protected override LocalizedString _LogCountInstruction(DiscardCount discardCount)
	{
		return discardCount.LocalizeAbilityInstruction();
	}

	protected override void _DrawMulligan()
	{
		AppendStep(base.state.abilityDeck.DrawStep());
	}

	protected override void OnEnable()
	{
		if (base.state.stack.GetSteps().OfType<IdDeck<Ability.Pile, Ability>.GameStepDraw>().Any((IdDeck<Ability.Pile, Ability>.GameStepDraw drawStep) => drawStep.drawTo == Ability.Pile.Hand && drawStep.countRemaining > 0))
		{
			Cancel();
			return;
		}
		base.abilityDeck.layout.onPointerClick += _OnAbilityClick;
		base.abilityDeck.layout.SetLayout(Ability.Pile.Hand, base.abilityDeckLayout.select, useSpecialEnterTransitionsForCardsAtRest: true);
		base.OnEnable();
	}

	protected override void End()
	{
		base.abilityDeck.layout.RestoreLayoutToDefault(Ability.Pile.Hand);
		base.End();
	}

	protected override void OnDisable()
	{
		base.abilityDeck.layout.onPointerClick -= _OnAbilityClick;
		base.OnDisable();
	}
}
