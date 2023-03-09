using System.Collections;
using UnityEngine;

public class SnowCultLevelSnowman : AbstractCollidableObject
{
	private LevelProperties.SnowCult.Snowman properties;

	private Collider2D coll;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	private bool goingRight;

	private bool melted;

	private float Health;

	private float yPos;

	public void Init(Vector3 pos, LevelProperties.SnowCult.Snowman properties, bool goingRight)
	{
		base.transform.position = pos;
		yPos = base.transform.position.y;
		this.properties = properties;
		this.goingRight = goingRight;
		if (goingRight)
		{
			base.transform.SetScale(0f - base.transform.localScale.x);
		}
		else
		{
			base.transform.SetScale(base.transform.localScale.x);
		}
	}

	private void Start()
	{
		coll = GetComponent<Collider2D>();
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		Health = properties.health;
		StartCoroutine(move_cr());
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
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

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		Health -= info.damage;
		if (Health < 0f && !melted)
		{
			melted = true;
			Melt();
		}
	}

	private void Melt()
	{
		StartCoroutine(melt_cr());
	}

	private IEnumerator melt_cr()
	{
		GetComponent<Collider2D>().enabled = false;
		base.animator.Play("Melt");
		yield return base.animator.WaitForAnimationToEnd(this, "Melt");
		yield return CupheadTime.WaitForSeconds(this, properties.timeUntilUnmelt);
		base.animator.SetTrigger("Continue");
		yield return CupheadTime.WaitForSeconds(this, properties.unmeltLoopTime);
		base.animator.SetTrigger("Continue");
		yield return base.animator.WaitForAnimationToEnd(this, "Unmelt");
		melted = false;
		Health = properties.health;
		GetComponent<Collider2D>().enabled = true;
		yield return null;
	}

	private void Turn()
	{
		base.transform.SetScale(0f - base.transform.localScale.x);
	}

	private IEnumerator move_cr()
	{
		float sizeX = coll.bounds.size.x;
		float left = -640f + sizeX / 2f;
		float right = 640f - sizeX / 2f;
		float t = 0f;
		float time = properties.runTime;
		EaseUtils.EaseType ease = EaseUtils.EaseType.linear;
		Vector3 endPos = Vector3.zero;
		endPos = ((!goingRight) ? new Vector3(left, yPos) : new Vector3(right, yPos));
		float speed = Vector3.Distance(new Vector3(right, yPos), new Vector3(left, yPos)) / time;
		while (base.transform.position != endPos)
		{
			while (melted)
			{
				yield return null;
			}
			base.transform.position = Vector3.MoveTowards(base.transform.position, endPos, speed * (float)CupheadTime.Delta);
			yield return null;
		}
		float start = 0f;
		float end = 0f;
		base.animator.Play("Turn");
		yield return base.animator.WaitForAnimationToEnd(this, "Turn");
		base.transform.SetScale(0f - base.transform.localScale.x);
		if (goingRight)
		{
			start = base.transform.position.x;
			end = left;
		}
		else
		{
			start = base.transform.position.x;
			end = right;
		}
		while (true)
		{
			t = 0f;
			while (t < time)
			{
				if (!melted)
				{
					float value = t / time;
					base.transform.SetPosition(EaseUtils.Ease(ease, start, end, value));
					t += (float)CupheadTime.Delta;
				}
				yield return null;
			}
			base.animator.Play("Turn");
			yield return base.animator.WaitForAnimationToEnd(this, "Turn");
			base.transform.SetScale(0f - base.transform.localScale.x);
			goingRight = !goingRight;
			if (!goingRight)
			{
				start = left;
				end = right;
			}
			else
			{
				start = right;
				end = left;
			}
			yield return null;
		}
	}

	public void Die()
	{
		StopAllCoroutines();
		Object.Destroy(base.gameObject);
	}
}
