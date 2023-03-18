using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public class ScreenMaterialEffectsManager : MonoBehaviour
{
	public enum SCREEN_EFFECTS
	{
		NONE,
		BACKLIT
	}

	[SerializeField]
	[FoldoutGroup("References", false, 0)]
	private CustomImageEffect screenEffect;

	[SerializeField]
	[BoxGroup("Screen effects materials", true, false, 0)]
	private Material bwMat;

	[SerializeField]
	[BoxGroup("Screen effects materials", true, false, 0)]
	private Material backlitMat;

	[SerializeField]
	[BoxGroup("Screen effects materials", true, false, 0)]
	private Material thunderMat;

	private Material originMat;

	private Coroutine coroutine;

	private void Awake()
	{
		originMat = screenEffect.material;
	}

	private IEnumerator ScreenMaterialSwap(float duration, Material newMat)
	{
		Material oldMat = screenEffect.material;
		screenEffect.material = newMat;
		yield return new WaitForSeconds(duration);
		screenEffect.material = oldMat;
	}

	public void ScreenModeBW(float duration)
	{
		ResetIfNeeded();
		coroutine = StartCoroutine(ScreenMaterialSwap(duration, bwMat));
	}

	public void ScreenModeThunder(float duration)
	{
		ResetIfNeeded();
		coroutine = StartCoroutine(ScreenMaterialSwap(duration, thunderMat));
	}

	private void ResetIfNeeded()
	{
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
			coroutine = null;
			screenEffect.material = originMat;
		}
	}

	public void SetEffect(SCREEN_EFFECTS e)
	{
		switch (e)
		{
		case SCREEN_EFFECTS.NONE:
			SetOrigin();
			break;
		case SCREEN_EFFECTS.BACKLIT:
			SetBacklit();
			break;
		}
	}

	public void SetOrigin()
	{
		screenEffect.material = originMat;
	}

	public void SetBacklit()
	{
		screenEffect.material = backlitMat;
	}
}
