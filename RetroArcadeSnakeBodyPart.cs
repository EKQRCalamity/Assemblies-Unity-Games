using System.Collections;
using UnityEngine;

public class RetroArcadeSnakeBodyPart : AbstractCollidableObject
{
	public enum Direction
	{
		Left,
		Right,
		Up,
		Down
	}

	public Vector3 turnPos;

	public Vector3 dir;

	private const float TOP_Y = 230f;

	private const float BOTTOM_Y = -120f;

	private const float OFFSCREEN_Y = 300f;

	private const float SIDE_X = 330f;

	private const float MIN_DISTANCE = 20f;

	private const float SPACING = 60f;

	private RetroArcadeSnakeBodyPart partInFront;

	private RetroArcadeSnakeBodyPart partBehind;

	private RetroArcadeSnakeManager manager;

	private float speed;

	private bool canTurn;

	private bool isHead;

	private bool isDead;

	public Direction currentDirection { get; private set; }

	public RetroArcadeSnakeBodyPart Create(Vector2 pos, bool isHead, Direction direction, RetroArcadeSnakeManager manager, RetroArcadeSnakeBodyPart previousPart, float speed)
	{
		RetroArcadeSnakeBodyPart retroArcadeSnakeBodyPart = InstantiatePrefab<RetroArcadeSnakeBodyPart>();
		retroArcadeSnakeBodyPart.transform.position = pos;
		retroArcadeSnakeBodyPart.currentDirection = direction;
		retroArcadeSnakeBodyPart.partInFront = previousPart;
		retroArcadeSnakeBodyPart.speed = speed;
		retroArcadeSnakeBodyPart.isHead = isHead;
		retroArcadeSnakeBodyPart.manager = manager;
		return retroArcadeSnakeBodyPart;
	}

	private void Start()
	{
		canTurn = true;
		ChangeDirection(currentDirection, checkTurn: false);
		if (isHead)
		{
			StartCoroutine(head_check_cr());
		}
		else
		{
			StartCoroutine(body_check_cr());
		}
	}

	private void FixedUpdate()
	{
		if (!isDead)
		{
			base.transform.position += dir * speed * CupheadTime.FixedDelta;
		}
	}

	private IEnumerator body_check_cr()
	{
		while (true)
		{
			if (currentDirection != partInFront.currentDirection)
			{
				if (currentDirection == Direction.Right)
				{
					if (base.transform.position.x >= partInFront.turnPos.x)
					{
						ClampDirectionChange();
					}
				}
				else if (currentDirection == Direction.Left)
				{
					if (base.transform.position.x <= partInFront.turnPos.x)
					{
						ClampDirectionChange();
					}
				}
				else if (currentDirection == Direction.Up)
				{
					if (base.transform.position.y >= partInFront.turnPos.y)
					{
						ClampDirectionChange();
					}
				}
				else if (currentDirection == Direction.Down && base.transform.position.y <= partInFront.turnPos.y)
				{
					ClampDirectionChange();
				}
			}
			yield return null;
		}
	}

	private void ClampDirectionChange()
	{
		base.transform.position = partInFront.transform.position + -partInFront.dir * 60f;
		ChangeDirection(partInFront.currentDirection, checkTurn: false);
	}

	private IEnumerator head_check_cr()
	{
		AbstractPlayerController player1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		AbstractPlayerController player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		while (true)
		{
			if (currentDirection == Direction.Up || currentDirection == Direction.Down)
			{
				if (base.transform.position.y >= 230f || base.transform.position.y <= -120f)
				{
					SwitchToHorizontal();
				}
				else
				{
					if (player1 != null && !player1.IsDead)
					{
						CheckPlayerY(player1);
					}
					if (player2 != null && !player2.IsDead)
					{
						CheckPlayerY(player2);
					}
				}
			}
			else if (base.transform.position.x >= 330f || base.transform.position.x <= -330f)
			{
				SwitchToVertical();
			}
			else
			{
				if (player1 != null && !player1.IsDead)
				{
					CheckPlayerX(player1);
				}
				if (player2 != null && !player2.IsDead)
				{
					CheckPlayerX(player2);
				}
			}
			yield return null;
		}
	}

	private void CheckPlayerX(AbstractPlayerController player)
	{
		float f = player.transform.position.x - base.transform.position.x;
		if (!(Mathf.Abs(f) < 20f))
		{
			return;
		}
		if (player.transform.position.y < base.transform.position.y)
		{
			if (!(base.transform.position.y <= -100f))
			{
				ChangeDirection(Direction.Down, checkTurn: true);
			}
		}
		else if (!(base.transform.position.y >= 210f))
		{
			ChangeDirection(Direction.Up, checkTurn: true);
		}
	}

	private void CheckPlayerY(AbstractPlayerController player)
	{
		float f = player.transform.position.y - base.transform.position.y;
		if (!(Mathf.Abs(f) < 20f))
		{
			return;
		}
		if (player.transform.position.x < base.transform.position.x)
		{
			if (!(player.transform.position.x <= -310f))
			{
				ChangeDirection(Direction.Left, checkTurn: true);
			}
		}
		else if (!(player.transform.position.x >= 310f))
		{
			ChangeDirection(Direction.Right, checkTurn: true);
		}
	}

	private void SwitchToHorizontal()
	{
		if (base.transform.position.x < 0f)
		{
			ChangeDirection(Direction.Right, checkTurn: true);
		}
		else
		{
			ChangeDirection(Direction.Left, checkTurn: true);
		}
	}

	private void SwitchToVertical()
	{
		if (base.transform.position.y < 0f)
		{
			ChangeDirection(Direction.Up, checkTurn: true);
		}
		else
		{
			ChangeDirection(Direction.Down, checkTurn: true);
		}
	}

	private void ChangeDirection(Direction direction, bool checkTurn)
	{
		if (!checkTurn || canTurn)
		{
			currentDirection = direction;
			turnPos = base.transform.position;
			switch (currentDirection)
			{
			case Direction.Down:
				dir = Vector3.down;
				break;
			case Direction.Left:
				dir = Vector3.left;
				break;
			case Direction.Right:
				dir = Vector3.right;
				break;
			case Direction.Up:
				dir = Vector3.up;
				break;
			}
			StartCoroutine(turn_timer_cr());
		}
	}

	private IEnumerator turn_timer_cr()
	{
		float t = 0f;
		float time = 0.5f;
		canTurn = false;
		while (t < time)
		{
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		canTurn = true;
		yield return null;
	}

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionEnemy(hit, phase);
		if (isHead && !isDead && hit.GetComponent<RetroArcadeSnakeBodyPart>() != partBehind)
		{
			manager.EndPhase();
			isDead = true;
		}
	}

	public void GetPartBehind(RetroArcadeSnakeBodyPart behind)
	{
		partBehind = behind;
	}

	public void Die()
	{
		StopAllCoroutines();
		isDead = true;
	}
}
