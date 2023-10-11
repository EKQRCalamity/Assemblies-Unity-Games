public class GameStepEncounterEndVictory : GameStep
{
	protected override void OnFirstEnabled()
	{
		base.state.SignalEndTurn();
	}

	public override void Start()
	{
		base.state.SignalEndRound();
	}

	protected override void End()
	{
		base.state.SignalEncounterEnd();
	}

	protected override void OnDestroy()
	{
		base.state.stoneDeck.Layout<StoneDeckLayout>().Transfer(StoneType.Turn, Stone.Pile.Inactive);
	}
}
