public class GameStepRestoreGameState : GameStep
{
	public override void Start()
	{
		base.state?.player?.abilityDeck?.TransferPile(Ability.Pile.ActivationHand, Ability.Pile.Discard);
		base.view?.CheckForExiledCardsAtRest();
	}

	protected override void End()
	{
		GameState gameState = base.state;
		int reloadCount = gameState.reloadCount + 1;
		gameState.reloadCount = reloadCount;
	}
}
