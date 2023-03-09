using System.Collections;
using UnityEngine;

public class FlyingCowboyLevelMeat : LevelProperties.FlyingCowboy.Entity
{
	public enum MeatPhase
	{
		Can,
		Sausage,
		Switching
	}

	public enum SausageType
	{
		H1,
		H2,
		H3,
		H4,
		L5,
		U1,
		U2,
		U3,
		D1,
		D2,
		D3
	}

	private static readonly float SausageLinkWidth = 120f;

	private static readonly SausageType[] SausageTypeAny = new SausageType[16]
	{
		SausageType.H1,
		SausageType.H2,
		SausageType.H3,
		SausageType.H4,
		SausageType.L5,
		SausageType.L5,
		SausageType.L5,
		SausageType.L5,
		SausageType.L5,
		SausageType.L5,
		SausageType.L5,
		SausageType.L5,
		SausageType.L5,
		SausageType.U1,
		SausageType.U2,
		SausageType.U3
	};

	private static readonly SausageType[] SausageTypeEnd = new SausageType[13]
	{
		SausageType.H1,
		SausageType.H2,
		SausageType.H3,
		SausageType.H4,
		SausageType.L5,
		SausageType.L5,
		SausageType.L5,
		SausageType.L5,
		SausageType.L5,
		SausageType.L5,
		SausageType.L5,
		SausageType.L5,
		SausageType.L5
	};

	private static readonly SausageType[] SausageTypeDown = new SausageType[3]
	{
		SausageType.D1,
		SausageType.D2,
		SausageType.D3
	};

	private static readonly float[] SausageGapWidths = new float[3] { 146f, 189f, 286f };

	private static readonly string[] SausageGapAnimationNames = new string[3] { "String1", "String2", "String3" };

	private static readonly int LowSausageLinkSortingOrder = 20;

	private static readonly int MidSausageLinkSortingOrder = 40;

	private static readonly int HighSausageLinkSortingOrder = 60;

	[Header("Sausage")]
	[SerializeField]
	private Transform sausageSpawnPosition;

	[SerializeField]
	private FlyingCowboyLevelBeans beansPrefab;

	[SerializeField]
	private FlyingCowboyLevelSpinningBullet sausageRunSpitBullet;

	[SerializeField]
	private Effect sausageRunSpitBulletEffect;

	[SerializeField]
	private Transform runTopSpitBulletSpawn;

	[SerializeField]
	private Transform runTopSpitBulletEffectSpawn;

	[SerializeField]
	private Transform runBottomSpitBulletSpawn;

	[SerializeField]
	private Transform runBottomSpitBulletEffectSpawn;

	[SerializeField]
	private Vector2 sausageWobbleRadius;

	[SerializeField]
	private Vector2 sausageWobbleDuration;

	[Header("Can")]
	[SerializeField]
	private Transform canTransform;

	[SerializeField]
	private GameObject sausageTransforms;

	[SerializeField]
	private BasicProjectile canBullet;

	[SerializeField]
	private Effect canBulletMuzzleFX;

	[SerializeField]
	private Transform bulletRoot;

	[SerializeField]
	private Transform shadowTransform;

	[SerializeField]
	private BasicProjectile sausage;

	[SerializeField]
	private Transform sausageLinkSqueezePoint;

	[SerializeField]
	private Transform sausageHolderA;

	[SerializeField]
	private Transform sausageHolderB;

	[SerializeField]
	private Transform nextBulletSpawnPointA;

	[SerializeField]
	private Transform nextBulletSpawnPointB;

	[SerializeField]
	private FlyingCowboyFloatingSausages floatingSausage;

	[SerializeField]
	private Transform floatingSausageSpawnPointLeft;

	[SerializeField]
	private Transform floatingSausageSpawnPointRight;

	[SerializeField]
	private BasicProjectile sausageString;

	[SerializeField]
	private TriggerZone[] beanCanTriggerZones;

	[SerializeField]
	private Effect sausageDeathEffect;

	[SerializeField]
	private Effect sausageStringDeathEffect;

	private AbstractPlayerController player;

	private MeatPhase meatPhase;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	private PatternString runningSpitBulletParryPattern;

