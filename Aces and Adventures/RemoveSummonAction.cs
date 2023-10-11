using ProtoBuf;

[ProtoContract]
[UIField("Remove Summon", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Specialized")]
public class RemoveSummonAction : AAction
{
	private static readonly Target.Summon TARGET = new Target.Summon();

	[ProtoMember(1)]
	[UIField(tooltip = "Bring summon back into owner's hand.")]
	private bool _bounce;

	public override Target target => TARGET;

	protected override void _Tick(ActionContext context)
	{
		context.gameState.stack.activeStep.AppendStep(new GameStepRemoveSummon(this, context, context.GetTarget<Ability>(), isBeingReplaced: false));
		if (_bounce)
		{
			context.gameState.stack.activeStep.AppendStep(context.gameState.abilityDeck.TransferCardStep(context.target as Ability, Ability.Pile.Hand));
		}
	}

	protected override string _ToStringUnique()
	{
		if (!_bounce)
		{
			return "Remove";
		}
		return "Bounce";
	}
}
