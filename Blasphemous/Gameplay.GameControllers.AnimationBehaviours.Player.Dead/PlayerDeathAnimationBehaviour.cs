using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Dead;

public class PlayerDeathAnimationBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private readonly int _deathFallAnimHash = Animator.StringToHash("Death Fall");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		if (_penitent.IsSmashed)
		{
			_penitent.IsSmashed = !_penitent.IsSmashed;
			animator.Play(_deathFallAnimHash);
		}
		else if (!_penitent.IsImpaled)
		{
			_penitent.Audio.PlayDeath();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_penitent.GrabLadder.EnableClimbLadderAbility(enable: false);
		if (stateInfo.normalizedTime >= 0.95f && _penitent.Status.Dead)
		{
			animator.enabled = false;
		}
	}
}
