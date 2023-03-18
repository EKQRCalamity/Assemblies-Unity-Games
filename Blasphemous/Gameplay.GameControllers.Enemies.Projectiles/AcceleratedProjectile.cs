using Framework.Managers;
using Gameplay.GameControllers.Enemies.BellGhost;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Projectiles;

public class AcceleratedProjectile : StraightProjectile
{
	public Vector2 acceleration;

	public float velocityMultiplier = 1f;

	private Vector2 _accel;

	public bool directionInvertsAcceleration = true;

	public float deflectedVelocityMultiplier = 1.4f;

	public ProjectileReaction projectileReaction;

	public bool resetVelocityAndAccelerationWhenDisabled = true;

	[FoldoutGroup("Max speed settings", 0)]
	public bool clampsMaxSpeed;

	[FoldoutGroup("Max speed settings", 0)]
	[ShowIf("clampsMaxSpeed", true)]
	public float maxSpeed;

	[FoldoutGroup("Deflection settings", 0)]
	public bool deflectWithSword;

	[ShowIf("deflectWithSword", true)]
	[FoldoutGroup("Deflection settings", 0)]
	public bool bounceBackToTarget;

	[ShowIf("deflectWithSword", true)]
	[FoldoutGroup("Deflection settings", 0)]
	public LayerMask layerMaskOnBounceBack;

	[ShowIf("deflectWithSword", true)]
	[FoldoutGroup("Deflection settings", 0)]
	public float validBounceBackAngle = 30f;

	[ShowIf("deflectWithSword", true)]
	[FoldoutGroup("Deflection settings", 0)]
	public bool changeProjectileMaskAfterDeflection;

	[ShowIf("deflectWithSword", true)]
	[ShowIf("changeProjectileMaskAfterDeflection", true)]
	[FoldoutGroup("Deflection settings", 0)]
	public LayerMask altProjectileMask;

	[ShowIf("deflectWithSword", true)]
	[FoldoutGroup("Deflection settings", 0)]
	public bool changeExplosionMaskAfterDeflection;

	[ShowIf("deflectWithSword", true)]
	[ShowIf("changeExplosionMaskAfterDeflection", true)]
	[FoldoutGroup("Deflection settings", 0)]
	public LayerMask altExplosionMask;

	private Vector2 _targetToBounceBackOffset;

	private Transform _targetToBounceBack;

	private LayerMask _originalLayerMask;

	private AttackArea _attackArea;

	private int _bounceBackDamage;

	private LayerMask _defaultExplosionMask;

	private LayerMask _defaultProjectileMask;

	protected override void OnAwake()
	{
		base.OnAwake();
		_attackArea = GetComponentInChildren<AttackArea>();
		_originalLayerMask = _attackArea.enemyLayerMask;
		originalDamage = GetComponent<ProjectileWeapon>().damage;
		if (changeExplosionMaskAfterDeflection)
		{
			GameObject explosion = GetComponent<ProjectileWeapon>().explosion;
			AttackArea componentInChildren = explosion.GetComponentInChildren<AttackArea>();
			_defaultExplosionMask = componentInChildren.enemyLayerMask;
		}
		if (changeProjectileMaskAfterDeflection)
		{
			ProjectileWeapon component = GetComponent<ProjectileWeapon>();
			AttackArea componentInChildren2 = component.GetComponentInChildren<AttackArea>();
			_defaultProjectileMask = componentInChildren2.enemyLayerMask;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (deflectWithSword)
		{
			projectileReaction.OnProjectileHit += AcceleratedProjectile_OnProjectileHit;
		}
	}

	public override void Init(Vector3 direction, float speed)
	{
		base.Init(direction, speed);
		GetComponent<ProjectileWeapon>().SetDamage(originalDamage);
		_attackArea.SetLayerMask(_originalLayerMask);
		_accel = acceleration;
		if (directionInvertsAcceleration && direction.x < 0f)
		{
			_accel.x *= -1f;
		}
		if (changeExplosionMaskAfterDeflection)
		{
			GameObject explosion = GetComponent<ProjectileWeapon>().explosion;
			AttackArea componentInChildren = explosion.GetComponentInChildren<AttackArea>();
			componentInChildren.SetLayerMask(_defaultExplosionMask);
		}
		if (changeProjectileMaskAfterDeflection)
		{
			ProjectileWeapon component = GetComponent<ProjectileWeapon>();
			AttackArea componentInChildren2 = component.GetComponentInChildren<AttackArea>();
			componentInChildren2.SetLayerMask(_defaultProjectileMask);
		}
	}

	public override void Init(Vector3 origin, Vector3 target, float speed)
	{
		Init((target - origin).normalized, speed);
	}

	public void SetBouncebackData(Transform t, Vector2 offset, int dmg = 4)
	{
		_targetToBounceBack = t;
		_targetToBounceBackOffset = offset;
		_bounceBackDamage = dmg;
	}

	private void AcceleratedProjectile_OnProjectileHit(ProjectileReaction obj)
	{
		acceleration = Vector2.zero;
		Vector3 vector = Core.Logic.Penitent.transform.position + Vector3.up;
		Vector2 vector2 = (base.transform.position - vector).normalized;
		GetComponent<ProjectileWeapon>().SetDamage(_bounceBackDamage);
		if (bounceBackToTarget && _targetToBounceBack != null)
		{
			Vector2 vector3 = (_targetToBounceBack.position + (Vector3)_targetToBounceBackOffset - base.transform.position).normalized;
			_attackArea.SetLayerMask(layerMaskOnBounceBack);
			if (Vector2.Angle(vector3, vector2) < validBounceBackAngle)
			{
				_accel = Vector2.zero;
				vector2 = vector3;
				Debug.DrawRay(base.transform.position, vector2 * 10f, Color.green, 10f);
			}
		}
		velocity = vector2 * (velocity.magnitude * deflectedVelocityMultiplier);
		if (faceVelocityDirection)
		{
			Vector2 normalized = velocity.normalized;
			float z = 57.29578f * Mathf.Atan2(normalized.y, normalized.x);
			base.transform.eulerAngles = new Vector3(0f, 0f, z);
		}
		if (changeExplosionMaskAfterDeflection)
		{
			GameObject explosion = GetComponent<ProjectileWeapon>().explosion;
			AttackArea componentInChildren = explosion.GetComponentInChildren<AttackArea>();
			componentInChildren.SetLayerMask(altExplosionMask);
		}
		if (changeProjectileMaskAfterDeflection)
		{
			ProjectileWeapon component = GetComponent<ProjectileWeapon>();
			AttackArea componentInChildren2 = component.GetComponentInChildren<AttackArea>();
			componentInChildren2.SetLayerMask(altProjectileMask);
		}
	}

	public void SetAcceleration(Vector2 a)
	{
		_accel = a;
		acceleration = a;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		velocity += _accel * Time.deltaTime;
		velocity += velocity * ((velocityMultiplier - 1f) * Time.deltaTime);
		if (clampsMaxSpeed)
		{
			velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
		}
	}

	private void OnDisable()
	{
		if (resetVelocityAndAccelerationWhenDisabled)
		{
			velocity = Vector2.zero;
			acceleration = Vector2.zero;
		}
	}
}
