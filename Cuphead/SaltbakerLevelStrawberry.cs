using UnityEngine;

public class SaltbakerLevelStrawberry : SaltbakerLevelPhaseOneProjectile
{
	private const float OFFSET = 40f;

	[SerializeField]
	private Animator anim;

	[SerializeField]
	private Collider2D coll;

	protected override Vector3 Direction => -base.transform.up;

	public BasicProjectile Create(Vector2 position, float rotation, float speed, int anim)
	{
		SaltbakerLevelStrawberry saltbakerLevelStrawberry = (SaltbakerLevelStrawberry)base.Create(position, rotation, speed);
		saltbakerLevelStrawberry.anim.Play(anim.ToString());
		return saltbakerLevelStrawberry;
	}

	protected override void Move()
	{
		if (coll.enabled)
		{
			base.Move();
			if (base.transform.position.y - 40f < (float)Level.Current.Ground)
			{
				shadow.enabled = false;
				Die();
			}
			else
			{
				HandleShadow(40f, 0f);
			}
		}
	}

	protected override void Die()
	{
		coll.enabled = false;
		createSparks = false;
		base.transform.eulerAngles = Vector3.zero;
		base.animator.SetTrigger("OnDeath");
	}

	private void OnDeathAnimationEnd()
	{
		Object.Destroy(base.gameObject);
	}
}
