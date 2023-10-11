using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using ProtoBuf;

[ProtoContract]
[UIField]
[Localize]
public class AchievementData : IDataContent
{
	[ProtoContract]
	[UIField]
	[ProtoInclude(5, typeof(AdventureComplete))]
	[ProtoInclude(6, typeof(LevelUp))]
	[ProtoInclude(7, typeof(AchievementUnlocked))]
	public abstract class Trigger
	{
		[ProtoContract]
		[UIField]
		public class AdventureComplete : Trigger
		{
			[ProtoMember(1)]
			[UIField]
			private AdventureCompletionRank? _minRank;

			[ProtoMember(2)]
			[UIField(collapse = UICollapseType.Open)]
			private DataRef<GameData> _game;

			private bool _gameSpecified => _game.ShouldSerialize();

			private void _OnAdventureCompleted(DataRef<GameData> game, DataRef<AdventureData> adventure, AdventureCompletionRank rank)
			{
				if ((!_game || ContentRef.Equal(_game, game)) && (!_minRank.HasValue || rank <= _minRank) && adventure.TrackedByAchievements())
				{
					base.onTrigger?.Invoke();
				}
			}

			public override void Register()
			{
				OnAdventureCompleted += _OnAdventureCompleted;
			}

			public override void Unregister()
			{
				OnAdventureCompleted -= _OnAdventureCompleted;
			}

			protected override string _ToString()
			{
				return "<b>Adventure Completed</b>" + (_game ? (" in <b>" + _game.friendlyName + "</b>") : "") + (_minRank.HasValue ? $" at <b>{_minRank}</b> or higher" : "");
			}
		}

		[ProtoContract]
		[UIField]
		public class LevelUp : Trigger
		{
			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Open)]
			private DataRef<CharacterData> _character;

			[ProtoMember(2)]
			[UIField]
			private RebirthLevel _rebirth;

			private bool _characterSpecified => _character.ShouldSerialize();

			private void _OnLevelUp(DataRef<CharacterData> character, RebirthLevel rebirth, int level)
			{
				if ((!_character || ContentRef.Equal(_character, character)) && _rebirth == rebirth)
				{
					base.onTrigger?.Invoke();
				}
			}

			public override void Register()
			{
				OnLevelUp += _OnLevelUp;
			}

			public override void Unregister()
			{
				OnLevelUp -= _OnLevelUp;
			}

