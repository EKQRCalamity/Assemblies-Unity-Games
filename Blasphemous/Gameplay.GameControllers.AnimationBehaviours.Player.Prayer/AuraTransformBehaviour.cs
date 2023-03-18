using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using Rewired;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Prayer;

public class AuraTransformBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private PrayerUse _prayer;

	private bool _cancel;

	protected global::Rewired.Player Rewired { get; private set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
			_prayer = _penitent.GetComponentInChildren<PrayerUse>();
			Rewired = ReInput.players.GetPlayer(0);
		}
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
		_cancel = false;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
		_penitent.Audio.StopPrayerCast();
	}
}
