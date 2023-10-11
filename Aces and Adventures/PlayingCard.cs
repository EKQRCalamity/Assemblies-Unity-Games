using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine.Localization;

[ProtoContract]
public readonly struct PlayingCard : IEquatable<PlayingCard>, IComparable<PlayingCard>
{
	public class DescendingComparer : IComparer<PlayingCard>
	{
		public static readonly DescendingComparer Default = new DescendingComparer();

		public int Compare(PlayingCard x, PlayingCard y)
		{
			return y.CompareTo(x);
		}
	}

	public class DescendingComparerAceLow : IComparer<PlayingCard>
	{
		public static readonly DescendingComparerAceLow Default = new DescendingComparerAceLow();

		public int Compare(PlayingCard a, PlayingCard b)
		{
			int num = b.aceAsLowCardValue - a.aceAsLowCardValue;
			if (num == 0)
			{
				return b.suit - a.suit;
			}
			return num;
		}
	}

	[ProtoContract]
	[UIField]
	public struct Filter : IEquatable<Filter>
	{
		[ProtoMember(1)]
		[UIField("Card Value Filter", 0u, null, null, null, null, null, null, false, null, 5, false, null, maxCount = 0)]
		[UIHorizontalLayout("Filter")]
		public readonly PlayingCardValues? valueFilter;

		[ProtoMember(2)]
		[UIField("Card Suit Filter", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		[UIHorizontalLayout("Filter")]
		public readonly PlayingCardSuits? suitFilter;

		public PlayingCardTypes cards => effectiveValueFilter.ToPlayingCardTypes(effectiveSuitFilter);

		public bool filtersValue
		{
			get
			{
				if (valueFilter > (PlayingCardValues)0)
				{
					return valueFilter != (PlayingCardValues.Two | PlayingCardValues.Three | PlayingCardValues.Four | PlayingCardValues.Five | PlayingCardValues.Six | PlayingCardValues.Seven | PlayingCardValues.Eight | PlayingCardValues.Nine | PlayingCardValues.Ten | PlayingCardValues.Jack | PlayingCardValues.Queen | PlayingCardValues.King | PlayingCardValues.Ace);
				}
				return false;
			}
		}

		public bool filtersSuit
		{
			get
			{
				if (suitFilter > (PlayingCardSuits)0)
				{
					return suitFilter != (PlayingCardSuits.Club | PlayingCardSuits.Diamond | PlayingCardSuits.Heart | PlayingCardSuits.Spade);
				}
				return false;
			}
		}

		public PlayingCardValues effectiveValueFilter
		{
			get
			{
				if (!(valueFilter > (PlayingCardValues)0))
				{
					return PlayingCardValues.Two | PlayingCardValues.Three | PlayingCardValues.Four | PlayingCardValues.Five | PlayingCardValues.Six | PlayingCardValues.Seven | PlayingCardValues.Eight | PlayingCardValues.Nine | PlayingCardValues.Ten | PlayingCardValues.Jack | PlayingCardValues.Queen | PlayingCardValues.King | PlayingCardValues.Ace;
				}
				return valueFilter.Value;
			}
		}

		public PlayingCardSuits effectiveSuitFilter
		{
			get
			{
				if (!(suitFilter > (PlayingCardSuits)0))
				{
					return PlayingCardSuits.Club | PlayingCardSuits.Diamond | PlayingCardSuits.Heart | PlayingCardSuits.Spade;
				}
				return suitFilter.Value;
			}
		}

		public AbilityPreventedBy? abilityPreventedBy => valueFilter.GetAbilityPreventedBy(suitFilter);

		public string adjective
		{
			get
			{
				if (!this)
				{
					return "";
				}
				return ToString();
			}
		}

		public Filter(PlayingCardSuits? suitFilter, PlayingCardValues? valueFilter)
		{
			this.suitFilter = suitFilter;
			this.valueFilter = valueFilter;
		}

		public Filter(PlayingCardValues? valueFilter)
		{
			this.valueFilter = valueFilter;
			suitFilter = null;
		}

		public Filter(PlayingCardSuits? suitFilter)
		{
			this.suitFilter = suitFilter;
			valueFilter = null;
		}

		public bool IsValid(PlayingCardType card)
		{
			if (IsValid(card.Suit()))
			{
				return IsValid(card.Value());
			}
			return false;
		}

		public bool IsValid(PlayingCardSuit suit)
		{
			return ((uint)(1 << (int)suit) & (uint)effectiveSuitFilter) != 0;
		}

		public bool AreValidSuits(PlayingCardSuits suits)
		{
			return (suits & effectiveSuitFilter) != 0;
		}

		public bool IsValid(PlayingCardValue value)
		{
			return ((uint)(1 << (int)value) & (uint)effectiveValueFilter) != 0;
		}

		public bool AreValid(PlayingCardTypes card)
		{
			if ((effectiveSuitFilter & card.Suits()) != 0)
			{
				return (effectiveValueFilter & card.Values()) != 0;
			}
			return false;
		}

		public void WildIntoValid(ResourceCard card)
		{
			if (!IsValid(card.suit))
			{
				card.suit = EnumUtil<PlayingCardSuits>.ConvertFromFlag<PlayingCardSuit>(EnumUtil.MaxActiveFlag(card.suits & suitFilter.Value));
			}
			if (!IsValid(card.value))
			{
				card.value = EnumUtil<PlayingCardValues>.ConvertFromFlag<PlayingCardValue>(EnumUtil.MaxActiveFlag(card.values & valueFilter.Value));
			}
		}

		public bool Contains(Filter other)
		{
			if (EnumUtil.HasFlags(effectiveSuitFilter, other.effectiveSuitFilter))
			{
				return EnumUtil.HasFlags(effectiveValueFilter, other.effectiveValueFilter);
			}
			return false;
		}

		public ResourceCostIconType GetCostIconType()
		{
			bool flag = filtersValue;
			bool flag2 = filtersSuit;
			if (!flag && !flag2)
			{
				return ResourceCostIconType.AnyCard;
			}
			if (flag && flag2)
			{
				return valueFilter.Value.GetCostIconType(suitFilter.Value);
			}
			if (!flag2)
			{
				return valueFilter.Value.GetCostIconType();
			}
			return suitFilter.Value.GetCostIconType();
		}

		public (LocalizedVariableName, object)[] GetCostIconLocalizationVariables()
		{
			PlayingCardValues values = effectiveValueFilter;
			PlayingCardSuits suits = effectiveSuitFilter;
			ResourceCostIconType iconType = GetCostIconType();
			switch (iconType)
			{
			case ResourceCostIconType.Value:
			case ResourceCostIconType.ValueOrHigher:
			case ResourceCostIconType.ValueOrLower:
				return new(LocalizedVariableName, object)[1] { (LocalizedVariableName.Value, LocalizedValue()) };
			case ResourceCostIconType.ValueAndSuit:
			case ResourceCostIconType.ValueOrHigherAndSuit:
			case ResourceCostIconType.ValueOrLowerAndSuit:
				return new(LocalizedVariableName, object)[2]
				{
					(LocalizedVariableName.Value, LocalizedValue()),
					(LocalizedVariableName.Suit, LocalizedSuit())
				};
			case ResourceCostIconType.FaceCardAndSuit:
				return new(LocalizedVariableName, object)[2]
				{
					(LocalizedVariableName.Value, ResourceCostIconType.FaceCard.GetTooltip()),
					(LocalizedVariableName.Suit, LocalizedSuit())
				};
			default:
				return null;
			}
			LocalizedString LocalizedSuit()
			{
				PlayingCardColor? color = suits.GetColor();
				if (color.HasValue)
				{
					PlayingCardColor valueOrDefault = color.GetValueOrDefault();
					return valueOrDefault.LocalizeSuits().SetArgumentsCloned(-1);
				}
				return EnumUtil<PlayingCardSuits>.ConvertFromFlag<PlayingCardSuit>(suits).Localize().SetArgumentsCloned(-1);
			}
			LocalizedString LocalizedValue()
			{
				return EnumUtil<PlayingCardValues>.ConvertFromFlag<PlayingCardValue>(iconType.IsSpecificValue() ? values : (iconType.IsValueOrHigher() ? EnumUtil.MinActiveFlag(values) : EnumUtil.MaxActiveFlag(values))).Localize().SetArgumentsCloned(1);
			}
		}

		public bool Equals(Filter other)
		{
			if (effectiveSuitFilter == other.effectiveSuitFilter)
			{
				return effectiveValueFilter == other.effectiveValueFilter;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Filter other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			PlayingCardSuits? playingCardSuits = suitFilter;
			int num = playingCardSuits.GetHashCode() * 397;
			PlayingCardValues? playingCardValues = valueFilter;
			return num ^ playingCardValues.GetHashCode();
		}

		public static bool operator ==(Filter a, Filter b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Filter a, Filter b)
		{
			return !a.Equals(b);
		}

		public static Filter operator |(Filter a, Filter b)
		{
			return new Filter((!(a.suitFilter > (PlayingCardSuits)0) || !(b.suitFilter > (PlayingCardSuits)0)) ? a.suitFilter : (a.suitFilter | b.suitFilter), (!(a.valueFilter > (PlayingCardValues)0) || !(b.valueFilter > (PlayingCardValues)0)) ? a.valueFilter : (a.valueFilter | b.valueFilter));
		}

		public static implicit operator bool(Filter f)
		{
			if (!f.filtersSuit)
			{
				return f.filtersValue;
			}
			return true;
		}

		public static implicit operator AbilityPreventedBy?(Filter f)
		{
			return f.abilityPreventedBy;
		}

		public override string ToString()
		{
			string text = ((!filtersValue) ? (filtersSuit ? suitFilter.Value.ToText() : "Any Card") : (filtersSuit ? (valueFilter.Value.ToText() + " of " + suitFilter.Value.ToText()) : valueFilter.Value.ToText()));
			if (!text.Contains(","))
			{
				return text;
			}
			return "(" + text + ")";
		}

		public static implicit operator PlayingCardTypes(Filter card)
		{
			return card.cards;
		}
	}

	public const PlayingCardSuits ALL_SUITS = PlayingCardSuits.Club | PlayingCardSuits.Diamond | PlayingCardSuits.Heart | PlayingCardSuits.Spade;

	public const PlayingCardSuits RED_SUITS = PlayingCardSuits.Diamond | PlayingCardSuits.Heart;

	public const PlayingCardSuits BLACK_SUITS = PlayingCardSuits.Club | PlayingCardSuits.Spade;

	public const PlayingCardValues ALL_VALUES = PlayingCardValues.Two | PlayingCardValues.Three | PlayingCardValues.Four | PlayingCardValues.Five | PlayingCardValues.Six | PlayingCardValues.Seven | PlayingCardValues.Eight | PlayingCardValues.Nine | PlayingCardValues.Ten | PlayingCardValues.Jack | PlayingCardValues.Queen | PlayingCardValues.King | PlayingCardValues.Ace;

	public const long ALL_CARDS_LONG = 18014398509481980L;

	public const PlayingCardTypes ALL_CARDS = PlayingCardTypes.TwoOfClubs | PlayingCardTypes.ThreeOfClubs | PlayingCardTypes.FourOfClubs | PlayingCardTypes.FiveOfClubs | PlayingCardTypes.SixOfClubs | PlayingCardTypes.SevenOfClubs | PlayingCardTypes.EightOfClubs | PlayingCardTypes.NineOfClubs | PlayingCardTypes.TenOfClubs | PlayingCardTypes.JackOfClubs | PlayingCardTypes.QueenOfClubs | PlayingCardTypes.KingOfClubs | PlayingCardTypes.AceOfClubs | PlayingCardTypes.TwoOfDiamonds | PlayingCardTypes.ThreeOfDiamonds | PlayingCardTypes.FourOfDiamonds | PlayingCardTypes.FiveOfDiamonds | PlayingCardTypes.SixOfDiamonds | PlayingCardTypes.SevenOfDiamonds | PlayingCardTypes.EightOfDiamonds | PlayingCardTypes.NineOfDiamonds | PlayingCardTypes.TenOfDiamonds | PlayingCardTypes.JackOfDiamonds | PlayingCardTypes.QueenOfDiamonds | PlayingCardTypes.KingOfDiamonds | PlayingCardTypes.AceOfDiamonds | PlayingCardTypes.TwoOfHearts | PlayingCardTypes.ThreeOfHearts | PlayingCardTypes.FourOfHearts | PlayingCardTypes.FiveOfHearts | PlayingCardTypes.SixOfHearts | PlayingCardTypes.SevenOfHearts | PlayingCardTypes.EightOfHearts | PlayingCardTypes.NineOfHearts | PlayingCardTypes.TenOfHearts | PlayingCardTypes.JackOfHearts | PlayingCardTypes.QueenOfHearts | PlayingCardTypes.KingOfHearts | PlayingCardTypes.AceOfHearts | PlayingCardTypes.TwoOfSpades | PlayingCardTypes.ThreeOfSpades | PlayingCardTypes.FourOfSpades | PlayingCardTypes.FiveOfSpades | PlayingCardTypes.SixOfSpades | PlayingCardTypes.SevenOfSpades | PlayingCardTypes.EightOfSpades | PlayingCardTypes.NineOfSpades | PlayingCardTypes.TenOfSpades | PlayingCardTypes.JackOfSpades | PlayingCardTypes.QueenOfSpades | PlayingCardTypes.KingOfSpades | PlayingCardTypes.AceOfSpades;

	public const PlayingCardValues ROYAL_FLUSH_VALUES = PlayingCardValues.Ten | PlayingCardValues.Jack | PlayingCardValues.Queen | PlayingCardValues.King | PlayingCardValues.Ace;

	public const PlayingCardValues FACE_VALUES = PlayingCardValues.Jack | PlayingCardValues.Queen | PlayingCardValues.King;

	public const PlayingCardValuesAceLow ALL_VALUES_ACE_LOW = PlayingCardValuesAceLow.One | PlayingCardValuesAceLow.Two | PlayingCardValuesAceLow.Three | PlayingCardValuesAceLow.Four | PlayingCardValuesAceLow.Five | PlayingCardValuesAceLow.Six | PlayingCardValuesAceLow.Seven | PlayingCardValuesAceLow.Eight | PlayingCardValuesAceLow.Nine | PlayingCardValuesAceLow.Ten | PlayingCardValuesAceLow.Jack | PlayingCardValuesAceLow.Queen | PlayingCardValuesAceLow.King | PlayingCardValuesAceLow.Ace;

	public const PlayingCardValuesAceLow ROYAL_FLUSH_VALUES_ACE_LOW = PlayingCardValuesAceLow.Ten | PlayingCardValuesAceLow.Jack | PlayingCardValuesAceLow.Queen | PlayingCardValuesAceLow.King | PlayingCardValuesAceLow.Ace;

	private static Random _Random;

	private static List<PlayingCardType> _AllPlayingCards;

	[ProtoMember(1)]
	public readonly PlayingCardType type;

	public static Random Random => _Random ?? (_Random = new Random());

	public static List<PlayingCardType> AllPlayingCards => _AllPlayingCards ?? (_AllPlayingCards = ((PlayingCardType[])Enum.GetValues(typeof(PlayingCardType))).ToList());

	public PlayingCardSuit suit => type.Suit();

	public PlayingCardValue value => type.Value();

	public PlayingCardValueAceLow aceAsLowCardValue => value.ToAceLow();

	public static List<PlayingCard> GetRandomHand(int handSize, Random random = null)
	{
		if (random == null)
		{
			random = Random;
		}
		List<PlayingCard> list = new List<PlayingCard>(handSize);
		for (int i = 0; i < handSize; i++)
		{
			if (AllPlayingCards.Count <= 0)
			{
				break;
			}
			int index = random.Next(AllPlayingCards.Count);
			list.Add(AllPlayingCards[index]);
			AllPlayingCards.RemoveAt(index);
		}
		foreach (PlayingCard item in list)
		{
			AllPlayingCards.Add(item);
		}
		return list;
	}

	public PlayingCard(PlayingCardType type)
	{
		this.type = type;
	}

	public PlayingCard(PlayingCardSuit suit, PlayingCardValue value)
	{
		type = (PlayingCardType)(value + (int)suit * 13);
	}

	public PlayingCard ChangeSuit(PlayingCardSuit newSuit)
	{
		return new PlayingCard(newSuit, this);
	}

	public PlayingCard ChangeSuit(PlayingCardSuit? newSuit)
	{
		return new PlayingCard(newSuit ?? ((PlayingCardSuit)this), this);
	}

	public PlayingCard ChangeValue(PlayingCardValue newValue)
	{
		return new PlayingCard(this, newValue);
	}

	public PlayingCard ChangeValue(PlayingCardValue? newValue)
	{
		return new PlayingCard(this, newValue ?? ((PlayingCardValue)this));
	}

	public PlayingCard ShiftValue(int valueShift)
	{
		return new PlayingCard(this, value.Shift(valueShift));
	}

	public PlayingCardTypes ProcessWilds(IEnumerable<AWild> wildModifications)
	{
		PlayingCardTypes playingCardTypes = this;
		if (wildModifications == null)
		{
			return playingCardTypes;
		}
		using PoolKeepItemListHandle<AWild> poolKeepItemListHandle = Pools.UseKeepItemList(wildModifications);
		List<AWild> list = poolKeepItemListHandle.value;
		for (int num = list.Count - 1; num >= 1; num--)
		{
			for (int num2 = num - 1; num2 >= 0; num2--)
			{
				AWild aWild = list[num].CombineWith(list[num2]) ?? list[num2].CombineWith(list[num]);
				if (aWild != null)
				{
					list.RemoveAt(num);
					list.RemoveAt(num2);
					list.Add(aWild);
					break;
				}
			}
		}
		foreach (IEnumerable<AWild> item in list.Permutations())
		{
			PlayingCardTypes cards = this;
			foreach (AWild item2 in item)
			{
				item2.Process(ref cards);
			}
			playingCardTypes |= cards;
		}
		return playingCardTypes;
	}

	public bool Equals(PlayingCard other)
	{
		return other.type == type;
	}

	public int CompareTo(PlayingCard other)
	{
		int num = value - other.value;
		if (num == 0)
		{
			return suit - other.suit;
		}
		return num;
	}

	public override bool Equals(object obj)
	{
		if (obj is PlayingCard other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)type;
	}

	public static bool operator ==(PlayingCard a, PlayingCard b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(PlayingCard a, PlayingCard b)
	{
		return !(a == b);
	}

	public static implicit operator PlayingCardValue(PlayingCard card)
	{
		return card.value;
	}

	public static implicit operator PlayingCardSuit(PlayingCard card)
	{
		return card.suit;
	}

	public static implicit operator PlayingCardType(PlayingCard card)
	{
		return card.type;
	}

	public static implicit operator PlayingCard(PlayingCardType cardType)
	{
		return new PlayingCard(cardType);
	}

	public static implicit operator PlayingCardTypes(PlayingCard card)
	{
		return (PlayingCardTypes)(1L << (int)card.type);
	}

	public override string ToString()
	{
		return type.ToString();
	}
}
