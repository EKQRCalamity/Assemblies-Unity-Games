using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class WildValueSelection : MonoBehaviour
{
	private static readonly ResourceBlueprint<GameObject> Blueprint = "GameState/PlayingCard/WildValueSelection";

	[Header("Containers")]
	public RectTransform suitsContainer;

	public RectTransform valuesContainer;

	[Header("Events")]
	public BoolEvent onShowSuitsChange;

	public BoolEvent onShowValuesChange;

	public BoolEvent onInputEnabledChange;

	private ResourceCard _card;

	private WildValueSelectionItem[] _suits;

	private WildValueSelectionItem[] _values;

	public ResourceCard card
	{
		get
		{
			return _card;
		}
		set
		{
			ResourceCard previousCard = _card;
			if (SetPropertyUtility.SetObject(ref _card, value))
			{
				_OnCardChange(previousCard);
			}
		}
	}

	private WildValueSelectionItem this[PlayingCardSuit suit] => _suits[(int)suit];

	private WildValueSelectionItem this[PlayingCardValue value] => _values[(int)(value - 2)];

	public static WildValueSelection Create(ResourceCard card, Transform parent = null)
	{
		return Pools.Unpool(Blueprint, parent).GetComponent<WildValueSelection>()._SetData(card);
	}

	private WildValueSelection _SetData(ResourceCard resourceCard)
	{
		card = resourceCard;
		return this;
	}

	private void _OnCardChange(ResourceCard previousCard)
	{
		if (previousCard != null)
		{
			previousCard.onWildValueChange = (Action<PlayingCard?>)Delegate.Remove(previousCard.onWildValueChange, new Action<PlayingCard?>(_OnWildValueChange));
			previousCard.onWildsChange = (Action<PlayingCardTypes>)Delegate.Remove(previousCard.onWildsChange, new Action<PlayingCardTypes>(_OnWildsChange));
		}
		if (_card != null)
		{
			Show();
			_UpdateValuesAndSuits();
			_UpdateToggles();
			ResourceCard resourceCard = card;
			resourceCard.onWildValueChange = (Action<PlayingCard?>)Delegate.Combine(resourceCard.onWildValueChange, new Action<PlayingCard?>(_OnWildValueChange));
			ResourceCard resourceCard2 = card;
			resourceCard2.onWildsChange = (Action<PlayingCardTypes>)Delegate.Combine(resourceCard2.onWildsChange, new Action<PlayingCardTypes>(_OnWildsChange));
		}
	}

	private void _OnWildsChange(PlayingCardTypes wilds)
	{
		_UpdateValuesAndSuits();
	}

	private void _UpdateValuesAndSuits()
	{
		PlayingCardSuit[] values = EnumUtil<PlayingCardSuit>.Values;
		foreach (PlayingCardSuit playingCardSuit in values)
		{
			this[playingCardSuit].SetIsNatural(playingCardSuit == (PlayingCardSuit)card.naturalValue).gameObject.SetActive(EnumUtil.HasFlagConvert(card.suits, playingCardSuit));
		}
		PlayingCardValue[] values2 = EnumUtil<PlayingCardValue>.Values;
		foreach (PlayingCardValue playingCardValue in values2)
		{
			this[playingCardValue].SetIsNatural(playingCardValue == (PlayingCardValue)card.naturalValue).gameObject.SetActive(EnumUtil.HasFlagConvert(card.values, playingCardValue));
		}
	}

	private void _UpdateToggles()
	{
		this[card.suit].isOn = true;
		this[card.value].isOn = true;
	}

	private void _OnWildValueChange(PlayingCard? wildValue)
	{
		_UpdateToggles();
	}

	private void _OnPointerEnter(PointerEventData eventData)
	{
		InputManager.I.RequestCursorOverride(this, SpecialCursorImage.DefaultCursor);
	}

	private void _OnPointerExit(PointerEventData eventData)
	{
		InputManager.I.ReleaseCursorOverride(this);
	}

	private void Awake()
	{
		suitsContainer.GetOrAddComponent<PointerOver3D>().SetEvents(_OnPointerEnter, _OnPointerExit);
		valuesContainer.GetOrAddComponent<PointerOver3D>().SetEvents(_OnPointerEnter, _OnPointerExit);
		_suits = suitsContainer.GetComponentsInChildren<WildValueSelectionItem>();
		_values = valuesContainer.gameObject.GetComponentsInChildrenPooled<WildValueSelectionItem>().AsEnumerable().Reverse()
			.ToArray();
		PlayingCardSuit[] values = EnumUtil<PlayingCardSuit>.Values;
		foreach (PlayingCardSuit playingCardSuit in values)
		{
			PlayingCardSuit currentSuit = playingCardSuit;
			this[playingCardSuit].onValueChanged.AddListener(delegate(bool b)
			{
				if (b && card != null)
				{
					card.suitUserInput = currentSuit;
				}
			});
		}
		PlayingCardValue[] values2 = EnumUtil<PlayingCardValue>.Values;
		foreach (PlayingCardValue playingCardValue in values2)
		{
			PlayingCardValue currentValue = playingCardValue;
			this[playingCardValue].onValueChanged.AddListener(delegate(bool b)
			{
				if (b && card != null)
				{
					card.valueUserInput = currentValue;
				}
			});
		}
	}

	private void OnEnable()
	{
		GetComponent<Canvas>().worldCamera = Camera.main;
	}

	private void OnDisable()
	{
		card = null;
	}

	private void LateUpdate()
	{
		base.gameObject.SetActive(suitsContainer.gameObject.activeSelf || valuesContainer.gameObject.activeSelf);
	}

	public void Show()
	{
		onShowSuitsChange?.Invoke(EnumUtil.FlagCount(card.suits) > 1);
		onShowValuesChange?.Invoke(EnumUtil.FlagCount(card.values) > 1);
		onInputEnabledChange?.Invoke(GameStateView.Instance?.WildInputEnabled(card) ?? false);
	}

	public void Hide()
	{
		onShowSuitsChange?.Invoke(arg0: false);
		onShowValuesChange?.Invoke(arg0: false);
	}
}
