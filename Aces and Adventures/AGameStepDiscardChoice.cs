using System;
using System.Collections;
using System.Linq;
using UnityEngine.Localization;

public abstract class AGameStepDiscardChoice : GameStep
{
	private static TextBuilder _Builder;

	private int _count;

	private int? _initialCount;

	private DiscardReason _reason;

	protected bool _removeDiscardedFromDeck;

	private PoolKeepItemListHandle<ResourceCard> _previousResourceActivationHand;

	private PoolKeepItemListHandle<Ability> _previousAbilityActivationHand;

	private bool _backButtonEnabled;

	private static TextBuilder Builder => _Builder ?? (_Builder = new TextBuilder(clearOnToString: true));

	protected IdDeck<Ability.Pile, Ability> abilityDeck => base.state.player.abilityDeck;

	protected AbilityDeckLayout abilityDeckLayout => abilityDeck.layout as AbilityDeckLayout;

	protected IdDeck<ResourceCard.Pile, ResourceCard> resourceDeck => base.state.player.resourceDeck;

	protected ResourceDeckLayout resourceDeckLayout => resourceDeck.layout as ResourceDeckLayout;

	private Ability.Piles abilityPiles => Ability.Piles.Hand | Ability.Piles.HeroAct;

	private ResourceCard.Piles resourcePiles => ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand;

