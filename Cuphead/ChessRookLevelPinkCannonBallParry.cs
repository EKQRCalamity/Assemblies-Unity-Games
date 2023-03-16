using UnityEngine;

public class ChessRookLevelPinkCannonBallParry : AbstractProjectile
{
	[SerializeField]
	private ChessRookLevelPinkCannonBall main;

	public override float ParryMeterMultiplier => 0f;

	protected override void RandomizeVariant()
	{
	}

	protected override void SetTrigger(string trigger)
	{
	}

	public override void OnParry(AbstractPlayerController player)
	{
		main.GotParried(player);
	}
}
