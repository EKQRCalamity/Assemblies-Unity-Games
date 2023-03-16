using System.Collections;
using UnityEngine;

public class FlyingGenieLevelHelixProjectile : AbstractProjectile
{
	private LevelProperties.FlyingGenie.Coffin properties;

	private bool topOne;

	public FlyingGenieLevelHelixProjectile Create(Vector3 pos, LevelProperties.FlyingGenie.Coffin properties, bool topOne)
	{
		FlyingGenieLevelHelixProjectile flyingGenieLevelHelixProjectile = base.Create() as FlyingGenieLevelHelixProjectile;
		flyingGenieLevelHelixProjectile.properties = properties;
		flyingGenieLevelHelixProjectile.transform.position = pos;
		flyingGenieLevelHelixProjectile.topOne = topOne;
		return flyingGenieLevelHelixProjectile;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (damageDealer != null && phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	protected override void Update()
	{
		base.Update();
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	protected override void Start()
	{
		base.Start();
		base.animator.SetBool("OnTop", topOne);
		StartCoroutine(moveY_cr());
	}

	private IEnumerator moveY_cr()
	{
		float angle = 0f;
		float xSpeed = properties.heartShotXSpeed;
		float ySpeed2 = properties.heartShotYSpeed;
		Vector3 moveX2 = base.transform.position;
		while (base.transform.position.x != -640f)
		{
			float loopSize;
			if (topOne)
			{
				loopSize = properties.heartLoopYSize;
				ySpeed2 = properties.heartShotYSpeed;
			}
			else
			{
				loopSize = 0f - properties.heartLoopYSize;
				ySpeed2 = 0f - properties.heartShotYSpeed;
			}
			angle += ySpeed2 * (float)CupheadTime.Delta;
			Vector3 moveY = new Vector3(0f, Mathf.Sin(angle + properties.heartLoopYSize) * (float)CupheadTime.Delta * 60f * loopSize / 2f);
			moveX2 = -base.transform.right * xSpeed * CupheadTime.Delta;
			base.transform.position += moveX2 + moveY;
			yield return null;
		}
		Die();
		yield return null;
	}
}
