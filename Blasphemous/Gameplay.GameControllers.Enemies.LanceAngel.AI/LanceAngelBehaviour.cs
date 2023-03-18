using System;
using System.Collections;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.LanceAngel.AI;

public class LanceAngelBehaviour : EnemyBehaviour
{
	public Core.SimpleEvent OnParry;

	private float _index;

	public float StartHeightPosition = 4f;

	[FoldoutGroup("Floating Motion", true, 0)]
	public float AmplitudeX = 10f;

	[FoldoutGroup("Floating Motion", true, 0)]
	public float AmplitudeY = 5f;

	[FoldoutGroup("Floating Motion", true, 0)]
	public float SpeedX = 1f;

	[FoldoutGroup("Floating Motion", true, 0)]
	public float SpeedY = 2f;

	[FoldoutGroup("Attack Motion", true, 0)]
	public float AttackSpeed = 7f;

	private float _minHeight = 1f;

	[FoldoutGroup("Chasing", true, 0)]
	public float ChasingElongation = 0.5f;

	[FoldoutGroup("Chasing", true, 0)]
	public float ChasingSpeed = 5f;

	[FoldoutGroup("Attack", true, 0)]
	public float DistanceAttack = 5f;

	[FoldoutGroup("Attack", true, 0)]
	public AnimationCurve RepositionCurve;

	[FoldoutGroup("Attack", true, 0)]
	public float RepositionTime = 1.5f;

	[FoldoutGroup("Attack", true, 0)]
	public float AttackCooldown = 2f;

	[FoldoutGroup("Attack", true, 0)]
	public Vector2 TargetOffset;

	[FoldoutGroup("Attack", true, 0)]
	public Vector2 RandomTargetOffset;

	[FoldoutGroup("Attack", true, 0)]
	public LayerMask targetFloorMask;

	private float _currentAmplitudeY;

	private float _currentAmplitudeX;

	public Vector3 PathOrigin;

	private Vector3 _velocity = Vector3.zero;

	private RaycastHit2D[] results;

	protected LanceAngel LanceAngel { get; set; }

	public bool IsRepositioning { get; set; }

	public bool CanSeeTarget => LanceAngel.VisionCone.CanSeeTarget(LanceAngel.Target.transform, "Penitent");

	private float GetHeight => Physics2D.Raycast(base.transform.position, -Vector2.up, 10f, BlockLayerMask).distance;

