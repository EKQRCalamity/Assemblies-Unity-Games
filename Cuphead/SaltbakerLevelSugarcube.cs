using System;
using System.Collections;
using UnityEngine;

public class SaltbakerLevelSugarcube : SaltbakerLevelPhaseOneProjectile
{
	private LevelProperties.Saltbaker.Sugarcubes properties;

	private Vector3 root;

	private bool onLeft;

	private float phase;

	protected override void OnDieDistance()
	{
	}

	protected override void OnDieLifetime()
	{
	}

	public virtual SaltbakerLevelSugarcube Init(Vector2 pos, bool onLeft, LevelProperties.Saltbaker.Sugarcubes properties, float phase, SaltbakerLevelSaltbaker parent, int anim, bool parryable)
	{
		ResetLifetime();
		ResetDistance();
		root = pos;
		base.transform.position = pos;
		this.properties = properties;
		this.onLeft = onLeft;
		Move();
		this.phase = phase * ((float)Math.PI / 180f);
		base.animator.Play((!parryable) ? anim.ToString() : "Pink");
		SetParryable(parryable);
		return this;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	private new void Move()
	{
		StartCoroutine(move_cr());
	}

	private IEnumerator move_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		float xVelocity = properties.sineFreq * properties.sineWavelength;
		xVelocity = ((!onLeft) ? (0f - xVelocity) : xVelocity);
		base.transform.localScale = new Vector3(Mathf.Sign(xVelocity), 1f);
		float t = 0f;
		bool ismoving = true;
		while (ismoving)
		{
			t += CupheadTime.FixedDelta;
			Vector3 pos = base.transform.position;
			float yAbsolute = properties.sineAmplitude * Mathf.Sin(properties.sineFreq * t + phase);
			pos.y = root.y + yAbsolute;
			pos.x += xVelocity * CupheadTime.FixedDelta;
			base.transform.position = pos;
			HandleShadow(50f, 0f);
			if ((onLeft && base.transform.position.x > (float)Level.Current.Right + 50f) || (!onLeft && base.transform.position.x < (float)Level.Current.Left - 50f))
			{
				ismoving = false;
			}
			yield return wait;
		}
		Death();
		yield return null;
	}

	private void Death()
	{
		this.Recycle();
	}
}
