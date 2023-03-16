using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;

public class GlowText : MonoBehaviour
{
	[SerializeField]
	private RenderTexture renderTextureGlow;

	[SerializeField]
	private GameObject cameraGlow;

	[SerializeField]
	private RawImage rawImageGlow;

	[SerializeField]
	private TextMeshProUGUI[] tmpTextsToGlow;

	[SerializeField]
	private Image[] imagesToGlow;

	public void InitTMPText(params MaskableGraphic[] tmp_texts)
	{
		if (tmp_texts.Length > tmpTextsToGlow.Length)
		{
			return;
		}
		for (int i = 0; i < tmp_texts.Length; i++)
		{
			if (tmp_texts[i] is Text)
			{
				Text text = tmp_texts[i] as Text;
				tmpTextsToGlow[i].enabled = true;
				tmpTextsToGlow[i].text = text.text;
				tmpTextsToGlow[i].fontSize = text.fontSize;
			}
			else if (tmp_texts[i] is TextMeshProUGUI)
			{
				TextMeshProUGUI textMeshProUGUI = tmp_texts[i] as TextMeshProUGUI;
				tmp_texts[i] = tmp_texts[i] as Text;
				tmpTextsToGlow[i].enabled = true;
				tmpTextsToGlow[i].text = textMeshProUGUI.text;
				tmpTextsToGlow[i].fontSize = textMeshProUGUI.fontSize;
				tmpTextsToGlow[i].font = textMeshProUGUI.font;
				tmpTextsToGlow[i].outlineWidth = textMeshProUGUI.outlineWidth;
			}
		}
	}

	public void DisableTMPText()
	{
		for (int i = 0; i < tmpTextsToGlow.Length; i++)
		{
			tmpTextsToGlow[i].enabled = false;
		}
	}

	public void InitImages(params Image[] images)
	{
		if (images.Length <= imagesToGlow.Length)
		{
			for (int i = 0; i < images.Length; i++)
			{
				imagesToGlow[i].enabled = true;
				imagesToGlow[i].sprite = images[i].sprite;
				imagesToGlow[i].color = images[i].color;
			}
		}
	}

	public void DisableImages()
	{
		for (int i = 0; i < imagesToGlow.Length; i++)
		{
			imagesToGlow[i].enabled = false;
		}
	}

	public void BeginGlow()
	{
		rawImageGlow.enabled = true;
		StartCoroutine(Glow_cr());
	}

	public void StopGlow()
	{
		rawImageGlow.enabled = false;
	}

	private IEnumerator Glow_cr()
	{
		RenderTexture rt = RenderTexture.active;
		RenderTexture.active = renderTextureGlow;
		GL.Clear(clearDepth: true, clearColor: true, Color.clear);
		RenderTexture.active = rt;
		cameraGlow.GetComponent<PostProcessingBehaviour>().enabled = true;
		yield return null;
		cameraGlow.GetComponent<PostProcessingBehaviour>().enabled = false;
	}
}