	public override void OnStart()
	{
		base.OnStart();
		results = new RaycastHit2D[1];
		LanceAngel = (LanceAngel)Entity;
		TargetOffset += new Vector2(UnityEngine.Random.Range(0f - RandomTargetOffset.x, RandomTargetOffset.x), UnityEngine.Random.Range(0f - RandomTargetOffset.x, RandomTargetOffset.x));
		if ((bool)LanceAngel.DashAttack)
		{
			LanceAngel.DashAttack.SetDamage((int)LanceAngel.Stats.Strength.Final);
		}
		DOTween.To(delegate(float x)
		{
			_currentAmplitudeX = x;
		}, _currentAmplitudeX, AmplitudeX, 1f);
		DOTween.To(delegate(float x)
		{
			_currentAmplitudeY = x;
		}, _currentAmplitudeY, AmplitudeY, 1f);
		LanceAngel.StateMachine.SwitchState<LanceAngelIdleState>();
		MotionLerper motionLerper = LanceAngel.MotionLerper;
		motionLerper.OnLerpStart = (Core.SimpleEvent)Delegate.Combine(motionLerper.OnLerpStart, new Core.SimpleEvent(OnLerpStart));
		MotionLerper motionLerper2 = LanceAngel.MotionLerper;
		motionLerper2.OnLerpStop = (Core.SimpleEvent)Delegate.Combine(motionLerper2.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
		LanceAngel.DashAttack.OnDashAdvancedEvent += OnDashUpdatedEvent;
		LanceAngel.DashAttack.OnDashFinishedEvent += OnDashFinishedEvent;
		Entity.OnDeath += OnDeath;
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (base.IsAttacking)
		{
			return;
		}
		if (targetPos.x > LanceAngel.transform.position.x)
		{
			if (LanceAngel.Status.Orientation != 0)
			{
				LanceAngel.SetOrientation(EntityOrientation.Right);
			}
		}
		else if (LanceAngel.Status.Orientation != EntityOrientation.Left)
		{
			LanceAngel.SetOrientation(EntityOrientation.Left);
		}
	}

	public void Reposition(Action callBack = null)
	{
		StartCoroutine(TargetReposition(callBack, 0.9f));
	}

	private IEnumerator TargetReposition(Action callBack, float callbackPercentage = 1f)
	{
		float currentRepositionTime = 0f;
		float repositionTime = LanceAngel.Behaviour.RepositionTime;
		IsRepositioning = true;
		PathOrigin = LanceAngel.Spline.transform.position;
		SetRepositionDirection();
		bool callbackLaunched = false;
		while (currentRepositionTime <= repositionTime && GetHeight > _minHeight)
		{
			float percentage = currentRepositionTime / repositionTime;
			float value = LanceAngel.Behaviour.RepositionCurve.Evaluate(percentage);
			Vector3 nextPoint = LanceAngel.Spline.GetPoint(value);
			LanceAngel.transform.position = nextPoint;
			currentRepositionTime += Time.deltaTime;
			if (callbackPercentage > percentage && !callbackLaunched)
			{
				callBack?.Invoke();
				callbackLaunched = true;
			}
			yield return null;
		}
		callBack?.Invoke();
	}

	public void Dash()
	{
		Vector3 position = LanceAngel.Target.transform.position;
		LanceAngel.DashAttack.dashDuration = 7f / AttackSpeed;
		LanceAngel.DashAttack.Dash(LanceAngel.transform, GetAttackDirection(position), 7f);
	}

	public void Chasing(Vector2 targetPosition)
	{
		Vector3 chasingTargetPosition = GetChasingTargetPosition(targetPosition);
		chasingTargetPosition.y += LanceAngel.Behaviour.StartHeightPosition;
		LanceAngel.transform.position = Vector3.SmoothDamp(LanceAngel.transform.position, chasingTargetPosition, ref _velocity, LanceAngel.Behaviour.ChasingElongation, LanceAngel.Behaviour.ChasingSpeed * 0.5f);
	}

	private Vector2 GetPointBelow(Vector2 p, float maxDistance = 2f)
	{
		LayerMask layerMask = targetFloorMask;
		if (Physics2D.RaycastNonAlloc(p, Vector2.down, results, maxDistance, layerMask) > 0)
		{
			return results[0].point;
		}
		GizmoExtensions.DrawDebugCross(p, Color.red, 10f);
		return p;
	}

	private Vector3 GetChasingTargetPosition(Vector3 targetPosition)
	{
		targetPosition = GetPointBelow(targetPosition, 3f);
		if (LanceAngel.Status.Orientation == EntityOrientation.Left)
		{
			targetPosition.x += LanceAngel.Behaviour.TargetOffset.x;
		}
		else
		{
			targetPosition.x -= LanceAngel.Behaviour.TargetOffset.x;
		}
		return targetPosition;
	}

	public void Floating()
	{
		_index += Time.deltaTime;
		float x = _currentAmplitudeX * Mathf.Sin(SpeedX * _index);
		float y = Mathf.Cos(SpeedY * _index) * _currentAmplitudeY;
		LanceAngel.SpriteRenderer.transform.localPosition = new Vector3(x, y, 0f);
	}

	private void SetRepositionDirection()
	{
		Vector3 one = Vector3.one;
		Vector3 one2 = Vector3.one;
		one2.x = -1f;
		LanceAngel.Spline.transform.localScale = ((LanceAngel.Status.Orientation != EntityOrientation.Left) ? one : one2);
	}

	private Vector3 GetTargetPosition(Vector2 targetOffset)
	{
		Vector3 position = LanceAngel.Target.transform.position;
		position.y += targetOffset.y;
		if (LanceAngel.Status.Orientation == EntityOrientation.Right)
		{
			position.x += targetOffset.x;
		}
		else
		{
			position.x -= targetOffset.x;
		}
		return new Vector2(position.x, position.y);
	}

	private Vector2 GetAttackDirection(Vector2 target)
	{
		Vector2 right = Vector2.right;
		if (LanceAngel.Target.transform.position.x <= LanceAngel.transform.position.x)
		{
			right *= -1f;
		}
		return right;
	}

	public void HurtDisplacement()
	{
		Vector3 dir = ((LanceAngel.Status.Orientation != EntityOrientation.Left) ? (-Vector3.right) : Vector3.right);
		LanceAngel.MotionLerper.StartLerping(dir);
	}

	public override void Damage()
	{
		HurtDisplacement();
	}

	public override void Parry()
	{
		base.Parry();
		LanceAngel.DashAttack.StopDash(LanceAngel.transform);
		if (OnParry != null)
		{
			OnParry();
		}
	}

	private void OnLerpStop()
	{
		LanceAngel.GhostSprites.EnableGhostTrail = false;
	}

	private void OnLerpStart()
	{
		LanceAngel.GhostSprites.EnableGhostTrail = true;
	}

	public override void Idle()
	{
		throw new NotImplementedException();
	}

	public override void Wander()
	{
		throw new NotImplementedException();
	}

	public override void Chase(Transform targetPosition)
	{
		throw new NotImplementedException();
	}

	public override void Attack()
	{
		throw new NotImplementedException();
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}

	private void OnDeath()
	{
		Entity.OnDeath -= OnDeath;
		LanceAngel.StateMachine.enabled = false;
	}

	private void OnDashFinishedEvent()
	{
		LanceAngel.IsAttacking = false;
	}

	private void OnDashUpdatedEvent(float value)
	{
		LanceAngel.IsAttacking = true;
	}

	private void OnDestroy()
	{
		if (!(LanceAngel == null))
		{
			MotionLerper motionLerper = LanceAngel.MotionLerper;
			motionLerper.OnLerpStart = (Core.SimpleEvent)Delegate.Remove(motionLerper.OnLerpStart, new Core.SimpleEvent(OnLerpStart));
			MotionLerper motionLerper2 = LanceAngel.MotionLerper;
			motionLerper2.OnLerpStop = (Core.SimpleEvent)Delegate.Remove(motionLerper2.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
			LanceAngel.DashAttack.OnDashAdvancedEvent -= OnDashUpdatedEvent;
			LanceAngel.DashAttack.OnDashFinishedEvent -= OnDashFinishedEvent;
		}
	}
}
