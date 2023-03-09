using System;
using System.Collections;
using UnityEngine;

public class SaltbakerLevelLime : SaltbakerLevelPhaseOneProjectile
{
	private LevelProperties.Saltbaker.Limes properties;

	private bool isDead;

	private bool onLeft;

	private bool isHigh;

	private int sfxID;

	private Vector3 pivot;

	public virtual SaltbakerLevelLime Init(Vector3 position, bool onLeft, bool isHigh, LevelProperties.Saltbaker.Limes properties, int id, int anim)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = position;
		this.properties = properties;
		this.onLeft = onLeft;
		this.isHigh = isHigh;
		Move();
		base.animator.Play(anim.ToString());
		sfxID = id;
		SFX_SALTBAKER_P1_LimeProjectileLoop();
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

	private new void Move()
	{
		StartCoroutine(move_cr());
	}

	protected override void OnDestroy()
	{
		AudioManager.Stop("sfx_dlc_saltbaker_p1_lime_projectile_loop_" + (sfxID + 1));
		base.OnDestroy();
	}

	private IEnumerator move_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		float startPosX = ((!onLeft) ? Level.Current.Right : Level.Current.Left);
		float curveStartY = ((!isHigh) ? properties.lowStartY : properties.highStartY);
		float curveEndY = ((!isHigh) ? properties.lowEndY : properties.highEndY);
		float boomerangSpeed = properties.straightSpeed;
		float distToTurn = properties.distToTurn;
		float loopSizeX = 100f;
		float gravity = properties.straightGravity;
		float speed = boomerangSpeed;
		float pivotY = Mathf.Lerp(curveStartY, curveEndY, 0.5f);
		float pivotX = ((!onLeft) ? (0f - distToTurn) : distToTurn);
		float loopSizeY = Mathf.Abs(pivotY - curveStartY);
		pivot = new Vector3(pivotX, pivotY);
		float offset = ((!isHigh) ? (0f - loopSizeY) : loopSizeY);
		base.transform.SetPosition(startPosX, pivot.y + offset);
		if (onLeft)
		{
			while (base.transform.position.x < distToTurn)
			{
				base.transform.position += Vector3.right * speed * CupheadTime.FixedDelta;
				HandleShadow(0f, 40f);
				yield return wait;
			}
		}
		else
		{
			while (base.transform.position.x > 0f - distToTurn)
			{
				base.transform.position += Vector3.left * speed * CupheadTime.FixedDelta;
				HandleShadow(0f, 40f);
				yield return wait;
			}
		}
		float angleToStopAt = (float)Math.PI;
		float angle2 = 0f;
		float angleStartSpeed = properties.angleSpeedToLerp.min;
		float angleEndSpeed = properties.angleSpeedToLerp.max;
		float timeTolerp = properties.angleLerpTime;
		float t = 0f;
		angle2 *= (float)Math.PI / 180f;
		while (angle2 < angleToStopAt)
		{
			t += CupheadTime.FixedDelta;
			float s = Mathf.Lerp(angleStartSpeed, angleEndSpeed, t / timeTolerp);
			angle2 += s * CupheadTime.FixedDelta;
			Vector3 handleRotationX = ((!onLeft) ? new Vector3((0f - Mathf.Sin(angle2)) * loopSizeX, 0f, 0f) : new Vector3(Mathf.Sin(angle2) * loopSizeX, 0f, 0f));
			Vector3 handleRotationY = ((!isHigh) ? new Vector3(0f, (0f - Mathf.Cos(angle2)) * loopSizeY, 0f) : new Vector3(0f, Mathf.Cos(angle2) * loopSizeY, 0f));
			base.transform.position = pivot;
			base.transform.position += handleRotationX + handleRotationY;
			HandleShadow(0f, 40f);
			yield return new WaitForFixedUpdate();
		}
		speed = boomerangSpeed;
		if (onLeft)
		{
			while (base.transform.position.x > (float)Level.Current.Left - 300f)
			{
				speed += gravity * CupheadTime.FixedDelta;
				base.transform.position += Vector3.left * speed * CupheadTime.FixedDelta;
				HandleShadow(0f, 40f);
				yield return wait;
			}
		}
		else
		{
			while (base.transform.position.x < (float)Level.Current.Right + 300f)
			{
				speed += gravity * CupheadTime.FixedDelta;
				base.transform.position += Vector3.right * speed * CupheadTime.FixedDelta;
				HandleShadow(0f, 40f);
				yield return wait;
			}
		}
		yield return CupheadTime.WaitForSeconds(this, 0.25f);
		AudioManager.Stop("sfx_dlc_saltbaker_p1_lime_projectile_loop_" + (sfxID + 1));
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Gizmos.DrawWireSphere(pivot, 10f);
	}

	private void SFX_SALTBAKER_P1_LimeProjectileLoop()
	{
		string key = "sfx_dlc_saltbaker_p1_lime_projectile_loop_" + (sfxID + 1);
		AudioManager.PlayLoop(key);
		emitAudioFromObject.Add(key);
	}
}
