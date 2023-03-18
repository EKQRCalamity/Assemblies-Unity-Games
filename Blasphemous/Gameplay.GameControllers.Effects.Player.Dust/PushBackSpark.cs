using Framework.FrameworkCore;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.Dust;

public class PushBackSpark : Trait
{
	public Animator PushBackSparkAnimator;

	private readonly int _pushBackSparkAnimHash = Animator.StringToHash("PushBackSparks");

	private SpriteRenderer _spriteRenderer;

	protected override void OnStart()
	{
		base.OnStart();
		if (PushBackSparkAnimator == null)
		{
			Debug.LogError("An animator is needed!");
		}
		else
		{
			_spriteRenderer = PushBackSparkAnimator.GetComponent<SpriteRenderer>();
		}
	}

	public void TriggerPushBackSparks()
	{
		if (!(PushBackSparkAnimator == null))
		{
			FlipSpriteRenderer();
			PushBackSparkAnimator.Play(_pushBackSparkAnimHash, 0, 0f);
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
