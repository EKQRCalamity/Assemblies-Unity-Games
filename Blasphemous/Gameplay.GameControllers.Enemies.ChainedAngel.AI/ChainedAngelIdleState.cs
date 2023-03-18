using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Enemies.ChainedAngel.AI;

public class ChainedAngelIdleState : State
{
	private ChainedAngel ChainedAngel { get; set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		ChainedAngel = machine.GetComponent<ChainedAngel>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		ChainedAngel.FloatingMotion.IsFloating = true;
	}

	public override void Update()
	{
		base.Update();
		ChainedAngel.Behaviour.LookAtTarget(ChainedAngel.Target.transform.position);
		if (ChainedAngel.Behaviour.CanSeeTarget)
		{
			ChainedAngel.StateMachine.SwitchState<ChainedAngelAttackState>();
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}
}