	private PatternString sausageTimeToMoveString;

	private PatternString spitBulletParryString;

	private bool isFlying;

	private bool waitingToShoot;

	private bool isDead;

	private bool canBulletsTriggered;

	private int currentSausageLinkSortingOrderA = MidSausageLinkSortingOrder;

	private int currentSausageLinkSortingOrderB = MidSausageLinkSortingOrder + 1;

	private void Start()
	{
		Level.Current.OnBossDeathExplosionsEvent += onBossDeathExplosionsEventHandler;
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		nextBulletSpawnPointA.position = sausageHolderA.position;
		nextBulletSpawnPointB.position = sausageHolderB.position;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	public override void LevelInit(LevelProperties.FlyingCowboy properties)
	{
		base.LevelInit(properties);
		runningSpitBulletParryPattern = new PatternString(properties.CurrentState.sausageRun.bulletParry);
		sausageTimeToMoveString = new PatternString(properties.CurrentState.sausageRun.timeTillSwitch);
		spitBulletParryString = new PatternString(properties.CurrentState.can.bulletParryString);
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
		if (meatPhase == MeatPhase.Can)
		{
			AudioManager.Play("sfx_dlc_cowgirl_p3_can_damage_metalimpact");
		}
		base.properties.DealDamage(info.damage);
		if (!isDead && base.properties.CurrentHealth <= 0f)
		{
			die();
		}
	}

	public void SelectPhase(MeatPhase meatPhase)
	{
		base.gameObject.SetActive(value: true);
		this.meatPhase = meatPhase;
		switch (meatPhase)
		{
		case MeatPhase.Can:
			Can();
			break;
		case MeatPhase.Sausage:
			Sausage();
			break;
		}
	}

	public void Sausage()
	{
		Vector3 position = sausageSpawnPosition.position;
		position.y = 42f;
		base.transform.position = position;
		StartCoroutine(sausage_intro_cr());
	}

	private IEnumerator sausage_intro_cr()
	{
		LevelProperties.FlyingCowboy.SausageRun p = base.properties.CurrentState.sausageRun;
		yield return CupheadTime.WaitForSeconds(this, p.mirrorTime);
		base.animator.SetTrigger("Continue");
		yield return base.animator.WaitForAnimationToEnd(this, "Sg_Mirror_Cont");
		StartCoroutine(beans_cr());
		if (p.shootBullets)
		{
			StartCoroutine(sausageTurret_cr());
		}
		StartCoroutine(sausageSwitchHeight_cr());
	}

	private void animationEvent_RepositionSausage()
	{
		StartCoroutine(repositionSausage_cr());
	}

	private IEnumerator repositionSausage_cr()
	{
		AudioManager.PlayLoop("sfx_dlc_cowgirl_p3_sausage_footstep_loop");
		float startX = base.transform.position.x;
		float elapsedTime = 0f;
		while (meatPhase == MeatPhase.Sausage && elapsedTime < 4f)
		{
			yield return null;
			elapsedTime += (float)CupheadTime.Delta;
			Vector3 position = base.transform.position;
			position.x = Mathf.Lerp(startX, 340f, elapsedTime / 4f);
			base.transform.position = position;
		}
		StartCoroutine(wobble_cr(base.transform, sausageWobbleRadius, sausageWobbleDuration, base.transform.position, MeatPhase.Sausage, useLocal: false, easeWobble: false));
	}

	private IEnumerator sausageSwitchHeight_cr()
	{
		while (true)
		{
			float time = sausageTimeToMoveString.PopFloat();
			yield return CupheadTime.WaitForSeconds(this, time);
			bool newFlyingStatus = !isFlying;
			base.animator.SetBool("IsFlying", newFlyingStatus);
			if (newFlyingStatus)
			{
				AudioManager.Stop("sfx_dlc_cowgirl_p3_sausage_footstep_loop");
			}
			string transitionAnimation = ((!newFlyingStatus) ? "Sg_Fly_To_Run" : "Sg_Run_To_Fly");
			yield return base.animator.WaitForAnimationToEnd(this, transitionAnimation);
			if (!newFlyingStatus)
			{
				AudioManager.PlayLoop("sfx_dlc_cowgirl_p3_sausage_footstep_loop");
			}
			isFlying = newFlyingStatus;
		}
	}

	private IEnumerator beans_cr()
	{
		float startBeansHealthPercentage = base.properties.CurrentState.healthTrigger;
		float endBeansPercentage = base.properties.GetNextStateHealthTrigger();
		float startBeansHealth = startBeansHealthPercentage * base.properties.TotalHealth;
		float endBeansHealth = endBeansPercentage * base.properties.TotalHealth;
		float seventyFivePercentof = startBeansHealth + (endBeansHealth - startBeansHealth) * 0.75f;
		LevelProperties.FlyingCowboy.SausageRun p = base.properties.CurrentState.sausageRun;
		PatternString groupDelayPattern = new PatternString(p.groupBeansDelayString);
		PatternString positionPattern = new PatternString(p.beansPositionString, randomizeMain: true, randomizeSub: false);
		PatternString extendTimerPattern = new PatternString(p.beansExtendTimer);
		float positionX = 690f;
		while (meatPhase == MeatPhase.Sausage)
		{
			string[] positionValues = positionPattern.GetString().Split(':');
			positionPattern.IncrementString();
			Parser.FloatTryParse(positionValues[0], out var positionY);
			bool pointingUp = positionValues[1] == "U";
			float currentPercentage = (base.properties.CurrentHealth - startBeansHealth) / (seventyFivePercentof - startBeansHealth);
			float speed = Mathf.Lerp(p.beansSpeed.min, p.beansSpeed.max, currentPercentage);
			FlyingCowboyLevelBeans beans = beansPrefab.Spawn();
			beans.Init(new Vector3(positionX, positionY), pointingUp, speed, extendTimerPattern.PopFloat());
			if (positionPattern.GetSubStringIndex() != 0)
			{
				float spawnDelay = Mathf.Lerp(p.beansSpawnDelay.max, p.beansSpawnDelay.min, currentPercentage);
				yield return CupheadTime.WaitForSeconds(this, spawnDelay);
			}
			else
			{
				yield return CupheadTime.WaitForSeconds(this, groupDelayPattern.PopFloat());
			}
		}
	}

	private IEnumerator sausageTurret_cr()
	{
		LevelProperties.FlyingCowboy.SausageRun p = base.properties.CurrentState.sausageRun;
		AbstractPlayerController player = PlayerManager.GetNext();
		while (meatPhase == MeatPhase.Sausage)
		{
			base.animator.SetTrigger("OnShoot");
			waitingToShoot = true;
			while (waitingToShoot)
			{
				yield return null;
			}
			yield return CupheadTime.WaitForSeconds(this, p.bulletDelay);
		}
	}

	private void aniEvent_ShootTurret()
	{
		player = PlayerManager.GetNext();
		LevelProperties.FlyingCowboy.SausageRun sausageRun = base.properties.CurrentState.sausageRun;
		Vector3 vector = ((!isFlying) ? runBottomSpitBulletSpawn.position : runTopSpitBulletSpawn.position);
		Vector3 position = ((!isFlying) ? runBottomSpitBulletEffectSpawn.position : runTopSpitBulletEffectSpawn.position);
		player = PlayerManager.GetNext();
		Vector3 vector2 = player.transform.position - vector;
		float num;
		for (num = MathUtils.DirectionToAngle(vector2); num < 0f; num += 360f)
		{
		}
		float min;
		float max;
		bool clockwise;
		if (isFlying)
		{
			min = 180f - sausageRun.bulletTopMaxUpAngle;
			max = 180f + sausageRun.bulletTopMaxDownAngle;
			clockwise = sausageRun.bulletTopRotateClockwise;
		}
		else
		{
			min = 180f - sausageRun.bulletBottomMaxUpAngle;
			max = 180f + sausageRun.bulletBottomMaxDownAngle;
			clockwise = sausageRun.bulletBottomRotateClockwise;
		}
		num = Mathf.Clamp(num, min, max);
		vector2 = MathUtilities.AngleToDirection(num);
		sausageRunSpitBullet.Create(vector, sausageRun.bulletSpeed, sausageRun.bulletRotationSpeed, sausageRun.bulletRotationRadius, vector2, clockwise, runningSpitBulletParryPattern.PopLetter() == 'P');
		Effect effect = sausageRunSpitBulletEffect.Create(position);
		if (!isFlying)
		{
			effect.transform.rotation = Quaternion.Euler(0f, 0f, -30f);
		}
		waitingToShoot = false;
	}

	public void Can()
	{
		StartCoroutine(toCan_cr());
	}

	private IEnumerator toCan_cr()
	{
		AudioManager.Stop("sfx_dlc_cowgirl_p3_sausage_footstep_loop");
		base.animator.SetBool("ToCan", value: true);
		yield return base.animator.WaitForNormalizedTime(this, 1f, "SausageToCanEnd", 0, allowEqualTime: true);
		base.animator.Play("CanIntro", 0);
		base.animator.Update(0f);
		StartCoroutine(repositionCan_cr());
		LevelProperties.FlyingCowboy.Can p = base.properties.CurrentState.can;
		StartCoroutine(wobble_cr(canTransform, new Vector2(p.wobbleRadiusX, p.wobbleRadiusY), new Vector2(p.wobbleDurationX, p.wobbleDurationY), canTransform.localPosition, MeatPhase.Can, useLocal: true, easeWobble: true));
	}

	private IEnumerator repositionCan_cr()
	{
		Vector3 startPosition = base.transform.position;
		Vector3 targetPosition = new Vector3(340f, 61f, startPosition.z);
		float elapsedTime = 0f;
		while (elapsedTime < 3f)
		{
			yield return null;
			elapsedTime += (float)CupheadTime.Delta;
			base.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / 3f);
		}
	}

