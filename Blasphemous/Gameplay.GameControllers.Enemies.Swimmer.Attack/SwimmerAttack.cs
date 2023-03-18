using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Swimmer.Attack;

public class SwimmerAttack : EnemyAttack
{
	public float RndThrowHorOffset = 1f;

	public float ProjectileForce = 3f;

	public float FiringAngle;

	protected Swimmer swimmer;

	public float ProjectileRange = 4f;

	public GameObject Projectile;

	public Vector3 JumpPosition { get; set; }

	public bool IsTargetTouched { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		swimmer = (Swimmer)base.EntityOwner;
		swimmer.GetComponentInChildren<AttackArea>().OnEnter += OnEnter;
		swimmer.GetComponentInChildren<AttackArea>().OnExit += OnExit;
	}

	private void OnExit(object sender, Collider2DParam e)
	{
		IsTargetTouched = false;
	}

	private void OnEnter(object sender, Collider2DParam e)
	{
		IsTargetTouched = true;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		swimmer.DamageByContact = !swimmer.Status.IsGrounded;
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
	}

	public void ThrowProjectile()
	{
		Vector2[] projectileTargets = GetProjectileTargets();
		Vector3 position = base.transform.position;
		Vector2 vector = Vector2.zero;
		for (int i = 0; i < projectileTargets.Length; i++)
		{
			PoolManager.ObjectInstance objectInstance = PoolManager.Instance.ReuseObject(Projectile, position, Quaternion.identity);
			if (objectInstance == null)
			{
				break;
			}
			SwimmerWeapon componentInChildren = objectInstance.GameObject.GetComponentInChildren<SwimmerWeapon>();
			if (componentInChildren != null)
			{
				componentInChildren.SetOwner(base.EntityOwner);
			}
			if (vector == Vector2.zero)
			{
				vector = GetProjectileMotion(position, projectileTargets[i]);
			}
			float x = ((i % 2 != 0) ? (0f - vector.x) : vector.x);
			SetProjectileSpeed(vel: new Vector2(x, vector.y), projectile: objectInstance.GameObject);
		}
	}

	private void SetProjectileSpeed(GameObject projectile, Vector2 vel)
	{
		Rigidbody2D component = projectile.GetComponent<Rigidbody2D>();
		component.velocity = vel;
	}

	private Vector3 GetProjectileMotion(Vector3 startPosition, Vector2 target)
	{
		float num = target.x - startPosition.x;
		float num2 = target.y - startPosition.y;
		float f = Mathf.Atan((num2 + FiringAngle) / num);
		float num3 = num / Mathf.Cos(f);
		float x = num3 * Mathf.Cos(f);
		float y = num3 * Mathf.Sin(f);
		return new Vector2(x, y);
	}

	public Vector2[] GetProjectileTargets()
	{
		Vector2 vector = JumpPosition;
		Vector2 vector2 = new Vector2(vector.x - ProjectileRange, vector.y);
		Vector2 vector3 = new Vector2(vector.x + ProjectileRange, vector.y);
		return new Vector2[2] { vector2, vector3 };
	}

	private void OnDestroy()
	{
		swimmer.GetComponentInChildren<AttackArea>().OnEnter -= OnEnter;
		swimmer.GetComponentInChildren<AttackArea>().OnExit -= OnExit;
	}
}
