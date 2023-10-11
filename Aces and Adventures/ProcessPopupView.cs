using System;
using UnityEngine;

public class ProcessPopupView : MonoBehaviour
{
	[Range(0.01f, 5f)]
	public float cancelHoldTime = 1.5f;

	public StringEvent onTextChange;

	public StringEvent onSubTextChange;

	public FloatEvent onProgressChange;

	public BoolEvent onShowCancelChange;

	public FloatEvent onCancelProgressChange;

	private float _cancelHoldTime;

	public static event Action OnCancelRequested;

	private void Start()
	{
		onShowCancelChange.Invoke(ProcessPopupView.OnCancelRequested != null);
	}

	private void Update()
	{
		if (ProcessPopupView.OnCancelRequested != null)
		{
			_cancelHoldTime = Mathf.Max(0f, Input.GetKey(KeyCode.Escape) ? (_cancelHoldTime + Time.unscaledDeltaTime) : (_cancelHoldTime - Time.unscaledDeltaTime));
			onCancelProgressChange.Invoke(Mathf.Clamp01(_cancelHoldTime / cancelHoldTime));
			if (!(_cancelHoldTime < cancelHoldTime))
			{
				ProcessPopupView.OnCancelRequested();
				ProcessPopupView.OnCancelRequested = null;
				onShowCancelChange.Invoke(arg0: false);
			}
		}
	}

	private void OnDestroy()
	{
		ProcessPopupView.OnCancelRequested = null;
	}

	public void SetText(string text)
	{
		onTextChange.Invoke(text);
	}

	public void SetSubText(string subText)
	{
		onSubTextChange.Invoke(subText);
	}

	public void SetProgress(float progress)
	{
		onProgressChange.Invoke(progress);
	}

	public void RegisterCancelRequest(Action onCancel)
	{
		OnCancelRequested += onCancel;
		onShowCancelChange.Invoke(arg0: true);
	}
}
