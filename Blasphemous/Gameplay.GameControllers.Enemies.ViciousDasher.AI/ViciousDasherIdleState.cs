using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Enemies.ViciousDasher.AI;

public class ViciousDasherIdleState : State
{
	public ViciousDasher ViciousDasher { get; private set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		ViciousDasher = machine.GetComponent<ViciousDasher>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		ViciousDasher.MotionLerper.StopLerping();
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}

	public override void Update()
	{
		base.Update();
		ViciousDasher.AnimatorInjector.StopAttack();
		if (ViciousDasher.IsTargetVisible && !ViciousDasher.ViciousDasherBehaviour.CanBeExecuted)
		{
			ViciousDasher.ViciousDasherBehaviour.LookAtTarget(ViciousDasher.Target.transform.position);
			ViciousDasher.StateMachine.SwitchState<ViciousDasherAttackState>();
		}
	}
}
