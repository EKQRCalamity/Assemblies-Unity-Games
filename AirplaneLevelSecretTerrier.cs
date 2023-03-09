using System.Collections;
using UnityEngine;

public class AirplaneLevelSecretTerrier : LevelProperties.Airplane.Entity
{
	private bool isDead;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	[SerializeField]
	private Transform bulletRoot;

	[SerializeField]
	private AirplaneLevelSecretTerrierBullet bulletPrefab;

	[SerializeField]
	private AirplaneLevelSecretTerrierBullet bulletPrefabPink;

	[SerializeField]
	private AirplaneLevel level;

	private bool attacked;

	private bool moved;

	private bool canAttack;

	private float hp;

	[SerializeField]
	private int currentHole;

	private int[] introNum = new int[5] { 1, 3, 2, 0, 4 };

	[SerializeField]
	private Collider2D coll;

	[SerializeField]
	private AirplaneLevelSecretLeader leader;

	private bool firstAttack;

	private bool nextAttackPink;

	[SerializeField]
	private SpriteRenderer rend;

	[SerializeField]
	private SpriteRenderer backerRend;

	private void Start()
	{
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		MoveToHolePosition();
		base.animator.Play("Intro_" + introNum[currentHole]);
		base.animator.Update(0f);
		firstAttack = true;
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
		hp = properties.CurrentState.secretTerriers.dogRetreatDamage;
		level.OccupyHole(currentHole);
		base.transform.localScale = new Vector3(0f - Mathf.Sign(level.GetHolePosition(currentHole, isLeader: false).x - Camera.main.transform.position.x), 1f);
		base.transform.position = level.GetHolePosition(currentHole, isLeader: false);
	}

	private void AniEvent_StartTerriers()
	{
		StartCoroutine(attack_cr());
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (hp > 0f)
		{
			hp -= info.damage;
			if (hp <= 0f)
			{
				Level.Current.RegisterMinionKilled();
				StopAllCoroutines();
				coll.enabled = false;
				StartCoroutine(timeout_cr());
			}
		}
	}

	public int CurrentHole()
	{
		return currentHole;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	protected void MoveToHolePosition()
	{
		rend.sortingOrder = currentHole % 3 + 50;
		backerRend.sortingOrder = currentHole % 3 + 13;
		base.transform.localScale = new Vector3(0f - Mathf.Sign(level.GetHolePosition(currentHole, isLeader: false).x - Camera.main.transform.position.x), 1f);
		base.transform.position = level.GetHolePosition(currentHole, isLeader: false);
	}

	public void Die(int index)
	{
		StopAllCoroutines();
		while (currentHole == -1)
		{
			currentHole = level.GetNextHole();
			MoveToHolePosition();
		}
		string stateName = "Death_" + (index + 1);
		base.animator.Play(stateName);
		if (index + 1 == 1 || index + 1 == 4)
		{
			base.animator.Play("Stars", 1);
		}
	}

	private void HideAnimationComplete()
	{
		moved = true;
		coll.enabled = false;
	}

	private void AttackAnimationComplete()
	{
		LevelProperties.Airplane.SecretTerriers secretTerriers = base.properties.CurrentState.secretTerriers;
		if (nextAttackPink)
		{
			bulletPrefabPink.Create(bulletRoot.position, PlayerManager.GetNext().transform.position, secretTerriers, base.transform.localScale);
		}
		else
		{
			bulletPrefab.Create(bulletRoot.position, PlayerManager.GetNext().transform.position, secretTerriers, base.transform.localScale);
		}
		attacked = true;
		AudioManager.Play("sfx_dlc_dogfight_ps_terrier_pineapplethrow");
	}

	public void TryStartAttack()
	{
		nextAttackPink = leader.TerrierProjectileParryable();
		if (canAttack)
		{
			base.animator.SetTrigger((!nextAttackPink) ? "Attack" : "AttackPink");
		}
	}

	private IEnumerator attack_cr()
	{
		level.OccupyHole(currentHole);
		while (true)
		{
			MoveToHolePosition();
			if (!firstAttack)
			{
				base.animator.Play("Emerge");
			}
			firstAttack = false;
			canAttack = true;
			coll.enabled = true;
			attacked = false;
			moved = false;
			while (!attacked)
			{
				yield return null;
			}
			canAttack = false;
			attacked = false;
			yield return CupheadTime.WaitForSeconds(this, base.properties.CurrentState.secretTerriers.dogPostAttackDelay);
			base.animator.SetTrigger("OnMove");
			while (!moved)
			{
				yield return null;
			}
			moved = false;
			int previousHole = currentHole;
			for (currentHole = -1; currentHole == -1; currentHole = level.GetNextHole())
			{
			}
			level.LeaveHole(previousHole);
		}
	}

	private IEnumerator timeout_cr()
	{
		base.animator.ResetTrigger("Attack");
		base.animator.ResetTrigger("OnMove");
		base.animator.Play("Move");
		canAttack = false;
		level.LeaveHole(currentHole);
		currentHole = -1;
		yield return CupheadTime.WaitForSeconds(this, base.properties.CurrentState.secretTerriers.dogTimeOut);
		hp = base.properties.CurrentState.secretTerriers.dogRetreatDamage;
		while (currentHole == -1)
		{
			currentHole = level.GetNextHole();
			yield return null;
		}
		StartCoroutine(attack_cr());
	}

	private void AniEvent_PullGrenadePin()
	{
		AudioManager.Play("sfx_dlc_dogfight_ps_terrier_pineapplepinclink");
	}

	private void WORKAROUND_NullifyFields()
	{
		damageDealer = null;
		bulletRoot = null;
		bulletPrefab = null;
		bulletPrefabPink = null;
		level = null;
		introNum = null;
		coll = null;
		leader = null;
		rend = null;
		backerRend = null;
	}
}
