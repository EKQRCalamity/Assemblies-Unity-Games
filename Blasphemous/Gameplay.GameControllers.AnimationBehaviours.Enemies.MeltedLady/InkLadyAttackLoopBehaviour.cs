using System.Collections;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.MeltedLady;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.MeltedLady;

public class InkLadyAttackLoopBehaviour : StateMachineBehaviour
{
	private static readonly int Firing = Animator.StringToHash("FIRING");

	private InkLady InkLady { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (InkLady == null)
		{
			InkLady = animator.GetComponentInParent<InkLady>();
		}
		animator.SetBool(Firing, value: true);
		Singleton<Core>.Instance.StartCoroutine(StopAttackCoroutine(animator));
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		InkLady.Behaviour.TeleportCooldownLapse = 0f;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		animator.SetBool(Firing, value: false);
		InkLady.IsAttacking = false;
	}

	private IEnumerator StopAttackCoroutine(Animator animator)
	{
		float attackTime = Mathf.Clamp(InkLady.BeamAttackTime - 0.55f, 0f, InkLady.BeamAttackTime);
		yield return new WaitForSeconds(attackTime);
		if ((bool)animator)
		{
			animator.SetBool(Firing, value: false);
		}
	}
}
