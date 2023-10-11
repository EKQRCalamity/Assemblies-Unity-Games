using System;
using System.Collections.Generic;
using System.Linq;

public static class Poker
{
	public const PokerHandTypes FIVE_CARD_HANDS = PokerHandTypes.Straight | PokerHandTypes.Flush | PokerHandTypes.FullHouse | PokerHandTypes.StraightFlush | PokerHandTypes.RoyalFlush | PokerHandTypes.FiveOfAKind;

	public static readonly PokerHandTypes ALL_HANDS_EXCEPT_TWO_PAIR;

	public static readonly PokerHandTypes ALL_HANDS_EXCEPT_TWO_PAIR_AND_FIVE_CARD;

	public static readonly PokerHandType[] PokerHandsDescending;

	public static readonly PokerHandType[] PokerHandsDescendingSize;

	public static readonly PokerHandType[] PokerHandsFindDefenseOrder;

	public static readonly PokerHandType[] PokerHandsFindDefenseOrderReverse;

	static Poker()
	{
		ALL_HANDS_EXCEPT_TWO_PAIR = EnumUtil.AllFlagsExcept(PokerHandTypes.TwoPair);
		ALL_HANDS_EXCEPT_TWO_PAIR_AND_FIVE_CARD = EnumUtil.AllFlagsExcept(PokerHandTypes.TwoPair | PokerHandTypes.Straight | PokerHandTypes.Flush | PokerHandTypes.FullHouse | PokerHandTypes.StraightFlush | PokerHandTypes.RoyalFlush | PokerHandTypes.FiveOfAKind);
		PokerHandsDescending = EnumUtil<PokerHandType>.Values.Reverse().ToArray();
		PokerHandsDescendingSize = PokerHandsDescending.OrderByDescending((PokerHandType hand) => hand.Size()).ToArray();
		PokerHandsFindDefenseOrder = PokerHandsDescending.OrderBy((PokerHandType hand) => hand.Size()).ToArray();
		PokerHandsFindDefenseOrderReverse = PokerHandsDescending.OrderByDescending((PokerHandType hand) => hand.Size()).ToArray();
		Pools.UseKeepItemList<ResourceCard>().IfNotNullDispose();
		Pools.UseKeepItemList<PlayingCard>().IfNotNullDispose();
		Pools.Use<PokerHandScanData>().IfNotNullDispose();
		Pools.Use<ResourceCard.FreezeWildValueSnapshot>().IfNotNullDispose();
		Pools.Use<FlagSum<PlayingCardValues>>().IfNotNullDispose();
		Pools.UseKeepItemHashSet<ResourceCard>().IfNotNullDispose();
		Pools.UseKeepItemHashSet<int>().IfNotNullDispose();
		Pools.UseDictionaryValues<PlayingCardSuit, PoolKeepItemListHandle<ResourceCard>>().IfNotNullDispose();
		Pools.UseKeepItemDictionary<PlayingCardSuit, int>().IfNotNullDispose();
		Pools.UseDictionaryValues<ResourceCard, PoolKeepItemListHandle<ResourceCard>>().IfNotNullDispose();
		EnumUtil<PokerHandType>.Warmup();
		EnumUtil<PokerHandTypes>.Warmup();
		EnumUtil<PlayingCardValue>.Warmup();
		EnumUtil<PlayingCardValues>.Warmup();
		EnumUtil<PlayingCardSuit>.Warmup();
		EnumUtil<PlayingCardSuits>.Warmup();
		EnumUtil<PlayingCardType>.Warmup();
		EnumUtil<PlayingCardTypes>.Warmup();
	}

	public static PokerHandTypes GetHandTypesBySize(int handSize)
	{
		PokerHandTypes pokerHandTypes = (PokerHandTypes)0;
		PokerHandType[] values = EnumUtil<PokerHandType>.Values;
		foreach (PokerHandType pokerHandType in values)
		{
			if (pokerHandType.Size() == handSize)
			{
				pokerHandTypes |= EnumUtil<PokerHandType>.ConvertToFlag<PokerHandTypes>(pokerHandType);
			}
		}
		return pokerHandTypes;
	}

	public static PokerHand GetPokerHand(this IEnumerable<ResourceCard> cards, PokerHandTypes? handTypeFilter = null)
	{
		using PoolKeepItemListHandle<PlayingCard> poolKeepItemListHandle = Pools.UseKeepItemList<PlayingCard>();
		foreach (ResourceCard card in cards)
		{
			poolKeepItemListHandle.Add(card);
		}
		return poolKeepItemListHandle.AsEnumerable().GetPokerHand(handTypeFilter);
	}

