using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class GameStepDeckCreationEditDeck : AGameStepDeckCreation
{
	public class GameStepAnimateListBackIntoDeck : AGameStepDeckCreation
	{
		private int _cardCount;

		private void _OnAbilityRest(DeckCreationPile pile, Ability card)
		{
			if (pile == DeckCreationPile.List)
			{
				card.view.RepoolCard();
			}
		}

		protected override void OnEnable()
		{
			base.abilityLayout.onRest += _OnAbilityRest;
		}

		public override void Start()
		{
			_cardCount = base.abilities.Count(DeckCreationPile.List);
			DataRef<AbilityDeckData> deckRef = base.selectedDeck.deckRef;
			deckRef.data.SetData(base.selectedDeck.abilityDeckView.creation?.nameInputField?.text ?? deckRef.data.name, from a in base.abilities.GetCards(DeckCreationPile.List)
				select a.dataRef, base.creationView.descriptionText);
			if (deckRef.hasUnsavedChanges)
			{
				deckRef.Save(forceOverwrite: true);
			}
			foreach (Ability item in base.abilities.GetCardsSafe(DeckCreationPile.Results))
			{
				base.exile.Transfer(item, ExilePile.ClearGameState);
			}
		}

		protected override IEnumerator Update()
		{
			AbilityDeck deck = base.selectedDeck;
			base.deckLayout.SetLayout(DeckCreationPile.List, base.creationView.deckOpenLayout);
			deck.deckView.Open();
			while (!deck.view.atRestInLayout)
			{
				yield return null;
			}
			base.abilityLayout.SetLayout(DeckCreationPile.List, deck.deckView.cardLayout);
			foreach (Ability card in base.abilities.GetCards(DeckCreationPile.List))
			{
				card.view.ClearExitTransitions();
			}
			while (!base.abilityLayout.GetLayout(DeckCreationPile.List).IsAtRest())
			{
				yield return null;
			}
			deck.deckView.Close();
			base.decks.Transfer(deck, DeckCreationPile.Discard);
		}

		protected override void End()
		{
			DeckCreationStateView deckCreationStateView = base.creationView;
			int maxPageNumber = (base.creationView.pageNumber = 1);
			deckCreationStateView.maxPageNumber = maxPageNumber;
			base.creationView.ClearSuitFilters();
			base.creationView.ClearCountFilters();
			base.creationView.ClearCardCounts();
			base.creationView.ClearSearchText();
			base.creationView.ClearDescription();
		}

		protected override void OnDisable()
		{
			base.abilityLayout.onRest -= _OnAbilityRest;
		}

		protected override void OnDestroy()
		{
			if (_cardCount < 30)
			{
				base.view.LogError(DeckCreationMessage.IncompleteDeck.Localize());
			}
		}
	}

	public class GameStepGenerateResults : GameStep
	{
		private GameStepDeckCreationEditDeck _editDeck;

		public GameStepGenerateResults(GameStepDeckCreationEditDeck editDeck)
		{
			_editDeck = editDeck;
		}

		protected override IEnumerator Update()
		{
			_editDeck._results.Clear();
			if (_editDeck._pages.Count == 0)
			{
				yield break;
			}
			foreach (DataRef<AbilityData> item in _editDeck._pages[_editDeck.pageIndex].value)
			{
				_editDeck._results.Add(new Couple<DataRef<AbilityData>, int>(item, _editDeck.abilityCounts[item] - _editDeck._listCounts.GetValueOrDefault(item)));
			}
			if (!(_editDeck.abilityLayout.GetLayout(DeckCreationPile.Results) is CardSlottedLayoutGroup resultsLayout))
			{
				yield break;
			}
			for (int x = 0; x < resultsLayout.layouts.Length; x++)
			{
				foreach (CardLayoutElement card in resultsLayout.layouts[x].GetCards())
				{
					_editDeck.exile.Transfer(card.card, ExilePile.ClearGameState);
				}
				Couple<DataRef<AbilityData>, int> abilityInSlot = _editDeck._results.GetValueOrDefault(x);
				int addAbilityCount = abilityInSlot;
				_editDeck.abilityLayout.SetLayoutWithoutTransferringCards(DeckCreationPile.Results, resultsLayout.layouts[x]);
				for (int a = 0; a < addAbilityCount; a++)
				{
					_editDeck.abilities.Transfer(_editDeck.abilities.Add(new Ability(abilityInSlot)), DeckCreationPile.Results).abilityCard?.SetHasUsesRemaining(ProfileManager.progress.abilities.read.IsUnlocked(abilityInSlot));
					yield return null;
				}
			}
			_editDeck.abilityLayout.RestoreLayoutToDefaultWithoutTransferringCards(DeckCreationPile.Results);
		}
	}

	private PoolKeepItemDictionaryHandle<DataRef<AbilityData>, int> _abilityCounts;

	private PoolDictionaryValuesHandle<DataRef<AbilityData>, PoolKeepItemHashSetHandle<AbilityKeyword>> _abilitySearchKeywords;

	private Dictionary<DataRef<AbilityData>, int> _listCounts = new Dictionary<DataRef<AbilityData>, int>();

	private PoolKeepItemListHandle<DataRef<AbilityData>> _searchResults;

	private List<PoolKeepItemListHandle<DataRef<AbilityData>>> _pages = new List<PoolKeepItemListHandle<DataRef<AbilityData>>>();

	private List<Couple<DataRef<AbilityData>, int>> _results = new List<Couple<DataRef<AbilityData>, int>>();

	private Dictionary<DataRef<AbilityData>, int> _suitFilterScore = new Dictionary<DataRef<AbilityData>, int>();

	private List<AbilityKeyword> _searchKeywordFilters = new List<AbilityKeyword>();

	private bool _abilityListDirty = true;

	private bool _searchResultsDirty = true;

	private bool _pagesDirty = true;

	private bool _resultsPageDirty = true;

	private Dictionary<DataRef<AbilityData>, int> abilityCounts => _abilityCounts.value;

	private PlayingCardSuits suitFilters => base.creationView.suits;

	private ResourceCardCounts countFilters => base.creationView.counts;

	private bool showUnlockedAbilities => base.creationView.showUnlockedAbilities;

	private bool showLockedAbilities => base.creationView.showLockedAbilities;

	private bool hasSearchFilter
	{
		get
		{
			if (suitFilters == (PlayingCardSuits)0 && countFilters == (ResourceCardCounts)0)
			{
				return base.creationView.searchText.HasVisibleCharacter();
			}
			return true;
		}
	}

	private void _OnAbilityTransfer(Ability card, DeckCreationPile? oldPile, DeckCreationPile? newPile)
	{
		if (oldPile != DeckCreationPile.List && newPile != DeckCreationPile.List)
		{
			return;
		}
		_abilityListDirty = true;
		if (oldPile == DeckCreationPile.List && abilityCounts.GetValueOrDefault(card.dataRef) - _listCounts.GetValueOrDefault(card.dataRef) <= 0)
		{
			int? num = _searchResults?.value?.IndexOf(card.dataRef);
			if (num.HasValue)
			{
				int valueOrDefault = num.GetValueOrDefault();
				if (valueOrDefault >= 0)
				{
					DataRef<AbilityData> dataRef = _pages.GetValueOrDefault(base.pageIndex)?.value?.LastOrDefault();
					if (dataRef != null && valueOrDefault > _searchResults.value.IndexOf(dataRef))
					{
						_SetPagesDirty();
					}
				}
			}
		}
		if (oldPile == DeckCreationPile.List && !newPile.HasValue)
		{
			List<PoolKeepItemListHandle<DataRef<AbilityData>>> pages = _pages;
			if (pages != null && pages.Count < 2 && _IsValidResult(card.dataRef))
			{
				_SetPagesDirty();
			}
		}
	}

	private void _OnAbilityPointerEnter(DeckCreationPile pile, Ability card)
	{
		DataRef<AbilityData> a = card.dataRef.BaseAbilityRef();
		switch (pile)
		{
		case DeckCreationPile.Results:
			if (!card.view.isLastCardInLayout)
			{
				break;
			}
			card.view.RequestGlow(card.view, Colors.TARGET);
			{
				foreach (Ability card2 in base.abilities.GetCards(DeckCreationPile.List))
				{
					if (ContentRef.Equal(a, card2.dataRef.BaseAbilityRef()))
					{
						card2.view.RequestGlow(card.view, Colors.TARGET);
					}
				}
				break;
			}
		case DeckCreationPile.List:
			card.view.RequestGlow(card.view, Colors.TARGET);
			{
				foreach (Ability card3 in base.abilities.GetCards(DeckCreationPile.Results))
				{
					if (ContentRef.Equal(a, card3.dataRef.BaseAbilityRef()) && card3.view.isLastCardInLayout)
					{
						card3.view.RequestGlow(card.view, Colors.TARGET);
					}
				}
				break;
			}
		}
	}

	private void _OnAbilityPointerExit(DeckCreationPile pile, Ability card)
	{
		card.view.ReleaseOwnedGlowRequests();
	}

	private void _OnAbilityClick(DeckCreationPile pile, Ability card)
	{
		if (pile == DeckCreationPile.Results)
		{
			if (!ProfileManager.progress.abilities.read.IsUnlocked(card.dataRef))
			{
				base.view.LogError(DeckCreationMessage.AbilityNotUnlocked.Localize());
			}
			else if (base.abilities.Count(DeckCreationPile.List) < 30)
			{
				if (_listCounts.GetValueOrDefault(card.dataRef) >= card.data.rank.Max())
				{
					if (!DoUpgradeSwap())
					{
						base.view.LogError(((DeckCreationMessage)card.data.rank).Localize());
					}
					return;
				}
				base.abilities.Transfer(card, DeckCreationPile.List, base.abilities.TryGetIndexOf(card, Ability.IdAbilityRefEqualityComparer.Default, DeckCreationPile.List));
				if (base.abilities.Count(DeckCreationPile.Results) == 0 && base.creationView.maxPageNumber > 1)
				{
					_SetPagesDirty();
					_SetResultsPageDirty();
				}
			}
			else if (!DoUpgradeSwap())
			{
				base.view.LogError(DeckCreationMessage.DeckIsFull.Localize());
			}
		}
		else if (pile == DeckCreationPile.List)
		{
			if (_pages[base.pageIndex].value.Contains(card.dataRef) || (_pages.Count < 2 && _IsValidResult(card.dataRef) && base.view.deckCreation.abilities.results.GetCardLayouts().Any((ACardLayout l) => l.Count == 0)))
			{
				base.abilities.Transfer(card, DeckCreationPile.Results);
				return;
			}
			card.view.ReleaseOwnedGlowRequests();
			base.exile.Transfer(card, ExilePile.ClearGameState);
		}
		bool DoUpgradeSwap()
		{
			if (pile != DeckCreationPile.Results || !card.data.upgradeOf)
			{
				return false;
			}
			PoolKeepItemListHandle<DataRef<AbilityData>> downgrades = Pools.UseKeepItemList(card.data.GetDowngrades());
			try
			{
				Ability ability = base.abilities.GetCards(DeckCreationPile.List).FirstOrDefault((Ability a) => downgrades.value.Contains(a.dataRef));
				if (ability != null)
				{
					card.view.deck.SignalPointerClick(ability);
					card.view.deck.SignalPointerClick(card);
					return true;
				}
				return false;
			}
			finally
			{
				if (downgrades != null)
				{
					((IDisposable)downgrades).Dispose();
				}
			}
		}
	}

	private void _OnDeckSmartDrag(ACardLayout smartDragLayout, AbilityDeck card)
	{
		_OnDonePressed();
	}

	private void _SetSearchResultsDirty()
	{
		_searchResultsDirty = true;
		_SetPagesDirty();
		_SetResultsPageDirty();
		base.pageNumber = 1;
	}

	private void _SetPagesDirty()
	{
		_pagesDirty = true;
	}

	private void _SetResultsPageDirty()
	{
		_resultsPageDirty = true;
	}

	private void _OnSearchTextChanged(string searchText)
	{
		_SetSearchResultsDirty();
	}

	private void _OnSuitFilterChanged(PlayingCardSuits suits)
	{
		_SetSearchResultsDirty();
	}

	private void _OnCountFilterChanged(ResourceCardCounts counts)
	{
		_SetSearchResultsDirty();
	}

	private void _OnShowLockedAbilitiesChanged(bool show)
	{
		_SetSearchResultsDirty();
	}

	private void _OnShowUnlockedAbilitiesChanged(bool show)
	{
		_SetSearchResultsDirty();
	}

	private void _OnPageChanged(int previousPage, int page)
	{
		if (page < previousPage)
		{
			_SetPagesDirty();
		}
		_SetResultsPageDirty();
	}

	private bool _FilterSuits(PlayingCard.Filter filter)
	{
		if (suitFilters == EnumUtil<PlayingCardSuits>.AllFlags)
		{
			return !filter.filtersSuit;
		}
		if (!filter.filtersSuit)
		{
			return false;
		}
		if (filter.effectiveSuitFilter == suitFilters)
		{
			return true;
		}
		EnumerateFlags<PlayingCardSuits>.Enumerator enumerator = EnumUtil.Flags(suitFilters).GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayingCardSuits current = enumerator.Current;
			if (filter.effectiveSuitFilter == current)
			{
				return true;
			}
		}
		return false;
	}

	private int _FilterSuitsComparison(DataRef<AbilityData> a, DataRef<AbilityData> b)
	{
		return _suitFilterScore[b] - _suitFilterScore[a];
	}

	private void _UpdateAbilityList()
	{
		_abilityListDirty = false;
		using PoolKeepItemListHandle<Ability> poolKeepItemListHandle = base.abilities.GetCardsSafe(DeckCreationPile.List);
		PlayingCardSuit[] values = EnumUtil<PlayingCardSuit>.Values;
		foreach (PlayingCardSuit suit in values)
		{
			int num = 0;
			foreach (Ability item in poolKeepItemListHandle.value)
			{
				foreach (PlayingCard.Filter resourceFilter in item.data.activation.cost.GetResourceFilters())
				{
					if (resourceFilter.filtersSuit && resourceFilter.IsValid(suit))
					{
						num++;
						break;
					}
				}
			}
			base.creationView.SetCardCount(suit, num);
		}
		base.creationView.cardCount = poolKeepItemListHandle.Count;
		_listCounts.Clear();
		foreach (Ability item2 in poolKeepItemListHandle.value)
		{
			_listCounts[item2.dataRef] = _listCounts.GetValueOrDefault(item2.dataRef) + 1;
		}
		_UpdateListGlows();
	}

	private void _UpdateSearchResults()
	{
		_searchResultsDirty = false;
		using PoolKeepItemListHandle<DataRef<AbilityData>> poolKeepItemListHandle = Pools.UseKeepItemList<DataRef<AbilityData>>();
		_searchKeywordFilters.Clear();
		bool flag = suitFilters > (PlayingCardSuits)0;
		bool flag2 = countFilters > (ResourceCardCounts)0 && countFilters < EnumUtil<ResourceCardCounts>.AllFlags;
		string text = base.creationView.searchText;
		if (text.HasVisibleCharacter())
		{
			text = text.ToLower();
			string text2 = text;
			foreach (KeyValuePair<string, AbilityKeyword> searchKeywordFilter in Ability.SearchKeywordFilters)
			{
				int length = text.Length;
				text = text.ReplaceWords(searchKeywordFilter.Key, "");
				if (length != text.Length)
				{
					_searchKeywordFilters.Add(searchKeywordFilter.Value);
				}
			}
			text = text2;
		}
		bool flag3 = _searchKeywordFilters.Contains(AbilityKeyword.Upgrade);
		PoolKeepItemHashSetHandle<DataRef<AbilityData>> uniqueAbilitiesInDeck = (flag3 ? Pools.UseKeepItemHashSet(from a in base.abilities.GetCards(DeckCreationPile.List)
			select a.dataRef) : null);
		try
		{
			foreach (KeyValuePair<DataRef<AbilityData>, int> abilityCount in abilityCounts)
			{
				DataRef<AbilityData> key = abilityCount.Key;
				if ((flag && key.data.activation.cost.GetResourceFilters().None(_FilterSuits)) || (flag2 && (key.data.activation.cost.cardCounts & countFilters) == 0))
				{
					continue;
				}
				if (showUnlockedAbilities ^ showLockedAbilities)
				{
					bool flag4 = ProfileManager.progress.abilities.read.IsUnlocked(key);
					if ((!showLockedAbilities && !flag4) || (!showUnlockedAbilities && flag4))
					{
						continue;
					}
				}
				if (!flag3 || key.data.GetDowngrades().Any((DataRef<AbilityData> d) => uniqueAbilitiesInDeck.value.Contains(d)))
				{
					poolKeepItemListHandle.Add(key);
				}
			}
			Pools.Repool(ref _searchResults);
			_searchResults = text.FuzzyMatchSort(poolKeepItemListHandle.value, (DataRef<AbilityData> abilityRef) => abilityRef.GetSearchString(), sortOutputWhenSearchStringIsEmpty: true, 5, stableSort: false, AbilityDataRefNameComparer.Default);
			if (_searchKeywordFilters.Count > 0)
			{
				_searchResults.value.StableSort((DataRef<AbilityData> a, DataRef<AbilityData> b) => _searchKeywordFilters.Count((AbilityKeyword f) => _abilitySearchKeywords[b].Contains(f)) - _searchKeywordFilters.Count((AbilityKeyword f) => _abilitySearchKeywords[a].Contains(f)));
			}
			if (flag && !text.HasVisibleCharacter())
			{
				_suitFilterScore.Clear();
				foreach (DataRef<AbilityData> item in _searchResults.value)
				{
					_suitFilterScore[item] = 0;
					foreach (PlayingCard.Filter resourceFilter in item.data.activation.cost.GetResourceFilters())
					{
						if ((bool)resourceFilter || suitFilters == EnumUtil<PlayingCardSuits>.AllFlags)
						{
							_suitFilterScore[item] += ((suitFilters & resourceFilter.effectiveSuitFilter) != 0).ToInt(1, -2) + (suitFilters == resourceFilter.effectiveSuitFilter).ToInt(4);
						}
					}
				}
				_searchResults.value.StableSort(_FilterSuitsComparison);
			}
			_UpdateListGlows();
		}
		finally
		{
			if (uniqueAbilitiesInDeck != null)
			{
				((IDisposable)uniqueAbilitiesInDeck).Dispose();
			}
		}
	}

	private void _UpdateListGlows()
	{
		bool flag = suitFilters > (PlayingCardSuits)0;
		bool flag2 = countFilters > (ResourceCardCounts)0 && countFilters < EnumUtil<ResourceCardCounts>.AllFlags;
		using PoolKeepItemListHandle<AbilityKeyword> poolKeepItemListHandle = Pools.UseKeepItemList(_searchKeywordFilters);
		poolKeepItemListHandle.Remove(AbilityKeyword.Upgrade);
		bool flag3 = poolKeepItemListHandle.Count > 0;
		Color uSED = Colors.USED;
		GlowTags tags = GlowTags.Persistent;
		_ClearGlows();
		if (!(flag || flag2 || flag3))
		{
			return;
		}
		foreach (Ability card in base.abilities.GetCards(DeckCreationPile.List))
		{
			if ((!flag || !card.data.activation.cost.GetResourceFilters().None(_FilterSuits)) && (!flag2 || (card.data.activation.cost.cardCounts & countFilters) != 0) && (!flag3 || _abilitySearchKeywords[card.dataRef].value.ContainsAll(poolKeepItemListHandle.value)))
			{
				card.view.RequestGlow(this, uSED, tags);
			}
		}
	}

	private bool _IsAvailableResult(DataRef<AbilityData> abilityRef)
	{
		return abilityCounts[abilityRef] > _listCounts.GetValueOrDefault(abilityRef);
	}

	private void _UpdatePages()
	{
		_pagesDirty = false;
		Pools.RepoolItems(_pages);
		foreach (IEnumerable<DataRef<AbilityData>> item in _searchResults.value.Where(_IsAvailableResult).AsEnumerable().ToPages(base.pageSize))
		{
			_pages.Add(Pools.UseKeepItemList(item));
		}
		base.creationView.maxPageNumber = _pages.Count;
	}

	private void _UpdateResultsPage()
	{
		_resultsPageDirty = false;
		AppendStep(new GameStepGenerateResults(this));
	}

	private void _OnLocaleChange(Locale locale)
	{
		ContentRef.ClearSearchStringCache();
		Ability.ClearSearchKeywordFilters();
	}

	private bool _IsValidResult(DataRef<AbilityData> ability)
	{
		if (hasSearchFilter)
		{
			PoolKeepItemListHandle<DataRef<AbilityData>> searchResults = _searchResults;
			if (searchResults == null)
			{
				return false;
			}
			return searchResults.value?.Contains(ability) == true;
		}
		return true;
	}

	private void _OnDeckClick(DeckCreationPile pile, AbilityDeck card)
	{
		if (pile == DeckCreationPile.List)
		{
			ShowTraits();
		}
	}

	protected override void OnFirstEnabled()
	{
		PlayerClass characterClass = base.selectedCharacter.characterClass;
		_abilityCounts = ProfileManager.progress.abilities.read.GetAbilityCounts(DevData.Unlocks.abilities);
		foreach (KeyValuePair<DataRef<AbilityData>, int> missingAbilityCount in ProfileManager.progress.abilities.read.GetMissingAbilityCounts())
		{
			if (missingAbilityCount.Key.data.characterClass == characterClass && !_abilityCounts.ContainsKey(missingAbilityCount.Key))
			{
				_abilityCounts[missingAbilityCount.Key] = missingAbilityCount.Value;
			}
		}
		_abilitySearchKeywords = Pools.UseDictionaryValues<DataRef<AbilityData>, PoolKeepItemHashSetHandle<AbilityKeyword>>();
		foreach (DataRef<AbilityData> item in abilityCounts.EnumerateKeysSafe())
		{
			if (item.data.characterClass != characterClass || item.data.category != 0)
			{
				abilityCounts.Remove(item);
			}
		}
		if (DevData.Unlocks.hideAbilitiesWithoutImage)
		{
			foreach (DataRef<AbilityData> item2 in abilityCounts.EnumerateKeysSafe())
			{
				if (!item2.data.cosmetic.hasImage)
				{
					abilityCounts.Remove(item2);
				}
			}
		}
		foreach (DataRef<AbilityData> key in abilityCounts.Keys)
		{
			_abilitySearchKeywords[key] = key.data.GetSearchFilterKeywords();
		}
		base.creationView.onPageNumberChanged += _OnPageChanged;
		base.creationView.onSearchTextChanged += _OnSearchTextChanged;
		base.creationView.onSuitFilterChanged += _OnSuitFilterChanged;
		base.creationView.onCountFilterChanged += _OnCountFilterChanged;
		base.creationView.onShowUnlockedAbilitiesChanged += _OnShowUnlockedAbilitiesChanged;
		base.creationView.onShowLockedAbilitiesChanged += _OnShowLockedAbilitiesChanged;
		base.creationView.title = DeckCreationMessage.EditDeckTitle.Localize();
		base.creationView.doneText = DeckCreationMessage.Done.Localize();
		base.creationView.deckPanelsOpen = true;
		base.creationView.descriptionTextInputField.text = base.selectedDeck.deckRef.data.description;
		LocalizationSettings.Instance.OnSelectedLocaleChanged += _OnLocaleChange;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.abilities.onTransfer += _OnAbilityTransfer;
		base.abilityLayout.onPointerEnter += _OnAbilityPointerEnter;
		base.abilityLayout.onPointerExit += _OnAbilityPointerExit;
		base.abilityLayout.onPointerClick += _OnAbilityClick;
		base.deckLayout.AddSmartDragTarget(DeckCreationPile.List, DeckCreationPile.Results, bidirectional: false);
		base.deckLayout.onSmartDrag += _OnDeckSmartDrag;
		base.deckLayout.onPointerClick += _OnDeckClick;
	}

	protected override void LateUpdate()
	{
		if (_abilityListDirty)
		{
			_UpdateAbilityList();
		}
		if (_searchResultsDirty)
		{
			_UpdateSearchResults();
		}
		if (_pagesDirty)
		{
			_UpdatePages();
		}
		if (_resultsPageDirty)
		{
			_UpdateResultsPage();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		base.abilities.onTransfer -= _OnAbilityTransfer;
		base.abilityLayout.onPointerEnter -= _OnAbilityPointerEnter;
		base.abilityLayout.onPointerExit -= _OnAbilityPointerExit;
		base.abilityLayout.onPointerClick -= _OnAbilityClick;
		base.deckLayout.ClearSmartDragTargets();
		base.deckLayout.onSmartDrag -= _OnDeckSmartDrag;
		base.deckLayout.onPointerClick -= _OnDeckClick;
	}

	protected override void OnFinish()
	{
		base.creationView.onPageNumberChanged -= _OnPageChanged;
		base.creationView.onSearchTextChanged -= _OnSearchTextChanged;
		base.creationView.onSuitFilterChanged -= _OnSuitFilterChanged;
		base.creationView.onCountFilterChanged -= _OnCountFilterChanged;
		base.creationView.onShowUnlockedAbilitiesChanged -= _OnShowUnlockedAbilitiesChanged;
		base.creationView.onShowLockedAbilitiesChanged -= _OnShowLockedAbilitiesChanged;
		base.creationView.deckPanelsOpen = false;
		_ClearGlows();
		AppendStep(new GameStepAnimateListBackIntoDeck());
	}

	protected override void OnDestroy()
	{
		Pools.Repool(ref _abilityCounts);
		Pools.Repool(ref _abilitySearchKeywords);
		LocalizationSettings.Instance.OnSelectedLocaleChanged -= _OnLocaleChange;
	}

	public void ShowTraits()
	{
		AppendStep(new GameStepViewCharacterTraits(base.selectedCharacter));
	}
}
