using UnityEngine;

public class StringHook : MonoBehaviour
{
	public BoolEvent onHasVisibleCharacterChange;

	public void SetText(string text)
	{
		onHasVisibleCharacterChange?.Invoke(text.HasVisibleCharacter());
	}
}
