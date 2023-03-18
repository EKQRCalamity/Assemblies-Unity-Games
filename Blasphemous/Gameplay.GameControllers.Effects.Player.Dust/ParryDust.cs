using Framework.FrameworkCore;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.Dust;

public class ParryDust : Trait
{
	public Animator ParryDustAnimator;

	private readonly int _parryDustAnim = Animator.StringToHash("ParryDust");

	private SpriteRenderer _spriteRenderer;

	protected override void OnStart()
	{
		base.OnStart();
		if (ParryDustAnimator == null)
		{
			Debug.LogError("An animator is needed!");
		}
		else
		{
			_spriteRenderer = ParryDustAnimator.GetComponent<SpriteRenderer>();
		}
	}

	public void TriggerParryDust()
	{
		if (!(ParryDustAnimator == null))
		{
			FlipSpriteRenderer();
			ParryDustAnimator.Play(_parryDustAnim, 0, 0f);
		}
	}

	public void FlipSpriteRenderer()
	{
		if (!(base.EntityOwner == null))
		{
			if (base.EntityOwner.Status.Orientation == EntityOrientation.Left && !_spriteRenderer.flipX)
			{
				_spriteRenderer.flipX = true;
			}
			else if (base.EntityOwner.Status.Orientation == EntityOrientation.Right && _spriteRenderer.flipX)
			{
				_spriteRenderer.flipX = false;
			}
		}
	}
}
