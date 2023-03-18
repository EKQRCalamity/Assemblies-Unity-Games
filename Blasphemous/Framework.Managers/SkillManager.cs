using System;
using System.Collections.Generic;
using System.Linq;
using Framework.FrameworkCore;
using I2.Loc;
using UnityEngine;

namespace Framework.Managers;

public class SkillManager : GameSystem, PersistentInterface
{
	[Serializable]
	public class SkillsPersistenceData : PersistentManager.PersistentData
	{
		public Dictionary<string, bool> Skills = new Dictionary<string, bool>();

		public SkillsPersistenceData()
			: base("ID_SKILLS")
		{
		}
	}

	public static readonly string[] LANGUAGE_ELEMENT_LIST = new string[3] { "caption", "description", "instructions" };

	private const string SKILL_RESOUCE_DIR = "Skill/";

	private const string SKILL_RESOUCE_CONFIG = "Skill/TIER_CONFIG";

	private Dictionary<string, UnlockableSkill> allSkills = new Dictionary<string, UnlockableSkill>();

	private Dictionary<int, List<UnlockableSkill>> tierSkills = new Dictionary<int, List<UnlockableSkill>>();

	private static string currentLanguage = string.Empty;

	private UnlockableSkillConfiguration TierConfiguration;

	private const string PERSITENT_ID = "ID_SKILLS";

	public override void Start()
	{
		I2.Loc.LocalizationManager.OnLocalizeEvent += OnLocalizationChange;
		LoadAllSkills();
	}

	private void LoadAllSkills(bool resetSkillUnlock = false)
	{
		TierConfiguration = Resources.Load<UnlockableSkillConfiguration>("Skill/TIER_CONFIG");
		if (!TierConfiguration)
		{
			Debug.LogError("Skill Manager: Config file NOT found: Skill/TIER_CONFIG");
		}
		UnlockableSkill[] array = Resources.LoadAll<UnlockableSkill>("Skill/");
		allSkills.Clear();
		tierSkills.Clear();
		UnlockableSkill[] array2 = array;
		foreach (UnlockableSkill unlockableSkill in array2)
		{
			allSkills[unlockableSkill.id] = unlockableSkill;
			if (resetSkillUnlock)
			{
				allSkills[unlockableSkill.id].unlocked = false;
			}
			if (!tierSkills.ContainsKey(unlockableSkill.tier))
			{
				tierSkills[unlockableSkill.tier] = new List<UnlockableSkill>();
			}
			tierSkills[unlockableSkill.tier].Add(unlockableSkill);
		}
		Log.Debug("Skills", allSkills.Count + " skills loaded succesfully.");
		currentLanguage = string.Empty;
		OnLocalizationChange();
	}

	public override void AllInitialized()
	{
		Core.Persistence.AddPersistentManager(this);
	}

	public int GetMaxSkillsTier()
	{
		return tierSkills.Keys.Max((int x) => x);
	}

	public UnlockableSkill GetSkill(string skill)
	{
		if (allSkills.ContainsKey(skill))
		{
			return allSkills[skill];
		}
		return null;
	}

	public List<UnlockableSkill> GetSkillsByTier(int tier)
	{
		if (tierSkills.ContainsKey(tier))
		{
			return tierSkills[tier];
		}
		return new List<UnlockableSkill>();
	}

	public int GetLockedSkillsNumber()
	{
		return allSkills.Values.Where((UnlockableSkill p) => !p.unlocked).Count();
	}

	public int GetUnLockedSkillsNumber()
	{
		return allSkills.Values.Where((UnlockableSkill p) => p.unlocked).Count();
	}

	public bool IsSkillUnlocked(string skill)
	{
		bool result = false;
		if (allSkills.ContainsKey(skill))
		{
			result = allSkills[skill].unlocked;
		}
		return result;
	}

	public bool CanUnlockSkill(string skill, bool ignoreChecks = false)
	{
		bool flag = false;
		if (allSkills.ContainsKey(skill))
		{
			flag = !allSkills[skill].unlocked;
			if (flag && !ignoreChecks)
			{
				flag = CanUnlockSkillNoCheckPoints(skill) && Core.Logic.Penitent.Stats.Purge.Current >= (float)allSkills[skill].cost;
			}
		}
		return flag;
	}

