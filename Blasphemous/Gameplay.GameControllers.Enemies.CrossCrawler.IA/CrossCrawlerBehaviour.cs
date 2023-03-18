using System;
using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.CrossCrawler.IA;

public class CrossCrawlerBehaviour : EnemyBehaviour
{
	[FoldoutGroup("Activation Settings", true, 0)]
	public float ActivationDistance;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float MinAttackDistance = 2f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float AttackCoolDown = 2f;

	[FoldoutGroup("Attack Settings", true, 0)]
	private float _currentAttackLapse;

	[FoldoutGroup("Vision Settings", true, 0)]
	public LayerMask sightCollisionMask;

	[FoldoutGroup("Vision Settings", true, 0)]
	public Vector2 sightOffset;

	[FoldoutGroup("Vision Settings", true, 0)]
	public float visionAngle;

	[FoldoutGroup("Vision Settings", true, 0)]
	public float sightDistance;

	public List<BoxCollider2D> orientationAffectedColliders;

	private bool isExecuted;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float DistanceToTarget { get; private set; }

	public CrossCrawler CrossCrawler { get; private set; }

	public bool Awaken { get; private set; }

	public override void OnAwake()
	{
		base.OnAwake();
		CrossCrawler = (CrossCrawler)Entity;
	}

	public override void OnStart()
	{
		base.OnStart();
		if (Core.Logic.Penitent != null)
		{
			Core.Logic.Penitent.OnDeath += PlayerOnDeath;
		}
	}

	private void Update()
	{
		DistanceToTarget = Vector2.Distance(CrossCrawler.transform.position, CrossCrawler.Target.transform.position);
		if (!base.IsAttacking)
		{
			_currentAttackLapse += Time.deltaTime;
		}
		if (DistanceToTarget <= ActivationDistance && !base.BehaviourTree.isRunning && !Awaken && !isExecuted)
		{
			Awaken = true;
			base.BehaviourTree.StartBehaviour();
		}
	}

	public override void Idle()
	{
		CrossCrawler.Audio.StopWalk();
		StopMovement();
	}

	public bool CanSeeTarget()
	{
		Transform target = GetTarget();
		Vector2 vector = (Vector2)base.transform.position + sightOffset;
		Vector2 vector2 = target.position + (Vector3)sightOffset * 0.5f;
		float num = Vector2.Distance(vector, target.position);
		if (num > sightDistance)
		{
			return false;
		}
		Vector2 vector3 = vector2 - vector;
		float num2 = Mathf.Atan2(Mathf.Abs(vector3.y), Mathf.Abs(vector3.x)) * 57.29578f;
		float num3 = visionAngle;
		if (num2 > num3)
		{
			Debug.DrawLine(vector, vector2, Color.magenta, 1f);
			return false;
		}
		RaycastHit2D[] array = new RaycastHit2D[1];
		if (Physics2D.LinecastNonAlloc(vector, vector2, array, sightCollisionMask) > 0)
		{
			if (array[0].collider.gameObject.layer == LayerMask.NameToLayer("Penitent"))
			{
				Debug.DrawLine(vector, array[0].point, Color.green, 1f);
				return true;
			}
			Debug.DrawLine(vector, array[0].point, Color.red, 1f);
		}
		else
		{
			Debug.DrawLine(vector, vector2, Color.red, 1f);
		}
		return false;
	}

	public void StartVulnerablePeriod()
	{
		CrossCrawler.VulnerablePeriod.StartVulnerablePeriod(CrossCrawler);
	}

	private void ToggleCollidersOrientation()
	{
		foreach (BoxCollider2D orientationAffectedCollider in orientationAffectedColliders)
		{
			Vector2 offset = orientationAffectedCollider.offset;
			offset.x *= -1f;
			orientationAffectedCollider.offset = offset;
		}
	}

