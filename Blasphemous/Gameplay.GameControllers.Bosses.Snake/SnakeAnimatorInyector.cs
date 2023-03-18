using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Snake;

public class SnakeAnimatorInyector : EnemyAnimatorInyector
{
	public enum OPEN_MOUTH_INTENTIONS
	{
		TO_BITE,
		TO_CAST,
		TO_SUMMON_SPIKES
	}

	private static readonly int B_Bite = Animator.StringToHash("BITE");

	private static readonly int B_Cast = Animator.StringToHash("CAST");

	private static readonly int B_SummonSpikes = Animator.StringToHash("SUMMON_SPIKES");

	private static readonly int B_Close = Animator.StringToHash("CLOSE");

	private static readonly int B_DeathBite = Animator.StringToHash("DEATH_BITE");

	private static readonly int T_CounterBite = Animator.StringToHash("COUNTER_BITE");

	private static readonly int T_Death = Animator.StringToHash("DEATH");

	public Animator LeftHeadAnimator;

	public Animator RightHeadAnimator;

	private SnakeBackgroundAnimator bgAnimator;

	public void PlayOpenMouth(OPEN_MOUTH_INTENTIONS intention)
	{
		switch (intention)
		{
		case OPEN_MOUTH_INTENTIONS.TO_BITE:
			SetHeadDualBool(B_Bite, value: true);
			break;
		case OPEN_MOUTH_INTENTIONS.TO_CAST:
			SetHeadDualBool(B_Cast, value: true);
			break;
		case OPEN_MOUTH_INTENTIONS.TO_SUMMON_SPIKES:
			SetHeadDualBool(B_SummonSpikes, value: true);
			break;
		}
	}

	private void GetBackgroundAnimationReference()
	{
		if (bgAnimator == null)
		{
			bgAnimator = Object.FindObjectOfType<SnakeBackgroundAnimator>();
		}
	}

	public void BackgroundAnimationSetActive(bool active)
	{
		GetBackgroundAnimationReference();
		if (bgAnimator != null)
		{
			bgAnimator.Activate(active);
		}
	}

	public void BackgroundAnimationSetSpeed(float spd, float seconds = 0.5f)
	{
		GetBackgroundAnimationReference();
		if (bgAnimator != null)
		{
			bgAnimator.LerpSpeed(spd, seconds);
		}
	}

	public void StopOpenMouth()
	{
		SetHeadDualBool(B_Bite, value: false);
		SetHeadDualBool(B_Cast, value: false);
		SetHeadDualBool(B_SummonSpikes, value: false);
	}

	public void PlayCloseMouth()
	{
		SetHeadDualBool(B_Close, value: true);
	}

	public void StopCloseMouth()
	{
		SetHeadDualBool(B_Close, value: false);
	}

	public void SetCounterBite()
	{
		SetHeadDualTrigger(T_CounterBite);
	}

	public void PlayDeath()
	{
		SetHeadDualTrigger(T_Death);
	}

	public void PlayDeathBite()
	{
		SetHeadDualBool(B_DeathBite, value: true);
	}

	public void StopDeathBite()
	{
		SetHeadDualBool(B_DeathBite, value: false);
	}

	public void ResetAll()
	{
		SetHeadDualBool(B_Bite, value: false);
		SetHeadDualBool(B_Close, value: false);
		SetHeadDualBool(B_SummonSpikes, value: false);
		SetHeadDualBool(B_Cast, value: false);
		ResetHeadDualTrigger(T_CounterBite);
	}

	private void ResetHeadDualTrigger(int name)
	{
		LeftHeadAnimator.ResetTrigger(name);
		RightHeadAnimator.ResetTrigger(name);
	}

	private void SetHeadDualTrigger(int name)
	{
		LeftHeadAnimator.SetTrigger(name);
		RightHeadAnimator.SetTrigger(name);
	}

	private void SetHeadDualBool(int name, bool value)
	{
		LeftHeadAnimator.SetBool(name, value);
		RightHeadAnimator.SetBool(name, value);
	}

	public void SetHeadDualSpeed(float speed)
	{
		LeftHeadAnimator.speed = speed;
		RightHeadAnimator.speed = speed;
	}
}
