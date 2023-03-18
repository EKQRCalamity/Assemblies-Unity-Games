using System.Collections.Generic;
using UnityEngine;

public class ReplicateSprite : MonoBehaviour
{
	public SpriteRenderer spriteRenderer;

	public List<SpriteRenderer> childRenderers;

	private void LateUpdate()
	{
		foreach (SpriteRenderer childRenderer in childRenderers)
		{
			childRenderer.sprite = spriteRenderer.sprite;
		}
	}
}
