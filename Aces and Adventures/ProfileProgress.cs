using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoBuf;

[ProtoContract]
public class ProfileProgress
{
	[ProtoContract]
	public class Games
	{
		[ProtoMember(1, OverwriteList = true)]
		private Dictionary<uint, Dictionary<uint, AdventureCompletion>> _completion;

		[ProtoMember(2, OverwriteList = true)]
		private Dictionary<uint, HashSet<uint>> _adventureUnlocks;

		[ProtoMember(3)]
		private LeaderboardProgress _leaderboardProgress;

		private Dictionary<uint, Dictionary<uint, AdventureCompletion>> completion => _completion ?? (_completion = new Dictionary<uint, Dictionary<uint, AdventureCompletion>>());

		private Dictionary<uint, HashSet<uint>> adventureUnlocks => _adventureUnlocks ?? (_adventureUnlocks = new Dictionary<uint, HashSet<uint>>());

		public LeaderboardProgress leaderboardProgress => _leaderboardProgress ?? (_leaderboardProgress = new LeaderboardProgress());

		private IEnumerable<DataRef<AdventureData>> _GetCustomUnlockedAdventures(DataRef<GameData> game)
		{
			HashSet<uint> valueOrDefault = adventureUnlocks.GetValueOrDefault(game);
			if (valueOrDefault == null)
			{
				yield break;
			}
			foreach (uint item in valueOrDefault)
			{
				yield return DataRef<AdventureData>.FromFileId(item);
			}
		}

		private IEnumerable<DataRef<AdventureData>> _GetCustomUnlockedAdventuresAndNextIfCompleted(DataRef<GameData> game)
		{
			foreach (DataRef<AdventureData> adventure in _GetCustomUnlockedAdventures(game))
			{
				yield return adventure;
				DataRef<AdventureData> currentAdventure = adventure;
				while (HasCompleted(game, currentAdventure))
				{
					DataRef<AdventureData> nextAdventure = game.data.GetNextAdventure(currentAdventure);
					if (nextAdventure == null)
					{
						break;
					}
					DataRef<AdventureData> dataRef;
					currentAdventure = (dataRef = nextAdventure);
					if (!dataRef)
					{
						break;
					}
					yield return currentAdventure;
				}
			}
		}

		public void Add(DataRef<GameData> game, DataRef<AdventureData> adventure, PlayerClass playerClass, AdventureCompletion.Data data)
		{
			UnlockGame(game);
			if (!completion[game].ContainsKey(adventure))
			{
				completion[game][adventure] = new AdventureCompletion(playerClass, data);
			}
			else
			{
				completion[game][adventure].Best(playerClass, data);
			}
			AchievementData.SignalAdventureCompleted(game, adventure, adventure.data.GetCompletionRank(data.strategyTime));
		}

		public bool HasCompleted(DataRef<GameData> game, DataRef<AdventureData> adventure)
		{
			return GetCompletion(game, adventure) != null;
		}

		public AdventureCompletion GetCompletion(DataRef<GameData> game, DataRef<AdventureData> adventure)
		{
			return completion.GetValueOrDefault(game)?.GetValueOrDefault(adventure);
		}

		public IEnumerable<DataRef<AdventureData>> GetUnlockedAdventures(DataRef<GameData> game, bool forceUnlock = false)
		{
			if (!forceUnlock && !game.data.unlockAllAdventures)
			{
				return game.data.adventures.TakeWhileAndNext((DataRef<AdventureData> adventure) => HasCompleted(game, adventure)).Concat(_GetCustomUnlockedAdventuresAndNextIfCompleted(game).TakeOrderFrom(game.data.adventures)).Distinct();
			}
			return game.data.adventures;
		}

		public bool IsCustomUnlocked(DataRef<GameData> game, DataRef<AdventureData> adventure)
		{
			return adventureUnlocks.GetValueOrDefault(game)?.Contains(adventure) ?? false;
		}

		public bool IsUnlocked(DataRef<GameData> game, DataRef<AdventureData> adventure)
		{
			if (!game.data.unlockAllAdventures && !IsCustomUnlocked(game, adventure))
			{
				return game.data.HasCompletedUpToAdventure(game, adventure);
			}
			return true;
		}

