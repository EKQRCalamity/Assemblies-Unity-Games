using UnityEngine;

public class SaltbakerLevelGlassChunk : AbstractMonoBehaviour
{
	private float fallSpeed;

	[SerializeField]
	private SpriteRenderer[] rend;

	public void Reset(Vector3 pos, float fallSpeed, bool isChunk, bool flip, bool reverse, bool inBack, int variant)
	{
		base.transform.position = pos;
		this.fallSpeed = fallSpeed;
		base.animator.SetFloat("Reverse", (!reverse || isChunk) ? 1 : (-1));
		base.animator.Play(((!isChunk) ? "Bit" : "Chunk") + variant, 0, Random.Range(0, 1));
		base.transform.eulerAngles = new Vector3(0f, 0f, (!isChunk) ? Random.Range(-30, 30) : 0);
		base.transform.localScale = new Vector3((!flip) ? 1 : (-1), 1f);
		SpriteRenderer[] array = rend;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			spriteRenderer.sortingLayerName = ((!inBack) ? "Foreground" : "Background");
			spriteRenderer.color = ((!inBack) ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f));
		}
	}

	private void Update()
	{
		base.transform.position += Vector3.down * fallSpeed * CupheadTime.Delta;
	}
}
