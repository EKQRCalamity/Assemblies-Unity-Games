using UnityEngine;

public class ReadonlyToggle : MonoBehaviour
{
	private CanvasGroup _canvasGroup;

	private CanvasGroup canvasGroup => this.CacheComponentSafe(ref _canvasGroup);

	private void OnEnable()
	{
		canvasGroup.blocksRaycasts = false;
		canvasGroup.alpha = 0.666f;
	}

	private void OnDisable()
	{
		if ((bool)this)
		{
			canvasGroup.blocksRaycasts = true;
			canvasGroup.alpha = 1f;
		}
	}
}
