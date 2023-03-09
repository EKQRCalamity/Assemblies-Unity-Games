using System.Collections;
using UnityEngine;

public class RetroArcadeSheriff : RetroArcadeEnemy
{
	public enum Side
	{
		Top,
		Bottom,
		Left,
		Right
	}

	public float speed;

	[SerializeField]
	private BasicProjectile projectile;

	private const float Y_TOP_THRESHOLD = 200f;

	private const float Y_BOTTOM_THRESHOLD = -100f;

	private const float X_THRESHOLD = 290f;

	public Side side;

	private LevelProperties.RetroArcade.Sheriff properties;

	private float offset;

	private bool clockwise;

	private Vector3 direction;

	private Vector2 targetPos;

	public RetroArcadeSheriff Create(Vector3 pos, float speed, bool clockwise, float offset, LevelProperties.RetroArcade.Sheriff properties)
	{
		RetroArcadeSheriff retroArcadeSheriff = InstantiatePrefab<RetroArcadeSheriff>();
		retroArcadeSheriff.transform.position = pos;
		retroArcadeSheriff.properties = properties;
		retroArcadeSheriff.speed = speed;
		retroArcadeSheriff.clockwise = clockwise;
		retroArcadeSheriff.offset = offset;
		return retroArcadeSheriff;
	}

	protected override void Start()
	{
		side = Side.Right;
		SelectDirection();
	}

	private void SelectDirection()
	{
		if (base.transform.position.y > 200f)
		{
			side = Side.Top;
			direction = ((!clockwise) ? Vector3.left : Vector3.right);
		}
		else if (base.transform.position.y < -100f)
		{
			side = Side.Bottom;
			direction = ((!clockwise) ? Vector3.right : Vector3.left);
		}
		else if (base.transform.position.x < 0f)
		{
			side = Side.Left;
			direction = ((!clockwise) ? Vector3.down : Vector3.up);
		}
		else
		{
			side = Side.Right;
			direction = ((!clockwise) ? Vector3.up : Vector3.down);
		}
	}

	public void StartMoving()
	{
		StartCoroutine(move_cr());
	}

	private IEnumerator move_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		while (true)
		{
			switch (side)
			{
			case Side.Top:
				if (clockwise && base.transform.position.x >= (float)Level.Current.Right - offset)
				{
					side = Side.Right;
					direction = Vector3.down;
				}
				else if (!clockwise && base.transform.position.x <= (float)Level.Current.Left + offset)
				{
					side = Side.Left;
					direction = Vector3.down;
				}
				break;
			case Side.Bottom:
				if (clockwise && base.transform.position.x <= (float)Level.Current.Left + offset)
				{
					side = Side.Left;
					direction = Vector3.up;
				}
				else if (!clockwise && base.transform.position.x >= (float)Level.Current.Right - offset)
				{
					side = Side.Right;
					direction = Vector3.up;
				}
				break;
			case Side.Left:
				if (clockwise && base.transform.position.y >= (float)Level.Current.Ceiling - offset)
				{
					side = Side.Top;
					direction = Vector3.right;
				}
				else if (!clockwise && base.transform.position.y <= (float)Level.Current.Ground + offset)
				{
					side = Side.Bottom;
					direction = Vector3.right;
				}
				break;
			case Side.Right:
				if (!clockwise && base.transform.position.y >= (float)Level.Current.Ceiling - offset)
				{
					side = Side.Top;
					direction = Vector3.left;
				}
				else if (clockwise && base.transform.position.y <= (float)Level.Current.Ground + offset)
				{
					side = Side.Bottom;
					direction = Vector3.left;
				}
				break;
			}
			Vector3 pos = base.transform.position;
			pos.x = Mathf.Clamp(base.transform.position.x, (float)Level.Current.Left + offset, (float)Level.Current.Right - offset);
			pos.y = Mathf.Clamp(base.transform.position.y, (float)Level.Current.Ground + offset, (float)Level.Current.Ceiling - offset);
			base.transform.position = pos;
			base.transform.position += direction * speed * CupheadTime.FixedDelta;
			yield return wait;
		}
	}

	public void Shoot(AbstractPlayerController player)
	{
		Vector3 vector = player.transform.position - base.transform.position;
		projectile.Create(base.transform.position, MathUtils.DirectionToAngle(vector), properties.shotSpeed);
	}
}
