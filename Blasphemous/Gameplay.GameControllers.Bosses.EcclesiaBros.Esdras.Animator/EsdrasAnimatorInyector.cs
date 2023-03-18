using System;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.EcclesiaBros.Esdras.Animator;

public class EsdrasAnimatorInyector : EnemyAnimatorInyector
{
	public event Action<EsdrasAnimatorInyector> OnHeavyAttackLightningSummon;

	public event Action<EsdrasAnimatorInyector, Vector2> OnSpinProjectilePoint;

	public void AnimationEvent_SpinProjectilePointRight()
	{
		if (this.OnSpinProjectilePoint != null)
		{
			this.OnSpinProjectilePoint(this, Vector2.right);
		}
	}

	public void AnimationEvent_SpinProjectilePointLeft()
	{
		if (this.OnSpinProjectilePoint != null)
		{
			this.OnSpinProjectilePoint(this, Vector2.left);
		}
	}

	public void AnimationEvent_LightAttack()
	{
		EsdrasBehaviour behaviour = (OwnerEntity as Esdras).Behaviour;
		behaviour.lightAttack.CurrentWeaponAttack();
	}

	public void AnimationEvent_HeavyAttack()
	{
		EsdrasBehaviour behaviour = (OwnerEntity as Esdras).Behaviour;
		behaviour.heavyAttack.CurrentWeaponAttack();
	}

	public void AnimationEvent_SpinAttackStarts()
	{
		EsdrasBehaviour behaviour = (OwnerEntity as Esdras).Behaviour;
		behaviour.singleSpinAttack.CurrentWeaponAttack();
		behaviour.singleSpinAttack.DealsDamage = true;
	}

	public void AnimationEvent_SpinAttackEnds()
	{
		EsdrasBehaviour behaviour = (OwnerEntity as Esdras).Behaviour;
		behaviour.singleSpinAttack.DealsDamage = false;
	}

	public void AnimationEvent_OpenToAttacks()
	{
		EsdrasBehaviour behaviour = (OwnerEntity as Esdras).Behaviour;
		behaviour.SetRecovering(recovering: true);
	}

	public void AnimationEvent_CloseAttackWindow()
	{
		EsdrasBehaviour behaviour = (OwnerEntity as Esdras).Behaviour;
		Debug.Log("CLOSING ATTACK WINDOW");
		behaviour.SetRecovering(recovering: false);
	}

	public void AnimationEvent_SetShieldedOn()
	{
		if (OwnerEntity is Enemy)
		{
			(OwnerEntity as Enemy).IsGuarding = true;
		}
	}

	public void AnimationEvent_SetShieldedOff()
	{
		if (OwnerEntity is Enemy)
		{
			(OwnerEntity as Enemy).IsGuarding = false;
		}
	}

	public void AnimationEvent_LightScreenShake()
	{
		Vector2 vector = ((OwnerEntity.Status.Orientation != 0) ? Vector2.left : Vector2.right);
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.15f, vector * 0.5f, 10, 0.01f, 0f, default(Vector3), 0.01f);
	}

	public void AnimationEvent_HeavySecondScreenShake()
	{
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.3f, Vector3.up * 3f, 60, 0.01f, 0f, default(Vector3), 0.01f);
	}

	public void AnimationEvent_HeavyFirstScreenShake()
	{
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.7f, Vector3.down * 2f, 30, 0.01f, 0f, default(Vector3), 0.001f);
	}

	public void AnimationEvent_AttackDisplacement()
	{
		EsdrasBehaviour behaviour = (OwnerEntity as Esdras).Behaviour;
		behaviour.AttackDisplacement();
	}

	public void AnimationEvent_SpinAttackDisplacement()
	{
		EsdrasBehaviour behaviour = (OwnerEntity as Esdras).Behaviour;
		behaviour.AttackDisplacement(1.5f, 10f, trail: false);
	}

	public void AnimationEvent_TauntImpact()
	{
		if (OwnerEntity is Esdras)
		{
			EsdrasBehaviour behaviour = (OwnerEntity as Esdras).Behaviour;
			behaviour.CounterImpactShockwave();
		}
	}

	public void SummonLightning()
	{
		if (this.OnHeavyAttackLightningSummon != null)
		{
			this.OnHeavyAttackLightningSummon(this);
		}
	}

	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}

	public void SpinAttack()
	{
		Debug.Log("AnimIn: SPIN ATK");
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("SPIN_ATTACK");
		}
	}

	public void SpinLoop(bool active)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("SPIN_LOOP", active);
		}
	}

	public void LightAttack()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("LIGHT_ATTACK");
		}
	}

	public void HeavyAttack()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("HEAVY_ATTACK");
		}
	}

	public void Taunt()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("TAUNT");
		}
	}

	public void Parry()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("PARRY");
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

	public void Run(bool run)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("RUNNING", run);
		}
	}
}
