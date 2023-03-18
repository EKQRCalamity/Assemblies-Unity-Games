using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Enemies.WalkingTomb.AI;

public class WalkingTombWalkState : State
{
	protected WalkingTomb WalkingTomb { get; set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		WalkingTomb = machine.GetComponent<WalkingTomb>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
	}

	public override void Update()
	{
		base.Update();
		WalkingTomb.Behaviour.Wander();
		if (!WalkingTomb.Behaviour.TargetIsDead && WalkingTomb.Behaviour.TargetIsInFront && WalkingTomb.Behaviour.TargetIsOnAttackDistance)
		{
			WalkingTomb.StateMachine.SwitchState<WalkingTombAttackState>();
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}
}