	private void animationEvent_StartSausageLinks()
	{
		AudioManager.PlayLoop("sfx_dlc_cowgirl_p3_sausagemeattin_loop");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p3_sausagemeattin_loop");
		sausageTransforms.SetActive(value: true);
		StartCoroutine(sausageTrain_cr(isTypeA: true));
		StartCoroutine(sausageRotation_cr(sausageHolderA, 0));
		StartCoroutine(sausageTrain_cr(isTypeA: false));
		StartCoroutine(sausageRotation_cr(sausageHolderB, 1));
		if (base.properties.CurrentState.can.shootBullets)
		{
			StartCoroutine(shootCanBullets_cr());
		}
		StartCoroutine(beanCanTriggerZone_cr());
	}

	private void animationEvent_TriggerCanBullets()
	{
		canBulletsTriggered = true;
	}

	private IEnumerator shootCanBullets_cr()
	{
		LevelProperties.FlyingCowboy.Can p = base.properties.CurrentState.can;
		int variant = 0;
		int fxVariant = Random.Range(0, 3);
		PatternString bulletCountPattern = new PatternString(p.bulletCount);
		canBulletsTriggered = false;
		while (meatPhase == MeatPhase.Can)
		{
			yield return CupheadTime.WaitForSeconds(this, p.shotDelay);
			base.animator.SetTrigger("OnShoot");
			while (!canBulletsTriggered)
			{
				yield return null;
			}
			canBulletsTriggered = false;
			Effect muzzleFX = canBulletMuzzleFX.Create(bulletRoot.position);
			muzzleFX.animator.SetInteger("Effect", fxVariant);
			fxVariant = MathUtilities.NextIndex(fxVariant, 3);
			SFX_CanSpitBurningFire();
			while (!canBulletsTriggered)
			{
				yield return null;
			}
			canBulletsTriggered = false;
			int count = bulletCountPattern.PopInt();
			float startAngle = (0f - p.bulletSpreadAngle) * 0.5f;
			float angleIncrement = p.bulletSpreadAngle / (float)(count - 1);
			for (int i = 0; i < count; i++)
			{
				float num = startAngle + angleIncrement * (float)i;
				float rotation = 180f - num;
				BasicProjectile basicProjectile = canBullet.Create(bulletRoot.position, rotation, p.bulletSpeed);
				bool flag = spitBulletParryString.PopLetter() == 'P';
				basicProjectile.SetParryable(flag);
				basicProjectile.animator.SetInteger("Variant", variant);
				basicProjectile.animator.Update(0f);
				basicProjectile.animator.Play(0, 0, Random.Range(0f, 1f));
				basicProjectile.GetComponent<SpriteRenderer>().sortingOrder = i;
				basicProjectile.transform.SetEulerAngles(0f, 0f, 0f - num);
				if (!flag)
				{
					variant = ((variant == 0) ? 1 : 0);
				}
			}
		}
		sausageTransforms.SetActive(value: false);
	}

