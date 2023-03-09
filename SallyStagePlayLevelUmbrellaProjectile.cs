using System.Collections;
using UnityEngine;

public class SallyStagePlayLevelUmbrellaProjectile : AbstractProjectile
{
	private bool active;

	private bool dropped;

	private int direction;

	private Vector3 currentVelocity;

	private LevelProperties.SallyStagePlay properties;

	[SerializeField]
	private Material change;

	[SerializeField]
	private SpriteDeathParts[] sprites;

	protected override void Update()
	{
		damageDealer.Update();
		base.Update();
	}

	public void InitProjectile(LevelProperties.SallyStagePlay properties, int direction)
	{
		this.properties = properties;
		active = false;
		this.direction = direction;
		base.transform.SetScale(-direction);
		currentVelocity = Vector3.down * properties.CurrentState.umbrella.objectSpeed;
		StartCoroutine(move_cr());
		StartCoroutine(check_bounds_cr());
	}

	private IEnumerator move_cr()
	{
		bool isFalling = false;
		while (true)
		{
			if (active)
			{
				for (int i = 0; i < 2; i++)
				{
					AbstractPlayerController next = PlayerManager.GetNext();
					if (base.transform.position.x >= next.center.x - 10f && base.transform.position.x <= next.center.x + 10f)
					{
						if (!isFalling)
						{
							base.animator.SetTrigger("OnFall");
							isFalling = true;
						}
						currentVelocity = Vector3.down * properties.CurrentState.umbrella.objectDropSpeed;
					}
				}
			}
			base.transform.position += currentVelocity * CupheadTime.Delta;
			yield return null;
		}
	}

	private IEnumerator check_bounds_cr()
	{
		float offset = 50f;
		bool goingVertically = false;
		bool goingUp = false;
		while (true)
		{
			if (base.transform.position.y >= 360f - offset && goingVertically)
			{
				base.transform.position = new Vector3(base.transform.position.x, 360f - offset, 0f);
				currentVelocity = Vector3.left * properties.CurrentState.umbrella.objectSpeed * direction;
				active = true;
				goingVertically = false;
			}
			else if (base.transform.position.y <= -360f + offset && goingVertically)
			{
				currentVelocity = Vector3.right * properties.CurrentState.umbrella.objectSpeed * direction;
				goingVertically = false;
			}
			else if ((base.transform.position.x <= -630f && !goingVertically) || (base.transform.position.x >= 630f && !goingVertically))
			{
				if (!goingUp)
				{
					base.animator.SetTrigger("OnClimb");
					goingUp = true;
				}
				if (!dropped)
				{
					GetComponent<SpriteRenderer>().material = change;
					dropped = true;
				}
				currentVelocity = Vector3.up * properties.CurrentState.umbrella.objectSpeed;
				goingVertically = true;
			}
			yield return null;
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
			Die();
		}
	}

	protected override void OnCollisionGround(GameObject hit, CollisionPhase phase)
	{
		if (active)
		{
			Die();
		}
		else if (!dropped)
		{
			GetComponent<SpriteRenderer>().material = change;
			base.animator.SetTrigger("OnDrop");
			dropped = true;
		}
		if (phase == CollisionPhase.Enter)
		{
			currentVelocity = Vector3.right * properties.CurrentState.umbrella.objectSpeed * direction;
		}
		base.OnCollisionGround(hit, phase);
	}

	protected override void Die()
	{
		StopAllCoroutines();
		base.animator.SetTrigger("OnDeath");
		SpriteDeathParts[] array = sprites;
		foreach (SpriteDeathParts spriteDeathParts in array)
		{
			spriteDeathParts.CreatePart(base.transform.position);
		}
		base.Die();
	}

	private void Kill()
	{
		Object.Destroy(base.gameObject);
	}

	protected override void OnDestroy()
	{
		StopAllCoroutines();
		base.OnDestroy();
		sprites = null;
	}
}
