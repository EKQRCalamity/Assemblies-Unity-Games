using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Localization;

[ProtoContract]
public class IdDeck<K, V> : IdDeckBase where K : struct, IConvertible where V : ATarget
{
	public delegate void OnTransfer(V card, K? oldPile, K? newPile);

	public delegate void OnReorder(V card, K pile);

	public delegate void Event(K pile, V card);

	public delegate void SmartDrag(ACardLayout smartDragTarget, V card);

	public abstract class AGameStep : GameStep
	{
		protected IdDeck<K, V> _deck;

		protected AGameStep(IdDeck<K, V> deck)
		{
			_deck = deck;
		}
	}

	public class GameStepDraw : AGameStep
	{
		protected int _count;

		private readonly K _drawFrom;

		private readonly K _drawTo;

		private readonly K _shuffleFrom;

		private readonly float _wait;

		private float _waitRemaining;

		private V _lastDrawnValue;

		private bool _canShuffle;

		public virtual int countRemaining => _count;

		public K drawTo => _drawTo;

		public V lastDrawnValue => _lastDrawnValue;

		public GameStepDraw(IdDeck<K, V> deck, int count, K drawFrom, K drawTo, K shuffleFrom, float wait)
			: base(deck)
		{
			_count = count;
			_drawFrom = drawFrom;
			_drawTo = drawTo;
			_shuffleFrom = shuffleFrom;
			_wait = wait;
			_waitRemaining = wait;
			_canShuffle = !EnumUtil.Equals(drawFrom, shuffleFrom);
		}

		protected virtual void _OnDraw()
		{
			_count--;
		}

		protected override IEnumerator Update()
		{
			while (countRemaining > 0)
			{
				if (!_canShuffle)
				{
					_count = Math.Min(_count, _deck.Count(_drawFrom));
					if (_count == 0)
					{
						break;
					}
				}
				if (_deck[_drawFrom].Count == 0)
				{
					yield return AppendStep(new GameStepShuffle(_deck, _shuffleFrom, _drawFrom));
				}
				if (_deck[_drawFrom].Count == 0)
				{
					break;
				}
				_OnDraw();
				yield return _lastDrawnValue = _deck._Draw(_drawFrom, _drawTo);
				while ((_waitRemaining -= Time.deltaTime) > 0f)
				{
					yield return null;
				}
				_waitRemaining = _wait;
			}
		}
	}

	public class GameStepDrawToSize : GameStepDraw
	{
		private Func<V, bool> _countsTowardsCount;

		public override int countRemaining => _count - _deck.Count(base.drawTo, _countsTowardsCount);

		public GameStepDrawToSize(IdDeck<K, V> deck, int count, K drawFrom, K drawTo, K shuffleFrom, float wait, Func<V, bool> countsTowardsCount)
			: base(deck, count, drawFrom, drawTo, shuffleFrom, wait)
		{
			_countsTowardsCount = countsTowardsCount;
		}

		protected override void _OnDraw()
		{
		}
	}

	public class GameStepDrawFiltered : AGameStep
	{
		private int _count;

		private K _drawFrom;

		private K _drawTo;

		private float _wait;

		private float _waitRemaining;

		private Func<V, bool> _filter;

		private bool _shuffleInto;

		public GameStepDrawFiltered(IdDeck<K, V> deck, int count, K drawFrom, K drawTo, float wait, Func<V, bool> filter, bool shuffleInto)
			: base(deck)
		{
			_count = count;
			_drawFrom = drawFrom;
			_drawTo = drawTo;
			_wait = (_waitRemaining = wait);
			_filter = filter;
			_shuffleInto = shuffleInto;
		}

		protected override IEnumerator Update()
		{
			while (_count > 0)
			{
				V val = _deck.GetCards(_drawFrom).Reverse().FirstOrDefault(_filter);
				if (val != null && --_count >= 0)
				{
					_deck.layout.TransferWithSpecialTransitions(val, _drawTo, _shuffleInto ? new int?(base.state.random.Next(_deck.Count(_drawTo))) : null);
					while ((_waitRemaining -= Time.deltaTime) > 0f)
					{
						yield return null;
					}
					_waitRemaining = _wait;
					continue;
				}
				break;
			}
		}
	}

	public class GameStepShuffle : AGameStep
	{
		private readonly K _shuffleFrom;

