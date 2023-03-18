using System;
using Framework.Managers;
using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Entities.MiriamPortal.AI;

public class MiriamPortalPrayerFollowState : State
{
	private MiriamPortalPrayerBehaviour _behaviour;

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
		_behaviour = Machine.GetComponent<MiriamPortalPrayerBehaviour>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		originalY = _behaviour.transform.position.y;
		_behaviour.MiriamPortal.Audio.PlayFollow();
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		_behaviour.MiriamPortal.Audio.StopFollow();
	}

	public override void Update()
	{
		base.Update();
		Float();
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
		if (!Core.Logic.IsPaused)
		{
			Vector3 position = _behaviour.transform.position;
			float x = position.x;
			float y = position.y + (float)Math.Sin(Time.time * _behaviour.FloatingSpeed) * _behaviour.FloatingVerticalElongation;
			_behaviour.transform.position = new Vector2(x, y);
		}
	}
}
