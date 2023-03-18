using System;
using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Camera;
using Gameplay.GameControllers.Enemies.BellGhost.AI;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.BellGhost.Animator;

public class BellGhostAnimatorInyector : EnemyAnimatorInyector
{
	private readonly int _turnAroundAnim = UnityEngine.Animator.StringToHash("TurnAround");

	private BellGhost _bellGhost;

	private CameraManager _cameraManager;

	public SpriteRenderer SpriteRenderer;

	private EnemyHealthBar _healthBar;

	public bool IsFading { get; private set; }

	public bool IsTurning
	{
		get
		{
			if (base.EntityAnimator == null)
			{
				base.EntityAnimator = OwnerEntity.Animator;
			}
			return OwnerEntity.gameObject.activeSelf && base.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("TurnAround");
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		_bellGhost = (BellGhost)OwnerEntity;
		BellGhostBehaviour component = _bellGhost.GetComponent<BellGhostBehaviour>();
		component.OnTurning = (Core.SimpleEvent)Delegate.Combine(component.OnTurning, new Core.SimpleEvent(OnTurning));
	}

	protected override void OnStart()
	{
		base.OnStart();
		base.EntityAnimator = OwnerEntity.Animator;
		_cameraManager = Core.Logic.CameraManager;
	}

	private void LateUpdate()
	{
		if (!_healthBar)
		{
			_healthBar = _bellGhost.GetComponentInChildren<EnemyHealthBar>();
		}
		else
		{
			_healthBar.transform.position = base.transform.position + (Vector3)_bellGhost.healthOffset;
		}
	}

	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}

	public void Hurt()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("HURT");
			_cameraManager.ProCamera2DShake.ShakeUsingPreset("SimpleHit");
		}
	}

	public void ShootAnimationEvent()
	{
		_bellGhost.Behaviour.Shoot();
	}

	public void Attack()
	{
		if (!(base.EntityAnimator == null) && OwnerEntity.gameObject.activeSelf)
		{
			base.EntityAnimator.SetTrigger("ATTACK");
		}
	}

	private void OnTurning()
	{
		if (base.EntityAnimator == null)
		{
			base.EntityAnimator = OwnerEntity.Animator;
		}
		if (!_bellGhost.Behaviour.TurningAround && OwnerEntity.gameObject.activeSelf)
		{
			base.EntityAnimator.Play(_turnAroundAnim);
		}
	}

	public void Appear(float time)
	{
		if (!IsFading)
		{
			Fade(1f, time);
		}
	}

	public void Disappear(float time)
	{
		if (!IsFading)
		{
			Fade(0f, time);
		}
	}

	public void Fade(float fadeValue, float time)
	{
		if (!(Math.Abs(SpriteRenderer.color.a - fadeValue) < Mathf.Epsilon))
		{
			SpriteRenderer.DOFade(fadeValue, time).OnStart(OnFadeStart).OnComplete(OnFadeCompleted);
		}
	}

	public void OnFadeStart()
	{
		IsFading = true;
	}

	public void OnFadeCompleted()
	{
		IsFading = false;
	}

	public void PlayFiringProjectile()
	{
		if (!(_bellGhost == null))
		{
			_bellGhost.Audio.PlayShoot();
		}
	}

	private void OnDisable()
	{
		Fade(0f, 0f);
	}

	private void OnDestroy()
	{
		BellGhostBehaviour component = _bellGhost.GetComponent<BellGhostBehaviour>();
		if ((bool)component)
		{
			component.OnTurning = (Core.SimpleEvent)Delegate.Remove(component.OnTurning, new Core.SimpleEvent(OnTurning));
		}
	}
}
