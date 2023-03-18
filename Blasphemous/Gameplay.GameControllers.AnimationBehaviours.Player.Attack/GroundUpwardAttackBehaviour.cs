using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Attack;

public class GroundUpwardAttackBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private bool _climbingDisabled;

	public Vector2 AttackAreaOffset;

	public Vector2 AttackAreaSize;

	private Vector2 _defaultAttackAreaOffset;

	private Vector2 _defaultAttackAreaSize;

	private bool _isAttackFired;

	[Range(0f, 1f)]
	public float DesiredPlayBackTime = 0.45f;

	[Range(0f, 1f)]
	public float DesiredBackwardTime = 0.2f;

	private SwordAnimatorInyector _swordAnimatorInyector;

	private PenitentSword _penitentSword;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
			_defaultAttackAreaOffset = new Vector2(_penitent.AttackArea.WeaponCollider.offset.x, _penitent.AttackArea.WeaponCollider.offset.y);
			_defaultAttackAreaSize = new Vector2(_penitent.AttackArea.WeaponCollider.bounds.size.x, _penitent.AttackArea.WeaponCollider.bounds.size.y);
			_penitentSword = (PenitentSword)_penitent.PenitentAttack.CurrentPenitentWeapon;
			_swordAnimatorInyector = _penitentSword.SlashAnimator;
		}
		_isAttackFired = false;
		_penitent.GrabLadder.EnableClimbLadderAbility(enable: false);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		_penitent.AttackArea.SetOffset(AttackAreaOffset);
		_penitent.AttackArea.SetSize(AttackAreaSize);
		if (!_isAttackFired && _penitent.PlatformCharacterInput.Attack)
		{
			_isAttackFired = true;
		}
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("GroundUpwardAttack") && stateInfo.normalizedTime >= DesiredPlayBackTime && _isAttackFired && !_penitent.AttackArea.IsTargetHit && _penitent.PlatformCharacterInput.isJoystickUp)
		{
			animator.Play("GroundUpwardAttack", 0, DesiredBackwardTime);
			_swordAnimatorInyector.PlayAttackDesiredTime(_penitent.PenitentAttack.CurrentLevel, DesiredBackwardTime, _penitent.PenitentAttack.AttackColor, "BasicUpward_Lv");
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_penitent.AttackArea.SetOffset(_defaultAttackAreaOffset);
		_penitent.AttackArea.SetSize(_defaultAttackAreaSize);
		_penitent.GrabLadder.EnableClimbLadderAbility();
	}
}
