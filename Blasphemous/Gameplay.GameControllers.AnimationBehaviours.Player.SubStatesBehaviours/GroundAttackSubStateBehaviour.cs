using System.Collections;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.SubStatesBehaviours;

public class GroundAttackSubStateBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private bool _isRunningClimbCoroutine;

	private WaitForSeconds _climbWaiting = new WaitForSeconds(0.42f);

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_penitent == null)
		{
			_penitent = animator.GetComponentInParent<Gameplay.GameControllers.Penitent.Penitent>();
		}
		Singleton<Core>.Instance.StartCoroutine(DisableClimbAbility());
		RaiseAttackEvent(animator);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		animator.speed = ((!(_penitent.PenitentAttack.AttackSpeed > 1f)) ? 1f : _penitent.PenitentAttack.AttackSpeed);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		animator.speed = 1f;
		_penitent.PenitentAttackAnimations.CloseAttackWindow();
		_penitent.PenitentAttack.ClearHitEntityList();
	}

	private IEnumerator DisableClimbAbility()
	{
		if (!_isRunningClimbCoroutine)
		{
			_isRunningClimbCoroutine = true;
			_penitent.GrabLadder.EnableClimbLadderAbility(enable: false);
			yield return _climbWaiting;
			_penitent.GrabLadder.EnableClimbLadderAbility();
			_isRunningClimbCoroutine = false;
		}
	}

	private void RaiseAttackEvent(Animator animator)
	{
		if (!IsRunningParry(animator) && !IsRunningGuardSlide(animator))
		{
			_penitent.AnimatorInyector.RaiseAttackEvent();
		}
	}

	private bool IsRunningParry(Animator animator)
	{
		Parry parry = Core.Logic.Penitent.Parry;
		bool flag = animator.GetCurrentAnimatorStateInfo(0).IsName("ParryFailed") || parry.IsRunningParryAnim;
		bool casting = Core.Logic.Penitent.Parry.Casting;
		return flag || casting;
	}

	private bool IsRunningGuardSlide(Animator animator)
	{
		return animator.GetCurrentAnimatorStateInfo(0).IsName("GuardToIdle") || animator.GetCurrentAnimatorStateInfo(0).IsName("GuardSlide");
	}
}
