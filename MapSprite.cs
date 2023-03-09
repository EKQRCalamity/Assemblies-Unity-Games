using UnityEngine;

public class MapSprite : AbstractPausableComponent
{
	[SerializeField]
	protected float zOffset;

	protected virtual bool ChangesDepth => true;

	protected override void Awake()
	{
		base.Awake();
		SetLayer(GetComponent<SpriteRenderer>());
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer layer in componentsInChildren)
		{
			SetLayer(layer);
		}
	}

	protected void SetLayer(SpriteRenderer renderer)
	{
		if (ChangesDepth && !(renderer == null))
		{
			renderer.sortingLayerName = "Map";
			renderer.sortingOrder = 0;
		}
	}

	protected virtual void Update()
	{
		Vector3 position = base.transform.position;
		base.transform.position = new Vector3(position.x, position.y, position.y + zOffset);
	}
}
