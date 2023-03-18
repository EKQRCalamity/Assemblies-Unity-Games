using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ChainedAngel.AI;

public class ChainedAngelAttackState : State
{
	private float _currentAttackLapse;

	private float _attackAwaitingLapse;

	private ChainedAngel ChainedAngel { get; set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		ChainedAngel = machine.GetComponent<ChainedAngel>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		_currentAttackLapse = 0f;
		ChainedAngel.FloatingMotion.IsFloating = false;
	}

	public override void Update()
	{
		base.Update();
		ChainedAngel.Behaviour.LookAtTarget(ChainedAngel.Target.transform.position);
		float deltaTime = Time.deltaTime;
		if (!ChainedAngel.BodyChainMaster.IsAttacking)
		{
			_currentAttackLapse += deltaTime;
		}
		if (!ChainedAngel.Behaviour.CanSeeTarget)
		{
			_currentAttackLapse = 0f;
			_attackAwaitingLapse += deltaTime;
			if (_attackAwaitingLapse > 1f)
			{
				ChainedAngel.StateMachine.SwitchState<ChainedAngelIdleState>();
			}
		}
		else
		{
			_attackAwaitingLapse = 0f;
		}
		if (_currentAttackLapse > ChainedAngel.Behaviour.AttackLapse)
		{
			_currentAttackLapse = 0f;
			ChainedAngel.Behaviour.Attack();
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}
}
