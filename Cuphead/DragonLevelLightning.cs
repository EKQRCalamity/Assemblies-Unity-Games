using UnityEngine;

public class DragonLevelLightning : AbstractPausableComponent
{
	private readonly int[] layerOrder = new int[3] { 91, 93, 95 };

	[SerializeField]
	private SpriteRenderer spriteRenderer;

	public void PlayLightning()
	{
		int value = Random.Range(1, 11);
		base.animator.SetInteger("LightningID", value);
		base.animator.SetTrigger("Continue");
		value = Random.Range(0, layerOrder.Length);
		spriteRenderer.sortingOrder = layerOrder[value];
		AudioManager.Play("level_dragon_amb_thunder");
	}
}
