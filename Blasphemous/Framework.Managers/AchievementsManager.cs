using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Achievements;
using Framework.FrameworkCore;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Framework.Managers;

public class AchievementsManager : GameSystem, PersistentInterface
{
	[Serializable]
	public class AchievementPersistenceData : PersistentManager.PersistentData
	{
		public List<Achievement> achievements = new List<Achievement>();

		public AchievementPersistenceData()
			: base("ID_ACHIEVEMENTS_MANAGER")
		{
		}
	}

	private IAchievementsHelper helper;

	public Dictionary<string, Achievement> Achievements;

	private const int TOTAL_NUMBER_OF_ENEMY_DEATHS_FOR_AC41 = 666;

	private const int TOTAL_NUMBER_OF_BODIES_FOR_AC14 = 26;

	private AC39Enemies ac39EnemiesData;

	private const string AC39_FIX_FLAG = "AC39_FIX";

	public readonly List<string> AC21DomainAndZones = new List<string>
	{
		"D01Z01", "D01Z02", "D01Z03", "D01Z04", "D01Z05", "D02Z01", "D02Z02", "D02Z03", "D03Z01", "D03Z02",
		"D03Z03", "D04Z01", "D04Z02", "D04Z03", "D05Z01", "D05Z02", "D06Z01", "D07Z01", "D08Z01", "D08Z02",
		"D09Z01", "D17Z01"
	};

	private const string ACHIEVEMENTS_PERSITENT_ID = "ID_ACHIEVEMENTS_MANAGER";

	public bool ShowPopUp { get; set; }

	public override void AllInitialized()
	{
		ShowPopUp = false;
		Core.Persistence.AddPersistentManager(this);
	}

	private IAchievementsHelper createHelper()
	{
		return new SteamAchievementsHelper();
	}

	private void CreateAchievements()
	{
		Achievements = new Dictionary<string, Achievement>();
		ResetPersistence();
	}

	public override void Initialize()
	{
		CreateAchievements();
		helper = createHelper();
		Entity.Death += AddProgressToAC39;
		Entity.Death += AddProgressToAC41;
		ac39EnemiesData = Resources.Load<AC39Enemies>("Achievements/AC39_ENEMIES_DATA");
	}

	public override void Dispose()
	{
		Entity.Death -= AddProgressToAC39;
		Entity.Death -= AddProgressToAC41;
	}

	public void AddAchievementProgress(string achievementId, float progress)
	{
		if (Core.GameModeManager.ShouldProgressAchievements())
		{
			if (Achievements.ContainsKey(achievementId.ToUpper()))
			{
				Achievements[achievementId.ToUpper()].AddProgress(progress);
				Debug.Log("AddAchievementProgress: achievement with id: " + achievementId.ToUpper() + " has been added a progress of: " + progress + " and now has a total of: " + Achievements[achievementId.ToUpper()].Progress);
			}
			else
			{
				Debug.Log("AddAchievementProgress: Achievements does not contains achievement with id: " + achievementId.ToUpper());
			}
		}
	}

	public void GrantAchievement(string achievementId)
	{
		if (!Core.GameModeManager.ShouldProgressAchievements())
		{
			return;
		}
		if (Achievements.ContainsKey(achievementId.ToUpper()))
		{
			if (Achievements[achievementId.ToUpper()].IsGranted())
			{
				Debug.Log("GrantAchievement: achievement with id: " + achievementId.ToUpper() + " try to grant but it was granted.");
				return;
			}
			Achievements[achievementId.ToUpper()].Grant();
			Debug.Log("GrantAchievement: achievement with id: " + achievementId.ToUpper() + " has been granted.");
		}
		else
		{
			Debug.Log("GrantAchievement: Achievements does not contains achievement with id: " + achievementId.ToUpper());
		}
	}

	public bool CheckAchievementGranted(string achievementId)
	{
		if (Achievements.ContainsKey(achievementId.ToUpper()))
		{
			if (Achievements[achievementId.ToUpper()].IsGranted())
			{
				Debug.Log("CheckAchievementGranted: achievement with id: " + achievementId.ToUpper() + " is granted.");
			}
			else
			{
				Debug.Log("CheckAchievementGranted: achievement with id: " + achievementId.ToUpper() + " is not granted.");
			}
		}
		else
		{
			Debug.Log("CheckAchievementGranted: Achievements does not contains achievement with id: " + achievementId.ToUpper());
		}
		return Achievements[achievementId.ToUpper()].IsGranted();
	}

	public float CheckAchievementProgress(string achievementId)
	{
		if (Achievements.ContainsKey(achievementId.ToUpper()))
		{
			Debug.Log("CheckAchievementProgress: achievement with id: " + achievementId.ToUpper() + " has a progress of: " + Achievements[achievementId.ToUpper()].Progress);
			return Achievements[achievementId.ToUpper()].Progress;
		}
		Debug.Log("CheckAchievementProgress: Achievements does not contains achievement with id: " + achievementId.ToUpper());
		return 0f;
	}

