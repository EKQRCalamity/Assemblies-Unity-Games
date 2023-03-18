using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Enemies.AshCharger.AI;

public class AshChargeWaitingState : State
{
	protected AshCharger AshCharger { get; set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		AshCharger = machine.GetComponent<AshCharger>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
	}

	public override void Update()
	{
		base.Update();
		AshCharger.Behaviour.Wander();
		if (!AshCharger.Behaviour.TargetIsDead && AshCharger.Behaviour.TargetIsInFront)
		{
			AshCharger.StateMachine.SwitchState<AshChargerAttackState>();
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}
}
