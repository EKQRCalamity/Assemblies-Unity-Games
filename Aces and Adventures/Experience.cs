using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

[ProtoContract]
public class Experience
{
	[ProtoMember(1)]
	private int _experience;

	[ProtoMember(2, OverwriteList = true)]
	private Dictionary<uint, int> _characterLevels;

	public bool enabled
	{
		get
		{
			if (_experience <= 0)
			{
				return characterLevels.Values.Any((int i) => i > 0);
			}
			return true;
		}
	}

	private Dictionary<uint, int> characterLevels => _characterLevels ?? (_characterLevels = new Dictionary<uint, int>());

	public int totalLevel
	{
		get
		{
			int num = 0;
			foreach (int value in characterLevels.Values)
			{
				num += value;
			}
			return num;
		}
	}

	public int experience
	{
		get
		{
			return _experience;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _experience, value))
			{
				Experience.OnExperienceChange?.Invoke();
			}
		}
	}

	public int totalExperience => experience + totalLevel * 100;

	public int pendingLevelUps => _experience / 100;

	public int levelUpOverflow => _experience % 100;

	public int toNextLevel => 100 - levelUpOverflow;

	public int currentVialXP
	{
		get
		{
			if (pendingLevelUps <= 0)
			{
				return levelUpOverflow;
			}
			return 100;
		}
	}

	public int packsOpened => totalLevel + pendingLevelUps + (levelUpOverflow >= 50).ToInt();

	public static event Action OnExperienceChange;

	public bool CanLevelUp(DataRef<CharacterData> characterDataRef)
	{
		if (pendingLevelUps > 0)
		{
			return GetLevelWithRebirth(characterDataRef) < 30;
		}
		return false;
	}

	public bool CanLevelUp()
	{
		if (pendingLevelUps > 0)
		{
			return DataRef<CharacterData>.All.Any(CanLevelUp);
		}
		return false;
	}

	public void LevelUp(DataRef<CharacterData> characterDataRef)
	{
		characterLevels[characterDataRef] = characterLevels.GetValueOrDefault(characterDataRef) + 1;
		_experience -= 100;
		AchievementData.SignalLevelUp(characterDataRef, EnumUtil<RebirthLevel>.Round(GetRebirth(characterDataRef)), GetLevelWithRebirth(characterDataRef));
	}

	public int GetLevel(DataRef<CharacterData> characterDataRef)
	{
		return Math.Min(characterLevels.GetValueOrDefault(characterDataRef), 30);
	}

	public int GetEffectiveLevel(DataRef<CharacterData> characterDataRef)
	{
		return Math.Min(ProfileManager.options.rebirth.GetLevelOverride(characterDataRef) ?? characterLevels.GetValueOrDefault(characterDataRef), 30);
	}

	public int GetLevelWithRebirth(DataRef<CharacterData> characterDataRef)
	{
		return Math.Min(characterLevels.GetValueOrDefault(characterDataRef) % 31, 30);
	}

	public bool CanRebirth(DataRef<CharacterData> characterDataRef)
	{
		if (pendingLevelUps > 0 && GetLevelWithRebirth(characterDataRef) == 30)
		{
			return GetRebirth(characterDataRef) < 2;
		}
		return false;
	}

	public int GetRebirth(DataRef<CharacterData> characterDataRef)
	{
		return Math.Min((characterLevels.GetValueOrDefault(characterDataRef) - characterLevels.GetValueOrDefault(characterDataRef) / 30) / 30, 2);
	}

	public bool HasRebirthOptions()
	{
		return DataRef<CharacterData>.All.Any(CanOverrideTraitRuleSet);
	}

	public bool HasRebirth2Options()
	{
		return DataRef<CharacterData>.All.Any(CanOverrideLevel);
	}

	public bool CanOverrideTraitRuleSet(DataRef<CharacterData> characterDataRef)
	{
		int rebirth = GetRebirth(characterDataRef);
		if (rebirth <= 1)
		{
			if (rebirth == 1)
			{
				return GetLevelWithRebirth(characterDataRef) == 30;
			}
			return false;
		}
		return true;
	}

	public bool CanOverrideLevel(DataRef<CharacterData> characterDataRef)
	{
		int rebirth = GetRebirth(characterDataRef);
		if (rebirth <= 2)
		{
			if (rebirth == 2)
			{
				return GetLevelWithRebirth(characterDataRef) == 30;
			}
			return false;
		}
		return true;
	}

	public bool HasRebirthed()
	{
		return DataRef<CharacterData>.All.Any((DataRef<CharacterData> c) => GetRebirth(c) > 0);
	}

	public bool IsHighestLevel(DataRef<CharacterData> characterDataRef)
	{
		int num = characterLevels[characterDataRef];
		foreach (KeyValuePair<uint, int> characterLevel in characterLevels)
		{
			if (characterLevel.Value >= num && characterLevel.Key != (uint)characterDataRef)
			{
				return false;
			}
		}
		return true;
	}
}
