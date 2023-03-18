using Gameplay.GameControllers.Enemies.NewFlagellant.Animator;
using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.NewFlagellant.AI;

public class NewFlagellantAttackState : State
{
	private float _currentAttackTime;

	public NewFlagellant NewFlagellant { get; private set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		NewFlagellant = machine.GetComponent<NewFlagellant>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		NewFlagellant.NewFlagellantBehaviour.StopMovement();
	}

	private void AnimatorInjector_OnAttackAnimationFinished(NewFlagellantAnimatorInyector obj)
	{
		obj.OnAttackAnimationFinished -= AnimatorInjector_OnAttackAnimationFinished;
		NewFlagellant.NewFlagellantBehaviour.LookAtPenitent();
		NewFlagellant.StateMachine.SwitchState<NewFlagellantChaseState>();
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}

	public override void Update()
	{
		base.Update();
		if (NewFlagellant.NewFlagellantBehaviour.CanAttack())
		{
			NewFlagellant.AnimatorInyector.OnAttackAnimationFinished -= AnimatorInjector_OnAttackAnimationFinished;
			NewFlagellant.AnimatorInyector.OnAttackAnimationFinished += AnimatorInjector_OnAttackAnimationFinished;
			float num = Random.Range(0f, 1f);
			if ((double)num < 0.33)
			{
				NewFlagellant.AnimatorInyector.FastAttack();
			}
			else
			{
				NewFlagellant.AnimatorInyector.Attack();
			}
			NewFlagellant.NewFlagellantBehaviour.ResetCooldown();
		}
	}

	public override void Destroy()
	{
		base.Destroy();
	}
}
