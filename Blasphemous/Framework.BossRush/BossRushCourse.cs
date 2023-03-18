using System;
using System.Collections.Generic;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Framework.BossRush;

[Serializable]
public class BossRushCourse : SerializedScriptableObject
{
	public bool IsRandomCourse;

	[SerializeField]
	[HideIf("IsRandomCourse", true)]
	private List<string> Scenes = new List<string>();

	[SerializeField]
	[HideIf("IsRandomCourse", true)]
	private List<string> FontRechargingScenes = new List<string>();

	public BossRushManager.BossRushCourseScore MaxScoreForFailedRuns;

	[FormerlySerializedAs("ScoresByMinutes")]
	public List<ScoreInterval> ScoresByMinutesInNormal;

	public List<ScoreInterval> ScoresByMinutesInHard;

	public GameObject healthFontRechargingVfx;

	public GameObject fervourFontRechargingVfx;

	public virtual List<string> GetScenes()
	{
		return Scenes;
	}

	public virtual List<string> GetFontRechargingScenes()
	{
		return FontRechargingScenes;
	}

	[Button(ButtonSizes.Small)]
	public void ResetDefaultMaxScoreForFailedRuns()
	{
		MaxScoreForFailedRuns = (BossRushManager.BossRushCourseScore)(Scenes.Count - 1);
	}

	private void ResetDefaultScores(List<ScoreInterval> scoresInterval)
	{
		scoresInterval.Clear();
		int num = 0;
		BossRushManager.BossRushCourseScore[] collection = (BossRushManager.BossRushCourseScore[])Enum.GetValues(typeof(BossRushManager.BossRushCourseScore));
		List<BossRushManager.BossRushCourseScore> list = new List<BossRushManager.BossRushCourseScore>(collection);
		list.Reverse();
		foreach (BossRushManager.BossRushCourseScore item2 in list)
		{
			if (item2 == MaxScoreForFailedRuns)
			{
				break;
			}
			ScoreInterval scoreInterval = default(ScoreInterval);
			scoreInterval.score = item2;
			scoreInterval.timeRangeInMinutes = new Vector2(num, num + 1);
			ScoreInterval item = scoreInterval;
			scoresInterval.Add(item);
			num++;
		}
	}

	[Button(ButtonSizes.Small)]
	public void ResetDefaultScoresInNormal()
	{
		ResetDefaultScores(ScoresByMinutesInNormal);
	}

	[Button(ButtonSizes.Small)]
	public void ResetDefaultScoresInHard()
	{
		ResetDefaultScores(ScoresByMinutesInHard);
	}

	private void PropagateIntervalModification(List<ScoreInterval> scoresInterval)
	{
		float num = 0f;
		for (int i = 0; i < scoresInterval.Count; i++)
		{
			float num2 = scoresInterval[i].timeRangeInMinutes[0];
			float num3 = scoresInterval[i].timeRangeInMinutes[1];
			if (num2 < num)
			{
				num3 += num - num2;
				num2 = num;
			}
			else if (num2 > num)
			{
				num3 -= num2 - num;
				num2 = num;
			}
			num = num3;
			ScoreInterval scoreInterval = default(ScoreInterval);
			scoreInterval.score = scoresInterval[i].score;
			scoreInterval.timeRangeInMinutes = new Vector2(num2, num3);
			ScoreInterval value = scoreInterval;
			scoresInterval[i] = value;
		}
	}

	[Button(ButtonSizes.Small)]
	public void PropagateIntervalModificationInNormal()
	{
		PropagateIntervalModification(ScoresByMinutesInNormal);
	}

	[Button(ButtonSizes.Small)]
	public void PropagateIntervalModificationInHard()
	{
		PropagateIntervalModification(ScoresByMinutesInHard);
	}
}
