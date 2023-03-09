using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class AirplaneLevel : Level
{
	private LevelProperties.Airplane properties;

	private const float PHASE_TWO_DELAY = 1f;

	private const float PHASE_THREE_DELAY = 0.5f;

	private const float SECRET_INTRO_REAPPEAR_DELAY = 0.3f;

	private const float PHASE_THREE_BOUNDS = 465f;

	private const float PLANE_MAX_PHASE_ONE = 480f;

	private const float PLANE_MAX_PHASE_THREE = 225f;

	private const int TERRIERCOUNT = 4;

	private static readonly float[] TERRIER_PIVOT_OFFSET_X = new float[4] { 20f, -20f, 20f, -20f };

	private static readonly float[] TERRIER_PIVOT_OFFSET_Y = new float[4] { 20f, -20f, -20f, 20f };

	private const float PIVOT_MOVE = 30f;

	private const float MOVE_TIME = 3f;

	private const float SECRET_INTRO_X_START = -780f;

	private const float SECRET_INTRO_X_END = -35f;

	private const float SECRET_INTRO_PLANE_POS = 325f;

	[SerializeField]
	private AirplaneLevelPlayerPlane airplane;

	[SerializeField]
	private AirplaneLevelCanteenAnimator canteenAnimator;

	[SerializeField]
	private AirplaneLevelBulldogPlane bulldogPlane;

	[SerializeField]
	private AirplaneLevelBulldogParachute bulldogParachute;

	[SerializeField]
	private AirplaneLevelBulldogCatAttack bulldogCatAttack;

	[SerializeField]
	public AirplaneLevelLeader leader;

	[SerializeField]
	private Animator secretIntro;

	[SerializeField]
	private Animator leaderAnimator;

	[SerializeField]
	private AirplaneLevelSecretLeader secretLeader;

	[SerializeField]
	private AirplaneLevelSecretTerrier[] secretTerriers;

	[SerializeField]
	private AnimationClip rotateClip;

	[SerializeField]
	private LevelPit pitTop;

	[SerializeField]
	private Transform terrierPivotPoint;

	[SerializeField]
	private Transform[] secretPhaseTerrierPositions;

	[SerializeField]
	private Transform[] secretPhaseLeaderPositions;

	[SerializeField]
	private Transform[] leaderDeathPositions;

	private bool[] secretPhaseHoleOccupied;

	[SerializeField]
	private BlurOptimized bgBlur;

	[SerializeField]
	private UnityEngine.Camera bgCamera;

	[Header("Prefabs")]
	[SerializeField]
	private AirplaneLevelTerrier terrierPrefab;

	public bool terriersIntroFinished;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortraitMain;

	[SerializeField]
	private Sprite _bossPortraitPhaseTwo;

	[SerializeField]
	private Sprite _bossPortraitPhaseThree;

	[SerializeField]
	private Sprite _bossPortraitPhaseSecret;

	[SerializeField]
	private string _bossQuoteMain;

	[SerializeField]
	private string _bossQuotePhaseTwo;

	[SerializeField]
	private string _bossQuotePhaseThree;

	private float terrierHPTotal;

	private float decreaseAmount;

	private bool allTerriersDead;

	private bool secretPhaseActivated;

	private int secretTargetTerrier;

	private List<AirplaneLevelTerrier> terriers;

	private List<AirplaneLevelTerrierSmokeFX> smokeFXPool = new List<AirplaneLevelTerrierSmokeFX>();

	[SerializeField]
	private AirplaneLevelTerrierSmokeFX smokePrefab;

	private HashSet<SpriteRenderer> shadowableRenderers = new HashSet<SpriteRenderer>();

	private float currentShadowLevel = 1f;

	public override Levels CurrentLevel => Levels.Airplane;

	public override Scenes CurrentScene => Scenes.scene_level_airplane;

	public override Sprite BossPortrait
	{
		get
		{
			switch (properties.CurrentState.stateName)
			{
			case LevelProperties.Airplane.States.Main:
			case LevelProperties.Airplane.States.Rocket:
				return _bossPortraitMain;
			case LevelProperties.Airplane.States.Terriers:
				return _bossPortraitPhaseTwo;
			case LevelProperties.Airplane.States.Leader:
				return (!secretPhaseActivated) ? _bossPortraitPhaseThree : _bossPortraitPhaseSecret;
			default:
				Debug.LogError(string.Concat("Couldn't find portrait for state ", properties.CurrentState.stateName, ". Using Main."));
				return _bossPortraitMain;
			}
		}
	}

	public override string BossQuote
	{
		get
		{
			switch (properties.CurrentState.stateName)
			{
			case LevelProperties.Airplane.States.Main:
			case LevelProperties.Airplane.States.Rocket:
				return _bossQuoteMain;
			case LevelProperties.Airplane.States.Terriers:
				return _bossQuotePhaseTwo;
			case LevelProperties.Airplane.States.Leader:
				return _bossQuotePhaseThree;
			default:
				Debug.LogError(string.Concat("Couldn't find quote for state ", properties.CurrentState.stateName, ". Using Main."));
				return _bossQuoteMain;
			}
		}
	}

	public bool Rotating { get; private set; }

	protected override void PartialInit()
	{
		properties = LevelProperties.Airplane.GetMode(base.mode);
		properties.OnStateChange += base.zHack_OnStateChanged;
		properties.OnBossDeath += base.zHack_OnWin;
		base.timeline = properties.CreateTimeline(base.mode);
		goalTimes = properties.goalTimes;
		properties.OnBossDamaged += base.timeline.DealDamage;
		base.PartialInit();
	}

	protected override void Start()
	{
		base.CameraRotates = true;
		Rotating = false;
		base.Start();
		airplane.LevelInit(properties);
		bulldogPlane.LevelInit(properties);
		bulldogParachute.LevelInit(properties);
		bulldogCatAttack.LevelInit(properties);
		canteenAnimator.LevelInit(properties);
		secretLeader.gameObject.SetActive(value: false);
		airplane.SetXRange(-480f, 480f);
		leader.LevelInit(properties);
		leader.gameObject.SetActive(value: false);
		Invoke("StartGetShadowRenderers", 0.1f);
		PlayerManager.OnPlayerJoinedEvent += GetShadowableRenderers;
	}

	protected override void OnDestroy()
	{
		PlayerManager.OnPlayerJoinedEvent -= GetShadowableRenderers;
		base.OnDestroy();
		_bossPortraitMain = null;
		_bossPortraitPhaseTwo = null;
		_bossPortraitPhaseThree = null;
		_bossPortraitPhaseSecret = null;
		WORKAROUND_NullifyFields();
	}

	private void StartGetShadowRenderers()
	{
		GetShadowableRenderers(PlayerId.PlayerOne);
		GetShadowableRenderers(PlayerId.PlayerTwo);
		SpriteRenderer[] componentsInChildren = airplane.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			if (spriteRenderer.name != "PaladinShadow0" && spriteRenderer.name != "PaladinShadow1")
			{
				shadowableRenderers.Add(spriteRenderer);
			}
		}
	}

	private void GetShadowableRenderers(PlayerId playerId)
	{
		AbstractPlayerController player = PlayerManager.GetPlayer(playerId);
		if ((bool)player)
		{
			SpriteRenderer[] componentsInChildren = player.GetComponentsInChildren<SpriteRenderer>();
			foreach (SpriteRenderer spriteRenderer in componentsInChildren)
			{
				if (spriteRenderer.name != "PaladinShadow0" && spriteRenderer.name != "PaladinShadow1")
				{
					shadowableRenderers.Add(spriteRenderer);
				}
			}
		}
		UpdateShadow(currentShadowLevel);
	}

	public void UpdateShadow(float shadowValue)
	{
		currentShadowLevel = shadowValue;
		shadowableRenderers.Remove(null);
		foreach (SpriteRenderer shadowableRenderer in shadowableRenderers)
		{
			if (shadowableRenderer != null)
			{
				shadowableRenderer.color = new Color(shadowValue, shadowValue, shadowValue);
			}
		}
	}

	protected override void OnStateChanged()
	{
		base.OnStateChanged();
		if (properties.CurrentState.stateName == LevelProperties.Airplane.States.Rocket)
		{
			bulldogPlane.StartRocket();
		}
		else if (properties.CurrentState.stateName == LevelProperties.Airplane.States.Terriers)
		{
			StopAllCoroutines();
			StartCoroutine(terrier_cr());
			canteenAnimator.triggerCheer = true;
		}
		else if (properties.CurrentState.stateName == LevelProperties.Airplane.States.Leader && !secretPhaseActivated)
		{
			AudioManager.FadeSFXVolume("sfx_dlc_dogfight_p2_terrierjetpack_loop", 0f, 0.5f);
			canteenAnimator.triggerCheer = true;
			StartPhase3();
		}
	}

	private void StartPhase3()
	{
		for (int i = 0; i < smokeFXPool.Count; i++)
		{
			smokeFXPool[i].dead = true;
			if (!smokeFXPool[i].inUse)
			{
				Object.Destroy(smokeFXPool[i].gameObject);
			}
		}
		StopAllCoroutines();
		StartCoroutine(leader_cr());
	}

	public Vector3 CurrentEnemyPos()
	{
		switch (properties.CurrentState.stateName)
		{
		case LevelProperties.Airplane.States.Main:
		case LevelProperties.Airplane.States.Generic:
		case LevelProperties.Airplane.States.Rocket:
			return bulldogPlane.transform.position;
		case LevelProperties.Airplane.States.Terriers:
		{
			int index = Random.Range(0, terriers.Count);
			if (terriers[index] != null)
			{
				return terriers[index].transform.position;
			}
			return Vector3.zero;
		}
		case LevelProperties.Airplane.States.Leader:
			return leader.transform.position;
		default:
			return Vector3.zero;
		}
	}

	public bool ScreenHorizontal()
	{
		return leader.camRotatedHorizontally;
	}

	private IEnumerator terrier_cr()
	{
		bulldogPlane.OnStageChange();
		while (bulldogPlane.state != AirplaneLevelBulldogPlane.State.Main)
		{
			yield return null;
		}
		bulldogPlane.BulldogDeath();
		while (!bulldogPlane.startPhaseTwo)
		{
			yield return null;
		}
		yield return CupheadTime.WaitForSeconds(this, 1f);
		StartCoroutine(handle_terriers_cr());
	}

	private IEnumerator handle_terriers_cr()
	{
		LevelProperties.Airplane.Terriers p = properties.CurrentState.terriers;
		PatternString shotOrder = new PatternString(p.shotOrder);
		PatternString delayString = new PatternString(p.shotDelayString);
		PatternString typeString = new PatternString(p.shotTypeString);
		decreaseAmount = 0f;
		Vector3 start = airplane.transform.position;
		Vector3 end = new Vector3(0f, properties.CurrentState.plane.moveDown);
		airplane.AutoMoveToPos(new Vector3(0f, properties.CurrentState.plane.moveDown));
		canteenAnimator.ForceLook(new Vector3(0f, properties.CurrentState.plane.moveDown), 5);
		yield return CupheadTime.WaitForSeconds(this, 2f);
		terriers = new List<AirplaneLevelTerrier>(4);
		float startHealthPercentage = properties.CurrentState.healthTrigger;
		float endHealthPercentage = properties.GetNextStateHealthTrigger();
		float endHealth = endHealthPercentage * properties.TotalHealth;
		float startHealth = startHealthPercentage * properties.TotalHealth;
		terrierHPTotal = startHealth - endHealth;
		float terrierHP = terrierHPTotal / 4f;
		bool isClockwise = Rand.Bool();
		for (int i = 0; i < 4; i++)
		{
			terriers.Add(Object.Instantiate(terrierPrefab));
			terriers[i].Init(terrierPivotPoint, 90 * i, properties.CurrentState.terriers, terrierHP, TERRIER_PIVOT_OFFSET_X[i], TERRIER_PIVOT_OFFSET_Y[i], isClockwise, i);
		}
		AudioManager.PlayLoop("sfx_dlc_dogfight_p2_terrierjetpack_loop");
		yield return null;
		AudioManager.FadeSFXVolume("sfx_dlc_dogfight_p2_terrierjetpack_loop", 0.3f, 3f);
		bool readyToMove = true;
		while (readyToMove)
		{
			readyToMove = true;
			foreach (AirplaneLevelTerrier terrier in terriers)
			{
				if (terrier.ReadyToMove)
				{
					readyToMove = false;
					break;
				}
			}
			yield return null;
		}
		foreach (AirplaneLevelTerrier terrier2 in terriers)
		{
			terrier2.StartMoving();
		}
		StartCoroutine(move_pivot_cr());
		Coroutine CheckTerriersCR = StartCoroutine(check_terriers_cr(terrierHP));
		float delay2 = 0f;
		bool isWow = false;
		while (!AllTerriersSmoking())
		{
			bool shotRound = false;
			bool isPink = typeString.PopLetter() == 'P';
			delay2 = delayString.PopFloat();
			yield return CupheadTime.WaitForSeconds(this, delay2 - decreaseAmount);
			while (!shotRound)
			{
				int shot = shotOrder.PopInt();
				if (terriers[shot] != null && !terriers[shot].IsDead)
				{
					float num = ((!PlayerManager.BothPlayersActive()) ? Vector3.Distance(terriers[shot].GetPredictedAttackPos(), PlayerManager.GetNext().transform.position) : Mathf.Min(Vector3.Distance(terriers[shot].GetPredictedAttackPos(), PlayerManager.GetPlayer(PlayerId.PlayerOne).transform.position), Vector3.Distance(terriers[shot].GetPredictedAttackPos(), PlayerManager.GetPlayer(PlayerId.PlayerTwo).transform.position)));
					if (num > p.minAttackDistance)
					{
						terriers[shot].StartAttack(isPink, isWow);
						isWow = !isWow;
						shotRound = true;
					}
				}
				yield return null;
			}
		}
		if (AllTerriersSmoking())
		{
			secretPhaseActivated = true;
			StartCoroutine(handle_secret_intro_cr(isClockwise, terrierHP));
		}
	}

	private IEnumerator check_terriers_cr(float terrierHP)
	{
		bool allDead = false;
		bool[] deadOnes = new bool[terriers.Count];
		for (int j = 0; j < deadOnes.Length; j++)
		{
			deadOnes[j] = false;
		}
		while (!allDead)
		{
			allDead = true;
			int count = 0;
			for (int i = 0; i < terriers.Count; i++)
			{
				if (terriers[i] == null || terriers[i].IsDead)
				{
					count++;
					if (!deadOnes[i])
					{
						properties.DealDamage(terrierHP);
						decreaseAmount += properties.CurrentState.terriers.shotMinus;
						deadOnes[i] = true;
					}
					yield return null;
				}
				else
				{
					allDead = false;
				}
				if (terriers[i].lastOne)
				{
					count = 0;
				}
			}
			if (count == 3)
			{
				for (int k = 0; k < terriers.Count; k++)
				{
					if (terriers[k] != null && !terriers[k].IsDead)
					{
						terriers[k].lastOne = true;
					}
				}
			}
			yield return null;
		}
		if (properties.CurrentState.stateName == LevelProperties.Airplane.States.Terriers)
		{
			properties.DealDamageToNextNamedState();
		}
	}

	public bool AllTerriersSmoking()
	{
		if (secretPhaseActivated)
		{
			return true;
		}
		bool result = true;
		for (int i = 0; i < terriers.Count; i++)
		{
			if (terriers[i] == null || terriers[i].IsDead || !terriers[i].IsSmoking())
			{
				result = false;
			}
		}
		return result;
	}

	public void CreateSmokeFX(Vector3 pos, Vector3 vel, bool isSmoking, int sortingLayerID, int sortingOrder)
	{
		AirplaneLevelTerrierSmokeFX airplaneLevelTerrierSmokeFX = null;
		for (int i = 0; i < smokeFXPool.Count; i++)
		{
			if (!smokeFXPool[i].inUse)
			{
				airplaneLevelTerrierSmokeFX = smokeFXPool[i];
				break;
			}
		}
		if (airplaneLevelTerrierSmokeFX == null)
		{
			airplaneLevelTerrierSmokeFX = (AirplaneLevelTerrierSmokeFX)smokePrefab.Create(pos);
			smokeFXPool.Add(airplaneLevelTerrierSmokeFX);
		}
		airplaneLevelTerrierSmokeFX.animator.SetInteger("Effect", Random.Range(0, 3));
		airplaneLevelTerrierSmokeFX.animator.Play((!isSmoking) ? "A" : "AGray", 0, 0f);
		airplaneLevelTerrierSmokeFX.rend.sortingLayerID = sortingLayerID;
		airplaneLevelTerrierSmokeFX.rend.sortingOrder = sortingOrder;
		airplaneLevelTerrierSmokeFX.rend.enabled = true;
		airplaneLevelTerrierSmokeFX.inUse = true;
		airplaneLevelTerrierSmokeFX.vel = vel;
		airplaneLevelTerrierSmokeFX.myTransform = airplaneLevelTerrierSmokeFX.transform;
		airplaneLevelTerrierSmokeFX.myTransform.position = pos;
	}

	private void HandleTimelineHP(float terrierHP)
	{
		base.timeline.DealDamage(terrierHP);
	}

	private IEnumerator move_pivot_cr()
	{
		float t = 1.5f;
		float val = 1f;
		bool reversed = Rand.Bool();
		float start = terrierPivotPoint.position.x + 30f;
		float end = terrierPivotPoint.position.x - 30f;
		YieldInstruction wait = new WaitForFixedUpdate();
		while (true)
		{
			if (t < 3f)
			{
				t += CupheadTime.FixedDelta;
				if (reversed)
				{
					terrierPivotPoint.transform.SetPosition(Mathf.Lerp(start, end, val - t / 3f));
				}
				else
				{
					terrierPivotPoint.transform.SetPosition(Mathf.Lerp(start, end, t / 3f));
				}
				yield return wait;
			}
			else
			{
				t = 0f;
				reversed = !reversed;
				yield return null;
			}
		}
	}

	private IEnumerator leader_cr()
	{
		LevelProperties.Airplane.Plane p = properties.CurrentState.plane;
		if (terriers == null)
		{
			while (bulldogPlane.state != AirplaneLevelBulldogPlane.State.Main)
			{
				yield return null;
			}
			bulldogPlane.BulldogDeath();
		}
		if (!secretPhaseActivated)
		{
			yield return CupheadTime.WaitForSeconds(this, 0.5f);
		}
		leader.gameObject.SetActive(value: true);
		leader.StartLeader();
		if (secretPhaseActivated)
		{
			leader.animator.Play("Intro", 0, 0.54f);
			yield return null;
		}
		else
		{
			airplane.AutoMoveToPos(new Vector3(0f, p.moveDownPhThree));
			canteenAnimator.ForceLook(new Vector3(0f, p.moveDownPhThree), 3);
			airplane.SetXRange(-225f, 225f);
			yield return CupheadTime.WaitForSeconds(this, 1f);
		}
		int target = Animator.StringToHash(leader.animator.GetLayerName(0) + ".Intro");
		while (leader.animator.GetCurrentAnimatorStateInfo(0).fullPathHash == target)
		{
			if (leader.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.75f)
			{
				((AirplaneLevel)Level.Current).UpdateShadow(1f - Mathf.InverseLerp(0.75f, 1f, leader.animator.GetCurrentAnimatorStateInfo(0).normalizedTime) * 0.1f);
			}
			yield return null;
		}
		if (!secretPhaseActivated)
		{
			StartCoroutine(rotate_camera());
		}
		else
		{
			StartCoroutine(secret_phase_cr());
		}
	}

	public void MoveBoundsIn()
	{
		StartCoroutine(move_bounds_for_phase_three_cr());
	}

	private IEnumerator move_bounds_for_phase_three_cr()
	{
		float t = 0f;
		float time = 0.2f;
		float boundsStart = bounds.left;
		while (t < time)
		{
			t += CupheadTime.FixedDelta;
			foreach (AbstractPlayerController allPlayer in PlayerManager.GetAllPlayers())
			{
				if (!allPlayer)
				{
					continue;
				}
				allPlayer.transform.position = new Vector3(Mathf.Clamp(allPlayer.transform.position.x, Mathf.Lerp(0f - boundsStart, -465f, t / time), Mathf.Lerp(boundsStart, 465f, t / time)), allPlayer.transform.position.y);
				if (allPlayer.stats.State == PlayerStatsManager.PlayerState.Super)
				{
					LevelPlayerWeaponManager component = allPlayer.GetComponent<LevelPlayerWeaponManager>();
					if (component.activeSuper != null)
					{
						component.activeSuper.transform.position = new Vector3(Mathf.Clamp(component.transform.position.x, Mathf.Lerp(0f - boundsStart, -465f, t / time), Mathf.Lerp(boundsStart, 465f, t / time)), component.transform.position.y);
					}
				}
			}
			yield return new WaitForFixedUpdate();
		}
		bounds.left = 465;
		bounds.right = 465;
	}

	public void BlurBGCamera()
	{
		StartCoroutine(bg_camera_blur_cr());
	}

	private IEnumerator bg_camera_blur_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		while (bgBlur.blurSize < 3f)
		{
			bgBlur.blurSize += CupheadTime.FixedDelta * 10f;
			yield return wait;
		}
		bgBlur.blurSize = 3f;
	}

	private IEnumerator rotate_camera()
	{
		LevelProperties.Airplane.Leader p = properties.CurrentState.leader;
		Vector3 airplanePhase3Pos = new Vector3(0f, properties.CurrentState.plane.moveDownPhThree);
		float startAngle = 0f;
		float endAngle = 360f;
		while (properties.CurrentState.stateName == LevelProperties.Airplane.States.Leader)
		{
			yield return CupheadTime.WaitForSeconds(this, p.attackDelay);
			if (leader.camRotatedHorizontally)
			{
				AudioManager.PlayLoop("sfx_dlc_dogfight_p3_dogcopter_close_loop");
				AudioManager.FadeSFXVolume("sfx_dlc_dogfight_p3_dogcopter_close_loop", 0.7f, 0.5f);
				leader.StartDropshot();
			}
			else
			{
				leader.StartLaser();
			}
			while (leader.IsAttacking)
			{
				yield return null;
			}
			if (Level.Current.mode == Mode.Easy)
			{
				continue;
			}
			startAngle = ((endAngle != 0f) ? endAngle : 360f);
			endAngle = startAngle - 90f;
			leader.RotateCamera();
			if (leader.camRotatedHorizontally)
			{
				pitTop.gameObject.SetActive(value: false);
			}
			leaderAnimator.SetTrigger("Continue");
			Rotating = true;
			yield return AnimatorExtensions.WaitForAnimationToEnd(name: "Rotate_Start_" + ((!leader.camRotatedHorizontally) ? "ToVer" : "ToHor"), animator: leaderAnimator, parent: this);
			SFX_DOGFIGHT_PlayerPlane_PositionChangeFlyby();
			float rotateTime = rotateClip.length;
			string animName = ((!leader.camRotatedHorizontally) ? "Rotate_Mid_ToVer" : "Rotate_Mid_ToHor");
			float start = airplane.transform.position.y;
			float end = ((!leader.camRotatedHorizontally) ? airplanePhase3Pos.y : properties.CurrentState.plane.moveWhenRotate);
			SFX_DOGFIGHT_P3_Dogcopter_ScreenRotateEndClunk();
			while (leaderAnimator.GetCurrentAnimatorStateInfo(0).IsName(animName))
			{
				float easePos = ((!leader.camRotatedHorizontally) ? Mathf.Clamp(EaseUtils.EaseInOutSine(-0.1f, 1f, leaderAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime), 0f, 1f) : EaseUtils.EaseOutSine(0f, 1f, leaderAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime));
				float rotationAmount = startAngle + Mathf.Lerp(0f, endAngle - startAngle, easePos);
				airplane.transform.SetPosition(null, Mathf.Lerp(start, end, leaderAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime));
				CupheadLevelCamera.Current.SetRotation(rotationAmount);
				bgCamera.transform.SetEulerAngles(null, null, rotationAmount);
				if (startAngle == 180f || startAngle == 90f)
				{
					leaderAnimator.transform.SetEulerAngles(null, null, 180f + rotationAmount);
				}
				else
				{
					leaderAnimator.transform.SetEulerAngles(null, null, rotationAmount);
				}
				yield return null;
			}
			if (!leader.camRotatedHorizontally)
			{
				CupheadLevelCamera.Current.Shake(20f, 0.5f);
				pitTop.gameObject.SetActive(value: true);
			}
			else
			{
				CupheadLevelCamera.Current.Shake(30f, 0.7f);
			}
			CupheadLevelCamera.Current.SetRotation(endAngle);
			bgCamera.transform.SetEulerAngles(null, null, endAngle);
			if (leader.camRotatedHorizontally)
			{
				leaderAnimator.transform.SetEulerAngles(null, null, (startAngle != 180f) ? endAngle : (0f - endAngle));
			}
			else
			{
				leaderAnimator.transform.SetEulerAngles(null, null, 180f);
				AudioManager.FadeSFXVolume("sfx_dlc_dogfight_p3_dogcopter_close_loop", 0f, 0.5f);
			}
			yield return leaderAnimator.WaitForAnimationToEnd(this, (!leader.camRotatedHorizontally) ? "Rotate_End_ToVer" : "Rotate_End_ToHor");
			leaderAnimator.SetBool("CloseUp", leader.camRotatedHorizontally);
			leaderAnimator.Update(0f);
			Rotating = false;
		}
	}

	private void LateUpdate()
	{
		if (!Rotating)
		{
			return;
		}
		if (leaderAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
		{
			leaderAnimator.transform.SetEulerAngles(null, null, 0f);
		}
		if (leaderAnimator.GetCurrentAnimatorStateInfo(0).IsName("Rotate_End_ToHor"))
		{
			leaderAnimator.Play("Push_Wait", 3, 0f);
			leaderAnimator.Update(0f);
		}
		if (leaderAnimator.GetCurrentAnimatorStateInfo(0).IsName("Rotate_Mid_ToVer"))
		{
			leaderAnimator.Play("None", 3, 0f);
			leaderAnimator.Update(0f);
		}
		if (properties.CurrentState.stateName != LevelProperties.Airplane.States.Terriers || terriersIntroFinished)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		for (int i = 0; i < smokeFXPool.Count; i++)
		{
			if (smokeFXPool[i] != null && smokeFXPool[i].inUse)
			{
				smokeFXPool[i].Step(deltaTime);
			}
		}
	}

	private IEnumerator handle_secret_intro_cr(bool clockwise, float terrierHP)
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		canteenAnimator.triggerCheer = true;
		for (int i = 0; i < terriers.Count; i++)
		{
			terriers[i].StartSecret();
		}
		Vector3 start = airplane.transform.position;
		Vector3 end2 = new Vector3(325f * (float)(clockwise ? 1 : (-1)), airplane.transform.position.y);
		float time = Mathf.Max(1f, Vector3.Distance(start, end2) / 200f);
		if (clockwise)
		{
			airplane.SetXRange(250f, 480f);
		}
		else
		{
			airplane.SetXRange(-480f, -250f);
		}
		canteenAnimator.ForceLook((!clockwise) ? (Vector3.right * 1000f) : (Vector3.left * 1000f), 9);
		airplane.AutoMoveToPos(end2);
		yield return CupheadTime.WaitForSeconds(this, time / 2f);
		if (clockwise)
		{
			secretIntro.transform.localScale = new Vector3(-1f, 1f);
		}
		yield return StartCoroutine(wait_for_terrier_to_reach_chomp_position_cr());
		secretIntro.transform.position = new Vector3(-780f * (float)(clockwise ? 1 : (-1)), secretIntro.transform.position.y);
		secretIntro.gameObject.SetActive(value: true);
		secretIntro.Play("Chomp", 0, 0f);
		AudioManager.Play("sfx_dlc_dogfight_ps_dogcopterintro_chompstart");
		StartCoroutine(handle_secret_intro_move_in_cr(clockwise));
		int hash = Animator.StringToHash(secretIntro.GetLayerName(0) + ".Chomp");
		while (terriers.Count > 0)
		{
			while (secretIntro.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f < 0.55f && secretIntro.GetCurrentAnimatorStateInfo(0).fullPathHash == hash)
			{
				yield return wait;
			}
			Object.Destroy(terriers[secretTargetTerrier].gameObject);
			terriers.RemoveAt(secretTargetTerrier);
			if (terriers.Count > 0)
			{
				yield return StartCoroutine(wait_for_terrier_to_reach_chomp_position_cr());
				secretIntro.Play("Chomp", 0, 0f);
				secretIntro.Update(0f);
			}
			if (terriers.Count == 1)
			{
				secretIntro.SetTrigger("Exit");
				AudioManager.FadeSFXVolume("sfx_dlc_dogfight_p2_terrierjetpack_loop", 0f, 0.5f);
			}
		}
		yield return secretIntro.WaitForAnimationToStart(this, "Exit");
		AudioManager.Play("sfx_dlc_dogfight_ps_dogcopterintro_chomplicklip");
		while (secretIntro.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.5f)
		{
			yield return wait;
		}
		AudioManager.Play("sfx_dlc_dogfight_ps_dogcopterintro_leaderintro");
		start = airplane.transform.position;
		end2 = new Vector3(0f, properties.CurrentState.plane.moveDownPhThree);
		airplane.AutoMoveToPos(end2);
		canteenAnimator.ForceLook(end2, 3);
		airplane.SetXRange(-225f, 225f);
		yield return CupheadTime.WaitForSeconds(this, 1f);
		yield return secretIntro.WaitForAnimationToEnd(this, "Exit", waitForEndOfFrame: false, waitForStart: false);
		secretIntro.gameObject.SetActive(value: false);
		yield return CupheadTime.WaitForSeconds(this, 0.3f);
		properties.DealDamage(terrierHP * (float)terriers.Count);
		StartPhase3();
	}

	private IEnumerator wait_for_terrier_to_reach_chomp_position_cr()
	{
		bool foundNext = false;
		while (!foundNext)
		{
			for (int i = 0; i < terriers.Count; i++)
			{
				if (terriers[i].RelativeAngle() > 2.8415928f && terriers[i].RelativeAngle() < 3.2415926f)
				{
					foundNext = true;
					secretTargetTerrier = i;
					terriers[secretTargetTerrier].PrepareForChomp();
				}
			}
			yield return null;
		}
	}

	private IEnumerator handle_secret_intro_move_in_cr(bool clockwise)
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		yield return wait;
		float t = 0f;
		while (t < 1f)
		{
			secretIntro.transform.position = new Vector3(EaseUtils.EaseOutSine(-780f * (float)(clockwise ? 1 : (-1)), -35f * (float)(clockwise ? 1 : (-1)), t), secretIntro.transform.position.y);
			t = ((!(t <= secretIntro.GetCurrentAnimatorStateInfo(0).normalizedTime)) ? 1f : secretIntro.GetCurrentAnimatorStateInfo(0).normalizedTime);
			yield return wait;
		}
		secretIntro.transform.position = new Vector3(-35f * (float)(clockwise ? 1 : (-1)), secretIntro.transform.position.y);
	}

	public int GetNextHole()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < secretPhaseHoleOccupied.Length; i++)
		{
			if (!secretPhaseHoleOccupied[i])
			{
				list.Add(i);
			}
		}
		if (list.Count == 0)
		{
			return -1;
		}
		int num = list[Random.Range(0, list.Count)];
		secretPhaseHoleOccupied[num] = true;
		return num;
	}

	public Vector3 GetHolePosition(int value, bool isLeader)
	{
		return (!isLeader) ? secretPhaseTerrierPositions[value].position : secretPhaseLeaderPositions[value].position;
	}

	public Vector3 GetLeaderDeathPosition(int value)
	{
		return leaderDeathPositions[value].position;
	}

	public void LeaveHole(int value)
	{
		secretPhaseHoleOccupied[value] = false;
	}

	public void OccupyHole(int value)
	{
		secretPhaseHoleOccupied[value] = true;
	}

	private IEnumerator secret_phase_cr()
	{
		leader.OpenPawHoles();
		yield return CupheadTime.WaitForSeconds(this, 0.25f);
		secretPhaseHoleOccupied = new bool[secretPhaseTerrierPositions.Length];
		secretLeader.gameObject.SetActive(value: true);
		secretLeader.LevelInit(properties);
		for (int i = 0; i < 4; i++)
		{
			secretTerriers[i].gameObject.SetActive(value: true);
			secretTerriers[i].LevelInit(properties);
		}
		PatternString dogAttackDelayString = new PatternString(properties.CurrentState.secretTerriers.dogAttackDelayString);
		PatternString dogAttackOrderString = new PatternString(properties.CurrentState.secretTerriers.dogAttackOrderString);
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, dogAttackDelayString.PopFloat());
			int nextAttacker2 = 0;
			nextAttacker2 = dogAttackOrderString.PopInt();
			secretTerriers[nextAttacker2].TryStartAttack();
			yield return null;
		}
	}

	public void LeaderDeath()
	{
		secretLeader.gameObject.SetActive(value: true);
		secretLeader.DieMain();
	}

	private void SFX_DOGFIGHT_PlayerPlane_PositionChangeFlyby()
	{
		AudioManager.Play("sfx_DLC_Dogfight_PlayerPlane_PositionChangeFlyby");
	}

	private void SFX_DOGFIGHT_P3_Dogcopter_ScreenRotateEndClunk()
	{
		AudioManager.Play("sfx_DLC_Dogfight_P3_DogCopter_ScreenRotateEndClunk");
	}

	private void WORKAROUND_NullifyFields()
	{
		leader = null;
		airplane = null;
		canteenAnimator = null;
		bulldogPlane = null;
		bulldogParachute = null;
		bulldogCatAttack = null;
		secretIntro = null;
		leaderAnimator = null;
		secretLeader = null;
		secretTerriers = null;
		rotateClip = null;
		pitTop = null;
		terrierPivotPoint = null;
		secretPhaseTerrierPositions = null;
		secretPhaseLeaderPositions = null;
		leaderDeathPositions = null;
		secretPhaseHoleOccupied = null;
		bgBlur = null;
		bgCamera = null;
		terrierPrefab = null;
		_bossPortraitMain = null;
		_bossPortraitPhaseTwo = null;
		_bossPortraitPhaseThree = null;
		_bossPortraitPhaseSecret = null;
		_bossQuoteMain = null;
		_bossQuotePhaseTwo = null;
		_bossQuotePhaseThree = null;
		terriers = null;
		smokeFXPool = null;
		smokePrefab = null;
		shadowableRenderers = null;
	}
}
