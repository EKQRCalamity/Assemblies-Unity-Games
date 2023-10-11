using UnityEngine;

public class ToggleInverter : MonoBehaviour
{
	[SerializeField]
	protected BoolEvent _OnInvertedToggle;

	private bool? _toggle;

	public BoolEvent OnInvertedToggle => _OnInvertedToggle ?? (_OnInvertedToggle = new BoolEvent());

	public bool toggle
	{
		get
		{
			return _toggle.GetValueOrDefault();
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _toggle, !value))
			{
				OnInvertedToggle.Invoke(_toggle.Value);
			}
		}
	}
}
