using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.JarThrower.AI;

public class JarThrowerAttackState : State
{
	protected JarThrower JarThrower { get; set; }

	protected float CurrentAttackCoolDown { get; set; }

	private bool CanAttack => CurrentAttackCoolDown <= 0f;

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		JarThrower = machine.GetComponent<JarThrower>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		JarThrower.AnimatorInjector.Walk(walk: false);
		ResetCoolDown();
	}

	public override void Update()
	{
		base.Update();
		CurrentAttackCoolDown -= Time.deltaTime;
		if (JarThrower.Behaviour.TargetSeen || JarThrower.IsAttacking)
		{
			Vector3 position = JarThrower.Target.transform.position;
			float num = Vector2.Distance(JarThrower.transform.position, position);
			if (JarThrower.Behaviour.CanWalk)
			{
				if (num >= JarThrower.Behaviour.ThrowingDistance)
				{
					JarThrower.Behaviour.Chase(JarThrower.Target.transform);
					return;
				}
				JarThrower.Behaviour.StopMovement();
				Attack();
			}
			else
			{
				JarThrower.Behaviour.StopMovement();
				JarThrower.Behaviour.LookAtTarget(position);
				JarThrower.StateMachine.SwitchState<JarThrowerChaseState>();
			}
		}
		else
		{
			JarThrower.StateMachine.SwitchState<JarThrowerChaseState>();
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}

	private void Attack()
	{
		if (CanAttack)
		{
			JarThrower.Behaviour.Attack();
			ResetCoolDown();
		}
	}

	private void ResetCoolDown()
	{
		if (CurrentAttackCoolDown < JarThrower.Behaviour.AttackCooldown)
		{
			CurrentAttackCoolDown = JarThrower.Behaviour.AttackCooldown;
		}
	}
}
