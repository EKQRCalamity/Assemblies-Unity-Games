using System.Linq;

public class GameStepTopDeckInstructionComplete : GameStepAAction
{
	public TopDeckResult defaultResult;

	public GameStepTopDeckInstructionComplete(AAction action, ActionContext context, TopDeckResult defaultResult)
		: base(action, context)
	{
		this.defaultResult = defaultResult;
	}

	private void _OnWildValueChange(ResourceCard card, ResourceCard.WildContext wildContext)
	{
		if (card.pile == ResourceCard.Pile.TopDeckHand && card.faction == base.context.actor.faction)
		{
			_CalculateResult();
		}
	}

	private void _CalculateResult()
	{
		if (!base.hasStarted && base.context.actor is ACombatant aCombatant && SetPropertyUtility.SetStruct(ref aCombatant.activeTopDeckResult, GetNextSteps().OfType<GameStepTopDeckCondition>().FirstOrDefault((GameStepTopDeckCondition c) => c.result != TopDeckResult.None && c.IsValid())?.result ?? defaultResult))
		{
			TopDeckAction obj = base.action as TopDeckAction;
			if (obj == null || obj.topDeck.shouldSignalAboutToFail || aCombatant.activeTopDeckResult != TopDeckResult.Failure)
			{
				base.state.SignalTopDeckFinishedDrawing(base.context, aCombatant.activeTopDeckResult.Value);
			}
		}
	}

	private void _Unregister()
	{
		base.state.onWildValueChanged -= _OnWildValueChange;
	}

	protected override void OnFirstEnabled()
	{
		base.state.onWildValueChanged += _OnWildValueChange;
	}

	protected override void OnEnable()
	{
		_CalculateResult();
	}

	public override void Start()
	{
		_Unregister();
	}

	protected override void OnCanceled()
	{
		_Unregister();
	}
}
