using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Enemies.NewFlagellant.AI;

public class NewFlagellantChaseState : State
{
	public NewFlagellant NewFlagellant { get; private set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		NewFlagellant = machine.GetComponent<NewFlagellant>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		NewFlagellant.AnimatorInyector.Run(run: true);
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		NewFlagellant.AnimatorInyector.Run(run: false);
	}

	public override void Update()
	{
		base.Update();
		if (!NewFlagellant.NewFlagellantBehaviour.CanAttack())
		{
			NewFlagellant.AnimatorInyector.Run(run: false);
			return;
		}
		if (NewFlagellant.DistanceToTarget > 10f || !NewFlagellant.NewFlagellantBehaviour.StillRemembersPlayer())
		{
			NewFlagellant.StateMachine.SwitchState<NewFlagellantPatrolState>();
			return;
		}
		NewFlagellant.NewFlagellantBehaviour.LookAtTarget(NewFlagellant.Target.transform.position);
		NewFlagellant.NewFlagellantBehaviour.Chase();
		if (NewFlagellant.NewFlagellantBehaviour.IsTargetInsideAttackRange())
		{
			NewFlagellant.StateMachine.SwitchState<NewFlagellantAttackState>();
		}
		NewFlagellant.NewFlagellantBehaviour.CheckFall();
	}
}
