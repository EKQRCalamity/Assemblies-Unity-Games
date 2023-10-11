using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TooltipCreator), typeof(TooltipVisibility), typeof(PointerOverTrigger))]
public abstract class TooltipGenerator : MonoBehaviour
{
	private TooltipCreator _creator;

	private TooltipVisibility _visibility;

	private PointerOverTrigger _pointerOverTrigger;

	public TooltipCreator creator
	{
		get
		{
			if (!(_creator != null))
			{
				return _creator = base.gameObject.GetOrAddComponent<TooltipCreator>();
			}
			return _creator;
		}
	}

	public TooltipVisibility visibility
	{
		get
		{
			if (!(_visibility != null))
			{
				return _visibility = base.gameObject.GetOrAddComponent<TooltipVisibility>();
			}
			return _visibility;
		}
	}

	public PointerOverTrigger pointerOverTrigger
	{
		get
		{
			if (!(_pointerOverTrigger != null))
			{
				return _pointerOverTrigger = base.gameObject.GetOrAddComponent<PointerOverTrigger>();
			}
			return _pointerOverTrigger;
		}
	}

	public bool ignorePointerEventsWhileDragging
	{
		get
		{
			return visibility.ignorePointerEventsWhileDragging;
		}
		set
		{
			visibility.ignorePointerEventsWhileDragging = value;
			pointerOverTrigger.pointerExitOnBeginDrag = value;
		}
	}

	public bool ignorePointerEvents
	{
		get
		{
			return visibility.ignorePointerEvents;
		}
		set
		{
			visibility.ignorePointerEvents = value;
		}
	}

	public bool ignoreClearTooltipRequests { get; set; }

	public bool clearOnDisable { get; set; }

	private void Awake()
	{
		UnityEventExtensions.AddListenerGeneric<PointerEvent, PointerEventData>(ref pointerOverTrigger.OnPointerEnterEvent, visibility.StartTimer);
		UnityEventExtensions.AddListenerGeneric<PointerEvent, PointerEventData>(ref pointerOverTrigger.OnPointerExitEvent, visibility.EndTimer);
		UnityEventExtensions.AddListener(ref visibility.OnShowTooltip, OnShowTooltip);
		UnityEventExtensions.AddListener(ref visibility.OnHideTooltip, OnHideTooltip);
		creator.AddGenerator(this);
	}

	private void OnDisable()
	{
		if (clearOnDisable)
		{
			_ClearTooltip();
		}
	}

	private void OnDestroy()
	{
		if ((bool)_creator)
		{
			_creator.RemoveGenerator(this);
		}
	}

	public void OnShowTooltip()
	{
		if (base.enabled)
		{
			_OnShowTooltip();
		}
	}

	protected abstract void _OnShowTooltip();

	public virtual void OnHideTooltip()
	{
		creator.Hide();
	}

	public void ClearTooltip()
	{
		if (!ignoreClearTooltipRequests)
		{
			_ClearTooltip();
		}
	}

	protected virtual void _ClearTooltip()
	{
	}
}
