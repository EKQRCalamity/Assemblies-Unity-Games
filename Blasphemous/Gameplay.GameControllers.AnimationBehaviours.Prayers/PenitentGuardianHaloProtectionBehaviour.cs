using Framework.Managers;
using Gameplay.GameControllers.Effects.Player;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Prayers;

public class PenitentGuardianHaloProtectionBehaviour : StateMachineBehaviour
{
	private PenitentGuardian _penitentGuardian;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_penitentGuardian == null)
		{
			_penitentGuardian = animator.GetComponent<PenitentGuardian>();
		}
		_penitentGuardian.IsTriggered = true;
		Core.Logic.Penitent.Audio.PrayerInvincibility();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_penitentGuardian.IsTriggered = false;
		_penitentGuardian.FadeOut();
		animator.gameObject.SetActive(value: false);
	}
}
