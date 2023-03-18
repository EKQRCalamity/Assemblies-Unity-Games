using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.GhostKnight.Animator;

public class GhostKnightAnimatorInyector : EnemyAnimatorInyector
{
	private readonly int _appearAnim = UnityEngine.Animator.StringToHash("Appear");

	private readonly int _attackAnim = UnityEngine.Animator.StringToHash("Attack");

	private readonly int _attackClueAnim = UnityEngine.Animator.StringToHash("AttackClue");

	private readonly int _attackToIdleAnim = UnityEngine.Animator.StringToHash("AttackToIdle");

	private ColorFlash _colorFlash;

	private static readonly int DeathTrigger = UnityEngine.Animator.StringToHash("DEATH");

	private static readonly int Hurt = UnityEngine.Animator.StringToHash("HURT");

	protected override void OnStart()
	{
		base.OnStart();
		_colorFlash = OwnerEntity.Animator.GetComponent<ColorFlash>();
	}

	public void Damage()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger(Hurt);
		}
	}

	public void ParryReaction()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.Play("ParryReaction");
		}
	}

	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger(DeathTrigger);
		}
	}

	public void AttackClue()
	{
		if (!(base.EntityAnimator == null) && (!(OwnerEntity != null) || !OwnerEntity.Status.Dead))
		{
			base.EntityAnimator.Play(_attackClueAnim);
		}
	}

	public void Attack()
	{
		if (!(base.EntityAnimator == null) && (!(OwnerEntity != null) || !OwnerEntity.Status.Dead))
		{
			base.EntityAnimator.Play(_attackAnim);
		}
	}

	public void Appear()
	{
		if (!(base.EntityAnimator == null) && (!(OwnerEntity != null) || !OwnerEntity.Status.Dead))
		{
			base.EntityAnimator.Play(_appearAnim);
		}
	}

	public void AttackToIdle()
	{
		if (!(base.EntityAnimator == null) && (!(OwnerEntity != null) || !OwnerEntity.Status.Dead))
		{
			base.EntityAnimator.Play(_attackToIdleAnim);
		}
	}

	public void ColorFlash(Color color)
	{
		_colorFlash.FlashColor = color;
		_colorFlash.TriggerColorFlash();
	}
}
