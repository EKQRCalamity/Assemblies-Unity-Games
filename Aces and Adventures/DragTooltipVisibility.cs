using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PointerDrag3D))]
public class DragTooltipVisibility : MonoBehaviour
{
	[Range(0.01f, 1f)]
	public float gracePeriod = 0.25f;

	[Range(0.01f, 10f)]
	public float dragSpeedThreshold = 1f;

	public UnityEvent onShowRequest;

	public UnityEvent onHideRequest;

	private bool _isDragging;

	private PointerDrag3D _drag;

	private bool _isShowing;

	private Vector3 _previousPosition;

	private float _timer;

	private int _frameOfLastEndDrag;

	private PointerDrag3D drag => this.CacheComponent(ref _drag);

	private void _OnBeginDrag(PointerEventData eventData)
	{
		if (_frameOfLastEndDrag != Time.frameCount)
		{
			_timer = gracePeriod;
			_previousPosition = base.transform.position;
			_isDragging = true;
		}
	}

	private void _OnEndDrag(PointerEventData eventData)
	{
		_frameOfLastEndDrag = Time.frameCount;
		_isDragging = false;
		_Hide();
	}

	private void _Show()
	{
		if (!_isShowing)
		{
			_isShowing = true;
			onShowRequest?.Invoke();
		}
	}

	private void _Hide()
	{
		if (_isShowing)
		{
			_isShowing = false;
			onHideRequest?.Invoke();
		}
	}

	private void Awake()
	{
		drag.OnBegin.AddListener(_OnBeginDrag);
		drag.OnEnd.AddListener(_OnEndDrag);
	}

	private void Update()
	{
		if (_isDragging)
		{
			if (((base.transform.position - _previousPosition) / Time.deltaTime.InsureNonZero()).sqrMagnitude > dragSpeedThreshold * dragSpeedThreshold)
			{
				_timer = gracePeriod;
			}
			else
			{
				_timer -= Time.deltaTime;
			}
			_timer = Mathf.Clamp(_timer, 0f, gracePeriod);
			if (_timer <= 0f)
			{
				_Show();
			}
			else if (_timer >= gracePeriod)
			{
				_Hide();
			}
			_previousPosition = base.transform.position;
		}
	}

	public void Stop()
	{
		_OnEndDrag(null);
	}
}
