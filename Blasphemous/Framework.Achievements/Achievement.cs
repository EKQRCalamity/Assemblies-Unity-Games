using System;
using Framework.Managers;
using Gameplay.UI;
using UnityEngine;

namespace Framework.Achievements;

[Serializable]
public class Achievement
{
	public enum Status
	{
		LOCKED,
		UNLOCKED,
		HIDDEN
	}

	public string Id;

	public float Progress;

	public string Name;

	public string Description;

	public Sprite Image;

	[HideInInspector]
	public Status CurrentStatus;

	public bool CanBeHidden;

	public bool PreserveProgressInNewGamePlus;

	public Achievement(Achievement other)
	{
		Id = other.Id;
		Progress = other.Progress;
		Name = other.Name;
		Description = other.Description;
		PreserveProgressInNewGamePlus = other.PreserveProgressInNewGamePlus;
		Image = other.Image;
		CurrentStatus = other.CurrentStatus;
		CanBeHidden = other.CanBeHidden;
	}

	public Achievement(string id)
	{
		Id = id;
	}

	public string GetNameLocalizationTerm()
	{
		return GetLocalizationBase() + "_NAME";
	}

	public string GetDescLocalizationTerm()
	{
		return GetLocalizationBase() + "_DESC";
	}

	public void AddProgress(float progress)
	{
		if (!IsGranted() && Core.GameModeManager.ShouldProgressAchievements())
		{
			Progress += progress;
			if (Progress >= 99.99f)
			{
				Progress = 100f;
			}
			SetAchievementProgress(Progress);
		}
	}

	public void AddProgressSafeTo99(float progress)
	{
		if (!IsGranted() && Core.GameModeManager.ShouldProgressAchievements())
		{
			Progress += progress;
			if (Progress >= 99.9f)
			{
				Progress = 100f;
			}
			SetAchievementProgress(Progress);
		}
	}

	public void Grant()
	{
		if (!IsGranted() && Core.GameModeManager.ShouldProgressAchievements())
		{
			Progress = 100f;
			SetAchievementProgress(Progress);
		}
	}

	private void SetAchievementProgress(float progress)
	{
		Core.AchievementsManager.SetAchievementProgress(Id, progress);
		if (IsGranted())
		{
			UIController.instance.ShowPopupAchievement(this);
		}
	}

	public bool IsGranted()
	{
		return Progress == 100f;
	}

	public void Reset()
	{
		Progress = 0f;
	}

	private string GetLocalizationBase()
	{
		return "Achievements/" + Id;
	}
}
