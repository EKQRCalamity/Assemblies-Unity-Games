using System.Collections;

public class GameStepWaitForParallelStep : GameStep
{
	private GameStep _step;

	public GameStepWaitForParallelStep(GameStep step)
	{
		_step = step;
	}

	protected override IEnumerator Update()
	{
		while (!_step.destroyed)
		{
			yield return null;
		}
	}
}
