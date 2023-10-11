using System.Collections.Generic;

public static class AdventureExtensions
{
	public static CanBeSelectedResult CanBeSelected(this IAdventureCard adventureCard, GameState gameState)
	{
		return adventureCard?.adventureCardCommon?.CanBeSelected(gameState) ?? ((CanBeSelectedResult)AbilityPreventedBy.ConditionNotMet);
	}

	public static IEnumerable<ATarget> GenerateCards(this IEnumerable<AdventureCard> cards, GameState state)
	{
		if (cards == null)
		{
			yield break;
		}
		foreach (AdventureCard card in cards)
		{
			foreach (ATarget item in card.GenerateCards(state))
			{
				yield return item;
			}
		}
	}

	public static IEnumerable<GameStep> GetSteps(this IEnumerable<AdventureCard.SelectInstruction> instructions, GameState state)
	{
		if (instructions == null)
		{
			yield break;
		}
		foreach (AdventureCard.SelectInstruction instruction in instructions)
		{
			foreach (GameStep gameStep in instruction.GetGameSteps(state))
			{
				yield return gameStep;
			}
		}
	}

	public static IAdventureCard SetCommon(this IAdventureCard adventureCard, AdventureCard.Common common)
	{
		adventureCard.adventureCardCommon = common;
		return adventureCard;
	}

	public static GameStepDrawAdventureCard DrawStepAdventure(this IdDeck<AdventureCard.Pile, ATarget> deck, AdventureCard.Pile? drawTo = null)
	{
		return new GameStepDrawAdventureCard(drawTo);
	}
}
