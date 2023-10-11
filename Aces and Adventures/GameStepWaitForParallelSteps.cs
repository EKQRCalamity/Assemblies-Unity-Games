using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameStepWaitForParallelSteps : GameStep
{
	private List<GameStep> _steps;

	public GameStepWaitForParallelSteps(IEnumerable<GameStep> steps)
	{
		_steps = new List<GameStep>(steps);
	}

	public GameStepWaitForParallelSteps(params GameStep[] steps)
	{
		_steps = new List<GameStep>(steps);
	}

	protected override IEnumerator Update()
	{
		while (_steps.Any((GameStep s) => !s.destroyed))
		{
			yield return null;
		}
	}
}
