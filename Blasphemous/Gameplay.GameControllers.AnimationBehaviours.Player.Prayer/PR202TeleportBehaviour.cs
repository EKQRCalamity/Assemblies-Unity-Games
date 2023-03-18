using System.Collections.Generic;
using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.UI;
using Tools.Items;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Prayer;

public class PR202TeleportBehaviour : StateMachineBehaviour
{
	public GameObject teleportVFX;

	public float delay = 0.35f;

	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private bool doTeleport;

	private List<string> prohibitedScenes = new List<string> { "D14", "D22", "D23", "D24", "D04BZ03S01", "D01BZ08S01", "D03Z01S06", "D07Z01S03" };

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		doTeleport = !prohibitedScenes.Exists((string x) => Core.LevelManager.currentLevel.LevelName.StartsWith(x));
		doTeleport = doTeleport && !Core.Input.HasBlocker("INTERACTABLE");
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		_penitent.Status.Invulnerable = true;
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval(delay);
		sequence.OnComplete(SpawnVfx);
		sequence.Play();
	}

	private void SpawnVfx()
	{
		PoolManager.Instance.ReuseObject(teleportVFX, _penitent.transform.position, Quaternion.identity, createPoolIfNeeded: true);
		Core.Logic.Penitent.Shadow.ManuallyControllingAlpha = true;
		Core.Logic.Penitent.Shadow.SetShadowAlpha(0f);
		if (!doTeleport)
		{
			_penitent.Animator.speed = 2f;
		}
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
		if (doTeleport)
		{
			Core.Events.SetFlag("CHERUB_RESPAWN", b: true);
			Core.SpawnManager.Respawn();
			UIController.instance.HideBossHealth();
			UIController.instance.HideMiriamTimer();
			ChaliceEffect.ShouldUnfillChalice = true;
		}
		Core.Logic.Penitent.Status.Invulnerable = false;
		_penitent.Animator.speed = 1f;
	}
}
