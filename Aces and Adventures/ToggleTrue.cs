using UnityEngine;
using UnityEngine.Events;

public class ToggleTrue : MonoBehaviour
{
	[SerializeField]
	protected UnityEvent _OnTrue;

	private bool? _toggle;

	public UnityEvent OnTrue => _OnTrue ?? (_OnTrue = new UnityEvent());

	public bool toggle
	{
		get
		{
			return _toggle.GetValueOrDefault();
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _toggle, value))
			{
				_OnToggleChange();
			}
		}
	}

	private void _OnToggleChange()
	{
		if (toggle)
		{
			OnTrue.Invoke();
		}
	}
}
