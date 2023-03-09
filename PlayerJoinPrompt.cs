public class PlayerJoinPrompt : FlashingPrompt
{
	protected override bool ShouldShow => PlayerManager.ShouldShowJoinPrompt;
}
