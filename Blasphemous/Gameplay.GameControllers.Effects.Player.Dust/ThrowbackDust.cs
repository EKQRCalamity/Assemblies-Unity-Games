using Framework.FrameworkCore;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.Dust;

public class ThrowbackDust : Trait
{
	public Animator ThrowbackDustAnimator;

	private readonly int _throwbackDustAnim = Animator.StringToHash("ThrowbackDust");

	private SpriteRenderer _spriteRenderer;

	protected override void OnStart()
	{
		base.OnStart();
		if (ThrowbackDustAnimator == null)
		{
			Debug.LogError("An animator is needed!");
		}
		else
		{
			_spriteRenderer = ThrowbackDustAnimator.GetComponent<SpriteRenderer>();
		}
	}

	public void TriggerThrowbackDust()
	{
		if (!(ThrowbackDustAnimator == null))
		{
			FlipSpriteRenderer();
			ThrowbackDustAnimator.Play(_throwbackDustAnim, 0, 0f);
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
