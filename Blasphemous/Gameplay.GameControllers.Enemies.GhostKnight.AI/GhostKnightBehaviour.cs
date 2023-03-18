using System;
using System.Collections;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.GhostKnight.Animator;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.GhostKnight.AI;

public class GhostKnightBehaviour : EnemyBehaviour
{
	private const float YPosOffset = -1f;

	private GhostKnightAnimatorInyector _animatorInyector;

	private float _elapsedTimeBeforeStalk;

	private GhostKnight _ghostKnight;

	private bool _transitioning;

	public float AwaitTimeBeforeAttack = 0.2f;

	public float AwaitTimeBeforeStalk = 2f;

	public float DistanceToPlayerBeforeAttack = 5f;

	public float FallbackDisplacement = 2f;

	public float TimeBecomeInVisible = 0.5f;

	public float TimeBecomeVisible = 0.5f;

	public bool GotCloseToPlayer { get; set; }

	public bool GotRamp { get; set; }

	public Vector2 GetDamageFallbackPosition
	{
		get
		{
			Vector3 position = _ghostKnight.transform.position;
			float x = ((_ghostKnight.Status.Orientation != EntityOrientation.Left) ? (position.x - FallbackDisplacement) : (position.x + FallbackDisplacement));
			return new Vector2(x, _ghostKnight.transform.position.y);
		}
	}

	public override void OnAwake()
	{
		base.OnAwake();
		_ghostKnight = (GhostKnight)Entity;
		_animatorInyector = _ghostKnight.GetComponentInChildren<GhostKnightAnimatorInyector>();
	}