		private readonly K _shuffleTo;

		private bool _shuffle;

		public GameStepShuffle(IdDeck<K, V> deck, K shuffleFrom, K shuffleTo, bool shuffle = true)
			: base(deck)
		{
			_shuffleFrom = shuffleFrom;
			_shuffleTo = shuffleTo;
			_shuffle = shuffle;
		}

		protected override IEnumerator Update()
		{
			float elapsed = 0f;
			using PoolKeepItemListHandle<V> cards = _deck.GetCardsSafe(_shuffleFrom);
			if (_shuffle)
			{
				cards.value.Shuffle(base.state.random);
			}
			int index = 0;
			while (index < cards.Count)
			{
				for (elapsed += Time.deltaTime; elapsed >= MathUtil.OneOver60; elapsed -= MathUtil.OneOver60)
				{
					_deck.Transfer(cards[index++], _shuffleTo);
					if (index == cards.Count)
					{
						yield break;
					}
				}
				yield return null;
			}
		}

		protected override void End()
		{
			base.state.SignalDeckShuffled(_deck);
		}
	}

	public class GameStepTransfer : AGameStep
	{
		private Id<V> _card;

		private K _pileToTransferTo;

		public GameStepTransfer(IdDeck<K, V> deck, V card, K pileToTransferTo)
			: base(deck)
		{
			_card = card;
			_pileToTransferTo = pileToTransferTo;
		}

		protected override IEnumerator Update()
		{
			yield return _deck.Transfer(_card, _pileToTransferTo);
		}
	}

	public class GameStepTransferChoice : AGameStep
	{
		private int _count;

		private K _transferFrom;

		private K _transferTo;

		public GameStepTransferChoice(IdDeck<K, V> deck, int count, K transferFrom, K transferTo)
			: base(deck)
		{
			_count = count;
			_transferFrom = transferFrom;
			_transferTo = transferTo;
		}

		private void _OnPointerClick(K pile, V card)
		{
			if (EnumUtil.Equals(_transferFrom, pile))
			{
				_count--;
				_deck.Transfer(card, _transferTo);
			}
		}

		protected override void OnEnable()
		{
			_deck.layout.onPointerClick += _OnPointerClick;
		}

		protected override IEnumerator Update()
		{
			while (_count > 0)
			{
				yield return null;
			}
		}

		protected override void OnDisable()
		{
			_deck.layout.onPointerClick -= _OnPointerClick;
		}
	}

	public class GameStepInspectPile<F> : AGameStep where F : struct, IConvertible
	{
		private static readonly float DELAY = 1f / 60f;

		private F _piles;

		private IComparer<V> _sort;

		private IEqualityComparer<CardLayoutElement> _grouping;

		private MessageData.GameTooltips? _message;

		protected CardHandLayoutSettings _layoutSettings;

		protected IEnumerable<V> _additionalCards;

		protected Func<V, bool> _clearExitTransition;

		protected Action<V> _onEnterInspect;

		protected Action<V> _onExitInspect;

		private bool _dragEntireHand;

		private PoolKeepItemDictionaryHandle<V, ACardLayout> _originalOrder;

		private LayoutOffset _layoutOffset;

		protected bool _done;

		private float _elapsed;

		protected bool _finishedAnimatingIn { get; private set; }

		protected Dictionary<V, ACardLayout> originalOrder => _originalOrder?.value;

		protected MessageData.GameTooltips? message => _message;

		public int count => (_originalOrder?.value?.Count).GetValueOrDefault();

		private IEnumerable<ACardLayout> _layoutsToOffset
		{
			get
			{
				yield return base.view.playerResourceDeckLayout.hand;
				yield return base.view.playerAbilityDeckLayout.hand;
				yield return base.view.playerResourceDeckLayout.activationHand;
				yield return base.view.playerResourceDeckLayout.select;
				yield return base.view.adventureDeckLayout.selectionHand;
				foreach (ACardLayout cardLayout in base.view.heroDeckLayout.selectionHandUnrestricted.GetCardLayouts())
				{
					yield return cardLayout;
				}
			}
		}

		protected IEnumerable<V> _cards
		{
			get
			{
				IEnumerable<V> enumerable = _originalOrder?.value?.Keys;
				return enumerable ?? Enumerable.Empty<V>();
			}
		}

