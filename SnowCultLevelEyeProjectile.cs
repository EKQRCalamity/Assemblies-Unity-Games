using System;
using System.Collections;
using UnityEngine;

public class SnowCultLevelEyeProjectile : AbstractProjectile
{
	private LevelProperties.SnowCult.EyeAttack properties;

	private Vector3 endPos;

	private Vector3 startPos;

	private float angle;

	private bool onRight;

	private bool upsideDown;

	public bool readyToOpenMouth;

	public bool readyToCloseMouth;

	[SerializeField]
	private Animator beamAnimator;

	[SerializeField]
	private float openMouthDistance = 400f;

	[SerializeField]
	private float beamEndDistance = 200f;

	[SerializeField]
	private float animatorTakeoverDistance = 31f;

	public SnowCultLevelJackFrost main;

	private Coroutine beamCR;

	private bool controlledByParent;

	[SerializeField]
	private GameObject shadow;

	private Vector3 vel;

	private Vector3 lastPos;

	private new bool dead;

	public bool IsGone { get; private set; }

	public virtual SnowCultLevelEyeProjectile Init(Vector3 startPos, Vector3 endPos, bool onRight, bool upsideDown, LevelProperties.SnowCult.EyeAttack properties)
	{
		ResetLifetime();
		ResetDistance();
		this.startPos = startPos;
		base.transform.position = startPos;
		this.properties = properties;
		this.endPos = endPos;
		this.onRight = onRight;
		base.transform.localScale = new Vector3(onRight ? 1 : (-1), 1f);
		this.upsideDown = upsideDown;
		readyToOpenMouth = false;
		angle = 0f;
		IsGone = false;
		StartCoroutine(move_cr());
		beamCR = StartCoroutine(beam_cr());
		return this;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private IEnumerator beam_cr()
	{
		beamAnimator.Play("AuraStart");
		float delay = properties.initialBeamDelay.RandomFloat();
		while (!IsGone)
		{
			yield return CupheadTime.WaitForSeconds(this, delay);
			delay = properties.beamDelay;
			beamAnimator.SetBool("Attack", value: true);
			SFX_SNOWCULT_JackFrostEyeballZap();
			yield return CupheadTime.WaitForSeconds(this, properties.beamDuration);
			beamAnimator.SetBool("Attack", value: false);
		}
	}

	private IEnumerator move_in_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		if (!onRight)
		{
			while (base.transform.position.x < properties.distanceToTurn)
			{
				lastPos = base.transform.position;
				base.transform.position += Vector3.right * properties.eyeStraightSpeed * CupheadTime.FixedDelta;
				yield return wait;
			}
		}
		else
		{
			while (base.transform.position.x > 0f - properties.distanceToTurn)
			{
				lastPos = base.transform.position;
				base.transform.position += Vector3.left * properties.eyeStraightSpeed * CupheadTime.FixedDelta;
				yield return wait;
			}
		}
		yield return null;
	}

