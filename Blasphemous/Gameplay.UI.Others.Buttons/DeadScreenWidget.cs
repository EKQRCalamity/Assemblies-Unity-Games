using System;
using System.Collections;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Penitent;
using Sirenix.OdinInspector;
using Tools.Audio;
using UnityEngine;

namespace Gameplay.UI.Others.Buttons;

public class DeadScreenWidget : MonoBehaviour
{
	[BoxGroup("Sounds", true, false, 0)]
	[EventRef]
	public string eventSound = "event:/Key Event/Player Death";

	[BoxGroup("Sounds", true, false, 0)]
	[EventRef]
	public string eventSoundDemake = "event:/Background Layer/DemakeDeathFanfarria";

	[BoxGroup("Sounds", true, false, 0)]
	[EventRef]
	public string reverbSound = "snapshot:/PlayerDeath";

	[BoxGroup("Controls", true, false, 0)]
	public CanvasGroup NormalGroup;

	[BoxGroup("Controls", true, false, 0)]
	public CanvasGroup BossRushGroup;

	[BoxGroup("Controls", true, false, 0)]
	public CanvasGroup DemakeGroup;

	[BoxGroup("Controls", true, false, 0)]
	public CanvasGroup ContinueGroup;

	[BoxGroup("Anim", true, false, 0)]
	public float DelayToAnimImage = 1f;

	[BoxGroup("Anim", true, false, 0)]
	public float FadeImageTime = 2.15f;

	[BoxGroup("Anim", true, false, 0)]
	public float DelayToAnimText = 1f;

	[BoxGroup("Anim", true, false, 0)]
	public float FadeTextTime = 0.5f;

	private EventInstance reverbInstance;

	private void Awake()
	{
		HideAll();
		SpawnManager.OnPlayerSpawn += OnPenitentReady;
		reverbInstance = default(EventInstance);
	}

	private void OnDestroy()
	{
		SpawnManager.OnPlayerSpawn -= OnPenitentReady;
	}

	private IEnumerator OnDeadAction()
	{
		DialogWidget dialogWidget = UIController.instance.GetDialog();
		if (dialogWidget != null)
		{
			while (dialogWidget.IsShowingDialog())
			{
				yield return new WaitForSecondsRealtime(0.2f);
			}
		}
		StartReverb();
		Core.Input.SetBlocker("DEAD_SCREEN", blocking: true);
		UIController.instance.HideBossHealth();
		Core.Audio.PlayOneShot(eventSound);
		yield return new WaitForSecondsRealtime(DelayToAnimImage);
		Tweener tween2 = NormalGroup.DOFade(1f, FadeImageTime);
		yield return tween2.WaitForCompletion();
		yield return new WaitForSecondsRealtime(DelayToAnimText);
		tween2 = ContinueGroup.DOFade(1f, FadeImageTime);
		yield return tween2.WaitForCompletion();
		while (!Input.anyKey)
		{
			yield return null;
		}
		Core.UI.Fade.Fade(toBlack: true, 1.5f, 0f, delegate
		{
			StopReverb();
			HideAll();
			Core.Logic.Penitent.Respawn();
		});
	}

	private void OnPenitentReady(Penitent penitent)
	{
		Core.Input.SetBlocker("DEAD_SCREEN", blocking: false);
		if (Core.Logic != null && Core.Logic.Penitent != null)
		{
			Penitent penitent2 = Core.Logic.Penitent;
			penitent2.OnDead = (Core.SimpleEvent)Delegate.Combine(penitent2.OnDead, new Core.SimpleEvent(OnDead));
		}
	}

	private IEnumerator OnDeadMiriam()
	{
		UIController.instance.HideBossHealth();
		StartReverb();
		Core.Audio.PlayOneShot(eventSound);
		yield return new WaitForSeconds(0.5f);
		yield return Core.UI.Fade.FadeCoroutine(new Color(0f, 0f, 0f, 0f), Color.white, 1.5f, toBlack: true, null);
		StopReverb();
		HideAll();
		Core.Persistence.RestoreStored();
		Core.SpawnManager.RespawnMiriamSameLevel(useFade: true, Color.white);
	}

	private void OnDead()
	{
		bool ignoreNextAutomaticRespawn = Core.SpawnManager.IgnoreNextAutomaticRespawn;
		Core.SpawnManager.IgnoreNextAutomaticRespawn = false;
		if (Core.SpawnManager.AutomaticRespawn && !ignoreNextAutomaticRespawn)
		{
			Core.Persistence.RestoreStored();
			Core.SpawnManager.RespawnSafePosition();
		}
		else if (Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.BOSS_RUSH))
		{
			Singleton<Core>.Instance.StartCoroutine(OnDeadActionBossRush());
		}
		else if (Core.Events.AreInMiriamLevel())
		{
			Singleton<Core>.Instance.StartCoroutine(OnDeadMiriam());
		}
		else if (Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.DEMAKE))
		{
			Singleton<Core>.Instance.StartCoroutine(OnDeadDemake());
		}
		else
		{
			Singleton<Core>.Instance.StartCoroutine(OnDeadAction());
		}
	}

	private IEnumerator OnDeadActionBossRush()
	{
		StartReverb();
		Core.Input.SetBlocker("DEAD_SCREEN", blocking: true);
		UIController.instance.HideBossHealth();
		Core.Audio.PlayOneShot(eventSound);
		yield return new WaitForSeconds(1f);
		StopReverb();
		UIController.instance.PlayBossRushRankAudio(complete: false);
		yield return Core.UI.Fade.FadeCoroutine(toBlack: true, 2.5f, 0.5f);
		yield return new WaitForSeconds(2.5f);
		HideAll();
		Core.Input.SetBlocker("DEAD_SCREEN", blocking: false);
		Core.BossRushManager.EndCourse(completed: false);
	}

	private IEnumerator OnDeadDemake()
	{
		Core.Input.SetBlocker("DEAD_SCREEN", blocking: true);
		UIController.instance.HideBossHealth();
		Core.Audio.Ambient.StopCurrent();
		yield return new WaitForSecondsRealtime(DelayToAnimImage);
		Core.Audio.Ambient.SetSceneParams(eventSoundDemake, string.Empty, new AudioParam[0], string.Empty);
		Tweener tween2 = DemakeGroup.DOFade(1f, FadeImageTime);
		yield return tween2.WaitForCompletion();
		yield return new WaitForSecondsRealtime(DelayToAnimText);
		tween2 = ContinueGroup.DOFade(1f, FadeImageTime);
		yield return tween2.WaitForCompletion();
		while (!Input.anyKey)
		{
			yield return null;
		}
		Core.UI.Fade.Fade(toBlack: true, 1f, 0f, delegate
		{
			HideAll();
			Core.DemakeManager.EndDemakeRun(completed: false, 0);
		});
	}

	private void StopReverb()
	{
		if (reverbInstance.isValid())
		{
			reverbInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			reverbInstance.release();
			reverbInstance = default(EventInstance);
		}
	}

	private void StartReverb()
	{
		if (reverbInstance.isValid())
		{
			StopReverb();
		}
		reverbInstance = Core.Audio.CreateEvent(reverbSound);
		reverbInstance.start();
	}

	private void HideAll()
	{
		NormalGroup.alpha = 0f;
		BossRushGroup.alpha = 0f;
		ContinueGroup.alpha = 0f;
		DemakeGroup.alpha = 0f;
	}
}