		protected virtual ButtonCardType _cancelButtonType => ButtonCardType.Back;

		protected virtual bool _showViewMap => false;

		public override bool canSafelyCancelStack => true;

		public override bool canInspect => false;

		public GameStepInspectPile(IdDeck<K, V> deck, F piles, IComparer<V> sort = null, IEqualityComparer<CardLayoutElement> grouping = null, MessageData.GameTooltips? message = null, CardHandLayoutSettings layoutSettings = null, IEnumerable<V> additionalCards = null, Func<V, bool> clearExitTransition = null, Action<V> onEnterInspect = null, Action<V> onExitInspect = null, bool dragEntireHand = false)
			: base(deck)
		{
			_piles = piles;
			_sort = sort;
			_grouping = grouping;
			_message = message;
			_layoutSettings = layoutSettings;
			_additionalCards = additionalCards;
			_clearExitTransition = clearExitTransition;
			_onEnterInspect = onEnterInspect;
			_onExitInspect = onExitInspect;
			_dragEntireHand = dragEntireHand;
		}

		private void _OnStoneClick(Stone.Pile pile, Stone card)
		{
			if (pile == Stone.Pile.Cancel && _finishedAnimatingIn)
			{
				_done = true;
			}
		}

		private void _OnBackPressed()
		{
			_done |= _finishedAnimatingIn && _cancelButtonType == ButtonCardType.Back;
		}

		protected void _RemoveCard(V card)
		{
			_originalOrder?.value?.Remove(card);
		}

		private void _OnMapClick(ProceduralMap.Pile pile, ProceduralMap card)
		{
			if (pile == ProceduralMap.Pile.Hidden)
			{
				_OnBackPressed();
				GameStepHideProceduralMap.ActiveStep?.ShowMap();
			}
		}

		protected virtual LocalizedString _GetMessage()
		{
			return _message?.Localize().SetVariables((LocalizedVariableName.Card, _message.Value.GetInspectedCardLocalize().SetArguments(_originalOrder.Count)));
		}

		protected override void OnFirstEnabled()
		{
			_originalOrder = Pools.UseKeepItemDictionary<V, ACardLayout>();
			foreach (V item in _deck.GetCards(_piles).Concat(_additionalCards ?? Enumerable.Empty<V>()))
			{
				_originalOrder[item] = item.view.layout;
			}
			_elapsed = DELAY;
		}

		protected override void OnEnable()
		{
			if (!_message.HasValue)
			{
				base.view.ClearMessage();
			}
			else
			{
				base.view.LogMessage(_GetMessage());
			}
			base.state.stoneDeck.layout.onPointerClick += _OnStoneClick;
			base.view.onBackPressed += _OnBackPressed;
			base.view.stoneDeckLayout.SetLayout(Stone.Pile.Cancel, base.view.stoneDeckLayout.cancelFloating);
			base.state.buttonDeck.Layout<ButtonDeckLayout>().Activate(_cancelButtonType);
			base.state.stoneDeck.Layout<StoneDeckLayout>()[StoneType.Cancel].view.RequestGlow(this, Colors.TARGET);
			base.view.adventureDeckLayout.inspectHand.groupingEqualityComparer = _grouping;
			_layoutOffset = new LayoutOffset(_layoutsToOffset, base.view.adventureDeckLayout.inspectHand.layoutTarget.transform.GetPlane(PlaneAxes.XZ).InvertNormal().TranslateReturn(base.view.adventureDeckLayout.inspectHand.layoutTarget.transform.up * 0.115f));
			if (_showViewMap)
			{
				MapCompassView instance = MapCompassView.Instance;
				if ((object)instance != null)
				{
					instance.canBeActiveWhileCancelIsActive = true;
				}
			}
			((CardHandLayout)base.view.adventureDeckLayout.inspectHand).settings = _layoutSettings ?? base.view.adventureDeckLayout.inspectSettings;
			((CardHandLayout)base.view.adventureDeckLayout.inspectHand).dragEntireHand = _dragEntireHand;
			base.view.mapDeckLayout.onPointerClick += _OnMapClick;
		}

