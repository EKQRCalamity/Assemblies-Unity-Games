using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.WaxCrawler.Animator;

public class WaxCrawlerAnimatorInyector : EnemyAnimatorInyector
{
	private SpriteRenderer _spriteRenderer;

	private WaxCrawler _waxCrawler;

	public UnityEngine.Animator Animator => base.EntityAnimator;

	public bool EnableSpriteRenderer
	{
		get
		{
			return _spriteRenderer.enabled;
		}
		set
		{
			_spriteRenderer.enabled = value;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		_waxCrawler = (WaxCrawler)OwnerEntity;
		base.EntityAnimator = OwnerEntity.Animator;
		_spriteRenderer = _waxCrawler.SpriteRenderer;
	}

	public void AnimatorSpeed(float speed)
	{
		if (base.EntityAnimator != null)
		{
			base.EntityAnimator.speed = Mathf.Clamp01(speed);
		}
	}

	public void Dead()
	{
		if (base.EntityAnimator != null)
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}

	public void Hide()
	{
		if (base.EntityAnimator != null)
		{
			base.EntityAnimator.SetTrigger("HIDE");
		}
	}

	public void Appear()
	{
		if (base.EntityAnimator != null)
		{
			base.EntityAnimator.Play("Appear", 0, 0f);
		}
	}

	public void Hurt()
	{
		if (base.EntityAnimator != null)
		{
			base.EntityAnimator.SetTrigger("HURT");
		}
	}
}
