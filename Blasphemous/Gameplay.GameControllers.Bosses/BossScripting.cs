using System;
using FMOD.Studio;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.PietyMonster;
using Gameplay.GameControllers.Entities;
using Gameplay.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay.GameControllers.Bosses;

public class BossScripting : MonoBehaviour
{
	private const string INTENSITY_PARAM = "Intensity";

	private const string STATE_PARAM2 = "State2";

	private const string STATE_PARAM3 = "State3";

	private const string STATE_PARAM4 = "State4";

	private const string STATE_PARAM5 = "State5";

	private const string STATE_PARAM6 = "State6";

	private const string ENDING_PARAM = "Ending";

	[SerializeField]
	private string id;

	[SerializeField]
	private Entity entity;

	[SerializeField]
	private bool empoweredPiety;

	[SerializeField]
	private GameObject[] enableObjects;

	private Gameplay.GameControllers.Bosses.PietyMonster.PietyMonster Boss;

	private EventInstance sound;

	private void Start()
	{
		SceneManager.sceneLoaded += OnLevelLoaded;
		Boss = UnityEngine.Object.FindObjectOfType<Gameplay.GameControllers.Bosses.PietyMonster.PietyMonster>();
		if (entity == null)
		{
			entity = Boss;
		}
		Boss.OnDeath += OnDead;
	}

	private void OnDead()
	{
		int @int = PlayerPrefs.GetInt("BOSS_TRIES", 1);
		if (!empoweredPiety)
		{
			Core.Metrics.CustomEvent("NORMAL_BOSS_KILLED", string.Empty, @int);
		}
		else
		{
			Core.Metrics.CustomEvent("EMPOWERED_BOSS_KILLED", string.Empty, @int);
		}
		Log.Trace("Metrics", "Boss fight Nº " + @int + " was successful.");
		PlayerPrefs.DeleteKey("BOSS_TRIES");
		sound.setParameterValue("Ending", 1f);
		UIController.instance.HideBossHealth();
		GameObject[] array = enableObjects;
		foreach (GameObject gameObject in array)
		{
			if (!(gameObject == null))
			{
				gameObject.SetActive(value: true);
			}
		}
	}

	private void OnLevelLoaded(Scene arg0, LoadSceneMode arg1)
	{
		if (sound.isValid())
		{
			sound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!sound.isValid() && !entity.Status.Dead)
		{
			PlayerPrefs.GetInt("BOSS_TRIES", 1);
			int @int = PlayerPrefs.GetInt("BOSS_TRIES");
			Log.Trace("Metrics", "Boss fight Nº" + @int + " started.");
			sound = Core.Audio.CreateCatalogEvent(id);
			sound.start();
			UIController.instance.ShowBossHealth(entity);
			if (entity is Enemy)
			{
				Enemy enemy = (Enemy)entity;
				enemy.UseHealthBar = false;
			}
			Core.Logic.CameraManager.CameraPlayerOffset.XOffset = 1f;
		}
	}

	private void Update()
	{
		if (sound.isValid())
		{
			switch (Boss.CurrentBossStatus)
			{
			case Gameplay.GameControllers.Bosses.PietyMonster.PietyMonster.BossStatus.First:
				sound.setParameterValue("Intensity", 1f);
				break;
			case Gameplay.GameControllers.Bosses.PietyMonster.PietyMonster.BossStatus.Second:
				sound.setParameterValue("Intensity", 3f);
				break;
			case Gameplay.GameControllers.Bosses.PietyMonster.PietyMonster.BossStatus.Third:
				sound.setParameterValue("Intensity", 4f);
				sound.setParameterValue("State2", 1f);
				break;
			case Gameplay.GameControllers.Bosses.PietyMonster.PietyMonster.BossStatus.Forth:
				sound.setParameterValue("Intensity", 5f);
				break;
			case Gameplay.GameControllers.Bosses.PietyMonster.PietyMonster.BossStatus.Fifth:
				sound.setParameterValue("Intensity", 6f);
				sound.setParameterValue("State3", 1f);
				break;
			case Gameplay.GameControllers.Bosses.PietyMonster.PietyMonster.BossStatus.Sixth:
				sound.setParameterValue("Intensity", 8f);
				sound.setParameterValue("State4", 1f);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}
}
