using System.Collections;
using UnityEngine;

public class PlatformingLevelReverseGravitySwitch : ParrySwitch
{
	[SerializeField]
	private float spinTimer;

	public override void OnParryPostPause(AbstractPlayerController player)
	{
		base.OnParryPostPause(player);
		LevelPlayerController levelPlayerController = player as LevelPlayerController;
		levelPlayerController.motor.SetGravityReversed(!levelPlayerController.motor.GravityReversed);
		StartCoroutine(start_spin_cr(levelPlayerController));
		StartParryCooldown();
	}

	private IEnumerator start_spin_cr(LevelPlayerController player)
	{
		base.animator.SetBool("IsSpin", value: true);
		base.animator.SetBool("IsUp", player.motor.GravityReversed);
		float t = 0f;
		while (t < spinTimer)
		{
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		base.animator.SetBool("IsSpin", value: false);
		yield return null;
	}
}
