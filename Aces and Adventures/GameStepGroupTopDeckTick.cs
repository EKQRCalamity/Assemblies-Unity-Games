using System.Collections.Generic;

public class GameStepGroupTopDeckTick : GameStepGroup
{
	private TopDeckAction _action;

	private TopDeckAction.TopDeckConditionActionPair _conditionActionPair;

	private ActionContext _context;

	public GameStepGroupTopDeckTick(TopDeckAction action, ActionContext context, TopDeckAction.TopDeckConditionActionPair conditionActionPair)
	{
		_action = action;
		_conditionActionPair = conditionActionPair;
		_context = context;
	}

	protected override IEnumerable<GameStep> _GetSteps()
	{
		yield return new GameStepTopDeckCondition(_action, _context, _conditionActionPair.condition, _conditionActionPair.result);
		foreach (GameStep tickGameStep in _conditionActionPair.action.GetTickGameSteps(_context))
		{
			yield return tickGameStep;
		}
	}
}
