using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealerCharmSparkEffect : Effect
{
	[SerializeField]
	private Material flashMaterial;

	[SerializeField]
	private HealerCharmParticleEffect particle;

	private List<Vector2> particleVectors;

	private int startedFlash;

	private AbstractPlayerController target;

	public void Create(Vector3 position, Vector3 scale, AbstractPlayerController target)
	{
		startedFlash = 0;
		HealerCharmSparkEffect healerCharmSparkEffect = base.Create(position, scale) as HealerCharmSparkEffect;
		healerCharmSparkEffect.target = target;
		particleVectors = new List<Vector2>
		{
			new Vector2(-0.26f, -0.93f),
			new Vector2(0.3f, -0.72f),
			new Vector2(0.77f, -0.28f),
			new Vector2(0.98f, 0.23f),
			new Vector2(0.65f, 0.74f),
			new Vector2(0.02f, 0.6f),
			new Vector2(-0.4f, 0.93f),
			new Vector2(-0.91f, 0.63f),
			new Vector2(-1f, 0.07f),
			new Vector2(-0.67f, -0.47f)
		};
		for (int i = 0; i < 5; i++)
		{
			int index = Random.Range(0, particleVectors.Count);
			Object.Instantiate(particle, position, Quaternion.identity).SetVars(new Vector2(particleVectors[index].x * base.transform.localScale.x, particleVectors[index].y), target, healerCharmSparkEffect);
			particleVectors.RemoveAt(index);
		}
	}

	public void StartPlayerFlash()
	{
		if (startedFlash >= 0)
		{
			startedFlash++;
			if (startedFlash > 4)
			{
				StartCoroutine(player_flash_cr());
				startedFlash = -1;
			}
		}
	}

	private IEnumerator player_flash_cr()
	{
		WaitForFrameTimePersistent wait = new WaitForFrameTimePersistent(1f / 24f);
		if (!target.stats.SuperInvincible)
		{
			LevelPlayerController levelPlayer = target as LevelPlayerController;
			PlanePlayerController planePlayer = target as PlanePlayerController;
			Material matInstance = null;
			SpriteRenderer playerRend = null;
			if (levelPlayer != null)
			{
				levelPlayer.animationController.SetMaterial(flashMaterial);
				matInstance = levelPlayer.animationController.GetMaterial();
				playerRend = levelPlayer.animationController.GetSpriteRenderer();
			}
			else if (planePlayer != null)
			{
				planePlayer.animationController.SetMaterial(flashMaterial);
				matInstance = planePlayer.animationController.GetMaterial();
				playerRend = planePlayer.animationController.GetSpriteRenderer();
			}
			Color lightColor = new Color(1f, 23f / 51f, 67f / 85f);
			Color darkColor = new Color(1f, 0.21960784f, 0.7019608f);
			matInstance.SetFloat("_Amount", 1f);
			playerRend.color = lightColor;
			yield return wait;
			playerRend.color = darkColor;
			yield return wait;
			playerRend.color = Color.white;
			yield return wait;
			matInstance.SetFloat("_Amount", 0f);
			yield return wait;
			matInstance.SetFloat("_Amount", 1f);
			playerRend.color = darkColor;
			yield return wait;
			playerRend.color = Color.white;
			yield return wait;
			playerRend.color = lightColor;
			yield return wait;
			playerRend.color = Color.white;
			if (levelPlayer != null)
			{
				levelPlayer.animationController.SetOldMaterial();
			}
			else if (planePlayer != null)
			{
				planePlayer.animationController.SetOldMaterial();
			}
		}
		Object.Destroy(base.gameObject);
	}
}
