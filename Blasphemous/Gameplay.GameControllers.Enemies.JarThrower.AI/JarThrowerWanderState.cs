using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.JarThrower.AI;

public class JarThrowerWanderState : State
{
	protected JarThrower JarThrower { get; set; }

	protected float CurrentHealingLapse { get; set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		JarThrower = machine.GetComponent<JarThrower>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		CurrentHealingLapse = JarThrower.Behaviour.HealingLapse;
	}

	public override void Update()
	{
		base.Update();
		CurrentHealingLapse -= Time.deltaTime;
		if (CurrentHealingLapse <= 0f)
		{
			JarThrower.Behaviour.Healing();
			ResetHealingTime();
		}
		if (JarThrower.Behaviour.IsHealing)
		{
			JarThrower.Behaviour.StopMovement();
		}
		else
		{
			JarThrower.Behaviour.Wander();
		}
		if (JarThrower.Behaviour.TargetSeen && !JarThrower.Behaviour.IsHealing && !JarThrower.Behaviour.TargetIsDead)
		{
			JarThrower.StateMachine.SwitchState<JarThrowerChaseState>();
		}
	}

	private void ResetHealingTime()
	{
		if (CurrentHealingLapse < JarThrower.Behaviour.HealingLapse)
		{
			CurrentHealingLapse = JarThrower.Behaviour.HealingLapse;
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		JarThrower.Behaviour.IsHealing = false;
	}
}
