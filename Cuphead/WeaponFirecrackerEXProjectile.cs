using System.Collections;
using UnityEngine;

public class WeaponFirecrackerEXProjectile : BasicProjectile
{
	public float bulletLife;

	public float explosionSize;

	public float explosionDuration;

	protected override void Start()
	{
		base.Start();
	}

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionEnemy(hit, phase);
		if (phase == CollisionPhase.Enter)
		{
			StartCoroutine(explosion_cr());
		}
	}

	private IEnumerator explosion_cr()
	{
		move = false;
		base.transform.SetScale(explosionSize, explosionSize);
		yield return CupheadTime.WaitForSeconds(this, explosionDuration);
		Object.Destroy(base.gameObject);
	}
}