			protected override string _ToString()
			{
				return string.Format("<b>{0}</b> levels in Rebirth {1}", _character ? _character.friendlyName : "Any Character", (int)_rebirth);
			}
		}

		[ProtoContract]
		[UIField]
		public class AchievementUnlocked : Trigger
		{
			private void _OnAchievementUnlocked(string achievementName)
			{
				base.onTrigger?.Invoke();
			}

			public override void Register()
			{
				OnAchievementUnlock += _OnAchievementUnlocked;
			}

			public override void Unregister()
			{
				OnAchievementUnlock -= _OnAchievementUnlocked;
			}

			protected override string _ToString()
			{
				return "An Achievement Is Unlocked";
			}
		}

		public event Action onTrigger;

		public abstract void Register();

		public abstract void Unregister();

		public sealed override string ToString()
		{
			return "When " + _ToString();
		}

		protected abstract string _ToString();
	}

	[ProtoContract]
	[UIField]
	[ProtoInclude(5, typeof(AdventureComplete))]
	[ProtoInclude(6, typeof(LevelUp))]
	[ProtoInclude(7, typeof(AllAchievementsUnlocked))]
	public abstract class Progress
	{
		[ProtoContract]
		[UIField]
		public class AdventureComplete : Progress
		{
			[ProtoMember(1)]
			[UIField]
			private AdventureCompletionRank? _minRank;

			[ProtoMember(2)]
			[UIField(collapse = UICollapseType.Open)]
			private DataRef<GameData> _game;

			private bool _gameSpecified => _game.ShouldSerialize();

			private UInt2 _GetProgress(DataRef<GameData> game)
			{
				using PoolKeepItemListHandle<DataRef<AdventureData>> poolKeepItemListHandle = Pools.UseKeepItemList(game.data.adventures.Where((DataRef<AdventureData> a) => a.TrackedByAchievements()));
				uint num = 0u;
				foreach (DataRef<AdventureData> item in poolKeepItemListHandle.value)
				{
					AdventureCompletion completion = ProfileManager.progress.games.read.GetCompletion(game, item);
					if (completion != null && (!_minRank.HasValue || completion.GetBestCompletionRank(item.data, game.data.newGameType)?.Value <= _minRank))
					{
						num++;
					}
				}
				return new UInt2(num, (uint)poolKeepItemListHandle.Count);
			}

			public override UInt2 GetProgress()
			{
				if ((bool)_game)
				{
					return _GetProgress(_game);
				}
				UInt2 result = default(UInt2);
				foreach (DataRef<GameData> item in DataRef<GameData>.All)
				{
					result += _GetProgress(item);
				}
				return result;
			}

			public override string ToString()
			{
				return "When all adventures " + (_game ? ("in <b>" + _game.friendlyName + "</b> ") : "") + "are completed" + (_minRank.HasValue ? (" at <b>" + EnumUtil.FriendlyName(_minRank.Value) + "</b> rank or higher") : "");
			}
		}

		[ProtoContract]
		[UIField]
		public class LevelUp : Progress
		{
			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Open)]
			private DataRef<CharacterData> _character;

			[ProtoMember(2)]
			[UIField]
			private RebirthLevel _rebirth;

			[ProtoMember(3)]
			[UIField(min = 1, max = 30)]
			[DefaultValue(30)]
			private uint _level = 30u;

			private bool _characterSpecified => _character.ShouldSerialize();

			private UInt2 _GetProgress(DataRef<CharacterData> character)
			{
				RebirthLevel rebirthLevel = EnumUtil<RebirthLevel>.Round(ProfileManager.progress.experience.read.GetRebirth(character));
				return new UInt2(Math.Min(_level, (rebirthLevel == _rebirth) ? ((uint)ProfileManager.progress.experience.read.GetLevelWithRebirth(character)) : ((rebirthLevel > _rebirth) ? 30u : 0u)), _level);
			}

			public override UInt2 GetProgress()
			{
				if ((bool)_character)
				{
					return _GetProgress(_character);
				}
				UInt2 result = default(UInt2);
				foreach (DataRef<CharacterData> item in DataRef<CharacterData>.All)
				{
					result += _GetProgress(item);
				}
				return result;
			}

			public override string ToString()
			{
				return string.Format("When {0} level {1} in Rebirth {2}", _character ? ("<b>" + _character.friendlyName + "</b> reaches") : "<b>All Characters</b> reach", _level, (int)_rebirth);
			}
		}

		[ProtoContract]
		[UIField]
		public class AllAchievementsUnlocked : Progress
		{
			private static uint? _OtherAchievementCount;

			private static uint OtherAchievementCount
			{
				get
				{
					uint valueOrDefault = _OtherAchievementCount.GetValueOrDefault();
					if (!_OtherAchievementCount.HasValue)
					{
						valueOrDefault = (uint)(ProfileManager.progress.GetAllAchievements().Count() - 1);
						_OtherAchievementCount = valueOrDefault;
						return valueOrDefault;
					}
					return valueOrDefault;
				}
			}

			public override int sortOrder => 10000;

			public override UInt2 GetProgress()
			{
				return new UInt2((uint)ProfileManager.progress.GetAchievements().Count(), OtherAchievementCount);
			}

			public override string ToString()
			{
				return "When All Other Achievements Are Unlocked";
			}
		}

		public virtual int sortOrder => 0;

		public abstract UInt2 GetProgress();
	}

	[ProtoMember(1)]
	[UIField("API Name", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Name of achievement in Steam API.")]
	private string _apiName;

	[ProtoMember(2)]
	[UIField(collapse = UICollapseType.Open)]
	private LocalizedStringData _name;

	[ProtoMember(3)]
	[UIField(collapse = UICollapseType.Open)]
	private LocalizedStringData _description;

	[ProtoMember(4, OverwriteList = true)]
	[UIField(tooltip = "Progress will be reevaluated when any of these triggers occur.", collapse = UICollapseType.Open)]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<Trigger> _triggers;

	[ProtoMember(5, OverwriteList = true)]
	[UIField(tooltip = "Achievement will be considered complete when all progress items are fully complete.", collapse = UICollapseType.Open)]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<Progress> _progress;

	[ProtoMember(6)]
	[UIField(min = 0, max = 100, tooltip = "Show steam achievement progress every time value is incremented beyond step.\n<i>A value of zero will not show progress.</i>")]
	private int _showProgressStep;

	private uint _fileId;

	public string apiName => _apiName;

	public List<Trigger> triggers => _triggers ?? (_triggers = new List<Trigger>());

	public List<Progress> progress => _progress ?? (_progress = new List<Progress>());

	public int sortOrder
	{
		get
		{
			if (progress.Count == 1)
			{
				return progress[0].sortOrder;
			}
			int num = int.MinValue;
			for (int i = 0; i < progress.Count; i++)
			{
				num = Math.Max(num, progress[i].sortOrder);
			}
			return num;
		}
	}

	[ProtoMember(15)]
	public string tags { get; set; }

	public static event Action<DataRef<GameData>, DataRef<AdventureData>, AdventureCompletionRank> OnAdventureCompleted;

	public static event Action<DataRef<CharacterData>, RebirthLevel, int> OnLevelUp;

	public static event Action<string> OnAchievementUnlock;

	public static async Task RegisterAll()
	{
		foreach (DataRef<AchievementData> item in from a in DataRef<AchievementData>.All
			where !ProfileManager.progress.achievements.read.IsComplete(a)
			orderby a.data.sortOrder
			select a)
		{
			item.data.Register(item);
			await item.data.UpdateProgress(indicateProgress: false);
		}
	}

	public static void SignalAdventureCompleted(DataRef<GameData> game, DataRef<AdventureData> adventure, AdventureCompletionRank rank)
	{
		AchievementData.OnAdventureCompleted?.Invoke(game, adventure, rank);
	}

	public static void SignalLevelUp(DataRef<CharacterData> character, RebirthLevel rebirth, int level)
	{
		AchievementData.OnLevelUp?.Invoke(character, rebirth, level);
	}

	public static void SignalAchievementUnlock(string achievementName)
	{
		AchievementData.OnAchievementUnlock?.Invoke(achievementName);
	}

	private async void _OnTrigger()
	{
		await UpdateProgress();
	}

	public void Register(DataRef<AchievementData> dataRef)
	{
		_fileId = dataRef;
		foreach (Trigger trigger in triggers)
		{
			trigger.Register();
			trigger.onTrigger += _OnTrigger;
		}
	}

	public void Unregister()
	{
		foreach (Trigger trigger in triggers)
		{
			trigger.Unregister();
			trigger.onTrigger -= _OnTrigger;
		}
	}

	public async Task UpdateProgress(bool indicateProgress = true)
	{
		UInt2 p = default(UInt2);
		foreach (Progress item in progress)
		{
			p += item.GetProgress();
		}
		uint num = ProfileManager.progress.achievements.read.GetProgress(_fileId);
		if (p.x != num)
		{
			ProfileManager.progress.achievements.write.SetProgress(_fileId, p);
			if (p.x >= p.y)
			{
				Unregister();
				await Steam.Stats.UnlockAchievement(_apiName);
			}
			else if (indicateProgress && _showProgressStep > 0 && (long)num / (long)_showProgressStep < (long)p.x / (long)_showProgressStep)
			{
				await Steam.Stats.IndicateAchievementProgress(_apiName, p.x, p.y);
			}
		}
	}

	public string GetTitle()
	{
		return _name;
	}

	public string GetAutomatedDescription()
	{
		return _description;
	}

	public List<string> GetAutomatedTags()
	{
		return null;
	}

	public void PrepareDataForSave()
	{
	}

	public string GetSaveErrorMessage()
	{
		if (!GetTitle().IsNullOrEmpty())
		{
			return "";
		}
		return "Please enter a name before saving.";
	}

	public void OnLoadValidation()
	{
	}
}