	public void DebugResetAchievement(string achievementId)
	{
		if (Achievements.ContainsKey(achievementId.ToUpper()))
		{
			Achievements[achievementId.ToUpper()].Reset();
			Debug.Log("DebugResetAchievement: achievement with id: " + achievementId.ToUpper() + " has been reset.");
		}
		else
		{
			Debug.Log("DebugResetAchievement: Achievements does not contains achievement with id: " + achievementId.ToUpper());
		}
	}

	public void PrepareForNewGamePlus()
	{
		foreach (Achievement item in Achievements.Values.Where((Achievement a) => !a.IsGranted() && !a.PreserveProgressInNewGamePlus))
		{
			Debug.Log("Reset achievement " + item.Id + " for a new game plus");
			item.Progress = 0f;
		}
	}

	public void CheckProgressToAC46()
	{
		Achievement achievement = Achievements["AC46"];
		if (!achievement.IsGranted())
		{
			float num = Core.Persistence.PercentCompleted - achievement.Progress;
			if (num > 0f)
			{
				achievement.AddProgress(num);
			}
		}
	}

	public void AddProgressToAC39(Entity entity)
	{
		if (Core.AchievementsManager.Achievements["AC39"].IsGranted())
		{
			return;
		}
		Enemy enemy = entity as Enemy;
		if ((bool)enemy && !string.IsNullOrEmpty(enemy.Id) && ac39EnemiesData.EnemiesList.Exists((EnemyIdAndName x) => x.id.Equals(enemy.Id)))
		{
			CheckAndApplyAC39Fix();
			string newSystemFlagName = GetNewSystemFlagName(enemy.Id);
			if (!Core.Events.GetFlag(newSystemFlagName))
			{
				Core.Events.SetFlag(newSystemFlagName, b: true, forcePreserve: true);
				Core.AchievementsManager.Achievements["AC39"].AddProgress(100f / (float)ac39EnemiesData.EnemiesList.Count);
			}
		}
	}

	private void RecalculateAC39Progress()
	{
		Core.AchievementsManager.Achievements["AC39"].Progress = 0f;
		foreach (EnemyIdAndName enemies in ac39EnemiesData.EnemiesList)
		{
			string oldSystemFlagName = GetOldSystemFlagName(enemies.name);
			if (!Core.Events.GetFlag(oldSystemFlagName) && enemies.hasAnotherName)
			{
				oldSystemFlagName = GetOldSystemFlagName(enemies.otherName);
			}
			if (Core.Events.GetFlag(oldSystemFlagName))
			{
				string newSystemFlagName = GetNewSystemFlagName(enemies.id);
				Core.Events.SetFlag(newSystemFlagName, b: true, forcePreserve: true);
				Core.AchievementsManager.Achievements["AC39"].AddProgress(100f / (float)ac39EnemiesData.EnemiesList.Count);
			}
		}
	}

	private string GetOldSystemFlagName(string enemyName)
	{
		string text = enemyName.ToUpper().Trim();
		return text + "_KILLED";
	}

	private string GetNewSystemFlagName(string enemyId)
	{
		return enemyId.ToUpper() + "_KILLED";
	}

	private void CheckAndApplyAC39Fix()
	{
		if (!Core.Events.GetFlag("AC39_FIX"))
		{
			RecalculateAC39Progress();
			Core.Events.SetFlag("AC39_FIX", b: true, forcePreserve: true);
		}
	}

	public void AddProgressToAC41(Entity entity)
	{
		if (!Core.AchievementsManager.Achievements["AC41"].IsGranted())
		{
			Enemy enemy = entity as Enemy;
			if ((bool)enemy && !enemy.gameObject.CompareTag("CherubCaptor"))
			{
				Core.AchievementsManager.Achievements["AC41"].AddProgress(0.15015015f);
			}
		}
	}

	public void CheckFlagsToGrantAC19()
	{
		bool flag = true;
		for (int i = 1; i <= 44; i++)
		{
			string id = $"CO{i:00}_OWNED";
			if (!Core.Events.GetFlag(id))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			GrantAchievement("AC19");
		}
	}

	public static void CheckFlagsToGrantAC14()
	{
		bool flag = true;
		List<string> list = new List<string>();
		for (int i = 1; i <= 26; i++)
		{
			list.Add($"CORPSES_{i:00}_FINISHED");
		}
		foreach (string item in list)
		{
			if (!Core.Events.GetFlag(item))
			{
				flag = false;
			}
		}
		if (flag)
		{
			Core.AchievementsManager.GrantAchievement("AC14");
		}
	}