		protected override IEnumerator Update()
		{
			if (_originalOrder?.value == null)
			{
				yield break;
			}
			IEnumerable<V> enumerable;
			if (_sort == null)
			{
				enumerable = _originalOrder.value.Keys.AsEnumerable();
			}
			else
			{
				IEnumerable<V> enumerable2 = _originalOrder.value.Keys.OrderBy((V c) => c, _sort);
				enumerable = enumerable2;
			}
			foreach (V item in enumerable)
			{
				_layoutOffset?.ClearOffset(item.view);
				base.view.adventureDeckLayout.inspectHand.Add(item.view);
				_onEnterInspect?.Invoke(item);
				item.view.frontIsVisible = true;
				while (_elapsed > 0f)
				{
					_elapsed -= Time.deltaTime;
					yield return null;
				}
				_elapsed += DELAY;
			}
			_finishedAnimatingIn = true;
			while (!_done)
			{
				yield return null;
			}
			OnDisable();
			foreach (V item2 in _originalOrder.value.Keys.Reverse())
			{
				_originalOrder.value.GetValueOrDefault(item2)?.Add(item2.view);
				if (_clearExitTransition?.Invoke(item2) ?? false)
				{
					item2.view.ClearExitTransitions();
				}
				_onExitInspect?.Invoke(item2);
			}
			Pools.Repool(ref _originalOrder);
		}

		protected override void OnDisable()
		{
			base.state.stoneDeck.layout.onPointerClick -= _OnStoneClick;
			base.view.onBackPressed -= _OnBackPressed;
			base.state.buttonDeck.Layout<ButtonDeckLayout>().Deactivate(_cancelButtonType);
			base.view.stoneDeckLayout.RestoreLayoutToDefault(Stone.Pile.Cancel);
			base.view.StartCoroutine(_layoutOffset.ClearLayoutOffsetsDelayed());
			if (_message.HasValue)
			{
				base.view.ClearMessage();
			}
			if (_showViewMap)
			{
				MapCompassView instance = MapCompassView.Instance;
				if ((object)instance != null)
				{
					instance.canBeActiveWhileCancelIsActive = false;
				}
			}
			((CardHandLayout)base.view.adventureDeckLayout.inspectHand).dragEntireHand = false;
			base.view.mapDeckLayout.onPointerClick -= _OnMapClick;
		}
	}

	[ProtoMember(1, OverwriteList = true)]
	private Dictionary<K, List<Id<V>>> _piles;

	[ProtoMember(2, OverwriteList = true)]
	private Dictionary<ushort, K?> _pileMap;

	[ProtoMember(3, OverwriteList = true)]
	private HashSet<K> _sortedPiles;

	private Dictionary<K, List<Id<V>>> piles => _piles ?? (_piles = new Dictionary<K, List<Id<V>>>());

	private Dictionary<ushort, K?> pileMap => _pileMap ?? (_pileMap = new Dictionary<ushort, K?>());

	private HashSet<K> sortedPiles => _sortedPiles ?? (_sortedPiles = new HashSet<K>());

	public ADeckLayout<K, V> layout => ADeckLayout<K, V>.GetLayout(this);

	public K drawPile => default(K);

	public K handPile => EnumUtil<K>.FromInt(1);

	public K discardPile => EnumUtil<K>.Values.Last();

	public int suppressEvents { get; set; }

	public bool isSuppressingEvents => suppressEvents != 0;

	private List<Id<V>> this[K pile] => piles.GetValueOrDefault(pile) ?? (piles[pile] = new List<Id<V>>());

	public K? this[Id<V> card]
	{
		get
		{
			return pileMap.GetValueOrDefault(card.id) ?? (pileMap[card.id] = null);
		}
		private set
		{
			pileMap[card.id] = value;
		}
	}

	public V this[K pile, int index] => this[pile][index];

	public event OnTransfer onTransfer;

	public event OnReorder onReorder;

	public L Layout<L>() where L : ADeckLayout<K, V>
	{
		return (L)layout;
	}

	public IdDeck()
	{
	}

	public IdDeck(IEnumerable<V> cards)
	{
		Add(cards);
	}

	private V _Draw(K fromPile, K toPile)
	{
		return Transfer(NextInPile(fromPile), toPile);
	}

	private void _OnDeckTransfer(ATarget card, IdDeckBase newDeck)
	{
		if (card is V val && this[val].HasValue)
		{
			Transfer(val, null);
		}
	}

