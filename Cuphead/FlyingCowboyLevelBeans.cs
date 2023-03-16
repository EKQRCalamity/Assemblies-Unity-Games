using System.Collections;
using UnityEngine;

public class FlyingCowboyLevelBeans : AbstractProjectile
{
	[SerializeField]
	private GameObject[] versionA;

	[SerializeField]
	private GameObject[] versionB;

	public virtual void Init(Vector3 position, bool pointingUp, float speed, float extendTimer)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = position;
		GameObject[] array = ((!Rand.Bool()) ? versionB : versionA);
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			gameObject.SetActive(value: false);
		}
		if (!pointingUp)
		{
			base.animator.Play("BottomIdle");
			base.animator.Update(0f);
		}
		base.animator.Play(0, 0, Random.Range(0f, 1f));
		StartCoroutine(move_cr(speed));
		StartCoroutine(extend_cr(extendTimer));
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	private IEnumerator move_cr(float speed)
	{
		WaitForFixedUpdate wait = new WaitForFixedUpdate();
		while (true)
		{
			base.transform.position += new Vector3((0f - speed) * CupheadTime.FixedDelta, 0f);
			if (base.transform.position.x < -745f)
			{
				Object.Destroy(base.gameObject);
			}
			yield return wait;
		}
	}

	private IEnumerator extend_cr(float extendTimer)
	{
		yield return CupheadTime.WaitForSeconds(this, extendTimer);
		base.animator.SetTrigger("Extend");
	}

	private void SFX_COWGIRL_P3_CanPropellerLoop()
	{
		AudioManager.FadeSFXVolume("sfx_dlc_cowgirl_p3_canpropeller_loop", 0.4f, 0.5f);
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p3_canpropeller_loop");
	}

	private void AnimationEvent_SFX_COWGIRL_P3_CanUnfurl()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p3_canpropeller_unfurl");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p3_canpropeller_unfurl");
	}
}
