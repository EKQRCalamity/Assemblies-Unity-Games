using UnityEngine;

public class WeaponAccuracyProjectile : BasicProjectile
{
	public delegate void OnEnemyDeath(bool hitEnemy);

	public OnEnemyDeath EnemyDeath;

	private bool hitEnemy;

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionEnemy(hit, phase);
		hitEnemy = true;
	}

	protected override void OnDestroy()
	{
		if (EnemyDeath != null)
		{
			EnemyDeath(hitEnemy);
		}
		base.OnDestroy();
	}

	protected override void Die()
	{
		base.Die();
		Object.Destroy(base.gameObject);
	}

	public void SetSize(float size)
	{
		base.transform.SetScale(size, size);
	}
}
