public class GameStepDeckSelectClass : AGameStepDeck
{
	protected override void _OnBackPressed()
	{
		TransitionTo(new GameStepLoadSceneAsync(base.manager.cosmeticScene));
		TransitionTo(new GameStepStateChange(base.manager.deckState, enabled: false));
		TransitionTo(new GameStepSetupEnvironmentRendering());
		foreach (GameStep item in new GameStepGroupSetupEnvironmentCosmetics())
		{
			TransitionTo(item);
		}
		TransitionTo(new GameStepGeneric
		{
			onStart = GameStateView.DestroyInstance
		});
		TransitionTo(new GameStepTimelineReverse(base.manager.establishShotToDeck, base.manager.adventureLookAt));
		TransitionTo(new GameStepEstablishShot());
	}
}
