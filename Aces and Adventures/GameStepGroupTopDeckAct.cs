using System.Collections.Generic;

public class GameStepGroupTopDeckAct : GameStepGroup
{
	private TopDeckAction _action;

	private TopDeckAction.TopDeckConditionActionPair _conditionActionPair;

	private ActionContext _context;

	public GameStepGroupTopDeckAct(TopDeckAction action, ActionContext context, TopDeckAction.TopDeckConditionActionPair conditionActionPair)
	{
		_action = action;
		_conditionActionPair = conditionActionPair;
		_context = context;
	}

	protected override IEnumerable<GameStep> _GetSteps()
	{
		yield return new GameStepTopDeckCondition(_action, _context, _conditionActionPair.condition, _conditionActionPair.result);
		foreach (GameStep actGameStep in _conditionActionPair.action.GetActGameSteps(_context))
		{
			yield return actGameStep;
		}
	}
}
