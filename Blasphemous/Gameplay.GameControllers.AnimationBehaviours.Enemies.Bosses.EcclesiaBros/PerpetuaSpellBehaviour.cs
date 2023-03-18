using Gameplay.GameControllers.Bosses.EcclesiaBros.Perpetua;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Bosses.EcclesiaBros;

public class PerpetuaSpellBehaviour : StateMachineBehaviour
{
	private Perpetua Perpetua { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (Perpetua == null)
		{
			Perpetua = animator.GetComponentInParent<Perpetua>();
		}
		Perpetua.Behaviour.IsSpelling = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Perpetua.Behaviour.IsSpelling = false;
	}
}
