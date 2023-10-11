using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class GameStepSelectCharacter : GameStep
{
	private ACardLayout.DragThresholdData _originalDragThresholds;

	private float _elapsedTime;

	public override bool canInspect => true;

	public GameStepSelectCharacter()
	{
		if (ProfileManager.progress.characters.read.GetUnlockedCharacters(DevData.Unlocks.characters).None())
		{
			AppendStep(new AReward.UnlockClassReward(ContentRef.Defaults.data.startingCharacter).GetUnlockStep());
		}
	}

	private void _GenerateCharacterCards()
	{
		PoolKeepItemHashSetHandle<DataRef<CharacterData>> existingCharacters = Pools.UseKeepItemHashSet(from p in base.state.targets.Values<Player>().AsEnumerable()
			select p.characterDataRef);
		try
		{
			using PoolKeepItemHashSetHandle<DataRef<CharacterData>> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet(ProfileManager.progress.characters.read.GetUnlockedCharacters(DevData.Unlocks.characters));
			base.state.exileDeck.AddSortedPile(ExilePile.Character);
			foreach (DataRef<CharacterData> item in from d in DataRef<CharacterData>.Search()
				where !existingCharacters.Contains(d)
				select d)
			{
				Player player = new Player(item);
				PlayerCardView playerCardView = (PlayerCardView)base.state.exileDeck.Add(player, ExilePile.Character).view;
				if (!poolKeepItemHashSetHandle.Contains(item))
				{
					Quaternion quaternion = Quaternion.Euler(0f, 0f, 180f);
					playerCardView.offsets.Add(Matrix4x4.TRS(new Vector3(0f, 0.002f, 0f), quaternion, Vector3.one));
					playerCardView.RequestGlow(player, Colors.FAILURE, GlowTags.Persistent);
					PointerDrag3D pointerDrag = playerCardView.pointerDrag;
					bool flag2 = (playerCardView.generateDiceWhenPlacedIntoTurnOrder = false);
					bool flag4 = (playerCardView.noiseAnimationEnabled = flag2);
					pointerDrag.enabled = flag4;
					GameObject gameObject = UnityEngine.Object.Instantiate(GameStepLevelUp.CardLock.value, playerCardView.transform);
					gameObject.transform.localRotation = quaternion;
					gameObject.GetOrAddComponent<CharacterCardLockView>().SetData(player);
				}
			}
			base.state.exileDeck.layout.GetLayout(ExilePile.Character).ForceFinishLayoutAnimations();
		}
		finally
		{
			if (existingCharacters != null)
			{
				((IDisposable)existingCharacters).Dispose();
			}
		}
	}

	private void _OnBackPressed()
	{
		Cancel();
	}

	private void _OnButtonClick(ButtonCard.Pile pile, ButtonCard card)
	{
		if (pile == ButtonCard.Pile.Active && card.type == ButtonCardType.Back)
		{
			_OnBackPressed();
		}
	}

	private void _OnClick(AdventureCard.Pile pile, ATarget card)
	{
		if (pile == AdventureCard.Pile.TurnOrder && card is Player player)
		{
			if (card.view.hasOffset)
			{
				base.view.LogError(LevelUpMessages.CompleteAdventureToUnlock.Localize());
				return;
			}
			if (IOUtil.IsDemo && !player.characterData.unlockedForDemo)
			{
				base.view.LogError(AdventureTutorial.CharacterLockedForDemo.Localize(), player.audio.character.error.generic, player.view.transform);
				return;
			}
			if (AbilityDeckData.Search(player.characterClass, mustBeValid: true).None())
			{
				base.view.LogError(AdventureTutorial.NoValidDeckFound.Localize(), player.audio.character.error.generic, player.view.transform);
				return;
			}
			base.state.player = player;
			base.state.adventureDeck.Transfer(player, AdventureCard.Pile.ActiveHand);
			player.view.ClearEnterTransitions();
			VoiceManager.Instance.Play(player.view.transform, player.audio.character.select, interrupt: true);
			AppendStep(new GameStepSelectDeck());
		}
	}

	private void _OnPointerEnter(AdventureCard.Pile pile, ATarget card)
	{
		if (card.view.hasOffset && pile == AdventureCard.Pile.TurnOrder && card is Player player)
		{
			ProjectedTooltipFitter.Create(player.characterClass.GetText(), card.view.gameObject, base.view.tooltipCanvas, TooltipAlignment.BottomCenter, 40);
		}
		else if (pile == AdventureCard.Pile.TurnOrder && card is Player player2)
		{
			AdventureCompletion.Data? data = ProfileManager.progress.games.read.GetCompletion(base.state.game, base.state.adventure)?.GetData(player2.characterClass);
			if (data.HasValue)
			{
				AdventureCompletion.Data valueOrDefault = data.GetValueOrDefault();
				ProjectedTooltipFitter.Create(AdventureResultType.Time.GetText() + ": " + TimeSpan.FromSeconds(valueOrDefault.time).ToStringSimple() + "\n" + AdventureResultType.StrategicTime.GetText() + ": " + TimeSpan.FromSeconds(valueOrDefault.strategyTime).ToStringSimple(), card.view.gameObject, base.view.tooltipCanvas, TooltipAlignment.TopCenter, 40);
			}
		}
	}

	private void _OnPointerExit(AdventureCard.Pile pile, ATarget card)
	{
		if (card is Player)
		{
			ProjectedTooltipFitter.Finish(card.view.gameObject);
		}
	}

	private void _OnDeckClick(DeckPile pile, ADeck card)
	{
		if (pile == DeckPile.Adventure)
		{
			_OnBackPressed();
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
				_OnBackPressed();
			}
		}
	}

	protected override void OnFirstEnabled()
	{
		_GenerateCharacterCards();
	}

	protected override void OnEnable()
	{
		base.state.adventureDeck.layout.onPointerClick += _OnClick;
		base.state.adventureDeck.layout.onPointerEnter += _OnPointerEnter;
		base.state.adventureDeck.layout.onPointerExit += _OnPointerExit;
		base.view.onBackPressed += _OnBackPressed;
		base.state.decks.layout.onPointerClick += _OnDeckClick;
		base.state.buttonDeck.layout.onPointerClick += _OnButtonClick;
		base.state.buttonDeck.Layout<ButtonDeckLayout>().SetActive(ButtonCardType.Back, setActive: true, forceUpdateCancelStone: true);
		base.state.gameStoneDeck.layout.onPointerClick += _OnGameStoneClick;
		foreach (Player item in from p in base.state.targets.Values<Player>().AsEnumerable()
			orderby p.characterClass.SortOrder()
			select p)
		{
			base.state.adventureDeck.Transfer(item, AdventureCard.Pile.TurnOrder);
		}
		base.state.player?.view.ClearExitTransitions();
		_originalDragThresholds = base.state.adventureDeck.layout.GetLayout(AdventureCard.Pile.TurnOrder).SetDragThresholds(new ACardLayout.DragThresholdData(0f, 0f, 0.25f, 0.25f, useDragTargetForDragThresholdOrigin: true));
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
			base.view.LogMessage(AdventureTutorial.SelectCharacter.Localize());
		}
	}

	protected override void OnDisable()
	{
		AGameStepTurn.ResetMessageTimer(ref _elapsedTime);
		base.state.adventureDeck.layout.onPointerClick -= _OnClick;
		base.state.adventureDeck.layout.onPointerEnter -= _OnPointerEnter;
		base.state.adventureDeck.layout.onPointerExit -= _OnPointerExit;
		base.view.onBackPressed -= _OnBackPressed;
		base.state.decks.layout.onPointerClick -= _OnDeckClick;
		base.state.buttonDeck.layout.onPointerClick -= _OnButtonClick;
		base.state.buttonDeck.Layout<ButtonDeckLayout>().SetActive(ButtonCardType.Back, setActive: false);
		base.state.gameStoneDeck.layout.onPointerClick -= _OnGameStoneClick;
		foreach (Player item in base.state.adventureDeck.GetCardsSafe(AdventureCard.Pile.TurnOrder).AsEnumerable().OfType<Player>())
		{
			base.state.exileDeck.Transfer(item, ExilePile.Character);
			item.view.ClearExitTransitions();
		}
		_originalDragThresholds.Restore();
	}

	protected override void OnCanceled()
	{
		base.state.player = null;
	}

	public override void OnCompletedSuccessfully()
	{
		foreach (Player item in base.state.targets.Values<Player>())
		{
			if (item != base.state.player)
			{
				item.view.DestroyCard();
			}
		}
	}
}
