using System.Collections;

public class GameStepWaitForCardTransition : GameStep
{
	public CardLayoutElement card;

	protected override bool shouldBeCanceled => !card;

	public GameStepWaitForCardTransition(CardLayoutElement card)
	{
		this.card = card;
	}

	protected override IEnumerator Update()
	{
		return card.WaitTillFinishedAnimating();
	}
}
