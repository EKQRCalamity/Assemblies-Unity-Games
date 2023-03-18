using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Hurt;

public class FallingOverBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private readonly int _throwbackTransitionAnim = Animator.StringToHash("ThrowbackTrans");

	private ThrowBack _throwBack;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
			_throwBack = _penitent.GetComponentInChildren<ThrowBack>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (!_penitent.RigidBody.isKinematic && _throwBack.IsOwnerFalling)
		{
			animator.Play(_throwbackTransitionAnim);
		}
	}
}
