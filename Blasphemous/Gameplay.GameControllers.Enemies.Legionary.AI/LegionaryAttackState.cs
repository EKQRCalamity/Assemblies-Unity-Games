using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Legionary.AI;

public class LegionaryAttackState : State
{
	protected float TargetLostLapse;

	protected Legionary Legionary { get; private set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		Legionary = machine.GetComponent<Legionary>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		TargetLostLapse = Legionary.Behaviour.TimeLapseToGoPatrolling;
	}

	public override void Update()
	{
		base.Update();
		bool canWalk = Legionary.Behaviour.CanWalk;
		Transform transform = Legionary.Target.transform;
		float num = Vector2.Distance(transform.position, Legionary.transform.position);
		if (Legionary.Behaviour.GotParry)
		{
			Legionary.Behaviour.Stop();
			return;
		}
		if (!Legionary.Behaviour.CanSeeTarget)
		{
			TargetLostLapse -= Time.deltaTime;
			if (TargetLostLapse <= 0f)
			{
				Legionary.StateMachine.SwitchState<LegionaryWanderState>();
			}
		}
		else
		{
			TargetLostLapse = Legionary.Behaviour.TimeLapseToGoPatrolling;
		}
		if (!canWalk)
		{
			Legionary.MotionLerper.StopLerping();
		}
		if (!Legionary.IsAttacking && !Legionary.Behaviour.IsHurt)
		{
			if (num > Legionary.Behaviour.MinDistanceAttack && canWalk)
			{
				Legionary.Behaviour.Chase(transform);
			}
			if (num <= Legionary.Behaviour.MinDistanceAttack)
			{
				Legionary.Behaviour.Stop();
				Legionary.Behaviour.RandMeleeAttack();
			}
		}
		else
		{
			if (!canWalk)
			{
				Legionary.Behaviour.LookAtTarget(transform.position);
			}
			Legionary.Behaviour.Stop();
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}
}