		public void UnlockAdventure(DataRef<GameData> game, DataRef<AdventureData> adventure)
		{
			(adventureUnlocks.GetValueOrDefault(game) ?? (adventureUnlocks[game] = new HashSet<uint>())).Add(adventure);
		}

		public bool IsUnlocked(DataRef<GameData> game)
		{
			return completion.ContainsKey(game);
		}

		public void UnlockGame(DataRef<GameData> game)
		{
			completion[game] = completion.GetValueOrDefault(game) ?? new Dictionary<uint, AdventureCompletion>();
		}

		public IEnumerable<DataRef<GameData>> GetUnlockedGames(bool forceUnlock = false)
		{
			if (!forceUnlock)
			{
				return completion.Keys.Select(DataRef<GameData>.FromFileId);
			}
			return DataRef<GameData>.All;
		}

		public void CheckForRewardUnlocks()
		{
			DataRef<GameData> selectedGame = ProfileManager.prefs.selectedGame;
			foreach (DataRef<GameData> item in DataRef<GameData>.All)
			{
				ProfileManager.prefs.selectedGame = item;
				foreach (DataRef<AdventureData> adventure in item.data.adventures)
				{
					if (!HasCompleted(item, adventure))
					{
						continue;
					}
					foreach (AReward item2 in adventure.data.GetRewardsThatShouldUnlock())
					{
						item2.GetUnlockStep()?._Unlock();
					}
				}
			}
			ProfileManager.prefs.selectedGame = selectedGame;
		}
	}

	[ProtoContract]
	public class Characters
	{
		[ProtoMember(1, OverwriteList = true)]
		private Dictionary<uint, CharacterProgress> _progress;

		private Dictionary<uint, CharacterProgress> progress => _progress ?? (_progress = new Dictionary<uint, CharacterProgress>());

		public CharacterProgress this[DataRef<CharacterData> character] => progress.GetValueOrDefault(character) ?? (progress[character] = new CharacterProgress());

		public CharacterProgress this[Player player] => this[player.characterDataRef];

		public CharacterProgress this[GameState gameState] => this[gameState.player];

		public bool IsUnlocked(DataRef<CharacterData> characterDataRef)
		{
			return this[characterDataRef].unlocked;
		}

		public bool IsLocked(DataRef<CharacterData> characterDataRef)
		{
			return !IsUnlocked(characterDataRef);
		}

		public void Unlock(DataRef<CharacterData> characterDataRef)
		{
			this[characterDataRef].unlocked = true;
		}

		public IEnumerable<DataRef<CharacterData>> GetUnlockedCharacters(bool forceUnlock = false)
		{
			if (!forceUnlock)
			{
				return DataRef<CharacterData>.Search().Where(IsUnlocked);
			}
			return DataRef<CharacterData>.Search();
		}

		public IEnumerable<DataRef<CharacterData>> GetLockedCharacters(bool forceUnlock = false)
		{
			if (!forceUnlock)
			{
				return DataRef<CharacterData>.Search().Where(IsLocked);
			}
			return Enumerable.Empty<DataRef<CharacterData>>();
		}
	}

	[ProtoContract]
	public class Abilities
	{
		[ProtoMember(1, OverwriteList = true)]
		private Dictionary<uint, int> _abilityCounts;

		[ProtoMember(2, OverwriteList = true)]
		private HashSet<uint> _unlockedDecks;

		private Dictionary<uint, int> abilityCounts => _abilityCounts ?? (_abilityCounts = new Dictionary<uint, int>());

		private HashSet<uint> unlockedDecks => _unlockedDecks ?? (_unlockedDecks = new HashSet<uint>());

