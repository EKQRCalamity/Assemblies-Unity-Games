using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CycleMessager : MonoBehaviour
{
	public string multipleMessagePostFix = "…";

	public bool useScaledTime = true;

	[Header("Transition")]
	[Range(0.001f, 3f)]
	public float transitionTime = 0.25f;

	public float transitionStartValue;

	public float transitionEndValue = 1f;

	public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[Header("Timing")]
	[Range(0.01f, 1f)]
	public float cycleDurationPerLetter = 0.1f;

	[Range(0f, 5f)]
	public float clearDelay = 0.5f;

	[Header("Events")]
	public StringEvent OnMessageChange;

	public FloatEvent OnTransitionChange;

	private List<string> _messages;

	private int _messageIndex;

	private float? _elapsedMessageTime;

	private float? _elapsedClearTime;

	private float _t;

	private bool _pendingClear;

	private int _frameOfLastRequest = -1;

	private List<string> messages
	{
		get
		{
			return _messages ?? (_messages = new List<string>());
		}
		set
		{
			if (value.IsNullOrEmpty())
			{
				messages.Clear();
				_elapsedMessageTime = null;
				_targetTransitionState = false;
				return;
			}
			_elapsedClearTime = null;
			_pendingClear = false;
			_frameOfLastRequest = Time.frameCount;
			if (value.Count > 1 && multipleMessagePostFix != string.Empty)
			{
				for (int i = 0; i < value.Count; i++)
				{
					value[i] += multipleMessagePostFix;
				}
			}
			if (!messages.SequenceEqual(value))
			{
				messages.ClearAndCopyFrom(value);
				_messageIndex = 0;
				_elapsedMessageTime = null;
				if (!_isOff)
				{
					_targetTransitionState = false;
				}
				else
				{
					_ProcessPendingMessages();
				}
			}
		}
	}

	private bool _isOff => _t == 0f;

	private bool _targetTransitionState { get; set; }

	private void Update()
	{
		_UpdateTransition();
		_UpdateMessageTime();
		_UpdateClearTime();
	}

	private void _ClearMessages()
	{
		messages = null;
	}

	private void _UpdateTransition()
	{
		float num = _targetTransitionState.ToFloat();
		if (num != _t)
		{
			_t = Mathf.Clamp01(_t + GameUtil.GetDeltaTime(useScaledTime) / transitionTime * _targetTransitionState.ToFloat(1f, -1f));
			OnTransitionChange.Invoke(Mathf.Lerp(transitionStartValue, transitionEndValue, transitionCurve.Evaluate(_t)));
			if (!_targetTransitionState && num == _t)
			{
				_ProcessPendingMessages();
			}
		}
	}

	private void _UpdateMessageTime()
	{
		if (_elapsedMessageTime.HasValue && !(_elapsedMessageTime <= 0f))
		{
			_elapsedMessageTime -= GameUtil.GetDeltaTime(useScaledTime);
			if (_elapsedMessageTime <= 0f)
			{
				_targetTransitionState = false;
			}
		}
	}

	private void _UpdateClearTime()
	{
		if (_pendingClear && !(_pendingClear = false))
		{
			ClearMessages();
		}
		if (_elapsedClearTime.HasValue)
		{
			_elapsedClearTime -= GameUtil.GetDeltaTime(useScaledTime);
			if (!(_elapsedClearTime > 0f))
			{
				_ClearMessages();
				_elapsedClearTime = null;
			}
		}
	}

	private void _ProcessPendingMessages()
	{
		if (messages.Count != 0)
		{
			string text = messages[_messageIndex];
			_elapsedMessageTime = ((messages.Count > 1) ? new float?((float)text.RemoveRichText().Length * cycleDurationPerLetter) : null);
			_messageIndex = ++_messageIndex % messages.Count;
			OnMessageChange.Invoke(text);
			_targetTransitionState = true;
		}
	}

	public void SetMessage(string message)
	{
		if (message.IsNullOrEmpty())
		{
			ClearMessages();
			return;
		}
		using PoolKeepItemListHandle<string> poolKeepItemListHandle = Pools.UseKeepItemList<string>().Add(message);
		messages = poolKeepItemListHandle;
	}

	public void SetMessages(string message, string message2)
	{
		using PoolKeepItemListHandle<string> poolKeepItemListHandle = Pools.UseKeepItemList<string>().Add(message).Add(message2);
		messages = poolKeepItemListHandle;
	}

	public void SetMessages(string[] messages)
	{
		using PoolKeepItemListHandle<string> poolKeepItemListHandle = Pools.UseKeepItemList(messages.AsEnumerable());
		this.messages = poolKeepItemListHandle;
	}

	public void ClearMessages()
	{
		_elapsedClearTime = clearDelay;
	}

	public void MarkMessagesToBeCleared()
	{
		_pendingClear = Time.frameCount != _frameOfLastRequest;
	}
}
