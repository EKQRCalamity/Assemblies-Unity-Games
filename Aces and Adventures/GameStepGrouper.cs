using System.Collections.Generic;

public class GameStepGrouper : GameStepGroup
{
	private IEnumerable<GameStep> _gameSteps;

	protected override bool _changesContext { get; }

	public GameStepGrouper(IEnumerable<GameStep> gameSteps, bool changesContext = false)
	{
		_gameSteps = gameSteps;
		_changesContext = changesContext;
	}

	protected override IEnumerable<GameStep> _GetSteps()
	{
		return _gameSteps;
	}
}
