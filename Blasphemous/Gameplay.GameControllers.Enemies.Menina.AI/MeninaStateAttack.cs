using System.Collections.Generic;
using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Menina.AI;

public class MeninaStateAttack : State
{
	protected enum MENINA_ACTIONS
	{
		SMASH,
		STEP_BACK,
		STEP_FORWARD
	}

	protected MENINA_ACTIONS lastAction = MENINA_ACTIONS.STEP_BACK;

	protected List<MENINA_ACTIONS> availableActions;

	private int walkBalance;

	private Menina Menina { get; set; }

	private MeninaBehaviour Behaviour { get; set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		if (!Menina)
		{
			Menina = machine.GetComponent<Menina>();
		}
		if (!Behaviour)
		{
			Behaviour = Menina.GetComponent<MeninaBehaviour>();
		}
		availableActions = new List<MENINA_ACTIONS>
		{
			MENINA_ACTIONS.SMASH,
			MENINA_ACTIONS.STEP_BACK,
			MENINA_ACTIONS.STEP_FORWARD
		};
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		walkBalance = 0;
	}

	public override void Update()
	{
		base.Update();
		Behaviour.CurrentAttackLapse += Time.deltaTime;
		if (Behaviour.CurrentAttackLapse >= Behaviour.AttackCooldown)
		{
			Behaviour.CurrentAttackLapse = 0f;
			ChooseAttackAction();
			CheckNewState();
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		Behaviour.CurrentAttackLapse = 0f;
		Menina.AnimatorInyector.ResetAttackTrigger();
	}

	private void ChooseAttackAction()
	{
		Behaviour.StopMovement();
		float num = Vector2.Distance(Menina.StartPosition, Menina.transform.position);
		MENINA_ACTIONS action = GetAction();
		Debug.Log("ACTION = " + action);
		if (action == MENINA_ACTIONS.STEP_FORWARD)
		{
			Behaviour.SingleStep(forward: true);
		}
		else if (action == MENINA_ACTIONS.STEP_BACK && num > 1f)
		{
			Behaviour.SingleStep(forward: false);
		}
		else
		{
			Menina.EnemyBehaviour.Attack();
			Menina.StateMachine.SwitchState<MeninaStateAttack>();
		}
		lastAction = action;
	}

	private void CheckNewState()
	{
		if (!Behaviour.PlayerSeen)
		{
			if (Behaviour.PlayerHeard)
			{
				Menina.StateMachine.SwitchState<MeninaStateChase>();
			}
			else
			{
				Menina.StateMachine.SwitchState<MeninaStateBackwards>();
			}
		}
	}

	private bool ShouldRepeatSmash()
	{
		return Behaviour.ShouldRepeatSmash();
	}

	private MENINA_ACTIONS GetAction()
	{
		if (lastAction != 0 || (lastAction == MENINA_ACTIONS.SMASH && ShouldRepeatSmash()))
		{
			return MENINA_ACTIONS.SMASH;
		}
		if (walkBalance < 0)
		{
			walkBalance++;
			return MENINA_ACTIONS.STEP_FORWARD;
		}
		walkBalance--;
		return MENINA_ACTIONS.STEP_BACK;
	}
}
