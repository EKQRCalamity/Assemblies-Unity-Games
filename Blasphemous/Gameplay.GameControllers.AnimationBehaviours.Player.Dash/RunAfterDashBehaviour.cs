using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Dash;

public class RunAfterDashBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private DashBehaviour _dashBehaviour;

	public bool StopDash;

	private static readonly int AttackRunningAnim = Animator.StringToHash("Attack_Running");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		if (_dashBehaviour == null)
		{
			_dashBehaviour = animator.GetBehaviour<DashBehaviour>();
		}
		StopDash = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent.PlatformCharacterInput.Rewired.GetButton(5))
		{
			animator.Play(AttackRunningAnim);
		}
		if (stateInfo.normalizedTime >= 0.5f && !StopDash)
		{
			StopDash = true;
			_penitent.Dash.StopCast();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent.Dash.StandUpAfterDash)
		{
			_penitent.Dash.StandUpAfterDash = false;
		}
		if (!StopDash)
		{
			StopDash = true;
			_penitent.Dash.StopCast();
		}
	}
}
