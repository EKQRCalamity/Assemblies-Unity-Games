using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PointerOver3D), typeof(PointerClick3D), typeof(PointerDrag3D))]
public class SelectableItem : MonoBehaviour
{
	public enum State : byte
	{
		None,
		Highlighted,
		Selected
	}

	[Flags]
	public enum StateFlags : byte
	{
		None = 1,
		Highlighted = 2,
		Selected = 4
	}

	[SerializeField]
	protected List<Component> _contextItems;

	[SerializeField]
	protected BoolEvent _onSelectedChange;

	[SerializeField]
	protected IntEvent _onStateChange;

	[SerializeField]
	protected PointerEvent _onDoubleClick;

	private PointerOver3D _pointerOver;

	private PointerClick3D _pointerClick;

	private PointerDrag3D _pointerDrag;

	private SelectableGroup _group;

	private bool _selected;

	private StateFlags _rawState = StateFlags.None;

	private StateFlags _state;

	private int _secondaryHighlights;

	public List<Component> contextItems => _contextItems ?? (_contextItems = new List<Component>());

	public BoolEvent onSelectedChange => _onSelectedChange ?? (_onSelectedChange = new BoolEvent());

	public IntEvent onStateChange => _onStateChange ?? (_onStateChange = new IntEvent());

	public PointerEvent onDoubleClick => _onDoubleClick ?? (_onDoubleClick = new PointerEvent());

	public PointerOver3D pointerOver => this.CacheComponent(ref _pointerOver);

	public PointerClick3D pointerClick => this.CacheComponent(ref _pointerClick);

	public PointerDrag3D pointerDrag => this.CacheComponent(ref _pointerDrag);

	public SelectableGroup group => this.CacheComponentInParent(ref _group);

	public bool selected
	{
		get
		{
			return _selected;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _selected, value))
			{
				onSelectedChange.Invoke(_selected);
			}
		}
	}

	public int secondaryHighlights
	{
		get
		{
			return _secondaryHighlights;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _secondaryHighlights, value))
			{
				state = rawState;
			}
		}
	}

	private StateFlags _secondaryHighlightFlag
	{
		get
		{
			if (_secondaryHighlights <= 0)
			{
				return (StateFlags)0;
			}
			return StateFlags.Highlighted;
		}
	}

	private StateFlags rawState
	{
		get
		{
			return _rawState;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _rawState, value))
			{
				state = rawState;
			}
		}
	}

	private StateFlags state
	{
		get
		{
			return _state;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _state, value | _secondaryHighlightFlag))
			{
				onStateChange.Invoke((int)EnumUtil<StateFlags>.ConvertFromFlag<State>(EnumUtil.MaxActiveFlag(value | _secondaryHighlightFlag)));
			}
		}
	}

	private void Awake()
	{
		onSelectedChange.AddListener(_OnSelectedChange);
	}

	private void OnEnable()
	{
		_RegisterEvents();
	}

	private void OnDisable()
	{
		selected = false;
		rawState = StateFlags.None;
		_UnregisterEvents();
		_group = null;
	}

	private void OnTransformParentChanged()
	{
		_group = null;
	}

	private void _RegisterEvents()
	{
		pointerOver.OnEnter.AddListener(_OnPointerEnter);
		pointerOver.OnExit.AddListener(_OnPointerExit);
		pointerClick.OnClick.AddListener(_OnLeftClick);
		pointerClick.OnRightClick.AddListener(_OnRightClick);
		pointerDrag.OnBegin.AddListener(_OnBeginDrag);
		pointerDrag.OnDragged.AddListener(_OnDrag);
		pointerDrag.OnEnd.AddListener(_OnEndDrag);
	}

	private void _UnregisterEvents()
	{
		pointerOver.OnEnter.RemoveListener(_OnPointerEnter);
		pointerOver.OnExit.RemoveListener(_OnPointerExit);
		pointerClick.OnClick.RemoveListener(_OnLeftClick);
		pointerClick.OnRightClick.RemoveListener(_OnRightClick);
		pointerDrag.OnBegin.RemoveListener(_OnBeginDrag);
		pointerDrag.OnDragged.RemoveListener(_OnDrag);
		pointerDrag.OnEnd.RemoveListener(_OnEndDrag);
	}

	private void _OnPointerEnter(PointerEventData eventData)
	{
		rawState = EnumUtil.Add(rawState, StateFlags.Highlighted);
	}

	private void _OnPointerExit(PointerEventData eventData)
	{
		rawState = EnumUtil.Subtract(rawState, StateFlags.Highlighted);
	}

	private void _OnLeftClick(PointerEventData eventData)
	{
		if ((bool)group)
		{
			group.OnSelectableItemClick(this, eventData);
		}
		if (eventData.clickCount == 2)
		{
			onDoubleClick.Invoke(eventData);
		}
	}

	private void _OnRightClick(PointerEventData eventData)
	{
		if ((bool)group && !selected)
		{
			group.OnSelectableItemClick(this, eventData, signalIfDirty: true);
		}
	}

	private void _OnBeginDrag(PointerEventData eventData)
	{
		if ((bool)group)
		{
			group.OnSelectableItemBeginDrag(this, eventData);
		}
	}

	private void _OnDrag(PointerEventData eventData)
	{
		if ((bool)group)
		{
			group.OnSelectableItemDrag(this, eventData);
		}
	}

	private void _OnEndDrag(PointerEventData eventData)
	{
		if ((bool)group)
		{
			group.OnSelectableItemEndDrag(this, eventData);
		}
	}

	private void _OnSelectedChange(bool isSelected)
	{
		rawState = (isSelected ? EnumUtil.Add(rawState, StateFlags.Selected) : EnumUtil.Subtract(rawState, StateFlags.Selected));
	}

	public void SetSelected(bool isSelected)
	{
		group[this] = isSelected;
	}
}
