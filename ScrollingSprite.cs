using System;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingSprite : AbstractPausableComponent
{
	public enum Axis
	{
		X,
		Y
	}

	public Axis axis;

	[SerializeField]
	protected bool negativeDirection;

	[SerializeField]
	private bool onLeft;

	[SerializeField]
	private bool isRotated;

	[Range(0f, 4000f)]
	public float speed;

	[SerializeField]
	public float offset;

	[SerializeField]
	[Range(1f, 10f)]
	private int count = 1;

	[NonSerialized]
	public float playbackSpeed = 1f;

	protected float size;

	protected Vector3 pos;

	private float startY;

	protected int direction;

	public List<SpriteRenderer> copyRenderers { get; private set; }

	public bool looping { get; set; }

	protected virtual void Start()
	{
		looping = true;
		copyRenderers = new List<SpriteRenderer>();
		direction = ((!negativeDirection) ? 1 : (-1));
		SpriteRenderer component = base.transform.GetComponent<SpriteRenderer>();
		copyRenderers.Add(component);
		size = ((axis != 0) ? component.sprite.bounds.size.y : component.sprite.bounds.size.x) - offset;
		for (int i = 0; i < count; i++)
		{
			GameObject gameObject = new GameObject(base.gameObject.name + " Copy");
			gameObject.transform.parent = base.transform;
			gameObject.transform.ResetLocalTransforms();
			SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
			spriteRenderer.sortingLayerID = component.sortingLayerID;
			spriteRenderer.sortingOrder = component.sortingOrder;
			spriteRenderer.sprite = component.sprite;
			spriteRenderer.material = component.material;
			copyRenderers.Add(spriteRenderer);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
			gameObject2.transform.parent = base.transform;
			gameObject2.transform.ResetLocalTransforms();
			copyRenderers.Add(gameObject2.GetComponent<SpriteRenderer>());
			if (axis == Axis.X)
			{
				gameObject.transform.SetLocalPosition((float)direction * (size + size * (float)i), 0f, 0f);
				gameObject2.transform.SetLocalPosition((float)direction * (0f - (size + size * (float)i)), 0f, 0f);
			}
			else
			{
				gameObject.transform.SetLocalPosition(0f, size + size * (float)i, 0f);
				gameObject2.transform.SetLocalPosition(0f, 0f - (size + size * (float)i), 0f);
			}
		}
		startY = base.transform.localPosition.y;
	}

	protected virtual void Update()
	{
		pos = base.transform.localPosition;
		if (axis == Axis.X)
		{
			if (pos.x <= 0f - size && looping)
			{
				pos.x += size;
				if (isRotated)
				{
					pos.y = startY;
				}
				onLoop();
			}
			if (pos.x >= size && looping)
			{
				pos.x -= size;
				onLoop();
			}
			if (!isRotated)
			{
				pos.x -= (float)((!negativeDirection) ? 1 : (-1)) * speed * (float)CupheadTime.Delta * playbackSpeed;
			}
		}
		else
		{
			if (pos.y <= 0f - size && looping)
			{
				pos.y += size;
				onLoop();
			}
			if (pos.y >= size && looping)
			{
				pos.y -= size;
				onLoop();
			}
			if (!isRotated)
			{
				pos.y -= (float)((!negativeDirection) ? 1 : (-1)) * speed * (float)CupheadTime.Delta * playbackSpeed;
			}
		}
		if (isRotated)
		{
			pos -= base.transform.right * speed * CupheadTime.Delta;
		}
		base.transform.localPosition = pos;
	}

	protected virtual void onLoop()
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		copyRenderers = null;
	}
}
