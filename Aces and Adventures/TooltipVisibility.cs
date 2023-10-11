using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class TooltipVisibility : MonoBehaviour
{
	public const float MIN_WAIT = 0f;

	public const float DEFAULT_WAIT = 0.2f;

	[Range(0f, 10f)]
	public float waitTime = 0.2f;

	public bool usePendingLogicForEndTimer;

	public bool ignorePointerEventsWhileDragging;

	public bool ignorePointerEvents;

	[Header("Events")]
	public UnityEvent OnShowTooltip;

	public UnityEvent OnHideTooltip;

	private float _elapsed;

	private bool _pendingEndTimer;

	private void OnEnable()
	{
		_elapsed = float.MinValue;
	}

	private void OnDisable()
	{
		if ((bool)this)
		{
			_EndTimer();
		}
	}

	private void Update()
	{
		if (_elapsed != float.MinValue && !(_elapsed > waitTime) && (_elapsed += Time.unscaledDeltaTime) > waitTime)
		{
			OnShowTooltip?.Invoke();
		}
	}

	private void LateUpdate()
	{
		if (usePendingLogicForEndTimer && _pendingEndTimer)
		{
			_EndTimer();
		}
	}

	private void _EndTimer()
	{
		if (_elapsed >= waitTime)
		{
			OnHideTooltip?.Invoke();
		}
		_elapsed = float.MinValue;
		_pendingEndTimer = false;
	}

	public void StartTimer()
	{
		_elapsed = 0f;
		_pendingEndTimer = false;
	}

	public void StartTimer(PointerEventData pointerEvent)
	{
		if (!ignorePointerEvents && (!ignorePointerEventsWhileDragging || !pointerEvent.dragging))
		{
			StartTimer();
		}
	}

	public void StartTimer(float? waitTimeOverride)
	{
		StartTimer();
		_elapsed = Mathf.Min(waitTime, (waitTime - waitTimeOverride).GetValueOrDefault());
	}

	public void EndTimer()
	{
		if (!usePendingLogicForEndTimer)
		{
			_EndTimer();
		}
		else
		{
			_pendingEndTimer = true;
		}
	}

	public void EndTimer(PointerEventData pointerEvent)
	{
		if (!ignorePointerEvents && (!ignorePointerEventsWhileDragging || !pointerEvent.dragging))
		{
			EndTimer();
		}
	}

	public void SetTimerState(bool isOn)
	{
		if (isOn)
		{
			StartTimer();
		}
		else
		{
			EndTimer();
		}
	}

	public void SetVisibilityImmediate(bool visible)
	{
		if (visible)
		{
			_elapsed = Mathf.Max(_elapsed, waitTime);
			Update();
		}
		else
		{
			_EndTimer();
		}
	}

	public void Clear()
	{
		EndTimer();
		OnShowTooltip?.RemoveAllListeners();
		OnHideTooltip?.RemoveAllListeners();
	}

	public TooltipVisibility SetEvents(UnityAction show = null, UnityAction hide = null)
	{
		if (show != null)
		{
			(OnShowTooltip ?? (OnShowTooltip = new UnityEvent())).AddListener(show);
		}
		if (hide != null)
		{
			(OnHideTooltip ?? (OnHideTooltip = new UnityEvent())).AddListener(hide);
		}
		return this;
	}
}
