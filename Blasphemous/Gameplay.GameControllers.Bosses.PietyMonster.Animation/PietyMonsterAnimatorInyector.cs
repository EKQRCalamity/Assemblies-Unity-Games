using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Bosses.PietyMonster.Animation;

public class PietyMonsterAnimatorInyector : EnemyAnimatorInyector
{
	private PietyMonster _pietyMonster;

	protected override void OnStart()
	{
		base.OnStart();
		_pietyMonster = (PietyMonster)OwnerEntity;
	}

	public void Idle()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("WALK", value: false);
			base.EntityAnimator.SetBool("TURN_AROUND", value: false);
			ResetAttacks();
		}
	}

	public void Walk()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("WALK", value: true);
			base.EntityAnimator.SetBool("TURN_AROUND", value: false);
			base.EntityAnimator.ResetTrigger("STOMP_ATTACK");
		}
	}

	public void Stop()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("WALK", value: false);
		}
	}

	public void CanMove(int allow)
	{
		if (!(_pietyMonster == null))
		{
			_pietyMonster.CanMove = allow > 0;
		}
	}

	public void AvoidBarrierCollision(int allow)
	{
		if (!(_pietyMonster == null))
		{
			if (allow > 0)
			{
				_pietyMonster.BodyBarrier.EnableCollider();
			}
			else
			{
				_pietyMonster.BodyBarrier.DisableCollider();
			}
		}
	}

	public void StompAttack()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("WALK", value: false);
			base.EntityAnimator.SetBool("TURN_AROUND", value: false);
			base.EntityAnimator.SetTrigger("STOMP_ATTACK");
		}
	}

	public void ClawAttack()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("WALK", value: false);
			base.EntityAnimator.SetBool("TURN_AROUND", value: false);
			base.EntityAnimator.SetTrigger("SLASH_ATTACK");
		}
	}

	public void AreaAttack()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("WALK", value: false);
			base.EntityAnimator.SetBool("TURN_AROUND", value: false);
			base.EntityAnimator.SetTrigger("SMASH_ATTACK");
		}
	}

	public void SpitAttack()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("WALK", value: false);
			base.EntityAnimator.SetBool("TURN_AROUND", value: false);
			base.EntityAnimator.SetTrigger("SPIT_ATTACK");
			base.EntityAnimator.SetBool("SPITING", value: true);
		}
	}

	public void StopSpiting()
	{
		base.EntityAnimator.ResetTrigger("SPIT_ATTACK");
		base.EntityAnimator.SetBool("SPITING", value: false);
	}

	public void ResetAttacks()
	{
		base.EntityAnimator.ResetTrigger("SPIT_ATTACK");
		base.EntityAnimator.SetBool("SPITING", value: false);
		base.EntityAnimator.ResetTrigger("SMASH_ATTACK");
		base.EntityAnimator.ResetTrigger("STOMP_ATTACK");
		base.EntityAnimator.ResetTrigger("SLASH_ATTACK");
	}

	public void TurnAround()
	{
		if (!(base.EntityAnimator == null) && !_pietyMonster.EnemyBehaviour.TurningAround)
		{
			base.EntityAnimator.SetBool("WALK", value: false);
			base.EntityAnimator.SetBool("TURN_AROUND", value: true);
		}
	}

	public void Death()
	{
		ResetAttacks();
		base.EntityAnimator.SetTrigger("DEATH");
	}

	public void StopTurning()
	{
		base.EntityAnimator.SetBool("TURN_AROUND", value: false);
	}
}
