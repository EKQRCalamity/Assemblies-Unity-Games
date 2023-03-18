using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.NewFlagellant.AI;

public class NewFlagellantIdleState : State
{
	private const float minIdleTime = 1f;

	private const float maxIdleTime = 2f;

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
		NewFlagellant.NewFlagellantBehaviour.StopMovement();
		_counter = Random.Range(1f, 2f);
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}

	public override void Update()
	{
		base.Update();
		NewFlagellant.AnimatorInyector.StopAttack();
		if (NewFlagellant.NewFlagellantBehaviour.CanSeeTarget() && NewFlagellant.NewFlagellantBehaviour.CanReachPlayer())
		{
			NewFlagellant.NewFlagellantBehaviour.ResetRememberTime();
			NewFlagellant.NewFlagellantBehaviour.LookAtTarget(NewFlagellant.Target.transform.position);
			NewFlagellant.StateMachine.SwitchState<NewFlagellantChaseState>();
		}
		_counter -= Time.deltaTime;
		if (_counter <= 0f)
		{
			NewFlagellant.StateMachine.SwitchState<NewFlagellantPatrolState>();
		}
		NewFlagellant.NewFlagellantBehaviour.CheckFall();
	}
}
