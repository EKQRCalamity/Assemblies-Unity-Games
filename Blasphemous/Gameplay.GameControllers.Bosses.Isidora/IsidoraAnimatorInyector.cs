using DG.Tweening;
using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Isidora;

public class IsidoraAnimatorInyector : EnemyAnimatorInyector
{
	private static readonly int T_Slash = Animator.StringToHash("SLASH");

	private static readonly int T_Rising = Animator.StringToHash("RISING");

	private static readonly int T_Orb = Animator.StringToHash("ORB_COLLECTED");

	private static readonly int B_Death = Animator.StringToHash("DEATH");

	private static readonly int B_Hold = Animator.StringToHash("HOLD");

	private static readonly int B_Twirl = Animator.StringToHash("TWIRL");

	private static readonly int B_Hide = Animator.StringToHash("HIDING");

	private static readonly int B_Cast = Animator.StringToHash("CASTING");

	private static readonly int B_Fade = Animator.StringToHash("FADE_SLASH");

	private bool fireScythe;

	public Animator mainAnimator;

	public Animator vfxAnimator;

	private SpriteRenderer vfxSprite;

	public bool resetAnimationSpeedFlag;

	private float minSpeed = 0.5f;

	private float maxSpeed = 2f;

	public float debugAnimatorSpeed;

	private Tween easeTween;

	private const float VANISH_ANIMATION_DURATION = 1.4f;

	internal void CheckFlagAnimationSpeed()
	{
		if (resetAnimationSpeedFlag)
		{
			ResetAnimationSpeed();
		}
	}

	public void SetFireScythe(bool on)
	{
		fireScythe = on;
		if (vfxSprite == null)
		{
			vfxSprite = vfxAnimator.GetComponentInChildren<SpriteRenderer>();
		}
		vfxSprite.enabled = on;
	}

	public bool IsScytheOnFire()
	{
		return fireScythe;
	}

	public void PlayDeath()
	{
		if ((bool)base.EntityAnimator)
		{
			ResetAll();
			SetDualBool(B_Death, value: true);
		}
	}

	public void StopDeath()
	{
		if ((bool)base.EntityAnimator)
		{
			SetDualBool(B_Death, value: false);
		}
	}

	public void PlaySlashAttack()
	{
		SetDualTrigger(T_Slash);
	}

	public void StopSlashAttack()
	{
		ResetDualTrigger(T_Slash);
	}

	public void PlayRisingSlash()
	{
		SetDualTrigger(T_Rising);
	}

	public void StopRisingSlash()
	{
		ResetDualTrigger(T_Rising);
	}

	public void SetTwirl(bool twirl)
	{
		if (!twirl)
		{
			resetAnimationSpeedFlag = !twirl;
		}
		SetDualBool(B_Twirl, twirl);
	}

	public void SetCasting(bool active)
	{
		SetDualBool(B_Cast, active);
	}

	public void SetHidden(bool hidden)
	{
		SetDualBool(B_Hide, hidden);
	}

	public void SetAttackAnticipation(bool hold)
	{
		SetDualBool(B_Hold, hold);
	}

	public void SetFadeSlash(bool fade)
	{
		SetDualBool(B_Fade, fade);
	}

	public void ResetAll()
	{
		ResetDualTrigger(T_Slash);
		ResetDualTrigger(T_Rising);
		SetDualBool(B_Cast, value: false);
		SetDualBool(B_Hide, value: false);
		SetDualBool(B_Hold, value: false);
		SetDualBool(B_Twirl, value: false);
		SetDualBool(B_Fade, value: false);
		ResetAnimationSpeed();
	}

	private void SetAnimatorSpeed(float s)
	{
		mainAnimator.speed = s;
		vfxAnimator.speed = s;
		debugAnimatorSpeed = s;
	}

	private float GetAnimatorSpeed()
	{
		return mainAnimator.speed;
	}

	public void Accelerate(float seconds)
	{
		resetAnimationSpeedFlag = false;
		EaseTwirl(GetAnimatorSpeed(), maxSpeed, seconds, Ease.InCubic);
	}

	public void Decelerate(float seconds)
	{
		resetAnimationSpeedFlag = false;
		EaseTwirl(GetAnimatorSpeed(), minSpeed, seconds, Ease.InCubic);
	}

	public void EaseTwirl(float minVal, float maxVal, float duration, Ease easingFunction)
	{
		if (easeTween != null)
		{
			easeTween.Kill();
		}
		SetAnimatorSpeed(minVal);
		easeTween = DOTween.To(GetAnimatorSpeed, SetAnimatorSpeed, maxVal, duration).SetEase(easingFunction);
	}

	public void ResetAnimationSpeed()
	{
		if (easeTween != null)
		{
			easeTween.Kill();
		}
		resetAnimationSpeedFlag = false;
		SetAnimatorSpeed(1f);
	}

	public float GetVanishAnimationDuration()
	{
		return 1.4f;
	}

	private void ResetDualTrigger(int name)
	{
		mainAnimator.ResetTrigger(name);
		vfxAnimator.ResetTrigger(name);
	}

	private void SetDualTrigger(int name)
	{
		mainAnimator.SetTrigger(name);
		vfxAnimator.SetTrigger(name);
	}

	private void SetDualBool(int name, bool value)
	{
		mainAnimator.SetBool(name, value);
		vfxAnimator.SetBool(name, value);
	}
}