	private static PokerHand GetPokerHand(this IEnumerable<PlayingCard> cards, PokerHandTypes? handTypeFilter = null)
	{
		PokerHandTypes types = handTypeFilter ?? EnumUtil<PokerHandTypes>.AllFlags;
		using PoolHandle<PokerHandScanData> poolHandle = Pools.Use<PokerHandScanData>();
		PokerHandScanData pokerHandScanData = poolHandle.value.SetCards(cards);
		if (pokerHandScanData.count == 0)
		{
			return null;
		}
		PokerHandType[] pokerHandsDescending = PokerHandsDescending;
		foreach (PokerHandType pokerHandType in pokerHandsDescending)
		{
			int num = pokerHandType.Size();
			if (pokerHandScanData.count >= num && types.HasType(pokerHandType))
			{
				List<PlayingCard> list2;
				switch (pokerHandType)
				{
				case PokerHandType.HighCard:
					list2 = ContainsXOfAKind(pokerHandScanData, num);
					break;
				case PokerHandType.Pair:
					list2 = ContainsXOfAKind(pokerHandScanData, num);
					break;
				case PokerHandType.ThreeOfAKind:
					list2 = ContainsXOfAKind(pokerHandScanData, num);
					break;
				case PokerHandType.FourOfAKind:
					list2 = ContainsXOfAKind(pokerHandScanData, num);
					break;
				case PokerHandType.FiveOfAKind:
					list2 = ContainsXOfAKind(pokerHandScanData, num);
					break;
				case PokerHandType.TwoPair:
					list2 = ContainsTwoPair(pokerHandScanData);
					break;
				case PokerHandType.Straight:
					list2 = ContainsStraight(pokerHandScanData);
					break;
				case PokerHandType.Flush:
					list2 = ContainsFlush(pokerHandScanData);
					break;
				case PokerHandType.FullHouse:
					list2 = ContainsFullHouse(pokerHandScanData);
					break;
				case PokerHandType.StraightFlush:
					list2 = ContainsStraightFlush(pokerHandScanData);
					break;
				case PokerHandType.RoyalFlush:
				{
					List<PlayingCard> list = ContainsStraightFlush(pokerHandScanData);
					list2 = ((list != null && (PlayingCardValue)list[0] == PlayingCardValue.Ace) ? list : null);
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
				}
				List<PlayingCard> list3 = list2;
				if (list3 != null)
				{
					return new PokerHand(pokerHandType, list3);
				}
			}
		}
		return null;
	}

	private static List<PlayingCard> ContainsXOfAKind(PokerHandScanData data, int xOfAKind)
	{
		if (data.count >= xOfAKind)
		{
			foreach (KeyValuePair<PlayingCardValue, List<PlayingCard>> item in data.cardsByValue)
			{
				if (item.Value.Count >= xOfAKind)
				{
					return new List<PlayingCard>(item.Value.Take(xOfAKind));
				}
			}
		}
		return null;
	}

	private static List<PlayingCard> _ContainsStraight(List<PlayingCard> sortedCards)
	{
		if (sortedCards.Count < 5)
		{
			return null;
		}
		using PoolKeepItemListHandle<PlayingCard> poolKeepItemListHandle = Pools.UseKeepItemList<PlayingCard>();
		bool flag = false;
		int? num = null;
		for (int i = 0; i < sortedCards.Count; i++)
		{
			PlayingCard value = sortedCards[i];
			int value2 = (int)value.value;
			flag = flag || value.value == PlayingCardValue.Ace;
			int valueOrDefault = num.GetValueOrDefault();
			if (!num.HasValue)
			{
				valueOrDefault = value2 + 1;
				num = valueOrDefault;
			}
			int? num2 = num - value2;
			if (num2 == 1)
			{
				poolKeepItemListHandle.Add(value);
			}
			else if (num2 != 0)
			{
				poolKeepItemListHandle.value.Clear();
			}
			num = value2;
			if (poolKeepItemListHandle.Count == 5)
			{
				return new List<PlayingCard>(poolKeepItemListHandle.value);
			}
		}
		if (flag)
		{
			poolKeepItemListHandle.value.Clear();
			num = null;
			using PoolKeepItemListHandle<PlayingCard> poolKeepItemListHandle2 = Pools.UseKeepItemList(sortedCards);
			poolKeepItemListHandle2.value.Sort(PlayingCard.DescendingComparerAceLow.Default);
			for (int j = 0; j < poolKeepItemListHandle2.Count; j++)
			{
				PlayingCard value3 = poolKeepItemListHandle2[j];
				int aceAsLowCardValue = (int)value3.aceAsLowCardValue;
				int valueOrDefault = num.GetValueOrDefault();
				if (!num.HasValue)
				{
					valueOrDefault = aceAsLowCardValue + 1;
					num = valueOrDefault;
				}
				int? num3 = num - aceAsLowCardValue;
				if (num3 == 1)
				{
					poolKeepItemListHandle.Add(value3);
				}
				else if (num3 != 0)
				{
					poolKeepItemListHandle.value.Clear();
				}
				num = aceAsLowCardValue;
				if (poolKeepItemListHandle.Count == 5)
				{
					return new List<PlayingCard>(poolKeepItemListHandle.value);
				}
			}
		}
		return null;
	}

	private static List<PlayingCard> ContainsStraightFlush(PokerHandScanData data)
	{
		if (data.count >= 5)
		{
			foreach (KeyValuePair<PlayingCardSuit, List<PlayingCard>> item in data.cardsBySuit)
			{
				List<PlayingCard> list = _ContainsStraight(item.Value);
				if (list != null)
				{
					return list;
				}
			}
		}
		return null;
	}

	private static List<PlayingCard> ContainsFullHouse(PokerHandScanData data)
	{
		if (data.count < 5)
		{
			return null;
		}
		PlayingCardValue? playingCardValue = null;
		PlayingCardValue? playingCardValue2 = null;
		foreach (KeyValuePair<PlayingCardValue, List<PlayingCard>> item in data.cardsByValue)
		{
			if (!playingCardValue.HasValue && item.Value.Count >= 3)
			{
				playingCardValue = item.Key;
			}
			else if (!playingCardValue2.HasValue && item.Value.Count >= 2)
			{
				playingCardValue2 = item.Key;
			}
			if (playingCardValue.HasValue && playingCardValue2.HasValue)
			{
				return new List<PlayingCard>(data.cardsByValue[playingCardValue.Value].Take(3).Concat(data.cardsByValue[playingCardValue2.Value].Take(2)));
			}
		}
		return null;
	}

	private static List<PlayingCard> ContainsFlush(PokerHandScanData data)
	{
		if (data.count >= 5)
		{
			foreach (KeyValuePair<PlayingCardSuit, List<PlayingCard>> item in data.cardsBySuit)
			{
				if (item.Value.Count >= 5)
				{
					return new List<PlayingCard>(item.Value.Take(5));
				}
			}
		}
		return null;
	}

	private static List<PlayingCard> ContainsStraight(PokerHandScanData data)
	{
		return _ContainsStraight(data.sortedCards);
	}

	private static List<PlayingCard> ContainsTwoPair(PokerHandScanData data)
	{
		if (data.count < 4)
		{
			return null;
		}
		PlayingCardValue? playingCardValue = null;
		PlayingCardValue? playingCardValue2 = null;
		foreach (KeyValuePair<PlayingCardValue, List<PlayingCard>> item in data.cardsByValue)
		{
			if (!playingCardValue.HasValue && item.Value.Count >= 2)
			{
				playingCardValue = item.Key;
			}
			else if (!playingCardValue2.HasValue && item.Value.Count >= 2)
			{
				playingCardValue2 = item.Key;
			}
			if (playingCardValue.HasValue && playingCardValue2.HasValue)
			{
				return new List<PlayingCard>(data.cardsByValue[playingCardValue.Value].Take(2).Concat(data.cardsByValue[playingCardValue2.Value].Take(2)));
			}
		}
		return null;
	}

	public static bool HasType(this PokerHandTypes types, PokerHandType type)
	{
		return ((uint)types & (uint)(1 << (int)type)) != 0;
	}

	public static int Size(this PokerHandType handType)
	{
		switch (handType)
		{
		case PokerHandType.HighCard:
			return 1;
		case PokerHandType.Pair:
			return 2;
		case PokerHandType.TwoPair:
			return 4;
		case PokerHandType.ThreeOfAKind:
			return 3;
		case PokerHandType.FourOfAKind:
			return 4;
		case PokerHandType.Straight:
		case PokerHandType.Flush:
		case PokerHandType.FullHouse:
		case PokerHandType.StraightFlush:
		case PokerHandType.RoyalFlush:
		case PokerHandType.FiveOfAKind:
			return 5;
		default:
			throw new ArgumentOutOfRangeException("handType", handType, null);
		}
	}

	public static string ShortText(this PokerHandType handType)
	{
		return MessageData.Instance.poker.shortHand[handType].Localize();
	}

	public static PokerHandTypes HandsOfSameSize(this PokerHandType handType)
	{
		return GetHandTypesBySize(handType.Size());
	}

	public static PokerHandTypes HandsOfSameSizeOrGreater(this PokerHandType handType)
	{
		PokerHandTypes pokerHandTypes = (PokerHandTypes)0;
		int num = handType.Size();
		int max = (int)EnumUtil<PokerHandTypes>.Max;
		int num2 = 1;
		int num3 = 0;
		while (num2 <= max)
		{
			if (((PokerHandType)num3).Size() >= num)
			{
				pokerHandTypes = (PokerHandTypes)((int)pokerHandTypes | num2);
			}
			num2 *= 2;
			num3++;
		}
		return pokerHandTypes;
	}

	public static int CompareTo(this PokerHandType handType, PokerHand a, PokerHand b)
	{
		int num = a.hand[0].value - b.hand[0].value;
		if (num == 0)
		{
			if (handType != PokerHandType.FullHouse && handType != PokerHandType.TwoPair)
			{
				return 0;
			}
			return a.hand[3].value - b.hand[3].value;
		}
		return num;
	}

	public static PokerHand Offset(this PokerHand hand, int valueOffset)
	{
		return hand?._Offset(valueOffset);
	}

	public static PoolHandle<ResourceCard.FreezeWildValueSnapshot> FreezeWildValues(this IEnumerable<ResourceCard> cards)
	{
		if (cards == null)
		{
			return null;
		}
		return ResourceCard.FreezeWildValueSnapshot.Create(cards);
	}

	public static IEnumerable<ResourceCard> CardsToFreeze(this ResourceCard.CustomWildTracker customWildTracker)
	{
		if (customWildTracker == null || !customWildTracker.hasCustomWilds)
		{
			return null;
		}
		return customWildTracker.cardsWithCustomWilds;
	}

	public static (PoolKeepItemListHandle<ResourceCard> hand, PokerHandType handType) GetCombatHand(this IEnumerable<ResourceCard> currentHand, PokerHandTypes? handTypeFilter, PokerHandType[] handOrder)
	{
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = Pools.UseKeepItemList(currentHand);
		poolKeepItemListHandle.value.StableSort(ResourceCard.HighestWildValueComparer.Descending);
		PokerHandTypes flags = handTypeFilter ?? EnumUtil<PokerHandTypes>.AllFlags;
		foreach (PokerHandType pokerHandType in handOrder)
		{
			if (EnumUtil.HasFlagConvert(flags, pokerHandType) && poolKeepItemListHandle.Count >= pokerHandType.Size())
			{
				PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle2 = pokerHandType._GetActivationCards(poolKeepItemListHandle.value);
				if (poolKeepItemListHandle2 != null)
				{
					return (poolKeepItemListHandle2, pokerHandType);
				}
			}
		}
		return default((PoolKeepItemListHandle<ResourceCard>, PokerHandType));
	}

	public static (PoolKeepItemListHandle<ResourceCard> hand, PokerHandType handType, DefenseResultType result) GetDefenseHand(this IEnumerable<ResourceCard> availableDefenseCards, PokerHand attackHandToBeat, PokerHandTypes? handTypeFilter = null, int tieBreaker = 0, int defenseOffset = 0, Func<List<ResourceCard>, bool> isValidHand = null)
	{
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = Pools.UseKeepItemList(availableDefenseCards);
		(PoolKeepItemListHandle<ResourceCard>, PokerHandType, DefenseResultType)? tuple = null;
		poolKeepItemListHandle.value.StableSort(ResourceCard.HighestWildValueComparer.Ascending);
		PokerHandTypes flags = handTypeFilter ?? EnumUtil<PokerHandTypes>.AllFlags;
		PokerHandType[] pokerHandsFindDefenseOrder = PokerHandsFindDefenseOrder;
		foreach (PokerHandType pokerHandType in pokerHandsFindDefenseOrder)
		{
			if (!EnumUtil.HasFlagConvert(flags, pokerHandType))
			{
				continue;
			}
			using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle2 = Pools.UseKeepItemList(poolKeepItemListHandle.value);
			for (; poolKeepItemListHandle2.Count >= pokerHandType.Size(); poolKeepItemListHandle2.value.RemoveAt(0))
			{
				PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle3 = pokerHandType._GetActivationCards(poolKeepItemListHandle2.value);
				if (poolKeepItemListHandle3 == null)
				{
					break;
				}
				if (isValidHand != null && !isValidHand(poolKeepItemListHandle3))
				{
					continue;
				}
				using (ResourceCard.WildSnapshot.Create(poolKeepItemListHandle3.value))
				{
					(poolKeepItemListHandle3, pokerHandType).WildIntoPokerHand(disposeHand: false);
					int? num = poolKeepItemListHandle3.value.GetPokerHand(EnumUtil<PokerHandType>.ConvertToFlag<PokerHandTypes>(pokerHandType)).Offset(defenseOffset)?.CompareTo(attackHandToBeat);
					if (num == 0)
					{
						num += tieBreaker;
					}
					if (num > 0)
					{
						tuple = (poolKeepItemListHandle3, pokerHandType, DefenseResultType.Success);
						break;
					}
					if (num == 0)
					{
						(PoolKeepItemListHandle<ResourceCard>, PokerHandType, DefenseResultType) valueOrDefault = tuple.GetValueOrDefault();
						if (!tuple.HasValue)
						{
							valueOrDefault = (poolKeepItemListHandle3, pokerHandType, DefenseResultType.Tie);
							tuple = valueOrDefault;
						}
					}
					continue;
				}
			}
		}
		return tuple.GetValueOrDefault();
	}

	public static PoolKeepItemListHandle<ResourceCard> GetActivationHand(this PokerHandType handType, IEnumerable<ResourceCard> availableCards)
	{
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = Pools.UseKeepItemList(availableCards);
		poolKeepItemListHandle.value.Sort(ResourceCard.LowestWildValueComparer.Ascending);
		return handType._GetActivationCards(poolKeepItemListHandle.value);
	}

	public static bool IsValidHand(this PokerHandType handType, IEnumerable<ResourceCard> cards, bool exactMatch = true)
	{
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = Pools.UseKeepItemList(cards);
		if (exactMatch && handType.Size() != poolKeepItemListHandle.Count)
		{
			return false;
		}
		IEnumerable<ResourceCard> cards2;
		if (!exactMatch)
		{
			cards2 = Enumerable.Empty<ResourceCard>();
		}
		else
		{
			IEnumerable<ResourceCard> value = poolKeepItemListHandle.value;
			cards2 = value;
		}
		using (cards2.FreezeWildValues())
		{
			return handType.GetActivationHand(poolKeepItemListHandle.value).IfNotNullDispose();
		}
	}

	private static PoolKeepItemListHandle<ResourceCard> _GetActivationCards(this PokerHandType handType, IEnumerable<ResourceCard> availableCards)
	{
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = Pools.UseKeepItemList(availableCards);
		if (poolKeepItemListHandle.Count < handType.Size())
		{
			return null;
		}
		switch (handType)
		{
		case PokerHandType.HighCard:
			return Pools.UseKeepItemList(poolKeepItemListHandle[0]);
		case PokerHandType.Pair:
		case PokerHandType.ThreeOfAKind:
		case PokerHandType.FourOfAKind:
		case PokerHandType.FiveOfAKind:
			return _GetXOfAKindActivationCards(poolKeepItemListHandle, handType.Size());
		case PokerHandType.TwoPair:
			return _GetTwoPairActivationCards(poolKeepItemListHandle);
		case PokerHandType.Straight:
			return _GetStraightActivationCards(poolKeepItemListHandle);
		case PokerHandType.Flush:
			return _GetFlushActivationCards(poolKeepItemListHandle);
		case PokerHandType.FullHouse:
			return _GetFullHouseActivationCards(poolKeepItemListHandle);
		case PokerHandType.StraightFlush:
			return _GetStraightFlushActivationCards(poolKeepItemListHandle);
		case PokerHandType.RoyalFlush:
			return _GetRoyalFlushActivationCards(poolKeepItemListHandle);
		default:
			throw new ArgumentOutOfRangeException("handType", handType, null);
		}
	}

	private static PoolKeepItemListHandle<ResourceCard> _GetXOfAKindActivationCards(List<ResourceCard> cards, int xOfAKind)
	{
		if (cards.Count < xOfAKind)
		{
			return null;
		}
		using PoolHandle<FlagSum<PlayingCardValues>> poolHandle = Pools.Use<FlagSum<PlayingCardValues>>();
		foreach (ResourceCard card in cards)
		{
			if (poolHandle.value.AddFlags(card) < xOfAKind)
			{
				continue;
			}
			goto IL_0059;
		}
		return null;
		IL_0059:
		PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = Pools.UseKeepItemList<ResourceCard>();
		foreach (IEnumerable<ResourceCard> item in cards.Permutations())
		{
			cards.ClearAndCopyFrom(item);
			for (int i = 0; i < cards.Count; i++)
			{
				poolKeepItemListHandle.Add(cards[i]);
				PlayingCardValues playingCardValues = cards[i];
				for (int j = i + 1; j < cards.Count; j++)
				{
					PlayingCardValues playingCardValues2 = playingCardValues & (PlayingCardValues)cards[j];
					if (playingCardValues2 != 0)
					{
						poolKeepItemListHandle.Add(cards[j]);
						if (poolKeepItemListHandle.Count == xOfAKind)
						{
							return poolKeepItemListHandle;
						}
						playingCardValues = playingCardValues2;
					}
				}
				poolKeepItemListHandle.value.Clear();
			}
			poolKeepItemListHandle.value.Clear();
		}
		return null;
	}

	private static PoolKeepItemListHandle<ResourceCard> _GetTwoPairActivationCards(List<ResourceCard> cards)
	{
		if (cards.Count < 4)
		{
			return null;
		}
		using PoolKeepItemHashSetHandle<ResourceCard> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<ResourceCard>();
		for (int i = 0; i < cards.Count; i++)
		{
			for (int j = i + 1; j < cards.Count; j++)
			{
				if (cards[i].CanFormPairWith(cards[j]))
				{
					poolKeepItemHashSetHandle.Add(cards[i]);
					poolKeepItemHashSetHandle.Add(cards[j]);
				}
			}
		}
		if (poolKeepItemHashSetHandle.Count < 4)
		{
			return null;
		}
		PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = Pools.UseKeepItemList<ResourceCard>();
		foreach (IEnumerable<ResourceCard> item in poolKeepItemHashSetHandle.value.Permutations())
		{
			foreach (var item2 in item.SimplePairings())
			{
				if (item2.Item1.CanFormPairWith(item2.Item2))
				{
					poolKeepItemListHandle.Add(item2.Item1);
					poolKeepItemListHandle.Add(item2.Item2);
					if (poolKeepItemListHandle.Count == 4)
					{
						return poolKeepItemListHandle;
					}
				}
			}
			poolKeepItemListHandle.value.Clear();
		}
		return null;
	}

	private static PoolKeepItemListHandle<ResourceCard> _GetStraightActivationCards(List<ResourceCard> cards, bool mustMatchSuit = false, PlayingCardValuesAceLow validValues = PlayingCardValuesAceLow.One | PlayingCardValuesAceLow.Two | PlayingCardValuesAceLow.Three | PlayingCardValuesAceLow.Four | PlayingCardValuesAceLow.Five | PlayingCardValuesAceLow.Six | PlayingCardValuesAceLow.Seven | PlayingCardValuesAceLow.Eight | PlayingCardValuesAceLow.Nine | PlayingCardValuesAceLow.Ten | PlayingCardValuesAceLow.Jack | PlayingCardValuesAceLow.Queen | PlayingCardValuesAceLow.King | PlayingCardValuesAceLow.Ace)
	{
		if (cards.Count < 5)
		{
			return null;
		}
		PlayingCardValuesAceLow playingCardValuesAceLow = (PlayingCardValuesAceLow)0;
		using PoolKeepItemHashSetHandle<int> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<int>();
		for (int num = cards.Count - 1; num >= 0; num--)
		{
			uint num2 = (uint)(cards[num].aceLowValues & validValues);
			bool flag = poolKeepItemHashSetHandle.Contains(num);
			if (!flag)
			{
				for (int i = 0; i < cards.Count; i++)
				{
					uint num3 = (uint)(cards[i].aceLowValues & validValues);
					if (num != i && (flag = ((num2 << 1) & num3) != 0 || ((num2 >> 1) & num3) != 0))
					{
						if (i < num)
						{
							poolKeepItemHashSetHandle.Add(i);
						}
						break;
					}
				}
			}
			if (!flag)
			{
				cards.RemoveAt(num);
			}
			else
			{
				playingCardValuesAceLow = (PlayingCardValuesAceLow)((int)playingCardValuesAceLow | (int)num2);
			}
		}
		if (cards.Count < 5)
		{
			return null;
		}
		if (playingCardValuesAceLow.Range() < 4)
		{
			return null;
		}
		cards.Sort(ResourceCard.LowestWildValueComparer.Ascending);
		PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = Pools.UseKeepItemList<ResourceCard>();
		foreach (IEnumerable<ResourceCard> item in cards.Permutations())
		{
			int? num4 = null;
			int? num5 = null;
			foreach (ResourceCard item2 in item)
			{
				int num6 = (int)(item2.aceLowValues & validValues);
				if (!num4.HasValue)
				{
					num4 = num6;
				}
				else if ((num4 = (num4 << 1) & num6) == 0)
				{
					break;
				}
				if (mustMatchSuit)
				{
					if (!num5.HasValue)
					{
						num5 = (int)item2.suits;
					}
					else if ((num5 = (int?)((uint?)num5 & (uint)item2.suits)) == 0)
					{
						break;
					}
				}
				poolKeepItemListHandle.Add(item2);
				if (poolKeepItemListHandle.Count == 5)
				{
					return poolKeepItemListHandle;
				}
			}
			poolKeepItemListHandle.value.Clear();
		}
		return null;
	}

	private static PoolKeepItemListHandle<ResourceCard> _GetFlushActivationCards(List<ResourceCard> cards)
	{
		if (cards.Count < 5)
		{
			return null;
		}
		using PoolDictionaryValuesHandle<PlayingCardSuit, PoolKeepItemListHandle<ResourceCard>> poolDictionaryValuesHandle = Pools.UseDictionaryValues<PlayingCardSuit, PoolKeepItemListHandle<ResourceCard>>();
		foreach (ResourceCard card in cards)
		{
			foreach (PlayingCardSuit item in card.suits.Suits())
			{
				if (!poolDictionaryValuesHandle.ContainsKey(item))
				{
					poolDictionaryValuesHandle[item] = Pools.UseKeepItemList<ResourceCard>();
				}
				PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = poolDictionaryValuesHandle[item];
				poolKeepItemListHandle.Add(card);
				if (poolKeepItemListHandle.Count == 5)
				{
					return Pools.UseKeepItemList(poolKeepItemListHandle.value);
				}
			}
		}
		return null;
	}

	private static PoolKeepItemListHandle<ResourceCard> _GetFullHouseActivationCards(List<ResourceCard> cards)
	{
		if (cards.Count < 5)
		{
			return null;
		}
		using PoolKeepItemHashSetHandle<ResourceCard> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<ResourceCard>();
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < cards.Count; i++)
		{
			int num = 1;
			for (int j = i + 1; j < cards.Count; j++)
			{
				if (cards[i].CanFormPairWith(cards[j]))
				{
					poolKeepItemHashSetHandle.Add(cards[i]);
					poolKeepItemHashSetHandle.Add(cards[j]);
					num++;
				}
			}
			switch (num)
			{
			case 2:
				flag = true;
				continue;
			case 1:
				continue;
			}
			if (!flag2)
			{
				flag2 = true;
			}
			else
			{
				flag = true;
			}
		}
		if (!flag || !flag2 || poolKeepItemHashSetHandle.Count < 5)
		{
			return null;
		}
		for (int num2 = cards.Count - 1; num2 >= 0; num2--)
		{
			if (!poolKeepItemHashSetHandle.Contains(cards[num2]))
			{
				cards.RemoveAt(num2);
			}
		}
		PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = Pools.UseKeepItemList<ResourceCard>();
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle2 = Pools.UseKeepItemList<ResourceCard>();
		foreach (IEnumerable<ResourceCard> item in cards.Permutations())
		{
			PlayingCardValues? playingCardValues = null;
			foreach (ResourceCard item2 in item)
			{
				if ((playingCardValues &= (PlayingCardValues)item2) == (PlayingCardValues)0)
				{
					if (poolKeepItemListHandle2.Count == 2 && poolKeepItemListHandle.Count != 2)
					{
						foreach (ResourceCard item3 in poolKeepItemListHandle2.value)
						{
							poolKeepItemListHandle.Add(item3);
						}
					}
					playingCardValues = null;
					poolKeepItemListHandle2.value.Clear();
				}
				PlayingCardValues valueOrDefault = playingCardValues.GetValueOrDefault();
				if (!playingCardValues.HasValue)
				{
					valueOrDefault = item2;
					playingCardValues = valueOrDefault;
				}
				poolKeepItemListHandle2.Add(item2);
				if ((poolKeepItemListHandle2.Count == 3 && poolKeepItemListHandle.Count != 3) || (poolKeepItemListHandle2.Count == 2 && poolKeepItemListHandle.Count == 3))
				{
					foreach (ResourceCard item4 in poolKeepItemListHandle2.value)
					{
						poolKeepItemListHandle.Add(item4);
					}
					playingCardValues = null;
					poolKeepItemListHandle2.value.Clear();
				}
				if (poolKeepItemListHandle.Count == 5)
				{
					return poolKeepItemListHandle;
				}
			}
			poolKeepItemListHandle2.value.Clear();
			poolKeepItemListHandle.value.Clear();
		}
		return null;
	}

