using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Isidora;

public class HomingBonfireChargeIsidoraState : State
{
	private HomingBonfireBehaviour Behaviour;

	private float timeToMaxRate;

	private AnimationCurve chargeRate;

	private float currentTime;

	private float currentCooldown;

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		Behaviour = machine.GetComponent<HomingBonfireBehaviour>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		timeToMaxRate = Behaviour.TimeToMaxRate;
		chargeRate = Behaviour.ChargeRate;
		currentTime = 0f;
		currentCooldown = chargeRate.Evaluate(0f);
	}

	public override void Update()
	{
		base.Update();
		if (!(currentTime > timeToMaxRate))
		{
			currentTime += Time.deltaTime;
			currentCooldown -= Time.deltaTime;
			if (currentCooldown < 0f)
			{
				currentCooldown = chargeRate.Evaluate(currentTime / timeToMaxRate);
				Behaviour.FireProjectile();
			}
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}
}
