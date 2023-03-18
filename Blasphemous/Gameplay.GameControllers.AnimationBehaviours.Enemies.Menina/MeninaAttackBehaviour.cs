using System.Collections;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Menina;
using Gameplay.GameControllers.Enemies.Menina.AI;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Menina;

public class MeninaAttackBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.Menina.Menina _menina;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_menina == null)
		{
			_menina = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Menina.Menina>();
		}
		_menina.IsAttacking = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_menina.GetComponent<MeninaBehaviour>().ResetAttackCoolDown();
		_menina.Audio.StopAttack();
		Singleton<Core>.Instance.StartCoroutine(UnsetEntityAttack(0.5f));
	}

	private IEnumerator UnsetEntityAttack(float delay)
	{
		yield return new WaitForSeconds(delay);
		_menina.IsAttacking = false;
	}
}
