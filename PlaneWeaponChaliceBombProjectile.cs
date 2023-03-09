using UnityEngine;

public class PlaneWeaponChaliceBombProjectile : AbstractProjectile
{
	[SerializeField]
	private PlaneWeaponBombExplosion explosion;

	public float explosionSize;

	public float gravity;

	public float damageExplosion;

	public float size;

	private bool isA;

	public Vector2 velocity;

	protected override void Start()
	{
		base.Start();
		base.transform.SetScale(size, size);
		AudioManager.Play("plane_shmup_bomb_fire");
		emitAudioFromObject.Add("plane_shmup_bomb_fire");
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!base.dead)
		{
			velocity.y -= gravity * CupheadTime.FixedDelta;
			base.transform.position += (Vector3)(velocity * CupheadTime.FixedDelta);
		}
	}

	private void DealDamage(GameObject hit)
	{
		damageDealer.DealDamage(hit);
	}

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		DealDamage(hit);
		base.OnCollisionEnemy(hit, phase);
	}

	protected override void OnCollisionOther(GameObject hit, CollisionPhase phase)
	{
		if (hit.tag != "Parry")
		{
			base.OnCollisionOther(hit, phase);
		}
	}

	protected override void Die()
	{
		base.Die();
		GetComponent<SpriteRenderer>().enabled = false;
		AudioManager.Play("plane_shmup_bomb_explosion");
		emitAudioFromObject.Add("plane_shmup_bomb_explosion");
		explosion.Create(base.transform.position, damageExplosion, base.DamageMultiplier, explosionSize);
		explosion.animator.Play((!isA) ? "B" : "A");
	}

	public void SetAnimation(bool isA)
	{
		this.isA = isA;
		base.animator.Play((!isA) ? "B" : "A");
	}
}
