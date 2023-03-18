using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Roller.Attack;

public class RollerProjectile : Weapon
{
	private Hit _projectileHit;

	public float DamageAmount = 10f;

	private Vector2 _defaultMotionVelocity;

	private bool _wallImpact;

	[EventRef]
	public string HitSoundId;

	[EventRef]
	public string DestroyedByHitFx;

	public UnityEngine.Animator ProjectileAnimator { get; private set; }

	public StraightProjectile Motion { get; private set; }

	public AttackArea AttackArea { get; private set; }

	public SpriteRenderer SpriteRenderer { get; private set; }

	public CollisionSensor CollisionSensor { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Motion = GetComponent<StraightProjectile>();
		AttackArea = GetComponentInChildren<AttackArea>();
		SpriteRenderer = GetComponent<SpriteRenderer>();
		ProjectileAnimator = GetComponent<UnityEngine.Animator>();
		_defaultMotionVelocity = new Vector2(Motion.velocity.x, Motion.velocity.y);
		CollisionSensor = GetComponentInChildren<CollisionSensor>();
		AttackArea.OnEnter += OnEnterAttackArea;
		_projectileHit = new Hit
		{
			AttackingEntity = base.gameObject,
			DamageType = DamageArea.DamageType.Normal,
			DamageAmount = DamageAmount,
			HitSoundId = HitSoundId
		};
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!SpriteRenderer.isVisible)
		{
			Destroy();
		}
		if (CollisionSensor.IsColliding() && !_wallImpact)
		{
			_wallImpact = true;
			StopByImpact();
		}
	}

	private void OnEnterAttackArea(object sender, Collider2DParam e)
	{
		Attack(_projectileHit);
		StopByImpact();
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	private void StopByImpact()
	{
		Motion.velocity = Vector2.zero;
		ProjectileAnimator.SetTrigger("IMPACT");
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		_wallImpact = false;
		Motion.velocity.x = Mathf.Abs(_defaultMotionVelocity.x);
	}

	public void SetOwner(Entity owner)
	{
		Motion.owner = owner;
		AttackArea.Entity = owner;
		WeaponOwner = owner;
		Motion.velocity = _defaultMotionVelocity;
		if (owner.Status.Orientation == EntityOrientation.Left)
		{
			Motion.velocity.x *= -1f;
		}
		SpriteRenderer.flipX = owner.Status.Orientation == EntityOrientation.Left;
	}

	public void Dispose()
	{
		Destroy();
	}

	private void OnDestroy()
	{
		AttackArea.OnEnter -= OnEnterAttackArea;
	}

	public bool BleedOnImpact()
	{
		return false;
	}

	public bool SparkOnImpact()
	{
		return true;
	}

	public void Damage(Hit hit)
	{
		Core.Audio.PlaySfx(DestroyedByHitFx);
		StopByImpact();
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}
}
