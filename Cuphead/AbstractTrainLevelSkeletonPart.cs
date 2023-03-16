using UnityEngine;

public class AbstractTrainLevelSkeletonPart : AbstractCollidableObject
{
	public const float X = 470f;

	private LevelBossDeathExploder exploder;

	private DamageDealer damageDealer;

	protected override void Awake()
	{
		base.Awake();
		exploder = GetComponent<LevelBossDeathExploder>();
		damageDealer = DamageDealer.NewEnemy();
	}

	public void SetPosition(TrainLevelSkeleton.Position position)
	{
		switch (position)
		{
		default:
			base.transform.SetPosition(0f);
			break;
		case TrainLevelSkeleton.Position.Left:
			base.transform.SetPosition(-470f);
			break;
		case TrainLevelSkeleton.Position.Right:
			base.transform.SetPosition(470f);
			break;
		}
	}

	public void In()
	{
		base.animator.Play("In");
	}

	public void Out()
	{
		base.animator.SetTrigger("Out");
	}

	public void Die()
	{
		exploder.StartExplosion();
		base.animator.Play("Death");
	}

	public void EndDeath()
	{
		exploder.StopExplosions();
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
		if (damageDealer != null && phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}
}