	public bool CanUnlockSkillNoCheckPoints(string skill)
	{
		bool flag = false;
		if (allSkills.ContainsKey(skill))
		{
			flag = !allSkills[skill].unlocked && GetCurrentMeaCulpa() >= (float)allSkills[skill].tier;
			string parentSkill = allSkills[skill].GetParentSkill();
			if (flag && parentSkill != string.Empty)
			{
				if (allSkills.ContainsKey(parentSkill))
				{
					flag = allSkills[parentSkill].unlocked;
				}
				else
				{
					Debug.Log("SkillManager: " + skill + "  Parent is " + parentSkill + " that can be found");
				}
			}
		}
		return flag;
	}

	public bool UnlockSkill(string skill, bool ignoreChecks = false)
	{
		bool flag = CanUnlockSkill(skill, ignoreChecks);
		if (flag)
		{
			allSkills[skill].unlocked = true;
			Core.Logic.Penitent.Stats.Purge.Current -= allSkills[skill].cost;
		}
		return flag;
	}

	public bool LockSkill(string skill)
	{
		bool result = false;
		if (allSkills.ContainsKey(skill))
		{
			result = allSkills[skill].unlocked;
			allSkills[skill].unlocked = false;
		}
		return result;
	}

	public float GetPurgePoints()
	{
		return Core.Logic.Penitent.Stats.Purge.Current;
	}

	public void AddPurgePoints(float points)
	{
		Core.Logic.Penitent.Stats.Purge.Current += points;
	}

	public float GetCurrentMeaCulpa()
	{
		return Core.Logic.Penitent.Stats.MeaCulpa.Final;
	}

	private void OnLocalizationChange()
	{
		if (!(currentLanguage != I2.Loc.LocalizationManager.CurrentLanguage))
		{
			return;
		}
		if (currentLanguage != string.Empty)
		{
			Log.Debug("Skills", "Language change, localize items to: " + I2.Loc.LocalizationManager.CurrentLanguage);
		}
		currentLanguage = I2.Loc.LocalizationManager.CurrentLanguage;
		LanguageSource mainLanguageSource = LocalizationManager.GetMainLanguageSource();
		int languageIndexFromCode = mainLanguageSource.GetLanguageIndexFromCode(I2.Loc.LocalizationManager.CurrentLanguageCode);
		foreach (UnlockableSkill value in allSkills.Values)
		{
			string[] lANGUAGE_ELEMENT_LIST = LANGUAGE_ELEMENT_LIST;
			foreach (string text in lANGUAGE_ELEMENT_LIST)
			{
				string text2 = value.GetBaseTranslationID() + "_" + text.ToUpper();
				TermData termData = mainLanguageSource.GetTermData(text2);
				if (termData == null)
				{
					Debug.LogError("Term " + text2 + " not found in Inventory Localization");
					continue;
				}
				string text3 = termData.Languages[languageIndexFromCode];
				switch (text)
				{
				case "caption":
					value.caption = text3;
					break;
				case "description":
					value.description = text3;
					break;
				case "instructions":
					value.instructions = text3;
					break;
				}
			}
		}
	}

	public int GetOrder()
	{
		return 1;
	}

	public string GetPersistenID()
	{
		return "ID_SKILLS";
	}

	public void ResetPersistence()
	{
		LoadAllSkills(resetSkillUnlock: true);
	}

	public PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		SkillsPersistenceData skillsPersistenceData = new SkillsPersistenceData();
		foreach (UnlockableSkill value in allSkills.Values)
		{
			skillsPersistenceData.Skills[value.id] = value.unlocked;
		}
		return skillsPersistenceData;
	}

	public void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		SkillsPersistenceData skillsPersistenceData = (SkillsPersistenceData)data;
		foreach (KeyValuePair<string, bool> skill in skillsPersistenceData.Skills)
		{
			allSkills[skill.Key].unlocked = skill.Value;
		}
	}
}
