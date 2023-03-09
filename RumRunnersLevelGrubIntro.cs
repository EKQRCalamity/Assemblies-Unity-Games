using UnityEngine;

public class RumRunnersLevelGrubIntro : AbstractPausableComponent
{
	[SerializeField]
	private SpriteRenderer rend;

	private void animationEvent_StartExit()
	{
		Animator component = GetComponent<Animator>();
		component.SetLayerWeight(1, 0f);
	}

	private void AnimationEvent_MoveToForeground()
	{
		rend.sortingLayerName = "Foreground";
		rend.sortingOrder = 200;
	}

	private void AnimationEvent_SFX_RUMRUN_FakeAnnouncer_BeginAhhh()
	{
		AudioManager.FadeSFXVolume("sfx_dlc_rumrun_vx_fakeannouncer_begin", 0f, 0.1f);
		AudioManager.Play("sfx_dlc_rumrun_vx_fakeannouncer_begin_ahhh");
	}

	private void AnimationEvent_SFX_RUMRUN_Intro_GrubFliesAway()
	{
		AudioManager.Play("sfx_dlc_rumrun_intro_grubfliesaway");
	}
}
