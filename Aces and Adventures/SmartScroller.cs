using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SmartScroller : MonoBehaviour
{
	public ScrollRect scrollRect;

	[Range(0.001f, 60f)]
	public float scrollEaseSpeed;

	public bool instantScroll;

	public Vector2Event OnScrollChange;

	private bool _shouldBeScrollingDown;

	private float? _previousScrollPosition;

	private void OnTransformChildrenChanged()
	{
		_shouldBeScrollingDown = true;
	}

	private void Update()
	{
		float value = scrollRect.verticalNormalizedPosition;
		if (_shouldBeScrollingDown)
		{
			if (!instantScroll)
			{
				MathUtil.Ease(ref value, 0f, scrollEaseSpeed, Time.unscaledDeltaTime);
			}
			else
			{
				value = 0f;
			}
			if (value < 0.001f)
			{
				value = 0f;
				_shouldBeScrollingDown = false;
			}
			scrollRect.verticalNormalizedPosition = value;
		}
		if (!_previousScrollPosition.HasValue)
		{
			_previousScrollPosition = value;
		}
		else if (Math.Abs(_previousScrollPosition.Value - value) > 0.005f && scrollRect.CanScrollVertical())
		{
			_previousScrollPosition = value;
			OnScrollChange.Invoke(scrollRect.normalizedPosition);
		}
	}
}