	public override void ReadSpawnerConfig(SpawnBehaviourConfig config)
	{
		base.ReadSpawnerConfig(config);
		if (CrossCrawler.Status.Orientation == EntityOrientation.Left)
		{
			ToggleCollidersOrientation();
		}
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (targetPos.x > CrossCrawler.transform.position.x)
		{
			if (CrossCrawler.Status.Orientation != 0)
			{
				CrossCrawler.SetOrientation(EntityOrientation.Right, allowFlipRenderer: false);
				ToggleCollidersOrientation();
				CrossCrawler.Audio.StopWalk();
				CrossCrawler.AnimatorInyector.TurnAround();
			}
		}
		else if (CrossCrawler.Status.Orientation != EntityOrientation.Left)
		{
			CrossCrawler.SetOrientation(EntityOrientation.Left, allowFlipRenderer: false);
			ToggleCollidersOrientation();
			CrossCrawler.Audio.StopWalk();
			CrossCrawler.AnimatorInyector.TurnAround();
		}
	}

	private bool CantChase()
	{
		return base.IsAttacking || IsDead() || base.IsHurt || base.TurningAround || !CrossCrawler.MotionChecker.HitsFloor;
	}

	public void Chase(Vector3 position)
	{
		if (CantChase())
		{
			StopMovement();
			return;
		}
		LookAtTarget(position);
		float horizontalInput = ((Entity.Status.Orientation != 0) ? (-1f) : 1f);
		CrossCrawler.Input.HorizontalInput = horizontalInput;
		CrossCrawler.AnimatorInyector.Walk();
	}

	public override void Damage()
	{
	}

	public void Death()
	{
		StopMovement();
		base.BehaviourTree.StopBehaviour();
		CrossCrawler.Audio.StopWalk();
		CrossCrawler.AnimatorInyector.Death();
	}

	private void PlayerOnDeath()
	{
		base.BehaviourTree.StopBehaviour();
	}

	public void ResetCoolDown()
	{
		if (_currentAttackLapse > 0f)
		{
			_currentAttackLapse = 0f;
		}
	}

	public override void Attack()
	{
		CrossCrawler.Audio.StopWalk();
		if (!base.TurningAround)
		{
			StopMovement();
			if (_currentAttackLapse >= AttackCoolDown)
			{
				_currentAttackLapse = 0f;
				CrossCrawler.AnimatorInyector.Attack();
			}
		}
	}

	public override void Execution()
	{
		base.Execution();
		isExecuted = true;
		CrossCrawler.gameObject.layer = LayerMask.NameToLayer("Default");
		CrossCrawler.Audio.StopAll();
		CrossCrawler.AnimatorInyector.ResetToIdle();
		StopMovement();
		StopBehaviour();
		CrossCrawler.SpriteRenderer.enabled = false;
		Core.Logic.Penitent.Audio.PlaySimpleHitToEnemy();
		CrossCrawler.EntExecution.InstantiateExecution();
		if (CrossCrawler.EntExecution != null)
		{
			CrossCrawler.EntExecution.enabled = true;
		}
	}

	public override void Alive()
	{
		base.Alive();
		CrossCrawler.gameObject.layer = LayerMask.NameToLayer("Enemy");
		CrossCrawler.SpriteRenderer.enabled = true;
		CrossCrawler.Animator.Play("Idle");
		CrossCrawler.CurrentLife = CrossCrawler.Stats.Life.Base / 2f;
		StartBehaviour();
		if (CrossCrawler.EntExecution != null)
		{
			CrossCrawler.EntExecution.enabled = false;
		}
	}

	public override void StopMovement()
	{
		CrossCrawler.AnimatorInyector.Stop();
		CrossCrawler.Input.HorizontalInput = 0f;
	}

	private void OnDestroy()
	{
		Core.Logic.Penitent.OnDeath -= PlayerOnDeath;
	}

	public override void Wander()
	{
		throw new NotImplementedException();
	}

	public override void Chase(Transform targetPosition)
	{
		throw new NotImplementedException();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.magenta;
		Vector2 vector = (Vector2)base.transform.position + sightOffset;
		Vector2 vector2 = vector + (Vector2)(Quaternion.Euler(0f, 0f, visionAngle) * Vector2.right * sightDistance);
		Vector2 vector3 = vector + (Vector2)(Quaternion.Euler(0f, 0f, 0f - visionAngle) * Vector2.right * sightDistance);
		Gizmos.DrawLine(vector, vector2);
		Gizmos.DrawLine(vector, vector3);
	}
}
