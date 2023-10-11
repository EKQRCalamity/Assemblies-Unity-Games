using UnityEngine;

public class ActiveLinker : MonoBehaviour
{
	[SerializeField]
	protected Transform _matchActiveStateOf;

	private Transform _registeredMatchState;

	public Transform matchActiveStateOf
	{
		get
		{
			return _matchActiveStateOf;
		}
		set
		{
			if (!(_matchActiveStateOf == value))
			{
				if ((bool)_matchActiveStateOf)
				{
					_UnregisterMatchTargetState(_matchActiveStateOf);
				}
				if ((bool)value)
				{
					_RegisterMatchTargetState(value);
				}
				_matchActiveStateOf = value;
			}
		}
	}

	private void _EnableHook(bool enabled)
	{
		base.gameObject.SetActive(enabled);
	}

	private void _DestroyHook()
	{
		Object.Destroy(base.gameObject);
	}

	private void _RegisterMatchTargetState(Transform targetState)
	{
		if (!(_registeredMatchState == targetState))
		{
			_registeredMatchState = targetState;
			targetState.gameObject.GetOrAddComponent<EnableHook>().AddOnEnableChangeListener(_EnableHook);
			targetState.gameObject.GetOrAddComponent<DestroyHook>().AddOnDestroyedListener(_DestroyHook);
			_EnableHook(targetState.gameObject.activeInHierarchy);
		}
	}

	private void _UnregisterMatchTargetState(Transform targetState)
	{
		if (!(_registeredMatchState != targetState))
		{
			_registeredMatchState = null;
			EnableHook component = targetState.gameObject.GetComponent<EnableHook>();
			if ((bool)component && component.OnEnableChange != null)
			{
				component.OnEnableChange.RemoveListener(_EnableHook);
			}
			DestroyHook component2 = targetState.gameObject.GetComponent<DestroyHook>();
			if ((bool)component2 && component2.OnDestroyed != null)
			{
				component2.OnDestroyed.RemoveListener(_DestroyHook);
			}
		}
	}

	private void OnEnable()
	{
		if ((bool)_matchActiveStateOf)
		{
			_EnableHook(_matchActiveStateOf.gameObject.activeInHierarchy);
		}
	}

	private void OnDestroy()
	{
		if ((bool)_matchActiveStateOf)
		{
			_UnregisterMatchTargetState(_matchActiveStateOf);
		}
	}
}
