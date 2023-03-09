using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaltbakerLevelPillarHandler : LevelProperties.Saltbaker.Entity
{
	[Header("Other")]
	[SerializeField]
	private GameObject leftPillar;

	[SerializeField]
	private GameObject rightPillar;

	[SerializeField]
	private Animator leftAnimator;

	[SerializeField]
	private Animator rightAnimator;

	[SerializeField]
	private SaltbakerLevelHeart darkHeart;

	[SerializeField]
	private GameObject[] smallPlatform;

	[SerializeField]
	private GameObject[] medPlatform;

	[SerializeField]
	private GameObject[] bigPlatform;

	private List<GameObject> platforms = new List<GameObject>();

	[SerializeField]
	private SaltbakerLevelGlassChunk chunkPrefab;

	private List<SaltbakerLevelGlassChunk> chunks = new List<SaltbakerLevelGlassChunk>();

	[SerializeField]
	private string chunkOrder;

	private PatternString chunkOrderString;

	[SerializeField]
	private string chunkPosition;

	private PatternString chunkPositionString;

	[SerializeField]
	private string chunkSpawnTime;

	private PatternString chunkSpawnTimeString;

	private bool[] chunkFlip = new bool[8];

	private float phaseTimer;

	public bool suppressCenterPlatforms;

	public void TakeDamage(DamageDealer.DamageInfo info)
	{
		base.properties.DealDamage(info.damage);
	}

	public void StartPlatforms()
	{
		leftPillar.transform.position = new Vector3(base.transform.position.x - base.properties.CurrentState.doomPillar.pillarPosition.min, base.transform.position.y);
		rightPillar.transform.position = new Vector3(base.transform.position.x + base.properties.CurrentState.doomPillar.pillarPosition.min, base.transform.position.y);
		StartCoroutine(create_platforms_cr());
		StartCoroutine(create_glass_cr());
	}

	public void StartPillarOfDoom()
	{
		LevelProperties.Saltbaker.DoomPillar doomPillar = base.properties.CurrentState.doomPillar;
		base.properties.OnBossDeath += Die;
		leftAnimator.Play("IntroA", 0, 0f);
		rightAnimator.Play("IntroB", 0, 0f);
		SFX_SALTB_P4_TornadoPillars_Loop();
		StartCoroutine(move_horizontal_cr());
	}

	public void StartHeart()
	{
		darkHeart.gameObject.SetActive(value: true);
		darkHeart.Init(Vector3.up * 500f, leftPillar, rightPillar, base.properties.CurrentState.darkHeart, this);
	}

	public float GetPlatformFallSpeed()
	{
		return Mathf.Lerp(base.properties.CurrentState.doomPillar.platformFallSpeed.min, base.properties.CurrentState.doomPillar.platformFallSpeed.max, Mathf.InverseLerp(0f, base.properties.CurrentState.doomPillar.phaseTime, phaseTimer));
	}

	private IEnumerator create_glass_cr()
	{
		chunkOrderString = new PatternString(chunkOrder);
		chunkPositionString = new PatternString(chunkPosition);
		chunkSpawnTimeString = new PatternString(chunkSpawnTime);
		while (true)
		{
			float t = chunkSpawnTimeString.PopFloat();
			while (t > 0f)
			{
				t -= (float)CupheadTime.Delta * Mathf.Lerp(0.5f, 1f, Mathf.InverseLerp(0f, base.properties.CurrentState.doomPillar.phaseTime, phaseTimer));
				yield return null;
			}
			int usableChunk = -1;
			for (int i = 0; i < chunks.Count; i++)
			{
				if (chunks[i].transform.position.y < -520f)
				{
					usableChunk = i;
					break;
				}
			}
			if (usableChunk == -1)
			{
				chunks.Add(Object.Instantiate(chunkPrefab));
				usableChunk = chunks.Count - 1;
			}
			float xPos = Mathf.Lerp(Level.Current.Left, Level.Current.Right, chunkPositionString.PopFloat());
			int id = chunkOrderString.PopInt();
			bool inBack = Rand.Bool();
			float fallSpeed = GetPlatformFallSpeed() + (float)((!inBack) ? Random.Range(100, 200) : Random.Range(-100, -75));
			chunks[usableChunk].Reset(new Vector3(xPos, 520f), fallSpeed, id < 4, chunkFlip[id], Rand.Bool(), inBack, id % 4);
			chunkFlip[id] = !chunkFlip[id];
			yield return null;
		}
	}

	private IEnumerator create_platforms_cr()
	{
		int bigCounter = 0;
		int mediumCounter = 0;
		int smallCounter = 0;
		float pillarSpawnOffset = 100f;
		LevelProperties.Saltbaker.DoomPillar p = base.properties.CurrentState.doomPillar;
		float pillarXBuffer = 146f + leftPillar.GetComponent<BoxCollider2D>().size.x / 2f;
		YieldInstruction wait = new WaitForFixedUpdate();
		PatternString platformSize = new PatternString(p.platformSizeString);
		PatternString platformPosX = new PatternString(p.platformXSpawnString);
		PatternString platformPosY = new PatternString(p.platformYSpawnString);
		if (p.platformXYUnified)
		{
			platformPosY.SetMainStringIndex(platformPosX.GetMainStringIndex());
			platformPosY.SetSubStringIndex(platformPosX.GetSubStringIndex());
			if (platformPosX.SubStringLength() != platformPosY.SubStringLength())
			{
				Debug.Break();
			}
			platformPosY.PopFloat();
		}
		float spawnDistance = 0f;
		while (true)
		{
			Vector3 spawnPos = new Vector3(Mathf.Lerp(leftPillar.transform.position.x + pillarXBuffer, rightPillar.transform.position.x - pillarXBuffer, (platformPosX.PopFloat() + 1f) / 2f), (float)Level.Current.Ceiling + pillarSpawnOffset + spawnDistance);
			if (suppressCenterPlatforms)
			{
				spawnPos = new Vector3((!Rand.Bool()) ? (rightPillar.transform.position.x - pillarXBuffer) : (leftPillar.transform.position.x + pillarXBuffer), spawnPos.y);
			}
			spawnDistance = platformPosY.PopFloat();
			GameObject platform = null;
			switch (platformSize.PopLetter())
			{
			case 'S':
				platform = Object.Instantiate(smallPlatform[smallCounter % 2]);
				platform.transform.GetChild(0).localScale = new Vector3((smallCounter % 4 < 2) ? 1 : (-1), 1f);
				smallCounter++;
				break;
			case 'M':
				platform = Object.Instantiate(medPlatform[mediumCounter % 2]);
				platform.transform.GetChild(0).localScale = new Vector3((mediumCounter % 4 < 2) ? 1 : (-1), 1f);
				mediumCounter++;
				break;
			case 'L':
				platform = Object.Instantiate(bigPlatform[bigCounter % 2]);
				platform.transform.GetChild(0).localScale = new Vector3((bigCounter % 4 < 2) ? 1 : (-1), 1f);
				bigCounter++;
				break;
			default:
				Debug.LogError("Pattern string is incorrect.");
				Debug.Break();
				break;
			}
			platform.transform.position = spawnPos;
			platforms.Add(platform.gameObject);
			while (spawnDistance > 0f)
			{
				spawnDistance -= CupheadTime.FixedDelta * GetPlatformFallSpeed();
				yield return wait;
			}
		}
	}

	private IEnumerator move_horizontal_cr()
	{
		LevelProperties.Saltbaker.DoomPillar p = base.properties.CurrentState.doomPillar;
		YieldInstruction wait = new WaitForFixedUpdate();
		while (true)
		{
			float t = Mathf.InverseLerp(0f, p.phaseTime, phaseTimer);
			leftPillar.transform.position = Vector3.Lerp(new Vector3(base.transform.position.x - p.pillarPosition.min, base.transform.position.y), new Vector3(base.transform.position.x - p.pillarPosition.max, base.transform.position.y), t);
			rightPillar.transform.position = Vector3.Lerp(new Vector3(base.transform.position.x + p.pillarPosition.min, base.transform.position.y), new Vector3(base.transform.position.x + p.pillarPosition.max, base.transform.position.y), t);
			phaseTimer += CupheadTime.FixedDelta;
			yield return wait;
		}
	}

	private void Update()
	{
		platforms.RemoveAll((GameObject g) => g == null);
		foreach (GameObject platform in platforms)
		{
			if (platform.transform.position.y < (float)Level.Current.Ground - 400f)
			{
				Object.Destroy(platform.gameObject);
			}
			else
			{
				platform.transform.position += Vector3.down * GetPlatformFallSpeed() * CupheadTime.Delta;
			}
		}
		foreach (LevelPlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if ((bool)allPlayer)
			{
				allPlayer.animationController.spriteRenderer.sortingOrder = (((int)allPlayer.motor.MoveDirection.y <= 0) ? 10 : 510);
			}
		}
	}

	private void Die()
	{
		leftAnimator.Play("Death", -1, 0f);
		rightAnimator.Play("Death", -1, 0.5f);
		SFX_SALTB_P4_TornadoPillar_LoopStop();
		darkHeart.Die();
	}

	private void SFX_SALTB_P4_TornadoPillars_Loop()
	{
		AudioManager.PlayLoop("sfx_DLC_Saltbaker_P4_Tornado_Left_Loop");
		AudioManager.PlayLoop("sfx_DLC_Saltbaker_P4_Tornado_Right_Loop");
	}

	private void SFX_SALTB_P4_TornadoPillar_LoopStop()
	{
		AudioManager.Stop("sfx_DLC_Saltbaker_P4_Tornado_Left_Loop");
		AudioManager.Stop("sfx_DLC_Saltbaker_P4_Tornado_Right_Loop");
	}
}
