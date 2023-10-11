using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameStepSelectAdventure : GameStep
{
	private DataRef<AdventureData> _autoSelectedAdventure;

	private readonly List<AdventureDeck> _adventureDecks = new List<AdventureDeck>();

	private DeckPile _selectPile;

	private float _elapsedTime;

	public GameStepSelectAdventure(DataRef<AdventureData> autoSelectedAdventure = null)
	{
		_autoSelectedAdventure = autoSelectedAdventure;
	}

	private void _GenerateAdventureDecks()
	{
		foreach (AdventureDeck item in Pools.UseKeepItemList(base.state.decks.GetCards().OfType<AdventureDeck>()))
		{
			base.state.decks.Transfer(item, DeckPile.Exile);
		}
		using PoolKeepItemListHandle<DataRef<AdventureData>> poolKeepItemListHandle = Pools.UseKeepItemList(ProfileManager.progress.games.read.GetUnlockedAdventures(base.state.game, DevData.Unlocks.adventures));
		_selectPile = ((poolKeepItemListHandle.Count > 10) ? DeckPile.SelectLarge : DeckPile.Select);
		if (_selectPile == DeckPile.SelectLarge)
		{
			base.view.decksLayout.SetLayout(DeckPile.InactiveSelectAdventure, base.view.decksLayout.inactiveSelectAdventureLarge);
		}
		else
		{
			base.view.decksLayout.RestoreLayoutToDefault(DeckPile.InactiveSelectAdventure);
		}
		_adventureDecks.Clear();
		foreach (DataRef<AdventureData> item2 in poolKeepItemListHandle.value)
		{
			base.state.decks.Add(_adventureDecks.AddReturn(new AdventureDeck(item2)), DeckPile.InactiveSelectAdventure);
		}
		base.state.decks.layout.GetLayout(DeckPile.InactiveSelectAdventure).ForceFinishLayoutAnimations();
	}

	private void _OnDeckClick(DeckPile pile, ADeck card)
	{
		if (pile == _selectPile && card is AdventureDeck adventureDeck)
		{
			if (!adventureDeck.adventureDataRef.CanBeUnlocked())
			{
				base.view.LogError(AdventureTutorial.AdventureLockedForDemo.Localize());
				return;
			}
			base.state.adventure = adventureDeck.adventureDataRef;
			base.state.decks.Transfer(card, DeckPile.Adventure);
			AppendStep(new GameStepSelectCharacter());
		}
	}

	private void _OnDeckEnter(DeckPile pile, ADeck card)
	{
		if (pile != _selectPile)
		{
			return;
		}
		AdventureDeck adventureDeck = card as AdventureDeck;
		if (adventureDeck == null)
		{
			return;
		}
		bool flag = false;
		DataRef<ProceduralNodePackData> modifier = adventureDeck.adventureDataRef.data.modifier;
		if (modifier != null && (bool)modifier && (flag = true))
		{
			base.view.ShowCardsAsTooltip(() => _ShowCardsAsTooltip(adventureDeck.adventureDataRef, modifier), ((AdventureDeckView)adventureDeck.deckView).tooltipCreator, 0.5f);
		}
		AdventureCompletion completion = ProfileManager.progress.games.read.GetCompletion(base.state.game, adventureDeck.adventureDataRef);
		if (completion != null)
		{
			KeyValuePair<PlayerClass, AdventureCompletion.Data> valueOrDefault = completion.GetBestTime().GetValueOrDefault();
			KeyValuePair<PlayerClass, AdventureCompletion.Data> valueOrDefault2 = completion.GetBestStrategyTime().GetValueOrDefault();
			string[] obj = new string[13]
			{
				AdventureResultType.Time.GetText(),
				": ",
				TimeSpan.FromSeconds(valueOrDefault.Value.time).ToStringSimple(),
				" <size=75%>",
				valueOrDefault.Key.GetText(),
				"</size>\n",
				AdventureResultType.StrategicTime.GetText(),
				": ",
				TimeSpan.FromSeconds(valueOrDefault2.Value.strategyTime).ToStringSimple(),
				" <size=75%>",
				valueOrDefault2.Key.GetText(),
				"</size>",
				null
			};
			AdventureCompletionRank? nextCompletionRank = adventureDeck.adventureDataRef.data.GetNextCompletionRank(valueOrDefault2.Value.strategyTime);
			object obj2;
			if (nextCompletionRank.HasValue)
			{
				AdventureCompletionRank valueOrDefault3 = nextCompletionRank.GetValueOrDefault();
				obj2 = "\n" + AdventureResultType.NextRankTime.GetText() + ": " + TimeSpan.FromSeconds(adventureDeck.adventureDataRef.data.GetCompletionTime(valueOrDefault3)).ToStringSimple();
			}
			else
			{
				obj2 = "";
			}
			obj[12] = (string)obj2;
			ProjectedTooltipFitter.Create(string.Concat(obj).SizeIf(flag), adventureDeck.view.gameObject, base.view.tooltipCanvas, flag ? TooltipAlignment.TopCenter : TooltipAlignment.BottomCenter, 132);
		}
		if (adventureDeck.adventureDataRef.data.dailyLeaderboardEnabled && Steam.Enabled && ProfileManager.options.game.preferences.leaderboard)
		{
			ProjectedTooltipFitter.Create("<size=66%>" + MessageData.GameTooltips.ViewLeaderboard.Localize().Localize() + "</size>", adventureDeck.adventureDeckView.additionalTooltipCreator, base.view.tooltipCanvas, TooltipAlignment.MiddleRight, 32);
		}
	}

	private IEnumerable<ATarget> _ShowCardsAsTooltip(DataRef<AdventureData> adventure, DataRef<ProceduralNodePackData> modifier, int? dayOverride = null)
	{
		return base.state.GetModifierCards(adventure, modifier, dayOverride);
	}

	private void _OnDeckExit(DeckPile pile, ADeck card)
	{
		if (card is AdventureDeck adventureDeck)
		{
			ProjectedTooltipFitter.Finish(card.view.gameObject);
			ProjectedTooltipFitter.Finish(adventureDeck.adventureDeckView.additionalTooltipCreator);
			base.view.HideCardsShownAsTooltip();
		}
	}

	private void _OnDragBegin(PointerEventData eventData, CardLayoutElement card)
	{
		if (card.card is AdventureDeck adventureDeck)
		{
			ProjectedTooltipFitter.Finish(card.gameObject);
			ProjectedTooltipFitter.Finish(adventureDeck.adventureDeckView.additionalTooltipCreator);
			base.view.HideCardsShownAsTooltip(immediate: true);
		}
	}

	private void _OnDeckRest(DeckPile pile, ADeck card)
	{
		if (pile == DeckPile.Exile)
		{
			card.view.DestroyCard();
		}
	}

	private void _OnBackPressed()
	{
		base.state.stack.Cancel();
		TransitionTo(new GameStepAnimateGameStateClear());
		TransitionTo(new GameStepWaitForParallelStep(base.state.stack.ParallelProcess(new GameStepLoadSceneAsync(base.manager.cosmeticScene))));
		TransitionTo(new GameStepSetupEnvironmentRendering());
		base.state.stack.Push(new GameStepGroupSetupEnvironmentCosmetics());
		TransitionTo(new GameStepDestroyGameState());
	}

	private void _OnButtonClick(ButtonCard.Pile pile, ButtonCard card)
	{
		if (pile == ButtonCard.Pile.Active && (ButtonCardType)card == ButtonCardType.Back)
		{
			_OnBackPressed();
		}
	}

	private void _OnGameStoneClick(GameStone.Pile pile, GameStone card)
	{
		_SelectGame(card.game);
	}

	private void _GenerateGameStones()
	{
		using PoolKeepItemListHandle<DataRef<GameData>> poolKeepItemListHandle = Pools.UseKeepItemList(ProfileManager.progress.games.read.GetUnlockedGames(DevData.Unlocks.games));
		if (poolKeepItemListHandle.Count >= 2)
		{
			base.state.gameStoneDeck.Add(from g in poolKeepItemListHandle.value
				orderby g.data.sortOrder
				select g into game
				select new GameStone(game));
			base.view.gameStoneDeckLayout.GetLayout(GameStone.Pile.Draw).ForceFinishLayoutAnimations();
			base.state.gameStoneDeck.TransferPile(GameStone.Pile.Draw, GameStone.Pile.Select);
		}
	}

	private void _SelectGame(DataRef<GameData> game)
	{
		if (ContentRef.Equal(base.state.game, game))
		{
			return;
		}
		if (!game.CanBeUnlocked())
		{
			base.view.LogError(AdventureTutorial.GameLockedForDemo.Localize());
			return;
		}
		GameState gameState = base.state;
		DataRef<GameData> game2 = (ProfileManager.prefs.selectedGame = game);
		gameState.game = game2;
		if ((bool)game.data.adventureSelectLighting)
		{
			AppendStep(GameStepLighting.Create(game.data.adventureSelectLighting.data));
		}
		if ((bool)game.data.adventureSelectMusic)
		{
			AppendStep(new GameStepMusic(MusicPlayType.Resume, game.data.adventureSelectMusic, game.data.adventureSelectMusicVolume));
		}
		if ((bool)game.data.environmentAmbient)
		{
			AppendStep(new GameStepAmbient(MusicPlayType.Resume, game.data.environmentAmbient, game.data.environmentAmbientVolume));
		}
		_GenerateAdventureDecks();
		foreach (GameStone card in base.state.gameStoneDeck.GetCards())
		{
			card.view.offsets.Clear();
			if (ContentRef.Equal(game, card.game))
			{
				card.view.offsets.Add(Matrix4x4.Translate(new Vector3(0f, 0f, 0.02f)));
			}
		}
		base.view.gameStoneDeckLayout.select.SetDirty();
	}

	private void _OnPointerClick(ACardLayout layout, CardLayoutElement card, PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right && !eventData.dragging && card is AdventureDeckView adventureDeckView && adventureDeckView.adventureDeck.adventureDataRef.data.dailyLeaderboardEnabled && (Steam.Enabled || IOUtil.IsEditor) && ProfileManager.options.game.preferences.leaderboard)
		{
			AppendStep(new GameStepGroupRewards.GameStepViewLeaderboard(adventureDeckView.adventureDeck.adventureDataRef, LeaderboardProgress.GetDay()));
		}
	}

	protected override void OnFirstEnabled()
	{
		_GenerateGameStones();
		if (!ProfileManager.prefs.selectedGame.CanBeUnlocked())
		{
			ProfileManager.prefs.selectedGame = null;
		}
		_SelectGame(ProfileManager.prefs.selectedGame);
	}

	protected override void OnEnable()
	{
		_SelectGame(ProfileManager.prefs.selectedGame);
		if (!_autoSelectedAdventure)
		{
			foreach (AdventureDeck adventureDeck in _adventureDecks)
			{
				base.state.decks.Transfer(adventureDeck, _selectPile);
			}
		}
		base.view.buttonDeckLayout.SetActive(ButtonCardType.Back, setActive: true, forceUpdateCancelStone: true);
		if (ProfileManager.progress.experience.read.CanLevelUp())
		{
			base.view.stoneDeckLayout[StoneType.Cancel].view.RequestGlow(this, Colors.TARGET);
			base.view.AddCancelStoneTooltipOverride(LevelUpMessages.AvailableLevelUps.Localize());
		}
		base.state.decks.layout.onPointerClick += _OnDeckClick;
		base.state.decks.layout.onPointerEnter += _OnDeckEnter;
		base.state.decks.layout.onPointerExit += _OnDeckExit;
		base.state.decks.layout.GetLayout(_selectPile).onDragBegin += _OnDragBegin;
		base.view.onBackPressed += _OnBackPressed;
		base.state.buttonDeck.layout.onPointerClick += _OnButtonClick;
		base.state.gameStoneDeck.layout.onPointerClick += _OnGameStoneClick;
		ACardLayout.OnPointerClick += _OnPointerClick;
	}

	public override void Start()
	{
		base.state.decks.layout.onRest += _OnDeckRest;
		if ((bool)_autoSelectedAdventure)
		{
			_OnDeckClick(_selectPile, base.state.targets.Values<AdventureDeck>().AsEnumerable().FirstOrDefault((AdventureDeck deck) => ContentRef.Equal(deck.adventureDataRef, _autoSelectedAdventure)));
		}
		_autoSelectedAdventure = null;
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
			base.view.LogMessage(AdventureTutorial.SelectAdventure.Localize());
		}
	}

	protected override void OnDisable()
	{
		AGameStepTurn.ResetMessageTimer(ref _elapsedTime);
		base.state.decks.TransferPile(_selectPile, DeckPile.InactiveSelectAdventure);
		base.view.buttonDeckLayout.SetActive(ButtonCardType.Back, setActive: false);
		if (ProfileManager.progress.experience.read.CanLevelUp())
		{
			base.view.RemoveCancelStoneTooltipOverride(LevelUpMessages.AvailableLevelUps.Localize());
		}
		base.state.decks.layout.onPointerClick -= _OnDeckClick;
		base.state.decks.layout.onPointerEnter -= _OnDeckEnter;
		base.state.decks.layout.onPointerExit -= _OnDeckExit;
		base.state.decks.layout.GetLayout(_selectPile).onDragBegin -= _OnDragBegin;
		base.view.onBackPressed -= _OnBackPressed;
		base.state.buttonDeck.layout.onPointerClick -= _OnButtonClick;
		base.state.gameStoneDeck.layout.onPointerClick -= _OnGameStoneClick;
		ACardLayout.OnPointerClick -= _OnPointerClick;
	}

	public override void OnCompletedSuccessfully()
	{
		base.state.StartAdventure();
	}

	protected override void OnDestroy()
	{
		if (!base.canceled)
		{
			foreach (AdventureDeck item in base.state.decks.GetCardsSafe().AsEnumerable().OfType<AdventureDeck>())
			{
				item.view.DestroyCard();
			}
		}
		if (!base.canceled)
		{
			foreach (GameStone item2 in base.state.gameStoneDeck.GetCardsSafe())
			{
				item2.view.DestroyCard();
			}
		}
		base.state.decks.layout.onRest -= _OnDeckRest;
		ProfileManager.Profile.SavePreferences();
	}
}
