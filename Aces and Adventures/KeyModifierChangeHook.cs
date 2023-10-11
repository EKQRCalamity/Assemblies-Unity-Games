using UnityEngine;

public class KeyModifierChangeHook : MonoBehaviour
{
	[EnumFlags]
	public KeyModifiers keyModifiers;

	public bool allowAdditionalModifiersBeingHeld;

	[SerializeField]
	protected BoolEvent _OnKeyModifiersActiveChange;

	private bool? _wasActive;

	public BoolEvent OnKeyModifiersActiveChange => _OnKeyModifiersActiveChange ?? (_OnKeyModifiersActiveChange = new BoolEvent());

	private void OnDisable()
	{
		_wasActive = null;
	}

	private void LateUpdate()
	{
		bool flag = (allowAdditionalModifiersBeingHeld ? InputManager.I[keyModifiers] : (InputManager.I.keyModifiers == keyModifiers));
		if (flag != _wasActive)
		{
			OnKeyModifiersActiveChange.Invoke(flag);
			_wasActive = flag;
		}
	}
}
