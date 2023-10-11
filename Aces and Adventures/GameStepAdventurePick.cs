using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameStepAdventurePick : AGameStepAdventure
{
	private readonly List<ATarget> _picked = new List<ATarget>();

	private bool _validSelectionExists;

	private int _drawnCount;

	private PoolKeepItemDictionaryHandle<ATarget, int> _originalIndices;

	public int pickCount { get; }

	public bool flexiblePickCount { get; }

	private int _pickedCount => _picked.Count;

	private ButtonCardType _buttonType => ButtonCardType.Finish;

	public GameStepAdventurePick(int pickCount, bool flexiblePickCount, int drawnCount)
	{
		this.pickCount = pickCount;
		this.flexiblePickCount = flexiblePickCount;
		_drawnCount = drawnCount;
	}

	private void _OnConfirmPressed()
	{
		if (flexiblePickCount && !_validSelectionExists)
		{
			base.finished = true;
		}
	}

	private void _OnBackPressed()
	{
		if (flexiblePickCount)
		{
			base.finished = true;
		}
	}

	private void _OnStoneClick(Stone.Pile pile, Stone card)
	{
		if (pile == Stone.Pile.Cancel)
		{
			_OnBackPressed();
		}
	}

	private void _OnAdventureTransfer(ATarget card, AdventureCard.Pile? oldPile, AdventureCard.Pile? newPile)
	{
		if (oldPile == AdventureCard.Pile.SelectionHand)
		{
			card.view.pointerOver.enabled = true;
		}
	}

	private int _GetOrderBy(ATarget card)
	{
		return _originalIndices?.value?.GetValueOrDefault(card) ?? int.MaxValue;
	}

	protected override void _OnAdventureCardSelected(IAdventureCard card)
	{
		if (_FinishTyperIfNeeded(card))
		{
			return;
		}
		if (!_validSelectionExists)
		{
			base.finished = true;
		}
		if (_validSelectionExists && (bool)card.CanBeSelected(base.state).Message())
		{
			_picked.Add(card.adventureCard);
			if (_pickedCount >= pickCount && pickCount == 1)
			{
				End();
			}
			AdventureCard.IsBeingChosen = true;
			base._OnAdventureCardSelected(card);
			AdventureCard.IsBeingChosen = false;
		}
	}

	protected override void OnFirstEnabled()
	{
		_originalIndices = Pools.UseKeepItemDictionary<ATarget, int>();
		int num = 0;
		foreach (ATarget card in base.adventureDeck.GetCards(AdventureCard.Pile.SelectionHand))
		{
			_originalIndices[card] = num++;
		}
	}

	protected override void OnEnable()
	{
		base.state.adventureDeck.onTransfer += _OnAdventureTransfer;
		foreach (ATarget item in base.adventureDeck.GetCardsSafe(AdventureCard.Pile.ActiveHand).AsEnumerable().OrderBy(_GetOrderBy))
		{
			if (item is IAdventureCard adventureCard && adventureCard.pileToTransferToOnSelect != AdventureCard.Pile.ActiveHand && adventureCard.pileToTransferToOnDraw != AdventureCard.Pile.ActiveHand)
			{
				base.adventureDeck.Transfer(item, AdventureCard.Pile.SelectionHand);
				_picked.Remove(item);
			}
		}
		base.OnEnable();
		MapCompassView instance = MapCompassView.Instance;
		if ((object)instance != null)
		{
			instance.canBeActiveWhileCancelIsActive = true;
		}
		if (flexiblePickCount)
		{
			base.view.onConfirmPressed += _OnConfirmPressed;
			if (!MapCompassView.Instance)
			{
				base.view.onBackPressed += _OnBackPressed;
			}
			base.state.stoneDeck.layout.onPointerClick += _OnStoneClick;
			base.view.stoneDeckLayout.SetLayout(Stone.Pile.Cancel, base.view.stoneDeckLayout.cancelFloating);
			base.state.buttonDeck.Layout<ButtonDeckLayout>().Activate(_buttonType);
			base.state.stoneDeck.Layout<StoneDeckLayout>()[StoneType.Cancel].view.RequestGlow(this, Colors.TARGET);
		}
		_validSelectionExists = false;
		foreach (ATarget card in base.adventureDeck.GetCards(AdventureCard.Pile.SelectionHand))
		{
			ATargetView aTargetView = card.view;
			bool flag2 = (card.view.pointerOver.enabled = (bool)(card as IAdventureCard).CanBeSelected(base.state) && (_validSelectionExists = true));
			aTargetView.RequestGlow(this, flag2 ? Colors.TARGET : Colors.FAILURE);
		}
		if (!_validSelectionExists)
		{
			base.state.buttonDeck.Layout<ButtonDeckLayout>()[_buttonType].view.RequestGlow(this, Colors.ACTIVATE);
		}
		int num = pickCount - _pickedCount;
		PickCount count = ((num >= base.adventureDeck.Count(AdventureCard.Pile.SelectionHand)) ? PickCount.All : ((PickCount)num));
		if (pickCount != 1 || _drawnCount != 1)
		{
			base.view.LogMessage(flexiblePickCount ? count.LocalizeFlexible() : count.Localize());
		}
		base.view.wildPiles = ResourceCard.Piles.Hand;
	}

	protected override IEnumerator Update()
	{
		while (_pickedCount < pickCount)
		{
			yield return null;
		}
	}

	protected override void End()
	{
		foreach (ATarget item in base.adventureDeck.GetCardsSafe(AdventureCard.Pile.SelectionHand))
		{
			if (!_picked.Contains(item))
			{
				base.adventureDeck.Discard(item);
			}
		}
	}

	protected override void OnDisable()
	{
		if (flexiblePickCount || pickCount < _drawnCount)
		{
			foreach (ATarget item in base.adventureDeck.GetCardsSafe(AdventureCard.Pile.SelectionHand))
			{
				if (!_picked.Contains(item))
				{
					base.adventureDeck.Transfer(item, AdventureCard.Pile.ActiveHand);
				}
			}
		}
		base.OnDisable();
		MapCompassView instance = MapCompassView.Instance;
		if ((object)instance != null)
		{
			instance.canBeActiveWhileCancelIsActive = false;
		}
		if (flexiblePickCount)
		{
			base.view.onConfirmPressed -= _OnConfirmPressed;
			if (!MapCompassView.Instance)
			{
				base.view.onBackPressed -= _OnBackPressed;
			}
			base.state.stoneDeck.layout.onPointerClick -= _OnStoneClick;
			base.state.buttonDeck.Layout<ButtonDeckLayout>().Deactivate(_buttonType);
			base.view.stoneDeckLayout.RestoreLayoutToDefault(Stone.Pile.Cancel);
		}
		base.state.adventureDeck.onTransfer -= _OnAdventureTransfer;
		base.view.ClearMessage();
	}

	protected override void OnDestroy()
	{
		Pools.Repool(ref _originalIndices);
	}
}
