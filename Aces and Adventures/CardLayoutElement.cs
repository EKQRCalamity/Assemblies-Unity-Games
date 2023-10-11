using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PointerOver3D), typeof(PointerClick3D), typeof(PointerDrag3D))]
public abstract class CardLayoutElement : MonoBehaviour, IComparable<CardLayoutElement>, IShowCanDrag, IDragThresholdSetter, IEventSystemHandler
{
	public struct Target : IEquatable<Target>
	{
		public enum TransitionType
		{
			None,
			Enter,
			Exit
		}

		public readonly ACardLayout.Target data;

		public readonly TransformData target;

		public readonly TransitionType transition;

		public bool isEnterTransition => transition == TransitionType.Enter;

		public Target(ACardLayout.Target data, TransformData target, TransitionType transition = TransitionType.None)
		{
			this.data = data;
			this.target = target;
			this.transition = transition;
		}

		public Target SetTarget(TransformData newTarget)
		{
			return new Target(data, newTarget, transition);
		}

		public static implicit operator TransformData(Target target)
		{
			return target.target;
		}

		public static implicit operator TransformSpringSettings(Target target)
		{
			return target.data.springSettings;
		}

		public bool Equals(Target other)
		{
			if (object.Equals(data, other.data))
			{
				return target.Equals(other.target);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Target other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int num = ((data != null) ? data.GetHashCode() : 0) * 397;
			TransformData transformData = target;
			return num ^ transformData.GetHashCode();
		}
	}

	[Serializable]
	public struct PointerOverPadding
	{
		public static readonly PointerOverPadding None;

		public Vector2 xPadding;

		public Vector2 yPadding;

		public Vector2 zPadding;

		public PointerOverPadding(Vector2 xPadding, Vector2 yPadding, Vector2 zPadding)
		{
			this.xPadding = xPadding;
			this.yPadding = yPadding;
			this.zPadding = zPadding;
		}

		public PointerOverPadding PadAxis(AxisType axis, float amount)
		{
			PointerOverPadding result = this;
			if (amount == 0f)
			{
				return result;
			}
			switch (axis)
			{
			case AxisType.X:
				result.xPadding += new Vector2(amount, amount);
				break;
			case AxisType.Y:
				result.yPadding += new Vector2(amount, amount);
				break;
			default:
				result.zPadding += new Vector2(amount, amount);
				break;
			}
			return result;
		}

		public static PointerOverPadding operator +(PointerOverPadding a, PointerOverPadding b)
		{
			return new PointerOverPadding(a.xPadding + b.xPadding, a.yPadding + b.yPadding, a.zPadding + b.zPadding);
		}

		public static implicit operator bool(PointerOverPadding padding)
		{
			if (!(padding.xPadding != Vector2.zero) && !(padding.yPadding != Vector2.zero))
			{
				return padding.zPadding != Vector2.zero;
			}
			return true;
		}
	}

	[NonSerialized]
	public List<Target> targets = new List<Target>();

	[NonSerialized]
	public TransformVelocities velocities;

	public BoolEvent onBeingDraggedChange;

	[NonSerialized]
	public Matrix4x4? offset;

	private List<Matrix4x4> _offsets;

	private PointerOver3D _pointerOver;

	private PointerClick3D _pointerClick;

	private PointerDrag3D _pointerDrag;

	private Collider _inputCollider;

	public ADeckLayoutBase deck { get; set; }

	public ACardLayout layout { get; set; }

	public abstract ATarget card { get; protected set; }

	public int index => deck?.GetIndexOf(card) ?? 0;

	public PointerOver3D pointerOver => _pointerOver;

	public PointerClick3D pointerClick => _pointerClick;

	public PointerDrag3D pointerDrag => _pointerDrag;

	public Collider inputCollider => _inputCollider;

	public bool shouldShowCanDrag { get; set; }

	public bool noiseAnimationEnabled { get; set; } = true;


	public bool ignoreOffsetInExitTarget { get; set; }

	public bool inputEnabled
	{
		get
		{
			return inputCollider?.enabled ?? false;
		}
		set
		{
			if ((bool)inputCollider)
			{
				inputCollider.enabled = value;
			}
		}
	}

	public PointerOverPadding pointerOverPadding
	{
		set
		{
			if ((bool)inputCollider)
			{
				inputCollider.transform.localScale = new Vector3(1f + value.xPadding.x + value.xPadding.y, 1f + value.yPadding.x + value.yPadding.y, 1f + value.zPadding.x + value.zPadding.y);
				inputCollider.transform.localPosition = new Vector3((value.xPadding.y - value.xPadding.x) * 0.5f, (value.yPadding.y - value.yPadding.x) * 0.5f, (value.zPadding.y - value.zPadding.x) * 0.5f);
			}
		}
	}

	public bool hasOffset => !_offsets.IsNullOrEmpty();

	public bool hasTransition => !targets.IsNullOrEmpty();

	public List<Matrix4x4> offsets => _offsets ?? (_offsets = new List<Matrix4x4>());

	public bool atRestInLayout => targets.IsNullOrEmpty();

	public bool isLastCardInPile => index == deck.GetCountInPile(card) - 1;

	public bool isLastCardInLayout
	{
		get
		{
			int siblingIndex = base.transform.GetSiblingIndex();
			ACardLayout aCardLayout = layout;
			return siblingIndex == (((object)aCardLayout != null) ? new int?(aCardLayout.Count - 1) : null);
		}
	}