	private IEnumerator sausageRotation_cr(Transform sausageHolder, int index)
	{
		LevelProperties.FlyingCowboy.Can p = base.properties.CurrentState.can;
		Transform holder = ((index != 0) ? sausageHolderB : sausageHolderA);
		float topAngle = 0f - p.maxSausageAngle;
		float bottomAngle = p.maxSausageAngle;
		bool goingUp = index == 0;
		float startAngle = ((!goingUp) ? bottomAngle : 0f);
		float endAngle = ((!goingUp) ? 0f : topAngle);
		int sortingOffset = index * 100;
		float elapsedTime = 0f;
		WaitForFixedUpdate wait = new WaitForFixedUpdate();
		while (meatPhase == MeatPhase.Can)
		{
			while (elapsedTime < 2f && meatPhase == MeatPhase.Can)
			{
				float t = ((!goingUp) ? (1f - elapsedTime / 2f) : (elapsedTime / 2f));
				float angle = Mathf.Lerp(startAngle, endAngle, t);
				sausageHolder.transform.SetEulerAngles(null, null, angle);
				int sortingOrder = ((angle >= 15f) ? (LowSausageLinkSortingOrder + sortingOffset) : ((!(angle <= -15f)) ? (MidSausageLinkSortingOrder + sortingOffset) : (HighSausageLinkSortingOrder + sortingOffset)));
				int childCount = holder.childCount;
				for (int i = 0; i < childCount; i++)
				{
					Transform child = holder.GetChild(i);
					SpriteRenderer component = child.GetComponent<SpriteRenderer>();
					if (component != null)
					{
						component.sortingOrder = sortingOrder + childCount - i;
					}
				}
				switch (index)
				{
				case 0:
					currentSausageLinkSortingOrderA = sortingOrder;
					break;
				case 1:
					currentSausageLinkSortingOrderB = sortingOrder;
					break;
				}
				elapsedTime += CupheadTime.FixedDelta;
				yield return wait;
			}
			if ((goingUp && startAngle == 0f) || (!goingUp && endAngle == 0f))
			{
				goingUp = !goingUp;
			}
			else if (!goingUp && startAngle == 0f)
			{
				startAngle = bottomAngle;
				endAngle = 0f;
			}
			else if (goingUp && startAngle == bottomAngle)
			{
				startAngle = 0f;
				endAngle = topAngle;
			}
			elapsedTime = 0f;
		}
	}

