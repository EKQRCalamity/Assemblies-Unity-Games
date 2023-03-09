using System.Collections;
using UnityEngine;

public class ChessBOldBLevelGameManager : AbstractPausableComponent
{
	private const int NUM_OF_BALLS = 6;

	private const float LOOP_SIZE = 370f;

	[SerializeField]
	private Color redFlash;

	[SerializeField]
	private Transform pivotPoint;

	[SerializeField]
	private ChessBOldBLevelBoss boss;

	[SerializeField]
	private ChessBOldBReduxLevelBirdie birdiePrefab;

	private ChessBOldBReduxLevelBirdie[] birdies;

	private LevelProperties.ChessBOldB properties;

	private bool goingClockwise;

	private int spinSpeedStringMainIndex;

	private string[] spinSpeedString;

	private int spinSpeedStringIndex;

	private int spinTimeStringMainIndex;

	private string[] spinTimeString;

	private int spinTimeStringIndex;

	private int changeDirStringMainIndex;

	private string[] changeDirString;

	private int changeDirStringIndex;

	private int initialDirStringMainIndex;

	private string[] initialDirString;

	private int initialDirStringIndex;

	private int chosenStringMainIndex;

	private string[] chosenString;

	private int chosenStringIndex;

	public bool WaitingForParry { get; private set; }

	public void SetupGameManager(LevelProperties.ChessBOldB properties)
	{
		this.properties = properties;
		goingClockwise = Rand.Bool();
		InitBalls();
	}

	private void InitBalls()
	{
		birdies = new ChessBOldBReduxLevelBirdie[6];
		Vector3 position = new Vector3(0f, 1000f);
		for (int i = 0; i < 6; i++)
		{
			birdies[i] = Object.Instantiate(birdiePrefab);
			birdies[i].transform.position = position;
			birdies[i].ParryBirdie = ParriedBall;
		}
		StartCoroutine(game_cr());
	}

