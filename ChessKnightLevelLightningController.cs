using System.Collections;
using UnityEngine;

public class ChessKnightLevelLightningController : AbstractMonoBehaviour
{
	[SerializeField]
	private MinMax lightningDelayRange = new MinMax(3f, 8f);

	[SerializeField]
	private SpriteRenderer rend;

	[SerializeField]
	private Renderer glowTexture;

	[SerializeField]
	private float[] glowIntensity = new float[3] { 0.8f, 0.4f, 0.1f };

	private void Start()
	{
		StartCoroutine(lightning_cr());
	}

	private IEnumerator lightning_cr()
	{
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, lightningDelayRange.RandomFloat());
			base.animator.Play((!(Random.Range(0f, 3f) < 1f)) ? "Short" : "Long");
			base.animator.Update(0f);
			if (base.animator.GetCurrentAnimatorStateInfo(0).IsName("Long"))
			{
				SFX_KOG_Thunder();
			}
			yield return base.animator.WaitForAnimationToStart(this, "None");
		}
	}

	private void LateUpdate()
	{
		int num = rend.sprite.name[rend.sprite.name.Length - 1] - 49;
		if (num == 51)
		{
			num = 3;
		}
		glowTexture.enabled = num < 3;
		if (glowTexture.enabled)
		{
			glowTexture.material.SetColor("_OutlineColor", new Color(1f, 1f, 1f, glowIntensity[num]));
			glowTexture.material.SetFloat("_DimFactor", glowIntensity[num] * 0.6f);
		}
	}

	private void SFX_KOG_Thunder()
	{
		AudioManager.Play("sfx_dlc_kog_knight_castlethunder");
	}
}