		private DataRef<AbilityData> _UnlockRandomAbility(Dictionary<DataRef<AbilityData>, int> missingAbilityCounts, Random random, PlayerClass? preferredCharacter = null, HashSet<DataRef<AbilityData>> preferredAbilities = null)
		{
			if (missingAbilityCounts.Count == 0)
			{
				return null;
			}
			using PoolWRandomDHandle<DataRef<AbilityData>> poolWRandomDHandle = Pools.UseWRandomD<DataRef<AbilityData>>();
			if (preferredAbilities != null && preferredAbilities.Count > 0)
			{
				foreach (DataRef<AbilityData> preferredAbility in preferredAbilities)
				{
					poolWRandomDHandle.value.Add(1f * preferredAbility.GetUnlockWeight(), preferredAbility);
				}
			}
			else if (preferredCharacter.HasValue)
			{
				foreach (KeyValuePair<DataRef<AbilityData>, int> missingAbilityCount in missingAbilityCounts)
				{
					if (missingAbilityCount.Key.data.characterClass == preferredCharacter)
					{
						poolWRandomDHandle.value.Add((float)missingAbilityCount.Value * missingAbilityCount.Key.GetUnlockWeight(), missingAbilityCount.Key);
					}
				}
			}
			if (poolWRandomDHandle.value.Count == 0)
			{
				foreach (KeyValuePair<DataRef<AbilityData>, int> missingAbilityCount2 in missingAbilityCounts)
				{
					poolWRandomDHandle.value.Add((float)missingAbilityCount2.Value * missingAbilityCount2.Key.GetUnlockWeight(), missingAbilityCount2.Key);
				}
			}
			return poolWRandomDHandle.value.Random(random.NextDouble());
		}

		public bool ShouldUnlock(DataRef<AbilityDeckData> deckRef)
		{
			return !unlockedDecks.Contains(deckRef);
		}

		public bool UnlockDeck(DataRef<AbilityDeckData> deckRef)
		{
			if (!unlockedDecks.Add(deckRef))
			{
				return false;
			}
			foreach (KeyValuePair<DataRef<AbilityData>, int> abilityCount in deckRef.data.abilityCounts)
			{
				abilityCounts[abilityCount.Key] = Math.Max(abilityCounts.GetValueOrDefault(abilityCount.Key), abilityCount.Value);
			}
			return AbilityDeckData.Search(deckRef.data.characterClass).Count() < 8;
		}

		public void Unlock(DataRef<AbilityData> abilityDataRef)
		{
			abilityCounts[abilityDataRef] = Math.Min(abilityCounts.GetValueOrDefault(abilityDataRef) + 1, abilityDataRef.data.rank.Max());
		}

		public bool IsUnlocked(DataRef<AbilityData> abilityDataRef, bool? devUnlockOverride = null)
		{
			if (Count(abilityDataRef) <= 0)
			{
				return devUnlockOverride ?? DevData.Unlocks.abilities;
			}
			return true;
		}

		public int Count(DataRef<AbilityData> abilityDataRef, bool unlockAll = false)
		{
			if (!unlockAll)
			{
				return abilityCounts.GetValueOrDefault(abilityDataRef);
			}
			return abilityDataRef.data.rank.Max();
		}

		public PoolKeepItemDictionaryHandle<DataRef<AbilityData>, int> GetAbilityCounts(bool unlockAll = false)
		{
			PoolKeepItemDictionaryHandle<DataRef<AbilityData>, int> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<DataRef<AbilityData>, int>();
			if (unlockAll)
			{
				foreach (DataRef<AbilityData> ability in AbilityData.GetAbilities())
				{
					poolKeepItemDictionaryHandle.Add(ability, ability.data.rank.Max());
				}
				return poolKeepItemDictionaryHandle;
			}
			foreach (KeyValuePair<uint, int> abilityCount in abilityCounts)
			{
				DataRef<AbilityData> dataRef = DataRef<AbilityData>.FromFileId(abilityCount.Key);
				if (dataRef != null)
				{
					poolKeepItemDictionaryHandle[dataRef] = abilityCount.Value;
				}
			}
			return poolKeepItemDictionaryHandle;
		}

