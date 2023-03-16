public class RumRunnersLevelPh2StartAnimation : AbstractPausableComponent
{
	public bool showBug { get; private set; }

	public bool dropped { get; private set; }

	private void animationEvent_StartWeb()
	{
		base.animator.Play("Loop", 1);
	}

	private void animationEvent_EndWeb()
	{
		base.animator.Play("Off", 1);
	}

	private void animationEvent_ShowBug()
	{
		showBug = true;
	}

	private void animationEvent_RopeDrop()
	{
		dropped = true;
	}

	private void AnimationEvent_SFX_RUMRUN_ExitPhase1_SpiderReturns()
	{
		AudioManager.Play("sfx_DLC_RUMRUN_ExitPhase1_SpiderReturns");
		emitAudioFromObject.Add("sfx_DLC_RUMRUN_ExitPhase1_SpiderReturns");
	}

	private void AnimationEvent_SFX_RUMRUN_ExitPhase1_GrammoDrop()
	{
		AudioManager.Play("sfx_dlc_rumrun_exitphase1_grammodrop");
		emitAudioFromObject.Add("sfx_dlc_rumrun_exitphase1_grammodrop");
	}
}
