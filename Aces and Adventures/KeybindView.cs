using UnityEngine;

public class KeybindView : MonoBehaviour
{
	public KeybindInputView keybindInputBlueprint;

	public RectTransform keybindInputLayoutParent;

	private ProfileOptions.ControlOptions.KeyBind _keybind;

	public ReflectTreeData<UIFieldAttribute> reflectedData { private get; set; }

	public ProfileOptions.ControlOptions.KeyBind keybind
	{
		get
		{
			return _keybind;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _keybind, value) && (bool)_keybind)
			{
				_OnKeybindChange();
			}
		}
	}

	private void _OnKeybindChange()
	{
		keybindInputLayoutParent.gameObject.DestroyChildren(immediate: false);
		for (int j = 0; j < 2; j++)
		{
			int i = j;
			KeybindInputView component = Object.Instantiate(keybindInputBlueprint.gameObject, keybindInputLayoutParent).GetComponent<KeybindInputView>();
			component.keyCode = keybind.keyCodes[j];
			component.OnKeyCodeChange.AddListener(delegate(KeyCode? keyCode)
			{
				keybind.keyCodes[i] = keyCode;
				if (reflectedData != null)
				{
					reflectedData.OnValueChanged();
				}
			});
		}
	}
}
