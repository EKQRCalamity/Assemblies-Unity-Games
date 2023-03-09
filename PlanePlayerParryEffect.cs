public class PlanePlayerParryEffect : AbstractParryEffect
{
	private PlanePlayerController planePlayer;

	protected override bool IsHit => false;

	protected override void SetPlayer(AbstractPlayerController player)
	{
		base.SetPlayer(player);
		planePlayer = player as PlanePlayerController;
	}

	protected override void OnSuccess()
	{
		base.OnSuccess();
		planePlayer.parryController.OnParrySuccess();
	}
}
