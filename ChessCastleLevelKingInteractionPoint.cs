public class ChessCastleLevelKingInteractionPoint : DialogueInteractionPoint
{
	public void BeginDialogue()
	{
		Activate();
		speechBubble.waitForRealease = false;
	}

	protected override void StartAnimation()
	{
		((ChessCastleLevel)Level.Current).StartTalkAnimation();
	}

	protected override void EndAnimation()
	{
		((ChessCastleLevel)Level.Current).EndTalkAnimation();
	}
}
