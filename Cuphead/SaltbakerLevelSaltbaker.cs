using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaltbakerLevelSaltbaker : LevelProperties.Saltbaker.Entity
{
	public enum State
	{
		Idle,
		Strawberries,
		Sugarcubes,
		Dough,
		Limes
	}

	private const float MOVE_POS_X = 370f;

	private const float MINT_ANIMATION_LENGTH = 1.75f;

	private const float PHASE_TWO_REFLECTION_OPACITY = 0.5f;

	public State currentAttack;

	private State prevAttack;

	private float[] preAttackTime = new float[4] { 1.75f, 4.5416665f, 5.2083335f, 4.2083335f };

	private float[] postAttackTime = new float[4]
	{
		1.125f,
		1.5833334f,
		1f / 3f,
		1.9166666f
	};

	[SerializeField]
	private SpriteRenderer sugarTextReversed;

	[SerializeField]
	private Transform[] playerDefrostPositions;

	[SerializeField]
	private GameObject shadow;

	[SerializeField]
	private GameObject table;

	[Header("Prefabs")]
	[SerializeField]
	private SaltbakerLevelStrawberry strawberryPrefab;

	[SerializeField]
	private SaltbakerLevelSugarcube sugarcubePrefab;

	[SerializeField]
	private SaltbakerLevelDough doughPrefab;

	[SerializeField]
	private SaltbakerLevelLime limePrefab;

	[SerializeField]
	private SaltbakerLevelStrawberryBasket strawberryBasket;

	[SerializeField]
	private SaltbakerLevelFeistTurret feistTurretPrefab;

	private SaltbakerLevelFeistTurret[] turrets = new SaltbakerLevelFeistTurret[4];

	private int turretIndex;

	private int[] turretFiringOrder = new int[4] { 2, 1, 3, 0 };

	private int turretFiringDir;

	private string[] turretHitAnimName = new string[4] { "PhaseTwoHitB", "PhaseTwoHitA", "PhaseTwoHitD", "PhaseTwoHitC" };

	[SerializeField]
	private SaltBakerLevelLeaf leafFallPrefab;

	[SerializeField]
	private SaltbakerLevelBGMint bgMintPrefab;

	[SerializeField]
	private Transform[] turretRoots;

	[SerializeField]
	private Animator handAnimator;

	[SerializeField]
	private GameObject transitionCamera;

	[SerializeField]
	private float transitionDelayAfterHandsClose;

	[SerializeField]
	private float transitionDuration = 2.5f;

	[SerializeField]
	private SpriteRenderer transitionFader;

	[SerializeField]
	private float endPhaseTwoShakeAmount = 75f;

	[SerializeField]
	private float startPhaseThreeShakeHoldover = 4f;

	private DamageReceiver damageReceiver;

	private Vector3 startPos;

	private bool onLeft;

	private float scale;

	private bool phaseOneEnded;

	public bool phaseTwoStarted;

	public bool preventAdditionalTurretLaunch;

	private float phaseTwoHPPrediction;

	private PatternString strawberriesSpawnString;

	private PatternString strawberriesDelayString;

	private PatternString sugarcubesPhaseString;

	private PatternString sugarcubesDelayString;

	private PatternString sugarcubesParryString;

	private PatternString doughSpawnSidePatternString;

	private PatternString doughSpawnTypeString;

	private PatternString doughSpawnDelayString;

	[SerializeField]
	private Animator doughFXAnimator;

	private PatternString limeHeightString;

	private PatternString limesDelayString;

	[SerializeField]
	private GameObject BG;

	[SerializeField]
	private Collider2D phaseOneCollider;

	[SerializeField]
	private SpriteRenderer phaseOneRenderer;

	[SerializeField]
	private float[] timeToNextAttack = new float[4];

	[SerializeField]
	private Animator mintHandAnimator;

	private List<GameObject> destroyOnPhaseEnd = new List<GameObject>();

	private List<Coroutine> attackCoroutines = new List<Coroutine>();

	[SerializeField]
	private GameObject reflectionCamera;

	[SerializeField]
	private Material reflectionMaterial;

	[SerializeField]
	private GameObject reflectionTexture;

	public event Action OnDeathEvent;

	private void Start()
	{
		scale = base.transform.localScale.x;
		startPos = base.transform.position;
		damageReceiver = phaseOneCollider.GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		doughFXAnimator.transform.parent = null;
		base.animator.SetBool("IntroCubes", Rand.Bool());
	}

	public override void LevelInit(LevelProperties.Saltbaker properties)
	{
		base.LevelInit(properties);
		strawberriesSpawnString = new PatternString(properties.CurrentState.strawberries.locationSpawnString);
		strawberriesDelayString = new PatternString(properties.CurrentState.strawberries.bulletDelayString);
		sugarcubesPhaseString = new PatternString(properties.CurrentState.sugarcubes.phaseString);
		sugarcubesDelayString = new PatternString(properties.CurrentState.sugarcubes.bulletDelayString);
		sugarcubesParryString = new PatternString(properties.CurrentState.sugarcubes.parryString);
		doughSpawnSidePatternString = new PatternString(properties.CurrentState.dough.doughSpawnSideString);
		doughSpawnDelayString = new PatternString(properties.CurrentState.dough.doughDelayString);
		doughSpawnTypeString = new PatternString(properties.CurrentState.dough.doughSpawnTypeString);
		limeHeightString = new PatternString(properties.CurrentState.limes.boomerangHeightString);
		limesDelayString = new PatternString(properties.CurrentState.limes.boomerangDelayString);
		timeToNextAttack[0] = properties.CurrentState.strawberries.startNextAtk;
		timeToNextAttack[1] = properties.CurrentState.sugarcubes.startNextAttack;
		timeToNextAttack[2] = properties.CurrentState.dough.startNextAttack;
		timeToNextAttack[3] = properties.CurrentState.limes.startNextAttack;
		StartCoroutine(intro_cr());
	}

	private IEnumerator intro_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1.5f);
		((SaltbakerLevel)Level.Current).SpawnSwoopers();
		yield return CupheadTime.WaitForSeconds(this, 1.5f);
		currentAttack = ((!base.animator.GetBool("IntroCubes")) ? State.Limes : State.Sugarcubes);
		prevAttack = currentAttack;
		AniEvent_StartProjectiles();
		attackCoroutines.Add(StartCoroutine(pattern_cr()));
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		base.properties.DealDamage(info.damage);
	}

	private IEnumerator pattern_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		do
		{
			currentAttack = (State)(base.properties.CurrentState.NextPattern + 1);
		}
		while (currentAttack == State.Strawberries || currentAttack == prevAttack);
		while (!phaseOneEnded)
		{
			switch (currentAttack)
			{
			case State.Sugarcubes:
				base.animator.Play("Sugarcubes");
				sugarTextReversed.enabled = base.transform.localScale.x == -1f;
				break;
			case State.Dough:
				base.animator.Play("Dough");
				break;
			case State.Limes:
				base.animator.Play("Limes");
				break;
			}
			prevAttack = currentAttack;
			while (currentAttack != 0)
			{
				yield return null;
			}
			if (!phaseOneEnded)
			{
				currentAttack = (State)(base.properties.CurrentState.NextPattern + 1);
				base.animator.SetBool("NextStrawberries", currentAttack == State.Strawberries);
				float timeToIdle = timeToNextAttack[(int)(prevAttack - 1)] - postAttackTime[(int)(prevAttack - 1)] - preAttackTime[(int)(currentAttack - 1)];
				yield return CupheadTime.WaitForSeconds(this, timeToIdle);
			}
			yield return base.animator.WaitForAnimationToStart(this, "Idle");
		}
	}

	private void AniEvent_StartProjectiles()
	{
		if (phaseOneEnded)
		{
			return;
		}
		switch (currentAttack)
		{
		case State.Strawberries:
			attackCoroutines.Add(StartCoroutine(strawberries_cr()));
			break;
		case State.Sugarcubes:
			attackCoroutines.Add(StartCoroutine(sugarcubes_cr()));
			break;
		case State.Dough:
			attackCoroutines.Add(StartCoroutine(dough_cr()));
			break;
		case State.Limes:
			attackCoroutines.Add(StartCoroutine(limes_cr()));
			break;
		}
		if (currentAttack == State.Strawberries)
		{
			prevAttack = State.Strawberries;
			if (!phaseOneEnded)
			{
				currentAttack = (State)(base.properties.CurrentState.NextPattern + 1);
			}
		}
		else
		{
			currentAttack = State.Idle;
		}
	}

	private void AniEvent_FinishMove()
	{
		base.transform.localScale = new Vector3(0f - base.transform.localScale.x, base.transform.localScale.y);
		onLeft = !onLeft;
	}

	private IEnumerator strawberries_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		LevelProperties.Saltbaker.Strawberries p = base.properties.CurrentState.strawberries;
		float delay = p.firstDelay;
		float attackTime = 0f;
		float delayTime = 0f;
		int anim = UnityEngine.Random.Range(0, 4);
		while (attackTime <= p.diagAtkDuration)
		{
			attackTime += CupheadTime.FixedDelta;
			delayTime += CupheadTime.FixedDelta;
			if (delayTime > delay)
			{
				delayTime -= delay;
				delay = strawberriesDelayString.PopFloat();
				destroyOnPhaseEnd.Add(strawberryPrefab.Create(new Vector3(strawberriesSpawnString.PopFloat(), (float)Level.Current.Ceiling + 100f), p.diagAngle + 90f, p.bulletSpeed, anim).gameObject);
				anim = (anim + 1) % 4;
			}
			yield return wait;
		}
	}

	private void AniEvent_StrawberryBasketStart()
	{
		strawberryBasket.StartRunIn(onLeft);
	}

	private void AniEvent_StrawberryBasketGrab()
	{
		strawberryBasket.GetGrabbed();
	}

	private void AniEvent_StrawberryBasketExit()
	{
		strawberryBasket.StartRunOut();
	}

	private IEnumerator sugarcubes_cr()
	{
		LevelProperties.Saltbaker.Sugarcubes p = base.properties.CurrentState.sugarcubes;
		bool side = onLeft;
		float delay = p.firstDelay;
		float phase2 = sugarcubesPhaseString.PopFloat();
		float delayTime = 0f;
		float attackTime = 0f;
		int anim = UnityEngine.Random.Range(0, 3);
		YieldInstruction wait = new WaitForFixedUpdate();
		while (attackTime <= p.sineAttackDuration)
		{
			attackTime += CupheadTime.FixedDelta;
			delayTime += CupheadTime.FixedDelta;
			if (delayTime > delay)
			{
				delayTime -= delay;
				delay = sugarcubesDelayString.PopFloat();
				phase2 = sugarcubesPhaseString.PopFloat();
				SaltbakerLevelSugarcube saltbakerLevelSugarcube = sugarcubePrefab.Spawn();
				saltbakerLevelSugarcube.Init(new Vector3((!side) ? (Level.Current.Right + 100) : (Level.Current.Left - 100), p.centerHeight), side, p, phase2, this, anim, sugarcubesParryString.PopLetter() == 'P');
				destroyOnPhaseEnd.Add(saltbakerLevelSugarcube.gameObject);
				anim = (anim + 1) % 3;
			}
			yield return wait;
		}
	}

	private IEnumerator dough_cr()
	{
		LevelProperties.Saltbaker.Dough p = base.properties.CurrentState.dough;
		bool side = onLeft;
		float attackTime = 0f;
		float delayTime = 0f;
		float delay = p.firstDelay;
		Vector3 left = new Vector3((float)Level.Current.Left - 100f, -300f);
		Vector3 right = new Vector3((float)Level.Current.Right + 100f, -300f);
		YieldInstruction wait = new WaitForFixedUpdate();
		int count = 0;
		int startAnimalType = UnityEngine.Random.Range(0, 3);
		while (attackTime <= p.doughAttackDuration)
		{
			attackTime += CupheadTime.FixedDelta;
			delayTime += CupheadTime.FixedDelta;
			if (delayTime > delay)
			{
				delayTime -= delay;
				delay = doughSpawnDelayString.PopFloat();
				int num = doughSpawnTypeString.PopInt();
				char c = doughSpawnSidePatternString.PopLetter();
				bool flag = ((c != 'P') ? (!side) : side);
				SaltbakerLevelDough saltbakerLevelDough = doughPrefab.Spawn();
				saltbakerLevelDough.Init((!flag) ? right : left, (!flag) ? (0f - p.doughXSpeed[num]) : p.doughXSpeed[num], p.doughYSpeed[num], p.doughGravity[num], p.doughHealth, count, (startAnimalType + count) % 3);
				destroyOnPhaseEnd.Add(saltbakerLevelDough.gameObject);
				count++;
			}
			yield return wait;
		}
	}

	private void AniEvent_FinishDough()
	{
		doughFXAnimator.transform.localScale = base.transform.localScale;
		doughFXAnimator.Play("DoughFX");
	}

	private IEnumerator limes_cr()
	{
		LevelProperties.Saltbaker.Limes p = base.properties.CurrentState.limes;
		bool side = onLeft;
		float attackTime = 0f;
		float delayTime = 0f;
		float delay = p.firstDelay;
		int sfxID = 0;
		int anim = UnityEngine.Random.Range(0, 4);
		YieldInstruction wait = new WaitForFixedUpdate();
		while (attackTime <= p.boomerangAttackDuration)
		{
			attackTime += CupheadTime.FixedDelta;
			delayTime += CupheadTime.FixedDelta;
			if (delayTime > delay)
			{
				delayTime -= delay;
				delay = limesDelayString.PopFloat();
				SaltbakerLevelLime saltbakerLevelLime = limePrefab.Spawn();
				saltbakerLevelLime.Init(new Vector3((!side) ? Level.Current.Right : Level.Current.Left, 0f), side, limeHeightString.PopLetter() == 'H', base.properties.CurrentState.limes, sfxID, anim);
				destroyOnPhaseEnd.Add(saltbakerLevelLime.gameObject);
				sfxID = (sfxID + 1) % 3;
				anim = (anim + 1) % 4;
			}
			yield return wait;
		}
		yield return wait;
	}

	public IEnumerator phase_one_to_two_cr()
	{
		phaseOneEnded = true;
		base.animator.SetTrigger("EndPhaseOne");
		yield return base.animator.WaitForAnimationToStart(this, "PhaseOneToTwo");
		phaseTwoHPPrediction = (int)(base.properties.CurrentHealth - base.properties.GetNextStateHealthTrigger() * base.properties.TotalHealth);
		foreach (LevelPlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (allPlayer != null)
			{
				allPlayer.weaponManager.EnableSuper(value: false);
			}
		}
		yield return base.animator.WaitForAnimationToEnd(this, "PhaseTwoIntro");
		phaseTwoStarted = true;
		Phase2SwitchOnPatterns();
	}

	private void AniEvent_HitTable()
	{
		phaseOneCollider.enabled = false;
		CupheadLevelCamera.Current.Shake(55f, 0.5f);
		base.transform.localScale = new Vector3(1f, 1f);
		AudioManager.StartBGMAlternate(0);
	}

	private void AniEvent_KillFires()
	{
		((SaltbakerLevel)Level.Current).KillFires();
	}

	private void AniEvent_HandsClosed()
	{
		ClearPhaseOneObjects();
		((SaltbakerLevel)Level.Current).ClearFires();
		foreach (LevelPlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (allPlayer != null)
			{
				allPlayer.weaponManager.InterruptSuper();
			}
		}
		foreach (LevelPlayerController allPlayer2 in PlayerManager.GetAllPlayers())
		{
			if (allPlayer2 != null)
			{
				allPlayer2.DisableInput();
				allPlayer2.motor.ClearBufferedInput();
				Level.Current.SetBounds(10780, -9220, 446, 370);
				allPlayer2.transform.position = playerDefrostPositions[(int)allPlayer2.id].position + Vector3.left * 10000f;
			}
		}
		transitionCamera.SetActive(value: true);
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: false, promptBeforeJoin: false);
		LevelPauseGUI.OnUnpauseEvent += SuppressPlayerJoin;
		if (((SaltbakerLevel)Level.Current).playerLost)
		{
			base.animator.speed = 0f;
		}
		else
		{
			StartCoroutine(scroll_bg_cr());
		}
	}

	private void AniEvent_ShakeScreen()
	{
		CupheadLevelCamera.Current.Shake(55f, 0.5f);
	}

	private void AniEvent_FadeInReflection()
	{
		StartCoroutine(fade_in_reflection_cr());
	}

	private void ClearPhaseOneObjects()
	{
		attackCoroutines.RemoveAll((Coroutine c) => c == null);
		foreach (Coroutine attackCoroutine in attackCoroutines)
		{
			StopCoroutine(attackCoroutine);
		}
		GameObject[] array = GameObject.FindGameObjectsWithTag("PlayerProjectile");
		for (int j = 0; j < array.Length; j++)
		{
			UnityEngine.Object.Destroy(array[j]);
		}
		Effect[] array2 = (Effect[])UnityEngine.Object.FindObjectsOfType(typeof(Effect));
		for (int k = 0; k < array2.Length; k++)
		{
			UnityEngine.Object.Destroy(array2[k].gameObject);
		}
		destroyOnPhaseEnd.RemoveAll((GameObject i) => i == null);
		for (int l = 0; l < destroyOnPhaseEnd.Count; l++)
		{
			UnityEngine.Object.Destroy(destroyOnPhaseEnd[l]);
		}
		UnityEngine.Object.Destroy(strawberryBasket.gameObject);
		foreach (LevelPlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (allPlayer != null)
			{
				allPlayer.weaponManager.AbortEX();
			}
		}
		PlayerSuperChaliceShieldHeart[] array3 = UnityEngine.Object.FindObjectsOfType<PlayerSuperChaliceShieldHeart>();
		foreach (PlayerSuperChaliceShieldHeart playerSuperChaliceShieldHeart in array3)
		{
			playerSuperChaliceShieldHeart.transform.parent = playerSuperChaliceShieldHeart.player.transform;
		}
	}

	private IEnumerator scroll_bg_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, transitionDelayAfterHandsClose);
		SaltbakerLevel level = Level.Current as SaltbakerLevel;
		float t = 0f;
		YieldInstruction wait = new WaitForFixedUpdate();
		CupheadLevelCamera.Current.Shake(8f, transitionDuration);
		Vector3 shadowOffset = shadow.transform.position - table.transform.position;
		while (t < transitionDuration)
		{
			level.yScrollPos = EaseUtils.EaseInOut(EaseUtils.EaseType.easeInSine, EaseUtils.EaseType.easeOutBack, 0f, 1f, Mathf.InverseLerp(0f, transitionDuration, t));
			shadow.transform.position = shadowOffset + table.transform.position + Vector3.up * level.yScrollPos * 1500f;
			t += CupheadTime.FixedDelta;
			yield return wait;
		}
		level.yScrollPos = 1f;
	}

	private IEnumerator fade_in_reflection_cr()
	{
		reflectionCamera.SetActive(value: true);
		yield return null;
		reflectionTexture.SetActive(value: true);
		float c = 0f;
		while (c < 0.5f)
		{
			c = Mathf.Clamp(c + (float)CupheadTime.Delta * 5f, 0f, 0.5f);
			reflectionMaterial.color = new Color(1f, 1f, 1f, c);
			yield return null;
		}
	}

	private void SuppressPlayerJoin()
	{
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: false, promptBeforeJoin: false);
	}

	private void AniEvent_HandsOpen()
	{
		foreach (LevelPlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (allPlayer != null)
			{
				allPlayer.EnableInput();
				allPlayer.weaponManager.DisableInput();
				allPlayer.transform.position = playerDefrostPositions[(int)allPlayer.id].position + Vector3.left * 10000f;
				allPlayer.motor.DoPostSuperHop();
			}
		}
	}

	private void AniEvent_SFX_MagicDough()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_magiccookie");
		emitAudioFromObject.Add("sfx_dlc_saltbaker_p1_magiccookie");
	}

	private void AniEvent_SpawnJumpers()
	{
		((SaltbakerLevel)Level.Current).SpawnJumpers();
	}

	private void AniEvent_ShakeScreenSaltFall()
	{
		CupheadLevelCamera.Current.Shake(20f, 2f);
	}

	private void AniEvent_RestorePlayers()
	{
		foreach (LevelPlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (allPlayer != null)
			{
				allPlayer.weaponManager.EnableSuper(value: true);
				allPlayer.weaponManager.EnableInput();
				Level.Current.SetBounds(780, 780, 446, 370);
				allPlayer.transform.position += Vector3.right * 10000f;
			}
		}
		PlayerSuperChaliceShieldHeart[] array = UnityEngine.Object.FindObjectsOfType<PlayerSuperChaliceShieldHeart>();
		foreach (PlayerSuperChaliceShieldHeart playerSuperChaliceShieldHeart in array)
		{
			playerSuperChaliceShieldHeart.transform.parent = null;
		}
		transitionCamera.SetActive(value: false);
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: true, promptBeforeJoin: true);
		LevelPauseGUI.OnUnpauseEvent -= SuppressPlayerJoin;
	}

	private void AniEvent_StartHandIdle()
	{
		phaseOneRenderer.enabled = false;
		handAnimator.Play("Idle");
		handAnimator.Update(0f);
		GetComponent<SpriteRenderer>().enabled = false;
	}

	private void Phase2SwitchOnPatterns()
	{
		if (base.properties.CurrentState.leaf.leavesOn)
		{
			StartCoroutine(leaf_fall_cr());
		}
	}

	public bool PreDamagePhaseTwoAndReturnWhetherDoomed(float damage)
	{
		phaseTwoHPPrediction -= damage;
		if (phaseTwoHPPrediction < 0f)
		{
			AudioManager.StopBGM();
			AudioManager.StartBGMAlternate(1);
		}
		return phaseTwoHPPrediction < 0f;
	}

	public void DamageSaltbaker(float damage, int turretIndex)
	{
		base.properties.DealDamage(damage);
		if (base.properties.CurrentState.stateName != LevelProperties.Saltbaker.States.PhaseThree)
		{
			base.animator.Play(turretHitAnimName[turretIndex]);
			base.animator.Update(0f);
			handAnimator.Play("Hit", 0, 0f);
			mintHandAnimator.Play((turretIndex != 3) ? "HitA" : "HitB", 1, 0f);
			CupheadLevelCamera.Current.Shake(30f, 0.5f);
		}
	}

	private void AniEvent_SpawnPepperShaker()
	{
		turrets[turretIndex] = UnityEngine.Object.Instantiate(feistTurretPrefab);
		turrets[turretIndex].transform.position = turretRoots[turretIndex].position;
		turrets[turretIndex].transform.localScale = new Vector3(Mathf.Sign(0f - turrets[turretIndex].transform.position.x), 1f);
		turrets[turretIndex].Setup(base.properties.CurrentState.turrets, this, turretIndex);
		turretIndex++;
		if (turretIndex == 4)
		{
			StartCoroutine(turret_cr());
		}
	}

	private IEnumerator turret_cr()
	{
		LevelProperties.Saltbaker.Turrets p = base.properties.CurrentState.turrets;
		turretIndex = UnityEngine.Random.Range(0, 4);
		turretFiringDir = Rand.PosOrNeg();
		PatternString bulletTypeString = new PatternString(p.bulletTypeString);
		yield return CupheadTime.WaitForSeconds(this, p.shotDelay);
		while (true)
		{
			if (turrets != null && turrets[turretFiringOrder[turretIndex]].IsActivated)
			{
				bool isPink = bulletTypeString.PopLetter() == 'P';
				turrets[turretFiringOrder[turretIndex]].Shoot(isPink, p.warningTime);
				yield return CupheadTime.WaitForSeconds(this, p.shotDelay);
			}
			turretIndex = (turretIndex + turretFiringDir + 4) % turrets.Length;
			yield return null;
		}
	}

	private IEnumerator leaf_fall_cr()
	{
		LevelProperties.Saltbaker.Leaf p = base.properties.CurrentState.leaf;
		PatternString leavesCountString = new PatternString(p.leavesCountString);
		bool animA = Rand.Bool();
		float posX2 = 0f;
		float posY = (float)Level.Current.Ceiling + 20f;
		while (true)
		{
			animA = !animA;
			base.animator.SetTrigger((!animA) ? "MintB" : "MintA");
			yield return base.animator.WaitForAnimationToStart(this, (!animA) ? "PhaseTwoMintB" : "PhaseTwoMintA");
			mintHandAnimator.Play((!animA) ? "MintB" : "MintA");
			yield return mintHandAnimator.WaitForAnimationToEnd(this, (!animA) ? "MintB" : "MintA");
			yield return CupheadTime.WaitForSeconds(this, p.reenterDelay);
			int leavesCount = leavesCountString.PopInt();
			float offset = Level.Current.Width / leavesCount;
			float extraOffset = p.leavesOffset.RandomFloat();
			List<int> animIDs = new List<int> { 0, 1, 2, 3 };
			for (int i = 0; i < animIDs.Count; i++)
			{
				int index = UnityEngine.Random.Range(0, animIDs.Count);
				int value = animIDs[i];
				animIDs[i] = animIDs[index];
				animIDs[index] = value;
			}
			for (int j = 0; j < leavesCount; j++)
			{
				posX2 = offset * ((float)j - (float)(leavesCount - 1) / 2f) - p.xDistance / 2f;
				Vector3 pos = new Vector3(posX2 + extraOffset, posY);
				SaltBakerLevelLeaf saltBakerLevelLeaf = leafFallPrefab.Spawn();
				saltBakerLevelLeaf.Init(pos, p.xTime, p.xDistance, p.yConstantSpeed, p.ySpeed, this, animIDs[j % 4]);
			}
			for (int k = 0; k < UnityEngine.Random.Range(4, 8); k++)
			{
				SaltbakerLevelBGMint saltbakerLevelBGMint = UnityEngine.Object.Instantiate(bgMintPrefab, new Vector3(UnityEngine.Random.Range(Level.Current.Left, 0), Level.Current.Ceiling + UnityEngine.Random.Range(250, 500)), Quaternion.identity, null);
				saltbakerLevelBGMint.StartAnimation(k % 4);
			}
			AudioManager.Play("sfx_dlc_saltbaker_p2_mintleafattack_leafdescend");
			yield return CupheadTime.WaitForSeconds(this, p.leavesDelay - 1.75f);
		}
	}

	public void OnPhaseThree()
	{
		StopAllCoroutines();
		StartCoroutine(phase_two_to_three_cr());
	}

	private IEnumerator phase_two_to_three_cr()
	{
		if (this.OnDeathEvent != null)
		{
			this.OnDeathEvent();
		}
		base.animator.Play("PhaseTwoDeath");
		handAnimator.Play("Death");
		mintHandAnimator.Play("None");
		yield return base.animator.WaitForAnimationToStart(this, "PhaseTwoDeath");
		transitionFader.gameObject.SetActive(value: true);
		turretIndex = 0;
		while (turretIndex < 4)
		{
			turrets[turretIndex].Die();
			turretIndex++;
			yield return CupheadTime.WaitForSeconds(this, 0.1f);
		}
		while (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.975f)
		{
			transitionFader.color = new Color(1f, 1f, 1f, Mathf.InverseLerp(0.8f, 0.975f, base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime));
			CupheadLevelCamera.Current.Shake(base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime * endPhaseTwoShakeAmount, startPhaseThreeShakeHoldover);
			yield return null;
		}
		CupheadLevelCamera.Current.Shake(endPhaseTwoShakeAmount, startPhaseThreeShakeHoldover);
		SaltbakerLevelFeistTurret[] array = turrets;
		foreach (SaltbakerLevelFeistTurret saltbakerLevelFeistTurret in array)
		{
			UnityEngine.Object.Destroy(saltbakerLevelFeistTurret.gameObject);
		}
		transitionFader.color = Color.white;
		yield return CupheadTime.WaitForSeconds(this, 0.5f);
		((SaltbakerLevel)Level.Current).StartPhase3();
		BG.SetActive(value: false);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected override void OnDestroy()
	{
		UnityEngine.Object.Destroy(reflectionCamera);
		UnityEngine.Object.Destroy(reflectionTexture);
		base.OnDestroy();
	}

	private void AnimationEvent_SFX_SALTBAKER_P1_DoughAttack_RollAndKnead()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_doughattack_rollandknead");
	}

	private void AnimationEvent_SFX_SALTBAKER_P1_DoughAttack_RollingPinAppear()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_doughattack_rollingpinappear");
	}

	private void AnimationEvent_SFX_SALTBAKER_P1_Intro_BowTiePull()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_intro_bowtiepull");
	}

	private void AnimationEvent_SFX_SALTBAKER_P1_Intro_HandSwipeLimesSugar()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_intro_handswipe_limessugar");
	}

	private void AnimationEvent_SFX_SALTBAKER_P1_Limes_Knife_ChopCut()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_limes_knife_chopcut");
	}

	private void AnimationEvent_SFX_SALTBAKER_P1_Limes_Knife_Scrape()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_limes_knife_scrape");
	}

	private void AnimationEvent_SFX_SALTBAKER_P1_Limes_Knife_SliceSwing()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_limes_knife_sliceswing");
	}

	private void AnimationEvent_SFX_SALTBAKER_P1_StrawberrySqueeze_Attack()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_strawberrysqueeze_attack");
	}

	private void AnimationEvent_SFX_SALTBAKER_P1_SugarCube_Blow()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_sugarcube_blow");
	}

	private void AnimationEvent_SFX_SALTBAKER_P1_SugarCube_KnockAndBreak()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_sugarcube_knockandbreak");
	}

	private void AnimationEvent_SFX_SALTBAKER_P1_SugarCube_PlaceOnTable()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_sugarcube_placeontable");
	}

	private void AnimationEvent_SFX_SALTBAKER_P1_to_P2_Transition_A_TableSlam()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_to_p2_transition_a_tableslam");
	}

	private void AnimationEvent_SFX_SALTBAKER_P1_to_P2_Transition_B_HatRemove()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_to_p2_transition_b_hatremove");
	}

	private void AnimationEvent_SFX_SALTBAKER_P1_to_P2_Transition_C_ShroomInsert()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_to_p2_transition_c_shroominsert");
	}

	private void AnimationEvent_SFX_SALTBAKER_P1_to_P2_Transition_D_BakerPowerup()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_to_p2_transition_d_bakerpowerup");
	}

	private void AnimationEvent_SFX_SALTBAKER_P1_to_P2_Transition_E_GrabandRise()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_to_p2_transition_e_grabandrise");
	}

	private void AnimationEvent_SFX_SALTBAKER_P2_MintLeafAttack_LaunchThrow()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p2_mintleafattack_launchthrow");
	}

	private void AnimationEvent_SFX_SALTBAKER_P2_MintLeafAttack_LeafRustle()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p2_mintleafattack_leafrustle");
	}

	private void AnimationEvent_SFX_SALTBAKER_P2_Intro_Fingersnap()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p2_intro_fingersnap");
	}

	private void AnimationEvent_SFX_SALTBAKER_P2_Intro_Fingersnap_Laugh()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p2_intro_fingersnap_laugh");
	}

	private void AnimationEvent_SFX_SALTBAKER_P2_VocalPain()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p2_vocal_pain");
	}

	private void AnimationEvent_SFX_SALTBAKER_P2_Death()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p2_death");
	}

	public void SFXLeafRustle()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p2_mintleafattack_leafrustle");
	}

	public void SFXLaunchThrow()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p2_mintleafattack_launchthrow");
	}
}
