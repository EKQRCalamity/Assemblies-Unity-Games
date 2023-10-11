using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(TextMeshProUGUI))]
[DisallowMultipleComponent]
public class TextMeshFlasher : MonoBehaviour
{
	private enum State
	{
		In,
		Hold,
		Out,
		Off
	}

	public bool overrideExistingMessage = true;

	[Space(20f)]
	[Range(0.01f, 2f)]
	public float inTime = 0.333f;

	[Range(0.01f, 5f)]
	public float holdTime = 2.333f;

	[Range(0.01f, 2f)]
	public float outTime = 0.333f;

	[Range(0f, 1f)]
	public float transitionEasing;

	[Space(20f)]
	public AnimationCurve colorTransition = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AnimationCurve colorHold = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public Color startColor = new Color(1f, 1f, 1f, 0f);

	public Color endColor = new Color(1f, 1f, 1f);

	[Space(20f)]
	public AnimationCurve translateTransition = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AnimationCurve translateHold = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public Vector3 startTranslate = Vector3.zero;

	public Vector3 endTranslate = Vector3.zero;

	[Space(20f)]
	public AnimationCurve scaleTransition = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AnimationCurve scaleHold = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public Vector3 startScale = Vector3.one;

	public Vector3 endScale = Vector3.one;

	[Space(20f)]
	public AnimationCurve rotationTransition = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AnimationCurve rotationHold = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public Vector3 startRotation = Vector3.zero;

	public Vector3 endRotation = Vector3.zero;

	private Queue<string> _queue;

	private float _lastShowCallTime;

	private bool _off;

	private RectTransform _rt;

	private TextMeshProUGUI _text;

	private State _state
	{
		get
		{
			float timeSinceShow = _timeSinceShow;
			if (timeSinceShow > _totalTime)
			{
				return State.Off;
			}
			if (timeSinceShow < inTime)
			{
				return State.In;
			}
			if (timeSinceShow < _inAndHold)
			{
				return State.Hold;
			}
			return State.Out;
		}
	}

	private float _timeSinceShow => Time.realtimeSinceStartup - _lastShowCallTime;

	private float _inAndHold => inTime + holdTime;

	private float _totalTime => inTime + holdTime + outTime;

	private float _transitionEaseTime => holdTime * transitionEasing;

	private void Awake()
	{
		_queue = new Queue<string>();
		_rt = GetComponent<RectTransform>();
		_text = GetComponent<TextMeshProUGUI>();
		_lastShowCallTime = float.MinValue;
		startTranslate += _rt.localPosition;
		endTranslate += _rt.localPosition;
	}

	private void LateUpdate()
	{
		State state = _state;
		if (_off && state == State.Off && _queue.Count == 0)
		{
			return;
		}
		float lerpAmount = GetLerpAmount(state);
		float num;
		float num2;
		float num3;
		float num4;
		if (state != State.Hold)
		{
			num = colorTransition.Evaluate(lerpAmount);
			num2 = translateTransition.Evaluate(lerpAmount);
			num3 = scaleTransition.Evaluate(lerpAmount);
			num4 = rotationTransition.Evaluate(lerpAmount);
		}
		else
		{
			float num5 = 1f;
			if (transitionEasing > 0f)
			{
				float num6 = _timeSinceShow - inTime;
				num5 = Mathf.Min(num6, holdTime - num6) / _transitionEaseTime;
			}
			num = colorHold.Evaluate(lerpAmount);
			num2 = translateHold.Evaluate(lerpAmount);
			num3 = scaleHold.Evaluate(lerpAmount);
			num4 = rotationHold.Evaluate(lerpAmount);
			if (num5 < 1f)
			{
				num = MathUtil.Lerp(colorTransition.Evaluate(1f), num, num5);
				num2 = MathUtil.Lerp(translateTransition.Evaluate(1f), num2, num5);
				num3 = MathUtil.Lerp(scaleTransition.Evaluate(1f), num3, num5);
				num4 = MathUtil.Lerp(rotationTransition.Evaluate(1f), num4, num5);
			}
		}
		_text.color = startColor.Lerp(endColor, num);
		_rt.localPosition = startTranslate.Lerp(endTranslate, num2);
		_rt.localScale = startScale.Lerp(endScale, num3);
		_rt.localRotation = Quaternion.Euler(startRotation.Lerp(endRotation, num4));
		if (state == State.Off && _queue.Count > 0)
		{
			SetText(_queue.Dequeue());
		}
		_off = state == State.Off;
	}

	private float GetLerpAmount(State state)
	{
		float timeSinceShow = _timeSinceShow;
		return state switch
		{
			State.Off => 0f, 
			State.In => timeSinceShow / inTime, 
			State.Hold => (timeSinceShow - inTime) / holdTime, 
			State.Out => 1f - (timeSinceShow - _inAndHold) / outTime, 
			_ => 0f, 
		};
	}

	public void QueueMessage(string text)
	{
		if (!base.enabled)
		{
			return;
		}
		_queue.Enqueue(text);
		if (overrideExistingMessage)
		{
			State state = _state;
			if (state < State.Out)
			{
				EndEarly(state);
			}
			while (_queue.Count > 1)
			{
				_queue.Dequeue();
			}
		}
	}

	private void EndEarly(State state)
	{
		switch (state)
		{
		case State.In:
		{
			float lerpAmount = GetLerpAmount(state);
			_lastShowCallTime = Time.realtimeSinceStartup - _inAndHold - outTime * (1f - lerpAmount);
			break;
		}
		case State.Hold:
			_lastShowCallTime = Time.realtimeSinceStartup - _inAndHold + _transitionEaseTime;
			break;
		}
	}

	private void SetText(string text)
	{
		_text.text = text;
		_lastShowCallTime = Time.realtimeSinceStartup;
	}
}
