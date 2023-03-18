using FMODUnity;
using Framework.Util;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Actionables;

public class SimpleDamageArea : Weapon
{
	public bool horizontalDamage = true;

	[FoldoutGroup("Attack config", 0)]
	[EventRef]
	public string SoundHitFx;

	[FoldoutGroup("Attack config", 0)]
	public float damage;

	[FoldoutGroup("Attack config", 0)]
	public DamageArea.DamageType damageType;

	[FoldoutGroup("Attack config", 0)]
	public DamageArea.DamageElement damageElement;

	[FoldoutGroup("Attack config", 0)]
	public bool unavoidable;

	[FoldoutGroup("Attack config", 0)]
	public float force;

	[FoldoutGroup("Attack config", 0)]
	public bool DestroyProjectile;

	private Hit _areaAttackHit;

	private GameObject _damageEntityDummy;

	private const string PENITENT_TAG = "Penitent";

	public float secondsBetweenTick = 0.2f;

	private float _tickCounter;

	public AttackArea attackArea { get; set; }

	public float Damage
	{
		set
		{
			damage = value;
			CreateHit();
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		_damageEntityDummy = new GameObject("DamageDummy");
		_damageEntityDummy.transform.SetParent(base.transform);
		_damageEntityDummy.AddComponent<AreaAttackDummyEntity>();
	}

	public AreaAttackDummyEntity GetDummyEntity()
	{
		return _damageEntityDummy.GetComponent<AreaAttackDummyEntity>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		CreateHit();
		attackArea = GetComponentInChildren<AttackArea>();
		attackArea.OnEnter += AttackArea_OnEnter;
		attackArea.OnStay += AttackArea_OnStay;
	}

	private void CreateHit()
	{
		_areaAttackHit = new Hit
		{
			DamageAmount = damage,
			DamageElement = damageElement,
			DamageType = damageType,
			Force = force,
			Unnavoidable = unavoidable,
			AttackingEntity = _damageEntityDummy,
			HitSoundId = SoundHitFx,
			DestroysProjectiles = DestroyProjectile
		};
	}

	private void AttackArea_OnEnter(object sender, Collider2DParam e)
	{
		Vector2 vector = Vector2.zero;
		vector = ((!horizontalDamage) ? new Vector2(e.Collider2DArg.gameObject.transform.position.x, base.transform.position.y) : new Vector2(base.transform.position.x, e.Collider2DArg.gameObject.transform.position.y));
		_damageEntityDummy.transform.position = vector;
		ResetTickCounter();
		Attack(_areaAttackHit);
	}

	private void AttackArea_OnStay(object sender, Collider2DParam e)
	{
		if (!(_tickCounter > 0f))
		{
			Vector2 vector = Vector2.zero;
			vector = ((!horizontalDamage) ? new Vector2(e.Collider2DArg.gameObject.transform.position.x, base.transform.position.y) : new Vector2(base.transform.position.x, e.Collider2DArg.gameObject.transform.position.y));
			_damageEntityDummy.transform.position = vector;
			ResetTickCounter();
			Attack(_areaAttackHit);
		}
	}

	private void UpdateTick()
	{
		if (secondsBetweenTick > 0f)
		{
			secondsBetweenTick -= Time.deltaTime;
		}
	}

	private void ResetTickCounter()
	{
		_tickCounter = secondsBetweenTick;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		UpdateTick();
	}

	private void OnDestroy()
	{
		if ((bool)attackArea)
		{
			attackArea.OnStay -= AttackArea_OnStay;
		}
		if ((bool)attackArea)
		{
			attackArea.OnEnter -= AttackArea_OnEnter;
		}
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}
}
