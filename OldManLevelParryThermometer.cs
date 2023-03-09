public class OldManLevelParryThermometer : ParrySwitch
{
	public bool isActivated { get; private set; }

	public override void OnParryPrePause(AbstractPlayerController player)
	{
		isActivated = true;
		base.OnParryPrePause(player);
	}

	public override void OnParryPostPause(AbstractPlayerController player)
	{
		base.OnParryPostPause(player);
		isActivated = false;
		base.gameObject.SetActive(value: false);
	}
}
