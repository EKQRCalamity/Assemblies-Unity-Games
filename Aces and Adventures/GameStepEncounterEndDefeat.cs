public class GameStepEncounterEndDefeat : GameStep
{
	protected override void OnFirstEnabled()
	{
		base.state.stoneDeck.Layout<StoneDeckLayout>().Transfer(StoneType.Turn, Stone.Pile.Inactive);
	}

	public override void Start()
	{
		base.state.stack.Cancel();
		AppendGroup(new GameStepGroupLossMedia());
		AppendGroup(new GameStepGroupRewards(AdventureEndType.Defeat));
		AppendStep(new GameStepAnimateGameStateClear());
		AppendStep(new GameStepRetry());
	}
}