	protected virtual bool _shouldCacheInputColliderOnAwake => true;

	protected virtual bool _includeInactiveInInputColliderSearch => false;

	private void _OnPointerEnter(PointerEventData eventData)
	{
		layout?.OnPointerEnter(eventData, this);
	}

	private void _OnPointerExit(PointerEventData eventData)
	{
		layout?.OnPointerExit(eventData, this);
		pointerOverPadding = layout?.defaultPointerOverPadding ?? PointerOverPadding.None;
	}

	private void _OnPointerDown(PointerEventData eventData)
	{
		layout?.OnPointerDown(eventData, this);
	}

	private void _OnPointerUp(PointerEventData eventData)
	{
		layout?.OnPointerUp(eventData, this);
	}

	private void _OnPointerClick(PointerEventData eventData)
	{
		ACardLayout aCardLayout = layout;
		if ((object)aCardLayout != null && aCardLayout.ShouldAllowClick(eventData))
		{
			deck?.SignalPointerClick(card, eventData);
		}
	}

	private void _OnInitializeDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Middle)
		{
			eventData.pointerDrag = null;
		}
		else
		{
			layout?.OnDragInitialize(eventData, this);
		}
	}

	private void _OnDragBegin(PointerEventData eventData)
	{
		layout?.OnDragBegin(eventData, this);
		onBeingDraggedChange?.Invoke(arg0: true);
	}

	private void _OnDrag(PointerEventData eventData)
	{
		layout?.OnDrag(eventData, this);
	}

	private void _OnDragEnd(PointerEventData eventData)
	{
		layout?.OnDragEnd(eventData, this);
		onBeingDraggedChange?.Invoke(arg0: false);
	}

	protected virtual void Awake()
	{
		this.CacheComponent(ref _pointerOver);
		this.CacheComponent(ref _pointerClick);
		this.CacheComponent(ref _pointerDrag);
		if (_shouldCacheInputColliderOnAwake)
		{
			this.CacheComponentInChildren(ref _inputCollider, _includeInactiveInInputColliderSearch);
		}
		pointerOver.OnEnter.AddListener(_OnPointerEnter);
		pointerOver.OnExit.AddListener(_OnPointerExit);
		pointerClick.OnDown.AddListener(_OnPointerDown);
		pointerClick.OnRightDown.AddListener(_OnPointerDown);
		pointerClick.OnUp.AddListener(_OnPointerUp);
		pointerClick.OnRightUp.AddListener(_OnPointerUp);
		pointerClick.OnClick.AddListener(_OnPointerClick);
		pointerDrag.OnInitialize.AddListener(_OnInitializeDrag);
		pointerDrag.OnBegin.AddListener(_OnDragBegin);
		pointerDrag.OnDragged.AddListener(_OnDrag);
		pointerDrag.OnEnd.AddListener(_OnDragEnd);
	}

	protected virtual void Start()
	{
		onBeingDraggedChange?.Invoke(arg0: false);
	}

	protected virtual void OnDisable()
	{
		layout?.RemoveCard(this);
		targets?.Clear();
		velocities = default(TransformVelocities);
		offset = null;
		_offsets?.Clear();
	}

	public CardLayoutElement SetData(ATarget cardToSet, ADeckLayoutBase deckToSet)
	{
		card = cardToSet;
		deck = deckToSet;
		return this;
	}

	public TransformData GetLayoutTarget()
	{
		return layout?.GetLayoutTarget(this) ?? new TransformData(base.transform);
	}

	public IEnumerator WaitTillFinishedAnimating()
	{
		while (targets.Count > 0)
		{
			yield return null;
		}
	}

	public void ClearExitTransitions()
	{
		if (targets.Count == 0)
		{
			return;
		}
		for (int num = targets.Count - 1; num >= 0; num--)
		{
			if (!targets[num].isEnterTransition)
			{
				targets.RemoveAt(num);
			}
		}
		if (targets.Count == 0)
		{
			layout._SetInputEnabled(this, inputEnabled: true);
		}
	}

	public void ClearEnterTransitions()
	{
		if (targets.Count == 0)
		{
			return;
		}
		for (int num = targets.Count - 1; num >= 0; num--)
		{
			if (targets[num].isEnterTransition)
			{
				targets.RemoveAt(num);
			}
		}
		if (targets.Count == 0)
		{
			layout._SetInputEnabled(this, inputEnabled: true);
		}
	}

	public void ClearTransitions()
	{
		if (targets.Count != 0)
		{
			targets.Clear();
			layout._SetInputEnabled(this, inputEnabled: true);
		}
	}

	public bool ShouldShowCanDrag()
	{
		if (shouldShowCanDrag)
		{
			return atRestInLayout;
		}
		return false;
	}

	public void OnSetDragThreshold(PointerEventData eventData)
	{
		InputManager.I.eventSystem.pixelDragThreshold = ProfileManager.options.controls.clickingAndDragging.cardDragThreshold;
	}

	public int CompareTo(CardLayoutElement other)
	{
		int? num = deck?.sortOrder.CompareTo(other.deck.sortOrder);
		if (num.HasValue)
		{
			int valueOrDefault = num.GetValueOrDefault();
			if (valueOrDefault != 0)
			{
				return valueOrDefault * layout.reverseDeckSortOrder.ToInt(-1, 1);
			}
		}
		return index.CompareTo(other.index);
	}
}
