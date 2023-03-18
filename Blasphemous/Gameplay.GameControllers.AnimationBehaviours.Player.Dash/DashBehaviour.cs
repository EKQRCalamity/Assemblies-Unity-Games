using System;
using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using Gameplay.GameControllers.Penitent.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Dash;

public class DashBehaviour : StateMachineBehaviour
{
	private readonly int _attackRunningAnimHash = Animator.StringToHash("Attack_Running");

	private readonly int _upwardAttackAnimHash = Animator.StringToHash("GroundUpwardAttack");

	private readonly int _runningAfterDashAnimHash = Animator.StringToHash("Start_Run_After_Dash");

	protected readonly int LungAttackAnim = Animator.StringToHash("LungeAttack");

	protected readonly int CrouchDownAnim = Animator.StringToHash("Crouch Down");

	protected readonly int ParryAnim = Animator.StringToHash("ParryChance");

	private bool _cancelToParry;

	private bool _addExtraDash;

	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		_penitent.PenitentMoveAnimations.PlayDash();
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
		ResetSwordSlashTriggers();
		_cancelToParry = false;
		_penitent.Dash.CrouchAfterDash = true;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (stateInfo.normalizedTime > 0.9f && _penitent.Dash.IsUpperBlocked && !_addExtraDash)
		{
			AddExtraDash();
		}
		if (_penitent.Dash.IsUpperBlocked || (_penitent.PlatformCharacterInput.Rewired.GetButtonTimedPressDown(5, 0.1f) && stateInfo.normalizedTime < 1f && CastLungeAttack()))
		{
			return;
		}
		if (_penitent.PlatformCharacterInput.Rewired.GetButtonDown(38))
		{
			_cancelToParry = true;
			_penitent.Dash.StopCast();
			_penitent.CancelEffect.PlayCancelEffect();
			_penitent.DashDustGenerator.GetStopDashDust(0.1f);
			_penitent.Parry.Cast();
			_penitent.Dash.CrouchAfterDash = false;
			_penitent.Animator.Play(ParryAnim);
		}
		bool button = _penitent.PlatformCharacterInput.Rewired.GetButton(6);
		if (_penitent.PlatformCharacterInput.Rewired.GetButtonUp(5) && !button && stateInfo.normalizedTime >= 0.1f)
		{
			_penitent.Dash.StopCast();
			_penitent.DashDustGenerator.GetStopDashDust(0.2f);
			_penitent.Dash.CrouchAfterDash = false;
			bool flag = _penitent.PlatformCharacterInput.Rewired.GetAxis(4) >= 0.75f;
			animator.Play((!flag) ? _attackRunningAnimHash : _upwardAttackAnimHash);
		}
		if (button && stateInfo.normalizedTime > 0.1f)
		{
			_penitent.AnimatorInyector.IsJumpWhileDashing = true;
			_penitent.Dash.StopCast();
			_penitent.Dash.CrouchAfterDash = false;
			Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
		}
		if (stateInfo.normalizedTime > 0.5f && stateInfo.normalizedTime < 1f && _penitent.PlatformCharacterInput.Rewired.GetAxis(4) < -0.5f)
		{
			Crouch();
		}
		else if (stateInfo.normalizedTime > 0.5f && stateInfo.normalizedTime < 1f && Math.Abs(_penitent.PlatformCharacterInput.Rewired.GetAxisRaw(0)) >= 0.1f)
		{
			if (!_penitent.Dash.StandUpAfterDash)
			{
				_penitent.Dash.StandUpAfterDash = true;
			}
			if (_penitent.Status.IsGrounded)
			{
				_penitent.DashDustGenerator.GetStopDashDust(0.1f);
			}
			_penitent.Dash.CrouchAfterDash = false;
			animator.Play(_runningAfterDashAnimHash);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (_penitent.Status.Unattacable)
		{
			_penitent.Status.Unattacable = false;
		}
		_penitent.PlatformCharacterInput.ResetInputs();
		if (!_cancelToParry)
		{
			Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
		}
		_addExtraDash = false;
		_penitent.Audio.StopDashSound();
	}

	private void AddExtraDash()
	{
		if (_addExtraDash)
		{
			return;
		}
		_addExtraDash = true;
		Vector3 vector = ((_penitent.Status.Orientation != 0) ? (-_penitent.transform.right) : _penitent.transform.right);
		Vector3 position = _penitent.transform.position;
		position.x += vector.x * 2f;
		_penitent.transform.DOMoveX(position.x, 0.3f).SetEase(Ease.OutSine).OnUpdate(delegate
		{
			Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
			if (!_penitent.Dash.IsUpperBlocked || _penitent.HasFlag("FRONT_BLOCKED"))
			{
				Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
				DOTween.Kill(_penitent.transform);
			}
		})
			.OnComplete(delegate
			{
				Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
			});
	}

	private bool CastLungeAttack()
	{
		LungeAttack componentInChildren = _penitent.GetComponentInChildren<LungeAttack>();
		if (!componentInChildren.IsAvailable)
		{
			return false;
		}
		_penitent.Dash.CrouchAfterDash = false;
		_penitent.Dash.StopCast();
		componentInChildren.Cast();
		componentInChildren.PlayLungeAnimByLevelReached();
		return true;
	}

	private void Crouch()
	{
		_penitent.Dash.StopCast();
		_penitent.Dash.StandUpAfterDash = false;
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
		if (_penitent.Status.IsGrounded)
		{
			_penitent.DashDustGenerator.GetStopDashDust(0f);
		}
		_penitent.Animator.Play(CrouchDownAnim, 0, 0.5f);
	}

	private void ResetSwordSlashTriggers()
	{
		if ((bool)_penitent)
		{
			SwordAnimatorInyector componentInChildren = _penitent.GetComponentInChildren<SwordAnimatorInyector>();
			if (componentInChildren != null)
			{
				componentInChildren.ResetParameters();
			}
		}
	}
}
