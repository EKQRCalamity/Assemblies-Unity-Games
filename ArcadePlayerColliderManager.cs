using System;
using System.Collections.Generic;
using UnityEngine;

public class ArcadePlayerColliderManager : AbstractArcadePlayerComponent
{
	public enum State
	{
		Default,
		Air,
		Dashing,
		Rocket
	}

	[Serializable]
	public class ColliderProperties
	{
		public Vector2 center;

		public Vector2 size;

		public ColliderProperties(Vector2 center, Vector2 size)
		{
			this.center = center;
			this.size = size;
		}

		public BoxCollider2D CreateCollider(GameObject gameObject)
		{
			BoxCollider2D boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
			boxCollider2D.offset = center;
			boxCollider2D.size = size;
			boxCollider2D.isTrigger = true;
			return boxCollider2D;
		}

		public void SetCollider(BoxCollider2D boxCollider)
		{
			boxCollider.offset = center;
			boxCollider.size = size;
			boxCollider.isTrigger = true;
		}
	}

	[Serializable]
	public class ColliderPropertiesGroup
	{
		public ColliderProperties @default = new ColliderProperties(new Vector2(0f, 40f), new Vector2(33f, 70f));

		public ColliderProperties air = new ColliderProperties(new Vector2(0f, 33f), new Vector2(33f, 33f));

		public ColliderProperties dashing = new ColliderProperties(new Vector2(0f, 18f), new Vector2(33f, 23f));

		public ColliderProperties rocket = new ColliderProperties(new Vector2(3.2f, 4f), new Vector2(4f, 66f));
	}

	[SerializeField]
	private ColliderPropertiesGroup colliders;

	private Dictionary<State, ColliderProperties> pairs;

	private BoxCollider2D boxCollider;

	private State _state;

	public ColliderProperties @default => colliders.@default;

	public float DefaultWidth => colliders.@default.size.x;

	public float DefaultHeight => colliders.@default.size.y;

	public float Width => pairs[_state].size.x;

	public float Height => pairs[_state].size.y;

	public Vector2 Center => boxCollider.offset + (Vector2)base.transform.position;

	public State state
	{
		get
		{
			return _state;
		}
		set
		{
			if (_state != value)
			{
				pairs[value].SetCollider(boxCollider);
			}
			_state = value;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		boxCollider = GetComponent<BoxCollider2D>();
		pairs = new Dictionary<State, ColliderProperties>();
		pairs[State.Default] = colliders.@default;
		pairs[State.Air] = colliders.air;
		pairs[State.Dashing] = colliders.dashing;
		pairs[State.Rocket] = colliders.rocket;
		state = State.Default;
	}

	private void Update()
	{
		UpdateColliders();
	}

	private void UpdateColliders()
	{
		boxCollider.enabled = base.player.CanTakeDamage;
		if (base.player.controlScheme == ArcadePlayerController.ControlScheme.Rocket)
		{
			if (state != State.Rocket)
			{
				state = State.Rocket;
			}
		}
		else if (base.player.motor.Dashing)
		{
			if (state != State.Dashing)
			{
				state = State.Dashing;
			}
		}
		else if (!base.player.motor.Grounded)
		{
			if (state != State.Air)
			{
				state = State.Air;
			}
		}
		else if (state != 0)
		{
			state = State.Default;
		}
	}
}
