using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelPlayerColliderManager : AbstractLevelPlayerComponent
{
	public enum State
	{
		Default,
		Air,
		Ducking,
		Dashing,
		ChaliceFirstJump
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
		public ColliderProperties @default = new ColliderProperties(new Vector2(0f, 62f), new Vector2(50f, 105f));

		public ColliderProperties air = new ColliderProperties(new Vector2(0f, 50f), new Vector2(50f, 50f));

		public ColliderProperties ducking = new ColliderProperties(new Vector2(0f, 27f), new Vector2(50f, 35f));

		public ColliderProperties dashing;

		public ColliderProperties chaliceFirstJump = new ColliderProperties(new Vector2(0f, 78f), new Vector2(50f, 75f));
	}

	[SerializeField]
	private ColliderPropertiesGroup colliders;

	private Dictionary<int, ColliderProperties> pairs;

	private BoxCollider2D boxCollider;

	private State _state;

	public ColliderProperties @default => colliders.@default;

	public float DefaultWidth => colliders.@default.size.x;

	public float DefaultHeight => colliders.@default.size.y;

	public float Width => pairs[(int)_state].size.x;

	public float Height => pairs[(int)_state].size.y;

	public Vector2 Center => new Vector2(boxCollider.offset.x, boxCollider.offset.y * base.player.motor.GravityReversalMultiplier) + (Vector2)base.transform.position;

	public Vector2 DefaultCenter => new Vector2(colliders.@default.center.x, colliders.@default.center.y * base.player.motor.GravityReversalMultiplier) + (Vector2)base.transform.position;

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
				pairs[(int)value].SetCollider(boxCollider);
			}
			_state = value;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		boxCollider = GetComponent<BoxCollider2D>();
		pairs = new Dictionary<int, ColliderProperties>();
		pairs[0] = colliders.@default;
		pairs[1] = colliders.air;
		pairs[2] = colliders.ducking;
		pairs[3] = colliders.ducking;
		pairs[4] = colliders.chaliceFirstJump;
		state = State.Default;
	}

	private void FixedUpdate()
	{
		UpdateColliders();
	}

	private void UpdateColliders()
	{
		base.gameObject.layer = ((!base.player.CanTakeDamage) ? 9 : 8);
		if (base.player.motor.Dashing)
		{
			if (state != State.Dashing)
			{
				state = State.Dashing;
			}
		}
		else if (!base.player.motor.Grounded)
		{
			if (base.player.stats.isChalice)
			{
				if (!base.player.motor.ChaliceDoubleJumped)
				{
					if (state != State.ChaliceFirstJump)
					{
						state = State.ChaliceFirstJump;
					}
				}
				else if (state != State.Air)
				{
					state = State.Air;
				}
			}
			else if (state != State.Air)
			{
				state = State.Air;
			}
		}
		else if (base.player.motor.Ducking)
		{
			if (state != State.Ducking)
			{
				state = State.Ducking;
			}
		}
		else if (state != 0)
		{
			state = State.Default;
		}
	}
}
