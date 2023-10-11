using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(StateAnimator3D))]
public class StateAnimator3DEvents : MonoBehaviour
{
	public BoolEvent[] onStateChange;

	public UnityEvent[] onStateAnimationComplete;

	private StateAnimator3D _stateAnimator;

	private int _stateIndex;

	private StateAnimator3D stateAnimator => this.CacheComponent(ref _stateAnimator);

	private void _InsureValidEventArrays()
	{
		Array.Resize(ref onStateChange, stateAnimator.Count);
		Array.Resize(ref onStateAnimationComplete, stateAnimator.Count);
		for (int i = 0; i < stateAnimator.Count; i++)
		{
			if (onStateChange[i] == null)
			{
				onStateChange[i] = new BoolEvent();
			}
			if (onStateAnimationComplete[i] == null)
			{
				onStateAnimationComplete[i] = new UnityEvent();
			}
		}
	}

	private void _RegisterEvents()
	{
		_InsureValidEventArrays();
		_stateIndex = stateAnimator.stateIndex;
		stateAnimator.OnStateChange.AddListener(_OnStateChange);
		stateAnimator.OnStateAnimationComplete.AddListener(_OnStateAnimationComplete);
	}

	private void _UnregisterEvents()
	{
		stateAnimator.OnStateChange.RemoveListener(_OnStateChange);
		stateAnimator.OnStateAnimationComplete.RemoveListener(_OnStateAnimationComplete);
	}

	private void _OnStateChange(int stateIndex)
	{
		if (_stateIndex != stateIndex)
		{
			onStateChange[_stateIndex].Invoke(arg0: false);
		}
		onStateChange[_stateIndex = stateIndex].Invoke(arg0: true);
	}

	private void _OnStateAnimationComplete(int stateIndex)
	{
		onStateAnimationComplete[stateIndex].Invoke();
	}

	private void OnEnable()
	{
		_RegisterEvents();
	}

	private void OnDisable()
	{
		_UnregisterEvents();
	}
}