	public void AddProgressToAC21(string domain, string zone)
	{
		string domainAndZone = domain.ToUpper() + zone.ToUpper();
		if (AC21DomainAndZones.Exists((string x) => x.Equals(domainAndZone)))
		{
			string id = "ZONE_NAME_OF_" + domainAndZone + "_DISPLAYED";
			if (!Core.Events.GetFlag(id))
			{
				Core.Events.SetFlag(id, b: true);
				Core.AchievementsManager.Achievements["AC21"].AddProgress(100f / (float)AC21DomainAndZones.Count);
			}
		}
	}

	public void SetAchievementProgress(string achievementId, float progress)
	{
		if (helper != null)
		{
			Debug.Log("SetAchievementProgress: ID: " + achievementId + "  Progress:" + progress);
			helper.SetAchievementProgress(achievementId, progress);
			if (progress >= 100f)
			{
				LocalAchievementsHelper.SetAchievementUnlocked(achievementId);
			}
		}
	}

	public void GetAchievementProgress(string achievementId, GetAchievementOperationEvent evt)
	{
		if (helper != null)
		{
			Debug.Log("GetAchievementProgress: string: " + achievementId);
			helper.GetAchievementProgress(achievementId, evt);
		}
	}

	public void DebugReset()
	{
		foreach (string key in Achievements.Keys)
		{
			Achievements[key].Reset();
		}
	}

	public override void OnGUI()
	{
		DebugResetLine();
		DebugDrawTextLine("Achievement Manager -------------------------------------");
		string text = string.Empty;
		foreach (Achievement item in Achievements.Values.OrderBy((Achievement p) => p.Id))
		{
			string text2 = item.Id + ": " + item.Progress.ToString("0.##");
			if (text == string.Empty)
			{
				text = text2;
				continue;
			}
			DebugDrawTextLine($"{text,-15}" + text2);
			text = string.Empty;
		}
		if (text != string.Empty)
		{
			DebugDrawTextLine(text);
		}
	}

	public List<Achievement> GetAllAchievements()
	{
		CreateAchievements();
		List<string> localAchievementIds = LocalAchievementsHelper.GetLocalAchievementIds();
		foreach (string key in Achievements.Keys)
		{
			if (localAchievementIds.Contains(key))
			{
				Achievements[key].CurrentStatus = Achievement.Status.UNLOCKED;
			}
			else
			{
				Achievements[key].CurrentStatus = (Achievements[key].CanBeHidden ? Achievement.Status.HIDDEN : Achievement.Status.LOCKED);
			}
		}
		List<Achievement> list = Achievements.Values.ToList();
		list.Sort((Achievement ach1, Achievement ach2) => ach1.Id.CompareTo(ach2.Id));
		return list;
	}

	public int GetOrder()
	{
		return 0;
	}

	public string GetPersistenID()
	{
		return "ID_ACHIEVEMENTS_MANAGER";
	}

	public PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		AchievementPersistenceData achievementPersistenceData = new AchievementPersistenceData();
		Achievement[] array = new Achievement[Achievements.Values.Count];
		Achievements.Values.CopyTo(array, 0);
		achievementPersistenceData.achievements = array.ToList();
		return achievementPersistenceData;
	}

	public void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		AchievementPersistenceData achievementPersistenceData = (AchievementPersistenceData)data;
		foreach (Achievement achievement2 in achievementPersistenceData.achievements)
		{
			if (Achievements.ContainsKey(achievement2.Id))
			{
				Sprite image = Achievements[achievement2.Id].Image;
				achievement2.PreserveProgressInNewGamePlus = achievement2.PreserveProgressInNewGamePlus || Achievements[achievement2.Id].PreserveProgressInNewGamePlus;
				Achievements[achievement2.Id] = achievement2;
				Achievements[achievement2.Id].Image = image;
			}
			else
			{
				Debug.LogWarning("***** AchievementsManager: SetCurrentPersistentState Achievement " + achievement2.Id + " NOT found, creating a new one");
				Achievement achievement = new Achievement(achievement2.Id);
				achievement.Progress = achievement2.Progress;
				Achievements[achievement2.Id] = achievement;
			}
		}
	}

	public void ResetPersistence()
	{
		Achievements.Clear();
		AchievementList[] array = Resources.LoadAll<AchievementList>("Achievements/");
		AchievementList[] array2 = array;
		foreach (AchievementList achievementList in array2)
		{
			foreach (Achievement achievement2 in achievementList.achievementList)
			{
				Achievement achievement = new Achievement(achievement2);
				achievement.CurrentStatus = Achievement.Status.LOCKED;
				Achievements[achievement.Id] = achievement;
			}
		}
	}
}
