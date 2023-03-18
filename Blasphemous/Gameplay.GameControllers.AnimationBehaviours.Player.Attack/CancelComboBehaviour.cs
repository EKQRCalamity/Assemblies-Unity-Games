using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Attack;

public class CancelComboBehaviour : StateMachineBehaviour
{
	private readonly int _parryAnimation = Animator.StringToHash("ParryChance");

	private Gameplay.GameControllers.Penitent.Penitent Penitent { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (Penitent == null)
		{
			Penitent = Core.Logic.Penitent;
		}
		RequestParry();
		RequestDash();
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		RequestParry();
		RequestDash();
	}

	private void RequestParry()
	{
		if (Penitent.PlatformCharacterInput.Rewired.GetButtonDown(38))
		{
			Penitent.CancelEffect.PlayCancelEffect();
			Penitent.Parry.StopCast();
			Penitent.Animator.SetBool("PARRY", value: true);
			Penitent.Animator.Play(_parryAnimation);
		}
	}

	private void RequestDash()
	{
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
