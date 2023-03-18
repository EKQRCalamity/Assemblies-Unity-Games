using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.MeltedLady.IA;

public class MeltedLadyAttackState : State
{
	protected FloatingLady MeltedLady { get; private set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		MeltedLady = machine.GetComponent<FloatingLady>();
		MeltedLady.Behaviour.Teleport();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
	}

	public override void Update()
	{
		base.Update();
		if (MeltedLady.Behaviour.CanTeleport)
		{
			MeltedLady.Behaviour.TeleportCooldownLapse += Time.deltaTime;
			if (MeltedLady.Behaviour.TeleportCooldownLapse >= MeltedLady.Behaviour.TeleportCooldown)
			{
				MeltedLady.Behaviour.Teleport();
			}
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		MeltedLady.Behaviour.TeleportCooldownLapse = 0f;
	}
}
