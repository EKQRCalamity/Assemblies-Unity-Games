using System.Collections;
using UnityEngine;

public class OldManLevelBleachers : AbstractPausableComponent
{
	[SerializeField]
	private GameObject gnomeBleacherRight;

	[SerializeField]
	private Transform gnomeBleacherRightEnd;

	[SerializeField]
	private GameObject gnomeBleacherLeft;

	[SerializeField]
	private Transform gnomeBleacherLeftEnd;

	[SerializeField]
	private OldManLevel level;

	[SerializeField]
	private float enterStepTime = 0.6f;

	[SerializeField]
	private float enterStepPause = 0.1f;

	[SerializeField]
	private float exitStepTime = 0.3f;

	[SerializeField]
	private float exitStepPause = 0.05f;

	[SerializeField]
	private float offset = 0.1f;

	private void Start()
	{
		StartCoroutine(move_bleachers_cr());
	}

	private IEnumerator move_bleachers_cr()
	{
		Vector3 rightStartPos = gnomeBleacherRight.transform.localPosition;
		Vector3 leftStartPos = gnomeBleacherLeft.transform.localPosition;
		Vector3 rightStepStartPos = rightStartPos;
		Vector3 leftStepStartPos = leftStartPos;
		SFX_OMM_P2_PuppetBleachersRaiseUp();
		SFX_OMM_BleachersCrowdLoop();
		yield return null;
		AudioManager.FadeSFXVolume("sfx_dlc_omm_p2_bleacherscrowd_loop", 0.15f, 0.5f);
		for (int j = 0; j < 3; j++)
		{
			float t = 0f;
			float time = enterStepTime;
			Vector3 rightEndPos = Vector3.Lerp(rightStartPos, gnomeBleacherRightEnd.position, 0.5f + (float)j * 0.25f);
			Vector3 leftEndPos = Vector3.Lerp(leftStartPos, gnomeBleacherLeftEnd.position, 0.5f + (float)j * 0.25f);
			while (t < time + offset)
			{
				t += (float)CupheadTime.Delta;
				gnomeBleacherRight.transform.localPosition = Vector3.Lerp(rightStepStartPos, rightEndPos, EaseUtils.EaseOutBounce(0f, 1f, Mathf.Clamp((t - offset) / time, 0f, 1f)));
				gnomeBleacherLeft.transform.localPosition = Vector3.Lerp(leftStepStartPos, leftEndPos, EaseUtils.EaseOutBounce(0f, 1f, Mathf.Clamp(t / time, 0f, 1f)));
				yield return null;
			}
			rightStepStartPos = gnomeBleacherRight.transform.localPosition;
			leftStepStartPos = gnomeBleacherLeft.transform.localPosition;
			yield return CupheadTime.WaitForSeconds(this, enterStepPause);
		}
		while (level.InPhase2())
		{
			yield return null;
		}
		SFX_OMM_P2_End_BleacherPuppetsLower();
		AudioManager.FadeSFXVolume("sfx_dlc_omm_p2_bleacherscrowd_loop", 0f, 1.5f);
		for (int i = 0; i < 3; i++)
		{
			float t = 0f;
			float time = exitStepTime;
			Vector3 rightEndPos2 = Vector3.Lerp(gnomeBleacherRightEnd.position, rightStartPos, (float)(i + 1) * 0.333f);
			Vector3 leftEndPos2 = Vector3.Lerp(gnomeBleacherLeftEnd.position, leftStartPos, (float)(i + 1) * 0.333f);
			while (t < time + offset)
			{
				t += (float)CupheadTime.Delta;
				gnomeBleacherRight.transform.localPosition = Vector3.Lerp(rightStepStartPos, rightEndPos2, EaseUtils.EaseOutElastic(0f, 1f, Mathf.Clamp((t - offset) / time, 0f, 1f)));
				gnomeBleacherLeft.transform.localPosition = Vector3.Lerp(leftStepStartPos, leftEndPos2, EaseUtils.EaseOutElastic(0f, 1f, Mathf.Clamp(t / time, 0f, 1f)));
				yield return null;
			}
			rightStepStartPos = gnomeBleacherRight.transform.localPosition;
			leftStepStartPos = gnomeBleacherLeft.transform.localPosition;
			yield return CupheadTime.WaitForSeconds(this, exitStepPause);
		}
		base.gameObject.SetActive(value: false);
		yield return null;
	}

	private void SFX_OMM_P2_PuppetBleachersRaiseUp()
	{
		AudioManager.Play("sfx_dlc_omm_p2_puppet_bleachersraiseup");
		emitAudioFromObject.Add("sfx_dlc_omm_p2_puppet_bleachersraiseup");
	}

	private void SFX_OMM_BleachersCrowdLoop()
	{
		AudioManager.FadeSFXVolume("sfx_dlc_omm_p2_bleacherscrowd_loop", 0.001f, 0.001f);
		AudioManager.PlayLoop("sfx_dlc_omm_p2_bleacherscrowd_loop");
		emitAudioFromObject.Add("sfx_dlc_omm_p2_bleacherscrowd_loop");
	}

	private void SFX_OMM_P2_End_BleacherPuppetsLower()
	{
		AudioManager.Stop("sfx_dlc_omm_p2_bleacherscrowd_loop");
		AudioManager.Play("sfx_dlc_omm_p2_end_bleacherpuppetslower");
		emitAudioFromObject.Add("sfx_dlc_omm_p2_end_bleacherpuppetslower");
	}
}
