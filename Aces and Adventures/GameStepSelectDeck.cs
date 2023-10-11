using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class GameStepSelectDeck : GameStep
{
	public class GameStepGroupInspectDeck : GameStepGroup
	{
		private readonly AbilityDeck _deck;

		public GameStepGroupInspectDeck(AbilityDeck deck)
		{
			_deck = deck;
		}

		protected override IEnumerable<GameStep> _GetSteps()
		{
			yield return new GameStepPrepareDeckForInspect(_deck);
			yield return new GameStepCleanupDeckInspect();
		}
	}

	public class GameStepPrepareDeckForInspect : GameStep
	{
		private readonly AbilityDeck _deck;

		public GameStepPrepareDeckForInspect(AbilityDeck deck)
		{
			_deck = deck;
		}

		public override void Start()
		{
			base.view.playerAbilityDeckLayout.SetLayout(Ability.Pile.Draw, base.view.playerAbilityDeckLayout.inspectDraw);
			base.state.abilityDeck.Add(_deck.deckRef.data.abilities.Select((DataRef<AbilityData> a) => new Ability(a, base.state.player, signalAbilityAdded: false)));
		}

		protected override void End()
		{
			AppendStep(base.state.view.InspectAllAbilitiesInDeckStep());
		}
	}

	public class GameStepCleanupDeckInspect : GameStep
	{
		private void _OnAbilityRest(Ability.Pile pile, Ability card)
		{
			if (pile == Ability.Pile.Draw)
			{
				card.view.RepoolCard();
			}
		}

		protected override void OnEnable()
		{
			base.view.playerAbilityDeckLayout.onRest += _OnAbilityRest;
		}

		protected override void OnDisable()
		{
			base.view.playerAbilityDeckLayout.onRest -= _OnAbilityRest;
		}

		protected override IEnumerator Update()
		{
			while (base.state.abilityDeck.Any(Ability.Pile.Draw))
			{
				yield return null;
			}
		}

		protected override void End()
		{
			base.view.playerAbilityDeckLayout.RestoreLayoutToDefault(Ability.Pile.Draw);
		}
	}

	private ACardLayout.DragThresholdData _originalDragThresholds;

	private float _elapsedTime;

	private readonly HashSet<AbilityDeck> _deckOverrides = new HashSet<AbilityDeck>();

	private void _GenerateDecks()
	{
		using PoolKeepItemHashSetHandle<DataRef<AbilityDeckData>> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet(from d in base.state.targets.Values<AbilityDeck>().AsEnumerable()
			select d.deckRef);
		foreach (AdventureData.ASetupInstruction validSetupInstruction in base.state.adventure.data.GetValidSetupInstructions(base.state))
		{
			if (validSetupInstruction is AdventureData.SetupAbilityCards setupAbilityCards && !poolKeepItemHashSetHandle.Contains(setupAbilityCards.deck))
			{
				base.state.decks.Add(_deckOverrides.AddReturn(new AbilityDeck(setupAbilityCards.deck)), DeckPile.InactiveSelectAbility);
			}
		}
		if (_deckOverrides.Count == 0)
		{
			foreach (DataRef<AbilityDeckData> item in from d in AbilityDeckData.Search(base.state.player.characterClass, mustBeValid: true)
				orderby d.lastUpdateTime descending
				select d)
			{
				if (!poolKeepItemHashSetHandle.Contains(item))
				{
					base.state.decks.Add(new AbilityDeck(item), DeckPile.InactiveSelectAbility);
				}
			}
		}
		base.view.decksLayout.GetLayout(DeckPile.InactiveSelectAbility).ForceFinishLayoutAnimations();
	}

	private void _OnBackPressed()
	{
		Cancel();
	}

	private void _OnAdventureClick(AdventureCard.Pile pile, ATarget card)
	{
		if (pile == AdventureCard.Pile.ActiveHand)
		{
			_OnBackPressed();
		}
	}

	private void _OnButtonClick(ButtonCard.Pile pile, ButtonCard card)
	{
		if (pile == ButtonCard.Pile.Active && card.type == ButtonCardType.Back)
		{
			_OnBackPressed();
		}
	}

	private void _OnDeckClick(DeckPile pile, ADeck deck)
	{
		if (pile == DeckPile.Adventure)
		{
			_OnBackPressed();
			base.state.exileDeck.Transfer(base.state.player, ExilePile.Character);
			base.state.player.view.ClearExitTransitions();
			GetPreviousSteps().OfType<GameStepSelectCharacter>().FirstOrDefault()?.Cancel();
		}
		if (pile == DeckPile.Select && deck is AbilityDeck)
		{
			base.state.decks.Transfer(deck, DeckPile.Ability);
			base.state.selectedAbilityDeck = ((AbilityDeck)deck).deckRef;
			AppendStep(new GameStepSetupAdventure());
		}
	}

	private void _OnGameStoneClick(GameStone.Pile pile, GameStone card)
	{
		if (!card.game.CanBeUnlocked())
		{
			base.view.LogError(AdventureTutorial.GameLockedForDemo.Localize());
		}
		else if (!ContentRef.Equal(base.state.game, card.game))
		{
			DataRef<GameData> dataRef = (ProfileManager.prefs.selectedGame = card.game);
			if ((bool)dataRef)
			{
				_OnDeckClick(DeckPile.Adventure, null);
			}
		}
	}

	private void _OnPointerClick(ACardLayout layout, CardLayoutElement card, PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right && !eventData.dragging && card is AbilityDeckView abilityDeckView)
		{
			AppendGroup(new GameStepGroupInspectDeck(abilityDeckView.abilityDeck));
		}
	}

	protected override void OnEnable()
	{
		_GenerateDecks();
		foreach (AbilityDeck item in from d in base.state.targets.Values<AbilityDeck>().AsEnumerable()
			where d.characterClass == base.state.player.characterClass
			orderby d.deckRef.lastUpdateTime descending
			select d)
		{
			base.state.decks.Transfer(item, DeckPile.Select);
		}
		base.state.decks.layout.onPointerClick += _OnDeckClick;
		base.view.onBackPressed += _OnBackPressed;
		base.state.adventureDeck.layout.onPointerClick += _OnAdventureClick;
		base.state.buttonDeck.layout.onPointerClick += _OnButtonClick;
		base.state.gameStoneDeck.layout.onPointerClick += _OnGameStoneClick;
		base.view.buttonDeckLayout.SetActive(ButtonCardType.Back, setActive: true);
		_originalDragThresholds = base.state.adventureDeck.layout.GetLayout(AdventureCard.Pile.ActiveHand).SetDragThresholds(new ACardLayout.DragThresholdData(-0.25f, -0.25f, 0f, 0f, useDragTargetForDragThresholdOrigin: true));
		ACardLayout.OnPointerClick += _OnPointerClick;
	}

	public override void Start()
	{
		if (_deckOverrides.Count == 1)
		{
			_OnDeckClick(DeckPile.Select, _deckOverrides.First());
		}
	}

	protected override IEnumerator Update()
	{
		while (!base.finished)
		{
			yield return null;
		}
	}

	protected override void LateUpdate()
	{
		if (AGameStepTurn.TickTutorialTimer(ref _elapsedTime, 5f))
		{
			base.view.LogMessage(AdventureTutorial.SelectDeck.Localize());
		}
	}

	protected override void OnDisable()
	{
		AGameStepTurn.ResetMessageTimer(ref _elapsedTime);
		base.state.decks.TransferPile(DeckPile.Select, DeckPile.Exile);
		base.state.decks.layout.onPointerClick -= _OnDeckClick;
		base.view.onBackPressed -= _OnBackPressed;
		base.state.adventureDeck.layout.onPointerClick -= _OnAdventureClick;
		base.state.buttonDeck.layout.onPointerClick -= _OnButtonClick;
		base.state.gameStoneDeck.layout.onPointerClick -= _OnGameStoneClick;
		base.view.buttonDeckLayout.SetActive(ButtonCardType.Back, setActive: false);
		_originalDragThresholds.Restore();
		ACardLayout.OnPointerClick -= _OnPointerClick;
	}

	public override void OnCompletedSuccessfully()
	{
		foreach (AbilityDeck item in base.state.decks.GetCardsSafe().AsEnumerable().OfType<AbilityDeck>())
		{
			item.view.DestroyCard();
		}
	}
}
