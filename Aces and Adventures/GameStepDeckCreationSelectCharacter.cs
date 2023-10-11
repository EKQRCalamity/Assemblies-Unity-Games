using System;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;

public class GameStepDeckCreationSelectCharacter : AGameStepDeckCreation
{
	public override bool canInspect => true;

	private void _OnCharacterPointerEnter(DeckCreationPile pile, Player card)
	{
		card.view.RequestGlow(card.view, Colors.TARGET);
	}

	private void _OnCharacterPointerExit(DeckCreationPile pile, Player card)
	{
		card.view.ReleaseOwnedGlowRequests();
	}

	private void _OnCharacterClick(DeckCreationPile pile, Player card)
	{
		base.characters.Transfer(card, DeckCreationPile.List);
		AppendStep(new GameStepDeckCreationSelectDeck());
	}

	private void _OnExileRest(ExilePile pile, ATarget card)
	{
		card.view.RepoolCard();
	}

	private void _OnDeckTransfer(AbilityDeck deck, DeckCreationPile? oldPile, DeckCreationPile? newPile)
	{
		AbilityDeckViewCreation abilityDeckViewCreation = deck.abilityDeckView.creation;
		abilityDeckViewCreation.deleteInputEnabled = newPile == DeckCreationPile.Results && (bool)deck.deckRef;
		abilityDeckViewCreation.copyInputEnabled = abilityDeckViewCreation.deleteInputEnabled;
		abilityDeckViewCreation.showCardCount = abilityDeckViewCreation.deleteInputEnabled && !deck.deckRef.data.isValid;
		if (abilityDeckViewCreation.showCardCount)
		{
			abilityDeckViewCreation.cardCount = deck.count;
		}
		abilityDeckViewCreation.nameInputEnabled = newPile == DeckCreationPile.List;
	}

	private void _OnAbilityTransfer(Ability card, DeckCreationPile? oldPile, DeckCreationPile? newPile)
	{
		if (newPile == DeckCreationPile.List)
		{
			card.view.pointerClick.OnMiddleClick.AddListener(_OnAbilityMiddleClick);
		}
		else if (oldPile == DeckCreationPile.List)
		{
			card.view.pointerClick.OnMiddleClick.RemoveListener(_OnAbilityMiddleClick);
		}
	}

	private void _OnAbilityMiddleClick(PointerEventData eventData)
	{
		DataRef<AbilityData> ability;
		PoolKeepItemListHandle<DataRef<AbilityData>> upgradeHierarchy;
		using (PoolKeepItemListHandle<Ability> poolKeepItemListHandle = base.abilities.GetCardsSafe(DeckCreationPile.List))
		{
			Ability ability2 = eventData.pointerPress.GetComponentInParent<AbilityCardView>().ability;
			int num = poolKeepItemListHandle.value.IndexOfStartFromLast(ability2, AbilityEqualityComparer.Default) + 1;
			ability = ability2.dataRef;
			DataRef<AbilityData> b = ability;
			upgradeHierarchy = ability.GetUpgradeHierarchy();
			try
			{
				if (base.abilities.Count(DeckCreationPile.List) >= 30)
				{
					FindUpgrade();
					if (ContentRef.Equal(ability, b))
					{
						base.view.LogError(DeckCreationMessage.DeckIsFull.Localize());
						return;
					}
					ability2.view.deck.SignalPointerClick(ability2);
					num--;
				}
				if (!CanBeAdded(ability) && (bool)(ability = ability.BaseAbilityRef()))
				{
					FindUpgrade();
				}
				int num2 = CountInList(ability);
				int num3 = UnlockedCount(ability);
				int num4 = MaxCount(ability);
				if (num2 >= num4)
				{
					base.view.LogError(DeckCreationMessage.MaxCopiesOfCardInDeck.Localize());
					return;
				}
				if (num2 >= num3)
				{
					base.view.LogError(DeckCreationMessage.NoMoreCopiesOfCardFound.Localize());
					return;
				}
				base.abilities.Transfer(base.abilities.GetCards(DeckCreationPile.Results).FirstOrDefault((Ability a) => ContentRef.Equal(a.dataRef, ability) && a.view.inputEnabled) ?? base.abilities.Add(new Ability(ability), DeckCreationPile.Discard), DeckCreationPile.List, num);
			}
			finally
			{
				if (upgradeHierarchy != null)
				{
					((IDisposable)upgradeHierarchy).Dispose();
				}
			}
		}
		bool CanBeAdded(DataRef<AbilityData> abilityRef)
		{
			return CountInList(abilityRef) < UnlockedCount(abilityRef);
		}
		int CountInList(DataRef<AbilityData> abilityRef)
		{
			return base.abilities.GetCards(DeckCreationPile.List).Count((Ability a) => ContentRef.Equal(a.dataRef, abilityRef));
		}
		void FindUpgrade()
		{
			foreach (DataRef<AbilityData> item in upgradeHierarchy.value)
			{
				if (item.data.rank > ability.data.rank && CanBeAdded(item))
				{
					ability = item;
					break;
				}
			}
		}
		static int MaxCount(DataRef<AbilityData> abilityRef)
		{
			return abilityRef.data.rank.Max();
		}
		static int UnlockedCount(DataRef<AbilityData> abilityRef)
		{
			return ProfileManager.progress.abilities.read.Count(abilityRef, DevData.Unlocks.abilities);
		}
	}

