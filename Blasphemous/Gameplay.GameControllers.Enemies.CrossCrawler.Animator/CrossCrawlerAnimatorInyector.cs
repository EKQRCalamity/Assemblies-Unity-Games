using Gameplay.GameControllers.Enemies.CrossCrawler.IA;
using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.CrossCrawler.Animator;

public class CrossCrawlerAnimatorInyector : EnemyAnimatorInyector
{
	private CrossCrawler CrossCrawler;

	protected override void OnStart()
	{
		base.OnStart();
		CrossCrawler = GetComponentInParent<CrossCrawler>();
	}

	public void TurnAround()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("TURN");
		}
	}

	public void Walk()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool("WALK", value: true);
		}
	}

	public void Stop()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool("WALK", value: false);
		}
	}

	public void Attack()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("ATTACK");
		}
	}

	public void Death()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}

	public void AttackAnimationEvent()
	{
		CrossCrawler crossCrawler = (CrossCrawler)OwnerEntity;
		crossCrawler.Attack.CurrentWeaponAttack();
	}

	public void StartAttackAnimationEvent()
	{
		CrossCrawler.Audio.PlayAttack();
	}

	public void SetVulnerableTrue()
	{
		CrossCrawler.Behaviour.StartVulnerablePeriod();
	}

	public void WalkAnimationEvent()
	{
		CrossCrawler.Audio.PlayWalk();
	}

	public void TurnAnimationEvent()
	{
		CrossCrawler.Audio.PlayTurnAround();
	}

	public void PlayCrossCrawlerTurnMoveOne()
	{
		CrossCrawler.Audio.SetTurnMoveParam(1f);
	}

	public void PlayCrossCrawlerAtkMoveOne()
	{
		CrossCrawler.Audio.SetAttackMoveParam(1f);
	}

	public void PlayCrossCrawlerAtkMoveTwo()
	{
		CrossCrawler.Audio.SetAttackMoveParam(2f);
	}

	public void DeathAnimationEvent()
	{
		CrossCrawler.Audio.PlayDeath();
	}

	public void ResetToIdle()
	{
		base.EntityAnimator.ResetTrigger("ATTACK");
		base.EntityAnimator.Play("Idle");
	}

	public void ResetCoolDownAttack()
	{
		CrossCrawlerBehaviour componentInChildren = OwnerEntity.GetComponentInChildren<CrossCrawlerBehaviour>();
		if (componentInChildren != null)
		{
			componentInChildren.ResetCoolDown();
		}
	}
}
