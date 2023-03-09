using UnityEngine;

public class FlashingPrompt : AbstractMonoBehaviour
{
	private const float FLASH_TIME = 0.75f;

	private float flashTimer;

	[SerializeField]
	private GameObject child;

	[SerializeField]
	private CanvasGroup childGroup;

	protected virtual bool ShouldShow => true;

	private void Update()
	{
		if (ShouldShow)
		{
			flashTimer = (flashTimer + (float)CupheadTime.Delta) % 1.5f;
			if (child != null)
			{
				child.SetActive(flashTimer < 0.75f);
			}
			else
			{
				childGroup.alpha = ((!(flashTimer < 0.75f)) ? 0f : 1f);
			}
		}
		else
		{
			if (child != null)
			{
				child.SetActive(value: false);
			}
			else
			{
				childGroup.alpha = 0f;
			}
			flashTimer = 0f;
		}
	}
}