		public PoolKeepItemDictionaryHandle<DataRef<AbilityData>, int> GetMissingAbilityCounts()
		{
			PoolKeepItemDictionaryHandle<DataRef<AbilityData>, int> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<DataRef<AbilityData>, int>();
			foreach (DataRef<AbilityData> ability in AbilityData.GetAbilities())
			{
				int num = ability.data.rank.Max() - Count(ability);
				if (num > 0)
				{
					poolKeepItemDictionaryHandle.Add(ability, num);
				}
			}
			return poolKeepItemDictionaryHandle;
		}

		public bool HasMissingAbility()
		{
			return AbilityData.GetAbilities().Any((DataRef<AbilityData> dataRef) => dataRef.data.rank.Max() > Count(dataRef));
		}

		public PoolKeepItemListHandle<DataRef<AbilityData>> UnlockRandomAbilities(Random random, int count, DataRef<CharacterData> preferredCharacter = null, float chanceOfKeepingPreferredCharacterPerCard = 0.5f, HashSet<Ability> abilitiesInDeck = null, float keepPreferredAbilityChance = 0f)
		{
			using PoolKeepItemHashSetHandle<PlayerClass> poolKeepItemHashSetHandle3 = Pools.UseKeepItemHashSet(from d in ProfileManager.progress.characters.read.GetLockedCharacters(DevData.Unlocks.characters)
				select d.data.characterClass);
			using PoolKeepItemDictionaryHandle<DataRef<AbilityData>, int> poolKeepItemDictionaryHandle = GetMissingAbilityCounts();
			if (poolKeepItemDictionaryHandle.Count == 0)
			{
				return Pools.UseKeepItemList<DataRef<AbilityData>>();
			}
			using PoolKeepItemDictionaryHandle<DataRef<AbilityData>, int> poolKeepItemDictionaryHandle2 = Pools.UseKeepItemDictionary(poolKeepItemDictionaryHandle.value);
			using PoolKeepItemDictionaryHandle<DataRef<AbilityData>, int> poolKeepItemDictionaryHandle3 = Pools.UseKeepItemDictionary(poolKeepItemDictionaryHandle.value);
			using PoolKeepItemHashSetHandle<DataRef<AbilityData>> poolKeepItemHashSetHandle2 = Pools.UseKeepItemHashSet<DataRef<AbilityData>>();
			PlayerClass? preferredClass = ((!preferredCharacter) ? null : preferredCharacter?.data.characterClass);
			if (abilitiesInDeck != null && preferredClass.HasValue)
			{
				Dictionary<DataRef<AbilityData>, DataRef<AbilityData>> dictionary = poolKeepItemDictionaryHandle.value.Keys.Where((DataRef<AbilityData> a) => (bool)a.data.upgradeOf && a.data.characterClass == preferredClass).Distinct().ToDictionarySafe((DataRef<AbilityData> a) => a.data.upgradeOf, (DataRef<AbilityData> a) => a);
				foreach (KeyValuePair<DataRef<AbilityData>, DataRef<AbilityData>> item in dictionary.EnumeratePairsSafe())
				{
					DataRef<AbilityData> upgradeOf = item.Key.data.upgradeOf;
					if (upgradeOf != null && (bool)upgradeOf && !poolKeepItemDictionaryHandle.ContainsKey(item.Key))
					{
						dictionary[upgradeOf] = item.Value;
					}
				}
				using PoolKeepItemHashSetHandle<DataRef<AbilityData>> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet(from a in abilitiesInDeck
					where a.data.characterClass == preferredClass && a.data.category == AbilityData.Category.Ability
					select a.dataRef.BaseAbilityRef());
				foreach (DataRef<AbilityData> item2 in poolKeepItemHashSetHandle.value)
				{
					DataRef<AbilityData> dataRef = dictionary.GetValueOrDefault(item2) ?? poolKeepItemDictionaryHandle.value.GetKeyOrDefault(item2);
					if (dataRef != null)
					{
						poolKeepItemHashSetHandle2.Add(dataRef);
					}
				}
			}
			foreach (DataRef<AbilityData> key in poolKeepItemDictionaryHandle2.value.Keys)
			{
				if (((bool)key.data.upgradeOf && poolKeepItemDictionaryHandle2.ContainsKey(key.data.upgradeOf)) || !key.data.cosmetic.hasImage)
				{
					poolKeepItemDictionaryHandle.value.Remove(key);
				}
			}
			poolKeepItemDictionaryHandle2.value.ClearAndCopyFrom(poolKeepItemDictionaryHandle);
			if (poolKeepItemHashSetHandle3.Count > 0)
			{
				foreach (DataRef<AbilityData> key2 in poolKeepItemDictionaryHandle2.value.Keys)
				{
					PlayerClass? characterClass = key2.data.characterClass;
					if (characterClass.HasValue)
					{
						PlayerClass valueOrDefault = characterClass.GetValueOrDefault();
						if (poolKeepItemHashSetHandle3.Contains(valueOrDefault))
						{
							poolKeepItemDictionaryHandle.value.Remove(key2);
						}
					}
				}
			}
			if (poolKeepItemDictionaryHandle.Count == 0)
			{
				poolKeepItemDictionaryHandle.value.ClearAndCopyFrom((poolKeepItemDictionaryHandle2.Count > 0) ? poolKeepItemDictionaryHandle2 : poolKeepItemDictionaryHandle3);
			}
			PoolKeepItemListHandle<DataRef<AbilityData>> poolKeepItemListHandle = Pools.UseKeepItemList<DataRef<AbilityData>>();
			for (int i = 0; i < count; i++)
			{
				DataRef<AbilityData> dataRef2 = _UnlockRandomAbility(poolKeepItemDictionaryHandle, random, preferredCharacter?.data.characterClass, poolKeepItemHashSetHandle2);
				if (dataRef2 == null)
				{
					break;
				}
				Unlock(poolKeepItemListHandle.AddReturn(dataRef2));
				if (poolKeepItemHashSetHandle2.Count > 0)
				{
					if (random.Chance(1f - keepPreferredAbilityChance))
					{
						poolKeepItemHashSetHandle2.value.Clear();
						abilitiesInDeck = null;
					}
					else
					{
						poolKeepItemHashSetHandle2.Remove(dataRef2);
					}
				}
				if (poolKeepItemDictionaryHandle.ContainsKey(dataRef2) && --poolKeepItemDictionaryHandle[dataRef2] == 0 && poolKeepItemDictionaryHandle.Remove(dataRef2) && poolKeepItemDictionaryHandle.Count == 0)
				{
					break;
				}
				if ((bool)preferredCharacter && random.Chance(1f - chanceOfKeepingPreferredCharacterPerCard))
				{
					preferredCharacter = null;
				}
			}
			while (poolKeepItemListHandle.Count < count)
			{
				int count2 = poolKeepItemListHandle.Count;
				foreach (DataRef<AbilityData> item3 in UnlockRandomAbilities(random, count - poolKeepItemListHandle.Count, preferredCharacter, chanceOfKeepingPreferredCharacterPerCard, abilitiesInDeck, keepPreferredAbilityChance))
				{
					poolKeepItemListHandle.Add(item3);
				}
				if (count2 == poolKeepItemListHandle.Count)
				{
					break;
				}
			}
			return poolKeepItemListHandle;
		}
	}

