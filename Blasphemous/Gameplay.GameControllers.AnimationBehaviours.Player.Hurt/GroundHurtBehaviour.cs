using System.Collections;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Hurt;

public class GroundHurtBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private bool _animCompleted;

	private readonly int _crouchDownAnimaHash = Animator.StringToHash("Crouch Down");

	private readonly int _groundingOverAnimaHash = Animator.StringToHash("Grounding Over");

	private bool IsDemakeMode => Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.DEMAKE);

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!_penitent)
		{
			_penitent = Core.Logic.Penitent;
		}
		Hit lastHit = _penitent.DamageArea.LastHit;
		_penitent.DamageArea.HitDisplacement(lastHit.AttackingEntity.transform.position);
		_penitent.Status.Unattacable = true;
		_penitent.IsJumpingOff = false;
		_penitent.AnimatorInyector.FireJumpOffTrigger = false;
		_animCompleted = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent.Stats.Life.Current <= 0f)
		{
			animator.Play(_groundingOverAnimaHash);
		}
		if (stateInfo.normalizedTime >= 0.9f)
		{
			_animCompleted = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (_penitent.MotionLerper.IsLerping && !IsDemakeMode)
		{
			_penitent.MotionLerper.StopLerping();
		}
		if (_animCompleted)
		{
			Singleton<Core>.Instance.StartCoroutine(SetInvulnerabilityCoroutine());
		}
		else
		{
			_penitent.Status.Unattacable = false;
		}
		if (_penitent.PlatformCharacterInput.isJoystickDown)
		{
			animator.Play(_crouchDownAnimaHash, 0, 0.9f);
		}
	}

	private IEnumerator SetInvulnerabilityCoroutine()
	{
		_penitent.Status.Unattacable = true;
		yield return new WaitForSeconds(_penitent.DamageArea.InvulnerabilityLapse);
		_penitent.Status.Unattacable = false;
	}
}
