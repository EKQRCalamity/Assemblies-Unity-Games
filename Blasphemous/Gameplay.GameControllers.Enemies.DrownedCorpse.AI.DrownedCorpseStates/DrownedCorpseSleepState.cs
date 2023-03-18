using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.DrownedCorpse.AI.DrownedCorpseStates;

public class DrownedCorpseSleepState : State
{
	private static readonly int Run = UnityEngine.Animator.StringToHash("RUN");

	private DrownedCorpse DrownedCorpse { get; set; }

	private DrownedCorpseBehaviour Behaviour { get; set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		DrownedCorpse = machine.GetComponent<DrownedCorpse>();
		Behaviour = DrownedCorpse.GetComponent<DrownedCorpseBehaviour>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		DrownedCorpse.DamageByContact = false;
		Behaviour.StopMovement(0.75f);
	}

	public override void Update()
	{
		base.Update();
		if (!DrownedCorpse.MotionChecker.HitsFloor || DrownedCorpse.MotionChecker.HitsBlock)
		{
			Behaviour.StopMovement();
		}
	}
}
