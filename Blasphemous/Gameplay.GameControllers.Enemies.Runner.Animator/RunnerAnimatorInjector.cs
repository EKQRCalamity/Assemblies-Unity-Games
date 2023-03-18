using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Runner.Animator;

public class RunnerAnimatorInjector : EnemyAnimatorInyector
{
	private static readonly int Around = UnityEngine.Animator.StringToHash("TURN_AROUND");

	private static readonly int Turning = UnityEngine.Animator.StringToHash("TURNING");

	private static readonly int Death1 = UnityEngine.Animator.StringToHash("DEATH");

	private static readonly int Run1 = UnityEngine.Animator.StringToHash("RUN");

	public Runner Runner { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		Runner = (Runner)OwnerEntity;
	}

	public void Run(bool run)
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetBool(Run1, run);
		}
	}

	public void Scream()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.Play("Scream");
		}
	}

	public void TurnAround()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger(Around);
			base.EntityAnimator.SetBool(Turning, value: true);
		}
	}

	public void Death()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger(Death1);
			SetPlayerOrientation();
		}
	}

	public void Dispose()
	{
		OwnerEntity.gameObject.SetActive(value: false);
	}

	private void SetPlayerOrientation()
	{
		Vector3 position = Core.Logic.Penitent.transform.position;
		if (Runner.Status.Orientation == EntityOrientation.Right)
		{
			if (position.x <= OwnerEntity.transform.position.x)
			{
				OwnerEntity.SetOrientation(EntityOrientation.Left);
			}
		}
		else if (position.x > OwnerEntity.transform.position.x)
		{
			OwnerEntity.SetOrientation(EntityOrientation.Right);
		}
	}
}
