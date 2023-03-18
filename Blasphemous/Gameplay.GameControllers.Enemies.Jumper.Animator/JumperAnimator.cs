using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Jumper.Animator;

public class JumperAnimator : EnemyAnimatorInyector
{
	private float _currentTimeAscending;

	protected Jumper Jumper { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		Jumper = (Jumper)OwnerEntity;
	}

	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (base.EntityAnimator == null)
		{
			return;
		}
		if (base.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("JumpAscending"))
		{
			_currentTimeAscending += Time.deltaTime;
			if (_currentTimeAscending > 0.1f && Jumper.Controller.PlatformCharacterPhysics.VSpeed <= -0.1f)
			{
				_currentTimeAscending = 0f;
				base.EntityAnimator.Play("JumpMax");
			}
		}
		if (base.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("JumpDescending") && Jumper.Controller.IsGrounded)
		{
			base.EntityAnimator.Play("Landing");
		}
	}
}
