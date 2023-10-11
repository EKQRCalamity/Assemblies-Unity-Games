using System.Linq;

public class GameStepTopDeckComplete : GameStepAAction
{
	public TopDeckResult defaultResult;

	public GameStepTopDeckComplete(AAction action, ActionContext context, TopDeckResult defaultResult)
		: base(action, context)
	{
		this.defaultResult = defaultResult;
	}

	protected override void End()
	{
		TopDeckResult? topDeckResult = null;
		foreach (GameStepTopDeckCondition item in base.group.GetPreviousStepsOfTypeUntilType<GameStepTopDeckCondition, GameStepTopDeckInstruction>(this))
		{
			if (item.ended && item.result != TopDeckResult.None)
			{
				topDeckResult = item.result;
				break;
			}
		}
		if (!topDeckResult.HasValue)
		{
			ProjectileMediaPack projectileMediaPack = ContentRef.Defaults[defaultResult];
			if (projectileMediaPack != null)
			{
				GameStepTopDeck gameStepTopDeck = base.group.GetPreviousStepsOfTypeUntilType<GameStepTopDeck, GameStepTopDeckInstruction>(this).FirstOrDefault((GameStepTopDeck topDeck) => topDeck.card != null);
				if (gameStepTopDeck != null)
				{
					ResourceCard card = gameStepTopDeck.card;
					if (card != null)
					{
						AppendStep(new GameStepProjectileMedia(projectileMediaPack, base.context.SetTarget(card)));
					}
				}
			}
		}
		defaultResult = topDeckResult ?? defaultResult;
	}

	protected override void OnDestroy()
	{
		base.context.GetTarget<ACombatant>(ActionContextTarget.Owner).activeTopDeckResult = null;
		TargetLineView.RemoveAll(TargetLineTags.TopDeck);
		foreach (GameStepTopDeck item in base.group.GetPreviousStepsOfTypeUntilType<GameStepTopDeck, GameStepTopDeckInstruction>(this).Reverse())
		{
			item.Discard();
		}
		base.state.SignalTopDeckComplete(base.context, defaultResult);
		GetPreviousSteps().OfType<GameStepTopDeckInstruction>().FirstOrDefault()?.RestorePreviousTopDeckHands();
	}
}
