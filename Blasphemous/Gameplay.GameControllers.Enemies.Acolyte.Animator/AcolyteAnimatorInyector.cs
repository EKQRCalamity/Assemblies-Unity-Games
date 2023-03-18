using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Acolyte.Animator;

public class AcolyteAnimatorInyector : EnemyAnimatorInyector
{
	private bool _chasing;

	public void Idle()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("WALK", value: false);
			base.EntityAnimator.SetBool("ATTACK", value: false);
			base.EntityAnimator.SetBool("CHASING", value: false);
		}
	}

	public void Wander()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("WALK", value: true);
			base.EntityAnimator.SetBool("ATTACK", value: false);
			base.EntityAnimator.SetBool("CHASING", value: false);
		}
	}

	public void Chase(Transform targetPosition)
	{
		if (!(base.EntityAnimator == null))
		{
			if (!_chasing)
			{
				_chasing = true;
			}
			base.EntityAnimator.SetBool("CHASING", value: true);
			base.EntityAnimator.SetBool("ATTACK", value: false);
			base.EntityAnimator.SetBool("WALK", value: false);
		}
	}

	public void Attack(bool isGrounded = true)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("WALK", value: false);
			base.EntityAnimator.SetBool("CHASING", value: false);
			base.EntityAnimator.SetBool("ATTACK", isGrounded);
		}
	}

	public void Damage()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("HURT");
		}
	}

	public void ParryReaction()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("CHASING", value: false);
			base.EntityAnimator.SetBool("ATTACK", value: false);
			base.EntityAnimator.SetBool("WALK", value: false);
			base.EntityAnimator.Play("ParryReaction");
		}
	}

	public void Overthrow()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("OVERTHROW");
		}
	}

	public void StopChasing()
	{
		if (!(base.EntityAnimator == null) && _chasing)
		{
			_chasing = !_chasing;
			base.EntityAnimator.SetBool("CHASING", value: false);
			base.EntityAnimator.Play("Stop Running");
		}
	}

	public void Dead()
	{
		base.EntityAnimator.SetTrigger("DEATH");
	}

	public void Grounded(bool isGrounded)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("GROUNDED", isGrounded);
		}
	}
}
