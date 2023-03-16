using System.Collections;
using UnityEngine;

public class RumRunnersLevelPoliceman : AbstractCollidableObject
{
	public enum Direction
	{
		Straight,
		Up,
		Down
	}

	private static readonly float SpiderYDistanceThreshold = 100f;

	[SerializeField]
	private RumRunnersLevelPoliceBullet regularBullet;

	[SerializeField]
	private Transform bulletOriginStraight;

	[SerializeField]
	private Transform bulletOriginUp;

	[SerializeField]
	private Transform bulletOriginDown;

	[SerializeField]
	private Vector2 spawnPositionOffset;

	[SerializeField]
	private SpriteRenderer gunSmokeRenderer;

	[SerializeField]
	private SpriteRenderer gunSmokeParryRenderer;

	private LevelProperties.RumRunners.Spider properties;

	private RumRunnersLevelSpider spider;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	private bool isPink;

	private float hp;

	private float scaleX;

	private Collider2D collider;

	private Transform currentBulletOrigin;

	private Coroutine deathCoroutine;

	private Direction lastShootDirection;

	public bool isActive { get; set; }

	private void Start()
	{
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		scaleX = base.transform.localScale.x;
		collider = GetComponent<Collider2D>();
		collider.enabled = false;
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
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		hp -= info.damage;
		if (hp <= 0f && deathCoroutine == null)
		{
			Level.Current.RegisterMinionKilled();
			StopAllCoroutines();
			deathCoroutine = StartCoroutine(death_cr());
		}
	}

	public void SetProperties(LevelProperties.RumRunners.Spider properties, RumRunnersLevelSpider spider)
	{
		this.properties = properties;
		this.spider = spider;
	}

	public void CopAppear(Vector3 appearPos, bool isPink, bool goingLeft)
	{
		if (deathCoroutine == null)
		{
			Vector3 vector = spawnPositionOffset;
			vector.x *= ((!goingLeft) ? 1 : (-1));
			base.transform.position = appearPos + vector;
			this.isPink = isPink;
			hp = properties.copHealth;
			StartCoroutine(shooting_cr());
			base.transform.SetScale((!goingLeft) ? scaleX : (0f - scaleX));
			isActive = true;
		}
	}

	private IEnumerator shooting_cr()
	{
		collider.enabled = true;
		gunSmokeRenderer.enabled = !isPink;
		gunSmokeParryRenderer.enabled = isPink;
		lastShootDirection = calculateDirection();
		string animatorParameter;
		string stateBaseName;
		if (lastShootDirection == Direction.Down)
		{
			animatorParameter = "ShootingDown";
			stateBaseName = "ShootDown";
			currentBulletOrigin = bulletOriginDown;
		}
		else if (lastShootDirection == Direction.Up)
		{
			animatorParameter = "ShootingUp";
			stateBaseName = "ShootUp";
			currentBulletOrigin = bulletOriginUp;
		}
		else
		{
			animatorParameter = "Shooting";
			stateBaseName = "ShootStraight";
			currentBulletOrigin = bulletOriginStraight;
		}
		Coroutine alignmentCoroutine = StartCoroutine(align_cr());
		base.animator.SetBool(animatorParameter, value: true);
		yield return base.animator.WaitForAnimationToStart(this, stateBaseName + "Hold");
		yield return CupheadTime.WaitForSeconds(this, properties.copAttackWarning);
		base.animator.SetTrigger("Shoot");
		yield return base.animator.WaitForAnimationToStart(this, stateBaseName + "ExitHold");
		yield return CupheadTime.WaitForSeconds(this, properties.copExitDelay);
		base.animator.SetTrigger("ShootExit");
		yield return base.animator.WaitForAnimationToStart(this, "ShootExit");
		yield return null;
		while (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
		{
			yield return null;
		}
		collider.enabled = false;
		base.transform.position = new Vector3(0f, 1000f);
		base.animator.SetBool(animatorParameter, value: false);
		isActive = false;
		StopCoroutine(alignmentCoroutine);
	}

	private IEnumerator align_cr()
	{
		YieldInstruction waitInstruction = new WaitForFixedUpdate();
		while (true)
		{
			yield return waitInstruction;
			base.transform.SetPosition(null, RumRunnersLevel.GroundWalkingPosY(base.transform.position, collider));
		}
	}

	private IEnumerator death_cr()
	{
		SFX_RUMRUN_Police_DiePoof();
		Vector3 puffPosition = collider.bounds.center;
		base.animator.SetBool("Shooting", value: false);
		base.animator.SetBool("ShootingUp", value: false);
		base.animator.SetBool("ShootingDown", value: false);
		isActive = false;
		collider.enabled = false;
		base.transform.position = puffPosition;
		base.animator.SetBool("Die", value: true);
		yield return base.animator.WaitForAnimationToStart(this, "Death");
		while (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
		{
			yield return null;
		}
		base.transform.position = new Vector3(0f, 1000f);
		base.animator.SetBool("Die", value: false);
		deathCoroutine = null;
	}

	private void animationEvent_SpawnBullet()
	{
		Vector3 vector = spider.transform.position - base.transform.position;
		RumRunnersLevelPoliceBullet rumRunnersLevelPoliceBullet = (RumRunnersLevelPoliceBullet)regularBullet.Create(currentBulletOrigin.transform.position, MathUtils.DirectionToAngle(vector), properties.copBulletSpeed);
		rumRunnersLevelPoliceBullet.spiderDamage = properties.copBulletBossDamage;
		rumRunnersLevelPoliceBullet.direction = lastShootDirection;
		rumRunnersLevelPoliceBullet.SetParryable(isPink);
		rumRunnersLevelPoliceBullet.GetComponent<SpriteRenderer>().flipY = Mathf.Sign(vector.x) < 0f;
	}

	private void animationEvent_ExitDisappeared()
	{
		collider.enabled = false;
	}

	private Direction calculateDirection()
	{
		if (base.transform.position.y - spider.transform.position.y > SpiderYDistanceThreshold)
		{
			return Direction.Down;
		}
		if (spider.transform.position.y - base.transform.position.y > SpiderYDistanceThreshold)
		{
			return Direction.Up;
		}
		return Direction.Straight;
	}

	private void AnimationEvent_SFX_RUMRUN_Police_GunShoot()
	{
		AudioManager.Play("sfx_dlc_rumrun_policegun_shoot");
		emitAudioFromObject.Add("sfx_dlc_rumrun_policegun_shoot");
	}

	private void SFX_RUMRUN_Police_DiePoof()
	{
		AudioManager.Play("sfx_dlc_rumrun_lackey_poof");
		emitAudioFromObject.Add("sfx_dlc_rumrun_lackey_poof");
		AudioManager.Stop("sfx_dlc_rumrun_policegun_shoot");
	}
}
