public class PirateLevelDogFishScope : AbstractMonoBehaviour
{
	public void In()
	{
		base.animator.Play("In");
	}

	private void SoundDogfishPeriStart()
	{
		AudioManager.Play("level_pirate_periscope_warning");
	}
}
