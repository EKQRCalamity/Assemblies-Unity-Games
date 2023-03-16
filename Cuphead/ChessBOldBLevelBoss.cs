using System.Collections;
using UnityEngine;

public class ChessBOldBLevelBoss : LevelProperties.ChessBOldB.Entity
{
	private const float Y_POS = 225f;

	[SerializeField]
	private BasicProjectile projectile;

	[SerializeField]
	private GameObject bossOne;

	[SerializeField]
	private GameObject bossTwo;

	private Color brown;

	private bool isMoving;

	private float moveTime;

	private int bulletDelayStringMainIndex;

	private string[] bulletDelayString;

	private int bulletDelayStringIndex;

	public override void LevelInit(LevelProperties.ChessBOldB properties)
	{
		base.LevelInit(properties);
		StartCoroutine(intro_cr());
	}

	private IEnumerator intro_cr()
	{
		brown = bossTwo.GetComponent<SpriteRenderer>().color;
		yield return CupheadTime.WaitForSeconds(this, 4f);
		MoveBosses();
		StartCoroutine(wait_to_shoot());
		yield return null;
	}

	public void HandleHurt(bool gettingHurt)
	{
		bossOne.GetComponent<SpriteRenderer>().color = ((!gettingHurt) ? brown : Color.red);
		bossTwo.GetComponent<SpriteRenderer>().color = ((!gettingHurt) ? brown : Color.red);
		isMoving = ((!gettingHurt) ? true : false);
	}

	public void OnStateChanged()
	{
		LevelProperties.ChessBOldB.Boss boss = base.properties.CurrentState.boss;
		moveTime = boss.bossTime;
		bulletDelayStringMainIndex = Random.Range(0, boss.bulletDelayString.Length);
		bulletDelayString = boss.bulletDelayString[bulletDelayStringMainIndex].Split(',');
		bulletDelayStringIndex = Random.Range(0, bulletDelayString.Length);
	}

	private void MoveBosses()
	{
		StartCoroutine(move_bosses_cr());
	}

	private IEnumerator move_bosses_cr()
	{
		LevelProperties.ChessBOldB.Boss p = base.properties.CurrentState.boss;
		float t = 0f;
		float one = 1f;
		moveTime = p.bossTime;
		bool countingUp = true;
		YieldInstruction wait = new WaitForFixedUpdate();
		isMoving = true;
		while (true)
		{
			if (!isMoving)
			{
				yield return null;
				continue;
			}
			t += CupheadTime.FixedDelta;
			bossOne.transform.SetPosition(null, Mathf.Lerp(225f, -225f, (!countingUp) ? (one - t / moveTime) : (t / moveTime)));
			bossTwo.transform.SetPosition(null, Mathf.Lerp(225f, -225f, (!countingUp) ? (t / moveTime) : (one - t / moveTime)));
			if (t >= moveTime)
			{
				countingUp = !countingUp;
				t = 0f;
			}
			yield return wait;
		}
	}

	private IEnumerator wait_to_shoot()
	{
		bool leftOneShoot = Rand.Bool();
		LevelProperties.ChessBOldB.Boss p2 = base.properties.CurrentState.boss;
		OnStateChanged();
		float delay = 0f;
		while (true)
		{
			if (!isMoving)
			{
				yield return null;
				continue;
			}
			p2 = base.properties.CurrentState.boss;
			bulletDelayString = p2.bulletDelayString[bulletDelayStringMainIndex].Split(',');
			Parser.FloatTryParse(bulletDelayString[bulletDelayStringIndex], out delay);
			yield return CupheadTime.WaitForSeconds(this, delay);
			GameObject boss = ((!leftOneShoot) ? bossTwo : bossOne);
			Shoot(boss);
			leftOneShoot = !leftOneShoot;
			if (bulletDelayStringIndex < bulletDelayString.Length - 1)
			{
				bulletDelayStringIndex++;
			}
			else
			{
				bulletDelayStringMainIndex = (bulletDelayStringMainIndex + 1) % p2.bulletDelayString.Length;
				bulletDelayStringIndex = 0;
			}
			yield return null;
		}
	}

	private void Shoot(GameObject boss)
	{
		AbstractPlayerController next = PlayerManager.GetNext();
		Vector3 vector = next.center - boss.transform.position;
		float rotation = MathUtils.DirectionToAngle(vector);
		projectile.Create(boss.transform.position, rotation, base.properties.CurrentState.boss.bulletSpeed);
	}
}
