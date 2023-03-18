using System;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.ChasingHead.Animator;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ChasingHead.AI;

public class ChasingHeadBehaviour : EnemyBehaviour
{
	public Core.SimpleEvent OnHurtDisplacement;

	private ChasingHead _chasingHead;

	private bool _clockWise;

	private float _index;

	private Vector3 _velocity = Vector3.zero;

	[FoldoutGroup("Floating Motion", true, 0)]
	public float amplitudeX = 10f;

	private float _currentAmplitudeX;

	[FoldoutGroup("Floating Motion", true, 0)]
	public float amplitudeY = 5f;

	private float _currentAmplitudeY;

	[FoldoutGroup("Chasing Motion", true, 0)]
	public float ChasingElongation = 0.5f;

	[FoldoutGroup("Hurt Displacement", true, 0)]
	public float hurtDisplacement = 2f;

	[FoldoutGroup("Chasing Motion", true, 0)]
	public float Speed = 5f;

	[FoldoutGroup("Floating Motion", true, 0)]
	public float speedX = 1f;

	[FoldoutGroup("Floating Motion", true, 0)]
	public float speedY = 2f;

	public ChasingHeadAnimatorInyector AnimatorInyector { get; private set; }

	public override void OnStart()
	{
		base.OnStart();
		_chasingHead = (ChasingHead)Entity;
		AnimatorInyector = _chasingHead.GetComponentInChildren<ChasingHeadAnimatorInyector>();
		_clockWise = UnityEngine.Random.value > 0.5f;
		DOTween.To(delegate(float x)
		{
			_currentAmplitudeX = x;
		}, _currentAmplitudeX, amplitudeX, 1f);
		DOTween.To(delegate(float x)
		{
			_currentAmplitudeY = x;
		}, _currentAmplitudeY, amplitudeY, 1f);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		GameObject target = _chasingHead.Target;
		EntityOrientation orientation = ((target.transform.position.x <= _chasingHead.SpriteRenderer.transform.position.x) ? EntityOrientation.Left : EntityOrientation.Right);
		if (!_chasingHead.Status.Dead)
		{
			_chasingHead.SetOrientation(orientation);
		}
		if (!_chasingHead.Status.IsHurt)
		{
			Chase(target.transform);
			Floating(_clockWise);
		}
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
		if (!base.IsChasing)
		{
			base.IsChasing = true;
			_chasingHead.GhostSprites.EnableGhostTrail = true;
		}
		Vector3 target = new Vector3(targetPosition.position.x, targetPosition.position.y);
		base.transform.position = Vector3.SmoothDamp(base.transform.position, target, ref _velocity, ChasingElongation, Speed);
	}

	public override void Attack()
	{
		throw new NotImplementedException();
	}

	public override void Damage()
	{
		AnimatorInyector.Hurt();
		HurtDisplacement(_chasingHead.DamageArea.LastHit.AttackingEntity, null);
	}

	public void Death()
	{
		AnimatorInyector.Death();
		_chasingHead.DamageArea.DamageAreaCollider.enabled = false;
		HurtDisplacement(_chasingHead.DamageArea.LastHit.AttackingEntity, null);
	}

	public void HurtDisplacement(GameObject attackingEntity, TweenCallback onCompleteCallback)
	{
		float num = ((_chasingHead.Status.Orientation != 0) ? hurtDisplacement : (0f - hurtDisplacement));
		_chasingHead.transform.DOMoveX(base.transform.position.x + num, 0.3f).SetEase(Ease.OutSine).OnStart(OnStartDisplacement)
			.OnComplete(onCompleteCallback);
	}

	private void OnStartDisplacement()
	{
		if (OnHurtDisplacement != null)
		{
			OnHurtDisplacement();
		}
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}

	public void Floating(bool clockWise = true)
	{
		_index += Time.deltaTime;
		float x;
		float y;
		if (clockWise)
		{
			x = _currentAmplitudeX * Mathf.Sin(speedX * _index);
			y = Mathf.Cos(speedY * _index) * _currentAmplitudeY;
		}
		else
		{
			x = _currentAmplitudeX * Mathf.Cos(speedX * _index);
			y = Mathf.Sin(speedY * _index) * _currentAmplitudeY;
		}
		_chasingHead.SpriteRenderer.transform.localPosition = new Vector3(x, y, 0f);
	}
}
