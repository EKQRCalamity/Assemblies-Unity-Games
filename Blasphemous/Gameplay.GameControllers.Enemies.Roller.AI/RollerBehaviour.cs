using System;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Roller.AI;

public class RollerBehaviour : EnemyBehaviour
{
	[FoldoutGroup("Activation Settings", true, 0)]
	public float EscapeDistance;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float ShotDistance;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float ProjectileCoolDown = 1f;

	public float tunnelDetectorYOffset = 1f;

	public float tunnelDetectorRange = 2f;

	public float tunnelDetectorRaySeparation = 0.25f;

	private float _currentCoolDown;

	private bool isExecuted;

	public bool isInTunnel;

	public float MaxRollingTime = 3.5f;

	private float _currentRollingTime;

	public Roller Roller { get; private set; }

	[FoldoutGroup("Activation Settings", true, 0)]
	public float DistanceToTarget { get; private set; }

	public bool IsRolling { get; private set; }

	public bool IsEscaping { get; set; }

	public bool IsShooting { get; set; }

	public bool IsSetRandDir { get; private set; }

	public bool IsCharginAttack { get; set; }

	private float _rndDir { get; set; }

	private bool ReachMaxTimeRolling => _currentRollingTime >= MaxRollingTime;

	public override void OnStart()
	{
		base.OnStart();
		Roller = (Roller)Entity;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		isInTunnel = IsInTunnel();
		if (Roller == null || Roller.Status.Dead || isExecuted)
		{
			return;
		}
		DistanceToTarget = Vector2.Distance(Entity.transform.position, GetTarget().position);
		if (DistanceToTarget <= EscapeDistance && !IsEscaping && !IsCharginAttack && _currentCoolDown <= 0f)
		{
			IsEscaping = true;
		}
		if (IsEscaping)
		{
			if ((DistanceToTarget <= ShotDistance && !ReachMaxTimeRolling) || IsInTunnel())
			{
				if (!IsSetRandDir)
				{
					IsSetRandDir = true;
					_rndDir = GetRandomDir();
				}
				if (Roller.MotionChecker.HitsBlock || !Roller.MotionChecker.HitsFloor)
				{
					_rndDir *= -1f;
				}
				Roll(_rndDir);
			}
			else
			{
				StopMovement();
				IsSetRandDir = false;
				IsCharginAttack = true;
				IsEscaping = false;
				LookAtTarget(GetTarget().position);
			}
		}
		if (IsCharginAttack || VisualSensor.IsColliding())
		{
			_currentCoolDown += Time.deltaTime;
			if (_currentCoolDown >= ProjectileCoolDown)
			{
				_currentCoolDown = 0f;
				IsCharginAttack = false;
				Roller.AnimatorInjector.Attack();
			}
		}
		if (HearingSensor.IsColliding() && !IsEscaping)
		{
			LookAtTarget(GetTarget().position);
		}
	}

	private bool IsInTunnel()
	{
		Vector2 vector = (Vector2)base.transform.position + Vector2.up * tunnelDetectorYOffset;
		int mask = LayerMask.GetMask("Floor");
		RaycastHit2D raycastHit2D = Physics2D.Raycast(vector + Vector2.right * tunnelDetectorRaySeparation / 2f, Vector2.up, Roller.MotionChecker.RangeGroundDetection, mask);
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(vector + Vector2.left * tunnelDetectorRaySeparation / 2f, Vector2.up, Roller.MotionChecker.RangeGroundDetection, mask);
		return (((bool)raycastHit2D && raycastHit2D.normal == Vector2.down) || ((bool)raycastHit2D2 && raycastHit2D2.normal == Vector2.down)) && Roller.MotionChecker.HitsFloor;
	}

	private void OnDrawGizmosSelected()
	{
		Vector2 vector = (Vector2)base.transform.position + Vector2.up * tunnelDetectorYOffset;
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(vector + Vector2.right * tunnelDetectorRaySeparation / 2f, vector + Vector2.right * tunnelDetectorRaySeparation / 2f + Vector2.up * tunnelDetectorRange);
		Gizmos.DrawLine(vector + Vector2.left * tunnelDetectorRaySeparation / 2f, vector + Vector2.left * tunnelDetectorRaySeparation / 2f + Vector2.up * tunnelDetectorRange);
	}

	public void OnDisable()
	{
		if ((bool)Roller && (bool)Roller.Audio)
		{
			Roller.Audio.StopRolling();
		}
	}

	public void Roll(float dir)
	{
		if (!(Roller.Input == null))
		{
			_currentRollingTime += Time.deltaTime;
			Roller.Input.HorizontalInput = dir;
			IsRolling = true;
			Roller.SetOrientation((dir < 0f) ? EntityOrientation.Left : EntityOrientation.Right);
			Roller.AnimatorInjector.Rolling(isRolling: true);
			Roller.DamageCollider.enabled = false;
			Roller.Audio.PlayRolling();
			Roller.DamageByContact = false;
		}
	}

	public override void StopMovement()
	{
		IsRolling = false;
		_currentRollingTime = 0f;
		Roller.Input.HorizontalInput = 0f;
		Roller.Controller.PlatformCharacterPhysics.Velocity = Vector3.zero;
		Roller.AnimatorInjector.Rolling(isRolling: false);
		Roller.DamageCollider.enabled = true;
		Roller.DamageByContact = true;
		Roller.Audio.StopRolling();
	}

	private float GetRandomDir()
	{
		float f = UnityEngine.Random.Range(-1f, 1f);
		return Mathf.Sign(f);
	}

	public override void Execution()
	{
		base.Execution();
		isExecuted = true;
		Roller.gameObject.layer = LayerMask.NameToLayer("Default");
		Roller.Audio.StopRolling();
		Roller.Animator.Play("Idle");
		StopMovement();
		Roller.SpriteRenderer.enabled = false;
		Core.Logic.Penitent.Audio.PlaySimpleHitToEnemy();
		Roller.Attack.enabled = false;
		Roller.EntExecution.InstantiateExecution();
		if (Roller.EntExecution != null)
		{
			Roller.EntExecution.enabled = true;
		}
	}

	public override void Alive()
	{
		base.Alive();
		isExecuted = false;
		Roller.gameObject.layer = LayerMask.NameToLayer("Enemy");
		Roller.SpriteRenderer.enabled = true;
		Roller.Animator.Play("Idle");
		Roller.CurrentLife = Roller.Stats.Life.Base / 2f;
		Roller.Attack.enabled = true;
		StartBehaviour();
		if (Roller.EntExecution != null)
		{
			Roller.EntExecution.enabled = false;
		}
	}

	public override void Idle()
	{
	}

	public override void Wander()
	{
	}

	public override void Chase(Transform targetPosition)
	{
	}

	public override void Attack()
	{
		throw new NotImplementedException();
	}

	public override void Damage()
	{
		throw new NotImplementedException();
	}
}
