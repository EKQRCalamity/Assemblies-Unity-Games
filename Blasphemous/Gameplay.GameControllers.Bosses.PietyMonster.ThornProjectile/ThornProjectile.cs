using System;
using System.Collections;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.GameControllers.Bosses.PietyMonster.ThornProjectile;

public class ThornProjectile : Weapon, IDamageable, IProjectileAttack
{
	public const float MinHorizontalDistance = 3.5f;

	public AttackArea AttackArea;

	public Enemy AttackingEntity;

	public Transform Projectile;

	public float StraightThrowSpeed = 5f;

	public float FiringAngle = 30f;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	protected string HitSound;

	[FormerlySerializedAs("DamageAmount")]
	[SerializeField]
	[BoxGroup("Damage", true, false, 0)]
	protected float DamageAmount = 15f;

	[Tooltip("Damage factor based on entity damage base amount.")]
	[Range(0f, 1f)]
	public float DamageFactor = 0.5f;

	private float _startTime;

	private float _journeyLength;

	private Animator _animator;

	public AnimationCurve SpeedCurve;

	public SpriteRenderer Renderer { get; set; }

	public Transform Target { get; set; }

	public GhostTrailGenerator GhostTrail { get; set; }

	public bool IsBroken { get; set; }

	protected EnemyDamageArea DamageArea { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		_animator = GetComponentInChildren<Animator>();
		Renderer = GetComponentInChildren<SpriteRenderer>();
		GhostTrail = GetComponentInChildren<GhostTrailGenerator>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		AttackArea.OnEnter += AttackAreaOnEnter;
	}

	private void AttackAreaOnEnter(object sender, Collider2DParam e)
	{
		if (AttackingEntity.Status.Dead)
		{
			return;
		}
		GameObject gameObject = e.Collider2DArg.gameObject;
		if (gameObject.CompareTag("Penitent"))
		{
			Entity componentInParent = gameObject.GetComponentInParent<Entity>();
			if (componentInParent.Status.Unattacable)
			{
				return;
			}
			Hit hit = default(Hit);
			hit.AttackingEntity = AttackingEntity.gameObject;
			hit.DamageType = Gameplay.GameControllers.Entities.DamageArea.DamageType.Normal;
			hit.DamageAmount = DamageAmount;
			hit.HitSoundId = HitSound;
			Hit weapondHit = hit;
			Attack(weapondHit);
		}
		Break();
	}

	public void SetOwner(Enemy enemy)
	{
		WeaponOwner = (AttackArea.Entity = enemy);
		AttackingEntity = enemy;
		GhostTrail.EntityOwner = enemy;
		DamageArea.SetOwner(enemy);
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	private void Break()
	{
		if (!(_animator == null))
		{
			StopMovement();
			_animator.SetTrigger("BREAK");
			if (!IsBroken)
			{
				IsBroken = true;
			}
			if (AttackArea.WeaponCollider.enabled)
			{
				AttackArea.WeaponCollider.enabled = false;
			}
			if (GhostTrail.EnableGhostTrail)
			{
				GhostTrail.EnableGhostTrail = false;
			}
			Core.Audio.PlaySfxOnCatalog("PietatSpitExplosion");
		}
	}

	public void Throw(Vector3 target)
	{
		_startTime = Time.time;
		_journeyLength = Vector3.Distance(base.transform.position, target);
		ParabolicThrow(SetGoal(target));
		if (!GhostTrail.EnableGhostTrail)
		{
			GhostTrail.EnableGhostTrail = true;
		}
	}

	private Vector3 SetGoal(Vector3 target)
	{
		Vector3 result = target;
		EntityOrientation orientation = AttackingEntity.Status.Orientation;
		result.x = ((orientation != EntityOrientation.Left) ? Mathf.Clamp(result.x, AttackingEntity.transform.position.x + 3.5f, target.x) : Mathf.Clamp(result.x, target.x, AttackingEntity.transform.position.x - 3.5f));
		return result;
	}

	private IEnumerator StraightThrowCoroutine(Vector3 target)
	{
		while (!IsBroken)
		{
			float distCovered = (Time.time - _startTime) * StraightThrowSpeed;
			float fracJourney = distCovered / _journeyLength;
			base.transform.position = Vector3.Lerp(base.transform.position, target, SpeedCurve.Evaluate(fracJourney));
			yield return new WaitForEndOfFrame();
		}
		if (!IsBroken)
		{
			Break();
		}
	}

	private void ParabolicThrow(Vector3 target)
	{
		float num = target.x - base.transform.position.x;
		float num2 = target.y - base.transform.position.y;
		float f = Mathf.Atan((num2 + 7f) / num);
		float num3 = num / Mathf.Cos(f);
		float x = num3 * Mathf.Cos(f);
		float y = num3 * Mathf.Sin(f);
		Rigidbody2D component = GetComponent<Rigidbody2D>();
		component.velocity = new Vector2(x, y);
	}

	private IEnumerator ParabolicThrowCoroutine(Vector3 target)
	{
		Projectile = base.transform;
		float gravity = Mathf.Abs(Physics2D.gravity.y);
		Projectile.position = base.transform.position + new Vector3(0f, 0f, 0f);
		float targetDistance = Vector3.Distance(Projectile.position, target);
		float rockVelocity = targetDistance / (Mathf.Sin(2f * FiringAngle * ((float)Math.PI / 180f)) / gravity);
		float Vx = Mathf.Sqrt(rockVelocity) * Mathf.Cos(FiringAngle * ((float)Math.PI / 180f));
		float Vy = Mathf.Sqrt(rockVelocity) * Mathf.Sin(FiringAngle * ((float)Math.PI / 180f));
		float flightDuration = targetDistance / Vx;
		Projectile.rotation = Quaternion.LookRotation(target - Projectile.position);
		Renderer.transform.rotation = Quaternion.LookRotation(Vector3.zero);
		float elapseTime = 0f;
		while (elapseTime < flightDuration && !IsBroken)
		{
			Projectile.Translate(0f, (Vy - gravity * elapseTime) * Time.deltaTime, Vx * Time.deltaTime);
			elapseTime += Time.deltaTime;
			yield return null;
		}
		if (!IsBroken)
		{
			Break();
		}
	}

	public override void OnHit(Hit weaponHit)
	{
		StopMovement();
	}

	public void Damage(Hit hit)
	{
		if (!IsBroken)
		{
			Core.Audio.PlaySfxOnCatalog("PietatSpitHit");
			Break();
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public void HitsOnFloor()
	{
		if (AttackArea.WeaponCollider.enabled)
		{
			AttackArea.WeaponCollider.enabled = false;
		}
		StopMovement();
		if (Renderer.enabled)
		{
			Renderer.enabled = false;
		}
		if (GhostTrail.EnableGhostTrail)
		{
			GhostTrail.EnableGhostTrail = false;
		}
		_animator.SetTrigger("BREAK");
		Core.Audio.PlaySfxOnCatalog("PietatSpitExplosion");
	}

	public void StopMovement()
	{
		Rigidbody2D component = GetComponent<Rigidbody2D>();
		if (!(component == null))
		{
			component.velocity = Vector2.zero;
			component.isKinematic = true;
		}
	}

	public bool BleedOnImpact()
	{
		return false;
	}

	public bool SparkOnImpact()
	{
		return false;
	}

	public void SetProjectileWeaponDamage(int damage)
	{
		DamageAmount = damage;
	}

	public void SetProjectileWeaponDamage(Projectile projectile, int damage)
	{
	}
}
