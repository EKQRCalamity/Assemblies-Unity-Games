using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.Flagellant.Animator;

public class FlagellantAnimatorInyector : EnemyAnimatorInyector
{
	public void Grounded(bool isGrounded)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("GROUNDED", isGrounded);
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

	public void Attack(bool isGrounded)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("WALK", value: false);
			base.EntityAnimator.SetBool("CHASING", value: false);
			base.EntityAnimator.SetBool("ATTACK", isGrounded);
		}
	}

	public void Chase(bool isGrounded)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("ATTACK", value: false);
			base.EntityAnimator.SetBool("WALK", value: false);
			base.EntityAnimator.SetBool("CHASING", value: true);
			base.EntityAnimator.SetBool("GROUNDED", isGrounded);
		}
	}

	public void Idle()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("WALK", value: false);
			base.EntityAnimator.SetBool("ATTACK", value: false);
			base.EntityAnimator.SetBool("CHASING", value: false);
		}
	}

	public void Fall()
	{
		if (!(base.EntityAnimator == null))
		{
		}
	}

	public void DamageImpact()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("HURT");
		}
	}

	public void Hurt()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("CHASING", value: false);
			base.EntityAnimator.SetBool("WALK", value: false);
		}
	}

	public void ParryReaction()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("ATTACK", value: false);
			base.EntityAnimator.SetBool("CHASING", value: false);
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

	public void Stunt()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.Play("Stunt");
		}
	}

	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.Play("Death");
		}
	}
}
