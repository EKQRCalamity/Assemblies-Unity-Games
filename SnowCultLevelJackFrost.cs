using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowCultLevelJackFrost : LevelProperties.SnowCult.Entity
{
	public enum States
	{
		Intro,
		Idle,
		Switch,
		Eye,
		Beam,
		Hazard,
		Shard,
		SplitShot,
		Arc
	}

	private const int BLINK_LOOP_COUNT_MIN = 1;

	private const int BLINK_LOOP_COUNT_MAX = 5;

	[SerializeField]
	private BoxCollider2D boxCollider;

	[SerializeField]
	private SnowCultLevelSplitShotBullet mouthPrefab;

	[SerializeField]
	private SnowCultLevelSplitShotBullet mouthPinkPrefab;

	[SerializeField]
	private SnowCultLevelShard shardPrefab;

	[SerializeField]
	private Effect iceCreamSparkle;

	[SerializeField]
	private SnowCultLevelEyeProjectile eyeProjectile;

	private SnowCultLevelEyeProjectile activeEyeProjectile;

	public Transform eyeProjectileGuide;

	[SerializeField]
	private Transform eyeRoot;

	[SerializeField]
	private Transform mouthRoot;

	[SerializeField]
	private Transform splitShotRoot;

	[SerializeField]
	private Transform platformPivotPoint;

	[SerializeField]
	private GameObject platformPrefab;

	[SerializeField]
	private Transform[] platformsPresetPositions;

	[SerializeField]
	private GameObject wizardDeath;

	[SerializeField]
	private GameObject bucket;

	[SerializeField]
	private SpriteRenderer iceCreamGhostRenderer;

	private bool onRight;

	private bool rightSideUp;

	private bool isClockwise;

	private bool firstAttack = true;

	private float positionX;

	private Vector3 scale;

	private SnowCultLevelPlatform[] presetPlatforms;

	private SnowCultLevelPlatform[] circlePlatforms;

	private AbstractPlayerController player;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	public States state;

	private PatternString faceOrientation;

	private PatternString splitShotPink;

	private PatternString shotCoord;

	private PatternString shardAngleOffsetString;

	private SpriteRenderer rend;

	private bool fireSplitShot;

	private int blinkCounter;

	private int blinkCount;

	public bool dead;

	protected override void Awake()
	{
		base.Awake();
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		base.properties.OnBossDeath += OnBossDeath;
		rend = GetComponent<SpriteRenderer>();
		blinkCount = UnityEngine.Random.Range(1, 5);
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		base.properties.DealDamage(info.damage);
	}

	public void Intro()
	{
		state = States.Intro;
	}

	public void StartPhase3()
	{
		bucket.SetActive(value: true);
		base.gameObject.SetActive(value: true);
		scale = base.transform.localScale;
		positionX = base.transform.position.x;
		onRight = Rand.Bool();
		ChangeSide();
		rightSideUp = true;
		faceOrientation = new PatternString(base.properties.CurrentState.face.faceOrientationString);
		splitShotPink = new PatternString(base.properties.CurrentState.splitShot.pinkString);
		shotCoord = new PatternString(base.properties.CurrentState.splitShot.shotCoordString);
		splitShotPink.SetSubStringIndex(-1);
		shotCoord.SetSubStringIndex(-1);
		shardAngleOffsetString = new PatternString(base.properties.CurrentState.shardAttack.angleOffset);
		StartCoroutine(remove_platforms_cr());
	}

	private IEnumerator remove_platforms_cr()
	{
		int count = presetPlatforms.Length;
		while (count > 0)
		{
			SnowCultLevelPlatform[] array = presetPlatforms;
			foreach (SnowCultLevelPlatform snowCultLevelPlatform in array)
			{
				if (snowCultLevelPlatform != null && snowCultLevelPlatform.transform.position.y < Camera.main.transform.position.y - 450f)
				{
					snowCultLevelPlatform.transform.DetachChildren();
					UnityEngine.Object.Destroy(snowCultLevelPlatform.gameObject);
					count--;
				}
			}
			yield return null;
		}
	}

	private void EndIntro()
	{
		state = States.Idle;
		boxCollider.enabled = true;
	}

	public void CreatePlatforms()
	{
		presetPlatforms = new SnowCultLevelPlatform[platformsPresetPositions.Length];
		isClockwise = Rand.Bool();
		LevelProperties.SnowCult.Platforms platforms = base.properties.CurrentState.platforms;
		circlePlatforms = new SnowCultLevelPlatform[platforms.platformNum];
		float num = 360f / (float)platforms.platformNum;
		for (int i = 0; i < platforms.platformNum; i++)
		{
			circlePlatforms[i] = UnityEngine.Object.Instantiate(platformPrefab).transform.GetChild(0).GetComponent<SnowCultLevelPlatform>();
			circlePlatforms[i].transform.parent.position = platformPivotPoint.transform.position;
			circlePlatforms[i].StartRotate(num * (float)i, new Vector3(platformPivotPoint.transform.position.x, platformPivotPoint.transform.position.y + platforms.pivotPointYOffset), platforms.loopSizeX, platforms.loopSizeY, platforms.platformSpeed, platforms.pivotPointYOffset, isClockwise);
			circlePlatforms[i].SetID(i);
		}
	}

	public void CreateAscendingPlatform(int i)
	{
		presetPlatforms[i] = UnityEngine.Object.Instantiate(platformPrefab).transform.GetChild(0).GetComponent<SnowCultLevelPlatform>();
		presetPlatforms[i].transform.parent.position = platformsPresetPositions[i].transform.position;
		presetPlatforms[i].SetID(i);
	}

	private void AniEvent_CheckBlink()
	{
		blinkCounter++;
		if (blinkCounter >= blinkCount)
		{
			base.animator.SetTrigger("Blink");
			blinkCount = UnityEngine.Random.Range(1, 5);
			blinkCounter = 0;
		}
	}

	public void StartSwitch()
	{
		if (firstAttack)
		{
			firstAttack = false;
		}
		else
		{
			StartCoroutine(switch_cr());
		}
	}

	private IEnumerator switch_cr()
	{
		state = States.Switch;
		LevelProperties.SnowCult.Face p = base.properties.CurrentState.face;
		bool flippedY = false;
		char c = faceOrientation.PopLetter();
		if ((c == 'U' && base.transform.parent.localScale.y == -1f) || (c == 'D' && base.transform.parent.localScale.y == 1f))
		{
			flippedY = true;
		}
		bool isFront = false;
		string triggerName;
		string stateName;
		if (UnityEngine.Random.Range(0f, 1f) < 0.25f)
		{
			triggerName = ((!flippedY) ? "FrontSwap" : "FrontSwapFlip");
			stateName = ((!flippedY) ? "SideSwapFront" : "SideSwapFrontFlip");
			isFront = true;
		}
		else
		{
			triggerName = ((!flippedY) ? "BackSwap" : "BackSwapFlip");
			stateName = ((!flippedY) ? "SideSwapBack" : "SideSwapBackFlip");
		}
		base.animator.SetTrigger(triggerName);
		yield return base.animator.WaitForAnimationToEnd(this, "Idle");
		if (isFront)
		{
			rend.sortingLayerName = "Foreground";
		}
		if (flippedY)
		{
			rightSideUp = !rightSideUp;
		}
		yield return base.animator.WaitForAnimationToEnd(this, stateName);
		yield return new WaitForEndOfFrame();
		rend.sortingLayerName = "Default";
		state = States.Idle;
	}

	private void FlipParentTransformX()
	{
		onRight = !onRight;
		ChangeSide();
	}

	private void FlipParentTransformXY()
	{
		base.transform.parent.SetScale(null, rightSideUp ? 1 : (-1));
		FlipParentTransformX();
	}

	private void ChangeSide()
	{
		base.transform.parent.SetScale((!onRight) ? (0f - scale.x) : scale.x);
	}

	public void StartEyeAttack()
	{
		base.animator.SetTrigger("EyeAttack");
		state = States.Eye;
	}

	private void aniEvent_LaunchEye()
	{
		StartCoroutine(eye_attack_cr());
	}

	private IEnumerator eye_attack_cr()
	{
		LevelProperties.SnowCult.EyeAttack p = base.properties.CurrentState.eyeAttack;
		activeEyeProjectile = eyeProjectile.Spawn();
		activeEyeProjectile.Init(eyeRoot.position, mouthRoot.position, onRight, rightSideUp, p);
		activeEyeProjectile.main = this;
		while (!activeEyeProjectile.readyToOpenMouth)
		{
			yield return null;
		}
		base.animator.SetTrigger("EyeAttackOpenMouth");
		while (!activeEyeProjectile.readyToCloseMouth)
		{
			yield return null;
		}
		base.animator.Play("EyeAttackEnd");
		base.animator.Update(0f);
		SFX_SNOWCULT_JackFrostEyeballReturn();
		yield return base.animator.WaitForAnimationToEnd(this, "EyeAttackEnd");
		yield return CupheadTime.WaitForSeconds(this, p.attackDelay);
		state = States.Idle;
	}

	private void AniEvent_RemoveEye()
	{
		activeEyeProjectile.ReturnToSnowflake();
	}

	public void StartShardAttack()
	{
		StartCoroutine(shard_attack_cr());
	}

	private IEnumerator shard_attack_cr()
	{
		state = States.Shard;
		LevelProperties.SnowCult.ShardAttack p = base.properties.CurrentState.shardAttack;
		float degrees = 360f / (float)p.shardNumber;
		float loopSizeX = p.circleSizeX;
		float loopSizeY = p.circleSizeY;
		SnowCultLevelShard[] shards = new SnowCultLevelShard[p.shardNumber];
		string[] angleOffsetString = p.angleOffset.Split(',');
		float angleOffset = shardAngleOffsetString.PopFloat();
		List<float> angleList = new List<float>();
		for (int k = 0; k < p.shardNumber; k++)
		{
			angleList.Add((degrees * (float)k + angleOffset) % 360f);
		}
		angleList.Sort((float a, float b) => ((a + 90f) % 360f).CompareTo((b + 90f) % 360f));
		iceCreamGhostRenderer.sortingOrder = -12;
		base.animator.SetTrigger("IceCreamAttack");
		yield return base.animator.WaitForAnimationToStart(this, "IceCream");
		SFX_SNOWCULT_JackFrostIcecream();
		YieldInstruction wait = new WaitForFixedUpdate();
		int count = 0;
		float sparkleDelay = UnityEngine.Random.Range(0.1f, 0.3f);
		while (count < p.shardNumber)
		{
			float normalizedAngle = Mathf.InverseLerp(0.11627907f, 0.7906977f, base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
			sparkleDelay -= CupheadTime.FixedDelta;
			if (sparkleDelay <= 0f)
			{
				float f = (normalizedAngle + 0.25f) % 1f * (float)Math.PI * 2f;
				iceCreamSparkle.Create(platformPivotPoint.position + p.circleOffsetY * Vector3.up + Vector3.right * base.transform.parent.localScale.x * Mathf.Sin(f) * loopSizeX + Vector3.down * base.transform.parent.localScale.y * Mathf.Cos(f) * loopSizeY);
				sparkleDelay += UnityEngine.Random.Range(0.1f, 0.3f);
			}
			if ((float)count < normalizedAngle * (float)p.shardNumber)
			{
				float num = angleList[count];
				if (base.transform.parent.localScale.x < 0f)
				{
					num = 360f - num;
				}
				if (base.transform.parent.localScale.y < 0f)
				{
					num = (num + (90f - num) * 2f) % 360f;
				}
				SnowCultLevelShard snowCultLevelShard = shardPrefab.Spawn();
				snowCultLevelShard.Init(platformPivotPoint.position + p.circleOffsetY * Vector3.up + Vector3.forward * count * 0.001f, num, loopSizeX, loopSizeY, p);
				shards[count] = snowCultLevelShard;
				count++;
			}
			yield return wait;
		}
		yield return CupheadTime.WaitForSeconds(this, p.warningLength);
		for (int l = 0; l < shards.Length; l++)
		{
			shards[l].Appear();
		}
		yield return CupheadTime.WaitForSeconds(this, p.shardHesitation);
		if (isClockwise)
		{
			for (int j = shards.Length - 1; j >= 0; j--)
			{
				yield return CupheadTime.WaitForSeconds(this, p.shardDelay);
				if (shards[j] != null)
				{
					shards[j].LaunchProjectile();
				}
			}
		}
		else
		{
			for (int i = 0; i < shards.Length; i++)
			{
				yield return CupheadTime.WaitForSeconds(this, p.shardDelay);
				if (shards[i] != null)
				{
					shards[i].LaunchProjectile();
				}
			}
		}
		yield return CupheadTime.WaitForSeconds(this, p.attackDelay);
		state = States.Idle;
		yield return null;
	}

	private void AniEvent_SetGhostLayerBehindSnowflakeMiddleLayer()
	{
		iceCreamGhostRenderer.sortingOrder = -17;
	}

	public void StartMouthShot()
	{
		StartCoroutine(split_shot_cr());
	}

	private IEnumerator split_shot_cr()
	{
		LevelProperties.SnowCult.SplitShot p = base.properties.CurrentState.splitShot;
		state = States.SplitShot;
		float posX = 0f;
		float posY = 0f;
		int timesToShoot = shotCoord.SubStringLength();
		base.animator.SetBool("SplitShot", value: true);
		yield return base.animator.WaitForAnimationToStart(this, "SplitShotStart");
		SFX_SNOWCULT_JackFrostSplitshotHandwavingStart();
		yield return base.animator.WaitForAnimationToStart(this, "SplitShotAnti");
		SFX_SNOWCULT_JackFrostSplitshotHandwavingLoop();
		for (int i = 0; i < timesToShoot; i++)
		{
			posY = shotCoord.PopFloat();
			posX = ((!onRight) ? 640 : (-640));
			Vector3 pos = new Vector3(posX, base.transform.position.y + posY);
			Vector3 dir = pos - splitShotRoot.position;
			SnowCultLevelSplitShotBullet splitShot = ((splitShotPink.PopLetter() != 'P') ? mouthPrefab.Spawn() : mouthPinkPrefab.Spawn());
			splitShot.Init(splitShotRoot.position, MathUtils.DirectionToAngle(dir), p.shotSpeed, p.shatterCount, p.spreadAngle, p);
			splitShot.transform.localScale = new Vector3(base.transform.parent.localScale.x, 1f);
			splitShot.main = this;
			yield return CupheadTime.WaitForSeconds(this, p.shotDelay - 0.45f);
			if ((bool)splitShot)
			{
				splitShot.Grow();
			}
			yield return CupheadTime.WaitForSeconds(this, 0.45f);
			if ((bool)splitShot)
			{
				base.animator.SetTrigger("SplitShotFire");
				SFX_SNOWCULT_JackFrostSplitshotBucketLaunch();
				while (!fireSplitShot)
				{
					yield return null;
				}
				fireSplitShot = false;
				if ((bool)splitShot)
				{
					splitShot.Fire();
				}
			}
		}
		base.animator.SetBool("SplitShot", value: false);
		SFX_SNOWCULT_JackFrostSplitshotHandwavingLoopStop();
		yield return CupheadTime.WaitForSeconds(this, p.attackDelay);
		state = States.Idle;
		yield return null;
	}

	private void AniEvent_FireSplitShot()
	{
		fireSplitShot = true;
	}

	private void OnBossDeath()
	{
		dead = true;
		base.transform.parent.SetScale(null, 1f);
		base.animator.Play((!rightSideUp) ? "FlipDeath" : "Death");
	}

	private void EnableWizardDeathAnimation()
	{
		wizardDeath.SetActive(value: true);
	}

	private void AnimationEvent_SFX_SNOWCULT_JackFrostIntroThumblick()
	{
		AudioManager.Play("sfx_dlc_snowcult_p3_snowflake_intro_thumblick");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p3_snowflake_intro_thumblick");
	}

	private void AnimationEvent_SFX_SNOWCULT_JackFrostEyeballAttack()
	{
		AudioManager.Play("sfx_dlc_snowcult_p3_snowflake_eyeball_attack");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p3_snowflake_eyeball_attack");
	}

	private void SFX_SNOWCULT_JackFrostEyeballReturn()
	{
		AudioManager.Play("sfx_dlc_snowcult_p3_snowflake_eyeball_return");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p3_snowflake_eyeball_return");
	}

	private void SFX_SNOWCULT_JackFrostIcecream()
	{
		AudioManager.Play("sfx_dlc_snowcult_p3_snowflake_icecreamattack");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p3_snowflake_icecreamattack");
	}

	private void AnimationEvent_SFX_SNOWCULT_JackFrostSideSwap()
	{
		AudioManager.Play("sfx_dlc_snowcult_p3_snowflake_sideswap");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p3_snowflake_sideswap");
	}

	private void SFX_SNOWCULT_JackFrostSplitshotHandwavingLoop()
	{
		AudioManager.PlayLoop("sfx_dlc_snowcult_p3_snowflake_splitshot_handwaving_attack_loop");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p3_snowflake_splitshot_handwaving_attack_loop");
	}

	private void SFX_SNOWCULT_JackFrostSplitshotHandwavingLoopStop()
	{
		AudioManager.Stop("sfx_dlc_snowcult_p3_snowflake_splitshot_handwaving_attack_loop");
	}

	private void SFX_SNOWCULT_JackFrostSplitshotHandwavingStart()
	{
		AudioManager.Play("sfx_dlc_snowcult_p3_snowflake_splitshot_handwaving_attack_start");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p3_snowflake_splitshot_handwaving_attack_start");
	}

	private void SFX_SNOWCULT_JackFrostSplitshotBucketLaunch()
	{
		AudioManager.Play("sfx_dlc_snowcult_p3_snowflake_splitshot_attack_bucket_launch");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p3_snowflake_splitshot_attack_bucket_launch");
	}

	private void AnimationEvent_SFX_SNOWCULT_JackFrostDeath()
	{
		AudioManager.Play("sfx_dlc_snowcult_p3_snowflake_death");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p3_snowflake_death");
	}
}
