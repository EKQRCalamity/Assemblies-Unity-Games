using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField]
public struct ReactionFilter
{
	[ProtoMember(1)]
	[UIField]
	[UIHorizontalLayout("Trigger")]
	public ReactionEntity triggeredBy;

	[ProtoMember(2)]
	[UIField]
	[UIHorizontalLayout("Trigger")]
	public ReactionEntity triggeredOn;

	[ProtoMember(3, OverwriteList = true)]
	[UIField]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	public List<AAction.Condition.Actor> triggeredByConditions;

	[ProtoMember(4, OverwriteList = true)]
	[UIField]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	public List<AAction.Condition.Actor> triggeredOnConditions;

	[ProtoMember(5)]
	[UIField]
	public bool preventRecursion;

	public bool IsValid(ReactionContext reactionContext, ActionContext actionContext)
	{
		if (triggeredBy.IsValid(reactionContext.triggeredBy, actionContext) && triggeredOn.IsValid(reactionContext.triggeredOn, actionContext) && triggeredByConditions.All(actionContext.SetTarget(reactionContext.triggeredBy)) && triggeredOnConditions.All(actionContext.SetTarget(reactionContext.triggeredOn)) && reactionContext.reaction.OnValid(actionContext))
		{
			if (preventRecursion)
			{
				return actionContext.gameState.stack.GetGroups<GameStepGroupActionTick>().AsEnumerable().None((GameStepGroupActionTick tick) => tick.reactionContext.reaction == reactionContext.reaction);
			}
			return true;
		}
		return false;
	}
}
