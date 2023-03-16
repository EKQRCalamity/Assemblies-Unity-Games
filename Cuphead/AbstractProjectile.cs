using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractProjectile : AbstractCollidableObject
{
	[Serializable]
	public class CollisionProperties
	{
		public bool Walls = true;

		public bool Ceiling = true;

		public bool Ground = true;

		public bool Enemies;

		public bool EnemyProjectiles;

		public bool Player;

		public bool PlayerProjectiles;

		public bool Other;

		public CollisionProperties Copy()
		{
			return MemberwiseClone() as CollisionProperties;
		}

		public void SetAll(bool b)
		{
			Walls = b;
			Ceiling = b;
			Ground = b;
			Enemies = b;
			EnemyProjectiles = b;
			Player = b;
			PlayerProjectiles = b;
			Other = b;
		}

		public void All()
		{
			SetAll(b: true);
		}

		public void None()
		{
			SetAll(b: false);
		}

		public void OnlyPlayer()
		{
			SetAll(b: false);
			Player = true;
		}

		public void OnlyEnemies()
		{
			SetAll(b: false);
			Player = true;
		}

		public void OnlyBounds()
		{
			SetAll(b: false);
			SetBounds(b: true);
		}

		public void SetBounds(bool b)
		{
			Walls = b;
			Ceiling = b;
			Ground = b;
		}

		public void PlayerProjectileDefault()
		{
			SetAll(b: false);
			SetBounds(b: true);
			Enemies = true;
			Other = true;
		}
	}

	protected static string Variant = "Variant";

	protected static string MaxVariants = "MaxVariants";

	protected static string OnDeathTrigger = "OnDeath";

	protected static string Parry = "Parry";

	private Vector3 startPosition;

	private Vector3 lastPosition;

	protected MeterScoreTracker tracker;

	private bool hasBeenRendered;

	[SerializeField]
	private bool _canParry;

	protected bool _countParryTowardsScore = true;

	protected bool missed = true;

	protected DamageDealer damageDealer;

	private float _setYPadding = 150f;

	private DamageDealer.DamageSource damageSource;

	public float Damage = 1f;

	public float DamageRate;

	public PlayerId PlayerId = PlayerId.None;

	public DamageDealer.DamageTypesManager DamagesType;

	public CollisionProperties CollisionDeath;

	[NonSerialized]
	public float DestroyDistance = 3000f;

	[NonSerialized]
	public bool DestroyDistanceAnimated;

	private LevelPlayerWeaponFiringHitbox firingHitbox;

	private bool firstUpdate = true;

	private static readonly Collider2D[] ColliderBuffer = new Collider2D[500];

	private static HashSet<DamageReceiver> DamageReceiverSearchSet = new HashSet<DamageReceiver>();

	private static List<DamageReceiver> DamageReceiverComponentBuffer = new List<DamageReceiver>();

	public bool CanParry => _canParry;

	public bool CountParryTowardsScore => _countParryTowardsScore;

	protected float distance { get; private set; }

	protected float lifetime { get; private set; }

	public bool dead { get; private set; }

	public float StoneTime { get; private set; }

	public virtual float ParryMeterMultiplier => 1f;

	public DamageDealer.DamageSource DamageSource
	{
		get
		{
			return damageSource;
		}
		set
		{
			damageSource = value;
		}
	}

	public float DamageMultiplier
	{
		get
		{
			float num = PlayerManager.DamageMultiplier;
			if (base.tag == "PlayerProjectile")
			{
				if (PlayerManager.GetPlayer(PlayerId).stats.Loadout.charm == Charm.charm_health_up_1)
				{
					num *= 1f - WeaponProperties.CharmHealthUpOne.weaponDebuff;
				}
				else if (PlayerManager.GetPlayer(PlayerId).stats.Loadout.charm == Charm.charm_health_up_2)
				{
					num *= 1f - WeaponProperties.CharmHealthUpTwo.weaponDebuff;
				}
				else if (PlayerManager.GetPlayer(PlayerId).stats.Loadout.charm == Charm.charm_EX && Level.Current.playerMode == PlayerMode.Plane && this is PlaneWeaponPeashotExProjectile)
				{
					num *= 1f - WeaponProperties.CharmEXCharm.planePeashotEXDebuff;
				}
			}
			return num;
		}
	}

	protected virtual float DestroyLifetime => 20f;

	protected virtual bool DestroyedAfterLeavingScreen => false;

	protected virtual float SafeTime => 0.005f;

	protected virtual float PlayerSafeTime => 0f;

	protected virtual float EnemySafeTime => 0f;

	protected override bool allowCollisionPlayer => lifetime > PlayerSafeTime;

	protected override bool allowCollisionEnemy => lifetime > EnemySafeTime;

	public event DamageDealer.OnDealDamageHandler OnDealDamageEvent;

	public event Action<AbstractProjectile> OnDie;

	protected override void Awake()
	{
		base.Awake();
		distance = 0f;
		lifetime = 0f;
		StoneTime = -1f;
		if (CompareTag("PlayerProjectile") || !CompareTag("EnemyProjectile"))
		{
		}
		if (base.gameObject.layer != 12)
		{
			base.gameObject.layer = 12;
		}
		RandomizeVariant();
		if (Level.Current != null && Level.Current.CurrentScene == Scenes.scene_level_airplane)
		{
			_setYPadding = -600f;
		}
	}

	protected virtual void Start()
	{
		damageDealer = new DamageDealer(this);
		damageDealer.OnDealDamage += OnDealDamage;
		damageDealer.SetStoneTime(StoneTime);
		damageDealer.PlayerId = PlayerId;
		if (tracker != null)
		{
			tracker.Add(damageDealer);
		}
	}

	protected virtual void Update()
	{
		Vector3 position = base.transform.position;
		if (lifetime == 0f)
		{
			lastPosition = (startPosition = position);
		}
		if (DestroyDistance > 0f && Vector3.Distance(startPosition, position) >= DestroyDistance)
		{
			OnDieDistance();
		}
		distance += Vector3.Distance(lastPosition, position);
		lastPosition = position;
		if (DestroyLifetime > 0f && lifetime >= DestroyLifetime)
		{
			OnDieLifetime();
		}
		lifetime += Time.deltaTime;
		if (DestroyedAfterLeavingScreen)
		{
			bool flag = CupheadLevelCamera.Current.ContainsPoint(position, new Vector2(150f, _setYPadding));
			if (hasBeenRendered && !flag)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			if (!hasBeenRendered)
			{
				hasBeenRendered = flag;
			}
		}
	}

	protected void ResetLifetime()
	{
		lifetime = 0f;
	}

	protected void ResetDistance()
	{
		distance = 0f;
	}

	protected override void checkCollision(Collider2D col, CollisionPhase phase)
	{
		if (!(lifetime < SafeTime))
		{
			base.checkCollision(col, phase);
		}
	}

	public virtual AbstractProjectile Create()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(base.gameObject);
		return gameObject.GetComponent<AbstractProjectile>();
	}

	public virtual AbstractProjectile Create(Vector2 position)
	{
		AbstractProjectile abstractProjectile = Create();
		abstractProjectile.transform.position = position;
		return abstractProjectile;
	}

	public virtual AbstractProjectile Create(Vector2 position, float rotation)
	{
		AbstractProjectile abstractProjectile = Create(position);
		abstractProjectile.transform.SetEulerAngles(0f, 0f, rotation);
		return abstractProjectile;
	}

	public virtual AbstractProjectile Create(Vector2 position, float rotation, Vector2 scale)
	{
		AbstractProjectile abstractProjectile = Create(position, rotation);
		abstractProjectile.transform.SetScale(scale.x, scale.y, 1f);
		return abstractProjectile;
	}

	protected virtual void OnDealDamage(float damage, DamageReceiver receiver, DamageDealer damageDealer)
	{
		if (this.OnDealDamageEvent != null)
		{
			this.OnDealDamageEvent(damage, receiver, damageDealer);
		}
	}

	public bool GetDamagesType(DamageReceiver.Type type)
	{
		return DamagesType.GetType(type);
	}

	public virtual void SetParryable(bool parryable)
	{
		_canParry = parryable;
		SetBool(Parry, parryable);
	}

	public void SetStoneTime(float stoneTime)
	{
		StoneTime = stoneTime;
		if (damageDealer != null)
		{
			damageDealer.SetStoneTime(stoneTime);
		}
	}

	protected override void OnCollisionWalls(GameObject hit, CollisionPhase phase)
	{
		if (CollisionDeath.Walls)
		{
			OnCollisionDie(hit, phase);
		}
	}

	protected override void OnCollisionCeiling(GameObject hit, CollisionPhase phase)
	{
		if (CollisionDeath.Ceiling)
		{
			OnCollisionDie(hit, phase);
		}
	}

	protected override void OnCollisionGround(GameObject hit, CollisionPhase phase)
	{
		if (CollisionDeath.Ground)
		{
			OnCollisionDie(hit, phase);
		}
	}

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		missed = false;
		if (CollisionDeath.Enemies)
		{
			OnCollisionDie(hit, phase);
		}
	}

	protected override void OnCollisionEnemyProjectile(GameObject hit, CollisionPhase phase)
	{
		if (CollisionDeath.EnemyProjectiles)
		{
			OnCollisionDie(hit, phase);
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (CollisionDeath.Player)
		{
			OnCollisionDie(hit, phase);
		}
	}

	protected override void OnCollisionPlayerProjectile(GameObject hit, CollisionPhase phase)
	{
		if (CollisionDeath.PlayerProjectiles)
		{
			OnCollisionDie(hit, phase);
		}
	}

	protected override void OnCollisionOther(GameObject hit, CollisionPhase phase)
	{
		if (CollisionDeath.Other)
		{
			OnCollisionDie(hit, phase);
		}
	}

	public virtual void OnCollisionWideShotEX(GameObject hit, CollisionPhase phase)
	{
		OnCollisionDie(hit, phase);
	}

	protected virtual void OnCollisionDie(GameObject hit, CollisionPhase phase)
	{
		if (!dead)
		{
			Die();
		}
	}

	protected virtual void OnDieAnimationComplete()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public virtual void OnParry(AbstractPlayerController player)
	{
		if (CanParry)
		{
			OnParryDie();
		}
	}

	public virtual void OnParryDie()
	{
		UnityEngine.Object.Destroy(base.gameObject);
		Die();
	}

	public override void OnLevelEnd()
	{
		base.OnLevelEnd();
		Die();
	}

	protected virtual void Die()
	{
		dead = true;
		if (GetComponent<Collider2D>() != null)
		{
			GetComponent<Collider2D>().enabled = false;
		}
		RandomizeVariant();
		SetTrigger(OnDeathTrigger);
		if (this.OnDie != null)
		{
			this.OnDie(this);
		}
	}

	protected virtual void OnDieDistance()
	{
		if (DestroyDistanceAnimated)
		{
			Die();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	protected virtual void OnDieLifetime()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected virtual void SetTrigger(string trigger)
	{
		base.animator.SetTrigger(trigger);
	}

	protected virtual void SetInt(string integer, int i)
	{
		base.animator.SetInteger(integer, i);
	}

	protected virtual void SetBool(string boolean, bool b)
	{
		base.animator.SetBool(boolean, b);
	}

	protected virtual int GetVariants()
	{
		return base.animator.GetInteger(MaxVariants);
	}

	protected virtual void RandomizeVariant()
	{
		int i = UnityEngine.Random.Range(0, GetVariants());
		SetInt(Variant, i);
	}

	public void AddFiringHitbox(LevelPlayerWeaponFiringHitbox hitbox)
	{
		firingHitbox = hitbox;
		RegisterCollisionChild(hitbox);
		GetComponent<Collider2D>().enabled = false;
	}

	protected virtual void FixedUpdate()
	{
		if (firingHitbox != null)
		{
			if (firstUpdate)
			{
				firstUpdate = false;
			}
			else
			{
				if (!dead)
				{
					GetComponent<Collider2D>().enabled = true;
				}
				UnityEngine.Object.Destroy(firingHitbox.gameObject);
				firingHitbox = null;
			}
		}
		if (damageDealer != null)
		{
			damageDealer.FixedUpdate();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (firingHitbox != null)
		{
			UnityEngine.Object.Destroy(firingHitbox.gameObject);
		}
	}

	public virtual void AddToMeterScoreTracker(MeterScoreTracker tracker)
	{
		this.tracker = tracker;
		if (damageDealer != null)
		{
			tracker.Add(damageDealer);
		}
	}

	public static IEnumerable<DamageReceiver> FindOverlapScreenDamageReceivers()
	{
		DamageReceiverComponentBuffer.Clear();
		DamageReceiverSearchSet.Clear();
		Vector2 padding = new Vector2(100f, 100f);
		Rect rect = CupheadLevelCamera.Current.CalculateContainsBounds(padding);
		int num = Physics2D.OverlapBoxNonAlloc(rect.center, rect.size, 0f, ColliderBuffer);
		for (int i = 0; i < num; i++)
		{
			DamageReceiverComponentBuffer.Clear();
			Collider2D collider2D = ColliderBuffer[i];
			collider2D.GetComponentsInParent(includeInactive: true, DamageReceiverComponentBuffer);
			DamageReceiverSearchSet.UnionWith(DamageReceiverComponentBuffer);
		}
		return DamageReceiverSearchSet;
	}
}
