using System.Collections;
using UnityEngine;

public class VeggiesLevelOnionTearProjectile : AbstractProjectile
{
	private const float startY = 420f;

	private float time;

	private int direction;

	public AbstractProjectile Create(float time, float x)
	{
		VeggiesLevelOnionTearProjectile veggiesLevelOnionTearProjectile = base.Create(new Vector2(x, 420f)) as VeggiesLevelOnionTearProjectile;
		veggiesLevelOnionTearProjectile.direction = direction;
		veggiesLevelOnionTearProjectile.time = time;
		veggiesLevelOnionTearProjectile.Init();
		return veggiesLevelOnionTearProjectile;
	}

	protected void Init()
	{
		StartCoroutine(projectile_cr());
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (damageDealer != null && phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	protected override void Die()
	{
		base.Die();
		base.transform.SetScale(MathUtils.RandomBool() ? 1 : (-1));
	}

	private IEnumerator projectile_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		float startY = base.transform.position.y;
		float endY = Level.Current.Ground;
		float calculatedTime = time;
		float t = 0f;
		while (t < calculatedTime)
		{
			float val = t / calculatedTime;
			TransformExtensions.SetPosition(y: EaseUtils.Ease(EaseUtils.EaseType.easeInQuad, startY, endY, val), transform: base.transform);
			t += CupheadTime.FixedDelta;
			yield return wait;
		}
		base.transform.SetPosition(null, endY);
		AudioManager.Play("level_veggies_onion_teardrop");
		Die();
	}
}
