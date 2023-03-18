using System;
using Framework.Managers;
using UnityEngine;

namespace Framework.BossRush;

[Serializable]
public class BossRushHighScore
{
	public BossRushManager.BossRushCourseId CourseId;

	public BossRushManager.BossRushCourseMode CourseMode;

	public int NumFlasksUsed;

	public int NumDodgesAchieved;

	public int NumPrayersUsed;

	public int NumBloodPenancesUsed;

	public int NumHitsReceived;

	public float RunDuration;

	public int NumScenesCompleted;

	public bool IsTimerActive;

	public BossRushManager.BossRushCourseScore Score;

	public bool IsNewHighScore;

	public bool WasTheCourseCompleted;

	public BossRushHighScore(BossRushManager.BossRushCourseId currentCourseId, BossRushManager.BossRushCourseMode currentCourseMode)
	{
		CourseId = currentCourseId;
		CourseMode = currentCourseMode;
	}

	public void CalculateScoreObtained(bool completed)
	{
		WasTheCourseCompleted = completed;
		if (completed)
		{
			int minutes = (int)Math.Ceiling(RunDuration) / 60;
			Score = Core.BossRushManager.GetRankByTimePassed(CourseId, CourseMode, minutes);
		}
		else
		{
			Score = Core.BossRushManager.GetRankByNumberOfCompletedBossfights(CourseId, NumScenesCompleted);
		}
	}

	public string RunDurationInString()
	{
		int num = Mathf.FloorToInt(RunDuration) / 60;
		TimeSpan timeSpan = TimeSpan.FromSeconds(RunDuration);
		int seconds = timeSpan.Seconds;
		int num2 = timeSpan.Milliseconds / 10;
		if (num < 100)
		{
			return $"{num:D2} : {seconds:D2} : {num2:D2}";
		}
		return $"{num:D3} : {seconds:D2} : {num2:D2}";
	}

	public void UpdateTimer()
	{
		if (IsTimerActive)
		{
			RunDuration += Time.deltaTime;
		}
	}

	public bool IsBetterThan(BossRushHighScore other)
	{
		bool flag = true;
		if (other != null)
		{
			Debug.Log("**** IsBetterThan Score");
			Debug.Log(string.Concat("* Base: ", Score, "  Duration:", RunDuration));
			Debug.Log(string.Concat("* Other: ", other.Score, "  Duration:", other.RunDuration));
			flag = ((Score != other.Score) ? (Score > other.Score) : (RunDuration <= other.RunDuration));
			Debug.Log("*** IS BETTER: " + flag);
		}
		return flag;
	}
}
