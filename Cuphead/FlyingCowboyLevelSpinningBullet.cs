using System.Collections;
using UnityEngine;

public class FlyingCowboyLevelSpinningBullet : AbstractProjectile
{
	[SerializeField]
	private Transform child;

	public FlyingCowboyLevelSpinningBullet Create(Vector2 pos, float speed, float rotationSpeed, float rotationRadius, Vector3 direction, bool clockwise, bool parryable)
	{
		FlyingCowboyLevelSpinningBullet flyingCowboyLevelSpinningBullet = Create() as FlyingCowboyLevelSpinningBullet;
		flyingCowboyLevelSpinningBullet.child.localPosition = new Vector3(rotationRadius, 0f);
		flyingCowboyLevelSpinningBullet.StartCoroutine(flyingCowboyLevelSpinningBullet.bullet_cr(pos, speed, rotationSpeed, direction, clockwise));
		flyingCowboyLevelSpinningBullet.StartCoroutine(flyingCowboyLevelSpinningBullet.scale_cr());
		flyingCowboyLevelSpinningBullet.SetParryable(parryable);
		return flyingCowboyLevelSpinningBullet;
	}

	protected override void Update()
	{
		base.Update();
		child.SetEulerAngles(0f, 0f, 0f);
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	protected override void Die()
	{
		child.SetLocalEulerAngles(0f, 0f, Random.Range(0, 360));
		base.Die();
	}

	private IEnumerator scale_cr()
	{
		Vector3 initialScale = base.transform.localScale;
		base.transform.localScale = initialScale * 0.75f;
		float elapsedTime = 0f;
		while (elapsedTime < 0.3f)
		{
			yield return null;
			elapsedTime += (float)CupheadTime.Delta;
			Vector3 scale = Vector3.Lerp(initialScale * 0.75f, initialScale, elapsedTime / 0.3f);
			base.transform.localScale = scale;
		}
	}

	private IEnumerator bullet_cr(Vector2 pos, float speed, float rotationSpeed, Vector3 direction, bool clockwise)
	{
		if (!clockwise)
		{
			base.animator.SetFloat("Speed", -1f);
		}
		base.transform.position = pos - (Vector2)child.localPosition;
		while (true)
		{
			base.transform.position += direction * speed * CupheadTime.Delta;
			base.transform.AddEulerAngles(0f, 0f, (float)((!clockwise) ? 1 : (-1)) * rotationSpeed * (float)CupheadTime.Delta);
			yield return null;
		}
	}
}
