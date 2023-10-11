using System.Collections.Generic;

public class GameStepGroupActionTick : GameStepGroup
{
	private AppliedAction _appliedAction;

	private ActionContext _context;

	private ReactionContext _reactionContext;

	public AppliedAction appliedAction => _appliedAction;

	public ReactionContext reactionContext => _reactionContext;

	protected override bool _changesContext => true;

	public GameStepGroupActionTick(AppliedAction appliedAction, ActionContext context, ReactionContext reactionContext)
	{
		_appliedAction = appliedAction;
		_context = context;
		_reactionContext = reactionContext;
	}

	protected override IEnumerable<GameStep> _GetSteps()
	{
		if (_context.hasAbility)
		{
			yield return new GameStepSetAbilityReactionContext(_context.ability, _reactionContext);
		}
		foreach (GameStep tickGameStep in _appliedAction.action.GetTickGameSteps(_context))
		{
			yield return tickGameStep;
		}
	}
}
