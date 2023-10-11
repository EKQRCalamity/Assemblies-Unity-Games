using System.Linq;
using UnityEngine.Localization;

public class GameStepDiscardResourceChoice : AGameStepDiscardChoice
{
	protected override int countInSelectHand => base.resourceDeck.Count(ResourceCard.Pile.Hand);

	public GameStepDiscardResourceChoice(int count, DiscardReason reason, bool removeDiscardedFromDeck = false)
		: base(count, reason, removeDiscardedFromDeck)
	{
	}

	private void _OnResourceClick(ResourceCard.Pile pile, ResourceCard card)
	{
		if (pile == ResourceCard.Pile.Hand)
		{
			if (_removeDiscardedFromDeck)
			{
				base.state.exileDeck.Transfer(card, ExilePile.ClearGameState);
				_OnResourceExit(pile, card);
			}
			else
			{
				base.resourceDeck.Discard(card);
			}
			int num = base.count - 1;
			base.count = num;
		}
	}

	protected override LocalizedString _LogCountMessage(DiscardCount discardCount)
	{
		return discardCount.LocalizeResource();
	}

	protected override LocalizedString _LogCountInstruction(DiscardCount discardCount)
	{
		return discardCount.LocalizeResourceInstruction();
	}

	protected override void _DrawMulligan()
	{
		AppendStep(base.state.playerResourceDeck.DrawStep());
	}

	protected override void OnEnable()
	{
		if (base.state.stack.GetSteps().OfType<IdDeck<ResourceCard.Pile, ResourceCard>.GameStepDraw>().Any((IdDeck<ResourceCard.Pile, ResourceCard>.GameStepDraw drawStep) => drawStep.drawTo == ResourceCard.Pile.Hand && drawStep.countRemaining > 0))
		{
			Cancel();
			return;
		}
		base.view.wildPiles = ResourceCard.Piles.Hand;
		base.resourceDeck.layout.onPointerClick += _OnResourceClick;
		base.resourceDeck.layout.SetLayout(ResourceCard.Pile.Hand, base.resourceDeckLayout.select, useSpecialEnterTransitionsForCardsAtRest: true);
		base.OnEnable();
	}

	protected override void End()
	{
		base.resourceDeck.layout.RestoreLayoutToDefault(ResourceCard.Pile.Hand);
		base.End();
	}

	protected override void OnDisable()
	{
		base.resourceDeck.layout.onPointerClick -= _OnResourceClick;
		base.OnDisable();
	}
}