	public void Register()
	{
		IdDeckBase.OnDeckTransfer += _OnDeckTransfer;
	}

	public void Unregister()
	{
		IdDeckBase.OnDeckTransfer -= _OnDeckTransfer;
	}

	public V Add(V card, K? toPile = null)
	{
		return Transfer(card, toPile ?? drawPile);
	}

	public IdDeck<K, V> Add(IEnumerable<V> cards, K? toPile = null)
	{
		foreach (V card in cards)
		{
			Add(card, toPile);
		}
		return this;
	}

	public void Remove(V card)
	{
		Transfer(card, null);
	}

	public IdDeck<K, V> AddSortedPile(K pile)
	{
		sortedPiles.Add(pile);
		return this;
	}

	public IdDeck<K, V> AddSortedPiles<F>(F flags) where F : struct, IConvertible
	{
		foreach (K item in EnumUtil.FlagsConverted<F, K>(flags))
		{
			sortedPiles.Add(item);
		}
		return this;
	}

	public IdDeck<K, V> RemoveSortedPile(K pile)
	{
		sortedPiles.Remove(pile);
		return this;
	}

	public IEnumerable<V> GetCards()
	{
		foreach (List<Id<V>> value in piles.Values)
		{
			foreach (Id<V> item in value)
			{
				yield return item;
			}
		}
	}

	public PoolKeepItemListHandle<V> GetCardsSafe()
	{
		return Pools.UseKeepItemList(GetCards());
	}

	public IEnumerable<V> GetCards(K pile)
	{
		foreach (Id<V> item in this[pile])
		{
			yield return item;
		}
	}

	public PoolKeepItemListHandle<V> GetCardsSafe(K pile)
	{
		PoolKeepItemListHandle<V> poolKeepItemListHandle = Pools.UseKeepItemList<V>();
		foreach (Id<V> item in this[pile])
		{
			poolKeepItemListHandle.Add(item);
		}
		return poolKeepItemListHandle;
	}

	public IEnumerable<V> GetCards<F>(F flags) where F : struct, IConvertible
	{
		foreach (K item in EnumUtil.FlagsConverted<F, K>(flags))
		{
			foreach (Id<V> item2 in this[item])
			{
				yield return item2;
			}
		}
	}

	public PoolKeepItemListHandle<V> GetCardsSafe<F>(F flags) where F : struct, IConvertible
	{
		PoolKeepItemListHandle<V> poolKeepItemListHandle = Pools.UseKeepItemList<V>();
		foreach (K item in EnumUtil.FlagsConverted<F, K>(flags))
		{
			foreach (Id<V> item2 in this[item])
			{
				poolKeepItemListHandle.Add(item2);
			}
		}
		return poolKeepItemListHandle;
	}

	public V Transfer(V card, K? toPile, int? insertIndex = null)
	{
		K? val = this[card];
		if (insertIndex.HasValue && toPile.HasValue)
		{
			insertIndex = Mathf.Clamp(insertIndex.Value, 0, this[toPile.Value].Count);
		}
		if (EnumUtil.Equals(val, toPile))
		{
			if (insertIndex.HasValue && toPile.HasValue)
			{
				int num = IndexOf(card);
				if (num == insertIndex)
				{
					return card;
				}
				this[toPile.Value].RemoveAt(num);
				this[toPile.Value].Insert(insertIndex.Value - (insertIndex > num).ToInt(), card);
				if (IndexOf(card) != num && suppressEvents == 0)
				{
					this.onReorder?.Invoke(card, toPile.Value);
				}
			}
			return card;
		}
		if (val.HasValue)
		{
			this[val.Value].Remove(card);
		}
		else
		{
			IdDeckBase._SignalDeckTransfer(card, this);
		}
		this[card] = toPile;
		if (toPile.HasValue)
		{
			if (insertIndex.HasValue)
			{
				this[toPile.Value].Insert(insertIndex.Value, card);
			}
			else
			{
				this[toPile.Value].AddSorted(card, sortedPiles.Contains(toPile.Value));
			}
		}
		if (suppressEvents == 0)
		{
			this.onTransfer?.Invoke(card, val, toPile);
		}
		return card;
	}

