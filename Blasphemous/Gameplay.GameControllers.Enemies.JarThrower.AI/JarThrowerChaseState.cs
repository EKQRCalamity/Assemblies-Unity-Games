using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.JarThrower.AI;

public class JarThrowerChaseState : State
{
	protected float CurrentTimeChasing;

	protected JarThrower JarThrower { get; set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		JarThrower = machine.GetComponent<JarThrower>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		JarThrower.Behaviour.StopMovement();
		ResetChasingTime();
	}

	public override void Update()
	{
		base.Update();
		JarThrower.Behaviour.StopMovement();
		Vector3 position = JarThrower.Target.transform.position;
		float num = Vector2.Distance(JarThrower.transform.position, position);
		if (JarThrower.Behaviour.TargetSeen)
		{
			ResetChasingTime();
		}
		else
		{
			CurrentTimeChasing -= Time.deltaTime;
			if (CurrentTimeChasing <= 0f)
			{
				JarThrower.StateMachine.SwitchState<JarThrowerWanderState>();
			}
		}
		if (num <= JarThrower.Behaviour.AttackDistance)
		{
			JarThrower.StateMachine.SwitchState<JarThrowerAttackState>();
		}
		if (JarThrower.Behaviour.TargetSeen && JarThrower.Controller.IsGrounded)
		{
			JarThrower.Behaviour.Jump(position);
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}

	private void ResetChasingTime()
	{
		if (CurrentTimeChasing < JarThrower.Behaviour.TimeChasing)
		{
			CurrentTimeChasing = JarThrower.Behaviour.TimeChasing;
		}
	}
}