	[ProtoMember(1)]
	private bool _openingPlayed;

	private ProtoProperty<HashSet<uint>> _tutorials;

	private ProtoProperty<Games> _games;

	private ProtoProperty<Characters> _characters;

	private ProtoProperty<Abilities> _abilities;

	private ProtoProperty<Experience> _experience;

	private ProtoProperty<AchievementProgress> _achievements;

	private string _propertyPath;

	private string _runPath;

	public bool openingPlayed
	{
		get
		{
			return _openingPlayed;
		}
		set
		{
			_openingPlayed = value;
		}
	}

	private string propertyPath => _propertyPath ?? (_propertyPath = ProfileManager.Profile.path);

	public ProtoProperty<HashSet<uint>> tutorials => _tutorials ?? (_tutorials = new ProtoProperty<HashSet<uint>>(_GetPropertyPath("Tutorials")));

	public ProtoProperty<Games> games => _games ?? (_games = new ProtoProperty<Games>(_GetPropertyPath("Games")));

	public ProtoProperty<Characters> characters => _characters ?? (_characters = new ProtoProperty<Characters>(_GetPropertyPath("Characters")));

	public ProtoProperty<Abilities> abilities => _abilities ?? (_abilities = new ProtoProperty<Abilities>(_GetPropertyPath("Abilities")));