	public void Transfer(IEnumerable<V> cards, K? toPile, int? insertIndex = null)
	{
		foreach (V card in cards)
		{
			Transfer(card, toPile, insertIndex++);
		}
	}

	public void Set(IEnumerable<V> cards, K pile)
	{
		int num = 0;
		foreach (V card in cards)
		{
			Transfer(card, pile, num++);
		}
	}

	public void TransferPile(K fromPile, K toPile)
	{
		foreach (Id<V> item in Pools.UseKeepItemList(this[fromPile]))
		{
			Transfer(item, toPile);
		}
	}

	public void TransferPile(K fromPile, K toPile, bool clearExitTransitions)
	{
		foreach (Id<V> item in Pools.UseKeepItemList(this[fromPile]))
		{
			Transfer(item, toPile);
			if (clearExitTransitions)
			{
				item.value.view?.ClearExitTransitions();
			}
		}
	}

	public PoolKeepItemListHandle<V> TransferPileReturn(K fromPile, K toPile)
	{
		PoolKeepItemListHandle<V> poolKeepItemListHandle = Pools.UseKeepItemList(GetCards(fromPile));
		foreach (V item in poolKeepItemListHandle.value)
		{
			Transfer(item, toPile);
		}
		return poolKeepItemListHandle;
	}

	public void TransferPiles<F>(F flags, K toPile) where F : struct, IConvertible
	{
		foreach (K item in EnumUtil.FlagsConverted<F, K>(flags))
		{
			TransferPile(item, toPile);
		}
	}

	public V Discard(V card, K? toPile = null)
	{
		return Transfer(card, toPile ?? discardPile);
	}

	public int GetIndexOf(ATarget card)
	{
		return this[this[card as V].GetValueOrDefault()].IndexOf(new Id<V>(card.id, card.tableId));
	}

	public V GetValueAtIndex(K pile, int index)
	{
		return this[pile][index];
	}

	public V FirstInPile(K pile)
	{
		Id<V>? id = this[pile].FirstValue();
		if (!id.HasValue)
		{
			return null;
		}
		return id.GetValueOrDefault();
	}

	public V NextInPile(K? pile = null)
	{
		Id<V>? id = this[pile ?? drawPile].LastValue();
		if (!id.HasValue)
		{
			return null;
		}
		return id.GetValueOrDefault();
	}

	public int Count()
	{
		int num = 0;
		foreach (List<Id<V>> value in _piles.Values)
		{
			num += value.Count;
		}
		return num;
	}

	public int Count(K pile)
	{
		return this[pile].Count;
	}

	public int Count(K pile, Func<V, bool> countsTowardCount)
	{
		if (countsTowardCount == null)
		{
			return Count(pile);
		}
		int num = 0;
		foreach (Id<V> item in this[pile])
		{
			if (countsTowardCount(item))
			{
				num++;
			}
		}
		return num;
	}

	public int Count<F>(F flags) where F : struct, IConvertible
	{
		int num = 0;
		foreach (K item in EnumUtil.FlagsConverted<F, K>(flags))
		{
			num += Count(item);
		}
		return num;
	}

