using System;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Gizmos;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.ClimbLadder;

public class GrabLadderDownBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private RootMotionDriver _rootMotionDriver;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
			_rootMotionDriver = _penitent.GetComponentInChildren<RootMotionDriver>();
		}
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
		_penitent.IsJumpingOff = false;
		_penitent.IsGrabbingLadder = true;
		_penitent.GrabLadder.SetClimbingSpeed(0f);
		_penitent.DamageArea.EnableEnemyAttack(enable: false);
		if (_penitent.IsCrouched)
		{
			_penitent.IsCrouched = !_penitent.IsCrouched;
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (stateInfo.normalizedTime > 0.9f && !_penitent.StartingGoingDownLadders)
		{
			_penitent.IsClimbingLadder = true;
			_penitent.DamageArea.EnableEnemyAttack();
			_penitent.GrabLadder.SetClimbingSpeed(2.25f);
			SetRootMotionPosition(delegate
			{
				Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
				animator.Play("ladder_going_down");
			});
		}
	}

	protected void SetRootMotionPosition(Action callback = null)
	{
		_penitent.RootMotionDrive = new Vector2(_rootMotionDriver.transform.position.x, _rootMotionDriver.transform.position.y);
		_penitent.transform.position = _penitent.RootMotionDrive;
		_penitent.StartingGoingDownLadders = true;
		callback?.Invoke();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (Core.Input.HasBlocker("PLAYER_LOGIC"))
		{
			Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
		}
	}
}
