using System;
using Framework.Managers;
using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Entities.Guardian.AI;

public class GuardianPrayerFollowState : State
{
	private GuardianPrayerBehaviour _behaviour;

	private Vector3 _refVelocity;

	private float originalY;

	private float GetFollowSpeedFactor
	{
		get
		{
			float value = (_behaviour.GetMasterDistance - _behaviour.FollowDistance.x) / (_behaviour.FollowDistance.y - _behaviour.FollowDistance.x);
			return Mathf.Clamp01(value);
		}
	}

	public override void OnStateInitialize(Gameplay.GameControllers.Entities.StateMachine.StateMachine machine)
	{
		base.OnStateInitialize(machine);
		_behaviour = Machine.GetComponent<GuardianPrayerBehaviour>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		originalY = _behaviour.transform.position.y;
		_behaviour.Guardian.Audio.PlayFollow();
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		_behaviour.Guardian.Audio.StopFollow();
	}

	public override void Update()
	{
		base.Update();
		Float();
		CheckForIdle();
	}

	public override void LateUpdate()
	{
		base.LateUpdate();
		FollowMaster();
		_behaviour.LookAtMaster();
	}

	private void FollowMaster()
	{
		if (!(_behaviour.Master == null))
		{
			float num = Mathf.Lerp(_behaviour.FollowSpeed.x, _behaviour.FollowSpeed.y, GetFollowSpeedFactor);
			float maxSpeed = num * Time.deltaTime;
			Machine.transform.position = Vector3.SmoothDamp(Machine.transform.position, _behaviour.GetMasterOffSetPosition, ref _refVelocity, _behaviour.SmoothDampElongation, maxSpeed);
		}
	}

	private void Float()
	{
		Vector3 position = _behaviour.transform.position;
		float x = position.x;
		float y = position.y + (float)Math.Sin(Time.time * _behaviour.FloatingSpeed) * _behaviour.FloatingVerticalElongation;
		_behaviour.transform.position = new Vector2(x, y);
	}

	private void CheckForIdle()
	{
		if (Core.Logic.Penitent.IsClimbingLadder || Core.Logic.Penitent.IsStickedOnWall)
		{
			_behaviour.IdleFlag = true;
		}
	}
}
