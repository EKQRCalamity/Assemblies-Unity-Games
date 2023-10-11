public class GameStepDestroyGameState : GameStep
{
	public override void Start()
	{
		base.state.Destroy();
		base.manager.adventureState.enabled = false;
		base.manager.stack.Push(new GameStepTimelineReverse(base.manager.establishShotToGame, base.manager.adventureLookAt));
		base.manager.stack.Push(new GameStepEstablishShot());
	}
}