	public ProtoProperty<Experience> experience => _experience ?? (_experience = new ProtoProperty<Experience>(_GetPropertyPath("Experience")));

	public ProtoProperty<AchievementProgress> achievements => _achievements ?? (_achievements = new ProtoProperty<AchievementProgress>(_GetPropertyPath("Achievements")));

	public bool deckCreationEnabled
	{
		get
		{
			if (experience.read.packsOpened <= 0)
			{
				return characters.read.GetUnlockedCharacters(DevData.Unlocks.characters).NoneValidButSomeExist((DataRef<CharacterData> character) => AbilityDeckData.Search(character.data.characterClass).Any((DataRef<AbilityDeckData> deck) => deck.data.isValid));
			}
			return true;
		}
	}

	public bool hasActiveRun => File.Exists(runPath);

	public string runPath => _runPath ?? (_runPath = _GetPropertyPath("Run"));

	private string _GetPropertyPath(string propertyName)
	{
		return IOUtil.Combine(propertyPath, propertyName + ".bytes");
	}

	public void UnlockDefaultCharacters()
	{
		if (!characters.read.IsUnlocked(ContentRef.Defaults.data.startingCharacter))
		{
			characters.write.Unlock(ContentRef.Defaults.data.startingCharacter);
		}
	}

	public void UnlockDefaultDecks()
	{
		foreach (DataRef<AbilityDeckData> item in DataRef<AbilityDeckData>.Search())
		{
			if (item.data.unlockedByDefault && abilities.read.ShouldUnlock(item) && abilities.write.UnlockDeck(item) && ContentRef.UGC)
			{
				new DataRef<AbilityDeckData>(new AbilityDeckData(item.data.name, item.data.character).SetData(item.data.name, item.data.abilities, item.data.description)).Save(forceOverwrite: true);
			}
		}
	}

	public void UnlockDefaultGames()
	{
		foreach (DataRef<GameData> item in DataRef<GameData>.All)
		{
			if (item.data.unlockedByDefault && !games.read.IsUnlocked(item))
			{
				games.write.UnlockGame(item);
			}
		}
	}

	public IEnumerable<string> GetAchievements()
	{
		return (from b in DataRef<CharacterData>.All.Select((DataRef<CharacterData> c) => characters.read[c]).SelectMany((CharacterProgress p) => p.GetUnlockedBonuses())
			select b.data.achievementName into s
			where s.HasVisibleCharacter()
			select s).Concat(from a in DataRef<AchievementData>.All
			where achievements.read.IsComplete(a)
			select a.data.apiName).Distinct();
	}

	public IEnumerable<string> GetAllAchievements()
	{
		return (from d in DataRef<BonusCardData>.All
			select d.data.achievementName into s
			where s.HasVisibleCharacter()
			select s).Concat(from d in DataRef<AchievementData>.All
			select d.data.apiName into s
			where s.HasVisibleCharacter()
			select s).Distinct();
	}

	public GameState LoadActiveRun()
	{
		if (hasActiveRun)
		{
			GameState gameState = IOUtil.LoadFromBytesBackup<GameState>(runPath);
			if (gameState != null && gameState.wasSerializedWithCurrentVersion)
			{
				return gameState;
			}
		}
		return null;
	}

	public void DeleteActiveRun()
	{
		lock (FileLock.Lock(runPath))
		{
			IOUtil.DeleteSafe(runPath, deleteBackupFile: true);
		}
	}

	[ProtoAfterSerialization]
	private void _ProtoAfterSerialization()
	{
		_tutorials?.SaveChanges();
		_games?.SaveChanges();
		_characters?.SaveChanges();
		_abilities?.SaveChanges();
		_experience?.SaveChanges();
		_achievements?.SaveChanges();
	}
}
