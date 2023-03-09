using UnityEngine;

public class RobotLevelHatchBombBot : HomingProjectile
{
	[SerializeField]
	private Sprite explosion;

	private bool isDead;

	private float health;

	private DamageReceiver damageReceiver;

	private LevelProperties.Robot.BombBot properties;

	public void InitBombBot(LevelProperties.Robot.BombBot properties)
	{
		this.properties = properties;
		health = properties.bombHP;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		Die();
	}

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		if (hit.GetComponent<RobotLevelRobotBodyPart>() != null)
		{
			Die();
		}
		else if (hit.GetComponent<RobotLevelHatchBombBot>() != null)
		{
			Die();
		}
		base.OnCollisionEnemy(hit, phase);
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		health -= info.damage;
		if (health <= 0f && !isDead)
		{
			Die();
		}
	}

	protected override void Awake()
	{
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		damageDealer.SetDamage(properties.bombBossDamage);
		damageDealer.SetRate(0f);
	}

	protected override void Update()
	{
		damageDealer.Update();
		base.Update();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void Die()
	{
		base.Die();
		isDead = true;
		StopAllCoroutines();
		base.transform.SetEulerAngles(0f, 0f, 0f);
		base.animator.Play("Explode");
		AudioManager.Play("robot_bombbot_death");
		emitAudioFromObject.Add("robot_bombbot_death");
	}
}
