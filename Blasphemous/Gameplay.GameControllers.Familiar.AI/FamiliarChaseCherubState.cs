using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Familiar.AI;

public class FamiliarChaseCherubState : State
{
	private Familiar Familiar { get; set; }

	private bool IsCloseToCherub { get; set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		Familiar = machine.GetComponent<Familiar>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		Familiar.Behaviour.ChasingElongation = 1f;
	}

	public override void Update()
	{
		base.Update();
		Familiar.Behaviour.Floating();
		if ((bool)Familiar.Behaviour.CherubInstance)
		{
			float num = Vector2.Distance(Familiar.transform.position, Familiar.Behaviour.CherubInstance.transform.position);
			IsCloseToCherub = num < Familiar.Behaviour.CherubCriticalDistance;
			Familiar.Behaviour.ChasingElongation = ((!IsCloseToCherub) ? 1f : 0.2f);
			Familiar.GhostTrail.EnableGhostTrail = !IsCloseToCherub;
		}
	}

	public override void LateUpdate()
	{
		base.LateUpdate();
		FamiliarBehaviour behaviour = Familiar.Behaviour;
		if ((bool)behaviour.CherubInstance)
		{
			behaviour.ChasingEntity(behaviour.CherubInstance, behaviour.CherubOffsetPosition);
			behaviour.SetOrientationByVelocity(behaviour.ChaseVelocity);
		}
	}

	private float GetDistanceColorParam(float min, float max, Transform a, Transform b)
	{
		float value = Vector2.Distance(a.position, b.position);
		value = Mathf.Clamp(value, min, max);
		return (max - value) / (max - min);
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}
}
