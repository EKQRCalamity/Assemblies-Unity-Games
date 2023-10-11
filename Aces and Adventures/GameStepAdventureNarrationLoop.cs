using System.Collections;

public class GameStepAdventureNarrationLoop : AGameStepAdventure
{
	protected virtual bool _saveState => false;

	protected override IEnumerator Update()
	{
		while (true)
		{
			if (base.adventureDeck.Count(AdventureCard.Pile.SelectionHand) > 0)
			{
				yield return null;
				continue;
			}
			if (base.adventureDeck.Count(AdventureCard.Pile.Draw) > 0)
			{
				if (_saveState)
				{
					yield return AppendStep(new GameStepSaveGameState());
				}
				yield return AppendStep(base.adventureDeck.DrawStepAdventure());
				continue;
			}
			break;
		}
	}
}
