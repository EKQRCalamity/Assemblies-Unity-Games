using System.Collections;
using UnityEngine;

public class AirplaneLevelSecretLeader : LevelProperties.Airplane.Entity
{
	private bool isDead;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	[SerializeField]
	private BasicProjectile rocketBGPrefab;

	[SerializeField]
	private AirplaneLevelRocket rocketPrefab;

	[SerializeField]
	private Effect rocketBGEffect;

	[SerializeField]
	private AirplaneLevel level;

	[SerializeField]
	private AirplaneLevelSecretTerrier[] terriers;

	private PatternString rocketPositionString;

	private PatternString terrierProjectileParryableString;

	private bool attacked;

	private bool moved;

	private bool hiding;

	private bool first = true;

	[SerializeField]
	private int currentHole;

	[SerializeField]
	private BoxCollider2D boxCollider;

	[SerializeField]
	private SpriteRenderer rend;

	[SerializeField]
	private SpriteRenderer backerRend;

	private bool effectSide;

	private void Start()
	{
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		effectSide = Rand.Bool();
	}

	protected override void OnDestroy()
	{
		damageReceiver.OnDamageTaken -= OnDamageTaken;
		base.OnDestroy();
		WORKAROUND_NullifyFields();
	}

	public override void LevelInit(LevelProperties.Airplane properties)
	{
		base.LevelInit(properties);
		rocketPositionString = new PatternString(properties.CurrentState.secretLeader.rocketHomingSpawnLocation);
		terrierProjectileParryableString = new PatternString(properties.CurrentState.secretTerriers.dogBulletParryString);
		StartCoroutine(attack_cr());
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		base.properties.DealDamage(info.damage);
		if (base.properties.CurrentHealth <= 0f && !isDead)
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
	}

	public bool TerrierProjectileParryable()
	{
		return terrierProjectileParryableString.PopLetter() == 'P';
	}

	public void DieMain()
	{
		StopAllCoroutines();
		StartCoroutine(die_main_cr());
	}

	private IEnumerator die_main_cr()
	{
		GetComponent<Collider2D>().enabled = false;
		hiding = true;
		currentHole = 3;
		base.animator.Play("Death");
		isDead = true;
		base.transform.localScale = new Vector3(Mathf.Sign(level.GetHolePosition(currentHole, isLeader: true).x - Camera.main.transform.position.x), 1f);
		base.transform.position = level.GetLeaderDeathPosition(currentHole);
		yield return base.animator.WaitForAnimationToStart(this, "DeathLoop");
		base.animator.Play("Tears", 1);
		AudioManager.Play("sfx_dlc_dogfight_leadervocal_death");
	}

	private void Die()
	{
		isDead = true;
		StopAllCoroutines();
		for (int i = 0; i < terriers.Length; i++)
		{
			terriers[i].Die(i);
		}
		level.leader.animator.Play("Off");
		level.leader.animator.Play("Copter_Death", level.leader.animator.GetLayerIndex("Death"));
		level.leader.animator.Play("Blades", base.animator.GetLayerIndex("DeathBlades"));
		base.animator.Play("DeathLoop");
		base.animator.Play("Tears", 1);
		base.transform.localScale = new Vector3(Mathf.Sign(level.GetHolePosition(currentHole, isLeader: true).x - Camera.main.transform.position.x), 1f);
		base.transform.position = level.GetLeaderDeathPosition(currentHole);
	}

	private void HideAnimationComplete()
	{
		moved = true;
	}

	private void AttackAnimationStart()
	{
		LevelProperties.Airplane.SecretLeader secretLeader = base.properties.CurrentState.secretLeader;
		Vector3 vector = new Vector3((!effectSide) ? 120 : (-120), 120f);
		rocketBGPrefab.Create(Camera.main.transform.position + vector, MathUtils.DirectionToAngle(Vector3.up) + Random.Range(5f, 12f) * (float)(effectSide ? 1 : (-1)), new Vector3(2f, 2f), 600f);
		rocketBGEffect.Create(Camera.main.transform.position + vector);
		effectSide = !effectSide;
	}

	private void AttackAnimationComplete()
	{
		LevelProperties.Airplane.SecretLeader secretLeader = base.properties.CurrentState.secretLeader;
		rocketPrefab.Create(PlayerManager.GetNext(), Camera.main.transform.position + Vector3.up * 800f + rocketPositionString.PopFloat() * Vector3.right, secretLeader.rocketHomingSpeed, secretLeader.rocketHomingRotation, secretLeader.rocketHomingHP, secretLeader.rocketHomingTime);
	}

	private IEnumerator attack_cr()
	{
		level.OccupyHole(currentHole);
		while (true)
		{
			base.transform.localScale = new Vector3(Mathf.Sign(level.GetHolePosition(currentHole, isLeader: true).x - Camera.main.transform.position.x), 1f);
			base.transform.position = level.GetHolePosition(currentHole, isLeader: true);
			rend.sortingOrder = currentHole % 3 + 50;
			backerRend.sortingOrder = currentHole % 3 + 13;
			bool lookingStraight = currentHole == 2 || currentHole == 5;
			base.animator.SetBool("EyesDown", !lookingStraight);
			hiding = false;
			if (!first)
			{
				base.animator.Play("Emerge");
			}
			first = false;
			boxCollider.enabled = true;
			yield return CupheadTime.WaitForSeconds(this, base.properties.CurrentState.secretLeader.leaderPreAttackDelay);
			base.animator.Play("AttackStart");
			yield return base.animator.WaitForAnimationToStart(this, "AttackPreHold");
			yield return CupheadTime.WaitForSeconds(this, base.properties.CurrentState.secretLeader.attackAnticipationHold);
			base.animator.SetTrigger("ContinueAttack");
			yield return base.animator.WaitForAnimationToStart(this, "AttackPostHold");
			yield return CupheadTime.WaitForSeconds(this, base.properties.CurrentState.secretLeader.attackRecoveryHold);
			base.animator.SetTrigger("ContinueAttack");
			yield return base.animator.WaitForAnimationToEnd(this, (!base.animator.GetBool("EyesDown")) ? "AttackEnd" : "AttackEndEyesDown");
			yield return CupheadTime.WaitForSeconds(this, base.properties.CurrentState.secretLeader.leaderPostAttackDelay);
			base.animator.Play((currentHole % 3 != 3) ? "Exit" : "ExitLow");
			while (!moved)
			{
				yield return null;
			}
			boxCollider.enabled = false;
			hiding = true;
			moved = false;
			int previousHole = currentHole;
			for (currentHole = -1; currentHole == -1; currentHole = level.GetNextHole())
			{
			}
			level.LeaveHole(previousHole);
			yield return CupheadTime.WaitForSeconds(this, base.properties.CurrentState.secretLeader.hideTime);
		}
	}

	private void AnimationEvent_SFX_DOGFIGHT_PS_LeaderAttack()
	{
		AudioManager.Play("sfx_dlc_dogfight_ps_leader_batonattack");
		emitAudioFromObject.Add("sfx_dlc_dogfight_ps_leader_batonattack");
		AudioManager.Play("sfx_dlc_dogfight_leadervocal_command");
		emitAudioFromObject.Add("sfx_dlc_dogfight_leadervocal_command");
	}

	private void WORKAROUND_NullifyFields()
	{
		damageDealer = null;
		rocketBGPrefab = null;
		rocketPrefab = null;
		rocketBGEffect = null;
		level = null;
		terriers = null;
		rocketPositionString = null;
		terrierProjectileParryableString = null;
		boxCollider = null;
		rend = null;
		backerRend = null;
	}
}
