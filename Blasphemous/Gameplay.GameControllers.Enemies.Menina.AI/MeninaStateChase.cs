using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Menina.AI;

public class MeninaStateChase : State
{
	public float ChasingTime;

	public float MinDistanceChasing;

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

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		Behaviour.CurrentChasingTime = 0f;
	}

	public override void Update()
	{
		base.Update();
		Behaviour.IsAwake = true;
		Behaviour.CurrentChasingTime += Time.deltaTime;
		CheckStateTransition();
		if (!Menina.MotionChecker.HitsFloor)
		{
			Behaviour.StopMovement();
		}
		else if (Behaviour.CurrentChasingTime >= ChasingTime && Menina.DistanceToTarget > MinDistanceChasing)
		{
			Behaviour.Chase(Menina.Target.transform);
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		Behaviour.StopMovement();
	}

	private void CheckStateTransition()
	{
		if (Behaviour.PlayerSeen)
		{
			Menina.StateMachine.SwitchState<MeninaStateAttack>();
		}
		else if (!Behaviour.PlayerHeard)
		{
			Menina.StateMachine.SwitchState<MeninaStateBackwards>();
		}
	}
}
