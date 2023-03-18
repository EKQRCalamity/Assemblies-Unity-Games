using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Enemies.AshCharger.AI;

public class AshChargerAttackState : State
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
		AshCharger.Behaviour.StopMovement();
		AshCharger.Behaviour.Attack();
	}

	public override void Update()
	{
		base.Update();
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}
}
