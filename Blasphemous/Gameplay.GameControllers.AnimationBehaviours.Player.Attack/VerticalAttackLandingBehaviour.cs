using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Attack;

public class VerticalAttackLandingBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent Penitent { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (Penitent == null)
		{
			Penitent = Core.Logic.Penitent;
		}
		Penitent.GrabLadder.EnableClimbLadderAbility(enable: false);
		Penitent.VerticalAttack.SetAttackAreaDimensionsBySkill();
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (stateInfo.normalizedTime > 0.5f)
		{
			Penitent.PlatformCharacterInput.ResetActions();
			Penitent.PlatformCharacterInput.ResetInputs();
			if (Penitent.PlatformCharacterInput.Rewired.GetButton(7))
			{
				Penitent.CancelEffect.PlayCancelEffect();
				Penitent.Parry.StopCast();
				Penitent.Animator.SetTrigger("DASH");
				Penitent.Animator.ResetTrigger("JUMP");
				Penitent.Animator.SetBool("DASHING", value: true);
				Penitent.Dash.Cast();
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Penitent.GrabLadder.EnableClimbLadderAbility();
		Penitent.VerticalAttack.SetDefaultAttackAreaDimensions();
	}
}
