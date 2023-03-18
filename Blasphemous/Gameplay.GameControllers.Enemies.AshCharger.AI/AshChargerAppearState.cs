using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.AshCharger.AI;

public class AshChargerAppearState : State
{
	private float appearSeconds = 0.5f;

	private float _counter;

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
		_counter += Time.deltaTime;
		if (_counter > appearSeconds)
		{
			AshCharger.StateMachine.SwitchState<AshChargeWaitingState>();
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}
}
