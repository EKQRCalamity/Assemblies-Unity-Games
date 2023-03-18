using System;
using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Camera;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.PontiffHusk.Animator;

public class PontiffHuskAnimatorInyector : EnemyAnimatorInyector
{
	private PontiffHuskMelee _PontiffHuskMelee;

	private PontiffHuskRanged _PontiffHuskRanged;

	private CameraManager _cameraManager;

	public SpriteRenderer SpriteRenderer;

	private EnemyHealthBar _healthBar;

	public bool IsFading { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		if (OwnerEntity is PontiffHuskRanged)
		{
			_PontiffHuskRanged = (PontiffHuskRanged)OwnerEntity;
		}
		if (OwnerEntity is PontiffHuskMelee)
		{
			_PontiffHuskMelee = (PontiffHuskMelee)OwnerEntity;
		}
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
			_healthBar = OwnerEntity.GetComponentInChildren<EnemyHealthBar>();
			return;
		}
		Enemy enemy = (Enemy)OwnerEntity;
		_healthBar.transform.position = base.transform.position + (Vector3)enemy.healthOffset;
	}

	public void Death(bool cut)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
			if (cut)
			{
				base.EntityAnimator.SetTrigger("CUT");
			}
		}
	}

	public void PlayAppear()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("APPEAR");
		}
	}

	public void Hurt()
	{
	}

	public void Ramming()
	{
		_PontiffHuskMelee.Behaviour.Ramming();
	}

	public void ShootAnimationEvent()
	{
		_PontiffHuskRanged.Behaviour.Shoot();
	}

	public void Attack()
	{
		if (!(base.EntityAnimator == null) && OwnerEntity.gameObject.activeSelf)
		{
			base.EntityAnimator.SetBool("DASH", value: true);
		}
	}

	public void StartShootingMines()
	{
		if (!(base.EntityAnimator == null) && OwnerEntity.gameObject.activeSelf)
		{
			base.EntityAnimator.SetBool("SHOOT", value: true);
		}
	}

	public void StopShootingMines()
	{
		if (!(base.EntityAnimator == null) && OwnerEntity.gameObject.activeSelf)
		{
			base.EntityAnimator.SetBool("SHOOT", value: false);
		}
	}

	public void StopAttack()
	{
		if (!(base.EntityAnimator == null) && OwnerEntity.gameObject.activeSelf && _PontiffHuskMelee != null)
		{
			base.EntityAnimator.SetBool("DASH", value: false);
		}
	}

	public void Appear(float time)
	{
		if (!IsFading)
		{
			Fade(1f, time);
			PlayAppear();
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
			Tweener t = SpriteRenderer.DOFade(fadeValue, time).OnStart(OnFadeStart).SetEase(Ease.InCirc);
			if (fadeValue > 0.5f)
			{
				t.OnComplete(OnFadeInCompleted);
			}
			else
			{
				t.OnComplete(OnFadeOutCompleted);
			}
		}
	}

	public void OnFadeStart()
	{
		IsFading = true;
	}

	public void OnFadeInCompleted()
	{
		IsFading = false;
		if (_PontiffHuskRanged != null)
		{
			_PontiffHuskRanged.Behaviour.IsAppear = true;
		}
		if (_PontiffHuskMelee != null)
		{
			_PontiffHuskMelee.Behaviour.IsAppear = true;
		}
	}

	public void OnFadeOutCompleted()
	{
		IsFading = false;
		if (_PontiffHuskRanged != null)
		{
			_PontiffHuskRanged.Behaviour.IsAppear = false;
		}
		if (_PontiffHuskMelee != null)
		{
			_PontiffHuskMelee.Behaviour.IsAppear = false;
		}
	}

	public void PlayFiringProjectile()
	{
		if (!(_PontiffHuskRanged == null))
		{
			_PontiffHuskRanged.Audio.PlayShoot();
		}
	}

	private void OnDisable()
	{
		Fade(0f, 0f);
	}
}
