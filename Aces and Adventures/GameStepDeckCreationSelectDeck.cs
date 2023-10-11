using System.Collections;
using System.Linq;

public class GameStepDeckCreationSelectDeck : AGameStepDeckCreation
{
	public class GameStepAnimateSelectedDeck : AGameStepDeckCreation
	{
		private AbilityDeck _deck;

		public GameStepAnimateSelectedDeck(AbilityDeck deck)
		{
			_deck = deck;
		}

		protected override void OnEnable()
		{
		}

		protected override IEnumerator Update()
		{
			base.deckLayout.SetLayout(DeckCreationPile.List, base.creationView.deckOpenLayout);
			base.decks.Transfer(_deck, DeckCreationPile.List);
			base.abilityLayout.SetLayout(DeckCreationPile.List, _deck.deckView.cardLayout);
			_deck.deckView.Open();
			GameStepAddCardsOverTime<DeckCreationPile, Ability> abilityCreationStep = base.abilities.AddOverTimeParallel(from Ability a in _deck.GenerateCards()
				orderby a
				select a, DeckCreationPile.List);
			while (!abilityCreationStep.finished || !_deck.view.atRestInLayout)
			{
				yield return null;
			}
			IEnumerator animation = base.abilityLayout.RestoreLayoutToDefaultAnimated(DeckCreationPile.List, clearExitTransitions: false, clearEnterTransitions: true);
			while (animation.MoveNext())
			{
				yield return null;
			}
			IEnumerator wait = Job.Wait(0.333f);
			while (wait.MoveNext())
			{
				yield return null;
			}
			base.deckLayout.RestoreLayoutToDefault(DeckCreationPile.List);
			_deck.deckView.Close();
		}

		protected override void OnDisable()
		{
		}
	}

	private void _OnDeckClick(DeckCreationPile pile, AbilityDeck deck)
	{
		if (!deck.deckRef)
		{
			deck.abilityDeckView.creation.nameInputField.text = DeckCreationMessage.UnnamedDeck.GetMessage();
		}
		AppendStep(new GameStepAnimateSelectedDeck(deck));
		AppendStep(new GameStepDeckCreationEditDeck());
	}

	private void _OnCharacterPointerEnter(DeckCreationPile pile, Player card)
	{
		if (pile == DeckCreationPile.List)
		{
			card.view.RequestGlow(card.view, Colors.TARGET);
		}
	}

	private void _OnCharacterPointerExit(DeckCreationPile pile, Player card)
	{
		card.view.ReleaseOwnedGlowRequests();
	}

	private void _OnCharacterClick(DeckCreationPile pile, Player card)
	{
		if (pile == DeckCreationPile.List)
		{
			_OnDonePressed();
		}
	}

	private void _OnDestroyRequested(AbilityDeckView deckView)
	{
		base.view.screenCanvasGraphicRaycaster.enabled = true;
		UIUtil.CreatePopup(DeckCreationMessage.DestroyDeck.GetMessage(), UIUtil.CreateMessageBox(DeckCreationMessage.DestroyDeckMessage.Localize().SetArguments(deckView.abilityDeck.deckRef.friendlyName).Localize()), null, parent: base.view.screenCanvasContainer, buttons: new string[2]
		{
			DeckCreationMessage.DestroyDeck.GetMessage(),
			DeckCreationMessage.Cancel.GetMessage()
		}, size: null, centerReferece: null, center: null, pivot: null, onClose: delegate
		{
			base.view.screenCanvasGraphicRaycaster.enabled = false;
		}, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
		{
			if (!(s != DeckCreationMessage.DestroyDeck.GetMessage()))
			{
				deckView.abilityDeck.deckRef.Delete();
				base.exile.Transfer(deckView.abilityDeck, ExilePile.ClearGameState);
				_AddNewDeckIfNeeded();
			}
		});
	}

	private void _OnCopyRequested(AbilityDeckView deckView)
	{
		int num = DataRef<AbilityDeckData>.Search().Count((DataRef<AbilityDeckData> d) => d.belongsToCurrentCreator && ContentRef.Equal(d.data.character, base.creation.selectedCharacter.characterDataRef));
		if (num >= 8)
		{
			base.view.LogError(DeckCreationMessage.MaxDecksAlreadyExist.Localize());
			return;
		}
		if (num == 7)
		{
			AbilityDeck abilityDeck = base.decks.GetCards(DeckCreationPile.Results).FirstOrDefault((AbilityDeck d) => !d.deckRef);
			if (abilityDeck != null)
			{
				base.exile.Transfer(abilityDeck, ExilePile.ClearGameState);
			}
		}
		AbilityDeckData abilityDeckData = ProtoUtil.Clone(deckView.abilityDeck.deckRef.data);
		abilityDeckData.unlockedByDefault = false;
		if (abilityDeckData.name.Length <= 20)
		{
			abilityDeckData.name = "*" + abilityDeckData.name;
		}
		DataRef<AbilityDeckData> dataRef = new DataRef<AbilityDeckData>(abilityDeckData);
		dataRef.Save(forceOverwrite: true).ForceCompletion();
		base.decks.Transfer(base.decks.Add(new AbilityDeck(dataRef)), DeckCreationPile.Results);
	}

	private void _AddNewDeckIfNeeded()
	{
		if (base.decks.Count() < base.pageSize && base.decks.GetCards(DeckCreationPile.Results).None((AbilityDeck d) => !d.deckRef))
		{
			base.decks.Transfer(base.decks.Add(new AbilityDeck(new DataRef<AbilityDeckData>(new AbilityDeckData(DeckCreationMessage.NewDeck.GetMessage(), base.creation.selectedCharacter.characterDataRef)))), DeckCreationPile.Results);
		}
	}

	protected override void OnFirstEnabled()
	{
		foreach (DataRef<AbilityDeckData> item in AbilityDeckData.Search(base.creation.selectedCharacter.characterClass))
		{
			base.decks.Add(new AbilityDeck(item));
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.creationView.title = DeckCreationMessage.SelectDeckTitle.Localize();
		base.creationView.doneText = DeckCreationMessage.Back.Localize();
		foreach (AbilityDeck item in from d in base.decks.GetCardsSafe().AsEnumerable()
			orderby d.deckRef.lastUpdateTime descending
			select d)
		{
			base.decks.Transfer(item, DeckCreationPile.Results);
		}
		_AddNewDeckIfNeeded();
		base.deckLayout.onPointerClick += _OnDeckClick;
		base.characterLayout.onPointerEnter += _OnCharacterPointerEnter;
		base.characterLayout.onPointerExit += _OnCharacterPointerExit;
		base.characterLayout.onPointerClick += _OnCharacterClick;
		AbilityDeckViewCreation.OnDestroyRequested += _OnDestroyRequested;
		AbilityDeckViewCreation.OnCopyRequested += _OnCopyRequested;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		base.decks.TransferPile(DeckCreationPile.Results, DeckCreationPile.Discard);
		base.deckLayout.onPointerClick -= _OnDeckClick;
		base.characterLayout.onPointerEnter -= _OnCharacterPointerEnter;
		base.characterLayout.onPointerExit -= _OnCharacterPointerExit;
		base.characterLayout.onPointerClick -= _OnCharacterClick;
		AbilityDeckViewCreation.OnDestroyRequested -= _OnDestroyRequested;
		AbilityDeckViewCreation.OnCopyRequested -= _OnCopyRequested;
	}

	protected override void OnDestroy()
	{
		foreach (AbilityDeck item in base.decks.GetCardsSafe())
		{
			base.exile.Transfer(item, ExilePile.ClearGameState);
		}
	}
}
