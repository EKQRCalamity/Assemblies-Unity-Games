using System.Collections;
using UnityEngine;

public class DicePalaceDominoLevelBouncyBall : AbstractProjectile
{
	public enum Colour
	{
		blue,
		green,
		red,
		yellow,
		none
	}

	private const float RotationFactor = 180f;

	[SerializeField]
	private Effect hitEffectPrefab;

	[SerializeField]
	private Effect explosion;

	[SerializeField]
	private Transform[] toRotate;

	private Vector3 deltaPosition;

	public void InitBouncyBall(float speed, Vector3 direction)
	{
		deltaPosition = direction * speed;
		StartCoroutine(move_cr());
		StartCoroutine(checkCollisions_cr());
	}

	public override void SetParryable(bool parryable)
	{
		base.SetParryable(parryable);
		if (parryable)
		{
			base.animator.SetInteger("Variation", 3);
		}
		else
		{
			base.animator.SetInteger("Variation", Random.Range(1, 3));
		}
	}

	private IEnumerator move_cr()
	{
		while (true)
		{
			base.transform.position += deltaPosition * CupheadTime.Delta;
			for (int i = 0; i < toRotate.Length; i++)
			{
				toRotate[i].Rotate(Vector3.forward, 180f * (float)CupheadTime.Delta);
			}
			yield return null;
		}
	}

	private IEnumerator checkCollisions_cr()
	{
		while (true)
		{
			if (base.transform.position.y > (float)Level.Current.Ceiling)
			{
				deltaPosition.y *= -1f;
				BounceSFX();
				base.animator.SetTrigger("Bounce");
				hitEffectPrefab.Create(base.transform.position, new Vector3(1f, -1f, 1f));
				yield return CupheadTime.WaitForSeconds(this, 1f);
			}
			if (base.transform.position.y < (float)Level.Current.Ground)
			{
				deltaPosition.y *= -1f;
				BounceSFX();
				base.animator.SetTrigger("Bounce");
				hitEffectPrefab.Create(base.transform.position);
				yield return CupheadTime.WaitForSeconds(this, 1f);
			}
			if (base.transform.position.x > (float)Level.Current.Right)
			{
				deltaPosition.x *= -1f;
				yield return CupheadTime.WaitForSeconds(this, 1f);
			}
			if (base.transform.position.x < (float)Level.Current.Left)
			{
				deltaPosition.x *= -1f;
				yield return CupheadTime.WaitForSeconds(this, 1f);
			}
			yield return null;
		}
	}

	protected override void OnCollisionWalls(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionWalls(hit, phase);
		if ((bool)hit.GetComponent<BasicDamageDealingObject>())
		{
			Die();
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	protected override void OnDestroy()
	{
		StopAllCoroutines();
		base.OnDestroy();
		hitEffectPrefab = null;
		explosion = null;
	}

	protected override void Die()
	{
		BounceSFX();
		explosion.Create(base.transform.position);
		Object.Destroy(base.gameObject);
	}

	private void BounceSFX()
	{
		AudioManager.Play("dice_projectile_bounce");
		emitAudioFromObject.Add("dice_projectile_bounce");
	}
}
