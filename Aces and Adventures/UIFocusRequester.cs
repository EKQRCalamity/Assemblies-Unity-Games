using UnityEngine;
using UnityEngine.EventSystems;

public class UIFocusRequester : MonoBehaviour
{
	public bool requestFocusOnEnable = true;

	private void OnEnable()
	{
		if (requestFocusOnEnable)
		{
			RequestFocus();
		}
	}

	public void RequestFocus()
	{
		if ((bool)EventSystem.current)
		{
			EventSystem.current.SetSelectedGameObject(base.gameObject);
		}
	}
}
