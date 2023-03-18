using Framework.FrameworkCore;
using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Runner.AI;

public class RunnerChaseState : State
{
	protected float CurrentChasingImpasse;

	protected Runner Runner { get; set; }

	private bool IsTargetBehind
	{
		get
		{
			bool result = false;
			Vector3 position = Runner.Target.transform.position;
			EntityOrientation orientation = Runner.Status.Orientation;
			if (Runner.transform.position.x > position.x && orientation == EntityOrientation.Right)
			{
				result = true;
			}
			else if (Runner.transform.position.x < position.x && orientation == EntityOrientation.Left)
			{
				result = true;
			}
			return result;
		}
	}

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		Runner = machine.GetComponent<Runner>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		Runner.Behaviour.LookAtTarget(Runner.Target.transform.position);
		CurrentChasingImpasse = Runner.Behaviour.ChasingImpasse;
	}

	public override void Update()
	{
		base.Update();
		if (Runner.Behaviour.TurningAround || Runner.Behaviour.IsScreaming)
		{
			Runner.Behaviour.StopMovement();
		}
		else if (Runner.Behaviour.CanChase)
		{
			if (IsTargetBehind)
			{
				CurrentChasingImpasse -= Time.deltaTime;
				if (CurrentChasingImpasse < 0f)
				{
					Runner.Behaviour.StopMovement();
					Runner.Behaviour.LookAtTarget(Runner.Target.transform.position);
					CurrentChasingImpasse = Runner.Behaviour.ChasingImpasse;
				}
				else
				{
					Runner.Behaviour.Chase(Runner.Target.transform);
				}
			}
			else
			{
				Runner.Behaviour.Chase(Runner.Target.transform);
			}
		}
		else
		{
			Runner.Behaviour.StopMovement();
			Runner.Behaviour.LookAtTarget(Runner.Target.transform.position);
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}
}
