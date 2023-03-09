using System.Collections;
using UnityEngine;

public class SallyStagePlayLevelWindowProjectile : AbstractProjectile
{
	[SerializeField]
	private bool isBaby;

	[SerializeField]
	private CollisionChild child;

	private float speed;

	private float rotation;

	private bool move;

	public SallyStagePlayLevelWindowProjectile Create(Vector2 pos, float rotation, float speed, SallyStagePlayLevel parent)
	{
		SallyStagePlayLevelWindowProjectile sallyStagePlayLevelWindowProjectile = base.Create() as SallyStagePlayLevelWindowProjectile;
		sallyStagePlayLevelWindowProjectile.transform.position = pos;
		sallyStagePlayLevelWindowProjectile.rotation = rotation;
		sallyStagePlayLevelWindowProjectile.speed = speed;
		return sallyStagePlayLevelWindowProjectile;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	private IEnumerator move_cr()
	{
		move = true;
		Vector3 dir = MathUtils.AngleToDirection(rotation);
		while (move)
		{
			base.transform.position += dir * speed * CupheadTime.FixedDelta;
			yield return new WaitForFixedUpdate();
		}
	}

	protected override void Start()
	{
		base.Start();
		if (child != null)
		{
			child.transform.SetEulerAngles(null, null, 0f);
			child.OnPlayerCollision += OnCollisionPlayer;
		}
		StartCoroutine(move_cr());
		StartCoroutine(on_ground_hit_cr());
	}

	private void OnPhase3()
	{
		Object.Destroy(base.gameObject);
	}

	protected override void Die()
	{
		base.Die();
	}

	private IEnumerator on_ground_hit_cr()
	{
		while (base.transform.position.y > (float)Level.Current.Ground)
		{
			yield return null;
		}
		move = false;
		if (isBaby)
		{
			base.animator.SetTrigger("OnSmash");
			AudioManager.Play("sally_bottle_smash");
			emitAudioFromObject.Add("sally_bottle_smash");
		}
		else
		{
			base.animator.Play("Death");
		}
		yield return null;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
