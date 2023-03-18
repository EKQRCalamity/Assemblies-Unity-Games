using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Enemies.LanceAngel.AI;

public class LanceAngelParryState : State
{
	private float _defaultDisplacement;

	private float _defaultLapse;

	protected LanceAngel LanceAngel { get; set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		LanceAngel = machine.GetComponent<LanceAngel>();
		_defaultDisplacement = LanceAngel.MotionLerper.distanceToMove;
		_defaultLapse = LanceAngel.MotionLerper.TimeTakenDuringLerp;
		LanceAngel.OnDamaged += OnDamaged;
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		LanceAngel.MotionLerper.distanceToMove = 0.25f;
		LanceAngel.MotionLerper.TimeTakenDuringLerp = 0.5f;
		LanceAngel.Behaviour.HurtDisplacement();
	}

	public override void Update()
	{
		base.Update();
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		LanceAngel.MotionLerper.distanceToMove = _defaultDisplacement;
		LanceAngel.MotionLerper.TimeTakenDuringLerp = _defaultLapse;
	}

	private void OnDamaged()
	{
		LanceAngel.StateMachine.SwitchState<LanceAngelAttackState>();
	}

	public override void Destroy()
	{
		base.Destroy();
		if ((bool)LanceAngel)
		{
			LanceAngel.OnDamaged -= OnDamaged;
		}
	}
}
