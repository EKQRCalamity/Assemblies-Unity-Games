using System;
using System.Collections.Generic;
using FMOD.Studio;
using Gameplay.GameControllers.Bosses.BossFight;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Snake.Audio;

public class SnakeAudio : EntityAudio
{
	public BossFightAudio BossFightAudio;

	private Dictionary<string, EventInstance> eventRefsByEventId = new Dictionary<string, EventInstance>();

	private const string Snake_Rain = "AquilonRain";

	private const string Snake_Grunt1 = "SnakeGrunt_#1";

	private const string Snake_Grunt2 = "SnakeGrunt_#2";

	private const string Snake_Grunt3 = "SnakeGrunt_#3";

	private const string Snake_Grunt4 = "SnakeGrunt_#4";

	private const string Sanake_ClosingMouth = "SnakeClosingMouth";

	private const string Snake_Bite = "SnakeBite";

	private const string Snake_Back = "SnakeBack";

	private const string Snake_VanishOut = "SnakeVanishOut";

	private const string Snake_VanishIn = "SnakeVanishIn";

	private const string Snake_PhaseMovement = "SnakePhaseMovement";

	private const string Snake_TailShot = "SnakeTailShot";

	private const string Snake_Tail = "SnakeTail";

	private const string Snake_Thunder = "Thunder";

	private const string Snake_ElectricTail = "SnakeElectricTail";

	private const string Snake_ElectricShot = "SnakeElectricShot";

	private const string Snake_TongleExplosion = "SnakeTongleExplosion";

	private const string Snake_Wind = "SnakeWind";

	private const string Snake_Death = "SnakeDeath";

	private const string Snake_DeathStinger = "SnakeDeathStinger";

	private int fightState = 1;

	private void Awake()
	{
		Owner = GetComponent<Snake>();
	}

	public void PlaySnakeRain()
	{
		Play_AUDIO("AquilonRain");
	}

	public void StopSnakeRain()
	{
		Stop_AUDIO("AquilonRain");
	}

	public void IncreaseSnakeRainState()
	{
		if (eventRefsByEventId.TryGetValue("AquilonRain", out var value))
		{
			fightState++;
			string text = "State" + fightState;
			value.setParameterValue(text, 1f);
			SetBossTrackParam(fightState);
		}
	}

	public void SetSnakeRainState(int state)
	{
		if (eventRefsByEventId.TryGetValue("AquilonRain", out var value))
		{
			for (int i = 1; i < 4; i++)
			{
				float value2 = ((i > state) ? 0f : 1f);
				string text = "State" + i;
				value.setParameterValue(text, value2);
			}
		}
	}

	public void PlaySnakeVanishOut()
	{
		PlayOneShot_AUDIO("SnakeVanishOut", FxSoundCategory.Motion);
	}

	public void PlaySnakeVanishIn()
	{
		PlayOneShot_AUDIO("SnakeVanishIn", FxSoundCategory.Motion);
	}

	public void PlaySnakePhaseMovement()
	{
		PlayOneShot_AUDIO("SnakePhaseMovement", FxSoundCategory.Motion);
	}

	public void PlaySnakeTailShot()
	{
		PlayOneShot_AUDIO("SnakeTailShot");
	}

	public void PlaySnakeTail()
	{
		PlayOneShot_AUDIO("SnakeTail", FxSoundCategory.Motion);
	}

	public void PlaySnakeThunder()
	{
		PlayOneShot_AUDIO("Thunder", FxSoundCategory.Motion);
	}

	public void PlaySnakeTongueExplosion()
	{
		PlayOneShot_AUDIO("SnakeTongleExplosion", FxSoundCategory.Motion);
	}

	public void PlaySnakeWind()
	{
		PlayOneShot_AUDIO("SnakeWind", FxSoundCategory.Motion);
	}

	public void PlaySnakeDeath()
	{
		PlayOneShot_AUDIO("SnakeDeath", FxSoundCategory.Motion);
	}

	public void PlaySnakeDeathStinger()
	{
		PlayOneShot_AUDIO("SnakeDeathStinger", FxSoundCategory.Motion);
	}

	public void PlaySnakeElectricTail()
	{
		Play_AUDIO("SnakeElectricTail");
	}

	public void StopSnakeElectricTail()
	{
		Stop_AUDIO("SnakeElectricTail");
	}

	public void PlaySnakeElectricShot()
	{
		Play_AUDIO("SnakeElectricShot");
	}

	public void StopSnakeElectricShot()
	{
		Stop_AUDIO("SnakeElectricShot");
	}

	public void SetBossTrackParam(int state)
	{
		if (BossFightAudio == null)
		{
			BossFightAudio = UnityEngine.Object.FindObjectOfType<BossFightAudio>();
		}
		BossFightAudio.SetBossTrackParam("State" + state, 1f);
	}

	public void PlaySnakeGrunt1()
	{
		PlayOneShot_AUDIO("SnakeGrunt_#1");
	}

	public void PlaySnakeGrunt2()
	{
		PlayOneShot_AUDIO("SnakeGrunt_#2");
	}

	public void PlaySnakeGrunt3()
	{
		PlayOneShot_AUDIO("SnakeGrunt_#3");
	}

	public void PlaySnakeGrunt4()
	{
		PlayOneShot_AUDIO("SnakeGrunt_#4");
	}

	public void PlaySnakeBite()
	{
		PlayOneShot_AUDIO("SnakeBite");
	}

	public void PlaySnakeBack()
	{
		PlayOneShot_AUDIO("SnakeBack", FxSoundCategory.Motion);
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

	public void SetParam(EventInstance eventInstance, string paramKey, float value)
	{
		try
		{
			eventInstance.getParameter(paramKey, out var instance);
			instance.setValue(value);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.Message + ex.StackTrace);
		}
	}
}
