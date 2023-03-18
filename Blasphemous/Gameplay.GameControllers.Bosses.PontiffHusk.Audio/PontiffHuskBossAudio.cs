using System.Collections.Generic;
using FMOD.Studio;
using Framework.Managers;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PontiffHusk.Audio;

public class PontiffHuskBossAudio : EntityAudio
{
	private Dictionary<string, EventInstance> eventRefsByEventId = new Dictionary<string, EventInstance>();

	private const string PontiffHusk_VanishIn = "PontiffHuskVanishIn";

	private const string PontiffHusk_VanishOut = "PontiffHuskVanishOut";

	private const string PontiffHusk_Execution = "PontiffHuskExecution";

	private const string PontiffHusk_TurnAround = "PontiffHuskTurnAround";

	private const string PontiffHusk_ChargedBlast = "PontiffHuskChargedBlast";

	private const string PontiffHusk_ChargedBlastNoVoice = "PontiffHuskChargedBlastNoVoice";

	private const string PontiffHusk_AmbientPostCombat = "AmbientPostPontiffCombat";

	private const string CombatParameter = "Combat";

	private void Awake()
	{
		Owner = GetComponent<PontiffHuskBoss>();
	}

	public void PlayVanishIn()
	{
		PlayOneShot_AUDIO("PontiffHuskVanishIn");
	}

	public void PlayVanishOut()
	{
		PlayOneShot_AUDIO("PontiffHuskVanishOut");
	}

	public void PlayExecution()
	{
		PlayOneShot_AUDIO("PontiffHuskExecution");
	}

	public void PlayTurnAround()
	{
		PlayOneShot_AUDIO("PontiffHuskTurnAround");
	}

	public void PlayChargedBlast()
	{
		PlayLocalized_AUDIO("PontiffHuskChargedBlast");
	}

	public void PlayChargedBlastNoVoice()
	{
		PlayLocalized_AUDIO("PontiffHuskChargedBlastNoVoice");
	}

	public void PlayAmbientPostCombat()
	{
		Play_AUDIO("AmbientPostPontiffCombat");
	}

	public void StopAmbientPostCombat()
	{
		Stop_AUDIO("AmbientPostPontiffCombat");
	}

	public void StartCombatMusic()
	{
		Core.Audio.Ambient.SetSceneParam("Combat", 1f);
	}

	public void PlayLocalized_AUDIO(string eventId)
	{
		EventInstance panning = base.AudioManager.CreateCatalogEvent(eventId);
		if (!panning.isValid())
		{
			Debug.LogError($"ERROR: Couldn't find catalog sound event called <{eventId}>");
			return;
		}
		float value = ((!Core.Localization.GetCurrentAudioLanguageCode().ToUpper().StartsWith("ES")) ? 0f : 1f);
		panning.setParameterValue("spanish", value);
		panning.setCallback(SetPanning(panning), EVENT_CALLBACK_TYPE.CREATED);
		panning.start();
		panning.release();
	}

	public void PlayOneShot_AUDIO(string eventId, FxSoundCategory category = FxSoundCategory.Attack)
	{
		PlayOneShotEvent(eventId, category);
	}

	public void Play_AUDIO(string eventId)
	{
		if (eventRefsByEventId.TryGetValue(eventId, out var value))
		{
			StopEvent(ref value);
			eventRefsByEventId.Remove(eventId);
		}
		value = default(EventInstance);
		PlayEvent(ref value, eventId, checkSpriteRendererVisible: false);
		eventRefsByEventId[eventId] = value;
	}

	public void Stop_AUDIO(string eventId)
	{
		if (eventRefsByEventId.TryGetValue(eventId, out var value))
		{
			StopEvent(ref value);
			eventRefsByEventId.Remove(eventId);
		}
	}

	public void StopAll()
	{
		foreach (string key in eventRefsByEventId.Keys)
		{
			EventInstance eventInstance = eventRefsByEventId[key];
			StopEvent(ref eventInstance);
		}
		eventRefsByEventId.Clear();
	}
}
