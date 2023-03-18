using CreativeSpore.SmartColliders;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Jump;

public class HardLandingBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private Vector2 _landingPosition;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_penitent == null)
		{
			_penitent = animator.GetComponentInParent<Gameplay.GameControllers.Penitent.Penitent>();
		}
		_landingPosition = new Vector2(_penitent.transform.position.x, _penitent.transform.position.y);
		_penitent.PlatformCharacterInput.ResetInputs();
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
		ClampHorizontalMovement(_landingPosition);
		_penitent.PlatformCharacterInput.ResetInputs();
		_penitent.PlatformCharacterController.SetActionState(eControllerActions.Jump, value: false);
		_penitent.PlatformCharacterInput.IsAttacking = true;
		_penitent.GrabLadder.EnableClimbLadderAbility(enable: false);
		_penitent.Dash.enabled = false;
		_penitent.Rumble.UsePreset("HardLanding");
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		ClampHorizontalMovement(_landingPosition);
		_penitent.PlatformCharacterInput.IsAttacking = true;
		if (stateInfo.normalizedTime > 0.5f)
		{
			if (_penitent.PlatformCharacterInput.Rewired.GetButton(7))
			{
				_penitent.CancelEffect.PlayCancelEffect();
				_penitent.Parry.StopCast();
				_penitent.Animator.SetTrigger("DASH");
				_penitent.Animator.ResetTrigger("JUMP");
				_penitent.Animator.SetBool("DASHING", value: true);
				_penitent.Dash.Cast();
			}
			if (!Core.InventoryManager.IsRosaryBeadEquipped("RB15"))
			{
				animator.speed = 1.25f;
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_penitent.PlatformCharacterInput.IsAttacking = false;
		_penitent.Dash.enabled = true;
		_penitent.GetComponentInChildren<Gameplay.GameControllers.Penitent.Abilities.RangeAttack>().enabled = true;
		_penitent.GrabLadder.EnableClimbLadderAbility();
		_penitent.PlatformCharacterInput.ResetInputs();
		animator.speed = 1f;
	}

	private void ClampHorizontalMovement(Vector2 pos)
	{
		if (!(_penitent == null))
		{
			_penitent.PlatformCharacterController.PlatformCharacterPhysics.HSpeed = 0f;
			float x = ((!_penitent.FloorChecker.OnMovingPlatform) ? pos.x : _penitent.transform.position.x);
			_penitent.transform.position = new Vector2(x, _penitent.transform.position.y);
		}
	}
}
