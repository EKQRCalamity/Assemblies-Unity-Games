using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonHook : MonoBehaviour
{
	private Button _button;

	public Button button => _button ?? (_button = GetComponent<Button>());

	public void Click()
	{
		button.onClick.Invoke();
	}

	public void SetNormalAlpha(float alpha)
	{
		Button s = button;
		Color? normal = button.colors.normalColor.SetAlpha(alpha);
		s.SetStateColors(null, null, normal);
	}
}
