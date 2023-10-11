using System.Linq;

public class GameStepTopDeckCondition : GameStepAAction
{
	private TopDeckCondition _condition;

	private TopDeckResult _result;

	public TopDeckResult result => _result;

	public GameStepTopDeckCondition(AAction action, ActionContext context, TopDeckCondition condition, TopDeckResult result)
		: base(action, context)
	{
		_condition = condition;
		_result = result;
	}

	protected override void OnEnable()
	{
		if (!IsValid())
		{
			Cancel();
		}
	}

	protected override void End()
	{
		ProjectileMediaPack projectileMediaPack = ContentRef.Defaults[_result];
		if (projectileMediaPack == null)
		{
			return;
		}
		GameStepTopDeck gameStepTopDeck = base.contextGroup.GetPreviousSteps(this).OfType<GameStepTopDeck>().FirstOrDefault((GameStepTopDeck topDeck) => topDeck.card != null);
		if (gameStepTopDeck != null)
		{
			ResourceCard card = gameStepTopDeck.card;
			if (card != null)
			{
				AppendStep(new GameStepProjectileMedia(projectileMediaPack, base.context.SetTarget(card)));
			}
		}
	}

	protected override void OnCanceled()
	{
		CancelGroup();
	}

	public bool IsValid()
	{
		return _condition.IsValid(base.context);
	}
}
