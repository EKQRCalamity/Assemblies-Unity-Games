using System.Collections;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Gizmos;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Jump;

public class JumpOffBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private RootMotionDriver _rootMotion;

	private WaitForSeconds _enable2DCollisionsLapse;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_enable2DCollisionsLapse == null)
		{
			_enable2DCollisionsLapse = new WaitForSeconds(0.1f);
		}
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		if (_rootMotion == null)
		{
			_rootMotion = _penitent.GetComponentInChildren<RootMotionDriver>();
		}
		if (_penitent.IsCrouchAttacking)
		{
			_penitent.IsCrouchAttacking = !_penitent.IsCrouchAttacking;
		}
		animator.SetBool("CROUCH_ATTACKING", value: false);
		_penitent.Status.Invulnerable = true;
		_penitent.Dash.enabled = false;
		_penitent.Dash.SetDashSkinCollision();
		_penitent.PlatformCharacterInput.ResetActions();
		_penitent.PlatformCharacterInput.ResetInputs();
		_penitent.PlatformCharacterController.PlatformCharacterPhysics.Velocity = Vector3.zero;
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_penitent.startedJumpOff = true;
		_penitent.jumpOffRoot = new Vector2(_penitent.transform.position.x, _rootMotion.transform.position.y);
		if (!_penitent.IsJumpingOff)
		{
			_penitent.IsJumpingOff = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_penitent.Status.Invulnerable = false;
		_penitent.DamageArea.EnableEnemyAttack();
		_penitent.Dash.enabled = true;
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
		Singleton<Core>.Instance.StartCoroutine(Enable2DPhysics());
	}

	private IEnumerator Enable2DPhysics()
	{
		yield return _enable2DCollisionsLapse;
		_penitent.Dash.SetDefaultSkinCollision();
		_penitent.Physics.Enable2DCollision();
	}
}
