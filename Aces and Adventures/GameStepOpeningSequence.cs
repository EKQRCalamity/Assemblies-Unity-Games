public class GameStepOpeningSequence : GameManagerStep
{
	public override void Start()
	{
		ProfileManager.progress.openingPlayed = true;
		base.finished = true;
		base.stack.ParallelProcess(new GameStepAdjustDynamicResolutionToFrameRate(50f, 55f, 1f, (int)base.manager.startToEstablish.duration - 3, 5, 0.5f));
		base.stack.Push(new GameStepVirtualCamera(base.manager.startCamera, 0f));
		base.stack.Push(new GameStepTimeline(base.manager.startToEstablish));
		base.stack.Push(new GameStepVirtualCamera(base.manager.establishCamera, 0f));
	}
}
