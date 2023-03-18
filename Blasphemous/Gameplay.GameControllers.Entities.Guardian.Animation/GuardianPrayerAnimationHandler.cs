using UnityEngine;

namespace Gameplay.GameControllers.Entities.Guardian.Animation;

public class GuardianPrayerAnimationHandler : MonoBehaviour
{
	public static readonly int AttackTrigger = Animator.StringToHash("ATTACK");

	public static readonly int GuardTrigger = Animator.StringToHash("GUARD");

	public static readonly int VanishTrigger = Animator.StringToHash("VANISH");

	public static readonly int TurnTrigger = Animator.StringToHash("TURN");

	public static readonly int AwaitingTrigger = Animator.StringToHash("AWAITING");

	public GuardianPrayer GuardianPrayer { get; set; }

	public void SetAnimatorTrigger(int animatorTrigger)
	{
		if (!(GuardianPrayer == null))
		{
			GuardianPrayer.Animator.SetTrigger(animatorTrigger);
		}
	}

	public void Appearing()
	{
		SetInvisible();
		GuardianPrayer.Animator.Play("Appearing");
	}

	public void WeaponAttack()
	{
		if (!(GuardianPrayer == null))
		{
			GuardianPrayer.Attack.CurrentWeaponAttack();
		}
	}

	public void Vanish()
	{
		GuardianPrayer.gameObject.SetActive(value: false);
	}

	public void SetInvisible(int visible = 1)
	{
		float num = Mathf.Clamp01(visible);
		GuardianPrayer.SpriteRenderer.enabled = num > 0f;
	}
}
