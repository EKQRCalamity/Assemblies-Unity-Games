public class SallyStagePlayLevelFirewheel : AbstractPausableComponent
{
	public void PlaySound()
	{
		AudioManager.Play("sally_cherub_fireprop_move");
		emitAudioFromObject.Add("sally_cherub_fireprop_move");
	}
}
