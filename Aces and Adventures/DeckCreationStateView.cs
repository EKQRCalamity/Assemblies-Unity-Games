using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class DeckCreationStateView : MonoBehaviour
{
	[Range(0f, 3f)]
	public float searchTextWaitTime = 1f;

	public TMP_InputField searchTextInputField;

	public TMP_InputField descriptionTextInputField;

	[Header("Layouts=======================================================================================================================================================")]
	public DeckCreationCharacterDeckLayout characters;

	public DeckCreationDeckDeckLayout decks;

	public DeckCreationAbilityDeckLayout abilities;

	public ExileDeckLayout exile;

	public ACardLayout deckOpenLayout;

	[Header("Events=========================================================================================================================================================")]
	public LocalStringEvent onTitleChange;

	public LocalStringEvent onDoneTextChange;

	public StringEvent onDescriptionTextChange;

	[Header("Suit Filters")]
	public BoolEvent onClubFilterChange;

	public BoolEvent onDiamondFilterChange;

	public BoolEvent onHeartFilterChange;

	public BoolEvent onSpadeFilterChange;

	[Header("Count Filters")]
	public BoolEvent onZeroCountChange;

	public BoolEvent onOneCountChange;

	public BoolEvent onTwoCountChange;

	public BoolEvent onThreePlusCountChange;

	public BoolEvent onShowUnlockedAbilitiesChange;

	public BoolEvent onShowLockedAbilitiesChange;

	[Header("Card Counts")]
	public IntEvent onCardCountChange;

	public IntEvent onClubCountChange;

	public IntEvent onDiamondCountChange;

	public IntEvent onHeartCountChange;

	public IntEvent onSpadeCountChange;

	[Header("Pages")]
	public IntEvent onPageNumberChange;

	public IntEvent onMaxPageNumberChange;

	public StringEvent onCurrentPageTextChange;

	[Header("Animations")]
	public BoolEvent onDeckPanelOpenChange;

	private LocalizedString _title;

	private LocalizedString _doneText;

	private string _descriptionText;

	private string _searchText;

	private float? _searchTextLastSetTime;

	private PlayingCardSuits _suits;

	private ResourceCardCounts _counts;

	private bool _showUnlockedAbilities = true;

	private bool _showLockedAbilities;

	private int _clubCount;

	private int _diamondCount;

	private int _heartCount;

	private int _spadeCount;

	private int _cardCount;

	private int _pageNumber = 1;

	private int _maxPageNumber = 1;

	private bool _deckPanelIsOpen;

	public LocalizedString title
	{
		get
		{
			return _title;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _title, value))
			{
				onTitleChange?.Invoke(value);
			}
		}
	}

	public LocalizedString doneText
	{
		get
		{
			return _doneText;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _doneText, value))
			{
				onDoneTextChange?.Invoke(value);
			}
		}
	}

	public string descriptionText
	{
		get
		{
			return _descriptionText;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _descriptionText, value))
			{
				onDescriptionTextChange?.Invoke(value);
			}
		}
	}

	public string searchText
	{
		get
		{
			return _searchText;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _searchText, value))
			{
				_searchTextLastSetTime = Time.unscaledTime;
			}
		}
	}

	public PlayingCardSuits suits
	{
		get
		{
			return _suits;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _suits, value))
			{
				_OnSuitsChange();
			}
		}
	}

	public ResourceCardCounts counts
	{
		get
		{
			return _counts;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _counts, value))
			{
				_OnCountsChange();
			}
		}
	}

	public bool showUnlockedAbilities
	{
		get
		{
			return _showUnlockedAbilities;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _showUnlockedAbilities, value))
			{
				_OnShowUnlockedAbilitiesChange();
			}
		}
	}

	public bool showLockedAbilities
	{
		get
		{
			return _showLockedAbilities;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _showLockedAbilities, value))
			{
				_OnShowLockedAbilitiesChange();
			}
		}
	}

	public int clubCount
	{
		get
		{
			return _clubCount;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _clubCount, value))
			{
				onClubCountChange?.Invoke(value);
			}
		}
	}

	public int diamondCount
	{
		get
		{
			return _diamondCount;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _diamondCount, value))
			{
				onDiamondCountChange?.Invoke(value);
			}
		}
	}

	public int heartCount
	{
		get
		{
			return _heartCount;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _heartCount, value))
			{
				onHeartCountChange?.Invoke(value);
			}
		}
	}

	public int spadeCount
	{
		get
		{
			return _spadeCount;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _spadeCount, value))
			{
				onSpadeCountChange?.Invoke(value);
			}
		}
	}

	public int cardCount
	{
		get
		{
			return _cardCount;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _cardCount, value))
			{
				onCardCountChange?.Invoke(value);
			}
		}
	}

	public int pageNumber
	{
		get
		{
			return _pageNumber;
		}
		set
		{
			int arg = pageNumber;
			value = Mathf.Clamp(value, 1, maxPageNumber);
			if (SetPropertyUtility.SetStruct(ref _pageNumber, value))
			{
				onPageNumberChange?.Invoke(value);
				this.onPageNumberChanged?.Invoke(arg, value);
			}
		}
	}

	public int maxPageNumber
	{
		get
		{
			return _maxPageNumber;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _maxPageNumber, value = Math.Max(1, value)))
			{
				onMaxPageNumberChange?.Invoke(value);
				if (pageNumber > maxPageNumber)
				{
					pageNumber = maxPageNumber;
				}
			}
		}
	}

	public bool deckPanelsOpen
	{
		get
		{
			return _deckPanelIsOpen;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _deckPanelIsOpen, value))
			{
				onDeckPanelOpenChange?.Invoke(value);
			}
		}
	}

	public event Action<string> onSearchTextChanged;

	public event Action<PlayingCardSuits> onSuitFilterChanged;

	public event Action<ResourceCardCounts> onCountFilterChanged;

	public event Action<bool> onShowUnlockedAbilitiesChanged;

	public event Action<bool> onShowLockedAbilitiesChanged;

	public event Action<int, int> onPageNumberChanged;

	public event Action onDonePressed;

	private void _OnSuitsChange()
	{
		this.onSuitFilterChanged?.Invoke(suits);
		onClubFilterChange?.Invoke(EnumUtil.HasFlag(suits, PlayingCardSuits.Club));
		onDiamondFilterChange?.Invoke(EnumUtil.HasFlag(suits, PlayingCardSuits.Diamond));
		onHeartFilterChange?.Invoke(EnumUtil.HasFlag(suits, PlayingCardSuits.Heart));
		onSpadeFilterChange?.Invoke(EnumUtil.HasFlag(suits, PlayingCardSuits.Spade));
	}

	private void _OnCountsChange()
	{
		this.onCountFilterChanged?.Invoke(counts);
		onZeroCountChange?.Invoke(EnumUtil.HasFlag(counts, ResourceCardCounts.Zero));
		onOneCountChange?.Invoke(EnumUtil.HasFlag(counts, ResourceCardCounts.One));
		onTwoCountChange?.Invoke(EnumUtil.HasFlag(counts, ResourceCardCounts.Two));
		onThreePlusCountChange?.Invoke(EnumUtil.HasFlag(counts, ResourceCardCounts.Three));
	}

	private void _OnShowUnlockedAbilitiesChange()
	{
		onShowUnlockedAbilitiesChange?.Invoke(showUnlockedAbilities);
		this.onShowUnlockedAbilitiesChanged?.Invoke(showUnlockedAbilities);
	}

	private void _OnShowLockedAbilitiesChange()
	{
		onShowLockedAbilitiesChange?.Invoke(showLockedAbilities);
		this.onShowLockedAbilitiesChanged?.Invoke(showLockedAbilities);
	}

	private void _OnPageTextChange()
	{
		onCurrentPageTextChange?.Invoke(DeckCreationMessage.Page.GetMessage() + $" {pageNumber} / {maxPageNumber}");
	}

	private void _UpdateSearchText()
	{
		if (Time.unscaledTime - _searchTextLastSetTime >= searchTextWaitTime)
		{
			this.onSearchTextChanged?.Invoke(searchText);
			_searchTextLastSetTime = null;
		}
	}

	private void Awake()
	{
		onPageNumberChange?.AddListener(delegate
		{
			_OnPageTextChange();
		});
		onMaxPageNumberChange?.AddListener(delegate
		{
			_OnPageTextChange();
		});
		_OnShowUnlockedAbilitiesChange();
		_OnShowLockedAbilitiesChange();
	}

	private void Update()
	{
		_UpdateSearchText();
	}

	public void SetState(DeckCreationState state)
	{
		characters.deck = state.characters;
		decks.deck = state.decks;
		exile.deck = state.exile;
		state.abilities.AddSortedPile(DeckCreationPile.List);
		abilities.GetLayout(DeckCreationPile.Results).groupingEqualityComparer = AbilityDataRefEqualityComparer.Default;
		abilities.GetLayout(DeckCreationPile.List).groupingEqualityComparer = AbilityDataRefEqualityComparer.Default;
		abilities.deck = state.abilities;
	}

	public int SetCardCount(PlayingCardSuit suit, int count)
	{
		return suit switch
		{
			PlayingCardSuit.Club => clubCount = count, 
			PlayingCardSuit.Diamond => diamondCount = count, 
			PlayingCardSuit.Heart => heartCount = count, 
			PlayingCardSuit.Spade => spadeCount = count, 
			_ => 0, 
		};
	}

	public void ClearSuitFilters()
	{
		suits = EnumUtil<PlayingCardSuits>.NoFlags;
	}

	public void ToggleSuitFilters()
	{
		suits = ((EnumUtil.FlagCount(suits) != 0) ? EnumUtil<PlayingCardSuits>.NoFlags : EnumUtil<PlayingCardSuits>.AllFlags);
	}

	public void ToggleClubFilter()
	{
		suits = EnumUtil.ToggleFlag(suits, PlayingCardSuits.Club);
	}

	public void ToggleDiamondFilter()
	{
		suits = EnumUtil.ToggleFlag(suits, PlayingCardSuits.Diamond);
	}

	public void ToggleHeartFilter()
	{
		suits = EnumUtil.ToggleFlag(suits, PlayingCardSuits.Heart);
	}

	public void ToggleSpadeFilter()
	{
		suits = EnumUtil.ToggleFlag(suits, PlayingCardSuits.Spade);
	}

	public void ClearCountFilters()
	{
		counts = EnumUtil<ResourceCardCounts>.NoFlags;
	}

	public void ToggleZeroCount()
	{
		counts = EnumUtil.ToggleFlag(counts, ResourceCardCounts.Zero);
	}

	public void ToggleOneCount()
	{
		counts = EnumUtil.ToggleFlag(counts, ResourceCardCounts.One);
	}

	public void ToggleTwoCount()
	{
		counts = EnumUtil.ToggleFlag(counts, ResourceCardCounts.Two);
	}

	public void ToggleThreePlusCount()
	{
		counts = EnumUtil.ToggleFlag(counts, ResourceCardCounts.Three | ResourceCardCounts.Four | ResourceCardCounts.Five);
	}

	public void ToggleShowUnlocked()
	{
		showUnlockedAbilities = !showUnlockedAbilities;
	}

	public void ToggleShowLocked()
	{
		showLockedAbilities = !showLockedAbilities;
	}

	public void ShowTraits()
	{
		(GameStepStack.Active?.activeStep as GameStepDeckCreationEditDeck)?.ShowTraits();
	}

	public void NextPage()
	{
		int num = pageNumber + 1;
		pageNumber = num;
	}

	public void PreviousPage()
	{
		int num = pageNumber - 1;
		pageNumber = num;
	}

	public void SubmitSearchText()
	{
		if (_searchTextLastSetTime.HasValue)
		{
			_searchTextLastSetTime = 0f;
		}
	}

	public void ClearCardCounts()
	{
		int num2 = (cardCount = 0);
		int num4 = (spadeCount = num2);
		int num6 = (heartCount = num4);
		int num8 = (diamondCount = num6);
		clubCount = num8;
	}

	public void ClearSearchText()
	{
		searchTextInputField.text = "";
	}

	public void ClearDescription()
	{
		descriptionTextInputField.text = "";
	}

	public void Done()
	{
		this.onDonePressed?.Invoke();
	}
}