	private static PoolKeepItemListHandle<ResourceCard> _GetStraightFlushActivationCards(List<ResourceCard> cards, PlayingCardValuesAceLow validValues = PlayingCardValuesAceLow.One | PlayingCardValuesAceLow.Two | PlayingCardValuesAceLow.Three | PlayingCardValuesAceLow.Four | PlayingCardValuesAceLow.Five | PlayingCardValuesAceLow.Six | PlayingCardValuesAceLow.Seven | PlayingCardValuesAceLow.Eight | PlayingCardValuesAceLow.Nine | PlayingCardValuesAceLow.Ten | PlayingCardValuesAceLow.Jack | PlayingCardValuesAceLow.Queen | PlayingCardValuesAceLow.King | PlayingCardValuesAceLow.Ace)
	{
		if (_GetFlushActivationCards(cards) != null)
		{
			return _GetStraightActivationCards(cards, mustMatchSuit: true, validValues);
		}
		return null;
	}

	private static PoolKeepItemListHandle<ResourceCard> _GetRoyalFlushActivationCards(List<ResourceCard> cards)
	{
		return _GetStraightFlushActivationCards(cards, PlayingCardValuesAceLow.Ten | PlayingCardValuesAceLow.Jack | PlayingCardValuesAceLow.Queen | PlayingCardValuesAceLow.King | PlayingCardValuesAceLow.Ace);
	}

