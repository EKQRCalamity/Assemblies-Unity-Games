using System;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Crisanta.Animator;

public class CrisantaAnimatorInyector : EnemyAnimatorInyector
{
	public event Action<CrisantaAnimatorInyector, Vector2> OnSpinProjectilePoint;

	public void AnimationEvent_SlashProjectile()
	{
		if (this.OnSpinProjectilePoint != null)
		{
			this.OnSpinProjectilePoint(this, Vector2.right);
		}
	}

	public void AnimationEvent_UpwardsSlashStarts()
	{
		Debug.Log("-----UPWARDS SLASH ATTACK STARTS-----");
		CrisantaBehaviour behaviour = (OwnerEntity as Crisanta).Behaviour;
		behaviour.lightAttack.damageOnEnterArea = true;
		behaviour.lightAttack.CurrentWeaponAttack();
	}

	public void AnimationEvent_UpwardsSlashEnds()
	{
		Debug.Log("-----UPWARDS SLASH ATTACK ENDS-----");
		CrisantaBehaviour behaviour = (OwnerEntity as Crisanta).Behaviour;
		behaviour.lightAttack.damageOnEnterArea = false;
	}

	public void AnimationEvent_DownwardsSlashStarts()
	{
		Debug.Log("-----DOWNWARDS SLASH ATTACK STARTS-----");
		CrisantaBehaviour behaviour = (OwnerEntity as Crisanta).Behaviour;
		behaviour.heavyAttack.damageOnEnterArea = true;
		behaviour.heavyAttack.CurrentWeaponAttack();
	}

	public void AnimationEvent_DownwardsSlashEnds()
	{
		Debug.Log("-----DOWNWARDS SLASH ATTACK ENDS-----");
		CrisantaBehaviour behaviour = (OwnerEntity as Crisanta).Behaviour;
		behaviour.heavyAttack.damageOnEnterArea = false;
	}

	public void AnimationEvent_OpenToAttacks()
	{
		CrisantaBehaviour behaviour = (OwnerEntity as Crisanta).Behaviour;
		behaviour.SetRecovering(recovering: true);
	}

	public void AnimationEvent_CloseAttackWindow()
	{
		CrisantaBehaviour behaviour = (OwnerEntity as Crisanta).Behaviour;
		behaviour.SetRecovering(recovering: false);
	}

	public void AnimationEvent_SetShieldedOn()
	{
		Crisanta crisanta = OwnerEntity as Crisanta;
		if (!crisanta.IsCrisantaRedux)
		{
			(OwnerEntity as Enemy).IsGuarding = true;
		}
	}

	public void AnimationEvent_SetShieldedOff()
	{
		Crisanta crisanta = OwnerEntity as Crisanta;
		if (!crisanta.IsCrisantaRedux)
		{
			(OwnerEntity as Enemy).IsGuarding = false;
		}
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
		CrisantaBehaviour behaviour = (OwnerEntity as Crisanta).Behaviour;
		if (!behaviour.ignoreAnimDispl)
		{
			behaviour.AttackDisplacement(0.4f, 2.5f, trail: true);
		}
	}

	public void AnimationEvent_MediumAttackDisplacement()
	{
		CrisantaBehaviour behaviour = (OwnerEntity as Crisanta).Behaviour;
		if (!behaviour.ignoreAnimDispl)
		{
			behaviour.AttackDisplacementToPoint(Core.Logic.Penitent.transform.position, 1.2f, 30f, trail: true);
		}
	}

	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}

	public void CancelAll()
	{
		Guard(active: false);
		Blinkslash(blinkIn: false);
		ComboMode(active: false);
		AirAttack(active: false);
		base.EntityAnimator.ResetTrigger("DEATH");
		base.EntityAnimator.ResetTrigger("UPWARDS_SLASH");
		base.EntityAnimator.ResetTrigger("DOWNWARDS_SLASH");
		base.EntityAnimator.ResetTrigger("BLINKIN");
		base.EntityAnimator.ResetTrigger("BLINKOUT");
		base.EntityAnimator.ResetTrigger("PARRY");
		base.EntityAnimator.ResetTrigger("HURT");
		base.EntityAnimator.ResetTrigger("BACKFLIP");
		base.EntityAnimator.ResetTrigger("CHANGE_STANCE");
	}

	public void Backflip()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("BACKFLIP");
		}
	}

	public void DeathBackflip()
	{
		if (!(base.EntityAnimator == null))
		{
			Backflip();
			Death();
		}
	}

	public void BackflipLand()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("LAND");
		}
	}

	public void AirAttack(bool active)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("AIR_ATTACK", active);
		}
	}

	public void ChangeStance()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("CHANGE_STANCE");
		}
	}

	public void Guard(bool active)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("GUARD", active);
		}
	}

	public void Unseal()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("UNSEAL");
		}
	}

	public void UpwardsSlash()
	{
		if (!(base.EntityAnimator == null))
		{
			Debug.Log("Setting trigger Upward Slash");
			base.EntityAnimator.SetTrigger("UPWARDS_SLASH");
		}
	}

	public void DownwardsSlash()
	{
		if (!(base.EntityAnimator == null))
		{
			Debug.Log("Setting trigger Downward Slash");
			base.EntityAnimator.SetTrigger("DOWNWARDS_SLASH");
		}
	}

	public void BlinkIn()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("BLINKIN");
		}
	}

	public void BlinkOut()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("BLINKOUT");
		}
	}

	public void Blinkslash(bool blinkIn)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("BLINKSLASH", blinkIn);
		}
	}

	public void ComboMode(bool active)
	{
		Debug.Log("Setting combo mode " + active);
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("COMBO_MODE", active);
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

	public void SetStayKneeling(bool active)
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("STAY_KNEELING", active);
		}
	}

	public void PlayHurtRecovery()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.Play("HURT_RECOVERY", 0, 0f);
		}
	}
}
