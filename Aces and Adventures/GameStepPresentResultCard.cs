public class GameStepPresentResultCard : AGameStepPresentResult
{
	public AdventureResultCard result => _card as AdventureResultCard;

	protected override ProjectileMediaPack _clickMedia
	{
		get
		{
			VoiceManager.Instance.Play(base.state.player.view.transform, base.state.player.audio.character.victory[result.rank], interrupt: true);
			return result.rank.GetClickMedia();
		}
	}

	public GameStepPresentResultCard(AdventureResultCard resultCard)
		: base(resultCard)
	{
	}
}
