using System;
using UnityEngine;
using UnityEngine.Localization.Components;

public class LeaderboardEntryView : MonoBehaviour
{
	public StringEvent onRankChange;

	public StringEvent onNameChange;

	public StringEvent onTimeChange;

	public StringEvent onManaChange;

	public LocalizeStringEvent className;

	public StringEvent onReloadsChange;

	public BoolEvent onIsPlayerEntryChange;

	public BoolEvent onClassFilterActiveChange;

	public BoolEvent onReloadFilterActiveChange;

	private LeaderboardView _leaderboard;

	private int _rank;

	private LeaderboardProgress.Entry _entry;

	public LeaderboardProgress.Entry entry => _entry;

	private async void _OnEntryChange()
	{
		onRankChange?.Invoke($"#{_rank}");
		onTimeChange?.Invoke(entry.data.completed ? TimeSpan.FromSeconds(entry.data.absTime).ToStringSimple() : "");
		StringEvent stringEvent = onManaChange;
		if (stringEvent != null)
		{
			int mana = entry.data.mana;
			stringEvent.Invoke(mana.ToString());
		}
		className.StringReference = entry.data.playerClass.Localize();
		StringEvent stringEvent2 = onReloadsChange;
		if (stringEvent2 != null)
		{
			int mana = entry.data.reloadCount;
			stringEvent2.Invoke(mana.ToString());
		}
		onIsPlayerEntryChange?.Invoke(entry.isPlayerEntry);
		onClassFilterActiveChange?.Invoke(_leaderboard.hasClassFilter);
		onReloadFilterActiveChange?.Invoke(_leaderboard.reloadFilter == entry.data.reloadCount);
		onNameChange?.Invoke("");
		onNameChange?.Invoke(await entry.GetPersonaNameAsync());
	}

	public LeaderboardEntryView SetData(LeaderboardView leaderboard, int rank, LeaderboardProgress.Entry data)
	{
		_leaderboard = leaderboard;
		_rank = rank;
		_entry = data;
		_OnEntryChange();
		return this;
	}

	public void OnClassClicked()
	{
		if (_leaderboard.hasClassFilter)
		{
			_leaderboard.ClearClassFilter();
		}
		else
		{
			_leaderboard.SetClassFilter(entry.data.playerClass);
		}
	}

	public void OnReloadClicked()
	{
		if (_leaderboard.reloadFilter == entry.data.reloadCount)
		{
			_leaderboard.ClearReloadFilter();
		}
		else
		{
			_leaderboard.SetReloadFilter(entry.data.reloadCount);
		}
	}
}
