using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Menina.AI;

public class MeninaStateBackwards : State
{
	private float _currentAwaitBeforeBackwards;

	protected Menina Menina { get; set; }

	protected MeninaBehaviour Behaviour { get; set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		if (Menina == null)
		{
			Menina = machine.GetComponent<Menina>();
		}
		if (Behaviour == null)
		{
			Behaviour = Menina.GetComponent<MeninaBehaviour>();
		}
	}

	public override void Update()
	{
		base.Update();
		if (Behaviour.IsAwake)
		{
			CheckIfPlayerIsClose();
			_currentAwaitBeforeBackwards += Time.deltaTime;
			float num = Vector2.Distance(Menina.StartPosition, Menina.transform.position);
			if (_currentAwaitBeforeBackwards >= Behaviour.AwaitBeforeBackward && num > 1f)
			{
				Behaviour.StepBackwards();
			}
			else
			{
				Behaviour.StopMovement();
			}
		}
	}

	private void CheckIfPlayerIsClose()
	{
		if (Behaviour.PlayerSeen)
		{
			Menina.StateMachine.SwitchState<MeninaStateAttack>();
		}
		else if (Behaviour.PlayerHeard)
		{
			Menina.StateMachine.SwitchState<MeninaStateChase>();
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		_currentAwaitBeforeBackwards = 0f;
	}
}
