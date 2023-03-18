using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.GoldenCorpse.Animator;

public class GoldenCorpseAnimatorInyector : EnemyAnimatorInyector
{
	private int totalAnimationVariants = 2;

	public float origAnimationSpeed = 1f;

	public void Walk()
	{
		if ((bool)base.EntityAnimator && !base.EntityAnimator.GetBool("WALK"))
		{
			base.EntityAnimator.SetBool("WALK", value: true);
		}
	}

	public void StopWalk()
	{
		if ((bool)base.EntityAnimator && base.EntityAnimator.GetBool("WALK"))
		{
			base.EntityAnimator.SetBool("WALK", value: false);
		}
	}

	public void PlayAwaken()
	{
		base.EntityAnimator.SetBool("AWAKE", value: true);
	}

	public void PlaySleep()
	{
		base.EntityAnimator.SetBool("AWAKE", value: false);
	}

	public void TurnAround()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("TURN");
		}
	}

	public void Death()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}

	public void PlayRandomSleepAnim()
	{
		if ((bool)base.EntityAnimator)
		{
			int num = Random.Range(0, totalAnimationVariants);
			base.EntityAnimator.SetInteger("ID", num);
			base.EntityAnimator.Play("Sleep" + (num + 1));
		}
	}

	public void PlayCrouchedAnim()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetInteger("ID", 0);
			base.EntityAnimator.SetBool("AWAKE", value: false);
		}
	}

	public void PlayFreezeAnim()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetInteger("ID", 1);
			base.EntityAnimator.SetBool("AWAKE", value: false);
		}
	}

	public void FreezeAnimation()
	{
		if (base.EntityAnimator.speed > 0.1f)
		{
			origAnimationSpeed = base.EntityAnimator.speed;
			base.EntityAnimator.speed = 0.01f;
		}
	}

	public void UnFreezeAnimation()
	{
		if (base.EntityAnimator.speed < 0.1f)
		{
			base.EntityAnimator.speed = origAnimationSpeed;
		}
	}
}
