using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GameControllers.Entities.StateMachine;

public class StateMachine : MonoBehaviour
{
	protected List<State> StatesList = new List<State>();

	protected State CurrentState;

	public State GetCurrentState => CurrentState;

	private void Update()
	{
		if ((bool)CurrentState)
		{
			CurrentState.Update();
		}
	}

	private void LateUpdate()
	{
		if ((bool)CurrentState)
		{
			CurrentState.LateUpdate();
		}
	}

	protected virtual bool SwitchState(State state)
	{
		bool result = false;
		if ((bool)state && state != CurrentState)
		{
			if ((bool)CurrentState)
			{
				CurrentState.OnStateExit();
			}
			CurrentState = state;
			CurrentState.OnStateEnter();
			result = true;
		}
		return result;
	}

	public virtual bool SwitchState<TStateType>() where TStateType : State, new()
	{
		bool result = false;
		bool flag = false;
		foreach (State states in StatesList)
		{
			if (!(states is TStateType))
			{
				continue;
			}
			flag = true;
			result = SwitchState(states);
			break;
		}
		if (flag)
		{
			return result;
		}
		State state = new TStateType();
		state.OnStateInitialize(this);
		StatesList.Add(state);
		return SwitchState(state);
	}

	private void OnDestroy()
	{
		foreach (State states in StatesList)
		{
			states.Destroy();
		}
		StatesList.Clear();
	}
}
