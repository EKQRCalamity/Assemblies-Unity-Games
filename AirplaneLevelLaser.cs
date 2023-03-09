using UnityEngine;

public class AirplaneLevelLaser : ParrySwitch
{
	[SerializeField]
	private Animator anim;

	public override void OnParryPrePause(AbstractPlayerController player)
	{
		base.OnParryPrePause(player);
		player.stats.ParryOneQuarter();
	}

	public override void OnParryPostPause(AbstractPlayerController player)
	{
		base.OnParryPostPause(player);
		StartParryCooldown();
		anim.Play("End");
	}
}