	private IEnumerator sausageTrain_cr(bool isTypeA)
	{
		LevelProperties.FlyingCowboy.Can p = base.properties.CurrentState.can;
		Transform sausageHolder = ((!isTypeA) ? sausageHolderB : sausageHolderA);
		Transform nextSpawn = ((!isTypeA) ? nextBulletSpawnPointB : nextBulletSpawnPointA);
		string[] sausageMainString = ((!isTypeA) ? p.sausageStringB : p.sausageStringA);
		PatternString sausageAmountPattern = new PatternString(sausageMainString);
		PatternString gapPattern = new PatternString((!isTypeA) ? p.gapDistB : p.gapDistA);
		int sausageCounter = 0;
		int sausageMax = sausageAmountPattern.PopInt();
		SausageType previousSausageType = SausageType.H1;
		FlyingCowboyLevelSausageLink previousSausage = null;
		AudioManager.PlayLoop("sfx_dlc_cowgirl_p3_sausagemeattin_loop");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p3_sausagemeattin_loop");
		while (meatPhase == MeatPhase.Can)
		{
			if (sausageCounter < sausageMax)
			{
				bool flag = false;
				SausageType sausageType;
				if (previousSausageType == SausageType.U1 || previousSausageType == SausageType.U2 || previousSausageType == SausageType.U3)
				{
					sausageType = SausageTypeDown.RandomChoice();
					flag = true;
				}
				else if (sausageMax - sausageCounter < 2)
				{
					sausageType = SausageTypeEnd.RandomChoice();
				}
				else
				{
					for (sausageType = previousSausageType; sausageType == previousSausageType; sausageType = SausageTypeAny.RandomChoice())
					{
					}
				}
				previousSausageType = sausageType;
				sausageCounter++;
				FlyingCowboyLevelSausageLink flyingCowboyLevelSausageLink = sausage.Create(nextSpawn.position, sausageHolder.transform.eulerAngles.z, 0f - p.sausageTrainSpeed) as FlyingCowboyLevelSausageLink;
				flyingCowboyLevelSausageLink.transform.parent = sausageHolder;
				flyingCowboyLevelSausageLink.Initialize(sausageType, sausageLinkSqueezePoint, (!flag) ? null : previousSausage);
				if (flag)
				{
					flyingCowboyLevelSausageLink.animator.Play("SqueezeLoopDown");
				}
				previousSausage = flyingCowboyLevelSausageLink;
				nextSpawn.parent = flyingCowboyLevelSausageLink.transform;
				nextSpawn.localPosition = new Vector3(SausageLinkWidth, 0f);
				SpriteRenderer component = flyingCowboyLevelSausageLink.GetComponent<SpriteRenderer>();
				component.sortingOrder = ((!isTypeA) ? currentSausageLinkSortingOrderB : currentSausageLinkSortingOrderA);
			}
			else
			{
				int num = gapPattern.PopInt() - 1;
				float x = SausageGapWidths[num];
				nextSpawn.localPosition = new Vector3(x, 0f);
				BasicProjectile basicProjectile = sausageString.Create(nextSpawn.position, sausageHolder.transform.eulerAngles.z, 0f - p.sausageTrainSpeed);
				basicProjectile.animator.Play(SausageGapAnimationNames[num]);
				SpriteRenderer component2 = basicProjectile.GetComponent<SpriteRenderer>();
				component2.sortingOrder = ((!isTypeA) ? currentSausageLinkSortingOrderB : currentSausageLinkSortingOrderA);
				basicProjectile.transform.parent = sausageHolder;
				nextSpawn.parent = basicProjectile.transform;
				nextSpawn.localPosition = new Vector3(SausageLinkWidth, 0f);
				sausageCounter = 0;
				sausageMax = sausageAmountPattern.PopInt();
			}
			while (nextSpawn.position.x > sausageHolder.position.x + 175f)
			{
				yield return null;
			}
		}
	}

