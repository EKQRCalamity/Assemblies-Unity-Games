using System.Linq;
using ProtoBuf;

[ProtoContract]
public class TutorialCard : ATarget
{
	[ProtoContract(EnumPassthru = true)]
	public enum Pile
	{
		Draw,
		TopLeft,
		Discard
	}

	[ProtoMember(1)]
	private DataRef<TutorialData> _dataRef;

	public TutorialData data => _dataRef.data;

	public Pile pile => base.gameState.tutorialDeck[this].GetValueOrDefault();

	public TutorialCardView tutorialCard => base.view as TutorialCardView;

	public ActionContext context => new ActionContext(base.gameState.player, null, base.gameState.player);

	public override bool shouldRegisterDuringGameStateInitialization => true;

	public override int registerDuringGameStateInitializationOrder => -100;

	private TutorialCard()
	{
	}

	public TutorialCard(DataRef<TutorialData> tutorialDataRef)
	{
		_dataRef = tutorialDataRef;
	}

	private void _OnTrigger(ReactionContext reactionContext, TargetedReactionFilter filter, int capturedValue)
	{
		if (ProfileManager.options.game.ui.tutorialEnabled && filter.IsValid(reactionContext, context) && data.playerConditions.All((AAction.Condition.Actor c) => c.IsValid(context)) && (data.targetLines.Count == 0 || data.targetLines.Any((TutorialData.TargetLineData t) => t.targetingAction.GetTargetable(context).Any())))
		{
			base.gameState.stack.Push(new GameStepGroupTutorial(this, filter.GetTarget(reactionContext, context)));
		}
	}

	private void _OnAntiTrigger(ReactionContext reactionContext, TargetedReactionFilter filter, int capturedValue)
	{
		if (filter.IsValid(reactionContext, context))
		{
			MarkAsFinished();
		}
	}

	public override void _Register()
	{
		foreach (AAction.Trigger trigger in data.triggers)
		{
			trigger.Register(context);
			trigger.onTrigger += _OnTrigger;
		}
		foreach (AAction.Trigger antiTrigger in data.antiTriggers)
		{
			antiTrigger.Register(context);
			antiTrigger.onTrigger += _OnAntiTrigger;
		}
	}

	public override void _Unregister()
	{
		foreach (AAction.Trigger trigger in data.triggers)
		{
			trigger.Unregister(context);
			trigger.onTrigger -= _OnTrigger;
		}
		foreach (AAction.Trigger antiTrigger in data.antiTriggers)
		{
			antiTrigger.Unregister(context);
			antiTrigger.onTrigger -= _OnAntiTrigger;
		}
	}

	public void MarkAsFinished()
	{
		this.Unregister();
		ProfileManager.progress.tutorials.write.Add(_dataRef);
	}
}
