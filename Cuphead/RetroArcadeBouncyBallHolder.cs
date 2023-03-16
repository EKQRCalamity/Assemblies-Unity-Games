using System.Collections;
using UnityEngine;

public class RetroArcadeBouncyBallHolder : RetroArcadeEnemy
{
	[SerializeField]
	private Transform[] ballPositions;

	[SerializeField]
	private RetroArcadeBouncyBall typeABall;

	[SerializeField]
	private RetroArcadeBouncyBall typeBBall;

	[SerializeField]
	private RetroArcadeBouncyBall typeCBall;

	private RetroArcadeBouncyBall[] ballsHeld;

	private float currentAngle;

	private string[] ballTypes;

	private Vector3 velocity;

	private LevelProperties.RetroArcade.Bouncy properties;

	private RetroArcadeBouncyManager manager;

	public RetroArcadeBouncyBallHolder Create(RetroArcadeBouncyManager manager, LevelProperties.RetroArcade.Bouncy properties, Vector3 pos, string[] ballTypes)
	{
		RetroArcadeBouncyBallHolder retroArcadeBouncyBallHolder = InstantiatePrefab<RetroArcadeBouncyBallHolder>();
		retroArcadeBouncyBallHolder.manager = manager;
		retroArcadeBouncyBallHolder.properties = properties;
		retroArcadeBouncyBallHolder.transform.position = pos;
		retroArcadeBouncyBallHolder.ballTypes = ballTypes;
		return retroArcadeBouncyBallHolder;
	}

	protected override void Start()
	{
		hp = 1f;
		SetBalls();
		StartCoroutine(move_cr());
	}

	private void SetBalls()
	{
		ballsHeld = new RetroArcadeBouncyBall[ballPositions.Length];
		RetroArcadeBouncyBall retroArcadeBouncyBall = typeABall;
		float num = 120f;
		for (int i = 0; i < ballPositions.Length; i++)
		{
			switch (ballTypes[i])
			{
			case "A":
				retroArcadeBouncyBall = typeABall;
				break;
			case "B":
				retroArcadeBouncyBall = typeBBall;
				break;
			case "C":
				retroArcadeBouncyBall = typeCBall;
				break;
			default:
				Debug.LogError("Something bad happened");
				break;
			}
			RetroArcadeBouncyBall retroArcadeBouncyBall2 = retroArcadeBouncyBall.Create(ballPositions[i].position, manager, properties, num * (float)i);
			ballsHeld[i] = retroArcadeBouncyBall2;
			ballsHeld[i].transform.parent = base.transform;
		}
	}

	private void SeparateBalls()
	{
		GetComponent<Collider2D>().enabled = false;
		RetroArcadeBouncyBall[] array = ballsHeld;
		foreach (RetroArcadeBouncyBall retroArcadeBouncyBall in array)
		{
			retroArcadeBouncyBall.StartMoving(base.transform.position);
		}
		StartCoroutine(check_to_die_cr());
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Gizmos.color = Color.red;
		Transform[] array = ballPositions;
		foreach (Transform transform in array)
		{
			Gizmos.DrawWireSphere(transform.position, 20f);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private IEnumerator move_cr()
	{
		velocity = MathUtils.AngleToDirection(properties.angleRange.RandomFloat());
		while (true)
		{
			base.transform.position += velocity * properties.groupMoveSpeed * CupheadTime.FixedDelta;
			yield return new WaitForFixedUpdate();
		}
	}

	protected override void OnCollisionCeiling(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionCeiling(hit, phase);
		Vector3 newVelocity = velocity;
		newVelocity.y = Mathf.Min(newVelocity.y, 0f - newVelocity.y);
		ChangeDir(newVelocity);
	}

	protected override void OnCollisionGround(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionGround(hit, phase);
		Vector3 newVelocity = velocity;
		newVelocity.y = Mathf.Max(newVelocity.y, 0f - newVelocity.y);
		ChangeDir(newVelocity);
	}

	protected void ChangeDir(Vector3 newVelocity)
	{
		velocity = newVelocity;
		currentAngle = Mathf.Atan2(velocity.y, velocity.x) * 57.29578f;
		base.transform.SetEulerAngles(0f, 0f, currentAngle);
		RetroArcadeBouncyBall[] array = ballsHeld;
		foreach (RetroArcadeBouncyBall retroArcadeBouncyBall in array)
		{
			retroArcadeBouncyBall.transform.SetEulerAngles(0f, 0f, 0f);
		}
	}

	protected override void OnCollisionWalls(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionWalls(hit, phase);
		Vector3 newVelocity = velocity;
		if (base.transform.position.x > 0f)
		{
			newVelocity.x = Mathf.Min(newVelocity.x, 0f - newVelocity.x);
			ChangeDir(newVelocity);
		}
		else
		{
			newVelocity.x = Mathf.Max(newVelocity.x, 0f - newVelocity.x);
			ChangeDir(newVelocity);
		}
	}

	public override void Dead()
	{
		GetComponent<Collider2D>().enabled = false;
		StopAllCoroutines();
		SeparateBalls();
	}

	private IEnumerator check_to_die_cr()
	{
		bool allDead2 = true;
		while (true)
		{
			allDead2 = true;
			for (int i = 0; i < ballsHeld.Length; i++)
			{
				if (!ballsHeld[i].IsDead)
				{
					allDead2 = false;
				}
			}
			if (allDead2)
			{
				break;
			}
			yield return null;
		}
		base.IsDead = true;
	}

	public void DestroyBallsHeld()
	{
		RetroArcadeBouncyBall[] array = ballsHeld;
		foreach (RetroArcadeBouncyBall retroArcadeBouncyBall in array)
		{
			Object.Destroy(retroArcadeBouncyBall.gameObject);
		}
	}
}
