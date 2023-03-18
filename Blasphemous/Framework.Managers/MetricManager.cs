using System.Collections.Generic;
using Framework.FrameworkCore;
using Sirenix.Utilities;
using UnityAnalyticsHeatmap;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

namespace Framework.Managers;

public class MetricManager : GameSystem
{
	private float timeInScene;

	private string trackedScene;

	public override void Start()
	{
		LevelManager.OnLevelLoaded += OnLevelLoaded;
		InputEvent();
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		SceneTimeCount();
	}

	public override void Update()
	{
		SceneTimeCounterUpdate();
	}

	public void CustomEvent(string eventId, string name = "", float amount = -1f)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (!name.IsNullOrWhitespace())
		{
			dictionary.Add("NAME", name);
		}
		if (amount > -1f)
		{
			dictionary.Add("AMOUNT", amount);
		}
		dictionary.Add("SCENE", SceneManager.GetActiveScene().name);
		Analytics.CustomEvent(eventId, dictionary);
	}

	public void HeatmapEvent(string eventId, Vector2 position)
	{
		UnityAnalyticsHeatmap.HeatmapEvent.Send(eventId, position);
	}

	private void InputEvent()
	{
	}

	private void SceneTimeCount()
	{
		if (trackedScene == null)
		{
			trackedScene = SceneManager.GetActiveScene().name;
		}
		else if (!(trackedScene == SceneManager.GetActiveScene().name))
		{
			CustomEvent("TIME_IN_SCENE", string.Empty, timeInScene);
			timeInScene = 0f;
			trackedScene = SceneManager.GetActiveScene().name;
		}
	}

	private void SceneTimeCounterUpdate()
	{
		timeInScene += Time.deltaTime;
		if (trackedScene != SceneManager.GetActiveScene().name)
		{
			SceneTimeCount();
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
	}
}
