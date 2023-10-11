using ProtoBuf;

[ProtoContract]
[UIField]
public struct TargetedReactionFilter
{
	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide)]
	public ReactionFilter filter;

	[ProtoMember(2)]
	[UIField]
	public TargetOfReaction target;

	public bool IsValid(ReactionContext reactionContext, ActionContext actionContext)
	{
		return filter.IsValid(reactionContext, actionContext);
	}

	public AEntity GetTarget(ReactionContext reactionContext, ActionContext actionContext)
	{
		return target.GetTarget(reactionContext, actionContext);
	}
}
