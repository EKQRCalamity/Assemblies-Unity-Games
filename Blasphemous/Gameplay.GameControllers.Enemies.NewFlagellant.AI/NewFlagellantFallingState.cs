using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Enemies.NewFlagellant.AI;

public class NewFlagellantFallingState : State
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
		NewFlagellant.AnimatorInyector.Falling();
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		NewFlagellant.AnimatorInyector.Landing();
	}

	public override void Update()
	{
		base.Update();
		if (NewFlagellant.MotionChecker.HitsFloor)
		{
			NewFlagellant.StateMachine.SwitchState<NewFlagellantIdleState>();
		}
	}
}
