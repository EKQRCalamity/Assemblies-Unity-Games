using UnityEngine;

public class ToggleFloatGroup : MonoBehaviour
{
	public bool includeInActive;

	[SerializeField]
	protected BoolEvent _OnToggle;

	private bool? _toggle;

	public BoolEvent OnToggle => _OnToggle ?? (_OnToggle = new BoolEvent());

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
		OnToggle.Invoke(toggle);
		foreach (ToggleFloat item in base.gameObject.GetComponentsInChildrenPooled<ToggleFloat>(includeInActive))
		{
			item.toggle = toggle;
		}
	}
}