	public static void WildIntoPokerHand(this (PoolKeepItemListHandle<ResourceCard> hand, PokerHandType handType) handData, bool disposeHand = true)
	{
		var (poolKeepItemListHandle, _) = handData;
		if ((bool)poolKeepItemListHandle)
		{
			switch (handData.handType)
			{
			case PokerHandType.HighCard:
			case PokerHandType.Pair:
			case PokerHandType.ThreeOfAKind:
			case PokerHandType.FourOfAKind:
			case PokerHandType.FiveOfAKind:
				_WildIntoXOfAKind(poolKeepItemListHandle);
				break;
			case PokerHandType.Straight:
				_WildIntoStraight(poolKeepItemListHandle);
				break;
			case PokerHandType.Flush:
				_WildIntoFlush(poolKeepItemListHandle);
				break;
			case PokerHandType.TwoPair:
			case PokerHandType.FullHouse:
				_WildIntoSet(poolKeepItemListHandle);
				break;
			case PokerHandType.StraightFlush:
			case PokerHandType.RoyalFlush:
				_WildIntoFlush(poolKeepItemListHandle);
				_WildIntoStraight(poolKeepItemListHandle);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (disposeHand)
			{
				poolKeepItemListHandle.Dispose();
			}
		}
	}

	private static void _WildIntoXOfAKind(List<ResourceCard> cards)
	{
		PlayingCardValues playingCardValues = EnumUtil<PlayingCardValues>.AllFlags;
		foreach (ResourceCard card in cards)
		{
			playingCardValues &= card.values;
		}
		PlayingCardValue value = EnumUtil<PlayingCardValues>.ConvertFromFlag<PlayingCardValue>(EnumUtil.MaxActiveFlag(playingCardValues));
		foreach (ResourceCard card2 in cards)
		{
			card2.value = value;
		}
	}

	private static void _WildIntoStraight(List<ResourceCard> inputCards)
	{
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = Pools.UseKeepItemList(inputCards);
		List<ResourceCard> value = poolKeepItemListHandle.value;
		value.Sort(ResourceCard.HighestWildValueWithLowestTieBreakComparer.Descending);
		PlayingCardValuesAceLow a = (PlayingCardValuesAceLow)0;
		foreach (ResourceCard item in value)
		{
			EnumUtil.Add(ref a, item.aceLowValues);
		}
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle2 = Pools.UseKeepItemList<ResourceCard>();
		List<ResourceCard> value2 = poolKeepItemListHandle2.value;
		foreach (IEnumerable<ResourceCard> item2 in value.Permutations())
		{
			value.ClearAndCopyFrom(item2);
			value2.ClearAndCopyFrom(value);
			for (int num = EnumUtil<PlayingCardValuesAceLow>.Values.Length - 1; num >= 0; num--)
			{
				int num2 = num;
				while (num2 >= 0)
				{
					PlayingCardValuesAceLow playingCardValuesAceLow = EnumUtil<PlayingCardValuesAceLow>.Values[num2];
					if (EnumUtil.HasFlag(a, playingCardValuesAceLow))
					{
						bool flag = false;
						for (int i = 0; i < value2.Count; i++)
						{
							ResourceCard resourceCard = value2[i];
							if (flag = EnumUtil.HasFlag(resourceCard.aceLowValues, playingCardValuesAceLow))
							{
								resourceCard.aceLowValue = EnumUtil<PlayingCardValuesAceLow>.ConvertFromFlag<PlayingCardValueAceLow>(playingCardValuesAceLow);
								value2.RemoveAt(i);
								break;
							}
						}
						if (flag)
						{
							if (value2.Count == 0)
							{
								return;
							}
							num2--;
							continue;
						}
					}
					value2.ClearAndCopyFrom(value);
					break;
				}
			}
		}
	}

	private static void _WildIntoFlush(List<ResourceCard> cards)
	{
		using PoolKeepItemDictionaryHandle<PlayingCardSuit, int> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<PlayingCardSuit, int>();
		PlayingCardSuits validSuits = EnumUtil<PlayingCardSuits>.AllFlags;
		foreach (ResourceCard card in cards)
		{
			poolKeepItemDictionaryHandle[card.suit] = poolKeepItemDictionaryHandle.value.GetValueOrDefault(card.suit) + 1;
			validSuits &= card.suits;
		}
		if (poolKeepItemDictionaryHandle.Count == 1)
		{
			return;
		}
		PlayingCardSuit key = poolKeepItemDictionaryHandle.value.Where((KeyValuePair<PlayingCardSuit, int> p) => EnumUtil.HasFlagConvert(validSuits, p.Key)).MaxBy((KeyValuePair<PlayingCardSuit, int> p) => p.Value).Key;
		foreach (ResourceCard card2 in cards)
		{
			card2.suit = key;
		}
	}

	private static void _WildIntoSet(List<ResourceCard> cards)
	{
		cards.Sort(ResourceCard.WildIntoSetComparer.Default);
		PoolDictionaryValuesHandle<ResourceCard, PoolKeepItemListHandle<ResourceCard>> pairings = Pools.UseDictionaryValues<ResourceCard, PoolKeepItemListHandle<ResourceCard>>();
		try
		{
			for (int i = 0; i < cards.Count; i++)
			{
				ResourceCard resourceCard = cards[i];
				for (int j = i + 1; j < cards.Count; j++)
				{
					ResourceCard resourceCard2 = cards[j];
					if ((resourceCard.values & resourceCard2.values) != 0)
					{
						if (!pairings.ContainsKey(resourceCard))
						{
							pairings.Add(resourceCard, Pools.UseKeepItemList<ResourceCard>());
						}
						if (!pairings.ContainsKey(resourceCard2))
						{
							pairings.Add(resourceCard2, Pools.UseKeepItemList<ResourceCard>());
						}
						pairings[resourceCard].Add(resourceCard2);
						pairings[resourceCard2].Add(resourceCard);
					}
				}
			}
			using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = Pools.UseKeepItemList(cards);
			KeyValuePair<ResourceCard, PoolKeepItemListHandle<ResourceCard>> keyValuePair = pairings.value.MinBy((KeyValuePair<ResourceCard, PoolKeepItemListHandle<ResourceCard>> p) => p.Value.Count);
			foreach (PoolKeepItemListHandle<ResourceCard> value4 in pairings.value.Values)
			{
				value4.Remove(keyValuePair.Key);
			}
			ResourceCard resourceCard3 = keyValuePair.Value.value.MinBy((ResourceCard c) => pairings[c].Count);
			ResourceCard key = keyValuePair.Key;
			PlayingCardValue value = (resourceCard3.value = EnumUtil<PlayingCardValues>.ConvertFromFlag<PlayingCardValue>(EnumUtil.MaxActiveFlag(keyValuePair.Key.values & resourceCard3.values)));
			key.value = value;
			poolKeepItemListHandle.Remove(keyValuePair.Key);
			poolKeepItemListHandle.Remove(resourceCard3);
			PlayingCardValues value2 = EnumUtil<PlayingCardValues>.AllFlags;
			foreach (ResourceCard item in poolKeepItemListHandle.value)
			{
				value2 &= item.values;
			}
			EnumUtil.Subtract(ref value2, EnumUtil<PlayingCardValue>.ConvertToFlag<PlayingCardValues>(resourceCard3.value));
			PlayingCardValue value3 = EnumUtil<PlayingCardValues>.ConvertFromFlag<PlayingCardValue>(EnumUtil.MaxActiveFlag(value2));
			foreach (ResourceCard item2 in poolKeepItemListHandle.value)
			{
				item2.value = value3;
			}
		}
		finally
		{
			if (pairings != null)
			{
				((IDisposable)pairings).Dispose();
			}
		}
	}
}
