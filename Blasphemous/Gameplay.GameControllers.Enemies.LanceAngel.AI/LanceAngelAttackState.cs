using System;
using Framework.Managers;
using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.LanceAngel.AI;

public class LanceAngelAttackState : State
{
	private float _cooldownCounter;

	private LanceAngelBehaviour _behaviour;

	protected LanceAngel LanceAngel { get; set; }

	private bool IsCooling => _cooldownCounter > 0f;

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		LanceAngel = machine.GetComponent<LanceAngel>();
		_behaviour = LanceAngel.Behaviour;
		LanceAngel.DashAttack.OnDashFinishedEvent += OnDashFinished;
		LanceAngel.OnDamaged += OnDamaged;
		LanceAngelBehaviour behaviour = _behaviour;
		behaviour.OnParry = (Core.SimpleEvent)Delegate.Combine(behaviour.OnParry, new Core.SimpleEvent(OnParry));
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		ResetCooldown();
	}

	public override void Update()
	{
		base.Update();
		_cooldownCounter -= Time.deltaTime;
		_behaviour.Floating();
		Vector3 position = LanceAngel.Target.transform.position;
		float num = Vector2.Distance(LanceAngel.transform.position, position);
		_behaviour.LookAtTarget(position);
		if (num > _behaviour.DistanceAttack || IsCooling)
		{
			LanceAngel.Behaviour.Chasing(position);
		}
		else if (!_behaviour.IsRepositioning)
		{
			LanceAngel.AnimatorInjector.AttackReady();
			LanceAngel.GhostSprites.EnableGhostTrail = true;
			_behaviour.Reposition(LanceAngel.AnimatorInjector.AttackStart);
		}
	}

	public override void LateUpdate()
	{
		base.LateUpdate();
		if (_behaviour.IsRepositioning)
		{
			LanceAngel.Spline.transform.position = _behaviour.PathOrigin;
		}
	}

	private void ResetCooldown()
	{
		if (_cooldownCounter < LanceAngel.Behaviour.AttackCooldown)
		{
			_cooldownCounter = LanceAngel.Behaviour.AttackCooldown;
		}
	}

	private void OnDashFinished()
	{
		_behaviour.IsRepositioning = false;
		LanceAngel.GhostSprites.EnableGhostTrail = false;
		LanceAngel.Spline.transform.localPosition = Vector3.zero;
		LanceAngel.AnimatorInjector.StopAttack();
		ResetCooldown();
	}

	private void OnDamaged()
	{
		_cooldownCounter = LanceAngel.MotionLerper.TimeTakenDuringLerp;
	}

	private void OnParry()
	{
		LanceAngel.StateMachine.SwitchState<LanceAngelParryState>();
	}

	public override void Destroy()
	{
		base.Destroy();
		if (LanceAngel != null)
		{
			LanceAngel.DashAttack.OnDashFinishedEvent -= OnDashFinished;
			LanceAngel.OnDamaged -= OnDamaged;
			LanceAngelBehaviour behaviour = _behaviour;
			behaviour.OnParry = (Core.SimpleEvent)Delegate.Remove(behaviour.OnParry, new Core.SimpleEvent(OnParry));
		}
	}
}
