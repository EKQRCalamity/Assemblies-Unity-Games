using System;
using System.Collections;
using UnityEngine;

public class DicePalaceEightBallLevelPoolBall : AbstractProjectile
{
	private const float OffsetY = 55f;

	[SerializeField]
	private GameObject shadowPrefab;

	[SerializeField]
	private GameObject dustPrefab;

	[SerializeField]
	private GameObject[] colorVariations;

	private DicePalaceEightBallLevelEightBall parent;

	private float horSpeed;

	private float verSpeed;

	private float gravity;

	private float delay;

	private bool onLeft;

	private Transform shadowInstance;

	private Transform dustInstance;

	public DicePalaceEightBallLevelPoolBall Create(Vector2 pos, float horSpeed, float verSpeed, float gravity, float delay, bool onLeft, DicePalaceEightBallLevelEightBall parent)
	{
		DicePalaceEightBallLevelPoolBall dicePalaceEightBallLevelPoolBall = base.Create() as DicePalaceEightBallLevelPoolBall;
		dicePalaceEightBallLevelPoolBall.transform.position = pos;
		dicePalaceEightBallLevelPoolBall.horSpeed = horSpeed;
		dicePalaceEightBallLevelPoolBall.verSpeed = verSpeed;
		dicePalaceEightBallLevelPoolBall.gravity = gravity;
		dicePalaceEightBallLevelPoolBall.delay = delay;
		dicePalaceEightBallLevelPoolBall.onLeft = onLeft;
		dicePalaceEightBallLevelPoolBall.parent = parent;
		return dicePalaceEightBallLevelPoolBall;
	}

	protected override void Start()
	{
		base.Start();
		shadowInstance = UnityEngine.Object.Instantiate(shadowPrefab).transform;
		shadowInstance.gameObject.SetActive(value: false);
		dustInstance = UnityEngine.Object.Instantiate(dustPrefab).transform;
		shadowInstance.gameObject.SetActive(value: false);
		StartCoroutine(jump_cr());
		StartCoroutine(check_dying_cr());
		DicePalaceEightBallLevelEightBall dicePalaceEightBallLevelEightBall = parent;
		dicePalaceEightBallLevelEightBall.OnEightBallDeath = (Action)Delegate.Combine(dicePalaceEightBallLevelEightBall.OnEightBallDeath, new Action(EightBallDead));
	}

	public void SetVariation(int index)
	{
		for (int i = 0; i < colorVariations.Length; i++)
		{
			colorVariations[i].SetActive(value: false);
		}
		if (index >= 0 && index < colorVariations.Length)
		{
			colorVariations[index].SetActive(value: true);
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	private IEnumerator jump_cr()
	{
		bool jumping = false;
		bool goingUp = false;
		bool upsideDown = false;
		float velocityY2 = verSpeed;
		float velocityX2 = horSpeed;
		float ground = (float)Level.Current.Ground + 55f;
		dustInstance.gameObject.SetActive(value: false);
		while (base.transform.position.y > ground)
		{
			velocityY2 -= gravity / 2f * (float)CupheadTime.Delta;
			base.transform.AddPosition(0f, velocityY2 * (float)CupheadTime.Delta);
			yield return null;
		}
		Vector3 p = base.transform.position;
		p.y = ground;
		base.transform.position = p;
		dustInstance.position = base.transform.position;
		dustInstance.gameObject.SetActive(value: true);
		base.animator.SetTrigger("Smash");
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, delay);
			jumping = true;
			goingUp = true;
			velocityY2 = verSpeed;
			velocityX2 = ((!onLeft) ? (0f - horSpeed) : horSpeed);
			base.animator.SetTrigger("Jump");
			shadowInstance.gameObject.SetActive(value: false);
			if (upsideDown)
			{
				yield return base.animator.WaitForAnimationToEnd(this, "UpsideDownJump", waitForEndOfFrame: true);
			}
			else
			{
				yield return base.animator.WaitForAnimationToEnd(this, "Jump", waitForEndOfFrame: true);
			}
			shadowInstance.gameObject.SetActive(value: true);
			dustInstance.gameObject.SetActive(value: false);
			while (jumping)
			{
				shadowInstance.position = new Vector3(base.transform.position.x, ground, 0f);
				velocityY2 -= gravity * (float)CupheadTime.Delta;
				base.transform.AddPosition(velocityX2 * (float)CupheadTime.Delta, velocityY2 * (float)CupheadTime.Delta);
				if (velocityY2 < 0f && goingUp)
				{
					base.animator.SetTrigger("Turn");
					goingUp = false;
					if (upsideDown)
					{
						yield return base.animator.WaitForAnimationToEnd(this, "RightSideUpSmash_start", waitForEndOfFrame: true);
					}
					else
					{
						yield return base.animator.WaitForAnimationToEnd(this, "JumpTurn", waitForEndOfFrame: true);
					}
				}
				if (velocityY2 < 0f && jumping && base.transform.position.y <= ground)
				{
					base.animator.SetTrigger("Smash");
					jumping = false;
					upsideDown = !upsideDown;
					Vector3 position = base.transform.position;
					position.y = ground;
					base.transform.position = position;
					dustInstance.position = base.transform.position;
					dustInstance.gameObject.SetActive(value: true);
				}
				yield return null;
			}
		}
	}

	private IEnumerator check_dying_cr()
	{
		while (true)
		{
			if (onLeft)
			{
				if (base.transform.position.x > 840f)
				{
					break;
				}
			}
			else if (base.transform.position.x < -840f)
			{
				break;
			}
			yield return null;
		}
		Die();
		yield return null;
	}

	private void EightBallDead()
	{
		StopAllCoroutines();
		StartCoroutine(eight_ball_death_cr());
	}

	private IEnumerator eight_ball_death_cr()
	{
		float speed = 2500f;
		float angle = UnityEngine.Random.Range(0, 360);
		Vector3 dir = MathUtils.AngleToDirection(angle);
		GetComponent<Collider2D>().enabled = false;
		while (true)
		{
			base.transform.position += dir * speed * CupheadTime.FixedDelta;
			yield return null;
		}
	}

	protected override void OnDestroy()
	{
		if (shadowInstance != null)
		{
			UnityEngine.Object.Destroy(shadowInstance.gameObject);
		}
		if (dustInstance != null)
		{
			UnityEngine.Object.Destroy(dustInstance.gameObject);
		}
		base.OnDestroy();
		shadowPrefab = null;
		dustPrefab = null;
	}

	protected override void Die()
	{
		base.Die();
		DicePalaceEightBallLevelEightBall dicePalaceEightBallLevelEightBall = parent;
		dicePalaceEightBallLevelEightBall.OnEightBallDeath = (Action)Delegate.Remove(dicePalaceEightBallLevelEightBall.OnEightBallDeath, new Action(EightBallDead));
	}
}
