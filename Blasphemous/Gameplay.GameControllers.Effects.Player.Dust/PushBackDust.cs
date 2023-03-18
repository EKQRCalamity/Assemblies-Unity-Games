using Framework.FrameworkCore;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.Dust;

public class PushBackDust : Trait
{
	public Animator PushBackDustAnimator;

	private readonly int _pushBackDustAnimHash = Animator.StringToHash("PushBackDust");

	private SpriteRenderer _spriteRenderer;

	protected override void OnStart()
	{
		base.OnStart();
		if (PushBackDustAnimator == null)
		{
			Debug.LogError("An animator is needed!");
		}
		else
		{
			_spriteRenderer = PushBackDustAnimator.GetComponent<SpriteRenderer>();
		}
	}

	public void TriggerPushBackDust()
	{
		bool flag = Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.DEMAKE);
		if ((bool)PushBackDustAnimator && !flag)
		{
			FlipSpriteRenderer();
			PushBackDustAnimator.Play(_pushBackDustAnimHash, 0, 0f);
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
