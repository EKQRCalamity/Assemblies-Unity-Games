using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Enemies.WalkingTomb.AI;

public class WalkingTombAttackState : State
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
		WalkingTomb.Behaviour.StopMovement();
		WalkingTomb.Behaviour.Attack();
	}

	public override void Update()
	{
		base.Update();
		bool attack = WalkingTomb.Behaviour.TargetIsInFront && WalkingTomb.Behaviour.TargetIsOnAttackDistance;
		WalkingTomb.AnimatorInjector.Attack(attack);
		if (!WalkingTomb.IsAttacking)
		{
			WalkingTomb.StateMachine.SwitchState<WalkingTombWalkState>();
		}
		else
		{
			WalkingTomb.Behaviour.StopMovement();
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}
}
