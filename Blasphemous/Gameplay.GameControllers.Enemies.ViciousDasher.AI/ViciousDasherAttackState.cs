using System;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ViciousDasher.AI;

public class ViciousDasherAttackState : State
{
	private float _currentAttackTime;

	public ViciousDasher ViciousDasher { get; private set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		ViciousDasher = machine.GetComponent<ViciousDasher>();
		MotionLerper motionLerper = ViciousDasher.MotionLerper;
		motionLerper.OnLerpStop = (Core.SimpleEvent)Delegate.Combine(motionLerper.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
	}

	private void OnLerpStop()
	{
		if (ViciousDasher.DistanceToTarget <= ViciousDasher.ViciousDasherBehaviour.CloseRange)
		{
			ViciousDasher.IsAttacking = true;
			ViciousDasher.AnimatorInjector.ResetDash();
			ViciousDasher.AnimatorInjector.Attack();
		}
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		ViciousDasher.AnimatorInjector.Dash();
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		_currentAttackTime = 0f;
		ViciousDasher.AnimatorInjector.StopAttack();
	}

	public override void Update()
	{
		base.Update();
		bool flag = ViciousDasher.DistanceToTarget <= ViciousDasher.ViciousDasherBehaviour.CloseRange;
		if (ViciousDasher.IsAttacking)
		{
			_currentAttackTime += Time.deltaTime;
			if (!(_currentAttackTime >= ViciousDasher.ViciousDasherBehaviour.AttackTime))
			{
				return;
			}
			_currentAttackTime = 0f;
			ViciousDasher.IsAttacking = false;
		}
		else if (flag)
		{
			ViciousDasher.AnimatorInjector.ResetDash();
			ViciousDasher.MotionLerper.StopLerping();
		}
		else
		{
			ViciousDasher.AnimatorInjector.StopAttack();
			ViciousDasher.AnimatorInjector.Dash();
		}
		if (ViciousDasher.MotionLerper.IsLerping)
		{
			ViciousDasher.AnimatorInjector.StopAttack();
		}
	}

	public override void Destroy()
	{
		base.Destroy();
		if (ViciousDasher != null)
		{
			MotionLerper motionLerper = ViciousDasher.MotionLerper;
			motionLerper.OnLerpStop = (Core.SimpleEvent)Delegate.Remove(motionLerper.OnLerpStop, new Core.SimpleEvent(OnLerpStop));
		}
	}
}
