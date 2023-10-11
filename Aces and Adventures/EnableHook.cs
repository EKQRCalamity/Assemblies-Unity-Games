using UnityEngine;
using UnityEngine.Events;

public class EnableHook : MonoBehaviour
{
	public UnityEvent OnEnabled;

	public UnityEvent OnDisabled;

	public BoolEvent OnEnableChange;

	private void OnEnable()
	{
		UnityEventExtensions.SafeInvoke(ref OnEnabled);
		UnityEventExtensions.SafeInvoke(ref OnEnableChange, value: true);
	}

	private void OnDisable()
	{
		if ((bool)this)
		{
			UnityEventExtensions.SafeInvoke(ref OnDisabled);
			UnityEventExtensions.SafeInvoke(ref OnEnableChange, value: false);
		}
	}

	public void Activate(bool shouldActivate)
	{
		if (shouldActivate)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	public void AddOnEnabledListener(UnityAction action)
	{
		UnityEventExtensions.AddListener(ref OnEnabled, action);
	}

	public void AddOnDisabledListener(UnityAction action)
	{
		UnityEventExtensions.AddListener(ref OnDisabled, action);
	}

	public void AddOnEnableChangeListener(UnityAction<bool> action)
	{
		UnityEventExtensions.AddListenerGeneric(ref OnEnableChange, action);
	}
}
