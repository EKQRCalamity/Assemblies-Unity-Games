using System;
using System.Collections.Generic;
using UnityEngine;

public static class LocalAchievementsManager
{
	public enum Achievement
	{
		DefeatBoss,
		ParryApprentice,
		ParryMaster,
		ExWin,
		SuperWin,
		ParryChain,
		NoHitsTaken,
		ARankWorld1,
		ARankWorld2,
		ARankWorld3,
		CompleteWorld1,
		CompleteWorld2,
		CompleteWorld3,
		UnlockedAllSupers,
		FoundAllLevelMoney,
		BoughtAllItems,
		CompleteDicePalace,
		ARankWorld4,
		GoodEnding,
		SRank,
		NewGamePlus,
		FoundSecretPassage,
		SmallPlaneOnlyWin,
		FoundAllMoney,
		PacifistRun,
		NoHitsTakenDicePalace,
		BadEnding,
		CompleteDevil,
		CompleteWorldDLC,
		ARankWorldDLC,
		DefeatBossAsChalice,
		DefeatXBossesAsChalice,
		ChaliceSuperWin,
		DefeatBossDLCWeapon,
		DefeatAllKOG,
		DefeatKOGGauntlet,
		DefeatSaltbaker,
		SRankAnyDLC,
		DefeatBossNoMinions,
		HP9,
		DefeatDevilPhase2,
		Paladin
	}

	[Serializable]
	private class AchievementData
	{
		public List<Achievement> unlockedAchievements = new List<Achievement>();

		public int parryCount;
	}

	private static readonly string CloudKey = "cuphead_ach";

	public static readonly Achievement[] DLCAchievements = new Achievement[14]
	{
		Achievement.CompleteWorldDLC,
		Achievement.ARankWorldDLC,
		Achievement.DefeatBossAsChalice,
		Achievement.DefeatXBossesAsChalice,
		Achievement.ChaliceSuperWin,
		Achievement.DefeatBossDLCWeapon,
		Achievement.DefeatAllKOG,
		Achievement.DefeatKOGGauntlet,
		Achievement.DefeatSaltbaker,
		Achievement.SRankAnyDLC,
		Achievement.DefeatBossNoMinions,
		Achievement.HP9,
		Achievement.DefeatDevilPhase2,
		Achievement.Paladin
	};

	private static bool initialized;

	private static AchievementData achievementData;

	public static event Action<Achievement> AchievementUnlockedEvent;

	public static void Initialize()
	{
		if (!initialized)
		{
			initialized = true;
			loadFromCloud();
		}
	}

	public static void UnlockAchievement(PlayerId playerId, string achievementName)
	{
		Achievement achievement = (Achievement)Enum.Parse(typeof(Achievement), achievementName);
		if (!IsAchievementUnlocked(achievement))
		{
			achievementData.unlockedAchievements.Add(achievement);
			saveToCloud();
			if (LocalAchievementsManager.AchievementUnlockedEvent != null)
			{
				LocalAchievementsManager.AchievementUnlockedEvent(achievement);
			}
		}
	}

	public static void IncrementStat(PlayerId player, string id, int value)
	{
		if (id == "Parries" && achievementData.parryCount < 100)
		{
			achievementData.parryCount += value;
			bool flag = true;
			if (achievementData.parryCount >= 20)
			{
				UnlockAchievement(PlayerId.Any, "ParryApprentice");
				flag = false;
			}
			if (achievementData.parryCount >= 100)
			{
				UnlockAchievement(PlayerId.Any, "ParryMaster");
				flag = false;
			}
			if (flag)
			{
				saveToCloud();
			}
		}
	}

	public static IList<Achievement> GetUnlockedAchievements()
	{
		return achievementData.unlockedAchievements;
	}

	public static bool IsAchievementUnlocked(Achievement achievement)
	{
		return achievementData.unlockedAchievements.Contains(achievement);
	}

	public static bool IsHiddenAchievement(Achievement achievement)
	{
		return achievement == Achievement.FoundSecretPassage || achievement == Achievement.SmallPlaneOnlyWin || achievement == Achievement.FoundAllMoney || achievement == Achievement.PacifistRun || achievement == Achievement.NoHitsTakenDicePalace || achievement == Achievement.BadEnding || achievement == Achievement.CompleteDevil || achievement == Achievement.DefeatDevilPhase2 || achievement == Achievement.Paladin;
	}

	private static void saveToCloud()
	{
		if (OnlineManager.Instance.Interface.CloudStorageInitialized)
		{
			string value = JsonUtility.ToJson(achievementData);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary[CloudKey] = value;
			OnlineManager.Instance.Interface.SaveCloudData(dictionary, onSavedCloudData);
		}
	}

	private static void onSavedCloudData(bool success)
	{
	}

	private static void loadFromCloud()
	{
		if (OnlineManager.Instance.Interface.CloudStorageInitialized)
		{
			OnlineManager.Instance.Interface.LoadCloudData(new string[1] { CloudKey }, onLoadedCloudData);
		}
	}

	private static void onLoadedCloudData(string[] data, CloudLoadResult result)
	{
		if (result == CloudLoadResult.Failed)
		{
			loadFromCloud();
			return;
		}
		try
		{
			if (result == CloudLoadResult.NoData)
			{
				achievementData = new AchievementData();
				saveToCloud();
			}
			else
			{
				achievementData = JsonUtility.FromJson<AchievementData>(data[0]);
			}
		}
		catch (ArgumentException)
		{
			achievementData = new AchievementData();
		}
	}
}
