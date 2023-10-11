using System.Collections.Generic;

public class GameStepGroupDynamic : GameStepGroup
{
	private GameStep _startingStep;

	public GameStepGroupDynamic(GameStep startingStep)
	{
		_startingStep = startingStep;
	}

	protected override IEnumerable<GameStep> _GetSteps()
	{
		yield return _startingStep;
	}
}
