using System;
using System.Collections;
using UnityEngine;

public class OldManLevelGnomeLeader : LevelProperties.OldMan.Entity
{
	private const float DROP_Y = 5f;

	private const int BULLET_COUNT = 4;

	private const float SPIT_DELAY = 13f / 24f;

	private const float SPIT_DISTANCE_OFFSET = 250f;

	private const float CLOSE_SPIT_ANIM_RANGE = 350f;

	[SerializeField]
	private OldManLevelParryThermometer parryThermometer;

	public OldManLevelSplashHandler splashHandler;

	[SerializeField]
	private GameObject pit;

	[SerializeField]
	private Transform[] spitRoots;

	[SerializeField]
	private Animator spitVFXAnimator;

	[SerializeField]
	private OldManLevelGnomeProjectile projectilePrefab;

	[SerializeField]
	private OldManLevelStomachPlatform stomachPlatformPrefab;

	private OldManLevelStomachPlatform[] stomachPlatforms;

	private int currentTongue = -1;

	[SerializeField]
	public Transform[] platformPositions;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	private AbstractPlayerController player;

	private bool isAlive;

	private bool movingRight;

	private bool readyToSpit;

	private bool spitFrame;

	private bool spitting;

	private bool restartSequence;

	private int[] sequence = new int[5];

	private int sequenceIndex;

	private int sequenceMainIndex;

	private float locationTime;

	private float locationStart;

	private float locationEnd;

	private float timeForScreenCross;

	private bool turnTrigger = true;

	private bool turning;

	private PatternString parryString;

	private bool isBehind;

	private float screenEdgeOffset = 200f;

	[SerializeField]
	private float baseHeight = 188f;

	[SerializeField]
	private float topAnimSpeed = 30f;

	[SerializeField]
	private float heightRange;

	[SerializeField]
	private Collider2D coll;

	private float SFXLoopVolume;

	protected override void Awake()
	{
		base.Awake();
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		base.transform.SetPosition(null, baseHeight + Mathf.Sin(GetPosition() * (float)Math.PI) * heightRange);
	}

