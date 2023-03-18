using System.Collections;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Bosses.Snake;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.AlliedCherub.AI;

public class AlliedCherubBehaviour : MonoBehaviour
{
	[FoldoutGroup("Chasing player", true, 0)]
	public float ChasingElongation = 0.5f;

	[FoldoutGroup("Chasing player", true, 0)]
	public float ChasingSpeed = 5f;

	[FoldoutGroup("Chasing enemy", true, 0)]
	public float ChasingEnemyElongation = 0.25f;

	[FoldoutGroup("Chasing enemy", true, 0)]
	public float ChasingEnemySpeed = 6f;

	private Vector3 _velocity = Vector3.zero;

	public BossInstantProjectileAttack railgun;

	public MasterShaderEffects spriteEffects;

	public float attackRange = 5f;

	public VisionCone visionCone;

	public float cherubBaseDamage = 60f;

	private float attackCd = 1f;

	private bool shooting;

	private AlliedCherub AlliedCherub { get; set; }

	public Entity Target { get; private set; }

	private void Awake()
	{
		AlliedCherub = GetComponent<AlliedCherub>();
		spriteEffects = GetComponentInChildren<MasterShaderEffects>();
		visionCone = GetComponentInChildren<VisionCone>();
	}

	private void Start()
	{
		AlliedCherub.StateMachine.SwitchState<AlliedCherubIdleState>();
	}

	private void Update()
	{
		if (!shooting)
		{
			SetOrientation();
		}
		if (attackCd > 0f)
		{
			attackCd -= Time.deltaTime;
		}
	}

	public bool CanSeeEnemy(Enemy t)
	{
		if (t is Snake)
		{
			Snake snake = t as Snake;
			DamageArea activeDamageArea = snake.GetActiveDamageArea();
			return visionCone.CanSeeTarget(activeDamageArea.transform, "Enemy");
		}
		return visionCone.CanSeeTarget(t.transform, "Enemy", useColliderBounds: true);
	}

	public void ChasingAlly()
	{
		Vector3 position = Core.Logic.Penitent.transform.position;
		Vector3 entityPos = position + (Vector3)AlliedCherub.FlyingOffset;
		float elongation = ((!Core.Logic.Penitent.Status.IsGrounded) ? 0.15f : ChasingElongation);
		ChaseEntity(entityPos, elongation, ChasingSpeed);
	}

	public void ChaseEntity(Vector3 entityPos, float elongation, float speed)
	{
		AlliedCherub.transform.position = Vector3.SmoothDamp(AlliedCherub.transform.position, entityPos + Vector3.up, ref _velocity, elongation, speed);
	}

	public bool IsInAttackRange(Vector2 v)
	{
		return Vector2.Distance(base.transform.position, v) < attackRange;
	}

	public bool CanAttack()
	{
		return attackCd <= 0f;
	}

	public bool IsShooting()
	{
		return shooting;
	}

	public IEnumerator ShootCoroutine(Collider2D target)
	{
		shooting = true;
		Vector2 dir = (Vector2)target.bounds.center - (Vector2)base.transform.position;
		float duration = 0.4f;
		if (dir.magnitude > 2f)
		{
			base.transform.DOLocalMove((Vector2)base.transform.position + dir.normalized * 2f, duration).SetEase(Ease.OutCubic);
		}
		if ((bool)spriteEffects)
		{
			spriteEffects.TriggerColorizeLerp(0f, 1f, duration);
		}
		SetOrientation(dir);
		yield return new WaitForSeconds(duration);
		if (target != null)
		{
			dir = (Vector2)target.bounds.center - (Vector2)base.transform.position;
		}
		railgun.Shoot(base.transform.position, dir.normalized);
		shooting = false;
		AlliedCherub.Store();
	}

	public void ShootRailgun(Collider2D target)
	{
		if (target != null)
		{
			StartCoroutine(ShootCoroutine(target));
		}
	}

	private void SetOrientation()
	{
		if (_velocity.x > 0.1f)
		{
			AlliedCherub.SetOrientation(EntityOrientation.Right);
		}
		else if (_velocity.x < -0.1f)
		{
			AlliedCherub.SetOrientation(EntityOrientation.Left);
		}
	}

	private void SetOrientation(Vector2 dir)
	{
		if (dir.x > 0f)
		{
			AlliedCherub.SetOrientation(EntityOrientation.Right);
		}
		else
		{
			AlliedCherub.SetOrientation(EntityOrientation.Left);
		}
	}

	public void OnTargetLost()
	{
		AlliedCherub.StateMachine.SwitchState<AlliedCherubIdleState>();
	}

	public void Attack(Entity currentTarget)
	{
		SetAttackDamage();
		Target = currentTarget;
		AlliedCherub.StateMachine.SwitchState<AlliedCherubAttackState>();
	}

	public void SetAttackDamage()
	{
		railgun.SetDamageStrength(CalculateDamageStrength(Core.Logic.Penitent.Stats.PrayerStrengthMultiplier.Final));
		railgun.SetDamage(Mathf.RoundToInt(cherubBaseDamage));
	}

	private float CalculateDamageStrength(float prayerStrMult)
	{
		return 1f + 0.125f * (prayerStrMult - 1f);
	}

	private void OnEnable()
	{
		attackCd = 1f;
		spriteEffects.SetColorizeStrength(0f);
	}

	private void OnDisable()
	{
		AlliedCherub.StateMachine.SwitchState<AlliedCherubIdleState>();
	}
}