	private IEnumerator beanCanTriggerZone_cr()
	{
		LevelProperties.FlyingCowboy.Can p = base.properties.CurrentState.can;
		PatternString extendTimerPattern = new PatternString(p.beanCanExtendTimer);
		PatternString topSpawnPattern = new PatternString(p.beanCanPostionUpper);
		PatternString bottomSpawnPattern = new PatternString(p.beanCanPositionLower);
		float[] timers = new float[beanCanTriggerZones.Length];
		while (true)
		{
			yield return null;
			for (int i = 0; i < beanCanTriggerZones.Length; i++)
			{
				bool flag = false;
				_ = Vector3.zero;
				foreach (AbstractPlayerController allPlayer in PlayerManager.GetAllPlayers())
				{
					TriggerZone triggerZone = beanCanTriggerZones[i];
					if (allPlayer != null && triggerZone.Contains(allPlayer.center))
					{
						timers[i] += CupheadTime.Delta;
						_ = allPlayer.center;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					timers[i] = 0f;
				}
				else if (timers[i] > p.beanCanTriggerTime)
				{
					timers[i] -= p.beanCanTriggerTime;
					string[] array = ((!(beanCanTriggerZones[i].transform.position.y > 0f)) ? bottomSpawnPattern.PopString() : topSpawnPattern.PopString()).Split(':');
					Parser.FloatTryParse(array[0], out var result);
					bool pointingUp = array[1] == "U";
					FlyingCowboyLevelBeans flyingCowboyLevelBeans = beansPrefab.Spawn();
					flyingCowboyLevelBeans.Init(new Vector3(690f, result), pointingUp, p.beanCanSpeed, extendTimerPattern.PopFloat());
				}
			}
		}
	}

	private void die()
	{
		isDead = true;
		StopAllCoroutines();
		if (Level.Current.mode == Level.Mode.Easy)
		{
			base.animator.Play("DeathEasy");
		}
		else
		{
			base.animator.Play("Death");
			StartCoroutine(spawnFloatingSausages_cr());
			AudioManager.Stop("sfx_dlc_cowgirl_p3_sausagemeattin_loop");
		}
		for (int i = 0; i < 2; i++)
		{
			Transform transform = ((i != 0) ? sausageHolderB : sausageHolderA);
			int childCount = transform.childCount;
			for (int j = 0; j < childCount; j++)
			{
				Transform child = transform.GetChild(j);
				if (child.position.x > sausageLinkSqueezePoint.position.x)
				{
					Object.Destroy(child.gameObject);
				}
			}
		}
	}

	private IEnumerator spawnFloatingSausages_cr()
	{
		float delay = 1f;
		string[] animations = new string[3] { "A", "B", "C" };
		float[] spawnFactors = new float[6] { 0.2f, 0.6f, 0f, 0.8f, 0.4f, 1f };
		int spawnFactorIndex = Random.Range(0, spawnFactors.Length);
		int animationIndex = Random.Range(0, animations.Length);
		while (true)
		{
			Vector3 position = Vector3.Lerp(t: spawnFactors[spawnFactorIndex], a: floatingSausageSpawnPointLeft.position, b: floatingSausageSpawnPointRight.position);
			FlyingCowboyFloatingSausages s = floatingSausage.Create(position) as FlyingCowboyFloatingSausages;
			s.SetAnimation(animations[animationIndex]);
			spawnFactorIndex = MathUtilities.NextIndex(spawnFactorIndex, spawnFactors.Length);
			animationIndex = MathUtilities.NextIndex(animationIndex, animations.Length);
			yield return CupheadTime.WaitForSeconds(this, delay);
		}
	}

	private void onBossDeathExplosionsEventHandler()
	{
		Level.Current.OnBossDeathExplosionsEvent -= onBossDeathExplosionsEventHandler;
		string[] list = new string[4] { "A", "B", "C", "H" };
		string[] list2 = new string[5] { "D", "E", "F", "G", "I" };
		string[] array = new string[3] { "E", "F", "I" };
		for (int i = 0; i < 2; i++)
		{
			Transform transform = ((i != 0) ? sausageHolderB : sausageHolderA);
			int childCount = transform.childCount;
			for (int j = 0; j < childCount; j++)
			{
				Transform child = transform.GetChild(j);
				if (child.name.Contains("String"))
				{
					Effect effect = sausageStringDeathEffect.Create(child.GetComponent<SpriteRenderer>().bounds.center);
					effect.transform.rotation = child.rotation;
					Animator animator = effect.animator;
					AnimatorStateInfo currentAnimatorStateInfo = child.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
					if (currentAnimatorStateInfo.IsName("String1"))
					{
						animator.Play(list.RandomChoice());
					}
					else if (currentAnimatorStateInfo.IsName("String2"))
					{
						animator.Play(list2.RandomChoice());
					}
					else
					{
						animator.Play(list2.RandomChoice());
					}
				}
				else
				{
					SpriteRenderer component = child.GetComponent<SpriteRenderer>();
					sausageDeathEffect.Create(component.bounds.center);
				}
				Object.Destroy(child.gameObject);
			}
		}
	}

	private void AnimationEvent_SFX_VocalSausageScreaming()
	{
		AudioManager.Play("sfx_dlc_cowgirl_vocal_p3sausagescreaming");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_vocal_p3sausagescreaming");
	}

	private void AnimationEvent_SFX_CanSlam()
	{
		AudioManager.Play("sfx_DLC_Cowgirl_P3_CanSlam_Transition");
		emitAudioFromObject.Add("sfx_DLC_Cowgirl_P3_CanSlam_Transition");
	}

	private void AnimationEvent_SFX_CanHoleBurst()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p3_can_holeburst_pop");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p3_can_holeburst_pop");
	}

	private void SFX_CanSpitBurningFire()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p3_canspitburningfire");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p3_canspitburningfire");
	}

	private void SFX_CanSpit()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p3_can_spit");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p3_can_spit");
	}

	private void AnimationEvent_SFX_SausageBullRoar()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p3_sausagebullroar");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p3_sausagebullroar");
	}

	private void AnimationEvent_SFX_SausageBullSpit()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p3_sausagebullspit");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p3_sausagebullspit");
	}

	private void AnimationEvent_SFX_SausageBullWingUp()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p3_sausagebullwingup");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p3_sausagebullwingup");
	}

	private void AnimationEvent_SFX_SausageBullWingDown()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p3_sausagebullwingdown");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p3_sausagebullwingdown");
	}

	private void AnimationEvent_SFX_SausageBullRunToFly()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p3_sausagebull_runtofly");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p3_sausagebull_runtofly");
	}

	private void AnimationEvent_SFX_SausageBullPositionTransfer()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p3_sausage_position_transfer");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p3_sausage_position_transfer");
	}

	private IEnumerator wobble_cr(Transform transform, Vector2 wobbleRadius, Vector2 wobbleDuration, Vector3 initialPosition, MeatPhase phase, bool useLocal, bool easeWobble)
	{
		float easeFactor = 0f;
		float elapsedEaseTime = 0f;
		Vector3 shadowInitialPosition = shadowTransform.position;
		Vector2 wobbleTimeElapsed = wobbleDuration * 0.5f;
		while (meatPhase == phase)
		{
			if (easeWobble && elapsedEaseTime < 2f)
			{
				elapsedEaseTime += (float)CupheadTime.Delta;
				easeFactor = Mathf.Lerp(0f, 1f, elapsedEaseTime / 2f);
			}
			wobbleTimeElapsed.x += CupheadTime.Delta;
			wobbleTimeElapsed.y += CupheadTime.Delta;
			if (wobbleTimeElapsed.x >= 2f * wobbleDuration.x)
			{
				wobbleTimeElapsed.x -= 2f * wobbleDuration.x;
			}
			float tx = ((!(wobbleTimeElapsed.x > wobbleDuration.x)) ? (wobbleTimeElapsed.x / wobbleDuration.x) : (1f - (wobbleTimeElapsed.x - wobbleDuration.x) / wobbleDuration.x));
			if (wobbleTimeElapsed.y >= 2f * wobbleDuration.y)
			{
				wobbleTimeElapsed.y -= 2f * wobbleDuration.y;
			}
			float ty = ((!(wobbleTimeElapsed.y > wobbleDuration.y)) ? (wobbleTimeElapsed.y / wobbleDuration.y) : (1f - (wobbleTimeElapsed.y - wobbleDuration.y) / wobbleDuration.y));
			Vector3 positionChange = new Vector3(EaseUtils.EaseInOutSine(wobbleRadius.x, 0f - wobbleRadius.x, tx), EaseUtils.EaseInOutSine(wobbleRadius.y, 0f - wobbleRadius.y, ty));
			if (useLocal)
			{
				transform.localPosition = initialPosition + positionChange;
			}
			else
			{
				transform.position = initialPosition + positionChange;
			}
			if (meatPhase == MeatPhase.Can && !Mathf.Approximately(wobbleRadius.y, 0f))
			{
				Vector3 vector = positionChange;
				vector.y *= 0.2f;
				Vector3 position = shadowInitialPosition + vector;
				float num = 0f;
				if (position.y >= -220f)
				{
					num = MathUtilities.LerpMapping(position.y, -220f, -150f, 0f, 0.65000004f, clamp: true);
					position.y = -220f;
				}
				shadowTransform.position = position;
				float num2 = 0.1f * (positionChange.y / wobbleRadius.y);
				float value = 0.8f - num2 - num;
				shadowTransform.SetScale(value, value);
			}
			yield return null;
		}
	}
}
