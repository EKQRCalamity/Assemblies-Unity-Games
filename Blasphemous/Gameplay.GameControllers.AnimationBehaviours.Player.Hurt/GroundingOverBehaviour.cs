using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Hurt;

public class GroundingOverBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private bool _animationSoundEffect;

	private WallJump _wallJump;

	private Collider2D _damageCollider;

	private static readonly int StickOnWall = Animator.StringToHash("STICK_ON_WALL");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
			_damageCollider = _penitent.DamageArea.DamageAreaCollider;
		}
		_animationSoundEffect = false;
		if (!_wallJump)
		{
			_wallJump = _penitent.GetComponentInChildren<WallJump>();
		}
		if (!_wallJump.enabled)
		{
			_wallJump.enabled = true;
			Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
			animator.SetBool(StickOnWall, value: false);
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!_animationSoundEffect)
		{
			_penitent.Audio.PlayOverthrow();
			_animationSoundEffect = true;
		}
		if (_penitent.Status.Dead && stateInfo.normalizedTime >= 0.9f)
		{
			animator.enabled = false;
			if (_damageCollider.enabled)
			{
				_damageCollider.enabled = false;
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
		Core.Logic.Penitent.PlatformCharacterInput.ResetInputs();
		Core.Logic.Penitent.PlatformCharacterInput.ResetHorizontalBlockers();
		if (_penitent.IsFallingStunt)
		{
			_penitent.IsFallingStunt = !_penitent.IsFallingStunt;
		}
	}
}
