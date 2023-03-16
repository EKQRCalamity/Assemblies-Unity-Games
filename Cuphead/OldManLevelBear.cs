using System.Collections;
using UnityEngine;

public class OldManLevelBear : BasicDamageDealingObject
{
	private const float FALL_TIME = 19f / 24f;

	[SerializeField]
	private SpriteRenderer rend;

	public bool thrown;

	public IEnumerator fall_cr()
	{
		thrown = true;
		Vector3 startPos = new Vector3(-520f, 525f);
		Vector3 endPos = new Vector3(-600f, -90f);
		Vector3 startScale = new Vector3(1f, 1f);
		Vector3 endScale = new Vector3(0.15f, 0.15f);
		base.transform.position = startPos;
		base.animator.Play("Falling");
		float t = 0f;
		rend.sortingLayerName = "Background";
		rend.sortingOrder = 1000;
		while (t < 19f / 24f)
		{
			yield return CupheadTime.WaitForSeconds(this, 1f / 48f);
			t += 1f / 48f;
			float tn = t / (19f / 24f);
			base.transform.position = new Vector3(Mathf.Lerp(startPos.x, endPos.x, EaseUtils.EaseOutSine(0f, 1f, tn)), Mathf.Lerp(startPos.y, endPos.y, EaseUtils.EaseInSine(0f, 1f, tn)));
			base.transform.localScale = Vector3.Lerp(startScale, endScale, EaseUtils.EaseOutSine(0f, 1f, tn));
		}
		yield return null;
		base.transform.localScale = new Vector3(1f, 1f);
		base.animator.Play("FX");
		base.animator.Update(0f);
		yield return base.animator.WaitForAnimationToEnd(this, "FX");
		rend.enabled = false;
		rend.sortingOrder = 0;
		rend.sortingLayerName = "Projectiles";
	}

	private void AnimationEvent_SFX_OMM_BearAttackClawing()
	{
		AudioManager.Play("sfx_dlc_omm_bearattack_clawing");
		emitAudioFromObject.Add("sfx_dlc_omm_bearattack_clawing");
	}

	private void AnimationEvent_SFX_OMM_BearAttackGrowling()
	{
		AudioManager.Play("sfx_dlc_omm_bearattack_growling");
		emitAudioFromObject.Add("sfx_dlc_omm_bearattack_growling");
	}

	private void AnimationEvent_SFX_OMM_BearAttackEnd()
	{
		AudioManager.Stop("sfx_dlc_omm_bearattack_growling");
		AudioManager.Play("sfx_dlc_omm_bearattack_end");
		emitAudioFromObject.Add("sfx_dlc_omm_bearattack_end");
	}
}
