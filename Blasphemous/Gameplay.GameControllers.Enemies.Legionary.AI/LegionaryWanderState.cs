using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Enemies.Legionary.AI;

public class LegionaryWanderState : State
{
	protected Legionary Legionary { get; private set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		Legionary = machine.GetComponent<Legionary>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
	}

	public override void Update()
	{
		base.Update();
		Legionary.Behaviour.Wander();
		if (Legionary.Behaviour.CanSeeTarget)
		{
			Legionary.StateMachine.SwitchState<LegionaryAttackState>();
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}
}