	private IEnumerator move_out_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		if (!onRight)
		{
			while (base.transform.position.x < endPos.x - openMouthDistance)
			{
				lastPos = base.transform.position;
				base.transform.position += Vector3.right * properties.eyeStraightSpeed * CupheadTime.FixedDelta;
				yield return wait;
			}
		}
		else
		{
			while (base.transform.position.x > endPos.x + openMouthDistance)
			{
				lastPos = base.transform.position;
				base.transform.position += Vector3.left * properties.eyeStraightSpeed * CupheadTime.FixedDelta;
				yield return wait;
			}
		}
		readyToOpenMouth = true;
		if (!onRight)
		{
			while (base.transform.position.x < endPos.x - beamEndDistance)
			{
				lastPos = base.transform.position;
				base.transform.position += Vector3.right * properties.eyeStraightSpeed * CupheadTime.FixedDelta;
				yield return wait;
			}
		}
		else
		{
			while (base.transform.position.x > endPos.x + beamEndDistance)
			{
				lastPos = base.transform.position;
				base.transform.position += Vector3.left * properties.eyeStraightSpeed * CupheadTime.FixedDelta;
				yield return wait;
			}
		}
		StopCoroutine(beamCR);
		beamAnimator.SetBool("Attack", value: false);
		beamAnimator.SetTrigger("End");
		if (!onRight)
		{
			while (base.transform.position.x < endPos.x - animatorTakeoverDistance)
			{
				lastPos = base.transform.position;
				base.transform.position += Vector3.right * properties.eyeStraightSpeed * CupheadTime.FixedDelta;
				yield return wait;
			}
		}
		else
		{
			while (base.transform.position.x > endPos.x + animatorTakeoverDistance)
			{
				lastPos = base.transform.position;
				base.transform.position += Vector3.left * properties.eyeStraightSpeed * CupheadTime.FixedDelta;
				yield return wait;
			}
		}
		readyToCloseMouth = true;
		controlledByParent = true;
		yield return null;
		shadow.SetActive(value: true);
	}

	private IEnumerator move_cr()
	{
		SFX_SNOWCULT_JackFrostEyeballLoop();
		float loopSpeed = properties.eyeCurveSpeed;
		float pivotY = (startPos.y + endPos.y) / 2f;
		float loopSizeY = Mathf.Abs(startPos.y - endPos.y) / 2f;
		float loopSizeX = loopSizeY;
		float pivotX = ((!onRight) ? properties.distanceToTurn : (0f - properties.distanceToTurn));
		Vector3 pivot = new Vector3(pivotX, pivotY);
		float angleToStopAt = (float)Math.PI;
		if (!upsideDown)
		{
			base.transform.SetPosition(null, pivot.y - loopSizeY);
		}
		else
		{
			base.transform.SetPosition(null, pivot.y + loopSizeY);
		}
		angle *= (float)Math.PI / 180f;
		yield return StartCoroutine(move_in_cr());
		while (angle < angleToStopAt)
		{
			angle += loopSpeed * CupheadTime.FixedDelta;
			Vector3 handleRotationX = (onRight ? new Vector3((0f - Mathf.Sin(angle)) * loopSizeX, 0f, 0f) : new Vector3(Mathf.Sin(angle) * loopSizeX, 0f, 0f));
			Vector3 handleRotationY = (upsideDown ? new Vector3(0f, Mathf.Cos(angle) * loopSizeY, 0f) : new Vector3(0f, (0f - Mathf.Cos(angle)) * loopSizeY, 0f));
			lastPos = base.transform.position;
			base.transform.position = pivot;
			base.transform.position += handleRotationX + handleRotationY;
			yield return new WaitForFixedUpdate();
		}
		onRight = !onRight;
		StartCoroutine(move_out_cr());
	}

	public void ReturnToSnowflake()
	{
		SFX_SNOWCULT_JackFrostEyeballLoopStop();
		this.Recycle();
		IsGone = true;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (dead)
		{
			base.transform.position += vel * CupheadTime.FixedDelta;
		}
	}

	private void LateUpdate()
	{
		if (main.dead != dead)
		{
			dead = true;
			vel /= CupheadTime.FixedDelta;
			SFX_SNOWCULT_JackFrostEyeballLoopStop();
			StopAllCoroutines();
		}
		else if (!dead)
		{
			vel = base.transform.position - lastPos;
		}
		if (controlledByParent)
		{
			base.transform.position = main.eyeProjectileGuide.position;
		}
	}

	private void SFX_SNOWCULT_JackFrostEyeballLoop()
	{
		AudioManager.PlayLoop("sfx_dlc_snowcult_p3_snowflake_eyeball_attack_loop");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p3_snowflake_eyeball_attack_loop");
	}

	private void SFX_SNOWCULT_JackFrostEyeballLoopStop()
	{
		AudioManager.Stop("sfx_dlc_snowcult_p3_snowflake_eyeball_attack_loop");
	}

	private void SFX_SNOWCULT_JackFrostEyeballZap()
	{
		AudioManager.Play("sfx_dlc_snowcult_p3_snowflake_eyeball_zap");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p3_snowflake_eyeball_zap");
	}
}
