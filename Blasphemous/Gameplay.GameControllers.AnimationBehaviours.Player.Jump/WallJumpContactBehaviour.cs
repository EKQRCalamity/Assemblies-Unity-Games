using Gameplay.GameControllers.Effects.Player.Dust;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Jump;

public class WallJumpContactBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private WallClimbDust _wallClimbDust;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		animator.SetBool("STICK_ON_WALL", value: true);
		if (_penitent == null)
		{
			_penitent = animator.GetComponentInParent<Gameplay.GameControllers.Penitent.Penitent>();
			_wallClimbDust = _penitent.GetComponentInChildren<WallClimbDust>();
		}
		_penitent.IsStickedOnWall = true;
		_penitent.AnimatorInyector.ResetStuntByFall();
		_wallClimbDust.TriggerDust();
	}
}
