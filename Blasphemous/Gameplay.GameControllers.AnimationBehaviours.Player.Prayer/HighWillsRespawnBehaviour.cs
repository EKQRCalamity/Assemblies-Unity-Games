using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Prayer;

public class HighWillsRespawnBehaviour : StateMachineBehaviour
{
	public GameObject crisantaVFX;

	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
		Core.Logic.Penitent.SetOrientation(EntityOrientation.Right);
		PoolManager.Instance.ReuseObject(crisantaVFX, _penitent.transform.position, Quaternion.identity, createPoolIfNeeded: true);
		Core.Logic.Penitent.Shadow.ManuallyControllingAlpha = true;
		Core.Logic.Penitent.Shadow.SetShadowAlpha(0f);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
		Tween t = DOTween.To(() => Core.Logic.Penitent.Shadow.GetShadowAlpha(), delegate(float x)
		{
			Core.Logic.Penitent.Shadow.SetShadowAlpha(x);
		}, 1f, 0.2f);
		t.OnComplete(delegate
		{
			Core.Logic.Penitent.Shadow.ManuallyControllingAlpha = false;
		});
	}
}
