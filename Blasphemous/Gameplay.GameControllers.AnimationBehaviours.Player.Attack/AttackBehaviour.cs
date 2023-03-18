using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Player.Sparks;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Attack;

public class AttackBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private readonly int _attackAnim = Animator.StringToHash("Attack");

	private readonly int _crouchDownAnim = Animator.StringToHash("Crouch Down");

	private AttackArea _penitentAttackArea;

	[Tooltip("The damage amount done by the penitent in this attack")]
	public int damageAmount;

	[Range(0f, 1f)]
	public float desiredBackwardTime = 0.2f;

	[Range(0f, 1f)]
	public float desiredPlayBackTime = 0.45f;

	private bool _isAttackFired;

	[Tooltip("The sword spark will be generated in this animation")]
	public SwordSpark swordSpark;

	private SwordSparkSpawner _swordSparkSpawner;

	private SwordAnimatorInyector _swordAnimatorInyector;

	private bool IsDemakeMode => Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.DEMAKE);

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
			_penitentAttackArea = _penitent.PenitentAttack.CurrentPenitentWeapon.AttackAreas[0];
		}
		_penitent.CurrentOutputDamage = damageAmount;
		if (_swordSparkSpawner == null)
		{
			_swordSparkSpawner = Core.Logic.Penitent.SwordSparkSpawner;
		}
		if (swordSpark != null)
		{
			_swordSparkSpawner.CurrentSwordSparkSpawningType = swordSpark.sparkType;
		}
		_isAttackFired = false;
		PenitentSword penitentSword = (PenitentSword)_penitent.PenitentAttack.CurrentPenitentWeapon;
		_swordAnimatorInyector = penitentSword.SlashAnimator;
		_penitent.OnAttackBehaviour_OnEnter(this);
		_penitent.Audio.StopLoadingChargedAttack();
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent.Status.IsGrounded && !animator.GetCurrentAnimatorStateInfo(0).IsName("Combo_3"))
		{
			float axis = _penitent.PlatformCharacterInput.Rewired.GetAxis(0);
			SetOrientation(axis, animator);
		}
		if (!_isAttackFired && _penitent.PlatformCharacterInput.Attack)
		{
			_isAttackFired = true;
		}
		if ((animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") || animator.GetCurrentAnimatorStateInfo(0).IsName("Attack_Running")) && stateInfo.normalizedTime >= desiredPlayBackTime && _isAttackFired && !_penitentAttackArea.IsTargetHit && !_penitent.PlatformCharacterInput.isJoystickUp)
		{
			animator.Play(_attackAnim, 0, desiredBackwardTime);
			_swordAnimatorInyector.PlayAttackDesiredTime(_penitent.PenitentAttack.CurrentLevel, desiredBackwardTime, _penitent.PenitentAttack.AttackColor);
		}
		if (_penitent.Status.IsGrounded && _penitent.PlatformCharacterInput.isJoystickDown && !_penitent.PenitentAttack.IsRunningCombo && !IsDemakeMode)
		{
			_penitent.Audio.AudioManager.StopSfx("PenitentAttack1");
			_penitent.Audio.AudioManager.StopSfx("PenitentAttack2");
			animator.Play(_crouchDownAnim);
		}
	}

	private void SetOrientation(float horAxis, Animator animator)
	{
		EntityOrientation orientation = _penitent.Status.Orientation;
		if (horAxis < -0.2f && orientation != EntityOrientation.Left)
		{
			_penitent.SetOrientation(EntityOrientation.Left);
			_penitent.PenitentAttack.ResetCombo();
			if (_penitent.PlatformCharacterInput.Attack)
			{
				animator.Play(_attackAnim);
			}
		}
		else if (horAxis > 0.2f && orientation != 0)
		{
			_penitent.SetOrientation(EntityOrientation.Right);
			_penitent.PenitentAttack.ResetCombo();
			if (_penitent.PlatformCharacterInput.Attack)
			{
				animator.Play(_attackAnim);
			}
		}
	}
}
