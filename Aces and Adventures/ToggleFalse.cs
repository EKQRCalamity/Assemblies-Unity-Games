using UnityEngine;
using UnityEngine.Events;

public class ToggleFalse : MonoBehaviour
{
	[SerializeField]
	protected UnityEvent _OnFalse;

	private bool? _toggle;

	public UnityEvent OnFalse => _OnFalse ?? (_OnFalse = new UnityEvent());

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
		if (!toggle)
		{
			OnFalse.Invoke();
		}
	}
}
