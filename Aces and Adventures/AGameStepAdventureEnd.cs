using System;
using System.Collections;
using System.Collections.Generic;

public abstract class AGameStepAdventureEnd : GameStep
{
	private Dictionary<AdventureDeck, Func<string>> _tooltipMap;

	protected abstract IEnumerable<Couple<DataRef<AdventureData>, Func<string>>> _GetAdventureDecks();

	protected virtual void _OnDeckClick(DeckPile pile, ADeck card)
	{
		if (pile == DeckPile.Select && card is AdventureDeck adventureDeck)
		{
			_WaitForDeckClick(adventureDeck);
			TransitionTo(new GameStepTransitionToNewGameState(adventureDeck.adventureDataRef));
		}
	}

	protected virtual void _OnDeckEnter(DeckPile pile, ADeck card)
	{
		if (pile == DeckPile.Select && card is AdventureDeck key)
		{
			ProjectedTooltipFitter.Create(_tooltipMap[key](), card.view.gameObject, base.view.tooltipCanvas, TooltipAlignment.BottomCenter);
		}
	}

	protected virtual void _OnDeckExit(DeckPile pile, ADeck card)
	{
		ProjectedTooltipFitter.Finish(card.view.gameObject);
	}

	protected virtual void _WaitForDeckClick(AdventureDeck adventureDeck)
	{
		TransitionTo(new GameStepWaitForCardTransition(adventureDeck.view));
	}

	protected virtual void _OnBackPressed()
	{
		TransitionTo(new GameStepAnimateGameStateClear());
		TransitionTo(new GameStepTransitionToNewGameState());
	}

	protected virtual void _OnButtonClick(ButtonCard.Pile pile, ButtonCard card)
	{
		if ((ButtonCardType)card == ButtonCardType.Back && pile == ButtonCard.Pile.Active)
		{
			_OnBackPressed();
		}
	}

	protected override void OnAboutToEnableForFirstTime()
	{
		if (base.state.game.data.unlockAllAdventures || (ProfileManager.progress.experience.read.totalLevel <= 0 && ProfileManager.progress.experience.read.CanLevelUp()))
		{
			_OnBackPressed();
		}
	}

	protected override void OnFirstEnabled()
	{
		base.state.decks.layout.RestoreLayoutToDefault(DeckPile.InactiveSelectAdventure);
		_tooltipMap = _GetAdventureDecks().ToDictionarySafe((Couple<DataRef<AdventureData>, Func<string>> c) => base.state.decks.Add(new AdventureDeck(c), DeckPile.InactiveSelectAdventure) as AdventureDeck, (Couple<DataRef<AdventureData>, Func<string>> c) => c.b);
		base.state.decks.layout.GetLayout(DeckPile.InactiveSelectAdventure).ForceFinishLayoutAnimations();
		if (base.state.stoneDeck.Count() == 0)
		{
			base.state.stoneDeck.Add(new Stone(StoneType.Cancel), Stone.Pile.CancelInactive);
		}
	}

	protected override void OnEnable()
	{
		base.state.decks.layout.onPointerClick += _OnDeckClick;
		base.state.decks.layout.onPointerEnter += _OnDeckEnter;
		base.state.decks.layout.onPointerExit += _OnDeckExit;
		foreach (AdventureDeck key in _tooltipMap.Keys)
		{
			base.state.decks.Transfer(key, DeckPile.Select);
		}
		if (_tooltipMap.IsNullOrEmpty())
		{
			TransitionTo(new GameStepTransitionToNewGameState());
		}
		base.view.buttonDeckLayout.SetActive(ButtonCardType.Back, setActive: true);
		base.view.onBackPressed += _OnBackPressed;
		base.view.buttonDeckLayout.onPointerClick += _OnButtonClick;
	}

	protected override IEnumerator Update()
	{
		while (!base.finished)
		{
			yield return null;
		}
	}

	protected override void OnDisable()
	{
		base.state.decks.layout.onPointerClick -= _OnDeckClick;
		base.state.decks.layout.onPointerEnter -= _OnDeckEnter;
		base.state.decks.layout.onPointerExit -= _OnDeckExit;
		foreach (AdventureDeck key in _tooltipMap.Keys)
		{
			base.state.decks.Transfer(key, DeckPile.InactiveSelectAdventure);
		}
		base.view.buttonDeckLayout.SetActive(ButtonCardType.Back, setActive: false);
		base.view.onBackPressed -= _OnBackPressed;
		base.view.buttonDeckLayout.onPointerClick -= _OnButtonClick;
	}
}
