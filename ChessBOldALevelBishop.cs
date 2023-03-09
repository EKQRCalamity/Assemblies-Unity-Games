using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBOldALevelBishop : LevelProperties.ChessBOldA.Entity
{
	private enum PathType
	{
		Straight,
		Infinite,
		Square,
		Pending
	}

	[SerializeField]
	private BasicProjectile turretShot;

	[SerializeField]
	private ParrySwitch pink;

	[SerializeField]
	private Transform pivotPoint;

	[SerializeField]
	private ChessBOldALevelWall wallPrefab;

	private List<ChessBOldALevelWall> walls;

	private DamageDealer damageDealer;

	private PathType pathType;

	private PathType previousPathType;

	private bool pathIsClockwise;

	private float pathSpeed;

	private int pathIndex;

	private Vector3[] positions;

	private bool pinkIsClockwise;

	private float pinkSpeed;

	private bool isStunned;

	private float HPToDecrease;

	private float lerpPos;

	private int phase;

	private int nullIndex;

	private float straightValue;

	private float infinityAngle;

	private void Start()
	{
		walls = new List<ChessBOldALevelWall>();
		pink.OnActivate += GotParried;
		damageDealer = DamageDealer.NewEnemy();
		GetComponent<SpriteRenderer>().color = Color.red;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	public override void LevelInit(LevelProperties.ChessBOldA properties)
	{
		base.LevelInit(properties);
		Level.Current.OnWinEvent += Win;
		LevelProperties.ChessBOldA.Bishop bishop = properties.CurrentState.bishop;
		if (!bishop.canHurtPlayer)
		{
			GetComponent<Collider2D>().enabled = false;
		}
		float f = properties.CurrentHealth / (float)properties.CurrentState.bishop.bishopHealth;
		HPToDecrease = Mathf.Ceil(f);
		base.transform.SetScale(bishop.bishopScale, bishop.bishopScale, bishop.bishopScale);
		pathIndex = 0;
		StartCoroutine(intro_cr());
		StartCoroutine(pink_cr());
		SetValues();
		StartCoroutine(turret_cr());
	}

	private void GotParried()
	{
		base.properties.DealDamage(HPToDecrease);
		StartCoroutine(stunned_cr());
	}

	private IEnumerator stunned_cr()
	{
		RemoveCurrentWalls();
		walls.Clear();
		isStunned = true;
		pink.enabled = false;
		GetComponent<SpriteRenderer>().color = Color.yellow;
		yield return CupheadTime.WaitForSeconds(this, base.properties.CurrentState.bishop.stunnedTime);
		GetComponent<SpriteRenderer>().color = Color.red;
		pink.enabled = true;
		isStunned = false;
		if (base.properties.CurrentHealth > 0f)
		{
			phase++;
			SetValues();
			SetPathValues();
		}
	}

	private void SetValues()
	{
		SetPinkValues();
		SetWallValues();
	}

	private void Win()
	{
		StopAllCoroutines();
	}

	private IEnumerator intro_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 4f);
		SetPathValues();
		yield return null;
	}

	public void SetPathValues()
	{
		LevelProperties.ChessBOldA.BishopPath bishopPath = base.properties.CurrentState.bishopPath;
		int num = Mathf.Clamp(phase, 0, bishopPath.pathTypeString.Split(',').Length - 1);
		int num2 = Mathf.Clamp(phase, 0, bishopPath.pathSpeedString.Split(',').Length - 1);
		int num3 = Mathf.Clamp(phase, 0, bishopPath.pathDirString.Split(',').Length - 1);
		Parser.FloatTryParse(bishopPath.pathSpeedString.Split(',')[num2], out pathSpeed);
		pathIsClockwise = bishopPath.pathDirString.Split(',')[num3][0] == 'R';
		previousPathType = pathType;
		switch (bishopPath.pathTypeString.Split(',')[num][0])
		{
		case 'S':
			pathType = PathType.Straight;
			StartCoroutine(straight_cr());
			break;
		case 'I':
			pathType = PathType.Infinite;
			StartCoroutine(infinite_cr());
			break;
		case 'Q':
			pathType = PathType.Square;
			StartCoroutine(box_cr());
			break;
		}
	}

	private IEnumerator move_cr(Vector3 start, Vector3 end)
	{
		float t = 0f;
		float time = pathSpeed;
		YieldInstruction wait = new WaitForFixedUpdate();
		while (t < time)
		{
			t += CupheadTime.FixedDelta;
			base.transform.position = Vector3.Lerp(start, end, t / time);
			yield return wait;
		}
	}

	private IEnumerator straight_cr()
	{
		LevelProperties.ChessBOldA.BishopPath p = base.properties.CurrentState.bishopPath;
		float t = lerpPos;
		float maxTime = pathSpeed;
		YieldInstruction wait = new WaitForFixedUpdate();
		float startX = 0f - p.straightPathLength;
		float endX = p.straightPathLength;
		float one = 1f;
		if (previousPathType != 0)
		{
			straightValue = ((!pathIsClockwise) ? (one - t / maxTime) : (t / maxTime));
			yield return StartCoroutine(move_cr(base.transform.position, new Vector3(Mathf.Lerp(startX, endX, straightValue), p.straightPathHeight)));
		}
		while (!isStunned)
		{
			if (t < maxTime)
			{
				t += CupheadTime.FixedDelta;
				straightValue = ((!pathIsClockwise) ? (one - t / maxTime) : (t / maxTime));
				base.transform.SetPosition(Mathf.Lerp(startX, endX, straightValue));
			}
			else
			{
				pathIsClockwise = !pathIsClockwise;
				t = 0f;
			}
			yield return wait;
		}
		lerpPos = t;
		yield return null;
	}

	private IEnumerator infinite_cr()
	{
		LevelProperties.ChessBOldA.BishopPath p = base.properties.CurrentState.bishopPath;
		YieldInstruction wait = new WaitForFixedUpdate();
		float loopSizeX = p.infinitePathLength;
		float loopSizeY = p.infinitePathWidth;
		float speed = pathSpeed;
		bool invert = pathIsClockwise;
		pivotPoint.transform.SetPosition(loopSizeX, p.infinitePathHeight);
		Vector3 endPos = Vector3.zero;
		Vector3 pivotOffset = Vector3.left * 2f * loopSizeX;
		if (previousPathType != PathType.Infinite)
		{
			endPos = ((!invert) ? pivotPoint.position : (pivotPoint.position + pivotOffset));
			float value2 = (invert ? 1 : (-1));
			Vector3 handleRotationX2 = new Vector3(Mathf.Cos(infinityAngle) * value2 * loopSizeX, 0f, 0f);
			Vector3 handleRotationY2 = new Vector3(0f, Mathf.Sin(infinityAngle) * loopSizeY, 0f);
			endPos += handleRotationX2 + handleRotationY2;
			yield return StartCoroutine(move_cr(base.transform.position, endPos));
		}
		while (!isStunned)
		{
			infinityAngle += speed * (float)CupheadTime.Delta;
			if (infinityAngle > (float)Math.PI * 2f)
			{
				invert = !invert;
				infinityAngle -= (float)Math.PI * 2f;
			}
			if (infinityAngle < 0f)
			{
				infinityAngle += (float)Math.PI * 2f;
			}
			float value2;
			if (invert)
			{
				base.transform.position = pivotPoint.position + pivotOffset;
				value2 = 1f;
			}
			else
			{
				base.transform.position = pivotPoint.position;
				value2 = -1f;
			}
			Vector3 handleRotationX2 = new Vector3(Mathf.Cos(infinityAngle) * value2 * loopSizeX, 0f, 0f);
			Vector3 handleRotationY2 = new Vector3(0f, Mathf.Sin(infinityAngle) * loopSizeY, 0f);
			base.transform.position += handleRotationX2 + handleRotationY2;
			yield return wait;
		}
		yield return null;
	}

	private IEnumerator box_cr()
	{
		LevelProperties.ChessBOldA.BishopPath p = base.properties.CurrentState.bishopPath;
		float boxCenter = p.squarePathHeight;
		float length = p.squarePathLength / 2f;
		float height = p.squarePathWidth / 2f;
		Vector3 topLeft = new Vector3(boxCenter - length, boxCenter + height);
		Vector3 topRight = new Vector3(boxCenter + length, boxCenter + height);
		Vector3 bottomLeft = new Vector3(boxCenter - length, boxCenter - height);
		Vector3 bottomRight = new Vector3(boxCenter + length, boxCenter - height);
		Vector3[] positions = new Vector3[4] { topRight, bottomRight, bottomLeft, topLeft };
		int incrementBy = (pathIsClockwise ? 1 : (-1));
		float distance2 = 0f;
		float speed2 = 0f;
		Vector3 endPos = positions[pathIndex];
		if (previousPathType != PathType.Square)
		{
			yield return StartCoroutine(move_cr(base.transform.position, endPos));
		}
		while (!isStunned)
		{
			YieldInstruction wait = new WaitForFixedUpdate();
			distance2 = Vector3.Distance(base.transform.position, endPos);
			speed2 = distance2 / pathSpeed;
			while (base.transform.position != endPos)
			{
				base.transform.position = Vector3.MoveTowards(base.transform.position, endPos, speed2 * CupheadTime.FixedDelta);
				if (isStunned)
				{
					break;
				}
				yield return wait;
			}
			if (!isStunned)
			{
				if (pathIsClockwise && pathIndex >= positions.Length - 1)
				{
					pathIndex = 0;
				}
				else if (!pathIsClockwise && pathIndex <= 0)
				{
					pathIndex = positions.Length - 1;
				}
				else
				{
					pathIndex += incrementBy;
				}
			}
			endPos = positions[pathIndex];
			yield return null;
		}
		yield return null;
	}

	private void SetPinkValues()
	{
		LevelProperties.ChessBOldA.Pink pink = base.properties.CurrentState.pink;
		int num = Mathf.Clamp(phase, 0, pink.pinkSpeedString.Split(',').Length - 1);
		int num2 = Mathf.Clamp(phase, 0, pink.pinkDirString.Split(',').Length - 1);
		Parser.FloatTryParse(pink.pinkSpeedString.Split(',')[num], out pinkSpeed);
		pinkIsClockwise = pink.pinkDirString.Split(',')[num2][0] == 'R';
	}

	private IEnumerator pink_cr()
	{
		LevelProperties.ChessBOldA.Pink p = base.properties.CurrentState.pink;
		float angle = 0f;
		pink.transform.SetScale(p.pinkScale, p.pinkScale);
		Vector3 handleRotationX2 = Vector3.zero;
		Vector3 handleRotationY2 = Vector3.zero;
		YieldInstruction wait = new WaitForFixedUpdate();
		while (true)
		{
			angle = ((!pinkIsClockwise) ? (angle - pinkSpeed * CupheadTime.FixedDelta) : (angle + pinkSpeed * CupheadTime.FixedDelta));
			handleRotationX2 = new Vector3(Mathf.Sin(angle) * p.pinkPathRadius, 0f, 0f);
			handleRotationY2 = new Vector3(0f, Mathf.Cos(angle) * p.pinkPathRadius, 0f);
			pink.transform.position = base.transform.position;
			pink.transform.position += handleRotationX2 + handleRotationY2;
			yield return wait;
		}
	}

	private void SetWallValues()
	{
		LevelProperties.ChessBOldA.Walls walls = base.properties.CurrentState.walls;
		nullIndex = Mathf.Clamp(phase, 0, walls.wallNullString.Length - 1);
		int num = Mathf.Clamp(phase, 0, walls.wallNumberString.Split(',').Length - 1);
		int num2 = Mathf.Clamp(phase, 0, walls.wallSpeedString.Split(',').Length - 1);
		int num3 = Mathf.Clamp(phase, 0, walls.wallDirString.Split(',').Length - 1);
		int result = 0;
		float result2 = 0f;
		bool flag = false;
		int[] array = new int[walls.wallNullString[nullIndex].Split(',').Length];
		Parser.IntTryParse(walls.wallNumberString.Split(',')[num], out result);
		Parser.FloatTryParse(walls.wallSpeedString.Split(',')[num2], out result2);
		flag = walls.wallDirString.Split(',')[num3][0] == 'R';
		bool flag2 = false;
		for (int i = 0; i < array.Length; i++)
		{
			flag2 = Parser.IntTryParse(walls.wallNullString[nullIndex].Split(',')[i], out array[i]);
		}
		float num4 = 360f / (float)result;
		bool flag3 = false;
		for (int j = 0; j < result; j++)
		{
			flag3 = false;
			for (int k = 0; k < array.Length; k++)
			{
				if (!flag2)
				{
					break;
				}
				if (j == array[k])
				{
					flag3 = true;
					break;
				}
			}
			if (!flag3)
			{
				ChessBOldALevelWall chessBOldALevelWall = wallPrefab.Spawn();
				chessBOldALevelWall.StartRotate(num4 * (float)j, this, walls.wallPathRadius, result2, flag, walls.wallLength);
				chessBOldALevelWall.transform.parent = base.transform;
				this.walls.Add(chessBOldALevelWall);
			}
		}
	}

	private void RemoveCurrentWalls()
	{
		foreach (ChessBOldALevelWall wall in walls)
		{
			wall.Dead();
		}
	}

	private IEnumerator turret_cr()
	{
		LevelProperties.ChessBOldA.BishopPath p = base.properties.CurrentState.bishopPath;
		AbstractPlayerController player = PlayerManager.GetNext();
		string[] turretString = p.turretShotDelayString.Split(',');
		int turretIndex = UnityEngine.Random.Range(0, turretString.Length);
		bool gotStunned = false;
		float delay = 0f;
		while (true)
		{
			if (gotStunned)
			{
				Parser.FloatTryParse(turretString[turretIndex], out delay);
				yield return CupheadTime.WaitForSeconds(this, delay);
				gotStunned = false;
			}
			Vector3 dir = player.transform.position - base.transform.position;
			turretShot.Create(base.transform.position, MathUtils.DirectionToAngle(dir), p.turretShotSpeed);
			player = PlayerManager.GetNext();
			Parser.FloatTryParse(turretString[turretIndex], out delay);
			yield return CupheadTime.WaitForSeconds(this, delay);
			while (isStunned)
			{
				gotStunned = true;
				yield return null;
			}
		}
	}
}
