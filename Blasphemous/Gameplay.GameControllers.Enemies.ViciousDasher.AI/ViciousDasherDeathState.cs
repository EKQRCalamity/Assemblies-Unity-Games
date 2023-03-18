using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Enemies.ViciousDasher.AI;

public class ViciousDasherDeathState : State
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
		ViciousDasher.AnimatorInjector.StopAttack();
		ViciousDasher.IsAttacking = false;
		base.Update();
	}
}
