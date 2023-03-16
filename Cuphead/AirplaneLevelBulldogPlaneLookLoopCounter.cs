using System.Collections;
using UnityEngine;

public class AirplaneLevelBulldogPlaneLookLoopCounter : MonoBehaviour
{
	[SerializeField]
	private Animator bullDogPlane;

	private void OnDestroy()
	{
		WORKAROUND_NullifyFields();
	}

	private void aniEvent_IncreaseIdleLookLoopCount()
	{
		bullDogPlane.SetInteger("IdleLoopCount", bullDogPlane.GetInteger("IdleLoopCount") + 1);
	}

	private void AniEvent_RecedeIntoDistance()
	{
		StartCoroutine(recede_cr());
	}

	private IEnumerator recede_cr()
	{
		float startTime = bullDogPlane.GetCurrentAnimatorStateInfo(0).normalizedTime;
		Vector3 startPos = base.transform.position;
		Vector3 endPos2 = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y - 100f, base.transform.position.z);
		endPos2 = Vector3.Lerp(startPos, endPos2, 0.8f);
		while (bullDogPlane.GetCurrentAnimatorStateInfo(0).IsName("Death"))
		{
			float t = Mathf.InverseLerp(startTime, 1f, bullDogPlane.GetCurrentAnimatorStateInfo(0).normalizedTime);
			base.transform.position = Vector3.Lerp(startPos, endPos2, EaseUtils.EaseInSine(0f, 1f, t));
			yield return null;
		}
	}

	private void AnimationEvent_SFX_DOGFIGHT_Intro_BulldogPlaneFlyby()
	{
		AudioManager.Play("sfx_dlc_dogfight_bulldogplane_introflyby");
	}

	private void AnimationEvent_SFX_DOGFIGHT_Bulldog_EjectDown()
	{
		AudioManager.Play("sfx_dlc_dogfight_p1_bulldog_ejectdown");
	}

	private void AnimationEvent_SFX_DOGFIGHT_Bulldog_EjectUp()
	{
		AudioManager.Play("sfx_dlc_dogfight_p1_bulldog_ejectUp");
	}

	private void AnimationEvent_SFX_DOGFIGHT_Bulldog_EjectLeverPull()
	{
		AudioManager.Play("sfx_dlc_dogfight_p1_bulldog_ejectleverpull");
	}

	private void AnimationEvent_SFX_DOGFIGHT_Bulldog_LandsCockpit()
	{
		AudioManager.Play("sfx_dlc_dogfight_p1_bulldog_landscockpit");
	}

	private void SFX_DOGFIGHT_Bulldog_WingExtend_WhimperOut()
	{
		AudioManager.Play("sfx_dlc_dogfight_p1_bulldog_whimperout");
	}

	private void SFX_DOGFIGHT_Bulldog_WingExtend_WhistleOut()
	{
		AudioManager.Play("sfx_DLC_Dogfight_P1_Bulldog_Whistle_Out");
	}

	private void AnimationEvent_SFX_DOGFIGHT_BulldogPlane_WingStretchOut()
	{
		AudioManager.Play("sfx_DLC_Dogfight_P1_Bulldog_WingStretch_Out");
	}

	private void AnimationEvent_SFX_DOGFIGHT_BulldogPlane_WingStretchIn()
	{
		AudioManager.Play("sfx_DLC_Dogfight_P1_Bulldog_WingStretch_In");
	}

	private void AnimationEvent_SFX_DOGFIGHT_BulldogPlane_DiePlaneExplodes()
	{
		AudioManager.Play("sfx_dlc_dogfight_p1_bulldog_planeexplodes");
	}

	private void AnimationEvent_SFX_DOGFIGHT_BulldogPlane_DiePlaneExplodes_VO()
	{
		AudioManager.Play("sfx_DLC_Dogfight_P1_Bulldog_PlaneExplodes_VO");
		CupheadLevelCamera.Current.Shake(30f, 7f / 24f);
	}

	private void WORKAROUND_NullifyFields()
	{
		bullDogPlane = null;
	}
}
