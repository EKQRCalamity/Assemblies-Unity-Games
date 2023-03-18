using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Framework.Managers;
using FullSerializer;
using Tools;
using UnityEngine;

namespace Framework.Achievements;

public class LocalAchievementsHelper : IAchievementsHelper
{
	private const string LocalAchievementsFilename = "/local_achievements.data";

	private static readonly string LocalFilePath = PersistentManager.GetPathAppSettings("/local_achievements.data");

	private static readonly List<string> LocalAchievementsCache = new List<string>();

	private static bool _initialized;

	public void SetAchievementProgress(string id, float value)
	{
		PlayerPrefs.SetFloat(id, value);
	}

	public void GetAchievementProgress(string id, GetAchievementOperationEvent evt)
	{
		float @float = PlayerPrefs.GetFloat(id, 0f);
		evt(id, @float);
	}

	public static List<string> GetLocalAchievementIds()
	{
		if (!_initialized)
		{
			Initialize();
		}
		return LocalAchievementsCache;
	}

	public static void SetAchievementUnlocked(string id)
	{
		if (!_initialized)
		{
			Initialize();
		}
		if (!LocalAchievementsCache.Contains(id))
		{
			LocalAchievementsCache.Add(id);
			SaveLocalAchievements();
		}
	}

	private static void Initialize()
	{
		if (!LoadLocalAchievementsCache())
		{
			GetAchievementsFromSlots();
			SaveLocalAchievements();
		}
		_initialized = true;
	}

	private static bool LoadLocalAchievementsCache()
	{
		LocalAchievementsCache.Clear();
		if (!File.Exists(LocalFilePath) || new FileInfo(LocalFilePath).Length == 0)
		{
			return false;
		}
		fsData fsData = PersistentManager.ReadAppSettings(LocalFilePath);
		if (fsData.IsList)
		{
			List<fsData> asList = fsData.AsList;
			foreach (fsData item in asList)
			{
				string asString = item.AsString;
				if (!LocalAchievementsCache.Contains(asString))
				{
					LocalAchievementsCache.Add(asString);
				}
			}
		}
		return true;
	}

	private static void GetAchievementsFromSlots()
	{
		for (int i = 0; i < 3; i++)
		{
			PersistentManager.PublicSlotData slotData = Core.Persistence.GetSlotData(i);
			if (slotData == null)
			{
				continue;
			}
			foreach (Achievement achievement in slotData.achievement.achievements)
			{
				if (achievement.Progress >= 100f)
				{
					LocalAchievementsCache.Add(achievement.Id);
				}
			}
		}
	}

	private static void SaveLocalAchievements()
	{
		fsData data = fsData.CreateList(LocalAchievementsCache.Count);
		LocalAchievementsCache.ForEach(delegate(string id)
		{
			data.AsList.Add(new fsData(id));
		});
		string s = fsJsonPrinter.CompressedJson(data);
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		string encryptedData = Convert.ToBase64String(bytes);
		FileTools.SaveSecure(LocalFilePath, encryptedData);
	}
}
