using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.CowardTrapper.AI;

public class CowardTrapperRunAwayState : State
{
	private float _currentTimeRunning;

	private float _currentTimeAwaiting;

	private Vector2 _targetPosition;

	protected CowardTrapper CowardTrapper { get; set; }

	private Vector2 CurrentRunAwayDirection { get; set; }

	private Vector2 GetRunningDirection => (!(CowardTrapper.Target.transform.position.x < CowardTrapper.transform.position.x)) ? Vector2.right : Vector2.left;

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		CowardTrapper = machine.GetComponent<CowardTrapper>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		_currentTimeRunning = CowardTrapper.Behaviour.TimeRunning;
		_currentTimeAwaiting = CowardTrapper.Behaviour.TimeAwaiting;
		_targetPosition = CowardTrapper.Target.transform.position;
		Debug.Log(CowardTrapper.Behaviour.ReverseDirection);
		CurrentRunAwayDirection = new Vector2(GetRunningDirection.x, GetRunningDirection.y);
		if (CowardTrapper.Behaviour.ReverseDirection)
		{
			CurrentRunAwayDirection *= -1f;
		}
	}

	public override void Update()
	{
		base.Update();
		_currentTimeRunning -= Time.deltaTime;
		if (_currentTimeRunning > 0f)
		{
			ResetTimeAwaiting();
			if (!CowardTrapper.Behaviour.IsBlocked)
			{
				CowardTrapper.Behaviour.IsRunAway = true;
				CowardTrapper.Behaviour.RunAway(CurrentRunAwayDirection);
			}
			else
			{
				CowardTrapper.Behaviour.ReverseDirection = true;
				CowardTrapper.Behaviour.StopMovement();
				_currentTimeRunning = 0f;
			}
		}
		else
		{
			CowardTrapper.Behaviour.IsRunAway = false;
			_currentTimeAwaiting -= Time.deltaTime;
			if (_currentTimeAwaiting < 0f)
			{
				ResetTimeRunning();
			}
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}

	private void ResetTimeRunning()
	{
		if (_currentTimeRunning < CowardTrapper.Behaviour.TimeRunning)
		{
			_currentTimeRunning = CowardTrapper.Behaviour.TimeRunning;
		}
	}

	private void ResetTimeAwaiting()
	{
		if (_currentTimeAwaiting < CowardTrapper.Behaviour.TimeAwaiting)
		{
			_currentTimeAwaiting = CowardTrapper.Behaviour.TimeAwaiting;
		}
	}
}
