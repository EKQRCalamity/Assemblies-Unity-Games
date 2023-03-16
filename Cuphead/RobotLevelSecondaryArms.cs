using UnityEngine;

public class RobotLevelSecondaryArms : AbstractCollidableObject
{
	private bool ShootTwicePerCycle;

	private float bulletSpeed;

	private Transform spawnPoint;

	private DamageDealer damageDealer;

	[SerializeField]
	private BasicProjectile twistyArmsProjectile;

	public bool BossAlive { private get; set; }

	protected override void Awake()
	{
		damageDealer = DamageDealer.NewEnemy();
		BossAlive = true;
		base.Awake();
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
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	public void InitHelper(LevelProperties.Robot properties)
	{
		spawnPoint = base.transform.GetChild(2).transform;
		ShootTwicePerCycle = properties.CurrentState.twistyArms.shootTwicePerCycle;
		bulletSpeed = properties.CurrentState.twistyArms.bulletSpeed;
	}

	private void OnTwistyArmsShoot()
	{
		if (twistyArmsProjectile != null)
		{
			AudioManager.Play("robot_arms_hand_shoot");
			emitAudioFromObject.Add("robot_arms_hand_shoot");
			twistyArmsProjectile.Create(spawnPoint.position + Vector3.up * 100f, 90f, bulletSpeed);
			twistyArmsProjectile.Create(spawnPoint.position + Vector3.down * 100f, -90f, bulletSpeed);
		}
	}

	private void SwapAnimations()
	{
		if (BossAlive)
		{
			float num = base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;
			if (ShootTwicePerCycle && num < 0.5f)
			{
				base.animator.Play("Shoot B");
			}
			else if (num > 0.5f)
			{
				base.animator.Play("Shoot A");
			}
		}
	}
}
