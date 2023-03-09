using System.Collections;
using UnityEngine;

public class RumRunnersLevelDiamond : AbstractCollidableObject
{
	[SerializeField]
	private SpriteRenderer horn;

	[SerializeField]
	private SpriteRenderer hornAttack;

	[SerializeField]
	private SpriteRenderer sparkle;

	private DamageDealer damageDealer;

	protected override void Awake()
	{
		base.Awake();
		damageDealer = DamageDealer.NewEnemy();
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
		damageDealer.DealDamage(hit);
	}

	public void Die()
	{
		StopAllCoroutines();
		Object.Destroy(base.gameObject);
	}

	public void SetAttack(bool attack)
	{
		if (attack)
		{
			horn.enabled = false;
			hornAttack.enabled = true;
		}
		else
		{
			horn.enabled = true;
			hornAttack.enabled = false;
		}
	}

	public void StartSparkle()
	{
		StartCoroutine(startSparkle_cr());
	}

	private IEnumerator startSparkle_cr()
	{
		Color color = sparkle.color;
		color.a = 0f;
		sparkle.color = color;
		base.animator.Play("On", 1);
		float elapsedTime = 0f;
		while (elapsedTime < 0.2f)
		{
			yield return null;
			elapsedTime += (float)CupheadTime.Delta;
			color.a = Mathf.Lerp(0f, 1f, elapsedTime / 0.2f);
			sparkle.color = color;
		}
	}

	public void EndSparkle()
	{
		StartCoroutine(endSparkle_cr());
	}

	private IEnumerator endSparkle_cr()
	{
		Color color = sparkle.color;
		sparkle.color = color;
		float elapsedTime = 0f;
		while (elapsedTime < 0.45f)
		{
			yield return null;
			elapsedTime += (float)CupheadTime.Delta;
			color.a = Mathf.Lerp(1f, 0f, elapsedTime / 0.45f);
			sparkle.color = color;
		}
		base.animator.Play("Off", 1);
	}
}
