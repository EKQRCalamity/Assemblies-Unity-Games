using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Enemies.NewFlagellant.AI;

public class NewFlagellantHurtState : State
{
	private float hurtRecoveryTime = 1.2f;

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
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}

	public override void Update()
	{
		base.Update();
		NewFlagellant.NewFlagellantBehaviour.UpdateHurtTime();
		if (NewFlagellant.NewFlagellantBehaviour._currentHurtTime > hurtRecoveryTime)
		{
			NewFlagellant.StateMachine.SwitchState<NewFlagellantIdleState>();
		}
		NewFlagellant.NewFlagellantBehaviour.CheckFall();
	}

	public override void Destroy()
	{
		base.Destroy();
	}
}
