using System.Collections;
using UnityEngine;

public class SaltbakerLevelPhaseThreeToFourTransition : MonoBehaviour
{
	[SerializeField]
	private Animator anim;

	[SerializeField]
	private GameObject heart;

	public void StartSaltman()
	{
		anim.Play("Start");
	}

	public void StartHeart()
	{
		StartCoroutine(move_heart_cr());
		anim.Play("Heart", 1, 0f);
	}

	private IEnumerator move_heart_cr()
	{
		yield return anim.WaitForAnimationToStart(this, "HeartLoop", 1);
		Vector3 start = heart.transform.position;
		Vector3 end = start + Vector3.up * 300f;
		for (float t = 0f; t < 1f; t += 1f / 12f)
		{
			heart.transform.position = Vector3.Lerp(start, end, EaseUtils.EaseInSine(0f, 1f, t));
			yield return CupheadTime.WaitForSeconds(this, 1f / 12f);
		}
		base.enabled = false;
	}

	private void AnimationEvent_SFX_SALTB_Phase3to4_HeartRise()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p3top4transition_heartrise");
	}

	private void AnimationEvent_SFX_SALTB_Phase3to4_Transition()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p3top4transition");
	}

	private void AnimationEvent_SFX_SALTB_Phase3to4_TransitionStart()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p3top4transition_start");
	}
}