	protected override void _OnBackPressed()
	{
	}

	protected override void OnFirstEnabled()
	{
		foreach (DataRef<CharacterData> unlockedCharacter in ProfileManager.progress.characters.read.GetUnlockedCharacters(DevData.Unlocks.characters))
		{
			base.characters.Add(new Player(unlockedCharacter));
		}
		base.exileLayout.onRest += _OnExileRest;
		base.decks.onTransfer += _OnDeckTransfer;
		base.abilities.onTransfer += _OnAbilityTransfer;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.creationView.title = DeckCreationMessage.SelectCharacterTitle.Localize();
		base.creationView.doneText = DeckCreationMessage.Exit.Localize();
		foreach (Player item in from c in base.characters.GetCardsSafe().AsEnumerable()
			orderby c.name
			select c)
		{
			base.characters.Transfer(item, DeckCreationPile.Results);
		}
		base.characterLayout.onPointerEnter += _OnCharacterPointerEnter;
		base.characterLayout.onPointerExit += _OnCharacterPointerExit;
		base.characterLayout.onPointerClick += _OnCharacterClick;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (!base.finished)
		{
			base.characters.TransferPile(DeckCreationPile.Results, DeckCreationPile.Discard);
		}
		base.characterLayout.onPointerEnter -= _OnCharacterPointerEnter;
		base.characterLayout.onPointerExit -= _OnCharacterPointerExit;
		base.characterLayout.onPointerClick -= _OnCharacterClick;
	}

	protected override void OnFinish()
	{
		GameStepStack stack = base.state.stack;
		float? maxAspect = 1.7777778f;
		stack.ParallelProcess(new GameStepLetterbox(0.333f, null, maxAspect));
		base.manager.deckState.boxOpen = false;
		AppendStep(new GameStepLoadSceneAsync(base.manager.cosmeticScene));
		AppendStep(new GameStepStateChange(base.manager.deckState, enabled: false));
		AppendStep(new GameStepSetupEnvironmentRendering());
		foreach (GameStep item in new GameStepGroupSetupEnvironmentCosmetics())
		{
			AppendStep(item);
		}
		AppendStep(new GameStepGeneric
		{
			onStart = delegate
			{
				Addressables.ReleaseInstance(GameStateView.Instance.gameObject);
				base.state.Destroy();
				GameStepStack stack2 = base.manager.stack;
				float? maxAspect2 = 2.3703704f;
				stack2.ParallelProcess(new GameStepLetterbox(1f, null, maxAspect2));
				base.manager.stack.Push(new GameStepTimelineReverse(base.manager.establishShotToDeck, base.manager.adventureLookAt));
				base.manager.stack.Push(new GameStepEstablishShot());
			}
		});
	}

	protected override void OnDestroy()
	{
		base.exileLayout.onRest -= _OnExileRest;
		base.decks.onTransfer -= _OnDeckTransfer;
		base.abilities.onTransfer -= _OnAbilityTransfer;
	}
}
