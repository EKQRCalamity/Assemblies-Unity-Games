using System.Linq;

public class GameStepActionTargetInherit<T> : GameStepActionTarget where T : AAction.Target
{
	public GameStepActionTargetInherit(AAction action, ActionContext context, AAction.Target targeting)
		: base(action, context, targeting)
	{
	}

	public override void Start()
	{
		GameStepActionTarget gameStepActionTarget = base.group.GetPreviousStepsOfTypeUntilType<Tick, GameStepActionTarget>(this).FirstOrDefault() ?? GetPreviousSteps(GroupType.Context).OfType<GameStepActionTarget>().FirstOrDefault((GameStepActionTarget t) => !t.action.target.inheritsTargets && t.hasTargets && typeof(T).IsSameOrSubclass(t.action.target.GetType()));
		if (gameStepActionTarget == null && base.targeting is AAction.Target.Combatant)
		{
			Ability ability = base.context.ability;
			if (ability != null && ability.data.type == AbilityData.Type.TriggeredTrait)
			{
				ATarget target = base.context.ability.reactionContext.context.target;
				if (target != null)
				{
					base.targeting.targetCountType = TargetCountType.SingleTarget;
					_SetTargets(base.action.CanAttemptToAct(base.context) ? base.targeting.FilterTargetsByConditions(base.context, Enumerable.Repeat(target, 1)) : Enumerable.Empty<ATarget>());
				}
			}
		}
		if (gameStepActionTarget != null)
		{
			base.targeting.targetCountType = gameStepActionTarget.targeting?.targetCountType ?? TargetCountType.SingleTarget;
			_SetTargets(base.action.CanAttemptToAct(base.context) ? base.targeting.FilterTargetsByConditions(base.context, gameStepActionTarget.targets) : Enumerable.Empty<ATarget>());
		}
	}
}
