using Gameplay.GameControllers.Enemies.Menina;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Menina;

public class MeninaWalkBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.Menina.Menina _menina;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_menina == null)
		{
			_menina = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Menina.Menina>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		float normalizedTime = stateInfo.normalizedTime;
		float num = normalizedTime - Mathf.Floor(normalizedTime);
		if (num > 0.9f)
		{
			_menina.Inputs.HorizontalInput = 0f;
			_menina.Controller.PlatformCharacterPhysics.HSpeed = 0f;
			_menina.AnimatorInyector.NotifyOnStepFinished();
		}
		else
		{
			Step();
		}
	}

	private void Step()
	{
		if (_menina.Animator.GetBool("STEP_FWD"))
		{
			_menina.Inputs.HorizontalInput = ((_menina.Status.Orientation != 0) ? (-1f) : 1f);
		}
		else if (_menina.Animator.GetBool("STEP_BCK"))
		{
			_menina.Inputs.HorizontalInput = ((_menina.Status.Orientation != 0) ? 1f : (-1f));
		}
	}
}
