using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RumRunnersLevelSpider : LevelProperties.RumRunners.Entity
{
	private enum SummonType
	{
		Grubs,
		Mine,
		Bouncing,
		None
	}

	private const float INTRO_EXIT_DELAY = 0.2f;

	private const float EDGE_OFFSET = 350f;

	private const float MINE_DELAY = 0.2f;

	private const float MINE_EXPLODE_INTERVAL_ON_PHASE_END = 0.6f;

	private const float GRUB_MIN_SPIDER_DISTANCE_TO_ENTER = 500f;

	private const float GRUB_MIN_OTHER_GRUB_DISTANCE_TO_ENTER = 200f;

	private const float BOUNCER_MIN_ANGLE = 10f;

	private const float BOUNCER_MAX_ANGLE = 80f;

	private const float KICK_SPAWN_RADIUS = 110f;

	[SerializeField]
	private Transform[] spawnPoints;

	[SerializeField]
	private AnimationClip runClip;

	[SerializeField]
	private RumRunnersLevelPoliceman policeman;

	[SerializeField]
	private float deathInvincibilityBuffer;

	[SerializeField]
	private Effect deathExplodeEffect;

	[Header("Summons")]
	[SerializeField]
	private RumRunnersLevelGrub grubPrefab;

	[SerializeField]
	private RumRunnersLevelGrubPath[] grubPaths;

	[SerializeField]
	private RumRunnersLevelMine minePrefab;

	[SerializeField]
	private RumRunnersLevelBouncingBeetle caterpillarPrefab;

	[SerializeField]
	private Transform caterpillarSpawnPoint;

	[SerializeField]
	private Effect kickFXEffect;

	[SerializeField]
	private Transform kickFXSpawnPoint;

	private SummonType summonType;

	public bool isSummoning;

	private Vector3 nextCopPosition;

	private PatternString grubDelayString;

	private PatternString grubPositionString;

	private List<RumRunnersLevelGrub> grubList = new List<RumRunnersLevelGrub>();

	private int mineMainIndex;

	private int mineIndex;

	private List<RumRunnersLevelMine> mineList = new List<RumRunnersLevelMine>();

	private Vector3[,] minePositions;

	private PatternString bouncingPattern;

	private int grubEnterVariant;

	private int grubVariant;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	private Collider2D collider;

	private float scaleX;

	private float copSpawnPos;

	private List<RumRunnersLevelBouncingBeetle> beetleList = new List<RumRunnersLevelBouncingBeetle>();

	public bool goingLeft { get; private set; }

	public bool moving { get; private set; }

	private float dir => (!goingLeft) ? 1 : (-1);

	public event Action OnDeathEvent;

	private void Start()
	{
		goingLeft = true;
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		collider = GetComponent<Collider2D>();
		scaleX = base.transform.localScale.x;
		base.transform.SetScale(scaleX * dir);
		SetUpMinePositions();
		grubEnterVariant = UnityEngine.Random.Range(0, 3);
		grubVariant = UnityEngine.Random.Range(0, 4);
	}

	public override void LevelInit(LevelProperties.RumRunners properties)
	{
		base.LevelInit(properties);
		mineMainIndex = UnityEngine.Random.Range(0, properties.CurrentState.mine.minePlacementString.Length);
		mineIndex = UnityEngine.Random.Range(0, properties.CurrentState.mine.minePlacementString[mineMainIndex].Split(',').Length);
		bouncingPattern = new PatternString(properties.CurrentState.bouncing.shootBeetleAngleString);
		grubDelayString = new PatternString(properties.CurrentState.grubs.delayString);
		grubPositionString = new PatternString(properties.CurrentState.grubs.appearPositionString, randomizeMain: true, randomizeSub: false);
		Level.Current.OnLevelStartEvent += OnIntroEnd;
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		float x = base.transform.position.x;
		float num = (float)(-Level.Current.Width) * 0.5f + deathInvincibilityBuffer;
		float num2 = (float)Level.Current.Width * 0.5f - deathInvincibilityBuffer;
		float num3 = base.properties.CurrentHealth - base.properties.GetNextStateHealthTrigger() * base.properties.TotalHealth;
		if (!(info.damage > num3) || (!(x > num2) && !(x < num)))
		{
			base.properties.DealDamage(info.damage);
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (damageDealer != null && phase == CollisionPhase.Enter)
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

	private void OnIntroEnd()
	{
		policeman.SetProperties(base.properties.CurrentState.spider, this);
		Level.Current.OnLevelStartEvent -= OnIntroEnd;
		StartCoroutine(introExit());
	}

	private IEnumerator introExit()
	{
		yield return CupheadTime.WaitForSeconds(this, 0.2f);
		base.animator.SetTrigger("IntroExit");
		yield return base.animator.WaitForAnimationToEnd(this, "IntroExit");
		StartCoroutine(run_cr());
	}

	private void SummonSelection()
	{
		StartCoroutine(check_to_start_summon_cr());
	}

	private IEnumerator check_to_start_summon_cr()
	{
		switch (summonType)
		{
		case SummonType.Bouncing:
			base.animator.Play("Kick");
			yield return null;
			isSummoning = true;
			yield return base.animator.WaitForAnimationToEnd(this, "Kick");
			isSummoning = false;
			break;
		case SummonType.Mine:
			base.animator.Play("MineSummon");
			yield return null;
			isSummoning = true;
			while (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.5f)
			{
				yield return null;
			}
			StartMine();
			yield return base.animator.WaitForAnimationToEnd(this, "MineSummon");
			isSummoning = false;
			break;
		case SummonType.Grubs:
			base.animator.Play("GrubSummonWait");
			yield return null;
			isSummoning = true;
			StartGrubs();
			yield return base.animator.WaitForAnimationToEnd(this, "GrubSummonWait");
			isSummoning = false;
			break;
		}
	}

	private void animationEvent_SpawnFrontPuffEffect()
	{
	}

	private void animationEvent_SpawnBackPuffEffect()
	{
	}

	private IEnumerator run_cr()
	{
		LevelProperties.RumRunners.Spider p = base.properties.CurrentState.spider;
		bool hasSummoned = false;
		bool spawnedCop = false;
		moving = false;
		PatternString copPositionString = new PatternString(p.copPositionString);
		PatternString copBulletTypeString = new PatternString(p.copBulletTypeString);
		PatternString spiderPositionString = new PatternString(p.spiderPositionString);
		PatternString spiderActionString = new PatternString(p.spiderActionString);
		PatternString spiderActionPositionString = new PatternString(p.spiderActionPositionString);
		YieldInstruction wait = new WaitForFixedUpdate();
		copSpawnPos = p.copSpawnSpiderDist;
		bool isInitial = true;
		while (true)
		{
			char summonChar;
			if (isInitial)
			{
				summonChar = ((!Rand.Bool()) ? 'M' : 'N');
				spawnedCop = true;
			}
			else
			{
				yield return CupheadTime.WaitForSeconds(this, p.spiderEnterDelay);
				summonChar = spiderActionString.PopLetter();
			}
			int spawnPointIndex = spiderPositionString.PopInt();
			float popOutX = ((!goingLeft) ? (-640) : 640);
			float summonPos = Mathf.Lerp(popOutX, 0f - popOutX, spiderActionPositionString.PopFloat());
			switch (summonChar)
			{
			case 'B':
				summonPos = Mathf.Lerp(popOutX, 0f - popOutX, 0.05f);
				summonType = SummonType.Bouncing;
				if (spawnPointIndex == 2)
				{
					spawnPointIndex = ((!Rand.Bool()) ? 1 : 0);
				}
				break;
			case 'M':
				summonType = SummonType.Mine;
				break;
			case 'G':
				summonType = SummonType.Grubs;
				break;
			default:
				summonType = SummonType.None;
				hasSummoned = true;
				break;
			}
			if (!isInitial)
			{
				base.transform.position = new Vector3(((float)Level.Current.Right + 350f) * (0f - dir), spawnPoints[spawnPointIndex].position.y);
			}
			if (isInitial && summonType == SummonType.Mine)
			{
				hasSummoned = false;
				summonPos = Mathf.Lerp(popOutX, 0f - popOutX, 0.75f);
			}
			float timeToSummon = Mathf.Abs(base.transform.position.x - summonPos) / p.spiderSpeed;
			float animatorStartTime = 1f - timeToSummon / runClip.length;
			int copPos = copPositionString.PopInt();
			nextCopPosition = new Vector3(spawnPoints[copPos].position.x * dir, spawnPoints[copPos].position.y);
			base.transform.SetScale(scaleX * dir);
			if (!isInitial)
			{
				string stateName = "Run";
				if (summonType == SummonType.Grubs)
				{
					stateName = "GrubSummonEnter";
				}
				else if (summonType == SummonType.Bouncing)
				{
					stateName = "RunCaterpillar";
				}
				base.animator.Play(stateName, 0, animatorStartTime);
			}
			bool isPink = copBulletTypeString.PopLetter() == 'P';
			moving = true;
			if (summonType == SummonType.Grubs)
			{
				SFX_RUMRUN_Spider_GrubSummon_PhoneTinyVoice();
			}
			while ((goingLeft && base.transform.position.x > popOutX) || (!goingLeft && base.transform.position.x < popOutX))
			{
				base.transform.position += Vector3.right * p.spiderSpeed * CupheadTime.FixedDelta * dir;
				base.transform.SetPosition(null, RumRunnersLevel.GroundWalkingPosY(base.transform.position, collider));
				yield return wait;
			}
			while (moving)
			{
				if (!hasSummoned && ((goingLeft && base.transform.position.x <= summonPos) || (!goingLeft && base.transform.position.x >= summonPos)))
				{
					SummonSelection();
					hasSummoned = true;
				}
				while (isSummoning)
				{
					yield return null;
				}
				if ((!goingLeft && (float)Level.Current.Right + 350f > base.transform.position.x) || (goingLeft && (float)Level.Current.Left - 350f < base.transform.position.x))
				{
					float copSpawnDistanceRemaining = copSpawnPos - dir * base.transform.position.x;
					if (!spawnedCop && copSpawnDistanceRemaining < 0f)
					{
						policeman.CopAppear(nextCopPosition, isPink, goingLeft);
						spawnedCop = true;
						nextCopPosition = Vector3.up * 5000f;
					}
					base.transform.position += Vector3.right * p.spiderSpeed * CupheadTime.FixedDelta * dir;
					base.transform.SetPosition(null, RumRunnersLevel.GroundWalkingPosY(base.transform.position, collider));
					yield return wait;
				}
				else
				{
					moving = false;
				}
			}
			hasSummoned = false;
			spawnedCop = false;
			goingLeft = !goingLeft;
			isInitial = false;
			yield return wait;
		}
	}

	public bool GrubCanEnter(Vector3 pos, float enterTime)
	{
		if (!moving)
		{
			return false;
		}
		if (Mathf.Abs(base.transform.position.y - pos.y) < 100f && (Mathf.Abs(base.transform.position.x + base.properties.CurrentState.spider.spiderSpeed * dir * enterTime - pos.x) < 500f || Mathf.Abs(base.transform.position.x - pos.x) < 500f))
		{
			return false;
		}
		if (Mathf.Abs(base.transform.position.x) > 400f)
		{
			if (policeman.isActive && Mathf.Abs(policeman.transform.position.y - pos.y) < 100f && Mathf.Sign(policeman.transform.position.x) == Mathf.Sign(pos.x))
			{
				return false;
			}
			if (Mathf.Abs(nextCopPosition.y - pos.y) < 100f && Mathf.Sign(nextCopPosition.x) == Mathf.Sign(pos.x))
			{
				return false;
			}
		}
		grubList.RemoveAll((RumRunnersLevelGrub g) => g == null);
		for (int i = 0; i < grubList.Count; i++)
		{
			if (grubList[i].startedEntering && Mathf.Abs(grubList[i].transform.position.y - pos.y) < 100f)
			{
				bool flag = !grubList[i].moving;
				if (Mathf.Abs((grubList[i].transform.position + Vector3.right * grubList[i].speed * (enterTime - grubList[i].GetTimeToMove())).x - pos.x) < 200f)
				{
					return false;
				}
				if (flag && Mathf.Abs((grubList[i].transform.position + Vector3.left * grubList[i].speed * (enterTime - grubList[i].GetTimeToMove())).x - pos.x) < 200f)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void StartGrubs()
	{
		StartCoroutine(grubs_cr());
	}

	private IEnumerator grubs_cr()
	{
		LevelProperties.RumRunners.Grubs p = base.properties.CurrentState.grubs;
		float delay2 = 0f;
		int y2 = 0;
		int x2 = 0;
		yield return CupheadTime.WaitForSeconds(this, base.properties.CurrentState.grubs.grubSummonWarning);
		int count = grubPositionString.SubStringLength();
		grubPositionString.SetSubStringIndex(count);
		for (int i = 0; i < count; i++)
		{
			AbstractPlayerController player = PlayerManager.GetNext();
			int appearPosition = grubPositionString.PopInt();
			x2 = appearPosition % 6;
			y2 = appearPosition / 6;
			if (x2 == 5 && y2 == 2)
			{
				continue;
			}
			grubList.RemoveAll((RumRunnersLevelGrub g) => g == null);
			bool canSpawn = true;
			for (int j = 0; j < grubList.Count; j++)
			{
				if (!grubList[j].startedEntering && grubList[j].x == x2 && grubList[j].y == y2)
				{
					canSpawn = false;
				}
			}
			if (canSpawn)
			{
				grubList.Add(grubPrefab.Create(grubPaths[y2 * 6 + x2], 0f, p.movementSpeed, p.warningDuration, p.hp, this, grubEnterVariant, grubVariant, count - i, x2, y2));
				grubEnterVariant = (grubEnterVariant + 1) % 3;
				grubVariant = (grubVariant + 1) % 4;
				if (i < count - 1)
				{
					delay2 = grubDelayString.PopFloat();
					yield return CupheadTime.WaitForSeconds(this, delay2);
				}
			}
		}
		yield return null;
	}

	private void SetUpMinePositions()
	{
		minePositions = new Vector3[5, 3];
		ref Vector3 reference = ref minePositions[0, 0];
		reference = new Vector3(-553f, 354f);
		ref Vector3 reference2 = ref minePositions[1, 0];
		reference2 = new Vector3(-282f, 321f);
		ref Vector3 reference3 = ref minePositions[2, 0];
		reference3 = new Vector3(16f, 354f);
		ref Vector3 reference4 = ref minePositions[3, 0];
		reference4 = new Vector3(311f, 313f);
		ref Vector3 reference5 = ref minePositions[4, 0];
		reference5 = new Vector3(545f, 343f);
		ref Vector3 reference6 = ref minePositions[0, 1];
		reference6 = new Vector3(-492f, 33f);
		ref Vector3 reference7 = ref minePositions[1, 1];
		reference7 = new Vector3(-247f, 19f);
		ref Vector3 reference8 = ref minePositions[2, 1];
		reference8 = new Vector3(42f, 35f);
		ref Vector3 reference9 = ref minePositions[3, 1];
		reference9 = new Vector3(287f, 7f);
		ref Vector3 reference10 = ref minePositions[4, 1];
		reference10 = new Vector3(509f, 36f);
		ref Vector3 reference11 = ref minePositions[0, 2];
		reference11 = new Vector3(-524f, -284f);
		ref Vector3 reference12 = ref minePositions[1, 2];
		reference12 = new Vector3(-224f, -265f);
		ref Vector3 reference13 = ref minePositions[2, 2];
		reference13 = new Vector3(-17f, -294f);
		ref Vector3 reference14 = ref minePositions[3, 2];
		reference14 = new Vector3(253f, -252f);
		ref Vector3 reference15 = ref minePositions[4, 2];
		reference15 = new Vector3(575f, -291f);
	}

	public void StartMine()
	{
		StartCoroutine(mine_cr());
	}

	private IEnumerator mine_cr()
	{
		LevelProperties.RumRunners.Mine p = base.properties.CurrentState.mine;
		AbstractPlayerController player1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		AbstractPlayerController player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		Vector2 pos = Vector2.zero;
		string[] minePlacementString = p.minePlacementString[mineMainIndex].Split(',');
		mineList.RemoveAll((RumRunnersLevelMine m) => m == null);
		mineIndex = 0;
		for (int i = 0; (float)i < Mathf.Min(p.mineNumber, minePlacementString.Length); i++)
		{
			pos = GetMinePos(Parser.IntParse(minePlacementString[mineIndex]));
			bool foundFreeSpot2 = false;
			int checkedPositionsCount = 0;
			while (!foundFreeSpot2 && checkedPositionsCount < 15)
			{
				float distP1 = 1000f;
				float distP2 = 1000f;
				bool spotOccupied = false;
				for (int j = 0; j < mineList.Count; j++)
				{
					if (mineList[j].xPos == (int)pos.x && mineList[j].yPos == (int)pos.y)
					{
						spotOccupied = true;
					}
				}
				if (!spotOccupied)
				{
					if (!player1.IsDead)
					{
						distP1 = Vector3.Distance(player1.transform.position, minePositions[(int)pos.x, (int)pos.y]);
					}
					if (player2 != null && !player2.IsDead)
					{
						distP2 = Vector3.Distance(player2.transform.position, minePositions[(int)pos.x, (int)pos.y]);
					}
				}
				if (distP1 > p.mineCheckToLand && distP2 > p.mineCheckToLand && !spotOccupied)
				{
					foundFreeSpot2 = true;
					break;
				}
				if (mineIndex < minePlacementString.Length - 1)
				{
					mineIndex++;
				}
				else
				{
					mineMainIndex = (mineMainIndex + 1) % p.minePlacementString.Length;
					mineIndex = 0;
				}
				pos = GetMinePos(Parser.IntParse(minePlacementString[mineIndex]));
				checkedPositionsCount++;
				yield return null;
			}
			if (checkedPositionsCount < 15)
			{
				RumRunnersLevelMine rumRunnersLevelMine = minePrefab.Spawn();
				mineList.Add(rumRunnersLevelMine.Init(minePositions[(int)pos.x, (int)pos.y], p, this, Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)));
			}
			yield return CupheadTime.WaitForSeconds(this, 0.2f);
			if (mineIndex < minePlacementString.Length - 1)
			{
				mineIndex++;
				continue;
			}
			mineMainIndex = (mineMainIndex + 1) % p.minePlacementString.Length;
			mineIndex = 0;
		}
		yield return null;
	}

	private Vector2 GetMinePos(int mineNum)
	{
		Vector2 zero = Vector2.zero;
		zero.x = mineNum % 5;
		zero.y = mineNum / 5;
		if (zero.x > 4f || zero.x < 0f || zero.y > 2f || zero.y < 0f)
		{
			Debug.Break();
		}
		return zero;
	}

	private void animationEvent_StartBouncing()
	{
		LevelProperties.RumRunners.Bouncing bouncing = base.properties.CurrentState.bouncing;
		do
		{
			beetleList.RemoveAll((RumRunnersLevelBouncingBeetle b) => b == null || b.leaveScreen);
			if (beetleList.Count >= bouncing.maxBeetleCount)
			{
				beetleList[0].leaveScreen = true;
			}
		}
		while (beetleList.Count >= bouncing.maxBeetleCount);
		float value = bouncingPattern.PopFloat();
		value = Mathf.Clamp(value, 10f, 80f);
		if (dir < 0f)
		{
			value = 180f - value;
		}
		Vector3 vector = MathUtils.AngleToDirection(value);
		RumRunnersLevelBouncingBeetle rumRunnersLevelBouncingBeetle = caterpillarPrefab.Spawn();
		rumRunnersLevelBouncingBeetle.Init(caterpillarSpawnPoint.position + vector * 110f, vector, bouncing.shootBeetleInitialSpeed, bouncing.shootBeetleTimeToSlowdown, bouncing.shootBeetleSpeed, bouncing.shootBeetleHealth);
		beetleList.Add(rumRunnersLevelBouncingBeetle);
		kickFXEffect.Create(kickFXSpawnPoint.position + vector * 110f * 0.8f);
	}

	public void Die()
	{
		base.animator.SetTrigger("Dead");
		GetComponent<SpriteRenderer>().sortingLayerName = SpriteLayer.Effects.ToString();
		GetComponent<SpriteRenderer>().sortingOrder = 0;
		StopAllCoroutines();
		deathExplodeEffect.Create(base.transform.position);
		foreach (RumRunnersLevelBouncingBeetle beetle in beetleList)
		{
			beetle.leaveScreen = true;
		}
		mineList.RemoveAll((RumRunnersLevelMine m) => m == null);
		mineList.Sort((RumRunnersLevelMine m1, RumRunnersLevelMine m2) => m1.endPhaseExplodePriority.CompareTo(m2.endPhaseExplodePriority));
		for (int i = 0; i < mineList.Count; i++)
		{
			mineList[i].SetTimer((float)i * 0.6f);
		}
		if (this.OnDeathEvent != null)
		{
			this.OnDeathEvent();
		}
		SFX_RUMRUN_ExitPhase1_SpiderFalling();
	}

	private void AniEvent_ChangeToForeground()
	{
		GetComponent<SpriteRenderer>().sortingLayerName = SpriteLayer.Foreground.ToString();
		GetComponent<SpriteRenderer>().sortingOrder = 100;
	}

	private void animationEvent_ShakeScreen()
	{
		CupheadLevelCamera.Current.Shake(30f, 0.6f);
	}

	private void AniEvent_DeathComplete()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		if ((bool)Level.Current)
		{
			float x = copSpawnPos * dir;
			Vector3 from = new Vector3(x, -360f);
			Vector3 to = new Vector3(x, 360f);
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(from, to);
		}
		Gizmos.DrawWireSphere(caterpillarSpawnPoint.position, 110f);
	}

	private void AnimationEvent_SFX_RUMRUN_Mine_SpiderButtonPress()
	{
		AudioManager.Play("sfx_dlc_rumrun_mine_spiderbuttonpress");
	}

	private void AnimationEvent_SFX_RUMRUN_Spider_GrubSummon_Phone()
	{
		AudioManager.Play("sfx_dlc_rumrun_spider_grubsummon_phone");
		AudioManager.Stop("sfx_dlc_rumrun_spider_grubsummon_phonetinyvoice");
	}

	private void SFX_RUMRUN_Spider_GrubSummon_PhoneTinyVoice()
	{
		AudioManager.Play("sfx_dlc_rumrun_spider_grubsummon_phonetinyvoice");
		emitAudioFromObject.Add("sfx_dlc_rumrun_spider_grubsummon_phonetinyvoice");
	}

	private void AnimationEvent_SFX_RUMRUN_CaterpillarBall_SpiderKick()
	{
		AudioManager.Play("sfx_dlc_rumrun_caterpillarball_spiderkick");
	}

	private void SFX_RUMRUN_ExitPhase1_SpiderFalling()
	{
		AudioManager.Play("sfx_DLC_RUMRUN_ExitPhase1_SpiderFalling");
		AudioManager.FadeSFXVolume("sfx_DLC_RUMRUN_ExitPhase1_SpiderFalling", 1f, 10f);
	}
}
