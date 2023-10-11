using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class SmartVisibility : MonoBehaviour
{
	[Header("[Fade In And Out]==============================")]
	[Range(0f, 3f)]
	public float fadeInTime;

	[Range(0f, 12f)]
	public float fadeOutTime;

	public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[Range(0f, 60f)]
	public float visibilityDuration;

	public bool useScaledTime;

	[Header("[Visibility Requests]==========================")]
	public bool ignoreRequestsWhenEmpty = true;

	public RectTransform requestVisibilityIfMouseOver;

	public CameraType mouseOverCameraType = CameraType.ScreenSpaceUI;

	public Camera mouseOverCamera;

	[Range(0f, 60f)]
	public float mouseOverVisibilyDuration;

	public bool requestVisibilityOnChildrenChange = true;

	[Header("[Events]=======================================")]
	public FloatEvent OnAlphaChange;

	public BoolEvent OnVisibilityChange;

	private Camera _camera;

	private float? _fadeInTimeRemaining;

	private float? _visibilityDurationRemaining;

	private float? _fadeOutTimeRemaining;

	private float _alpha = -1f;

	private void _UpdateVisibility(float alpha)
	{
		int num = Math.Sign(alpha);
		if (Math.Sign(_alpha) != num)
		{
			OnVisibilityChange.Invoke(num == 1);
		}
		OnAlphaChange.SInvoke(ref _alpha, alpha);
	}

	private void Awake()
	{
		_camera = (mouseOverCamera ? mouseOverCamera : CameraManager.Instance.GetCamera(mouseOverCameraType));
	}

	private void OnTransformChildrenChanged()
	{
		if (requestVisibilityOnChildrenChange)
		{
			RequestVisibility();
		}
	}

	private void Update()
	{
		if ((bool)requestVisibilityIfMouseOver && !EventSystem.current.IsPointerDragging() && RectTransformUtility.RectangleContainsScreenPoint(requestVisibilityIfMouseOver, Input.mousePosition, _camera))
		{
			_RequestVisibility(mouseOverVisibilyDuration);
		}
		float elapsed = (useScaledTime ? Time.deltaTime : Time.unscaledDeltaTime);
		float alpha = 0f;
		if (!_UpdateDuration(ref _fadeInTimeRemaining, elapsed, ref alpha, fadeInTime, fadeCurve, oneMinusCurve: true) && !_UpdateDuration(ref _visibilityDurationRemaining, elapsed, ref alpha))
		{
			_UpdateDuration(ref _fadeOutTimeRemaining, elapsed, ref alpha, fadeOutTime, fadeCurve);
		}
		_UpdateVisibility(alpha);
	}

	private bool _UpdateDuration(ref float? durationRemaining, float elapsed, ref float alpha, float duration = 1f, AnimationCurve curve = null, bool oneMinusCurve = false)
	{
		if (!durationRemaining.HasValue)
		{
			return false;
		}
		durationRemaining -= elapsed;
		float num = Mathf.Max(0f, durationRemaining.Value) / duration;
		alpha = curve?.Evaluate((!oneMinusCurve) ? num : (1f - num)) ?? 1f;
		if (durationRemaining <= 0f)
		{
			durationRemaining = null;
		}
		return true;
	}

	private void _RequestVisibility(float visibilityDuration)
	{
		if (!ignoreRequestsWhenEmpty || base.transform.childCount != 0)
		{
			if (fadeInTime > 0f && !_fadeInTimeRemaining.HasValue && !_visibilityDurationRemaining.HasValue)
			{
				float num = ((!_fadeOutTimeRemaining.HasValue) ? 1f : MathUtil.GetLerpAmount(fadeOutTime, 0f, _fadeOutTimeRemaining.Value));
				_fadeInTimeRemaining = fadeInTime * num;
			}
			if (this.visibilityDuration > 0f)
			{
				_visibilityDurationRemaining = Math.Max(_visibilityDurationRemaining.GetValueOrDefault(), visibilityDuration);
			}
			if (fadeOutTime > 0f)
			{
				_fadeOutTimeRemaining = fadeOutTime;
			}
		}
	}

	public void RequestVisibility()
	{
		_RequestVisibility(visibilityDuration);
	}

	public void RequestVisibility(bool request)
	{
		if (request)
		{
			RequestVisibility();
		}
	}

	public void RequestVisibility(float duration)
	{
		_RequestVisibility(duration);
	}

	public void Hide()
	{
		if (_visibilityDurationRemaining.HasValue)
		{
			_fadeInTimeRemaining = null;
			_visibilityDurationRemaining = null;
			_fadeOutTimeRemaining = fadeOutTime;
		}
	}
}