	private void Update()
	{
		NontargetablePlatformCount();
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	public override void LevelInit(LevelProperties.OldMan properties)
	{
		base.LevelInit(properties);
		parryThermometer.gameObject.SetActive(value: false);
		parryString = new PatternString(properties.CurrentState.gnomeLeader.shotParryString);
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		base.properties.DealDamage(info.damage);
		if (base.properties.CurrentHealth <= 0f)
		{
			StartDeath();
		}
	}

	private void StartDeath()
	{
		isAlive = false;
		coll.enabled = false;
		StopAllCoroutines();
		base.animator.SetTrigger("Dead");
		SFX_Death();
		AudioManager.Stop("sfx_dlc_omm_p3_ulcer_movement_loop");
	}

	public void StartGnomeLeader()
	{
		LevelProperties.OldMan.GnomeLeader gnomeLeader = base.properties.CurrentState.gnomeLeader;
		isAlive = true;
		pit.SetActive(value: true);
		stomachPlatforms = new OldManLevelStomachPlatform[platformPositions.Length];
		for (int i = 0; i < stomachPlatforms.Length; i++)
		{
			stomachPlatforms[i] = UnityEngine.Object.Instantiate(stomachPlatformPrefab);
			stomachPlatforms[i].transform.position = platformPositions[i].position;
			if (i < 3)
			{
				stomachPlatforms[i].FlipX();
			}
			stomachPlatforms[i].sparkAnimator = platformPositions[i].GetComponent<Animator>();
			stomachPlatforms[i].main = this;
		}
		StartCoroutine(moving_cr());
	}

	public float GetPosition()
	{
		return Mathf.InverseLerp((float)Level.Current.Right - screenEdgeOffset, (float)Level.Current.Left + screenEdgeOffset, base.transform.position.x);
	}

	private IEnumerator moving_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		movingRight = MathUtils.RandomBool();
		AudioManager.Play("sfx_dlc_omm_p3_ulcer_introlaugh");
		base.animator.Play((!movingRight) ? "IntroRight" : "IntroLeft");
		yield return wait;
		if (!movingRight)
		{
			while (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.99f)
			{
				yield return null;
			}
			base.animator.Play("Idle");
			base.animator.Update(0f);
			base.transform.localScale = new Vector3(1f, 1f);
		}
		else
		{
			yield return base.animator.WaitForAnimationToStart(this, "Idle");
		}
		SFX_MoveLoop();
		StartCoroutine(gnome_leader_cr());
		timeForScreenCross = base.properties.CurrentState.gnomeLeader.bossMoveTime;
		locationEnd = 0f;
		AnimationHelper animHelper = base.animator.GetComponent<AnimationHelper>();
		while (true)
		{
			locationTime = 0f;
			locationStart = base.transform.position.x;
			if (movingRight)
			{
				locationEnd = (float)Level.Current.Right - screenEdgeOffset;
			}
			else
			{
				locationEnd = (float)Level.Current.Left + screenEdgeOffset;
			}
			while (locationTime < timeForScreenCross)
			{
				base.transform.SetPosition(GetXPositionAtTimeValue(locationTime), baseHeight + Mathf.Sin(GetPosition() * (float)Math.PI) * heightRange);
				base.transform.SetEulerAngles(null, null, Mathf.Lerp((!movingRight) ? (-7) : 7, (!movingRight) ? 7 : (-7), locationTime / timeForScreenCross));
				locationTime += CupheadTime.FixedDelta;
				if (turnTrigger && locationTime / timeForScreenCross > 0.8f)
				{
					base.animator.SetTrigger("OnTurn");
					turning = true;
					turnTrigger = false;
				}
				if (PauseManager.state != PauseManager.State.Paused)
				{
					float num = Mathf.Sin(Mathf.InverseLerp(locationStart, locationEnd, base.transform.position.x) * (float)Math.PI);
					animHelper.Speed = 1f + num * (topAnimSpeed / 24f - 1f);
					SFXLoopVolume = 0.0001f + num * 0.3f;
					if (!spitting)
					{
						AudioManager.FadeSFXVolume("sfx_dlc_omm_p3_ulcer_movement_loop", SFXLoopVolume, 1E-05f);
					}
				}
				yield return wait;
			}
			turnTrigger = true;
			base.transform.SetPosition(locationEnd);
			movingRight = !movingRight;
			yield return null;
		}
	}

