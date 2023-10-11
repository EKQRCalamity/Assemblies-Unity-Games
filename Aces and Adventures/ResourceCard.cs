using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class ResourceCard : ATarget, IComparable<ResourceCard>
{
	[ProtoContract(EnumPassthru = true)]
	public enum Pile
	{
		DrawPile,
		Hand,
		ActivationHand,
		AttackHand,
		DefenseHand,
		TopDeckHand,
		ActivationHandWaiting,
		DiscardPile
	}

	[Flags]
	public enum Piles
	{
		DrawPile = 1,
		Hand = 2,
		ActivationHand = 4,
		AttackHand = 8,
		DefenseHand = 0x10,
		TopDeckHand = 0x20,
		ActivationHandWaiting = 0x40,
		DiscardPile = 0x80
	}

	public enum WildContext
	{
		Automated,
		UserInput
	}

	public class WildSnapshot
	{
		private PoolKeepItemDictionaryHandle<ResourceCard, PlayingCard?> _wildValues;

		static WildSnapshot()
		{
			Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new WildSnapshot(), delegate(WildSnapshot s)
			{
				s.Clear();
			}, delegate(WildSnapshot s)
			{
				s.OnUnpool();
			});
		}

		public static PoolHandle<WildSnapshot> Create(IEnumerable<ResourceCard> cards)
		{
			PoolHandle<WildSnapshot> poolHandle = Pools.Use<WildSnapshot>();
			poolHandle.value.SetValues(cards);
			return poolHandle;
		}

		private WildSnapshot()
		{
		}

		private void OnUnpool()
		{
			_wildValues = Pools.UseKeepItemDictionary<ResourceCard, PlayingCard?>();
		}

		private void SetValues(IEnumerable<ResourceCard> cards)
		{
			foreach (ResourceCard card in cards)
			{
				_wildValues[card] = card.wildValue;
			}
			foreach (ResourceCard key in _wildValues.value.Keys)
			{
				key._suppressWildEvents++;
			}
		}

		private void Clear()
		{
			foreach (KeyValuePair<ResourceCard, PlayingCard?> item in _wildValues.value)
			{
				item.Key.wildValue = item.Value;
				item.Key._suppressWildEvents--;
			}
			Pools.Repool(ref _wildValues);
		}
	}

	public class CustomWildTracker
	{
		private PoolKeepItemHashSetHandle<ResourceCard> _cardsWithCustomWilds;

		public bool hasCustomWilds => _cardsWithCustomWilds.Count > 0;

		public IEnumerable<ResourceCard> cardsWithCustomWilds => _cardsWithCustomWilds.value;

		public bool this[ResourceCard card]
		{
			get
			{
				return _cardsWithCustomWilds.Contains(card);
			}
			set
			{
				_cardsWithCustomWilds.value.AddOrRemove(card, value);
			}
		}

		static CustomWildTracker()
		{
			Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new CustomWildTracker(), delegate(CustomWildTracker c)
			{
				c.Clear();
			}, delegate(CustomWildTracker c)
			{
				c.OnUnpool();
			});
		}

		public static CustomWildTracker Create()
		{
			return Pools.Unpool<CustomWildTracker>();
		}

		private void OnUnpool()
		{
			_cardsWithCustomWilds = Pools.UseKeepItemHashSet<ResourceCard>();
		}

		private void Clear()
		{
			Pools.Repool(ref _cardsWithCustomWilds);
		}
	}

	public class FreezeWildValueSnapshot
	{
		private PoolKeepItemDictionaryHandle<ResourceCard, SnapshotData> _snapshots;

		static FreezeWildValueSnapshot()
		{
			Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new FreezeWildValueSnapshot(), delegate(FreezeWildValueSnapshot s)
			{
				s.Clear();
			}, delegate(FreezeWildValueSnapshot s)
			{
				s.OnUnpool();
			});
		}

		public static PoolHandle<FreezeWildValueSnapshot> Create(IEnumerable<ResourceCard> cardsToFreeze)
		{
			PoolHandle<FreezeWildValueSnapshot> poolHandle = Pools.Use<FreezeWildValueSnapshot>();
			poolHandle.value.SetData(cardsToFreeze);
			return poolHandle;
		}

		private void OnUnpool()
		{
			_snapshots = Pools.UseKeepItemDictionary<ResourceCard, SnapshotData>();
		}

		private void SetData(IEnumerable<ResourceCard> cardsToFreeze)
		{
			foreach (ResourceCard item in cardsToFreeze)
			{
				_snapshots[item] = item;
			}
			foreach (ResourceCard key in _snapshots.value.Keys)
			{
				key._suppressWildEvents++;
				key._FreezeWildValue();
			}
		}

		private void Clear()
		{
			foreach (KeyValuePair<ResourceCard, SnapshotData> item in _snapshots.value)
			{
				item.Value.SetData(item.Key);
				item.Key._suppressWildEvents--;
			}
			Pools.Repool(ref _snapshots);
		}
	}

	public struct SnapshotData
	{
		public readonly PlayingCard natural;

		public readonly PlayingCard? wild;

		public readonly PlayingCardTypes wilds;

		public readonly PlayingCardValues values;

		public readonly PlayingCardSuits suits;

		public SnapshotData(PlayingCard natural, PlayingCard? wild, PlayingCardTypes wilds, PlayingCardValues values, PlayingCardSuits suits)
		{
			this.natural = natural;
			this.wild = wild;
			this.wilds = wilds;
			this.values = values;
			this.suits = suits;
		}

		public void SetData(ResourceCard card)
		{
			card._natural = natural;
			card._wild = wild;
			card._cachedWilds = wilds;
			card._cachedValues = values;
			card._cachedSuits = suits;
		}

		public static implicit operator SnapshotData(ResourceCard card)
		{
			return new SnapshotData(card._natural, card._wild, card.wilds, card.values, card.suits);
		}
	}

	public class NaturalValueComparer : IComparer<ResourceCard>
	{
		public static readonly NaturalValueComparer Ascending = new NaturalValueComparer();

		public int Compare(ResourceCard x, ResourceCard y)
		{
			int num = ((int?)x?.naturalValue.value).GetValueOrDefault() - ((int?)y?.naturalValue.value).GetValueOrDefault();
			if (num == 0)
			{
				int num2 = ((int?)x?.naturalValue.suit).GetValueOrDefault() - ((int?)y?.naturalValue.suit).GetValueOrDefault();
				if (num2 == 0)
				{
					return (x?.isJoker ?? false).ToInt() - (y?.isJoker ?? false).ToInt();
				}
				return num2;
			}
			return num;
		}
	}

	public class LowestWildValueComparer : IComparer<ResourceCard>
	{
		public static readonly LowestWildValueComparer Ascending = new LowestWildValueComparer
		{
			_sign = 1
		};

		public static readonly LowestWildValueComparer Descending = new LowestWildValueComparer
		{
			_sign = -1
		};

		private int _sign;

		public int Compare(ResourceCard x, ResourceCard y)
		{
			int num = ((int?)x?.values).GetValueOrDefault().LowestBit() - ((int?)y?.values).GetValueOrDefault().LowestBit();
			return ((num != 0) ? num : NaturalValueComparer.Ascending.Compare(x, y)) * _sign;
		}
	}

	public class HighestWildValueComparer : IComparer<ResourceCard>
	{
		public static readonly HighestWildValueComparer Ascending = new HighestWildValueComparer
		{
			_sign = 1
		};

		public static readonly HighestWildValueComparer Descending = new HighestWildValueComparer
		{
			_sign = -1
		};

		private int _sign;

		public int Compare(ResourceCard x, ResourceCard y)
		{
			int num = ((int?)x?.values).GetValueOrDefault().HighestBit() - ((int?)y?.values).GetValueOrDefault().HighestBit();
			return ((num != 0) ? num : NaturalValueComparer.Ascending.Compare(x, y)) * _sign;
		}
	}

	public class HighestWildValueWithLowestTieBreakComparer : IComparer<ResourceCard>
	{
		public static readonly HighestWildValueWithLowestTieBreakComparer Ascending = new HighestWildValueWithLowestTieBreakComparer
		{
			_sign = 1
		};

		public static readonly HighestWildValueWithLowestTieBreakComparer Descending = new HighestWildValueWithLowestTieBreakComparer
		{
			_sign = -1
		};

		private int _sign;

		public int Compare(ResourceCard x, ResourceCard y)
		{
			int num = HighestWildValueComparer.Ascending.Compare(x, y);
			return ((num != 0) ? num : LowestWildValueComparer.Ascending.Compare(x, y)) * _sign;
		}
	}

	public class NaturalValueEqualityComparer : IEqualityComparer<ResourceCard>
	{
		public static readonly NaturalValueEqualityComparer Default = new NaturalValueEqualityComparer();

		public bool Equals(ResourceCard x, ResourceCard y)
		{
			if (x != null && x.naturalValue.value.CompareTo(y?.naturalValue.value) == 0)
			{
				return x.isJoker == y?.isJoker;
			}
			return false;
		}

		public int GetHashCode(ResourceCard card)
		{
			return card.naturalValue.value.GetHashCode() + card.isJoker.ToInt(100);
		}
	}

	public class WildIntoSetComparer : IComparer<ResourceCard>
	{
		public static readonly WildIntoSetComparer Default = new WildIntoSetComparer();

		public int Compare(ResourceCard x, ResourceCard y)
		{
			int valueOrDefault = ((int?)y?.values).GetValueOrDefault();
			int valueOrDefault2 = ((int?)x?.values).GetValueOrDefault();
			int num = valueOrDefault.HighestBit() - valueOrDefault2.HighestBit();
			if (num != 0)
			{
				return num;
			}
			int num2 = valueOrDefault.LowestBit() - valueOrDefault2.LowestBit();
			if (num2 != 0)
			{
				return num2;
			}
			return -NaturalValueComparer.Ascending.Compare(x, y);
		}
	}

	public const Piles COMBAT_PILES = Piles.AttackHand | Piles.DefenseHand;

	public const Piles ACT_PILES = Piles.Hand | Piles.ActivationHand | Piles.AttackHand | Piles.DefenseHand;

	private static WildContext? _WildContext;

	public static readonly Func<ResourceCard, bool> IsPermanent;

	[ProtoMember(1)]
	private PlayingCard _natural;

	[ProtoMember(2)]
	private PlayingCard? _wild;

	[ProtoMember(3, OverwriteList = true)]
	private List<AWild> _wildModifications;

	[ProtoMember(4)]
	private PlayingCardSkinType _skin;

	[ProtoMember(5)]
	private BBool _ephemeral;

	private PlayingCardTypes? _cachedWilds;

	private PlayingCardValues? _cachedValues;

	private PlayingCardSuits? _cachedSuits;

	private int _suppressWildEvents;

	private PlayingCardValueAceLow _aceLowValue;

	public Action<PlayingCard?> onWildValueChange;

	public Action<PlayingCardTypes> onWildsChange;

	private static bool WildIsUserInput
	{
		get
		{
			WildContext? wildContext = (_WildContext = WildContext.UserInput);
			return wildContext.HasValue;
		}
	}

	public PlayingCard naturalValue
	{
		get
		{
			return _natural;
		}
		set
		{
			if (!(_natural == value))
			{
				PlayingCard? playingCard = wildValue;
				PlayingCard valueOrDefault = playingCard.GetValueOrDefault();
				if (!playingCard.HasValue)
				{
					valueOrDefault = _natural;
					PlayingCard? playingCard2 = valueOrDefault;
					wildValue = playingCard2;
				}
				_natural = value;
				_CalculateWilds();
			}
		}
	}

	public PlayingCard? wildValue
	{
		get
		{
			return _wild;
		}
		set
		{
			WildContext valueOrDefault = _WildContext.GetValueOrDefault();
			_WildContext = null;
			if (SetPropertyUtility.SetStruct(ref _wild, value) && _suppressWildEvents <= 0)
			{
				onWildValueChange?.Invoke(value);
				base.gameState.SignalWildValueChanged(this, valueOrDefault);
			}
		}
	}

	public PlayingCard? wildValueUserInput
	{
		set
		{
			if (WildIsUserInput)
			{
				wildValue = value;
			}
		}
	}

	public PlayingCard currentValue => _wild ?? _natural;

	public PlayingCardSuit suit
	{
		get
		{
			return currentValue.suit;
		}
		set
		{
			wildValue = new PlayingCard(value, this.value);
		}
	}

	public PlayingCardSuit suitUserInput
	{
		set
		{
			if (WildIsUserInput)
			{
				suit = value;
			}
		}
	}

	public PlayingCardValue value
	{
		get
		{
			return currentValue.value;
		}
		set
		{
			wildValue = new PlayingCard(suit, value);
		}
	}

	public PlayingCardValueAceLow aceLowValue
	{
		get
		{
			return _aceLowValue;
		}
		set
		{
			_aceLowValue = value;
			this.value = value.ToValue();
		}
	}

	public PlayingCardValue valueUserInput
	{
		set
		{
			if (WildIsUserInput)
			{
				this.value = value;
			}
		}
	}

	public bool isWild
	{
		get
		{
			if (_wild.HasValue)
			{
				PlayingCard? wild = _wild;
				PlayingCard natural = _natural;
				if (!wild.HasValue)
				{
					return true;
				}
				if (!wild.HasValue)
				{
					return false;
				}
				return wild.GetValueOrDefault() != natural;
			}
			return false;
		}
	}

	public bool hasWild => (wilds & ~(PlayingCardTypes)_natural) != 0;

	private List<AWild> wildModifications => _wildModifications ?? (_wildModifications = new List<AWild>());

	public PlayingCardTypes wilds
	{
		get
		{
			PlayingCardTypes valueOrDefault = _cachedWilds.GetValueOrDefault();
			if (!_cachedWilds.HasValue)
			{
				valueOrDefault = _natural.ProcessWilds(wildModifications);
				_cachedWilds = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public PlayingCardValues values
	{
		get
		{
			PlayingCardValues valueOrDefault = _cachedValues.GetValueOrDefault();
			if (!_cachedValues.HasValue)
			{
				valueOrDefault = wilds.Values();
				_cachedValues = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public PlayingCardValuesAceLow aceLowValues => values.ToAceLow();

	public PlayingCardSuits suits
	{
		get
		{
			PlayingCardSuits valueOrDefault = _cachedSuits.GetValueOrDefault();
			if (!_cachedSuits.HasValue)
			{
				valueOrDefault = wilds.Suits();
				_cachedSuits = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public PlayingCardSkinType skin
	{
		get
		{
			return _skin;
		}
		set
		{
			_skin = value;
		}
	}

	public IdDeck<Pile, ResourceCard> deck
	{
		get
		{
			if (skin != PlayingCardSkinType.Enemy)
			{
				return base.gameState.player.resourceDeck;
			}
			return base.gameState.enemyResourceDeck;
		}
	}

	public Faction faction
	{
		get
		{
			if (skin != PlayingCardSkinType.Enemy)
			{
				return Faction.Player;
			}
			return Faction.Enemy;
		}
	}

	public Pile pile => deck[this].GetValueOrDefault();

	public ExilePile exilePile
	{
		get
		{
			if (faction != 0)
			{
				return ExilePile.EnemyResource;
			}
			return ExilePile.PlayerResource;
		}
	}

	public BBool ephemeral => _ephemeral ?? (_ephemeral = new BBool());

	public bool permanent => !ephemeral;

	public bool isJoker
	{
		get
		{
			List<AWild> list = _wildModifications;
			if (list != null && list.Count > 0)
			{
				return _wildModifications[0] is JokerWild;
			}
			return false;
		}
	}

	public override bool canBePooled => true;

	private bool _ephemeralSpecified => _ephemeral;

	static ResourceCard()
	{
		IsPermanent = (ResourceCard card) => card.permanent;
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new ResourceCard(), delegate(ResourceCard r)
		{
			r._Clear();
		}, delegate(ResourceCard r)
		{
			r._OnUnpool();
		});
	}

	public static ResourceCard CreateJoker()
	{
		ResourceCard resourceCard = new ResourceCard(PlayingCardType.AceOfSpades, ProfileManager.options.cosmetic.playingCardDeck);
		resourceCard.AddWildModification(new JokerWild());
		return resourceCard;
	}

	private ResourceCard()
	{
	}

	public ResourceCard(PlayingCard natural)
	{
		_natural = natural;
	}

	public ResourceCard(PlayingCard natural, PlayingCardSkinType skin)
		: this(natural)
	{
		_skin = skin;
	}

	private void _CalculateWilds()
	{
		PlayingCardTypes? cachedWilds = _cachedWilds;
		_ClearCachedValues();
		if (cachedWilds != wilds)
		{
			onWildsChange?.Invoke(wilds);
			base.gameState.SignalWildsChanged(this);
			if (_wild.HasValue && (wilds & (PlayingCardTypes)_wild.Value) == (PlayingCardTypes)0L)
			{
				wildValue = null;
			}
		}
	}

	private void _ClearCachedValues()
	{
		_cachedWilds = null;
		_cachedValues = null;
		_cachedSuits = null;
	}

	private ResourceCard _SetNatural(PlayingCard natural)
	{
		_natural = natural;
		return this;
	}

	private void _FreezeWildValue()
	{
		_natural = currentValue;
		_wild = null;
		_cachedWilds = _natural;
		_cachedValues = _cachedWilds.Value.Values();
		_cachedSuits = _cachedWilds.Value.Suits();
	}

	private void _OnUnpool()
	{
		_wild = null;
		_ClearCachedValues();
		_skin = PlayingCardSkinType.Default;
	}

	private void _Clear()
	{
		_wildModifications?.Clear();
	}

	public void AddWildModification(AWild wildModification)
	{
		wildModifications.Add(wildModification);
		_CalculateWilds();
	}

	public void RemoveWildModification(AWild wildModification)
	{
		if (wildModifications.Remove(wildModification))
		{
			_CalculateWilds();
		}
	}

	public bool CanFormPairWith(ResourceCard otherCard)
	{
		return (values & otherCard.values) != 0;
	}

	public bool CanShareSuitWith(ResourceCard otherCard)
	{
		return (suits & otherCard.suits) != 0;
	}

	public static implicit operator PlayingCardTypes(ResourceCard card)
	{
		return card.wilds;
	}

	public static implicit operator PlayingCardValues(ResourceCard card)
	{
		return card.values;
	}

	public static implicit operator PlayingCardSuits(ResourceCard card)
	{
		return card.suits;
	}

	public static implicit operator PlayingCardType(ResourceCard card)
	{
		return card.currentValue;
	}

	public static implicit operator PlayingCard(ResourceCard card)
	{
		return card.currentValue;
	}

	public int CompareTo(ResourceCard other)
	{
		int num = currentValue.CompareTo(other.currentValue);
		if (num == 0)
		{
			int num2 = isJoker.ToInt() - other.isJoker.ToInt();
			if (num2 == 0)
			{
				int num3 = other.ephemeral.value.ToInt() - ephemeral.value.ToInt();
				if (num3 == 0)
				{
					return base.id.CompareTo(other.id);
				}
				return num3;
			}
			return num2;
		}
		return num;
	}

	public override string ToString()
	{
		return string.Format("(<b>Natural Value</b>: [{0}], <b>Wild Values</b>: [{1}], <b>Values</b>: [{2}], <b>Suits</b>: [{3}])", _natural, (wilds != (PlayingCardTypes.TwoOfClubs | PlayingCardTypes.ThreeOfClubs | PlayingCardTypes.FourOfClubs | PlayingCardTypes.FiveOfClubs | PlayingCardTypes.SixOfClubs | PlayingCardTypes.SevenOfClubs | PlayingCardTypes.EightOfClubs | PlayingCardTypes.NineOfClubs | PlayingCardTypes.TenOfClubs | PlayingCardTypes.JackOfClubs | PlayingCardTypes.QueenOfClubs | PlayingCardTypes.KingOfClubs | PlayingCardTypes.AceOfClubs | PlayingCardTypes.TwoOfDiamonds | PlayingCardTypes.ThreeOfDiamonds | PlayingCardTypes.FourOfDiamonds | PlayingCardTypes.FiveOfDiamonds | PlayingCardTypes.SixOfDiamonds | PlayingCardTypes.SevenOfDiamonds | PlayingCardTypes.EightOfDiamonds | PlayingCardTypes.NineOfDiamonds | PlayingCardTypes.TenOfDiamonds | PlayingCardTypes.JackOfDiamonds | PlayingCardTypes.QueenOfDiamonds | PlayingCardTypes.KingOfDiamonds | PlayingCardTypes.AceOfDiamonds | PlayingCardTypes.TwoOfHearts | PlayingCardTypes.ThreeOfHearts | PlayingCardTypes.FourOfHearts | PlayingCardTypes.FiveOfHearts | PlayingCardTypes.SixOfHearts | PlayingCardTypes.SevenOfHearts | PlayingCardTypes.EightOfHearts | PlayingCardTypes.NineOfHearts | PlayingCardTypes.TenOfHearts | PlayingCardTypes.JackOfHearts | PlayingCardTypes.QueenOfHearts | PlayingCardTypes.KingOfHearts | PlayingCardTypes.AceOfHearts | PlayingCardTypes.TwoOfSpades | PlayingCardTypes.ThreeOfSpades | PlayingCardTypes.FourOfSpades | PlayingCardTypes.FiveOfSpades | PlayingCardTypes.SixOfSpades | PlayingCardTypes.SevenOfSpades | PlayingCardTypes.EightOfSpades | PlayingCardTypes.NineOfSpades | PlayingCardTypes.TenOfSpades | PlayingCardTypes.JackOfSpades | PlayingCardTypes.QueenOfSpades | PlayingCardTypes.KingOfSpades | PlayingCardTypes.AceOfSpades)) ? ((object)wilds) : "All", EnumUtil.FriendlyNameFlagRanges(values), EnumUtil.FriendlyName(suits));
	}
}
