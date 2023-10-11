using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField]
public class CharacterData : CombatantData
{
	[ProtoContract]
	[UIField]
	[ProtoInclude(5, typeof(UnlockAbilityLevelUp))]
	[ProtoInclude(6, typeof(StatisticLevelUp))]
	[ProtoInclude(7, typeof(PlayerStatisticLevelUp))]
	[ProtoInclude(8, typeof(GameStateParameterLevelUp))]
	[ProtoInclude(9, typeof(UnlockAbilityByCategoryAndRankLevelUp))]
	public abstract class ALevelUp
	{
		[ProtoContract]
		[UIField("Unlock Ability", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		public class UnlockAbilityLevelUp : ALevelUp
		{
			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Open)]
			private DataRef<AbilityData> _abilityToUnlock;

			public override DataRef<AbilityData> GetUnlockedAbility(PlayerClass characterClass)
			{
				return _abilityToUnlock;
			}

			public override ATarget GenerateCard(PlayerClass characterClass)
			{
				if (!_abilityToUnlock)
				{
					return null;
				}
				return new Ability(_abilityToUnlock);
			}

			public override string ToString()
			{
				return "Unlock <b>" + _abilityToUnlock.GetFriendlyName() + "</b> Ability";
			}
		}

		[ProtoContract]
		[UIField("Statistic", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		public class StatisticLevelUp : ALevelUp
		{
			[ProtoMember(1)]
			[UIField]
			[UIHorizontalLayout("A", expandHeight = false)]
			private StatType _stat;

			[ProtoMember(2)]
			[UIField(min = 1, max = 5)]
			[UIHorizontalLayout("A")]
			[DefaultValue(1)]
			private int _adjustment = 1;

			public override void Apply(Player player)
			{
				player.stats[_stat].value += _adjustment;
			}

			public override string ToString()
			{
				return $"+{_adjustment} <b>{EnumUtil.FriendlyName(_stat)}</b>";
			}
		}

		[ProtoContract]
		[UIField("Player Statistic", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		public class PlayerStatisticLevelUp : ALevelUp
		{
			[ProtoMember(1)]
			[UIField]
			[UIHorizontalLayout("A", expandHeight = false)]
			private PlayerStatType _stat;

			[ProtoMember(2)]
			[UIField(min = 1, max = 5)]
			[UIHorizontalLayout("A")]
			[DefaultValue(1)]
			private int _adjustment = 1;

			public override void Apply(Player player)
			{
				player.playerStats[_stat].value += _adjustment;
			}

			public override string ToString()
			{
				return $"+{_adjustment} <b>{EnumUtil.FriendlyName(_stat)}</b>";
			}
		}

		[ProtoContract]
		[UIField("Game State Parameter", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		public class GameStateParameterLevelUp : ALevelUp
		{
			[ProtoMember(1)]
			[UIField]
			[UIHorizontalLayout("A", expandHeight = false)]
			private GameState.ParameterType _parameter;

			[ProtoMember(2)]
			[UIField(min = 1, max = 5)]
			[UIHorizontalLayout("A")]
			[DefaultValue(1)]
			private int _adjustment = 1;

			public override void Apply(GameState state)
			{
				state.parameters[_parameter] += _adjustment;
			}

			public override string ToString()
			{
				return $"+{_adjustment} <b>{EnumUtil.FriendlyName(_parameter)}</b>";
			}
		}

		[ProtoContract]
		[UIField("Unlock Ability by Category and Rank", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		public class UnlockAbilityByCategoryAndRankLevelUp : ALevelUp
		{
			[ProtoMember(1)]
			[UIField]
			[UIHorizontalLayout("A")]
			private AbilityData.Category _category;

			[ProtoMember(2)]
			[UIField]
			[UIHorizontalLayout("A")]
			private AbilityData.Rank _rank;

			public override ATarget GenerateCard(PlayerClass characterClass)
			{
				DataRef<AbilityData> unlockedAbility = GetUnlockedAbility(characterClass);
				if (unlockedAbility == null)
				{
					return null;
				}
				return new Ability(unlockedAbility);
			}

			public override DataRef<AbilityData> GetUnlockedAbility(PlayerClass characterClass)
			{
				return AbilityData.GetAbility(characterClass, _category, _rank);
			}

			public override string ToString()
			{
				return "Unlock " + EnumUtil.FriendlyName(_rank) + " " + EnumUtil.FriendlyName(_category);
			}
		}

		public virtual ATarget GenerateCard(PlayerClass characterClass)
		{
			return null;
		}

		public virtual DataRef<AbilityData> GetUnlockedAbility(PlayerClass characterClass)
		{
			return null;
		}

		public virtual void Apply(GameState state)
		{
		}

		public virtual void Apply(Player player)
		{
		}
	}

	public const int XP_PER_LEVEL = 100;

	public const int XP_PER_PACK = 50;

	public const int MAX_LEVEL = 30;

	public const int MAX_LEVEL_CLAMP = 30;

	public const int MAX_REBIRTH = 2;

	[ProtoMember(1)]
	[UIField(order = 1u)]
	[UICategory("Main")]
	private PlayerClass _class;

	[ProtoMember(3, OverwriteList = true)]
	[UIField(maxCount = 30, collapse = UICollapseType.Open)]
	[UIFieldCollectionItem]
	[UICategory("Level Ups")]
	private List<DataRef<LevelUpData>> _levelUpData;

	[ProtoMember(4)]
	[UIField]
	[UICategory("Main")]
	private bool _unlockedForDemo;

	public PlayerClass characterClass => _class;

	public bool unlockedForDemo => _unlockedForDemo;

	protected override bool _hideTraits => _traits.IsNullOrEmpty();

	private bool _hideAutoFillLevelUp => !_levelUpData.IsNullOrEmpty();

	public IEnumerable<ALevelUp> GetLevelUps(int? level = null)
	{
		if (!_levelUpData.IsNullOrEmpty())
		{
			return _levelUpData.Take(level ?? 30).SelectMany((DataRef<LevelUpData> levelUpRef) => levelUpRef.data.levelUps);
		}
		return Enumerable.Empty<ALevelUp>();
	}

	public IEnumerable<ATarget> GetLevelUpCards(int level)
	{
		for (int x = 1; x <= level; x++)
		{
			DataRef<LevelUpData> levelUp = GetLevelUp(x);
			if (levelUp != null && (bool)levelUp)
			{
				yield return levelUp.data.GenerateCard(characterClass);
			}
		}
	}

	public DataRef<LevelUpData> GetLevelUp(int level)
	{
		return _levelUpData.GetValueOrDefault(level - 1);
	}

	public void ApplyLevelUps(Player player, int? level = null)
	{
		foreach (ALevelUp levelUp in GetLevelUps(level))
		{
			levelUp.Apply(player);
		}
	}

	public void ApplyLevelUps(GameState state, int? level = null)
	{
		foreach (ALevelUp levelUp in GetLevelUps(level))
		{
			levelUp.Apply(state);
		}
	}

	public IEnumerable<DataRef<AbilityData>> AbilitiesUnlockedByLevelUps(int? level = null)
	{
		foreach (ALevelUp levelUp in GetLevelUps(level))
		{
			DataRef<AbilityData> unlockedAbility = levelUp.GetUnlockedAbility(characterClass);
			if (unlockedAbility != null)
			{
				yield return unlockedAbility;
			}
		}
	}

	public int? GetUnlockLevelForAbility(DataRef<AbilityData> ability)
	{
		if (!_levelUpData.IsNullOrEmpty())
		{
			for (int i = 0; i < _levelUpData.Count; i++)
			{
				if (_levelUpData[i].data.levelUps.FirstOrDefault((ALevelUp levelUp) => ContentRef.Equal(ability, levelUp.GetUnlockedAbility(characterClass))) != null)
				{
					return i + 1;
				}
			}
		}
		return null;
	}

	[UIField]
	[UIHideIf("_hideAutoFillLevelUp")]
	private void _AutoFillLevelUps()
	{
		_levelUpData = new List<DataRef<LevelUpData>>(ContentRef.Defaults.data.startingCharacter.data._levelUpData);
	}
}
