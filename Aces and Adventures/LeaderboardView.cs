using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

public class LeaderboardView : ATargetView
{
	public static ResourceBlueprint<GameObject> Blueprint = "GameState/LeaderboardView";

	[Header("Leaderboard")]
	[Range(100f, 10000f)]
	public int maxDisplayedEntries = 1000;

	public LocalizeStringEvent title;

	public RectTransform showModifiersTransform;

	[Header("Entries")]
	public LeaderboardEntryView entryBlueprint;

	public PagedRect entryPagedRect;

	public SingleSoundPackSource pageChangeSoundSource;

	[Header("Events")]
	public BoolEvent onHasNextChange;

	public BoolEvent onHasNextAnyChange;

	public BoolEvent onHasPreviousChange;

	public BoolEvent onHasPreviousAnyChange;

	public BoolEvent onHasPlayerEntryChange;

	public StringEvent onDateChange;

	public BoolEvent onSortingByTimeChange;

	public BoolEvent onSortingByManaChange;

	public BoolEvent onRetrievingDataChange;

	public BoolEvent onNoEntriesFoundChange;

	public BoolEvent onViewFriendsChange;

	public UnityEvent onCloseRequested;

	public StringEvent onTimeToResetChange;

	private PlayerClass? _classFilter;

	private int? _reloadFilter;

	private readonly List<LeaderboardProgress.Entry> _entries = new List<LeaderboardProgress.Entry>();

	private LeaderboardProgress.Entry? _playerEntry;

	private int? _playerEntryIndex;

	private SortLeaderboardBy _sort;

	private bool _viewFriends;

	private int? _previous;

	private int? _next;

	private Dictionary<int, Task<List<LeaderboardProgress.Entry>>> _cachedEntries = new Dictionary<int, Task<List<LeaderboardProgress.Entry>>>();

	private int _secondsToReset;

	private DataRef<ProceduralNodeData> _modifierOverride;

	public Leaderboard leaderboard => base.target as Leaderboard;

	public bool hasClassFilter => _classFilter.HasValue;

	public int? reloadFilter => _reloadFilter;

