public class GameStepRemoveSummon : GameStepAAction
{
	private Ability _summon;

	private bool _isBeingReplaced;

	protected override bool shouldBeCanceled => _summon == null;

	public GameStepRemoveSummon(AAction action, ActionContext context, Ability summon, bool isBeingReplaced)
		: base(action, context)
	{
		_summon = summon;
		_isBeingReplaced = isBeingReplaced;
	}

	protected override void OnFirstEnabled()
	{
		base.context.gameState.SignalSummonRemoved(_summon, isFinishedBeingRemoved: false, _isBeingReplaced);
	}

	public override void Start()
	{
		_summon.Remove(base.context);
	}

	protected override void End()
	{
		base.context.gameState.SignalSummonRemoved(_summon, isFinishedBeingRemoved: true, _isBeingReplaced);
	}
}