	public override void OnStart()
	{
		base.OnStart();
		MotionLerper motionLerper = _ghostKnight.MotionLerper;
		motionLerper.OnLerpStop = (Core.SimpleEvent)Delegate.Combine(motionLerper.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
		MotionLerper motionLerper2 = _ghostKnight.MotionLerper;
		motionLerper2.OnLerpStart = (Core.SimpleEvent)Delegate.Combine(motionLerper2.OnLerpStart, new Core.SimpleEvent(OnLerpStart));
		_ghostKnight.OnDeath += GhostKnightOnDeath;
		Core.Logic.Penitent.OnDeath += OnDeathPlayer;
	}

	public bool AwakeAttackBehaviour()
	{
		if (_ghostKnight == null)
		{
			return false;
		}
		bool result = false;
		if (_ghostKnight.DistanceToTarget <= _ghostKnight.ActivationRange)
		{
			_elapsedTimeBeforeStalk += Time.deltaTime;
			result = _elapsedTimeBeforeStalk >= AwaitTimeBeforeStalk;
		}
		else
		{
			_elapsedTimeBeforeStalk = 0f;
		}
		return result;
	}

	public void GetCloseToPlayer()
	{
		if (!(_ghostKnight == null) && !_transitioning && !GotCloseToPlayer)
		{
			GotCloseToPlayer = true;
			Vector3 position = _ghostKnight.Target.transform.position;
			float y = position.y + -1f;
			float horizontalRndPositionBeforeAttack = GetHorizontalRndPositionBeforeAttack(position);
			_ghostKnight.transform.position = new Vector2(horizontalRndPositionBeforeAttack, y);
			Appear(TimeBecomeVisible);
		}
	}

	public void Appear(float time)
	{
		if (!_transitioning)
		{
			_animatorInyector.Appear();
			_ghostKnight.SpriteRenderer.DOFade(1f, time).OnComplete(BecomeVisible).OnUpdate(OnTransition);
			_ghostKnight.Audio.Appear();
		}
	}

	public void Disappear(float time)
	{
		if (!_transitioning)
		{
			if (time <= 0f)
			{
				SetSpriteInvisible();
			}
			_ghostKnight.SpriteRenderer.DOFade(0f, time).OnComplete(BecomeInvisible).OnUpdate(OnTransition);
			_ghostKnight.Audio.Disappear();
		}
	}

	private void SetSpriteInvisible()
	{
		Color white = Color.white;
		white.a = 0f;
		_ghostKnight.SpriteRenderer.color = white;
	}

	private void BecomeVisible()
	{
		_transitioning = false;
		EnableDamageArea(isEnabled: true);
		_animatorInyector.AttackClue();
	}

	private void BecomeInvisible()
	{
		_transitioning = false;
		EnableDamageArea(isEnabled: false);
		BackToOrigin();
		_ghostKnight.Status.IsHurt = false;
		base.GotParry = false;
		_ghostKnight.Audio.StopAttack();
	}

	private void OnTransition()
	{
		EnableDamageArea(_ghostKnight.SpriteRenderer.color.a > 0.5f);
		_transitioning = true;
	}

	private void OnLerpStart()
	{
		if (!_ghostKnight.Status.Dead)
		{
			_ghostKnight.Audio.Attack();
			_ghostKnight.GhostSprites.EnableGhostTrail = true;
		}
	}

	private void OnLerpStop()
	{
		Entity.IsAttacking = false;
		if (!_ghostKnight.Status.Dead)
		{
			if (!base.GotParry)
			{
				_animatorInyector.AttackToIdle();
				Disappear(TimeBecomeInVisible);
			}
			_ghostKnight.GhostSprites.EnableGhostTrail = false;
		}
	}

	private void BackToOrigin()
	{
		_ghostKnight.transform.position = _ghostKnight.StartPoint;
		GotCloseToPlayer = false;
		GotRamp = false;
		_elapsedTimeBeforeStalk = 0f;
	}

	private float GetHorizontalRndPositionBeforeAttack(Vector3 targetPos)
	{
		float value = UnityEngine.Random.value;
		return (!(value >= 0.5f)) ? (targetPos.x - DistanceToPlayerBeforeAttack) : (targetPos.x + DistanceToPlayerBeforeAttack);
	}

	private void EnableDamageArea(bool isEnabled)
	{
		_ghostKnight.EntityDamageArea.DamageAreaCollider.enabled = isEnabled;
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
		if (!(_ghostKnight == null) && !GotRamp)
		{
			GotRamp = true;
			float num = ((_ghostKnight.Status.Orientation != EntityOrientation.Left) ? 1f : (-1f));
			MotionLerper motionLerper = _ghostKnight.MotionLerper;
			motionLerper.StartLerping(Vector2.right * num);
			Entity.IsAttacking = true;
		}
	}

	public override void StopMovement()
	{
		if (!base.GotParry && _ghostKnight.MotionLerper.IsLerping)
		{
			_ghostKnight.MotionLerper.StopLerping();
		}
	}

	public override void Damage()
	{
		_animatorInyector.ColorFlash(Color.red);
		_animatorInyector.EntityAnimator.speed = 1f;
		if (base.GotParry)
		{
			DamageDisplacement(FallbackDisplacement, 1f);
			_animatorInyector.Damage();
			_ghostKnight.Audio.StopAttack();
			if (!_ghostKnight.Status.Dead)
			{
				_ghostKnight.Audio.Damage();
			}
		}
		if (_ghostKnight.Status.Dead)
		{
			_ghostKnight.MotionLerper.StopLerping();
		}
		EnableDamageArea(isEnabled: false);
	}

	public void DamageDisplacement(float displacement, float time)
	{
		StopMovement();
		DOTween.Kill(base.transform);
		float num = ((_ghostKnight.Status.Orientation != 0) ? 1f : (-1f));
		_ghostKnight.transform.DOMoveX(base.transform.position.x + displacement * num, time).SetEase(Ease.OutSine);
	}

	public override void Parry()
	{
		base.Parry();
		base.GotParry = true;
		_ghostKnight.Status.IsHurt = true;
		DamageDisplacement(FallbackDisplacement * 0.5f, 3f);
		_ghostKnight.MotionLerper.StopLerping();
		_animatorInyector.ParryReaction();
		Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("Parry");
	}

	public override void Alive()
	{
		base.Alive();
		base.GotParry = false;
		_ghostKnight.Status.IsHurt = false;
		_ghostKnight.Animator.SetTrigger("HURT");
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (!_transitioning)
		{
			base.LookAtTarget(targetPos);
		}
	}

	private void GhostKnightOnDeath()
	{
		StopMovement();
		base.BehaviourTree.StopBehaviour();
		_ghostKnight.Audio.Death();
		_ghostKnight.StartCoroutine(CallAnimatorInyectorDeath());
	}

	private IEnumerator CallAnimatorInyectorDeath()
	{
		while (true)
		{
			_animatorInyector.Death();
			yield return new WaitForSeconds(1f);
		}
	}

	private void OnDeathPlayer()
	{
		base.BehaviourTree.StopBehaviour();
	}
}