	public SortLeaderboardBy sort
	{
		get
		{
			return _sort;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _sort, value))
			{
				_OnSortChange();
			}
		}
	}

	public int day
	{
		get
		{
			return leaderboard.day;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref leaderboard.day, value))
			{
				_OnDayChange();
			}
		}
	}

	public int? playerEntryIndex
	{
		get
		{
			return _playerEntryIndex;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _playerEntryIndex, value))
			{
				onHasPlayerEntryChange?.Invoke(value.HasValue);
			}
		}
	}

	public bool viewFriends
	{
		get
		{
			return _viewFriends;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _viewFriends, value))
			{
				_OnViewFriendsChange();
			}
		}
	}

	public int secondsToReset
	{
		get
		{
			return _secondsToReset;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _secondsToReset, value))
			{
				_OnSecondsToResetChange();
			}
		}
	}

	public static LeaderboardView Create(Leaderboard leaderboard, Transform parent = null)
	{
		return Pools.Unpool(Blueprint, parent).GetComponent<LeaderboardView>().SetData(leaderboard) as LeaderboardView;
	}

	private void _OnDayChange()
	{
		onDateChange?.Invoke(LeaderboardProgress.GetDate(leaderboard.day + 1).ToString("d", LocalizationSettings.SelectedLocale.Formatter));
		BoolEvent boolEvent = onHasNextChange;
		int? num;
		if (boolEvent != null)
		{
			num = (_next = ProfileManager.progress.games.read.leaderboardProgress.GetNextDayWithEntry(leaderboard.adventure, leaderboard.day));
			boolEvent.Invoke(num.HasValue);
		}
		num = LeaderboardProgress.GetDay();
		int num2;
		if (num.HasValue)
		{
			int valueOrDefault = num.GetValueOrDefault();
			num2 = ((leaderboard.day < valueOrDefault) ? 1 : 0);
		}
		else
		{
			num2 = 0;
		}
		bool flag = (byte)num2 != 0;
		onHasNextAnyChange?.Invoke(flag);
		BoolEvent boolEvent2 = onHasPreviousChange;
		if (boolEvent2 != null)
		{
			num = (_previous = ProfileManager.progress.games.read.leaderboardProgress.GetPreviousDayWithEntry(leaderboard.adventure, leaderboard.day));
			boolEvent2.Invoke(num.HasValue);
		}
		bool flag2 = leaderboard.day > 31;
		onHasPreviousAnyChange?.Invoke(flag2);
		_GetLeaderboardEntries();
		if (_previous.HasValue)
		{
			_GetEntries(_previous.Value);
		}
		if (flag2)
		{
			_GetEntries(day - 1);
		}
		if (flag)
		{
			_GetEntries(day + 1);
		}
	}

	private void _OnSortChange()
	{
		onSortingByTimeChange?.Invoke(sort == SortLeaderboardBy.Time);
		onSortingByManaChange?.Invoke(sort == SortLeaderboardBy.Mana);
		_SetSortingDirty();
	}

	private void _OnViewFriendsChange()
	{
		onViewFriendsChange?.Invoke(viewFriends);
		_SetSortingDirty();
	}

	private void _SetSortingDirty()
	{
		if (!_entries.IsNullOrEmpty())
		{
			_CreatePagedEntries();
		}
	}

	private void _OnSecondsToResetChange()
	{
		onTimeToResetChange?.Invoke((_secondsToReset > 0) ? TimeSpan.FromSeconds(_secondsToReset).ToStringSimple() : "");
	}

	private async Task<List<LeaderboardProgress.Entry>> _GetEntryListAsync(int leaderboardDay)
	{
		List<LeaderboardProgress.Entry> list = new List<LeaderboardProgress.Entry>();
		await foreach ((LeaderboardEntry_t, int[]) leaderboardEntry in Steam.Stats.GetLeaderboardEntries(LeaderboardProgress.GetDailyName(leaderboardDay)))
		{
			LeaderboardProgress.Entry entry = new LeaderboardProgress.Entry(leaderboardEntry.Item1, LeaderboardProgress.GetData(leaderboard.adventure, leaderboardEntry.Item2));
			if ((bool)entry)
			{
				list.Add(entry);
			}
		}
		return list;
	}

	private Task<List<LeaderboardProgress.Entry>> _GetEntries(int leaderboardDay)
	{
		return _cachedEntries.GetValueOrDefault(leaderboardDay) ?? (_cachedEntries[leaderboardDay] = _GetEntryListAsync(leaderboardDay));
	}

	private async void _GetLeaderboardEntries()
	{
		_entries.Clear();
		entryPagedRect.ClearItems();
		_playerEntry = null;
		_reloadFilter = null;
		_classFilter = null;
		_modifierOverride = null;
		playerEntryIndex = null;
		onNoEntriesFoundChange?.Invoke(arg0: false);
		onRetrievingDataChange?.Invoke(arg0: true);
		foreach (LeaderboardProgress.Entry item in await _GetEntries(leaderboard.day))
		{
			_entries.Add(item);
			if (item.isPlayerEntry)
			{
				_playerEntry = item;
			}
		}
		onRetrievingDataChange?.Invoke(arg0: false);
		_CreatePagedEntries();
	}

	private void _CreatePagedEntries()
	{
		playerEntryIndex = null;
		int? num = _reloadFilter;
		if (!num.HasValue)
		{
			_reloadFilter = _playerEntry?.data.reloadCount;
		}
		using PoolKeepItemListHandle<LeaderboardProgress.Entry> poolKeepItemListHandle = Pools.UseKeepItemList(_entries.Where((LeaderboardProgress.Entry e) => (!hasClassFilter || e.data.playerClass == _classFilter) && (!_reloadFilter.HasValue || e.data.reloadCount <= _reloadFilter) && (bool)e.data).OrderBy((sort == SortLeaderboardBy.Time) ? ((Func<LeaderboardProgress.Entry, int>)((LeaderboardProgress.Entry e) => e.data.score)) : ((Func<LeaderboardProgress.Entry, int>)((LeaderboardProgress.Entry e) => -e.data.mana))).ThenBy((sort == SortLeaderboardBy.Time) ? ((Func<LeaderboardProgress.Entry, int>)((LeaderboardProgress.Entry e) => -e.data.mana)) : ((Func<LeaderboardProgress.Entry, int>)((LeaderboardProgress.Entry e) => e.data.score)))
			.AsEnumerable());
		onNoEntriesFoundChange?.Invoke(poolKeepItemListHandle.Count == 0);
		entryPagedRect.ClearItems();
		entryPagedRect.page = 1;
		for (int i = 0; i < poolKeepItemListHandle.value.Count; i++)
		{
			int rank = i + 1;
			LeaderboardProgress.Entry entry2 = poolKeepItemListHandle[i];
			if ((!viewFriends || entry2.isFriendEntry) && (entryPagedRect.count < maxDisplayedEntries - playerEntryIndex.HasValue.ToInt(0, 1) || entry2.isPlayerEntry))
			{
				if (!playerEntryIndex.HasValue && entry2.isPlayerEntry)
				{
					playerEntryIndex = entryPagedRect.count;
				}
				entryPagedRect.AddItem((Transform parent) => Pools.Unpool(entryBlueprint.gameObject, parent).GetComponent<LeaderboardEntryView>().SetData(this, rank, entry2)
					.gameObject);
			}
		}
		if (playerEntryIndex.HasValue)
		{
			ProfileManager.progress.games.write.leaderboardProgress.AddPreviousDay(leaderboard.day, leaderboard.adventure);
		}
		if (!_modifierOverride && poolKeepItemListHandle.value.Count > 0)
		{
			_modifierOverride = poolKeepItemListHandle.value.Select((LeaderboardProgress.Entry entry) => DataRef<ProceduralNodeData>.FromFileId(entry.data.modifierId)).FirstOrDefault((DataRef<ProceduralNodeData> modifier) => modifier);
		}
		if (IOUtil.IsEditor && (bool)_modifierOverride)
		{
			Debug.Log(_modifierOverride.GetFriendlyName());
		}
		ViewPlayerEntry();
		pageChangeSoundSource?.Play();
	}

	private void Update()
	{
		secondsToReset = ((leaderboard.day == LeaderboardProgress.GetDay()) ? ((int)Math.Ceiling(LeaderboardProgress.GetTimeToReset().TotalSeconds)) : 0);
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		if (newTarget is Leaderboard leaderboard)
		{
			title.StringReference = leaderboard.adventure.data.nameLocalized;
			_OnDayChange();
			_OnSortChange();
		}
	}

	public void SetClassFilter(PlayerClass classFilter)
	{
		if (SetPropertyUtility.SetStruct(ref _classFilter, classFilter))
		{
			_SetSortingDirty();
		}
	}

	public void ClearClassFilter()
	{
		if (_classFilter.HasValue)
		{
			PlayerClass? playerClass = (_classFilter = null);
			if (!playerClass.HasValue)
			{
				_SetSortingDirty();
			}
		}
	}

	public void SetReloadFilter(int reload)
	{
		if (SetPropertyUtility.SetStruct(ref _reloadFilter, reload))
		{
			_SetSortingDirty();
		}
	}

	public void ClearReloadFilter()
	{
		if (SetPropertyUtility.SetStruct(ref _reloadFilter, int.MaxValue))
		{
			_SetSortingDirty();
		}
	}

	public void Next()
	{
		day = _next ?? day;
	}

	public void NextAny()
	{
		int num = day + 1;
		day = num;
	}

	public void Previous()
	{
		day = _previous ?? day;
	}

	public void PreviousAny()
	{
		int num = day - 1;
		day = num;
	}

	public void Close()
	{
		onCloseRequested?.Invoke();
	}

	public void ViewPlayerEntry()
	{
		int? num = playerEntryIndex;
		if (num.HasValue)
		{
			int valueOrDefault = num.GetValueOrDefault();
			entryPagedRect.page = entryPagedRect.GetPageOfItem(valueOrDefault);
		}
	}

	public void SortByTime()
	{
		sort = SortLeaderboardBy.Time;
	}

	public void SortByMana()
	{
		sort = SortLeaderboardBy.Mana;
	}

	public void ShowModifiers()
	{
		DataRef<AdventureData> adventure = leaderboard?.adventure;
		if (adventure != null && (bool)adventure && (bool)adventure.data.modifier)
		{
			base.view.ShowCardsAsTooltip(() => base.view.state.GetModifierCards(adventure, adventure.data.modifier, leaderboard.day, _modifierOverride), showModifiersTransform);
		}
	}

	public void HideModifiers()
	{
		base.view.HideCardsShownAsTooltip();
	}
}
