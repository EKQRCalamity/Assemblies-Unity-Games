using UnityEngine;

namespace Gameplay.GameControllers.Entities.MiriamPortal.Animation;

public class MiriamPortalPrayerAnimationHandler : MonoBehaviour
{
	public static readonly int AttackTrigger = Animator.StringToHash("ATTACK");

	public static readonly int VanishTrigger = Animator.StringToHash("VANISH");

	public static readonly int TurnTrigger = Animator.StringToHash("TURN");

	public MiriamPortalPrayer MiriamPortalPrayer { get; set; }

	public void SetAnimatorTrigger(int animatorTrigger)
	{
		if (!(MiriamPortalPrayer == null))
		{
			MiriamPortalPrayer.Animator.SetTrigger(animatorTrigger);
		}
	}

	public void Appearing()
	{
		if (!(MiriamPortalPrayer == null))
		{
			SetInvisible();
			MiriamPortalPrayer.Animator.Play("Appearing");
		}
	}

	public void StopAnimator()
	{
		if (!(MiriamPortalPrayer == null))
		{
			MiriamPortalPrayer.Animator.speed = 0.1f;
		}
	}

	public void RestartAnimator()
	{
		if (!(MiriamPortalPrayer == null))
		{
			MiriamPortalPrayer.Animator.speed = 1f;
		}
	}

	public void WeaponAttack()
	{
		if (!(MiriamPortalPrayer == null))
		{
			MiriamPortalPrayer.Attack.DealsDamage = true;
		}
	}

	public void WeaponAttackFinished()
	{
		if (!(MiriamPortalPrayer == null))
		{
			MiriamPortalPrayer.Attack.DealsDamage = false;
		}
	}

	public void Vanish()
	{
		if (!MiriamPortalPrayer.Behaviour.ReappearFlag)
		{
			MiriamPortalPrayer.gameObject.SetActive(value: false);
		}
	}

	public void SetInvisible(int visible = 1)
	{
		float num = Mathf.Clamp01(visible);
		MiriamPortalPrayer.SpriteRenderer.enabled = num > 0f;
	}
}
