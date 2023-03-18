using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.UIGameLogic;

public class InteractionSignal : MonoBehaviour
{
	public Text label;

	public Image yIcon;

	public Image downIcon;

	public float yOffset = 5f;

	private void Awake()
	{
	}

	private void Update()
	{
		Penitent penitent = Core.Logic.Penitent;
		if (penitent != null)
		{
			Vector3 position = Camera.main.WorldToScreenPoint(penitent.transform.position);
			position += Vector3.up * yOffset;
			base.gameObject.transform.position = position;
		}
	}

	public void SetText(string text)
	{
		label.text = text;
	}

	public void Show(PadButton btn)
	{
		switch (btn)
		{
		case PadButton.Y:
			yIcon.enabled = true;
			downIcon.enabled = false;
			break;
		case PadButton.Down:
			yIcon.enabled = false;
			downIcon.enabled = true;
			break;
		}
		GetComponent<CanvasGroup>().alpha = 1f;
	}

	public void Hide()
	{
		GetComponent<CanvasGroup>().alpha = 0f;
	}
}
