using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameStepSetupAdventure : GameStep
{
	protected override IEnumerator Update()
	{
		base.state.parameters.adventureBeganInitialize = true;
		if (base.state.adventure.data.dailyLeaderboardEnabled)
		{
			base.state.CalculateDailySeed();
			GameState.Parameters parameters = base.state.parameters;
			int value = parameters[GameState.ParameterType.ViewMapNodes] + 1;
			parameters[GameState.ParameterType.ViewMapNodes] = value;
		}
		else
		{
			base.state.ClearDailySeed();
		}
		using PoolKeepItemListHandle<AdventureData.ASetupInstruction> setupInstructions = base.state.adventure.data.GetValidSetupInstructions(base.state);
		base.state.gameStoneDeck.TransferPile(GameStone.Pile.Select, GameStone.Pile.Discard);
		base.state.player.ApplyLevelUpsToGameState();
		AdventureDeck adventureDeck = base.state.decks.GetCards(DeckPile.Adventure).OfType<AdventureDeck>().First();
		base.view.adventureDeckLayout.SetLayout(AdventureCard.Pile.Draw, adventureDeck.deckView.cardLayout);
		GameStepAddCardsOverTime<AdventureCard.Pile, ATarget> adventureCreationStep = base.state.adventureDeck.AddOverTimeParallel(adventureDeck.GenerateCards().Reverse());
		base.state.decks.Transfer(adventureDeck, DeckPile.AdventureOpen);
		adventureDeck.view.ClearExitTransitions();
		adventureDeck.deckView.Open();
		while (!adventureCreationStep.finished || !adventureDeck.view.atRestInLayout)
		{
			yield return null;
		}
		base.view.adventureDeckLayout.RestoreLayoutToDefault(AdventureCard.Pile.Draw, clearExitTransitions: false, clearEnterTransitions: true);
		AbilityDeck abilityDeck = base.state.decks.GetCards(DeckPile.Ability).OfType<AbilityDeck>().First();
		base.view.playerAbilityDeckLayout.SetLayout(Ability.Pile.Draw, abilityDeck.deckView.cardLayout);
		GameStepAddCardsOverTime<Ability.Pile, Ability> abilityCreationStep = base.state.abilityDeck.AddOverTimeParallel(abilityDeck.GenerateCards().Reverse().Cast<Ability>()
			.Shuffled(base.state.random, setupInstructions.value.OfType<AdventureData.SetupAbilityCards>().FirstOrDefault((AdventureData.SetupAbilityCards s) => ContentRef.Equal(abilityDeck.deckRef, s.deck))?.shuffle ?? true));
		while (!base.view.adventureDeckLayout.GetLayout(AdventureCard.Pile.Draw).IsAtRest())
		{
			yield return null;
		}
		adventureDeck.deckView.Close();
		base.state.decks.Transfer(adventureDeck, DeckPile.Exile);
		base.state.decks.Transfer(abilityDeck, DeckPile.AbilityOpen);
		abilityDeck.deckView.Open();
		while (!abilityCreationStep.finished || !abilityDeck.view.atRestInLayout)
		{
			yield return null;
		}
		base.view.playerAbilityDeckLayout.RestoreLayoutToDefault(Ability.Pile.Draw, clearExitTransitions: false, clearEnterTransitions: true);
		base.view.playerResourceDeckLayout.SetLayout(ResourceCard.Pile.DrawPile, base.view.exileDeckLayout.playerResourceGenerate);
		GameStepAddCardsOverTime<ResourceCard.Pile, ResourceCard> playerCardsStep = base.state.player.resourceDeck.AddOverTimeParallel(setupInstructions.value.Select((AdventureData.ASetupInstruction s) => s.GeneratePlayerCards(base.state)).FirstOrDefault((IEnumerable<ResourceCard> c) => c != null) ?? EnumUtil<PlayingCardType>.Values.Select((PlayingCardType c) => new ResourceCard(c, ProfileManager.options.cosmetic.playingCardDeck)).Concat(CollectionUtil.Repeat(ResourceCard.CreateJoker, base.state.parameters.jokerCount)).Shuffled(base.state.random));
		while (!playerCardsStep.finished || !base.view.playerAbilityDeckLayout.GetLayout(Ability.Pile.Draw).IsAtRest())
		{
			yield return null;
		}
		abilityDeck.deckView.Close();
		base.state.decks.Transfer(abilityDeck, DeckPile.Exile);
		base.view.playerResourceDeckLayout.RestoreLayoutToDefault(ResourceCard.Pile.DrawPile, clearExitTransitions: false, clearEnterTransitions: true);
		base.view.enemyResourceDeckLayout.SetLayout(ResourceCard.Pile.DrawPile, base.view.exileDeckLayout.enemyResourceGenerate);
		GameStepAddCardsOverTime<ResourceCard.Pile, ResourceCard> enemyCardsStep = base.state.enemyResourceDeck.AddOverTimeParallel(setupInstructions.value.Select((AdventureData.ASetupInstruction s) => s.GenerateEnemyCards(base.state)).FirstOrDefault((IEnumerable<ResourceCard> c) => c != null) ?? EnumUtil<PlayingCardType>.Values.Select((PlayingCardType c) => new ResourceCard(c, PlayingCardSkinType.Enemy)).Shuffled(base.state.random));
		while (!enemyCardsStep.finished || !base.view.playerResourceDeckLayout.GetLayout(ResourceCard.Pile.DrawPile).IsAtRest())
		{
			yield return null;
		}
		base.view.enemyResourceDeckLayout.RestoreLayoutToDefault(ResourceCard.Pile.DrawPile, clearExitTransitions: false, clearEnterTransitions: true);
		base.state.InitializeHeroDeck();
		yield return null;
		base.state.InitializeTurnOrderSpaces();
		yield return null;
		base.state.InitializeChips();
		yield return null;
		base.state.InitializeBonuses();
		while (!base.view.enemyResourceDeckLayout.GetLayout(ResourceCard.Pile.DrawPile).IsAtRest())
		{
			yield return null;
		}
		base.state.adventureDeck.Transfer(base.state.player, AdventureCard.Pile.TurnOrder);
		while (!base.state.player.view.atRestInLayout)
		{
			yield return null;
		}
	}

	public override void OnCompletedSuccessfully()
	{
		FinishGroup();
	}
}
