using UnityEngine;

public class PlayerScreenEffectController : AbstractMonoBehaviour
{
	[SerializeField]
	private bool dontCenter;

	[SerializeField]
	private SpriteRenderer[] spriteRenderers;

	private void Update()
	{
		if (!dontCenter)
		{
			UpdateToCamera();
		}
	}

	private void LateUpdate()
	{
		if (!dontCenter)
		{
			UpdateToCamera();
		}
	}

	private void UpdateToCamera()
	{
		Camera main = Camera.main;
		Transform transform = main.transform;
		base.transform.position = transform.position;
		base.transform.localScale = Vector3.one * (main.orthographicSize / 360f);
		base.transform.rotation = transform.rotation;
	}

	public void SetSpriteLayer(int index, SpriteLayer layer)
	{
		spriteRenderers[index].sortingLayerName = layer.ToString();
	}

	public void SetSpriteOrder(int index, int order)
	{
		spriteRenderers[index].sortingOrder = order;
	}

	public void ResetSprites()
	{
		for (int i = 0; i < spriteRenderers.Length; i++)
		{
			spriteRenderers[i].sortingOrder = -2010 - i;
			spriteRenderers[i].sortingLayerName = "Player";
			spriteRenderers[i].sprite = null;
		}
	}
}