	private float GetXPositionAtTimeValue(float time)
	{
		if (time > timeForScreenCross)
		{
			float value = time % timeForScreenCross / timeForScreenCross;
			return EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, locationEnd, locationStart, value);
		}
		float value2 = time / timeForScreenCross;
		return EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, locationStart, locationEnd, value2);
	}

	private void AniEvent_Turn()
	{
		base.transform.SetScale(0f - base.transform.localScale.x);
		turning = false;
	}

	private int NontargetablePlatformCount()
	{
		int num = 0;
		for (int i = 0; i < stomachPlatforms.Length; i++)
		{
			if (!stomachPlatforms[i].isActivated || stomachPlatforms[i].isTargeted)
			{
				num++;
			}
		}
		return num;
	}

	private IEnumerator gnome_leader_cr()
	{
		LevelProperties.OldMan.GnomeLeader p = base.properties.CurrentState.gnomeLeader;
		PatternString platformCountToRemove = new PatternString(p.platformParryString);
		PatternString shotDelayString = new PatternString(p.shotDelayString);
		sequenceMainIndex = UnityEngine.Random.Range(0, p.shotPlatformString.Length);
		int numOfPlatformsToDestroy2 = 0;
		int platformToTarget2 = 0;
		bool projectileSpawnsParryable3 = false;
		while (isAlive)
		{
			restartSequence = false;
			projectileSpawnsParryable3 = false;
			currentTongue = -1;
			sequenceMainIndex = (sequenceMainIndex + 1) % p.shotPlatformString.Length;
			PatternString shotPlatformString = new PatternString(p.shotPlatformString[sequenceMainIndex]);
			shotPlatformString.SetSubStringIndex(-1);
			sequenceIndex = 0;
			for (int i = 0; i < 5; i++)
			{
				sequence[i] = shotPlatformString.PopInt();
			}
			numOfPlatformsToDestroy2 = platformCountToRemove.PopInt();
			while (!restartSequence)
			{
				yield return CupheadTime.WaitForSeconds(this, shotDelayString.PopFloat());
				if (NontargetablePlatformCount() < 5 && !restartSequence)
				{
					while (turning)
					{
						yield return null;
					}
					base.animator.SetTrigger("Spit");
					int count = 0;
					do
					{
						platformToTarget2 = sequence[sequenceIndex];
						sequenceIndex = (sequenceIndex + 1) % 5;
						count++;
					}
					while ((!stomachPlatforms[platformToTarget2].isActivated || stomachPlatforms[platformToTarget2].isTargeted) && count < 5);
					if (currentTongue == -1 && NontargetablePlatformCount() >= numOfPlatformsToDestroy2)
					{
						projectileSpawnsParryable3 = true;
						currentTongue = platformToTarget2;
						StartCoroutine(wait_to_parry_cr());
					}
					else
					{
						projectileSpawnsParryable3 = false;
					}
					SFX_PreSpit();
					while (!readyToSpit)
					{
						yield return null;
					}
					yield return StartCoroutine(shoot_cr(stomachPlatforms[platformToTarget2], projectileSpawnsParryable3));
				}
			}
			yield return CupheadTime.WaitForSeconds(this, 0.1f);
		}
	}

	private IEnumerator wait_to_parry_cr()
	{
		while (!parryThermometer.isActivated)
		{
			yield return null;
		}
		OldManLevelStomachPlatform[] array = stomachPlatforms;
		foreach (OldManLevelStomachPlatform oldManLevelStomachPlatform in array)
		{
			oldManLevelStomachPlatform.ActivatePlatform();
		}
		restartSequence = true;
	}

	public void SpawnParryable(Vector3 spawnPosition)
	{
		parryThermometer.transform.position = spawnPosition;
		parryThermometer.gameObject.SetActive(value: true);
	}

	private IEnumerator shoot_cr(OldManLevelStomachPlatform selectedPlatform, bool projectileSpawnsParryable)
	{
		float predictedPos = GetXPositionAtTimeValue(locationTime + 13f / 24f);
		isBehind = false;
		bool willBeMovingRight = movingRight;
		if (locationTime + 13f / 24f > timeForScreenCross)
		{
			willBeMovingRight = !willBeMovingRight;
		}
		if (willBeMovingRight)
		{
			isBehind = predictedPos + 250f > selectedPlatform.transform.position.x;
		}
		else
		{
			isBehind = predictedPos - 250f < selectedPlatform.transform.position.x;
		}
		if (turning)
		{
			isBehind = !isBehind;
		}
		string animationName;
		if (Mathf.Abs(predictedPos - selectedPlatform.transform.position.x) < 350f)
		{
			base.animator.SetTrigger((!isBehind) ? "SpitForwardClose" : "SpitBehindClose");
			animationName = ((!isBehind) ? "Spit_Forward_Close" : "Spit_Behind_Close");
			spitVFXAnimator.transform.localPosition = spitRoots[0].transform.localPosition;
		}
		else
		{
			base.animator.SetTrigger((!isBehind) ? "SpitForward" : "SpitBehind");
			animationName = ((!isBehind) ? "Spit_Forward" : "Spit_Behind");
			spitVFXAnimator.transform.localPosition = spitRoots[1].transform.localPosition;
		}
		yield return base.animator.WaitForAnimationToStart(this, animationName);
		spitting = true;
		AudioManager.FadeSFXVolume("sfx_dlc_omm_p3_ulcer_movement_loop", Mathf.Min(0.25f, SFXLoopVolume), 0.25f);
		SFX_StartSpit();
		readyToSpit = false;
		while (!spitFrame)
		{
			yield return null;
		}
		LevelProperties.OldMan.GnomeLeader p = base.properties.CurrentState.gnomeLeader;
		spitVFXAnimator.transform.localPosition = new Vector3(Mathf.Abs(spitVFXAnimator.transform.localPosition.x) * (float)(isBehind ? 1 : (-1)), spitVFXAnimator.transform.localPosition.y);
		spitVFXAnimator.transform.localScale = new Vector3(Mathf.Sign(spitVFXAnimator.transform.localPosition.x), 1f);
		Vector3 startPos = spitVFXAnimator.transform.position;
		float x = selectedPlatform.transform.position.x - startPos.x;
		float y = selectedPlatform.transform.position.y - startPos.y;
		float timeToApex = p.shotApexTime;
		float height = p.shotApexHeight;
		float apexTime2 = timeToApex * timeToApex;
		float g = -2f * height / apexTime2;
		float viY = 2f * height / timeToApex;
		float viX2 = viY * viY;
		float sqrtRooted = viX2 + 2f * g * y;
		float tEnd2 = (0f - viY + Mathf.Sqrt(sqrtRooted)) / g;
		float tEnd3 = (0f - viY - Mathf.Sqrt(sqrtRooted)) / g;
		float tEnd = Mathf.Max(tEnd2, tEnd3);
		float velocityX = x / tEnd;
		Vector3 speed = new Vector3(velocityX, viY);
		selectedPlatform.Anticipation();
		spitVFXAnimator.Play("SpitSmoke");
		spitVFXAnimator.Update(0f);
		OldManLevelGnomeProjectile projectile = projectilePrefab.Spawn();
		projectile.Init(startPos, speed, g, projectileSpawnsParryable, parryString.PopLetter() == 'P' && !projectileSpawnsParryable, selectedPlatform);
		SFX_SpawnProjectile();
		spitFrame = false;
		AudioManager.FadeSFXVolume("sfx_dlc_omm_p3_ulcer_movement_loop", SFXLoopVolume, 1f);
		spitting = false;
	}

	private void AniEvent_ReadyToSpit()
	{
		readyToSpit = true;
	}

	private void AniEvent_Shoot()
	{
		spitFrame = true;
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		if (platformPositions != null)
		{
			Transform[] array = platformPositions;
			foreach (Transform transform in array)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(transform.position, 50f);
			}
		}
	}

	private void SFX_MoveLoop()
	{
		AudioManager.PlayLoop("sfx_dlc_omm_p3_ulcer_movement_loop");
		emitAudioFromObject.Add("sfx_dlc_omm_p3_ulcer_movement_loop");
	}

	private void SFX_PreSpit()
	{
		AudioManager.Play("sfx_dlc_omm_p3_ulcer_bonespitpre");
		emitAudioFromObject.Add("sfx_dlc_omm_p3_ulcer_bonespitpre");
	}

	private void SFX_StartSpit()
	{
		AudioManager.Play("sfx_dlc_omm_p3_ulcer_spitbonevocal");
		emitAudioFromObject.Add("sfx_dlc_omm_p3_ulcer_spitbonevocal");
	}

	private void SFX_SpawnProjectile()
	{
		AudioManager.Play("sfx_dlc_omm_p3_ulcerspitbone");
		emitAudioFromObject.Add("sfx_dlc_omm_p3_ulcerspitbone");
	}

	private void SFX_Death()
	{
		AudioManager.Play("sfx_dlc_omm_p3_ulcer_deathvocal");
		emitAudioFromObject.Add("sfx_dlc_omm_p3_ulcer_deathvocal");
		AudioManager.FadeSFXVolume("sfx_dlc_omm_p3_stomachacid_amb_loop", 0f, 2f);
	}
}
