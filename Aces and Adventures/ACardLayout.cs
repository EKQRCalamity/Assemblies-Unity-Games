using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ACardLayout : MonoBehaviour
{
	[Serializable]
	public class Target
	{
		public Transform transform;

		public CardLayoutSpringSettings springSettings;

		public CardLayoutSpringThresholds thresholds;
	}

	public struct DragThresholdData
	{
		public ACardLayout cardLayout;

		public float dragThresholdMinX;

		public float dragThresholdMinY;

		public float dragThresholdMaxX;

		public float dragThresholdMaxY;

		public bool useDragTargetForDragThresholdOrigin;

		public DragThresholdData(float dragThresholdMinX, float dragThresholdMinY, float dragThresholdMaxX, float dragThresholdMaxY, bool useDragTargetForDragThresholdOrigin)
		{
			this.dragThresholdMinX = dragThresholdMinX;
			this.dragThresholdMinY = dragThresholdMinY;
			this.dragThresholdMaxX = dragThresholdMaxX;
			this.dragThresholdMaxY = dragThresholdMaxY;
			this.useDragTargetForDragThresholdOrigin = useDragTargetForDragThresholdOrigin;
			cardLayout = null;
		}

		public DragThresholdData(ACardLayout layout)
			: this(layout.dragThresholdMinX, layout.dragThresholdMinY, layout.dragThresholdMaxX, layout.dragThresholdMaxY, layout.useDragTargetForDragThresholdOrigin)
		{
			cardLayout = layout;
		}

		public void SetThresholds(ACardLayout layout)
		{
			layout.dragThresholdMinX = dragThresholdMinX;
			layout.dragThresholdMinY = dragThresholdMinY;
			layout.dragThresholdMaxX = dragThresholdMaxX;
			layout.dragThresholdMaxY = dragThresholdMaxY;
			layout.useDragTargetForDragThresholdOrigin = useDragTargetForDragThresholdOrigin;
		}

		public void Restore()
		{
			if ((bool)cardLayout)
			{
				SetThresholds(cardLayout);
			}
		}
	}

	private static System.Random _Random;

	public CardDimensionSettings size;

	[Header("Layout")]
	public Transform cardContainer;

	public Transform smartDragTarget;

	public Target layoutTarget;

	public bool reverseDeckSortOrder;

	[Header("Transitions")]
	public List<Target> exitTargets;

	public List<Target> enterTargets;

	public ACardLayout[] skipExitTransitionWhenTransferredTo;

	[Range(-1f, 0f)]
	public float dragThresholdMinX;

	[Range(-1f, 0f)]
	public float dragThresholdMinY;

	[Range(0f, 1f)]
	public float dragThresholdMaxX;

	[Range(0f, 1f)]
	public float dragThresholdMaxY;

	public bool useDragTargetForDragThresholdOrigin;

	[Header("Sound")]
	public CardLayoutSoundPack soundPack;

	private CardLayoutElement _pointerDown;

	private CardLayoutElement _pointerOver;

	protected CardLayoutElement _pointerDrag;

	private int? _inputIndex;

	protected Vector3 _dragTargetPosition;

	private Vector3 _beginDragPosition;

	private Vector3 _dragOffset;

	private HashSet<ACardLayout> _smartDragTargets;

	private PointerEventData.InputButton? _inputButton;

	private PointerEventData.InputButton _lastInputButton;

	private PointerEventData _pointerDownEvent;

	private float _atRestTime;

	private readonly List<CardLayoutElement> _cards = new List<CardLayoutElement>();

	private IEqualityComparer<CardLayoutElement> _groupingEqualityComparer;

	private Dictionary<CardLayoutElement, PoolKeepItemListHandle<CardLayoutElement>> _cardGroups;

	private List<int> _groupingCountByIndex;

	public static System.Random Random => _Random ?? (_Random = new System.Random());

	protected CardLayoutElement _effectivePointerOver
	{
		get
		{
			if (!_pointerDown)
			{
				return _pointerOver;
			}
			return _pointerDown;
		}
	}

	protected int inputIndex
	{
		get
		{
			if (_inputIndex.HasValue)
			{
				return _inputIndex.Value;
			}
			CardLayoutElement cardLayoutElement = (_pointerDrag ? _pointerDrag : (_effectivePointerOver ? _effectivePointerOver : null));
			if (!cardLayoutElement)
			{
				_inputIndex = -1;
			}
			using PoolKeepItemListHandle<CardLayoutElement> poolKeepItemListHandle = GetCards();
			int? num = (_inputIndex = poolKeepItemListHandle.value.IndexOf(cardLayoutElement));
			return num.Value;
		}
	}

	public float cardWidth => size.scaledWidth;

	public float cardHeight => size.scaledHeight;

	public float cardThickness => size.scaledThickness;

	protected bool _draggedBeyondThreshold
	{
		get
		{
			Vector3 lhs = _dragTargetPosition - _beginDragPosition;
			float num = Vector3.Dot(lhs, layoutTarget.transform.right) / cardWidth;
			float num2 = Vector3.Dot(lhs, layoutTarget.transform.forward) / cardHeight;
			if ((dragThresholdMinX == 0f || !(num < dragThresholdMinX)) && (dragThresholdMinY == 0f || !(num2 < dragThresholdMinY)) && (dragThresholdMaxX == 0f || !(num > dragThresholdMaxX)))
			{
				if (dragThresholdMaxY != 0f)
				{
					return num2 > dragThresholdMaxY;
				}
				return false;
			}
			return true;
		}
	}

	protected Transform _smartDragTarget
	{
		get
		{
			if (!smartDragTarget)
			{
				return layoutTarget.transform;
			}
			return smartDragTarget;
		}
	}

	public HashSet<ACardLayout> smartDragTargets => _smartDragTargets ?? (_smartDragTargets = new HashSet<ACardLayout>());

	public CardLayoutElement pointerDrag => _pointerDrag;

	private CardLayoutElement pointerDown
	{
		get
		{
			return _pointerDown;
		}
		set
		{
			_OnEffectivePointerOverChange(ref _pointerDown, value);
		}
	}

	public CardLayoutElement pointerOver
	{
		get
		{
			return _pointerOver;
		}
		private set
		{
			_OnEffectivePointerOverChange(ref _pointerOver, value);
		}
	}

	protected PointerEventData.InputButton? inputButton => _inputButton;

	[HideInInspector]
	public bool disableInputColliderManagement { get; set; }

	public int Count => _cards.Count;

	protected bool _isAtRest => _atRestTime >= _restTime;

	public bool dragEndWasForced { get; private set; }

	public IEqualityComparer<CardLayoutElement> groupingEqualityComparer
	{
		get
		{
			return _groupingEqualityComparer;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _groupingEqualityComparer, value))
			{
				Pools.RepoolDictionaryValues(_cardGroups);
				_cardGroups = null;
				_UpdateCardGrouping();
			}
		}
	}

	public bool usesCardGrouping => groupingEqualityComparer != null;

	private List<int> groupingCountByIndex => _groupingCountByIndex ?? (_groupingCountByIndex = new List<int>());

	protected virtual bool _addSorted => true;

	protected virtual Transform _dragTarget => layoutTarget.transform;

	protected virtual float _dragLerp => 1f;

	public virtual bool draggingEntireHand => false;

	protected virtual bool _useDragOffset => false;

	public virtual CardLayoutElement.PointerOverPadding? pointerOverPaddingOverride
	{
		get
		{
			return null;
		}
		protected set
		{
		}
	}

	protected virtual bool _processExitTarget => true;

	protected virtual bool _checkForNestedLayouts => false;

	protected virtual bool _useDynamicTransitionTargets => false;

	public virtual CardLayoutElement.PointerOverPadding defaultPointerOverPadding => CardLayoutElement.PointerOverPadding.None;

	protected virtual CardLayoutElement.PointerOverPadding _pointerOverPadding => CardLayoutElement.PointerOverPadding.None;

	protected virtual float? _restTime => null;

	protected virtual int _startUpdateIndex => 0;

	public virtual int maxCount => 0;

	public static event Action<ACardLayout, CardLayoutElement> OnDragBegan;

	public static event Action<ACardLayout, CardLayoutElement> OnDragEnded;

	public static event Action<ACardLayout, CardLayoutElement, PointerEventData> OnPointerClick;

	public static event Action<ACardLayout, CardLayoutElement> OnPointerHeld;

	public event Action<ACardLayout, CardLayoutElement> onCardAdded;

	public event Action<ACardLayout, CardLayoutElement> onCardRemoved;

	public event Action<PointerEventData, CardLayoutElement> onDragBegin;

	public event Action<PointerEventData, CardLayoutElement> onDragEnd;

	private IEnumerable<CardLayoutElement.Target> _GetExitTargets(CardLayoutElement card, bool wasPointerOver, List<CardLayoutElement> cards)
	{
		if (exitTargets.IsNullOrEmpty())
		{
			yield break;
		}
		TransformData cardTransformData = new TransformData(card.transform);
		if (card.offset.HasValue && !card.ignoreOffsetInExitTarget)
		{
			cardTransformData = cardTransformData.TransformTRS(card.offset.Value.inverse);
		}
		foreach (Target exitTarget in exitTargets)
		{
			yield return new CardLayoutElement.Target(exitTarget, _processExitTarget ? _Transform(_ProcessExitTarget(card, ref cardTransformData, wasPointerOver, cards), exitTarget) : new TransformData(exitTarget.transform), CardLayoutElement.Target.TransitionType.Exit);
		}
	}

	protected IEnumerable<CardLayoutElement.Target> _GetEnterTargets(CardLayoutElement card)
	{
		if (enterTargets == null)
		{
			yield break;
		}
		using PoolKeepItemListHandle<CardLayoutElement> cards = GetCards();
		int cardIndex = cards.value.IndexOf(card);
		TransformData target = _TransformEnterLayoutTarget(card, _GetLayoutTarget(card, cardIndex, cards.Count).target, cardIndex);
		int enterTargetIndex = 0;
		foreach (Target enterTarget in enterTargets)
		{
			if (_ProcessEnterTarget(enterTarget, cardIndex, cards.Count, enterTargetIndex++))
			{
				yield return new CardLayoutElement.Target(enterTarget, _Transform(target, enterTarget), CardLayoutElement.Target.TransitionType.Enter);
			}
		}
	}

	private TransformData _DragTransform(TransformData layoutData, Target transitionData)
	{
		Vector3 vector = _dragTargetPosition - layoutTarget.transform.position;
		layoutData = layoutData.Translate(-vector);
		layoutData = _Transform(layoutData, transitionData);
		layoutData = layoutData.Translate(vector);
		return layoutData;
	}

	private TransformData _Transform(TransformData layoutData, Target transitionData)
	{
		return layoutData.Transform(transitionData.transform.localToWorldMatrix * layoutTarget.transform.worldToLocalMatrix);
	}

	public void _SetInputEnabled(CardLayoutElement card, bool inputEnabled)
	{
		if (disableInputColliderManagement)
		{
			return;
		}
		if (!_checkForNestedLayouts)
		{
			card.inputEnabled = inputEnabled;
			return;
		}
		foreach (CardLayoutElement item in card.gameObject.GetComponentsInChildrenPooled<CardLayoutElement>())
		{
			item.inputEnabled = inputEnabled;
		}
	}

	protected void _SetInputEnabledAt(int cardIndex, bool inputEnabled)
	{
		_SetInputEnabled(_cards[cardIndex], inputEnabled);
	}

	private void _UpdateDragInputCollider()
	{
		if ((bool)_pointerDrag)
		{
			_pointerDrag.inputCollider.transform.CopyFrom(_dragTarget, copyPosition: true, copyRotation: true, copyScale: false);
			_pointerDrag.pointerOverPadding = new CardLayoutElement.PointerOverPadding(new Vector2(100f, 100f), Vector2.one, new Vector2(100f, 100f));
			if (_checkForNestedLayouts)
			{
				_pointerDrag.inputCollider.transform.position += _dragTarget.up * size.max * 0.5f;
			}
		}
	}

	private void _ClearPointerDown()
	{
		pointerDown = null;
		_inputButton = null;
		_pointerDownEvent = null;
	}

	private void _OnEffectivePointerOverChange(ref CardLayoutElement card, CardLayoutElement newCard)
	{
		CardLayoutElement effectivePointerOver = _effectivePointerOver;
		card = newCard;
		CardLayoutElement effectivePointerOver2 = _effectivePointerOver;
		if (!(effectivePointerOver == effectivePointerOver2))
		{
			if ((bool)effectivePointerOver)
			{
				effectivePointerOver.deck?.SignalPointerExit(effectivePointerOver.card);
			}
			if ((bool)effectivePointerOver2)
			{
				effectivePointerOver2.deck?.SignalPointerEnter(effectivePointerOver2.card);
			}
			_SetAtRestDirty(effectivePointerOver2 ?? effectivePointerOver, _inputIndex);
		}
	}

	private Matrix4x4? _UpdateOffsetMatrix(CardLayoutElement card)
	{
		if (!card.hasOffset || card == _pointerDrag)
		{
			return null;
		}
		Matrix4x4 identity = Matrix4x4.identity;
		foreach (Matrix4x4 offset in card.offsets)
		{
			identity *= offset;
		}
		return card.offset = identity;
	}

	protected int _GetGroupingCountUpToIndex(int cardIndex)
	{
		if (cardIndex >= groupingCountByIndex.Count)
		{
			return 0;
		}
		return groupingCountByIndex[cardIndex];
	}

	protected bool _BelongsToGroupButNotLastInGroup(int? cardIndex)
	{
		if (cardIndex >= 0 && cardIndex < Count - 1)
		{
			return groupingCountByIndex[cardIndex.Value + 1] > groupingCountByIndex[cardIndex.Value];
		}
		return false;
	}

	protected bool _BelongsToGroupButNotFirstInGroup(int? cardIndex)
	{
		if (cardIndex > 0 && cardIndex < Count)
		{
			return groupingCountByIndex[cardIndex.Value] > groupingCountByIndex[cardIndex.Value - 1];
		}
		return false;
	}

	protected PoolListHandle<PoolKeepItemListHandle<CardLayoutElement>> _GetCardGroups()
	{
		PoolListHandle<PoolKeepItemListHandle<CardLayoutElement>> poolListHandle = Pools.UseList<PoolKeepItemListHandle<CardLayoutElement>>();
		if (usesCardGrouping)
		{
			foreach (PoolKeepItemListHandle<CardLayoutElement> value in _cardGroups.Values)
			{
				poolListHandle.Add(value);
			}
			return poolListHandle;
		}
		poolListHandle.Add(GetCards());
		return poolListHandle;
	}

	protected abstract CardLayoutElement.Target _GetLayoutTarget(CardLayoutElement card, int cardIndex, int cardCount);

	protected virtual void _UpdateCardGrouping()
	{
		if (usesCardGrouping)
		{
			_cardGroups = _cardGroups ?? (_cardGroups = new Dictionary<CardLayoutElement, PoolKeepItemListHandle<CardLayoutElement>>(groupingEqualityComparer));
			Pools.RepoolDictionaryValues(_cardGroups);
			groupingCountByIndex.Clear();
			for (int i = 0; i < _cards.Count; i++)
			{
				CardLayoutElement cardLayoutElement = _cards[i];
				(_cardGroups.GetValueOrDefault(cardLayoutElement) ?? (_cardGroups[cardLayoutElement] = Pools.UseKeepItemList<CardLayoutElement>())).Add(cardLayoutElement);
				groupingCountByIndex.Add(i - (_cardGroups.Count - 1));
			}
		}
	}

	protected virtual void _OnRemovedFromLayout(CardLayoutElement card, int cardIndex, int cardCount)
	{
		PointerEventData pointerDownEvent = _pointerDownEvent;
		if (card == pointerOver)
		{
			pointerOver = null;
		}
		if (card == pointerDown)
		{
			_ClearPointerDown();
		}
		if (card == _pointerDrag)
		{
			_pointerDrag = null;
			pointerDownEvent.CancelDrag();
			if (card.layout != this)
			{
				return;
			}
		}
		_SetAtRestDirty(card, cardIndex);
		_cards.RemoveAt(cardIndex);
		_UpdateCardGrouping();
		this.onCardRemoved?.Invoke(this, card);
	}

	protected virtual void _OnAddedToLayout(CardLayoutElement card, int cardIndex, int cardCount)
	{
		card.pointerOverPadding = defaultPointerOverPadding;
		_SetAtRestDirty(card, cardIndex);
		_cards.Insert(cardIndex, card);
		_UpdateCardGrouping();
		this.onCardAdded?.Invoke(this, card);
	}

	protected virtual void _OnEnteredLayout(CardLayoutElement card, int cardIndex, int cardCount)
	{
	}

	public virtual void OnPointerEnter(PointerEventData eventData, CardLayoutElement card)
	{
		if (_ShouldPlayPointerOverAudio(card))
		{
			soundPack?.onPointerEnter?.Play(card.transform.position, Random, soundPack.mixerGroup);
		}
		pointerOver = card;
	}

	public virtual void OnPointerExit(PointerEventData eventData, CardLayoutElement card)
	{
		if (_ShouldPlayPointerOverAudio(card))
		{
			soundPack?.onPointerExit?.Play(card.transform.position, Random, soundPack.mixerGroup);
		}
		pointerOver = null;
	}

	public virtual void OnPointerDown(PointerEventData eventData, CardLayoutElement card)
	{
		if (!_inputButton.HasValue)
		{
			soundPack?.onPointerDown?.Play(eventData.pointerPressRaycast.worldPosition, Random, soundPack.mixerGroup);
			pointerDown = card;
			_inputButton = (_lastInputButton = eventData.button);
			_pointerDownEvent = eventData;
		}
	}

	public virtual void OnPointerUp(PointerEventData eventData, CardLayoutElement card)
	{
		if (eventData.button == _inputButton)
		{
			soundPack?.onPointerUp?.Play(eventData.pointerPressRaycast.worldPosition, Random, soundPack.mixerGroup);
			_ClearPointerDown();
			ACardLayout.OnPointerClick?.Invoke(this, card, eventData);
		}
	}

	public bool ShouldAllowClick(PointerEventData eventData)
	{
		if (!pointerDrag)
		{
			return eventData.button == _lastInputButton;
		}
		return false;
	}

	public virtual void OnDragInitialize(PointerEventData eventData, CardLayoutElement card)
	{
		if (!_CanDrag(card) || eventData.button != _inputButton)
		{
			eventData.pointerDrag = null;
		}
	}

	public virtual void OnDragBegin(PointerEventData eventData, CardLayoutElement card)
	{
		_beginDragPosition = (useDragTargetForDragThresholdOrigin ? _GetDragTargetPosition(eventData) : card.transform.position);
		if (_useDragOffset)
		{
			_dragOffset = card.transform.InverseTransformDirection(card.transform.position - _GetDragTargetPosition(eventData));
		}
		_pointerDrag = card;
		soundPack?.onDragBegin?.Play(eventData.pointerPressRaycast.worldPosition, Random, soundPack.mixerGroup);
		_SetAtRestDirty(card, _inputIndex);
		this.onDragBegin?.Invoke(eventData, card);
		ACardLayout.OnDragBegan?.Invoke(this, card);
	}

	private Vector3 _GetDragTargetPosition(PointerEventData eventData)
	{
		return layoutTarget.transform.GetPlane(PlaneAxes.XZ).Lerp(_dragTarget.GetPlane(PlaneAxes.XZ), _dragLerp).ClosestPointOnPlane(eventData.pressEventCamera.ScreenPointToRay(Input.mousePosition)) + ((_useDragOffset && (bool)_pointerDrag) ? _pointerDrag.transform.TransformDirection(_dragOffset) : Vector3.zero);
	}

	public virtual void OnDrag(PointerEventData eventData, CardLayoutElement card)
	{
		_dragTargetPosition = _GetDragTargetPosition(eventData);
	}

	public virtual void OnDragEnd(PointerEventData eventData, CardLayoutElement card)
	{
		dragEndWasForced = !_pointerDrag;
		if (_draggedBeyondThreshold && eventData.button == PointerEventData.InputButton.Left && (bool)_pointerDrag)
		{
			if (!_smartDragTargets.IsNullOrEmpty())
			{
				Vector3 targetCorrection = (Vector3.ProjectOnPlane(card.velocities.position, _dragTarget.up) + InputManager.I.MouseVelocity(_dragTarget.position)) * -0.0666f;
				targetCorrection += (_beginDragPosition - _smartDragTarget.position) * 0.5f;
				Vector2 mousePosition = (Input.mousePosition.Project(AxisType.Z) + Camera.main.WorldToScreenPoint(card.transform.position).Project(AxisType.Z)) * 0.5f;
				ACardLayout aCardLayout = _smartDragTargets.MinBy((ACardLayout layout) => (Camera.main.WorldToScreenPoint(layout._smartDragTarget.position + targetCorrection).Project(AxisType.Z) - mousePosition).sqrMagnitude);
				card.deck.SignalSmartDrag(aCardLayout, card.card);
			}
			else
			{
				card.deck.SignalPointerClick(card.card);
			}
		}
		_pointerDrag = null;
		soundPack?.onDragEnd?.Play(eventData.pointerPressRaycast.worldPosition, Random, soundPack.mixerGroup);
		card.inputCollider.transform.SetLocalToIdentity();
		card.pointerOverPadding = defaultPointerOverPadding;
		this.onDragEnd?.Invoke(eventData, card);
		ACardLayout.OnDragEnded?.Invoke(this, card);
	}

	protected virtual TransformSpringSettings _GetSpringSettings(CardLayoutElement.Target target)
	{
		return target;
	}

	protected virtual TransformData _ProcessExitTarget(CardLayoutElement card, ref TransformData cardTransformData, bool wasPointerOver, List<CardLayoutElement> cards)
	{
		return cardTransformData;
	}

	protected virtual TransformData _TransformEnterLayoutTarget(CardLayoutElement card, TransformData enterLayoutTarget, int cardIndex)
	{
		return enterLayoutTarget;
	}

	protected virtual bool _ProcessEnterTarget(Target target, int cardIndex, int cardCount, int enterTargetIndex)
	{
		return true;
	}

	protected virtual bool _CanDrag(CardLayoutElement card)
	{
		return false;
	}

	protected virtual void _SetAtRestDirty(CardLayoutElement card, int? cardIndex)
	{
		_atRestTime = 0f;
	}

	protected virtual void _OnAtRest()
	{
	}

	protected virtual bool _ShouldPlayPointerOverAudio(CardLayoutElement card)
	{
		return true;
	}

	private void _UpdateCard(CardLayoutElement card, int x, PoolKeepItemListHandle<CardLayoutElement> cards, float deltaTime, ref bool atRest)
	{
		CardLayoutElement.Target target = card.targets.FirstValue() ?? _GetLayoutTarget(card, x, cards.Count);
		if (_useDynamicTransitionTargets && target.isEnterTransition)
		{
			TransformData layoutData = _TransformEnterLayoutTarget(card, _GetLayoutTarget(card, x, cards.Count).target, x);
			target = new CardLayoutElement.Target(target.data, _pointerDrag ? _DragTransform(layoutData, target.data) : _Transform(layoutData, target.data));
		}
		TransformData target2 = target.target;
		if (card.hasOffset)
		{
			Matrix4x4? matrix4x = _UpdateOffsetMatrix(card);
			if (matrix4x.HasValue)
			{
				Matrix4x4 valueOrDefault = matrix4x.GetValueOrDefault();
				target2 = target2.TransformTRS(valueOrDefault);
			}
		}
		else if (card.offset.HasValue)
		{
			card.offset = card.offset.Value.AffineLerp(Matrix4x4.identity, MathUtil.CalculateEaseStiffnessSubjectToTime(15f, deltaTime));
			target2 = target2.TransformTRS(card.offset.Value);
			if (card.offset.Value == Matrix4x4.identity)
			{
				card.offset = null;
			}
		}
		new TransformData(card.transform).Spring(target2, ref card.velocities, _GetSpringSettings(target), deltaTime).SetValues(card.transform);
		if (card.targets.Count > 0 && target2.ApproximatelyEqual(new TransformData(card.transform), target.data.thresholds.distance, target.data.thresholds.degrees, target.data.thresholds.scale, target.data.thresholds.perAxisDistance, target.data.thresholds.useScaleThreshold))
		{
			card.targets.RemoveAt(0);
			if (card.targets.Count == 0)
			{
				_SetInputEnabled(card, inputEnabled: true);
				soundPack?.GetEnterLayout(cards.Count == 1)?.Play(card.transform.position, Random, soundPack.mixerGroup);
				_OnEnteredLayout(card, x, cards.Count);
				card.deck?.SignalAtRest(card.card);
			}
		}
		atRest = atRest && card.targets.Count == 0 && card.velocities.position.sqrMagnitude < 0.0001f;
	}

	protected virtual void Update()
	{
		if (_pointerDownEvent?.ClickHeldTime() > InputManager.I.ClickThreshold)
		{
			if (_pointerDownEvent.useDragThreshold)
			{
				ACardLayout.OnPointerHeld?.Invoke(this, _pointerDown);
			}
			_pointerDownEvent.useDragThreshold = false;
		}
		if (_isAtRest)
		{
			return;
		}
		bool atRest = !_pointerDrag && _restTime.HasValue;
		_inputIndex = null;
		using PoolKeepItemListHandle<CardLayoutElement> poolKeepItemListHandle = GetCards();
		for (int num = poolKeepItemListHandle.Count - 1; num >= _startUpdateIndex; num--)
		{
			CardLayoutElement cardLayoutElement = poolKeepItemListHandle[num];
			if (!(cardLayoutElement.layout != this))
			{
				if (cardLayoutElement.targets.Count == 0)
				{
					_UpdateCard(cardLayoutElement, num, poolKeepItemListHandle, Time.deltaTime, ref atRest);
				}
				else
				{
					float deltaTime = Time.deltaTime;
					while (deltaTime > 0f && (bool)cardLayoutElement)
					{
						_UpdateCard(cardLayoutElement, num, poolKeepItemListHandle, MathUtil.DiscretePhysicsTick(ref deltaTime), ref atRest);
					}
				}
			}
		}
		_UpdateDragInputCollider();
		if (atRest)
		{
			if ((_atRestTime += Time.deltaTime) >= _restTime)
			{
				_OnAtRest();
			}
		}
		else
		{
			_atRestTime = 0f;
		}
	}

	public virtual CardLayoutElement Add(CardLayoutElement card, bool addEvenIfInLayoutAlready = false)
	{
		bool flag = card.layout;
		if (card.layout == this && !addEvenIfInLayoutAlready)
		{
			return card;
		}
		bool flag2 = flag && card.layout.pointerOver == card;
		int count = card.targets.Count;
		if ((bool)card.layout)
		{
			using PoolKeepItemListHandle<CardLayoutElement> poolKeepItemListHandle = card.layout.GetCards();
			bool flag3 = card.layout._pointerDrag;
			card.layout._OnRemovedFromLayout(card, poolKeepItemListHandle.value.IndexOf(card), poolKeepItemListHandle.Count);
			if (card.targets.Count == 0 && !flag3)
			{
				card.layout.soundPack?.GetExitLayout(poolKeepItemListHandle.Count == 1)?.Play(card.transform.position, Random, card.layout.soundPack.mixerGroup);
				ACardLayout[] array = card.layout.skipExitTransitionWhenTransferredTo;
				if (array == null || !array.Contains(this))
				{
					foreach (CardLayoutElement.Target item in card.layout._GetExitTargets(card, flag2, poolKeepItemListHandle))
					{
						card.targets.Add(item);
					}
				}
			}
			if (addEvenIfInLayoutAlready && card.layout == this)
			{
				card.transform.SetParent(null, worldPositionStays: true);
			}
		}
		if (count == card.targets.Count)
		{
			card.ClearTransitions();
		}
		using PoolKeepItemListHandle<CardLayoutElement> poolKeepItemListHandle2 = GetCards();
		int num = (_addSorted ? poolKeepItemListHandle2.value.AddSorted(card) : (poolKeepItemListHandle2.value.AddReturnList(card).Count - 1));
		card.transform.SetParent(cardContainer, worldPositionStays: true);
		card.transform.SetSiblingIndex(num);
		card.layout = this;
		_inputIndex = null;
		_OnAddedToLayout(card, num, poolKeepItemListHandle2.Count);
		if (!flag)
		{
			_GetLayoutTarget(card, num, poolKeepItemListHandle2.Count).target.SetValues(card.transform);
		}
		else
		{
			foreach (CardLayoutElement.Target item2 in _GetEnterTargets(card))
			{
				card.targets.Add(item2);
			}
		}
		if (card.targets.Count > 0)
		{
			_SetInputEnabled(card, inputEnabled: false);
		}
		else
		{
			card.deck?.SignalAtRest(card.card);
		}
		if (flag2 && card.inputEnabled)
		{
			pointerOver = card;
		}
		card.shouldShowCanDrag = _CanDrag(card);
		return card;
	}

	public virtual void RefreshPointerOver()
	{
		CardLayoutElement cardLayoutElement = pointerOver;
		if ((object)cardLayoutElement != null && cardLayoutElement.inputEnabled)
		{
			cardLayoutElement.deck?.SignalPointerExit(cardLayoutElement.card);
			cardLayoutElement.deck?.SignalPointerEnter(cardLayoutElement.card);
		}
	}

	public virtual void RefreshPointerExit()
	{
		CardLayoutElement cardLayoutElement = pointerOver;
		if ((object)cardLayoutElement != null && cardLayoutElement.inputEnabled)
		{
			cardLayoutElement.deck?.SignalPointerExit(cardLayoutElement.card);
		}
	}

	public virtual PoolKeepItemListHandle<CardLayoutElement> GetCards()
	{
		return Pools.UseKeepItemList(_cards);
	}

	public virtual void ClearSmartTargets()
	{
		_smartDragTargets?.Clear();
	}

	public virtual TransformData GetLayoutTarget(CardLayoutElement card)
	{
		using PoolKeepItemListHandle<CardLayoutElement> poolKeepItemListHandle = GetCards();
		return _GetLayoutTarget(card, poolKeepItemListHandle.value.IndexOf(card), poolKeepItemListHandle.Count).target;
	}

	public TransformData GetLayoutTargetWithOffset(CardLayoutElement card)
	{
		TransformData result = GetLayoutTarget(card);
		Matrix4x4? matrix4x = _UpdateOffsetMatrix(card);
		if (matrix4x.HasValue)
		{
			Matrix4x4 valueOrDefault = matrix4x.GetValueOrDefault();
			result = result.TransformTRS(valueOrDefault);
		}
		return result;
	}

	public virtual bool IsAtRest()
	{
		foreach (CardLayoutElement card in GetCards())
		{
			if (!card.atRestInLayout)
			{
				return false;
			}
		}
		return true;
	}

	public virtual DragThresholdData SetDragThresholds(DragThresholdData dragThresholds)
	{
		DragThresholdData result = new DragThresholdData(this);
		dragThresholds.SetThresholds(this);
		return result;
	}

	public virtual void ForceFinishLayoutAnimations()
	{
		foreach (CardLayoutElement card in GetCards())
		{
			card.ClearTransitions();
			GetLayoutTargetWithOffset(card).SetValues(card.transform);
		}
	}

	public virtual void RemoveCard(CardLayoutElement card)
	{
		_cards.Remove(card);
		_UpdateCardGrouping();
		this.onCardRemoved?.Invoke(this, card);
		card.layout = null;
	}

	public virtual void DisableInput()
	{
		disableInputColliderManagement = false;
		foreach (CardLayoutElement card in GetCards())
		{
			_SetInputEnabled(card, inputEnabled: false);
		}
		disableInputColliderManagement = true;
	}

	public virtual void EnableInput()
	{
		disableInputColliderManagement = false;
		foreach (CardLayoutElement card in GetCards())
		{
			_SetInputEnabled(card, inputEnabled: true);
		}
	}

	public void SetDirty()
	{
		_atRestTime = 0f;
	}

	public SpecialTransitionTargets GetSpecialTransitionTargets(SpecialTransitionTargets.TargetType type)
	{
		foreach (SpecialTransitionTargets item in base.gameObject.GetComponentsPooled<SpecialTransitionTargets>(includeInactive: false))
		{
			if (item.transitionType == type)
			{
				return item;
			}
		}
		return null;
	}

	public virtual IEnumerable<ACardLayout> GetCardLayouts()
	{
		yield return this;
	}

	public void SetPointerOverPaddingOverride(CardLayoutElement.PointerOverPadding? paddingOverride)
	{
		pointerOverPaddingOverride = paddingOverride;
		if ((bool)pointerOver)
		{
			pointerOver.pointerOverPadding = paddingOverride ?? _pointerOverPadding;
		}
	}
}
