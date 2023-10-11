public class GameStepSetAbilityReactionContext : GameStep
{
	private Ability _ability;

	private ReactionContext _reactionContext;

	public GameStepSetAbilityReactionContext(Ability ability, ReactionContext reactionContext)
	{
		_ability = ability;
		_reactionContext = reactionContext;
	}

	public override void Start()
	{
		_ability.reactionContext = _reactionContext;
	}
}
