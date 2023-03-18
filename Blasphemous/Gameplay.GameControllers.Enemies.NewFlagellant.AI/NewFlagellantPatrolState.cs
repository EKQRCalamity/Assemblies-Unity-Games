using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.NewFlagellant.AI;

public class NewFlagellantPatrolState : State
{
	private const float minPatrolTime = 4f;

	private const float maxPatrolTime = 7f;

	private float _counter;

	public NewFlagellant NewFlagellant { get; private set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		NewFlagellant = machine.GetComponent<NewFlagellant>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		NewFlagellant.MotionLerper.StopLerping();
		_counter = Random.Range(4f, 7f);
	}

	public override void OnStateExit()
	{
		NewFlagellant.AnimatorInyector.Walk(walk: false);
		base.OnStateExit();
	}

	public override void Update()
	{
		base.Update();
		NewFlagellant.NewFlagellantBehaviour.Patrol();
		if (NewFlagellant.NewFlagellantBehaviour.CanSeeTarget())
		{
			if (NewFlagellant.NewFlagellantBehaviour.CanReachPlayer())
			{
				NewFlagellant.NewFlagellantBehaviour.ResetRememberTime();
				NewFlagellant.NewFlagellantBehaviour.LookAtTarget(NewFlagellant.Target.transform.position);
				NewFlagellant.StateMachine.SwitchState<NewFlagellantChaseState>();
			}
			else
			{
				NewFlagellant.NewFlagellantBehaviour.ResetRememberTime();
				NewFlagellant.NewFlagellantBehaviour.LookAtTarget(NewFlagellant.Target.transform.position);
				NewFlagellant.StateMachine.SwitchState<NewFlagellantIdleState>();
			}
		}
		_counter -= Time.deltaTime;
		if (_counter <= 0f)
		{
			NewFlagellant.StateMachine.SwitchState<NewFlagellantIdleState>();
		}
	}
}
