using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Enemies.LanceAngel.AI;

public class LanceAngelIdleState : State
{
	protected LanceAngel LanceAngel { get; set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		LanceAngel = machine.GetComponent<LanceAngel>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
	}

	public override void Update()
	{
		base.Update();
		LanceAngel.Behaviour.Floating();
		if (LanceAngel.Behaviour.CanSeeTarget)
		{
			LanceAngel.StateMachine.SwitchState<LanceAngelAttackState>();
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}
}
