using FMODUnity;
using Framework.FrameworkCore;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Menina.Attack;

public class MeninaGroundWave : Weapon
{
	private float _timeSpawned;

	private const float TimeOffset = 0.1f;

	[Range(0f, 1f)]
	public float damageAmountFactor = 0.5f;

	[EventRef]
	public string HitSoundId;

	private static readonly int Fire = UnityEngine.Animator.StringToHash("FIRE");

	public AttackArea AttackArea { get; set; }

	public SpriteRenderer SpriteRenderer { get; set; }

	public UnityEngine.Animator Animator { get; set; }

	public EntityMotionChecker MotionChecker { get; set; }

	public bool IsSpawned { get; set; }

	private Hit GetHit
	{
		get
		{
			Hit result = default(Hit);
			result.AttackingEntity = base.gameObject;
			result.DamageType = DamageArea.DamageType.Normal;
			result.DamageAmount = WeaponOwner.Stats.Strength.Final * damageAmountFactor;
			result.HitSoundId = HitSoundId;
			result.Unnavoidable = false;
			return result;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		AttackArea = GetComponentInChildren<AttackArea>();
		Animator = GetComponent<UnityEngine.Animator>();
		MotionChecker = GetComponent<EntityMotionChecker>();
		SpriteRenderer = GetComponent<SpriteRenderer>();
	}

	protected override void OnStart()
	{
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		_timeSpawned += Time.deltaTime;
		if (!(_timeSpawned < 0.1f) && IsSpawned && !MotionChecker.HitsFloor)
		{
			Recycle();
		}
	}

	public void Attack()
	{
		Attack(GetHit);
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	public void SetOwner(Entity owner)
	{
		WeaponOwner = owner;
		AttackArea.Entity = owner;
		MotionChecker.EntityOwner = owner;
		SpriteRenderer.flipX = owner.Status.Orientation == EntityOrientation.Left;
	}

	public void TriggerWave()
	{
		if ((bool)Animator)
		{
			Animator.SetTrigger(Fire);
		}
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		_timeSpawned = 0f;
		IsSpawned = true;
	}

	public void Recycle()
	{
		IsSpawned = false;
		Destroy();
	}
}
