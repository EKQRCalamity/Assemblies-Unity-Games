using UnityEngine;

public class SpriteOrderChanger : AbstractMonoBehaviour
{
	public int change = 1;

	public int frameDelay = 2;

	private SpriteRenderer spriteRenderer;

	private int t;

	protected override void Awake()
	{
		base.Awake();
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		if (t >= frameDelay)
		{
			t = 0;
			spriteRenderer.sortingOrder += change;
		}
		t++;
	}
}
