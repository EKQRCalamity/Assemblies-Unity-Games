using System;
using System.Collections.Generic;

public class GameStepContinue : AGameStepAdventureEnd
{
	private readonly bool _shouldShowReplayOption;

	public GameStepContinue(bool shouldShowReplayOption)
	{
		_shouldShowReplayOption = shouldShowReplayOption;
	}

	protected override IEnumerable<Couple<DataRef<AdventureData>, Func<string>>> _GetAdventureDecks()
	{
		if (_shouldShowReplayOption)
		{
			yield return new Couple<DataRef<AdventureData>, Func<string>>(base.state.adventure, () => AdventureResultType.Replay.GetText());
		}
		DataRef<AdventureData> nextAdventure = base.state.game.data.GetNextAdventure(base.state.adventure);
		if (nextAdventure != null)
		{
			yield return new Couple<DataRef<AdventureData>, Func<string>>(nextAdventure, () => AdventureResultType.Continue.GetText());
		}
	}

	protected override void _WaitForDeckClick(AdventureDeck adventureDeck)
	{
		TransitionTo(new GameStepAnimateGameStateClear());
	}
}
