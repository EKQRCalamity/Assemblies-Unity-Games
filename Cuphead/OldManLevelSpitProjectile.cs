using System.Collections;
using UnityEngine;

public class OldManLevelSpitProjectile : AbstractProjectile
{
	private const float OFFSCREEN_OFFSET = 100f;

	private Vector3 speed;

	private float gravity;

	private float stopPosX;

	private float apexTime;

	[SerializeField]
	private float firstSmokeDelay = 1f;

	[SerializeField]
	private float smokeDelay = 0.05f;

	private float smokeTimer;

	private int count;

	public void Move(Vector3 position, float speedX, float speedY, float stopPosX, float gravity, float apexTime, int count)
	{
		base.transform.position = position;
		speed = new Vector3(speedX, speedY);
		this.stopPosX = stopPosX;
		this.gravity = gravity;
		this.apexTime = apexTime;
		smokeTimer = firstSmokeDelay;
		this.count = count;
		StartCoroutine(move_cr());
		StartCoroutine(changeAnimations_cr());
		SFX_OMM_MouthCauldron_ProjLoop();
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	public override void SetParryable(bool parryable)
	{
		base.SetParryable(parryable);
		GetComponent<SpriteRenderer>().color = ((!parryable) ? Color.white : Color.magenta);
	}

	private IEnumerator changeAnimations_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, apexTime + 11f / 48f);
		base.animator.SetTrigger("OnApex");
		yield return null;
	}

	private IEnumerator move_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 11f / 48f);
		while (base.transform.position.x > stopPosX)
		{
			speed += new Vector3(gravity * CupheadTime.FixedDelta, 0f);
			base.transform.Translate(speed * CupheadTime.FixedDelta);
			yield return new WaitForFixedUpdate();
		}
		float time = 0.6f;
		float t = 0f;
		while (base.transform.position.x > (float)Level.Current.Left - 100f)
		{
			if (base.transform.position.x <= (float)Level.Current.Left)
			{
				SFX_OMM_MouthCauldron_ProjLoopEnd();
			}
			if (speed.y > 0f)
			{
				speed.y = Mathf.Lerp(speed.y, 0f, t / time);
				t += CupheadTime.FixedDelta;
			}
			base.transform.Translate(speed * CupheadTime.FixedDelta);
			yield return new WaitForFixedUpdate();
		}
		SFX_OMM_MouthCauldron_ProjLoopEnd();
		this.Recycle();
	}

	protected override void Update()
	{
		base.Update();
		smokeTimer -= CupheadTime.Delta;
		if (smokeTimer <= 0f)
		{
			smokeTimer += smokeDelay;
			((OldManLevel)Level.Current).CreateFX(base.transform.position, isSparkle: false, base.CanParry);
		}
	}

	private void AnimationEvent_SFX_OMM_MouthCauldron_ProjStart()
	{
		AudioManager.Play("sfx_dlc_omm_mouthcauldron_projectile_loop_start");
		emitAudioFromObject.Add("sfx_dlc_omm_mouthcauldron_projectile_loop_start");
	}

	private void SFX_OMM_MouthCauldron_ProjLoop()
	{
		AudioManager.PlayLoop("sfx_dlc_omm_mouthcauldron_projectile_loop_0" + count);
		emitAudioFromObject.Add("sfx_dlc_omm_mouthcauldron_projectile_loop_0" + count);
	}

	private void AnimationEvent_SFX_OMM_MouthCauldron_ProjHitPlayer()
	{
		SFX_OMM_MouthCauldron_ProjLoopEnd();
		AudioManager.Play("sfx_dlc_omm_mouthcauldron_projectile_damageplayer");
		emitAudioFromObject.Add("sfx_dlc_omm_mouthcauldron_projectile_damageplayer");
	}

	private void SFX_OMM_MouthCauldron_ProjLoopEnd()
	{
		AudioManager.Stop("sfx_dlc_omm_mouthcauldron_projectile_loop_0" + count);
	}
}