	public bool Any()
	{
		foreach (List<Id<V>> value in _piles.Values)
		{
			if (value.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool Any(K pile)
	{
		return Count(pile) > 0;
	}

	public bool Any<F>(F flags) where F : struct, IConvertible
	{
		foreach (K item in EnumUtil.FlagsConverted<F, K>(flags))
		{
			if (Any(item))
			{
				return true;
			}
		}
		return false;
	}

	public int IndexOf(V card)
	{
		return this[this[card].GetValueOrDefault()].IndexOf(card);
	}

	public int? TryGetIndexOf(V card, IEqualityComparer<Id<V>> equalityComparer, K? pileToCheck = null)
	{
		int num = this[pileToCheck ?? this[card].GetValueOrDefault()].IndexOf(card, equalityComparer);
		if (num < 0)
		{
			return null;
		}
		return num;
	}

	public int IndexOfStartFromLast(V card)
	{
		return this[this[card].GetValueOrDefault()].IndexOfStartFromLast(card);
	}

	public bool Contains(V card)
	{
		return IndexOf(card) >= 0;
	}

	public bool HasDrawn()
	{
		foreach (KeyValuePair<K, List<Id<V>>> pile in _piles)
		{
			if (pile.Value.Count > 0 && !EnumUtil.Equals(default(K), pile.Key))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasDrawn<F>(F pilesToIgnore) where F : struct, IConvertible
	{
		foreach (KeyValuePair<K, List<Id<V>>> pile in _piles)
		{
			if (pile.Value.Count > 0 && !EnumUtil.Equals(default(K), pile.Key) && !EnumUtil.HasFlagConvert(pilesToIgnore, pile.Key))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanDraw(K? drawFrom = null, K? shuffleFrom = null)
	{
		if (!Any(drawFrom ?? drawPile))
		{
			return Any(shuffleFrom ?? discardPile);
		}
		return true;
	}

	public GameStepDraw DrawStep(int count = 1, K? drawFrom = null, K? drawTo = null, K? shuffleFrom = null, float wait = 0.1f)
	{
		return new GameStepDraw(this, count, drawFrom.GetValueOrDefault(), drawTo ?? handPile, shuffleFrom ?? discardPile, wait);
	}

	public GameStepDrawToSize DrawToSizeStep(int size = 1, K? drawFrom = null, K? drawTo = null, K? shuffleFrom = null, float wait = 0.1f, Func<V, bool> countsTowardsCount = null)
	{
		return new GameStepDrawToSize(this, size, drawFrom.GetValueOrDefault(), drawTo ?? handPile, shuffleFrom ?? discardPile, wait, countsTowardsCount);
	}

	public GameStepDrawFiltered DrawFilteredStep(Func<V, bool> filter, int count = 1, K? drawFrom = null, K? drawTo = null, bool shuffleInto = false, float wait = 0.1f)
	{
		return new GameStepDrawFiltered(this, count, drawFrom.GetValueOrDefault(), drawTo ?? handPile, wait, filter, shuffleInto);
	}

	public GameStepTransfer DiscardStep(V card)
	{
		return new GameStepTransfer(this, card, discardPile);
	}

	public GameStepInspectPile<F> InspectStep<F>(F flags, IComparer<V> sort = null, IEqualityComparer<CardLayoutElement> grouping = null, MessageData.GameTooltips? message = null, CardHandLayoutSettings layoutSettings = null, IEnumerable<V> additionalCards = null, Func<V, bool> clearExitTransition = null, Action<V> onEnterInspect = null, Action<V> onExitInspect = null, bool dragEntireHand = false) where F : struct, IConvertible
	{
		return new GameStepInspectPile<F>(this, flags, sort, grouping, message, layoutSettings, additionalCards, clearExitTransition, onEnterInspect, onExitInspect, dragEntireHand);
	}

	public GameStepTransferChoice TransferCardChoiceStep(int count = 1, K? discardFrom = null, K? discardTo = null)
	{
		return new GameStepTransferChoice(this, count, discardFrom ?? handPile, discardTo ?? discardPile);
	}

	public GameStepTransfer TransferCardStep(V card, K transferTo)
	{
		return new GameStepTransfer(this, card, transferTo);
	}

	public GameStepShuffle TransferPileStep(K from, K to)
	{
		return new GameStepShuffle(this, from, to, shuffle: false);
	}

	public GameStepShuffle ShuffleStep(K shuffleFrom, K shuffleTo)
	{
		return new GameStepShuffle(this, shuffleFrom, shuffleTo);
	}

	public GameStepAddCardsOverTime<K, V> AddOverTime(IEnumerable<V> cards, K? toPile = null, int cardsPerFrame = 1)
	{
		return new GameStepAddCardsOverTime<K, V>(this, cards, toPile, cardsPerFrame);
	}

	public GameStepAddCardsOverTime<K, V> AddOverTimeParallel(IEnumerable<V> cards, K? toPile = null, int cardsPerFrame = 1)
	{
		return GameState.Instance.stack.ParallelProcess(AddOverTime(cards, toPile, cardsPerFrame)) as GameStepAddCardsOverTime<K, V>;
	}

	[ProtoAfterDeserialization]
	private void _ProtoAfterDeserialization()
	{
		foreach (K item in piles.EnumerateKeysSafe())
		{
			Dictionary<K, List<Id<V>>> dictionary = piles;
			K key = item;
			if (dictionary[key] == null)
			{
				List<Id<V>> list2 = (dictionary[key] = new List<Id<V>>());
			}
		}
	}
}
