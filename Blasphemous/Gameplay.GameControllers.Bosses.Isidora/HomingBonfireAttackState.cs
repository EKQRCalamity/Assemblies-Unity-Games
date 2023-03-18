using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Isidora;

public class HomingBonfireAttackState : State
{
	private HomingBonfireBehaviour Behaviour;

	private float currentCooldown;

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		Behaviour = machine.GetComponent<HomingBonfireBehaviour>();
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
		if (currentCooldown < 0f)
		{
			currentCooldown = Behaviour.AttackCooldown;
			Behaviour.FireProjectile();
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}
}
