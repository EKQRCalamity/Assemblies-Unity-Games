using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField]
[Localize]
public class GameData : IDataContent
{
	[ProtoContract]
	[UIField]
	public class AdventureMedia
	{
		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Open, tooltip = "Lighting to use during adventure select.")]
		private DataRef<LightingData> _selectLighting;

		[ProtoMember(2)]
		[UIField(collapse = UICollapseType.Open, tooltip = "Music to play during adventure select.")]
		private DataRef<MusicData> _selectMusic;

		[ProtoMember(3)]
		[UIField]
		[DefaultValue(AudioVolumeType.Soft)]
		private AudioVolumeType _selectMusicVolume = AudioVolumeType.Soft;

		[ProtoMember(4)]
		[UIField(collapse = UICollapseType.Open, tooltip = "Lighting to use when an adventure is completed.")]
		private DataRef<LightingData> _victoryLighting;

		[ProtoMember(5)]
		[UIField(collapse = UICollapseType.Open, tooltip = "Music to play when an adventure is completed.")]
		private DataRef<MusicData> _victoryMusic;

		[ProtoMember(6)]
		[UIField]
		[DefaultValue(AudioVolumeType.Soft)]
		private AudioVolumeType _victoryMusicVolume = AudioVolumeType.Soft;

		[ProtoMember(7)]
		[UIField(collapse = UICollapseType.Open, tooltip = "Lighting to use when an adventure is failed.")]
		private DataRef<LightingData> _lossLighting;

		[ProtoMember(8)]
		[UIField(collapse = UICollapseType.Open, tooltip = "Music to play when an adventure is failed.")]
		private DataRef<MusicData> _lossMusic;

		[ProtoMember(9)]
		[UIField]
		[DefaultValue(AudioVolumeType.Soft)]
		private AudioVolumeType _lossMusicVolume = AudioVolumeType.Soft;

		public DataRef<LightingData> selectLighting
		{
			get
			{
				if (!_selectLighting)
				{
					return ContentRef.Defaults.lighting.adventureSelect;
				}
				return _selectLighting;
			}
		}

		public DataRef<MusicData> selectMusic
		{
			get
			{
				if (!_selectMusic)
				{
					return ContentRef.Defaults.audio.environmentMusic;
				}
				return _selectMusic;
			}
		}

		public float selectMusicVolume => _selectMusicVolume.Volume();

		public DataRef<LightingData> victoryLighting
		{
			get
			{
				if (!_victoryLighting)
				{
					return ContentRef.Defaults.lighting.victory;
				}
				return _victoryLighting;
			}
		}

		public DataRef<MusicData> victoryMusic
		{
			get
			{
				if (!_victoryMusic)
				{
					return ContentRef.Defaults.audio.victoryMusic;
				}
				return _victoryMusic;
			}
		}

		public float victoryMusicVolume => _victoryMusicVolume.Volume();

		public DataRef<LightingData> lossLighting
		{
			get
			{
				if (!_lossLighting)
				{
					return ContentRef.Defaults.lighting.loss;
				}
				return _lossLighting;
			}
		}

		public DataRef<MusicData> lossMusic
		{
			get
			{
				if (!_lossMusic)
				{
					return ContentRef.Defaults.audio.lossMusic;
				}
				return _lossMusic;
			}
		}

		public float lossMusicVolume => _lossMusicVolume.Volume();

		private bool _selectLightingSpecified => _selectLighting.ShouldSerialize();

		private bool _selectMusicSpecified => _selectMusic.ShouldSerialize();

		private bool _victoryLightingSpecified => _victoryLighting.ShouldSerialize();

		private bool _victoryMusicSpecified => _victoryMusic.ShouldSerialize();

		private bool _lossMusicSpecified => _lossMusic.ShouldSerialize();

		private bool _lossLightingSpecified => _lossLighting.ShouldSerialize();
	}

	[ProtoContract]
	[UIField]
	public class EnvironmentMedia
	{
		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<LightingData> _lighting;

		[ProtoMember(2)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<MusicData> _music;

		[ProtoMember(3)]
		[UIField]
		[DefaultValue(AudioVolumeType.Soft)]
		private AudioVolumeType _musicVolume = AudioVolumeType.Soft;

		[ProtoMember(4)]
		[UIField(filter = AudioCategoryType.Ambient, collapse = UICollapseType.Open, category = "Environment Ambient")]
		private AudioRef _ambient;

		[ProtoMember(5)]
		[UIField]
		[DefaultValue(AudioVolumeType.Soft)]
		private AudioVolumeType _ambientVolume = AudioVolumeType.Soft;

		public DataRef<LightingData> lighting
		{
			get
			{
				if (!_lighting)
				{
					return ContentRef.Defaults.lighting.environment;
				}
				return _lighting;
			}
		}

		public DataRef<MusicData> music
		{
			get
			{
				if (!_music)
				{
					return ContentRef.Defaults.audio.environmentMusic;
				}
				return _music;
			}
		}

		public AudioRef ambient
		{
			get
			{
				if (!_ambient)
				{
					return ContentRef.Defaults.audio.environmentAmbient;
				}
				return _ambient;
			}
		}

		public float musicVolume => _musicVolume.Volume();

		public float ambientVolume => _ambientVolume.Volume();

		private bool _lightingSpecified => _lighting.ShouldSerialize();

		private bool _musicSpecified => _music.ShouldSerialize();

		private bool _ambientSpecified => _ambient.ShouldSerialize();
	}

	private const string CAT_MAIN = "Main";

	private const string CAT_ADVENTURE_MEDIA = "Adventure Media";

	private const string CAT_ENV = "Environment";

	[ProtoMember(14)]
	[UIField("Name", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
	[UICategory("Main")]
	[UIDeepValueChange]
	private LocalizedStringData _nameLocalized;

	[ProtoMember(2)]
	[UIField]
	[UICategory("Main")]
	private bool _unlockedByDefault;

	[ProtoMember(7)]
	[UIField(validateOnChange = true)]
	[UICategory("Main")]
	private DataRef<GameData> _takeAdventuresFrom;

	[ProtoMember(17, OverwriteList = true)]
	[UIField]
	[UIFieldCollectionItem]
	[UICategory("Main")]
	[UIHideIf("_hideExcludeTakenFromAdventures")]
	[UIDeepValueChange]
	private HashSet<DataRef<AdventureData>> _excludedTakenFromAdventures;

	[ProtoMember(3)]
	[UIField(maxCount = 0, collapse = UICollapseType.Open)]
	[UIFieldCollectionItem]
	[UICategory("Main")]
	[UIHideIf("_hideAdventures")]
	private List<DataRef<AdventureData>> _adventures;

	[ProtoMember(4)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Adventure Media")]
	private AdventureMedia _adventureMedia;

	[ProtoMember(8)]
	[UIField]
	[UICategory("Main")]
	private DataRef<GameData> _includeBonusesFrom;

	[ProtoMember(5, OverwriteList = true)]
	[UIField(maxCount = 0)]
	[UIFieldCollectionItem]
	[UICategory("Main")]
	private HashSet<DataRef<BonusCardData>> _bonuses;

	[ProtoMember(6)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Environment")]
	private EnvironmentMedia _environmentMedia;

	[ProtoMember(11)]
	[UIField]
	[UICategory("Main")]
	private NewGameType _newGameType;

	[ProtoMember(15)]
	[UIField]
	[UICategory("Main")]
	private SpecialGameType? _specialGameType;

	[ProtoMember(16)]
	[UIField]
	[UICategory("Main")]
	private bool _unlockedForDemo;

	[ProtoMember(18)]
	[UIField(tooltip = "Chance that rewarded ability will belong to character being played.\n<i>Leave null to use default value (.5).</i>", min = 0.01, max = 1)]
	[UICategory("Main")]
	private float? _unlockAbilityForCurrentClassChance;

	[ProtoMember(19)]
	[UIField(tooltip = "Chance that preferred abilities <i>(abilities in deck)</i> will continue to be unlocked after first guaranteed preferred ability is unlocked.", min = 0, max = 1)]
	[UICategory("Main")]
	private float _keepPreferredAbilityChance;

	[ProtoMember(20)]
	[UIField(tooltip = "All adventures will be available when game is first unlocked.")]
	[UICategory("Main")]
	private bool _unlockAllAdventures;

	private List<DataRef<AdventureData>> _takenAdventures;

	[ProtoMember(1)]
	private string _name
	{
		get
		{
			return null;
		}
		set
		{
			if (_nameLocalized == null)
			{
				_nameLocalized = new LocalizedStringData(value);
			}
		}
	}

	public AdventureMedia adventureMedia => _adventureMedia ?? (_adventureMedia = new AdventureMedia());

	public EnvironmentMedia environmentMedia => _environmentMedia ?? (_environmentMedia = new EnvironmentMedia());

	public DataRef<LightingData> adventureSelectLighting => adventureMedia.selectLighting;

	public DataRef<LightingData> environmentLighting => environmentMedia.lighting;

	public DataRef<MusicData> environmentMusic => environmentMedia.music;

	public float environmentMusicVolume => environmentMedia.musicVolume;

	public AudioRef environmentAmbient => environmentMedia.ambient;

	public float environmentAmbientVolume => environmentMedia.ambientVolume;

	public DataRef<MusicData> adventureSelectMusic => adventureMedia.selectMusic;

	public float adventureSelectMusicVolume => adventureMedia.selectMusicVolume;

	public DataRef<LightingData> victoryLighting => adventureMedia.victoryLighting;

	public DataRef<MusicData> victoryMusic => adventureMedia.victoryMusic;

	public float victoryMusicVolume => adventureMedia.victoryMusicVolume;

	public DataRef<LightingData> lossLighting => adventureMedia.lossLighting;

	public DataRef<MusicData> lossMusic => adventureMedia.lossMusic;

	public float lossMusicVolume => adventureMedia.lossMusicVolume;

	public AudioRef victoryAmbient => environmentAmbient;

	public float victoryAmbientVolume => environmentAmbientVolume;

	public AudioRef lossAmbient => environmentAmbient;

	public float lossAmbientVolume => environmentAmbientVolume;

	private List<DataRef<AdventureData>> _adventureList
	{
		get
		{
			if (!_takeAdventuresFrom)
			{
				return _adventures;
			}
			return _takenAdventures ?? (_takenAdventures = new List<DataRef<AdventureData>>(_takeAdventuresFrom.data._adventures.Where(delegate(DataRef<AdventureData> a)
			{
				HashSet<DataRef<AdventureData>> excludedTakenFromAdventures = _excludedTakenFromAdventures;
				return excludedTakenFromAdventures == null || !excludedTakenFromAdventures.Contains(a);
			})));
		}
	}

	public IEnumerable<DataRef<AdventureData>> adventures
	{
		get
		{
			IEnumerable<DataRef<AdventureData>> adventureList = _adventureList;
			return adventureList ?? Enumerable.Empty<DataRef<AdventureData>>();
		}
	}

	public IEnumerable<DataRef<BonusCardData>> bonuses
	{
		get
		{
			IEnumerable<DataRef<BonusCardData>> enumerable = _bonuses;
			return (enumerable ?? Enumerable.Empty<DataRef<BonusCardData>>()).Concat(_includeBonusesFrom ? _includeBonusesFrom.data.bonuses : Enumerable.Empty<DataRef<BonusCardData>>()).Distinct();
		}
	}

	public NewGameType newGameType => _newGameType;

	public SpecialGameType? specialGameType => _specialGameType;

	public bool unlockedByDefault => _unlockedByDefault;

	public bool unlockedForDemo => _unlockedForDemo;

	public int sortOrder
	{
		get
		{
			NewGameType num = newGameType;
			ref SpecialGameType? reference = ref _specialGameType;
			return (int)(num + (reference.HasValue ? (reference.GetValueOrDefault().SortValue() + 1000) : 0));
		}
	}

	public float unlockAbilityForCurrentClassChance => _unlockAbilityForCurrentClassChance ?? 0.5f;

	public float keepPreferredAbilityChance => _keepPreferredAbilityChance;

	public bool unlockAllAdventures => _unlockAllAdventures;

	[ProtoMember(128)]
	public string tags { get; set; }

	private bool _hideAdventures => _takeAdventuresFrom;

	private bool _hideExcludeTakenFromAdventures => !_takeAdventuresFrom;

	private bool _takeAdventuresFromSpecified => _takeAdventuresFrom.ShouldSerialize();

	private bool _includeBonusesFromSpecified => _includeBonusesFrom.ShouldSerialize();

	public DataRef<AdventureData> GetNextAdventure(DataRef<AdventureData> currentAdventure)
	{
		List<DataRef<AdventureData>> adventureList = _adventureList;
		int? num = adventureList?.IndexOf(currentAdventure);
		if (num >= 0 && num < adventureList.Count - 1)
		{
			DataRef<AdventureData> dataRef = adventureList[num.Value + 1];
			if (dataRef != null && dataRef.CanBeUnlocked())
			{
				return dataRef;
			}
		}
		return null;
	}

	public int? GetCharacterUnlockIndex(DataRef<CharacterData> character)
	{
		List<DataRef<AdventureData>> adventureList = _adventureList;
		if (adventureList.IsNullOrEmpty())
		{
			return null;
		}
		for (int i = 0; i < adventureList.Count; i++)
		{
			if (adventureList[i].data.GetRewardsThatShouldUnlock().OfType<AReward.UnlockClassReward>().FirstOrDefault((AReward.UnlockClassReward r) => ContentRef.Equal(character, r.characterToUnlock)) != null)
			{
				return i + 1;
			}
		}
		return null;
	}

	public bool HasCompletedUpToAdventure(DataRef<GameData> game, DataRef<AdventureData> adventure)
	{
		int? num = _adventures?.IndexOf(adventure);
		if (!(num >= 0))
		{
			return false;
		}
		ProfileProgress.Games read = ProfileManager.progress.games.read;
		for (int num2 = num.Value - 1; num2 >= 0; num2--)
		{
			if (!read.HasCompleted(game, _adventures[num2]))
			{
				return false;
			}
			if (read.IsCustomUnlocked(game, _adventures[num2]))
			{
				return true;
			}
		}
		return true;
	}

	public string GetTitle()
	{
		return _nameLocalized ?? (_nameLocalized = new LocalizedStringData(""));
	}

	public string GetAutomatedDescription()
	{
		return null;
	}

	public List<string> GetAutomatedTags()
	{
		return null;
	}

	public void PrepareDataForSave()
	{
		if ((bool)_takeAdventuresFrom)
		{
			_adventures?.Clear();
		}
		_adventures?.RemoveAll((DataRef<AdventureData> d) => !d);
		_bonuses?.RemoveWhere((DataRef<BonusCardData> d) => !d);
	}

	public string GetSaveErrorMessage()
	{
		if (!GetTitle().HasVisibleCharacter())
		{
			return "Please enter name before saving.";
		}
		if (adventures.None())
		{
			return "A game must contain at least 1 adventure in order to be saved.";
		}
		return null;
	}

	public void OnLoadValidation()
	{
	}
}
