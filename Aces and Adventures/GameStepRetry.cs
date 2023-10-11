using System;
using System.Collections.Generic;

public class GameStepRetry : AGameStepAdventureEnd
{
	protected override IEnumerable<Couple<DataRef<AdventureData>, Func<string>>> _GetAdventureDecks()
	{
		yield return new Couple<DataRef<AdventureData>, Func<string>>(base.state.adventure, () => AdventureResultType.Retry.GetText());
	}
}