	private IEnumerator game_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 4f);
		LevelProperties.ChessBOldB.Birdie p = properties.CurrentState.birdie;
		YieldInstruction wait = new WaitForFixedUpdate();
		OnStateChanged();
		float t = 0f;
		float spinTime = 0f;
		float speedTime = 0f;
		int timesToSwitchDir = 0;
		while (true)
		{
			t = 0f;
			yield return StartCoroutine(flash_balls_cr());
			WaitingForParry = true;
			p = properties.CurrentState.birdie;
			spinSpeedString = p.spinSpeedString[spinSpeedStringMainIndex].Split(',');
			spinTimeString = p.spinTimeString[spinTimeStringMainIndex].Split(',');
			changeDirString = p.changeDirectionString[changeDirStringMainIndex].Split(',');
			initialDirString = p.initialDirectionString[initialDirStringMainIndex].Split(',');
			Parser.FloatTryParse(spinTimeString[spinTimeStringIndex], out spinTime);
			Parser.FloatTryParse(spinSpeedString[spinSpeedStringIndex], out speedTime);
			Parser.IntTryParse(changeDirString[changeDirStringIndex], out timesToSwitchDir);
			goingClockwise = initialDirString[initialDirStringIndex][0] == 'R';
			for (int i = 0; i < birdies.Length; i++)
			{
				birdies[i].HandleMovement(speedTime, goingClockwise);
			}
			float timeUntilSwitch = spinTime / (float)timesToSwitchDir;
			int dirCounter = 0;
			bool turnedPink = false;
			while (t < spinTime && WaitingForParry)
			{
				t += CupheadTime.FixedDelta;
				if (!turnedPink && t >= p.prePinkTime)
				{
					for (int j = 0; j < birdies.Length; j++)
					{
						birdies[j].TurnPink();
					}
					turnedPink = true;
				}
				if (dirCounter < timesToSwitchDir && t > timeUntilSwitch * (float)dirCounter)
				{
					dirCounter++;
					goingClockwise = !goingClockwise;
					for (int k = 0; k < birdies.Length; k++)
					{
						birdies[k].HandleMovement(speedTime, goingClockwise);
					}
				}
				yield return wait;
			}
			for (int l = 0; l < birdies.Length; l++)
			{
				birdies[l].StopMoving();
			}
			while (WaitingForParry)
			{
				yield return null;
			}
			if (spinSpeedStringIndex < spinSpeedString.Length - 1)
			{
				spinSpeedStringIndex++;
			}
			else
			{
				spinSpeedStringMainIndex = (spinSpeedStringMainIndex + 1) % p.spinSpeedString.Length;
				spinSpeedStringIndex = 0;
			}
			if (spinTimeStringIndex < spinTimeString.Length - 1)
			{
				spinTimeStringIndex++;
			}
			else
			{
				spinTimeStringMainIndex = (spinTimeStringMainIndex + 1) % p.spinTimeString.Length;
				spinTimeStringIndex = 0;
			}
			if (changeDirStringIndex < changeDirString.Length - 1)
			{
				changeDirStringIndex++;
			}
			else
			{
				changeDirStringMainIndex = (changeDirStringMainIndex + 1) % p.changeDirectionString.Length;
				changeDirStringIndex = 0;
			}
			if (initialDirStringIndex < initialDirString.Length - 1)
			{
				initialDirStringIndex++;
			}
			else
			{
				initialDirStringMainIndex = (initialDirStringMainIndex + 1) % p.initialDirectionString.Length;
				initialDirStringIndex = 0;
			}
			yield return wait;
		}
	}

	private void ParriedBall(bool chosenBall)
	{
		if (chosenBall)
		{
			WaitingForParry = false;
			properties.DealDamage(1f);
			boss.HandleHurt(gettingHurt: true);
		}
	}

	public void OnStateChanged()
	{
		LevelProperties.ChessBOldB.Birdie birdie = properties.CurrentState.birdie;
		spinSpeedStringMainIndex = Random.Range(0, birdie.spinSpeedString.Length);
		spinSpeedString = birdie.spinSpeedString[spinSpeedStringMainIndex].Split(',');
		spinSpeedStringIndex = Random.Range(0, spinSpeedString.Length);
		spinTimeStringMainIndex = Random.Range(0, birdie.spinTimeString.Length);
		spinTimeString = birdie.spinTimeString[spinTimeStringMainIndex].Split(',');
		spinTimeStringIndex = Random.Range(0, spinTimeString.Length);
		changeDirStringMainIndex = Random.Range(0, birdie.changeDirectionString.Length);
		changeDirString = birdie.changeDirectionString[changeDirStringMainIndex].Split(',');
		changeDirStringIndex = Random.Range(0, changeDirString.Length);
		initialDirStringMainIndex = Random.Range(0, birdie.initialDirectionString.Length);
		initialDirString = birdie.initialDirectionString[initialDirStringMainIndex].Split(',');
		initialDirStringIndex = Random.Range(0, initialDirString.Length);
		chosenStringMainIndex = Random.Range(0, birdie.chosenString.Length);
		chosenString = birdie.chosenString[chosenStringMainIndex].Split(',');
		chosenStringIndex = Random.Range(0, chosenString.Length);
	}

	private IEnumerator flash_balls_cr()
	{
		LevelProperties.ChessBOldB.Birdie p = properties.CurrentState.birdie;
		for (int i = 0; i < birdies.Length; i++)
		{
			birdies[i].transform.position = new Vector3(0f, 1000f);
		}
		yield return CupheadTime.WaitForSeconds(this, p.fadeInTime);
		boss.HandleHurt(gettingHurt: false);
		float angleOffset = 60f;
		int chosenIndex = 0;
		chosenString = p.chosenString[chosenStringMainIndex].Split(',');
		Parser.IntTryParse(chosenString[chosenStringIndex], out chosenIndex);
		for (int j = 0; j < birdies.Length; j++)
		{
			bool chosenBall = j == chosenIndex;
			birdies[j].Setup(pivotPoint, angleOffset * (float)j, properties.CurrentState.birdie, 370f, chosenBall);
		}
		Color color = birdies[chosenIndex].GetComponent<SpriteRenderer>().color;
		birdies[chosenIndex].GetComponent<SpriteRenderer>().color = redFlash;
		yield return CupheadTime.WaitForSeconds(this, p.flashTime);
		birdies[chosenIndex].GetComponent<SpriteRenderer>().color = color;
		if (chosenStringIndex < chosenString.Length - 1)
		{
			chosenStringIndex++;
		}
		else
		{
			chosenStringMainIndex = (chosenStringMainIndex + 1) % p.chosenString.Length;
			chosenStringIndex = 0;
		}
		yield return null;
	}
}
