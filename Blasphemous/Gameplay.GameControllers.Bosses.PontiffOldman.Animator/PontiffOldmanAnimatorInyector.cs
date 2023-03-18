using System;
using Framework.Managers;
using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PontiffOldman.Animator;

public class PontiffOldmanAnimatorInyector : EnemyAnimatorInyector
{
	public UnityEngine.Animator castFXAnimator;

	public event Action<PontiffOldmanAnimatorInyector, Vector2> OnSpinProjectilePoint;

	public void AnimationEvent_OpenToAttacks()
	{
		PontiffOldmanBehaviour behaviour = (OwnerEntity as PontiffOldman).Behaviour;
		behaviour.SetRecovering(recovering: true);
	}

	public void AnimationEvent_CloseAttackWindow()
	{
		PontiffOldmanBehaviour behaviour = (OwnerEntity as PontiffOldman).Behaviour;
		behaviour.SetRecovering(recovering: false);
	}

	public void AnimationEvent_LightScreenShake()
	{
		Vector2 vector = ((OwnerEntity.Status.Orientation != 0) ? Vector2.left : Vector2.right);
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.15f, vector * 0.5f, 10, 0.01f, 0f, default(Vector3), 0.01f);
	}

	public void AnimationEvent_HeavyScreenShake()
	{
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.3f, Vector3.up * 3f, 60, 0.01f, 0f, default(Vector3), 0.01f);
	}

	public void AnimationEvent_ShortAttackDisplacement()
	{
		PontiffOldmanBehaviour behaviour = (OwnerEntity as PontiffOldman).Behaviour;
		behaviour.AttackDisplacement(0.4f, 2.5f);
	}

	public void AnimationEvent_MediumAttackDisplacement()
	{
		PontiffOldmanBehaviour behaviour = (OwnerEntity as PontiffOldman).Behaviour;
		behaviour.AttackDisplacement(0.5f, 5.5f);
	}

	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
			castFXAnimator.SetBool("CASTING", value: false);
		}
	}

	public void CancelAll()
	{
		Cast(cast: false);
		Vanish(dissapear: false);
		ComboMode(active: false);
		base.EntityAnimator.ResetTrigger("DEATH");
		base.EntityAnimator.ResetTrigger("HURT");
	}

	public void Cast(bool cast)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("CASTING", cast);
			castFXAnimator.SetBool("CASTING", cast);
		}
	}

	public void Vanish(bool dissapear)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger((!dissapear) ? "APPEAR" : "VANISH");
		}
	}

	public void ComboMode(bool active)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("COMBO_MODE", active);
		}
	}

	public void Hurt()
	{
		if (!(base.EntityAnimator == null))
		{
			if (base.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("HURT"))
			{
				Debug.Log("PLAY TRICK HURT");
				base.EntityAnimator.Play("HURT", 0, 0f);
			}
			else
			{
				base.EntityAnimator.SetTrigger("HURT");
			}
		}
	}
}
