using System.Collections;

public class GameStepAdventureConfirmResults : GameStep
{
	private bool _shouldShowReplayOption;

	private float _elapsedTime;

	public GameStepAdventureConfirmResults(bool shouldShowReplayOption)
	{
		_shouldShowReplayOption = shouldShowReplayOption;
	}

	private void _OnRewardClick(RewardPile pile, ATarget card)
	{
		if (card is AdventureResultCard)
		{
			_OnConfirm();
		}
	}

	private void _OnConfirm()
	{
		base.finished = true;
	}

	protected override void OnEnable()
	{
		base.state.rewardDeck.layout.onPointerClick += _OnRewardClick;
		base.view.onConfirmPressed += _OnConfirm;
		base.view.onBackPressed += _OnConfirm;
		foreach (ATarget card in base.state.rewardDeck.GetCards(RewardPile.Results))
		{
			if (card is AdventureResultCard)
			{
				card.view.RequestGlow(this, Colors.TARGET);
			}
		}
	}

	protected override IEnumerator Update()
	{
		while (!base.finished)
		{
			yield return null;
		}
	}

	protected override void LateUpdate()
	{
		if (AGameStepTurn.TickTutorialTimer(ref _elapsedTime, 10f))
		{
			base.view.LogMessage(PlayerTurnTutorial.ConfirmAdventureResults.Localize());
		}
	}

	protected override void OnDisable()
	{
		base.state.rewardDeck.layout.onPointerClick -= _OnRewardClick;
		base.view.onConfirmPressed -= _OnConfirm;
		base.view.onBackPressed -= _OnConfirm;
	}

	public override void OnCompletedSuccessfully()
	{
		AppendGroup(new GameStepGroupRewards(AdventureEndType.Victory));
		AppendStep(new GameStepContinue(_shouldShowReplayOption));
	}
}
