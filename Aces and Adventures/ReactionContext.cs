public struct ReactionContext
{
	public Reaction reaction;

	public AEntity triggeredBy;

	public AEntity triggeredOn;

	public int capturedValue;

	public ActionContext context => new ActionContext(triggeredBy, null, triggeredOn).SetCapturedValue(capturedValue);

	public ReactionContext(Reaction reaction, AEntity triggeredBy, AEntity triggeredOn, int capturedValue)
	{
		this.reaction = reaction;
		this.triggeredBy = triggeredBy;
		this.triggeredOn = triggeredOn;
		this.capturedValue = capturedValue;
	}

	public ReactionContext SetTarget(TargetedReactionFilter targetFilter, ActionContext actionContext)
	{
		ReactionContext reactionContext = this;
		reactionContext.triggeredOn = targetFilter.GetTarget(reactionContext, actionContext);
		return reactionContext;
	}
}
