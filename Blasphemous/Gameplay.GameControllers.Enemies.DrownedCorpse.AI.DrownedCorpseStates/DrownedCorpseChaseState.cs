using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.DrownedCorpse.AI.DrownedCorpseStates;

public class DrownedCorpseChaseState : State
{
	private bool isSetChaseDirection;

	private float timeoutBeforeChase;

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
		Behaviour.LookAtTarget(DrownedCorpse.Target.transform.position);
		DrownedCorpse.AnimatorInyector.Awake();
		DrownedCorpse.AnimatorInyector.DontRun();
		isSetChaseDirection = false;
		timeoutBeforeChase = 0f;
	}

	public override void Update()
	{
		base.Update();
		timeoutBeforeChase += Time.deltaTime;
		if (!(timeoutBeforeChase < Behaviour.MaxTimeAwaitingBeforeChase))
		{
			DrownedCorpse.AnimatorInyector.Run();
			Vector3 position = DrownedCorpse.Target.transform.position;
			if (DrownedCorpse.Animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
			{
				Chase(position);
			}
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		DrownedCorpse.Behaviour.IsChasing = false;
	}

	private void Chase(Vector3 position)
	{
		if (!DrownedCorpse.MotionChecker.HitsFloor || DrownedCorpse.MotionChecker.HitsBlock || DrownedCorpse.Status.Dead)
		{
			Behaviour.StopMovement();
			return;
		}
		DrownedCorpse.Behaviour.IsChasing = true;
		float horizontalInput = ((DrownedCorpse.Status.Orientation != 0) ? (-1f) : 1f);
		DrownedCorpse.Input.HorizontalInput = horizontalInput;
	}
}
