using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ADeckLayout<K, V> : ADeckLayoutBase where K : struct, IConvertible where V : ATarget
{
	private static Dictionary<IdDeck<K, V>, ADeckLayout<K, V>> _Map = new Dictionary<IdDeck<K, V>, ADeckLayout<K, V>>();

	private IdDeck<K, V> _deck;

	private Dictionary<ushort, int> _indices;

	private Dictionary<K, ACardLayout> _defaultLayouts;

	private List<K> _faceDownStacks;

	private List<K> _faceUpStacks;

	public IdDeck<K, V> deck
	{
		get
		{
			return _deck ?? (_deck = new IdDeck<K, V>());
		}
		set
		{
			_SetDeck(value);
		}
	}

	private Dictionary<ushort, int> indices => _indices ?? (_indices = new Dictionary<ushort, int>());

	private Dictionary<K, ACardLayout> defaultLayouts => _defaultLayouts ?? (_defaultLayouts = new Dictionary<K, ACardLayout>());

	public List<K> faceDownStacks
	{
		get
		{
			List<K> list = _faceDownStacks;
			if (list == null)
			{
				List<K> obj = new List<K> { default(K) };
				List<K> list2 = obj;
				_faceDownStacks = obj;
				list = list2;
			}
			return list;
		}
		set
		{
			_faceDownStacks = value;
		}
	}

	public List<K> faceUpStacks
	{
		get
		{
			List<K> list = _faceUpStacks;
			if (list == null)
			{
				List<K> obj = new List<K> { EnumUtil<K>.Max };
				List<K> list2 = obj;
				_faceUpStacks = obj;
				list = list2;
			}
			return list;
		}
		set
		{
			_faceUpStacks = value;
		}
	}

	protected CardLayoutElement this[V value]
	{
		get
		{
			ATargetView view = value.view;
			if ((object)view == null || !view)
			{
				return this[deck[value]]?.Add(_CreateView(value).SetData(value, this));
			}
			return view;
		}
	}

	protected abstract ACardLayout this[K? pile] { get; set; }

	public event IdDeck<K, V>.Event onPointerEnter;

	public event IdDeck<K, V>.Event onPointerExit;

	public event IdDeck<K, V>.Event onPointerClick;

	public event IdDeck<K, V>.Event onRest;

	public event IdDeck<K, V>.SmartDrag onSmartDrag;

	public static ADeckLayout<K, V> GetLayout(IdDeck<K, V> deck)
	{
		return _Map.GetValueOrDefault(deck);
	}

	private void _SetDeck(IdDeck<K, V> newDeck)
	{
		if (_deck != null)
		{
			_deck.Unregister();
			_deck.onTransfer -= _OnTransfer;
			_deck.onReorder -= _OnReorder;
			GameState instance = GameState.Instance;
			if (instance != null)
			{
				instance.stack.onEnabledChange -= _OnGameStepEnableChange;
			}
			GameStateView instance2 = GameStateView.Instance;
			if ((object)instance2 != null)
			{
				instance2.onRefreshPointerOverRequest -= _OnRefreshPointerOverRequest;
			}
			_Map.Remove(_deck);
		}
		if ((_deck = newDeck) == null)
		{
			return;
		}
		_deck.Register();
		_deck.onTransfer += _OnTransfer;
		_deck.onReorder += _OnReorder;
		GameState instance3 = GameState.Instance;
		if (instance3 != null)
		{
			instance3.stack.onEnabledChange += _OnGameStepEnableChange;
		}
		GameStateView instance4 = GameStateView.Instance;
		if ((object)instance4 != null)
		{
			instance4.onRefreshPointerOverRequest += _OnRefreshPointerOverRequest;
		}
		foreach (V card in deck.GetCards())
		{
			_ = this[card];
		}
		_Map[_deck] = this;
		_UpdateCardFrontVisibilityStates();
	}

	private void _OnTransfer(V value, K? oldPile, K? newPile)
	{
		indices.Clear();
		CardLayoutElement cardLayoutElement = this[value];
		cardLayoutElement.deck = (newPile.HasValue ? this : null);
		this[newPile]?.Add(cardLayoutElement);
		if ((newPile.HasValue && !faceDownStacks.Contains(newPile.Value)) || value.view.hasTransition)
		{
			value.view.frontIsVisible = true;
		}
		if (oldPile.HasValue && (faceDownStacks.Contains(oldPile.Value) || faceUpStacks.Contains(oldPile.Value)))
		{
			V val = _deck.NextInPile(oldPile);
			if (val != null)
			{
				val.view.frontIsVisible = true;
			}
		}
	}

	private void _OnReorder(V value, K pile)
	{
		indices.Clear();
		this[pile].Add(this[value], addEvenIfInLayoutAlready: true);
	}

	private void _OnPointerEvent(ATarget card, IdDeck<K, V>.Event pointerEvent)
	{
		if (card is V val)
		{
			pointerEvent?.Invoke(deck[val].GetValueOrDefault(), val);
		}
	}

	private void _OnRefreshPointerOverRequest()
	{
		if (this.onPointerEnter == null && !ADeckLayoutBase.IsListeningToPointerEnter)
		{
			return;
		}
		foreach (ACardLayout layout in GetLayouts())
		{
			layout.RefreshPointerOver();
		}
	}

	private void _OnRefreshPointerExitRequest()
	{
		if (this.onPointerExit == null && !ADeckLayoutBase.IsListeningToPointerExit)
		{
			return;
		}
		foreach (ACardLayout layout in GetLayouts())
		{
			layout.RefreshPointerExit();
		}
	}

	private void _OnGameStepEnableChange(GameStep step, bool isEnabled)
	{
		if (!step.isParallel)
		{
			if (isEnabled)
			{
				_OnRefreshPointerOverRequest();
			}
			else
			{
				_OnRefreshPointerExitRequest();
			}
		}
	}

	private void _UpdateCardFrontVisibilityStates()
	{
		K[] values = EnumUtil<K>.Values;
		foreach (K val in values)
		{
			int num = deck.Count(val);
			if (num == 0)
			{
				continue;
			}
			List<K> list = faceDownStacks;
			int num2;
			if (list == null || !list.Contains(val))
			{
				List<K> list2 = faceUpStacks;
				if (list2 == null || !list2.Contains(val))
				{
					num2 = 0;
					goto IL_0058;
				}
			}
			num2 = num - 1;
			goto IL_0058;
			IL_0058:
			int num3 = num2;
			int num4 = 0;
			foreach (V card in deck.GetCards(val))
			{
				card.view.frontIsVisible = num4++ >= num3;
			}
		}
	}

	private void OnApplicationQuit()
	{
		OnDestroy();
	}

	private void OnDestroy()
	{
		_SetDeck(null);
	}

	protected abstract CardLayoutElement _CreateView(V value);

	public override int GetIndexOf(ATarget card)
	{
		if (!indices.ContainsKey(card.id))
		{
			return indices[card.id] = deck.GetIndexOf(card);
		}
		return indices[card.id];
	}

	public override int GetCountInPile(ATarget card)
	{
		return deck.Count(deck[card as V].GetValueOrDefault());
	}

	public override void SignalPointerEnter(ATarget card)
	{
		_SignalPointerEnter(CastTo<int>.From(deck[card as V].GetValueOrDefault()), card);
		_OnPointerEvent(card, this.onPointerEnter);
	}

	public override void SignalPointerExit(ATarget card)
	{
		_SignalPointerExit(CastTo<int>.From(deck[card as V].GetValueOrDefault()), card);
		_OnPointerEvent(card, this.onPointerExit);
	}

	public override void SignalPointerClick(ATarget card, PointerEventData eventData = null)
	{
		if (!(eventData?.ClickHeldTime() > InputManager.I.ClickThreshold))
		{
			_SignalPointerClick(CastTo<int>.From(deck[card as V].GetValueOrDefault()), card);
			_OnPointerEvent(card, this.onPointerClick);
		}
	}

	public override void SignalAtRest(ATarget card)
	{
		if (!(card is V val))
		{
			return;
		}
		K valueOrDefault = deck[val].GetValueOrDefault();
		this.onRest?.Invoke(valueOrDefault, val);
		if (faceDownStacks.Contains(valueOrDefault))
		{
			if (val.id == 0)
			{
				return;
			}
			int num = _deck.IndexOfStartFromLast(val);
			if (num >= 1 && card.view.layout == GetLayout(valueOrDefault))
			{
				ATargetView view = _deck.GetValueAtIndex(valueOrDefault, num - 1).view;
				if ((object)view != null && view.layout == GetLayout(valueOrDefault))
				{
					view.frontIsVisible = false;
				}
				if (num < _deck.Count(valueOrDefault) - 1)
				{
					val.view.frontIsVisible = false;
				}
			}
		}
		else
		{
			if (!faceUpStacks.Contains(valueOrDefault) || val.id == 0)
			{
				return;
			}
			int? num2 = _deck?.IndexOfStartFromLast(val);
			if (!num2.HasValue)
			{
				return;
			}
			int valueOrDefault2 = num2.GetValueOrDefault();
			if (valueOrDefault2 < 2 || !(card.view?.layout == GetLayout(valueOrDefault)))
			{
				return;
			}
			ATargetView view2 = _deck.GetValueAtIndex(valueOrDefault, valueOrDefault2 - 2).view;
			if ((object)view2 != null && view2.layout == GetLayout(valueOrDefault))
			{
				view2.frontIsVisible = false;
			}
			if (valueOrDefault2 < _deck.Count(valueOrDefault) - 2)
			{
				ATargetView view3 = _deck.GetValueAtIndex(valueOrDefault, valueOrDefault2 + 2).view;
				if ((object)view3 != null && view3.atRestInLayout)
				{
					val.view.frontIsVisible = false;
				}
			}
		}
	}

	public override void SignalSmartDrag(ACardLayout smartDragTarget, ATarget card)
	{
		if (card is V card2)
		{
			this.onSmartDrag?.Invoke(smartDragTarget, card2);
		}
	}

	public override PoolKeepItemListHandle<ACardLayout> GetLayouts()
	{
		PoolKeepItemListHandle<ACardLayout> poolKeepItemListHandle = Pools.UseKeepItemList<ACardLayout>();
		K[] values = EnumUtil<K>.Values;
		foreach (K value in values)
		{
			if ((bool)this[value])
			{
				poolKeepItemListHandle.Add(this[value]);
			}
		}
		return poolKeepItemListHandle;
	}

	public override void DestroyCard(ATarget card)
	{
		if (card is V val)
		{
			_deck.Remove(val);
			UnityEngine.Object.DestroyImmediate(val.view.gameObject);
			val.ToId<ATarget>().ReleaseId();
		}
	}

	public override void RepoolCard(ATarget card)
	{
		if (card is V val)
		{
			_deck?.Remove(val);
			val.view.gameObject.SetActive(value: false);
			val.ToId<ATarget>().ReleaseId();
		}
	}

	public override IEnumerable<ATarget> GetNextInPiles()
	{
		K[] values = EnumUtil<K>.Values;
		foreach (K value in values)
		{
			V val = _deck.NextInPile(value);
			if (val != null)
			{
				yield return val;
			}
		}
	}

	public V TransferWithSpecialTransitions(V card, K? toPile, int? insertIndex = null, bool useSpecialEnterTransition = true, bool useSpecialExitTransition = true)
	{
		SpecialTransitionTargets specialTransitionTargets = null;
		SpecialTransitionTargets specialTransitionTargets2 = null;
		if (useSpecialExitTransition)
		{
			ACardLayout aCardLayout = card.view?.layout;
			if ((object)aCardLayout != null && (bool)(specialTransitionTargets = aCardLayout.GetSpecialTransitionTargets(SpecialTransitionTargets.TargetType.Exit)))
			{
				specialTransitionTargets.Set();
			}
		}
		if (useSpecialEnterTransition && toPile.HasValue && (bool)(specialTransitionTargets2 = GetLayout(toPile.Value).GetSpecialTransitionTargets(SpecialTransitionTargets.TargetType.Enter)))
		{
			specialTransitionTargets2.Set();
		}
		V result = _deck.Transfer(card, toPile, insertIndex);
		if (specialTransitionTargets != null)
		{
			specialTransitionTargets.Restore();
		}
		if (specialTransitionTargets2 != null)
		{
			specialTransitionTargets2.Restore();
		}
		return result;
	}

	public ACardLayout GetLayout(K pile)
	{
		return this[pile];
	}

	public void SetLayout(K pile, ACardLayout layout, bool useSpecialEnterTransitionsForCardsAtRest = false)
	{
		if (layout == null || this[pile] == layout)
		{
			return;
		}
		defaultLayouts[pile] = defaultLayouts.GetValueOrDefault(pile) ?? this[pile];
		using PoolKeepItemHashSetHandle<CardLayoutElement> poolKeepItemHashSetHandle = (useSpecialEnterTransitionsForCardsAtRest ? Pools.UseKeepItemHashSet(from c in this[pile].GetCards().AsEnumerable()
			where c.atRestInLayout
			select c) : null);
		foreach (CardLayoutElement card2 in this[pile].GetCards())
		{
			if (card2.card is V card && _deck.Contains(card))
			{
				SpecialTransitionTargets specialTransitionTargets = null;
				if (poolKeepItemHashSetHandle != null && (poolKeepItemHashSetHandle.Contains(card2) || poolKeepItemHashSetHandle.Count == 0) && (bool)(specialTransitionTargets = layout.GetSpecialTransitionTargets(SpecialTransitionTargets.TargetType.Enter)))
				{
					specialTransitionTargets.Set();
				}
				layout.Add(card2);
				if (specialTransitionTargets != null)
				{
					specialTransitionTargets.Restore();
				}
			}
		}
		this[pile] = layout;
	}

	public void SetLayoutWithoutTransferringCards(K pile, ACardLayout layout)
	{
		defaultLayouts[pile] = defaultLayouts.GetValueOrDefault(pile) ?? this[pile];
		this[pile] = layout;
	}

	public void RestoreLayoutToDefault(K pile, bool clearExitTransitions = false, bool clearEnterTransitions = false)
	{
		ACardLayout valueOrDefault = defaultLayouts.GetValueOrDefault(pile);
		if ((object)valueOrDefault == null || valueOrDefault == GetLayout(pile))
		{
			return;
		}
		SetLayout(pile, valueOrDefault);
		if (!clearExitTransitions && !clearEnterTransitions)
		{
			return;
		}
		foreach (CardLayoutElement card in GetLayout(pile).GetCards())
		{
			if (clearExitTransitions && clearEnterTransitions)
			{
				card.ClearTransitions();
			}
			else if (clearExitTransitions)
			{
				card.ClearExitTransitions();
			}
			else
			{
				card.ClearEnterTransitions();
			}
		}
	}

	public IEnumerator RestoreLayoutToDefaultAnimated(K pile, bool clearExitTransitions = false, bool clearEnterTransitions = false)
	{
		ACardLayout defaultLayout = defaultLayouts.GetValueOrDefault(pile);
		if ((object)defaultLayout == null || defaultLayout == GetLayout(pile))
		{
			yield break;
		}
		foreach (CardLayoutElement card in GetLayout(pile).GetCards())
		{
			defaultLayout.Add(card);
			if (clearExitTransitions && clearEnterTransitions)
			{
				card.ClearTransitions();
			}
			else if (clearExitTransitions)
			{
				card.ClearExitTransitions();
			}
			else
			{
				card.ClearEnterTransitions();
			}
			yield return null;
		}
		SetLayout(pile, defaultLayout);
	}

	public void RestoreLayoutToDefaultWithoutTransferringCards(K pile)
	{
		this[pile] = defaultLayouts.GetValueOrDefault(pile) ?? this[pile];
	}

	public ADeckLayout<K, V> AddSmartDragTarget(K from, K to, bool bidirectional = true)
	{
		ACardLayout layout = GetLayout(from);
		ACardLayout layout2 = GetLayout(to);
		layout.smartDragTargets.Add(layout2);
		if (bidirectional)
		{
			layout2.smartDragTargets.Add(layout);
		}
		return this;
	}

	public void ClearSmartDragTargets()
	{
		foreach (ACardLayout layout in GetLayouts())
		{
			layout.ClearSmartTargets();
		}
	}

	public IEnumerable<V> GetCardsInLayout(K pile)
	{
		foreach (CardLayoutElement card in this[pile].GetCards())
		{
			if (card.card is V val)
			{
				yield return val;
			}
		}
	}
}
