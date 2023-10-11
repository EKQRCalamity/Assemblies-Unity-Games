using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonLink : MonoBehaviour
{
	public Button buttonToLinkTo;

	private void Start()
	{
		if (!(buttonToLinkTo != null))
		{
			return;
		}
		GetComponent<Button>().onClick.AddListener(delegate
		{
			if (base.isActiveAndEnabled)
			{
				buttonToLinkTo.onClick.Invoke();
			}
		});
	}
}