	protected int count
	{
		get
		{
			return _count;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _count, value))
			{
				_OnCountChange();
			}
		}
	}

	protected bool _isMulligan => _reason == DiscardReason.Mulligan;

	protected bool backButtonEnabled
	{
		get
		{
			return _backButtonEnabled;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _backButtonEnabled, value))
			{
				_OnBackButtonEnabledChange();
			}
		}
	}

	protected abstract int countInSelectHand { get; }

	protected virtual bool _showBackButton
	{
		get
		{
			if (!_isMulligan)
			{
				if (_reason == DiscardReason.PayingForItem)
				{
					return _count == _initialCount;
				}
				return false;
			}
			return true;
		}
	}

	protected virtual ButtonCardType _buttonType
	{
		get
		{
			if (!_isMulligan)
			{
				return ButtonCardType.Back;
			}
			return ButtonCardType.Finish;
		}
	}

	public override bool canSafelyCancelStack => true;

	protected AGameStepDiscardChoice(int count, DiscardReason reason, bool removeDiscardedFromDeck = false)
	{
		_count = count;
		_reason = reason;
		_removeDiscardedFromDeck = removeDiscardedFromDeck;
	}

	protected virtual void _OnAbilityOver(Ability.Pile pile, Ability card)
	{
		if (!abilityPiles.Contains(pile))
		{
			return;
		}
		foreach (ResourceCard resourceCard in resourceDeck.GetCards(resourcePiles))
		{
			if (card.cost.GetResourceFilters().Any((PlayingCard.Filter t) => (bool)t && t.AreValid(resourceCard)))
			{
				resourceCard.view.RequestGlow(card.view, Colors.CAN_BE_USED);
			}
		}
		if (!card.CanActivate())
		{
			return;
		}
		foreach (ATarget target in card.GetTargets())
		{
			target.view.RequestGlow(card.view, Colors.TARGET);
		}
		PoolKeepItemListHandle<ResourceCard> activationCards = card.GetActivationCards();
		if (activationCards == null)
		{
			return;
		}
		foreach (ResourceCard item in activationCards)
		{
			item.view.RequestGlow(card.view, Colors.USED);
		}
	}

	protected virtual void _OnAbilityExit(Ability.Pile pile, Ability card)
	{
		if (abilityPiles.Contains(pile))
		{
			card.view.ReleaseOwnedGlowRequests();
		}
	}

	protected virtual void _OnResourceOver(ResourceCard.Pile pile, ResourceCard card)
	{
		if (!resourcePiles.Contains(pile))
		{
			return;
		}
		foreach (Ability card2 in abilityDeck.GetCards(Ability.Piles.Hand | Ability.Piles.HeroAct))
		{
			if (card2.cost.GetResourceFilters().Any((PlayingCard.Filter f) => (bool)f && f.AreValid(card)))
			{
				card2.view.RequestGlow(card.view, Colors.CAN_BE_USED);
			}
		}
	}

	protected virtual void _OnResourceExit(ResourceCard.Pile pile, ResourceCard card)
	{
		if (resourcePiles.Contains(pile) || pile == ResourceCard.Pile.DiscardPile)
		{
			card.view.ReleaseOwnedGlowRequests();
		}
	}

	private void _TransferActivationHandsIntoWaitingPiles()
	{
		_previousResourceActivationHand = resourceDeck.TransferPileReturn(ResourceCard.Pile.ActivationHand, ResourceCard.Pile.ActivationHandWaiting);
		_previousAbilityActivationHand = abilityDeck.TransferPileReturn(Ability.Pile.ActivationHand, Ability.Pile.ActivationHandWaiting);
	}

	private void _RestorePreviousActivationHands()
	{
		if ((bool)_previousResourceActivationHand)
		{
			foreach (ResourceCard item in _previousResourceActivationHand)
			{
				resourceDeck.Transfer(item, ResourceCard.Pile.ActivationHand);
			}
		}
		if (!_previousAbilityActivationHand)
		{
			return;
		}
		foreach (Ability item2 in _previousAbilityActivationHand)
		{
			abilityDeck.Transfer(item2, Ability.Pile.ActivationHand);
		}
	}

	private void _OnCountChange()
	{
		DiscardCount discardCount = (DiscardCount)count;
		if (count > 0 && !base.finished)
		{
			base.view.UpdateLogMessage(MessageData.Instance.discard.combined.SetVariables(("Reason", _reason.Localize()), ("Instruction", _isMulligan ? discardCount.LocalizeMulliganInstruction() : _LogCountInstruction(discardCount)), ("Count", _isMulligan ? discardCount.LocalizeMulligan() : _LogCountMessage(discardCount))));
			backButtonEnabled = _showBackButton;
		}
		else
		{
			base.view.ClearMessage();
		}
	}

	private void _OnBackPressed()
	{
		base.finished = true;
		if (!_isMulligan)
		{
			CancelNextSteps(GroupType.Context);
		}
	}

	private void _OnStoneClick(Stone.Pile pile, Stone card)
	{
		if (pile == Stone.Pile.Cancel)
		{
			_OnBackPressed();
		}
	}

	private void _OnBackButtonEnabledChange()
	{
		if (backButtonEnabled)
		{
			base.view.stoneDeckLayout.SetLayout(Stone.Pile.Cancel, base.view.stoneDeckLayout.cancelFloating);
			base.view.buttonDeckLayout.SetActive(_buttonType, setActive: true);
			base.view.stoneDeckLayout[StoneType.Cancel].view.RequestGlow(this, Colors.TARGET);
			if (!MapCompassView.Instance)
			{
				base.view.onBackPressed += _OnBackPressed;
			}
			base.state.stoneDeck.layout.onPointerClick += _OnStoneClick;
		}
		else
		{
			base.view.buttonDeckLayout.SetActive(_buttonType, setActive: false);
			if (!MapCompassView.Instance)
			{
				base.view.onBackPressed -= _OnBackPressed;
			}
			base.state.stoneDeck.layout.onPointerClick -= _OnStoneClick;
			base.view.stoneDeckLayout.RestoreLayoutToDefault(Stone.Pile.Cancel);
		}
	}

	protected abstract LocalizedString _LogCountMessage(DiscardCount discardCount);

	protected abstract LocalizedString _LogCountInstruction(DiscardCount discardCount);

	protected abstract void _DrawMulligan();

	protected override void OnEnable()
	{
		_TransferActivationHandsIntoWaitingPiles();
		abilityDeck.layout.onPointerEnter += _OnAbilityOver;
		abilityDeck.layout.onPointerExit += _OnAbilityExit;
		resourceDeck.layout.onPointerEnter += _OnResourceOver;
		resourceDeck.layout.onPointerExit += _OnResourceExit;
		foreach (Ability card in abilityDeck.GetCards(abilityPiles))
		{
			card.view.RequestGlow(this, Colors.ACTIVATE, card.CanActivate());
		}
		_count = Math.Min(countInSelectHand, _count);
		int valueOrDefault = _initialCount.GetValueOrDefault();
		if (!_initialCount.HasValue)
		{
			valueOrDefault = _count;
			_initialCount = valueOrDefault;
		}
		_OnCountChange();
		AEntity entityTakingTurn = base.state.entityTakingTurn;
		if (entityTakingTurn != null && entityTakingTurn.faction == Faction.Player)
		{
			base.state.stoneDeck.Layout<StoneDeckLayout>().Transfer(StoneType.Turn, Stone.Pile.TurnInactive);
		}
		MapCompassView instance = MapCompassView.Instance;
		if ((object)instance != null)
		{
			instance.canBeActiveWhileCancelIsActive = true;
		}
		backButtonEnabled = _showBackButton;
		base.view.wildPiles = ResourceCard.Piles.Hand;
	}

	protected override IEnumerator Update()
	{
		while (_count > 0 && countInSelectHand > 0)
		{
			yield return null;
		}
	}

	protected override void OnDisable()
	{
		_RestorePreviousActivationHands();
		abilityDeck.layout.onPointerEnter -= _OnAbilityOver;
		abilityDeck.layout.onPointerExit -= _OnAbilityExit;
		resourceDeck.layout.onPointerEnter -= _OnResourceOver;
		resourceDeck.layout.onPointerExit -= _OnResourceExit;
		_OnCountChange();
		MapCompassView instance = MapCompassView.Instance;
		if ((object)instance != null)
		{
			instance.canBeActiveWhileCancelIsActive = false;
		}
		backButtonEnabled = false;
	}

	protected override void End()
	{
		if (!_isMulligan)
		{
			return;
		}
		int? num = _initialCount - _count;
		if (num.HasValue)
		{
			for (int i = 0; i < num.Value; i++)
			{
				_DrawMulligan();
			}
		}
	}
}
