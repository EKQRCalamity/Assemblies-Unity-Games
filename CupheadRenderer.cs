using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CupheadRenderer : AbstractMonoBehaviour
{
	public enum RenderLayer
	{
		None,
		Game,
		UI,
		Loader
	}

	public static CupheadRenderer Instance;

	[SerializeField]
	private CupheadRendererCamera cameraPrefab;

	private CupheadRendererCamera rendererCamera;

	private Camera bgCamera;

	private Canvas canvas;

	private Dictionary<RenderLayer, RectTransform> rendererParents;

	private Image background;

	private Image fader;

	public bool fuzzyEffectPlaying;

	protected override void Awake()
	{
		base.Awake();
		if (Instance == null)
		{
			Instance = this;
			Setup();
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void Setup()
	{
		rendererCamera = Object.Instantiate(cameraPrefab);
		rendererCamera.transform.SetParent(base.transform);
		rendererCamera.transform.ResetLocalTransforms();
	}

	public void TouchFuzzy(float amount, float speed, float time)
	{
		rendererCamera.GetComponent<ChromaticAberrationFilmGrain>().PsychedelicEffect(amount, speed, time);
		StartCoroutine(change_blur_cr(time));
	}

	private IEnumerator change_blur_cr(float time)
	{
		float t = 0f;
		float incrementTime = 1f;
		float blurStart = rendererCamera.GetComponent<BlurGamma>().blurSize;
		rendererCamera.GetComponent<BlurGamma>().blurSize += incrementTime;
		while (rendererCamera.GetComponent<BlurGamma>().blurSize > blurStart)
		{
			t += Time.deltaTime;
			if (t >= time / 2f)
			{
				rendererCamera.GetComponent<BlurGamma>().blurSize -= incrementTime * Time.deltaTime;
			}
			else
			{
				rendererCamera.GetComponent<BlurGamma>().blurSize += incrementTime * Time.deltaTime;
			}
			yield return null;
		}
		rendererCamera.GetComponent<BlurGamma>().blurSize = blurStart;
	}
}
