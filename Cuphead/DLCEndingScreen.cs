public class DLCEndingScreen : DLCCutsceneScreen
{
	private void AniEvent_SwapChars()
	{
		((DLCEndingCutscene)cutscene).SwapChars();
	}

	private void AniEvent_IrisOut()
	{
		((DLCEndingCutscene)cutscene).IrisOut();
	}

	private void AniEvent_StartShake()
	{
		((DLCEndingCutscene)cutscene).StartShake();
	}

	private void AniEvent_StopShake()
	{
		((DLCEndingCutscene)cutscene).StopShake();
	}

	private void AniEvent_AdvanceMusic()
	{
		((DLCEndingCutscene)cutscene).AdvanceMusic();
	}

	private void AniEvent_ActivateChaliceRArm()
	{
		base.animator.Play("ChaliceArmRLoop", 1, 0f);
		base.animator.Update(0f);
	}

	private void AniEvent_LowerChaliceRArm()
	{
		base.animator.SetTrigger("Arm");
	}

	private void AniEvent_HideChaliceRArm()
	{
		base.animator.Play("None", 1, 0f);
		base.animator.Update(0f);
	}

	private void AnimEvent_SFX_Ending_BakeryGoesDown()
	{
		AudioManager.Play("sfx_DLC_Cutscene_Ending_BakeryGoesDown");
	}

	private void AnimEvent_SFX_Ending_ChaliceGlassBreak()
	{
		AudioManager.Play("sfx_DLC_Cutscene_Ending_ChaliceGlassBreak");
	}

	private void AnimEvent_SFX_Ending_ChaliceGlassShake()
	{
		AudioManager.Play("sfx_DLC_Cutscene_Ending_ChaliceGlassShake");
	}

	private void AnimEvent_SFX_Ending_ChaliceWink()
	{
		AudioManager.Play("sfx_DLC_Cutscene_Ending_ChaliceWink");
	}

	private void AnimEvent_SFX_Ending_ChaliceHug()
	{
		AudioManager.Play("sfx_dlc_cutscene_ending_chalicehug");
	}

	private void AnimEvent_SFX_Ending_CollapsingBegins()
	{
		AudioManager.Play("sfx_DLC_Cutscene_Ending_CollapsingBegins");
	}

	private void AnimEvent_SFX_Ending_EscapingBaker()
	{
		AudioManager.Play("sfx_DLC_Cutscene_Ending_EscapingBaker");
	}

	private void AnimEvent_SFX_Ending_EscapingGroup()
	{
		AudioManager.Play("sfx_DLC_Cutscene_Ending_EscapingGroup");
	}

	private void AnimEvent_SFX_Ending_BakerSit()
	{
		AudioManager.Play("sfx_DLC_Cutscene_Ending_BakerSit");
	}

	private void AnimEvent_SFX_Ending_RumbleLoopStart()
	{
		AudioManager.PlayLoop("sfx_DLC_Cutscene_Ending_Rumble_Loop");
		AudioManager.FadeSFXVolume("sfx_DLC_Cutscene_Ending_Rumble_Loop", 0.2f, 3f);
	}

	private void AnimEvent_SFX_Ending_RumbleLoopStop()
	{
		AudioManager.FadeSFXVolume("sfx_DLC_Cutscene_Ending_Rumble_Loop", 0f, 3f);
		AudioManager.Stop("sfx_DLC_Cutscene_Ending_Rumble_Loop");
	}
}
