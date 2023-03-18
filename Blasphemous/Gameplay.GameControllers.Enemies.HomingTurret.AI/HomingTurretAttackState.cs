using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.HomingTurret.AI;

public class HomingTurretAttackState : State
{
	private HomingTurretBehaviour Behaviour;

	private float currentReadyAttackTime;

	private float currentCooldown;

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		Behaviour = machine.GetComponent<HomingTurretBehaviour>();
		Behaviour.Spawn();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		currentCooldown = Behaviour.AttackCooldown;
	}

	public override void Update()
	{
		base.Update();
		currentCooldown -= Time.deltaTime;
		if (!(currentCooldown > 0f))
		{
			if (currentReadyAttackTime == 0f)
			{
				Behaviour.ChargeAttack();
			}
			currentReadyAttackTime += Time.deltaTime;
			if (currentReadyAttackTime >= Behaviour.ReadyAttackTime)
			{
				currentReadyAttackTime = 0f;
				currentCooldown = Behaviour.AttackCooldown;
				Behaviour.ReleaseAttack();
			}
		}
	}
}
