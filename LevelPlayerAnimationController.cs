using System.Collections;
using UnityEngine;

public class LevelPlayerAnimationController : AbstractLevelPlayerComponent
{
	private static class Booleans
	{
		public static readonly int Dashing = Animator.StringToHash("Dashing");

		public static readonly int Locked = Animator.StringToHash("Locked");

		public static readonly int Shooting = Animator.StringToHash("Shooting");

		public static readonly int Grounded = Animator.StringToHash("Grounded");

		public static readonly int Turning = Animator.StringToHash("Turning");

		public static readonly int Intro = Animator.StringToHash("Intro");

		public static readonly int Dead = Animator.StringToHash("Dead");

		public static readonly int HasParryCharm = Animator.StringToHash("HasParryCharm");

		public static readonly int HasParryAttack = Animator.StringToHash("HasParryAttack");

		public static readonly int ChaliceOffIdle = Animator.StringToHash("ChaliceOffIdle");

		public static readonly int DoubleJump = Animator.StringToHash("DoubleJump");

		public static readonly int ChaliceAirEX = Animator.StringToHash("ChaliceAirEX");
	}

	private static class Integers
	{
		public static readonly int MoveX = Animator.StringToHash("MoveX");

		public static readonly int MoveY = Animator.StringToHash("MoveY");

		public static readonly int LookX = Animator.StringToHash("LookX");

		public static readonly int LookY = Animator.StringToHash("LookY");

		public static readonly int ChaliceJumpDescendLoopCounter = Animator.StringToHash("ChaliceJumpDescendLoopCounter");
	}

	private static class Triggers
	{
		public static readonly int OnJump = Animator.StringToHash("OnJump");

		public static readonly int OnGround = Animator.StringToHash("OnGround");

		public static readonly int OnParry = Animator.StringToHash("OnParry");

		public static readonly int OnWin = Animator.StringToHash("OnWin");

		public static readonly int OnTurn = Animator.StringToHash("OnTurn");

		public static readonly int OnFire = Animator.StringToHash("OnFire");
	}

	private enum AnimLayers
	{
		Base,
		ShootRun,
		ShootRunDiag,
		ChaliceSpecial,
		ChaliceSync,
		ChaliceShootRun,
		ChaliceShootRunDiag
	}

	private enum ChaliceAim
	{
		UpAim,
		DiagUpAim,
		ForwardAim,
		DiagDownAim,
		DownAim
	}

	private const int PALADIN_SHADOW_BUFFER_SIZE = 10;

	private int ChaliceSuper1Return = Animator.StringToHash("Chalice_Super_1_Return");

	private int ChaliceSuper2Return = Animator.StringToHash("Chalice_Super_2_Return");

	private int ChaliceSuper2ReturnAir = Animator.StringToHash("Chalice_Super_2_Return_Air");

	private int ChaliceAirEXRecovery = Animator.StringToHash("Air_EX_Recovery");

	private int ChaliceJumpBall = Animator.StringToHash("Jump_Ball");

	private int ChaliceJumpDescend = Animator.StringToHash("Jump_Descend");

	[SerializeField]
	private GameObject cuphead;

	[SerializeField]
	private GameObject mugman;

	[SerializeField]
	private GameObject chalice;

	[SerializeField]
	private SpriteRenderer[] chaliceSprites;

	[Space(10f)]
	[SerializeField]
	private Transform runDustRoot;

	[SerializeField]
	private Transform sparkleRoot;

	[Space(10f)]
	[SerializeField]
	private Effect dashEffect;

	[SerializeField]
	private Effect groundedEffect;

	[SerializeField]
	private Effect hitEffect;

	[SerializeField]
	private Effect runEffect;

	[SerializeField]
	private Effect curseEffect;

	[SerializeField]
	private Effect smokeDashEffect;

	[SerializeField]
	private HealerCharmSparkEffect healerCharmEffect;

	[SerializeField]
	private Effect powerUpBurstEffect;

	[SerializeField]
	private Effect chaliceDoubleJumpEffect;

	[SerializeField]
	private Effect chaliceDashEffect;

	private Effect chaliceDashEffectActive;

	[SerializeField]
	private Effect chaliceDashSparkle;

	[SerializeField]
	private SpriteRenderer[] chaliceJumpShootRenderers;

	[SerializeField]
	private Material chaliceDuckDashMaterial;

	[SerializeField]
	private Effect chaliceDuckDashSparkles;

	private Coroutine chaliceInvincibleSparklesCoroutine;

	private bool chaliceFellFromDuckDash;

	[SerializeField]
	private LevelPlayerChaliceIntroAnimation chaliceIntroAnimation;

	private LevelPlayerChaliceIntroAnimation chaliceIntroCurrent;

	[SerializeField]
	private Sprite cupheadScaredSprite;

	[SerializeField]
	private Sprite mugmanScaredSprite;

	private bool hitAnimation;

	private bool super;

	private bool shooting;

	private bool fired;

	private bool intropowerupactive;

	private string exDirection;

	private Trilean2 lastTrueLookDir = new Trilean2(1, 0);

	private float timeSinceStoppedShooting = 100f;

	private Material tempMaterial;

	private const float STOP_SHOOTING_DELAY = 0.0833f;

	private bool isIntroB;

	private bool chaliceActivated;

	private bool inScaredIntro;

	[SerializeField]
	private float curseEffectDelay = 0.15f;

	[SerializeField]
	private MinMax curseAngleShiftRange = new MinMax(60f, 300f);

	[SerializeField]
	private MinMax curseDistanceRange = new MinMax(0f, 20f);

	private float curseEffectAngle;

	private float curseEffectTimer;

	private int curseCharmLevel = -1;

	private Vector3[] paladinShadowPosition;

	private Vector3[] paladinShadowScale;

	private Sprite[] paladinShadowSprite;

	[SerializeField]
	private SpriteRenderer[] paladinShadows;

	private bool showCurseFX;

	private IEnumerator colorCoroutine;

	public SpriteRenderer spriteRenderer { get; private set; }

	private bool Flashing => base.player.damageReceiver.state == PlayerDamageReceiver.State.Invulnerable;

	private void Start()
	{
		if (!chaliceActivated)
		{
			base.animator.SetLayerWeight(3, 0f);
			base.animator.SetLayerWeight(4, 0f);
		}
		base.basePlayer.OnPlayIntroEvent += PlayIntro;
		base.basePlayer.OnPlatformingLevelAwakeEvent += CheckIfChaliceAndActivate;
		base.player.motor.OnParryEvent += OnParryStart;
		base.player.motor.OnGroundedEvent += OnGrounded;
		base.player.motor.OnDashStartEvent += OnDashStart;
		base.player.motor.OnDashEndEvent += OnDashEnd;
		base.player.motor.OnDoubleJumpEvent += ChaliceDoubleJumpFX;
		base.player.damageReceiver.OnDamageTaken += OnDamageTaken;
		base.player.weaponManager.OnExStart += OnEx;
		base.player.weaponManager.OnSuperStart += OnSuper;
		base.player.weaponManager.OnSuperEnd += OnSuperEnd;
		base.player.weaponManager.OnWeaponFire += OnShotFired;
		LevelPauseGUI.OnPauseEvent += OnGuiPause;
		LevelPauseGUI.OnPauseEvent += OnGuiUnpause;
		lastTrueLookDir = base.player.motor.TrueLookDirection;
		SetBool(Booleans.HasParryCharm, (base.player.stats.Loadout.charm == Charm.charm_parry_plus && !Level.IsChessBoss) ? true : false);
		PlayerRecolorHandler.SetChaliceRecolorEnabled(chalice.GetComponent<SpriteRenderer>().sharedMaterial, SettingsData.Data.filter == BlurGamma.Filter.Chalice);
		if (base.player.stats.Loadout.charm == Charm.charm_curse)
		{
			curseCharmLevel = CharmCurse.CalculateLevel(base.player.id);
		}
		if (Level.Current.LevelType != Level.Type.Platforming)
		{
			if (base.player.stats.isChalice)
			{
				if ((Level.IsDicePalace && !DicePalaceMainLevelGameInfo.IS_FIRST_ENTRY) || SceneLoader.CurrentLevel == Levels.Kitchen || SceneLoader.CurrentLevel == Levels.ChaliceTutorial)
				{
					CheckIfChaliceAndActivate();
					base.basePlayer.OnPlayIntroEvent -= PlayIntro;
				}
				else if (SceneLoader.CurrentLevel != Levels.Devil && SceneLoader.CurrentLevel != Levels.Saltbaker)
				{
					StartChaliceIntroHold(fail: false);
				}
			}
			else if (base.player.stats.Loadout.charm == Charm.charm_chalice && SceneLoader.CurrentLevel != Levels.Devil && SceneLoader.CurrentLevel != Levels.Saltbaker && SceneLoader.CurrentLevel != Levels.Kitchen && SceneLoader.CurrentLevel != Levels.ChaliceTutorial && (!Level.IsDicePalace || DicePalaceMainLevelGameInfo.IS_FIRST_ENTRY))
			{
				StartChaliceIntroHold(fail: true);
			}
		}
		if (SceneLoader.CurrentLevel == Levels.ChaliceTutorial)
		{
			this.spriteRenderer.gameObject.layer = 31;
			SpriteRenderer[] array = chaliceSprites;
			foreach (SpriteRenderer spriteRenderer in array)
			{
				spriteRenderer.gameObject.layer = 31;
			}
			SpriteRenderer[] array2 = chaliceJumpShootRenderers;
			foreach (SpriteRenderer spriteRenderer2 in array2)
			{
				spriteRenderer2.gameObject.layer = 31;
			}
		}
	}

	private void OnEnable()
	{
		StartCoroutine(flash_cr());
	}

	private void OnDisable()
	{
		if ((bool)paladinShadows[0])
		{
			paladinShadows[0].enabled = false;
		}
		if ((bool)paladinShadows[1])
		{
			paladinShadows[1].enabled = false;
		}
	}

	private void Update()
	{
		if (base.player.IsDead || !base.player.levelStarted)
		{
			return;
		}
		if (curseCharmLevel > -1 && !showCurseFX && !Level.IsChessBoss)
		{
			InitializeCurseFX();
			showCurseFX = true;
		}
		if (base.player.stats.isChalice && chaliceActivated)
		{
			ChaliceAimSpriteHandling();
			ChaliceJumpHandling();
			ChaliceJumpShootHandling();
			if (!base.player.motor.Dashing)
			{
				base.animator.SetLayerWeight(3, 1f);
				if (chaliceInvincibleSparklesCoroutine != null)
				{
					StopCoroutine(chaliceInvincibleSparklesCoroutine);
					chaliceInvincibleSparklesCoroutine = null;
				}
			}
		}
		if (curseCharmLevel > -1)
		{
			HandleCurseFX();
		}
		if (!hitAnimation && (int)base.player.motor.LookDirection.x != 0 && (int)lastTrueLookDir.x != (int)base.player.motor.TrueLookDirection.x)
		{
			SetBool(Booleans.Turning, value: true);
		}
		else
		{
			SetBool(Booleans.Turning, value: false);
		}
		lastTrueLookDir = base.player.motor.TrueLookDirection;
		SetBool(Booleans.Grounded, base.player.motor.Grounded);
		SetBool(Booleans.Locked, base.player.motor.Locked);
		if (base.player.motor.Locked)
		{
			SetInt(Integers.MoveX, 0);
		}
		else
		{
			SetInt(Integers.MoveX, base.player.motor.LookDirection.x);
		}
		if (base.player.motor.Ducking || base.player.motor.IsUsingSuperOrEx)
		{
			SetInt(Integers.MoveY, 0);
			SetBool(Booleans.ChaliceOffIdle, value: true);
		}
		else
		{
			SetInt(Integers.MoveY, base.player.motor.MoveDirection.y);
			SetBool(Booleans.ChaliceOffIdle, value: false);
		}
		SetInt(Integers.LookX, base.player.motor.LookDirection.x);
		SetInt(Integers.LookY, base.player.motor.LookDirection.y);
		SetBool(Booleans.Shooting, base.player.weaponManager.IsShooting);
		float num = ((!base.player.weaponManager.IsShooting && !(timeSinceStoppedShooting < 0.0833f)) ? 0f : 1f);
		if (!base.player.stats.isChalice)
		{
			base.animator.SetLayerWeight(1, num);
			base.animator.SetLayerWeight(2, ((int)base.player.motor.LookDirection.y <= 0) ? 0f : num);
		}
		else
		{
			if (!base.player.motor.Grounded && base.animator.GetBool(Booleans.ChaliceAirEX))
			{
				num = 0f;
			}
			if (!ExitingChaliceSuper())
			{
				base.animator.SetLayerWeight(4, 1f - num);
			}
			else
			{
				base.animator.SetLayerWeight(4, 0f);
			}
			base.animator.SetLayerWeight(5, num);
			base.animator.SetLayerWeight(6, ((int)base.player.motor.LookDirection.y <= 0) ? 0f : num);
			if (base.player.motor.ChaliceDuckDashed && !base.player.motor.Grounded)
			{
				chaliceFellFromDuckDash = true;
			}
			if (base.player.motor.Grounded)
			{
				chaliceFellFromDuckDash = false;
			}
		}
		if (shooting)
		{
			timeSinceStoppedShooting = 0f;
		}
		else
		{
			timeSinceStoppedShooting += CupheadTime.Delta;
		}
		bool flag = false;
		if (fired && ((base.player.motor.Grounded && ((int)base.player.motor.LookDirection.x == 0 || base.player.motor.Locked || (int)base.player.motor.LookDirection.y < 0)) || (base.player.stats.isChalice && !base.player.motor.ChaliceDoubleJumped)))
		{
			SetTrigger(Triggers.OnFire);
			flag = true;
		}
		fired = false;
		shooting = base.player.weaponManager.IsShooting;
		if (!shooting && !flag)
		{
			ResetTrigger(Triggers.OnFire);
		}
		if (base.player.motor.Dashing && GetBool(Booleans.Dashing) != base.player.motor.Dashing)
		{
			if (base.player.stats.isChalice)
			{
				base.animator.SetLayerWeight(3, 0f);
			}
			if (base.player.stats.isChalice && base.player.motor.Ducking)
			{
				ChaliceDuckDashHandling();
			}
			else
			{
				Play("Dash.Air");
				if (base.player.stats.Loadout.charm != Charm.charm_smoke_dash || !base.player.stats.CurseSmokeDash || Level.IsChessBoss || (base.player.stats.isChalice && !base.player.motor.Ducking))
				{
					dashEffect.Create(base.transform.position, base.transform.localScale);
				}
				if (base.player.stats.isChalice)
				{
					chaliceDashEffectActive = chaliceDashEffect.Create(base.transform.position, base.transform.localScale);
					chaliceDashEffectActive.transform.parent = base.transform;
				}
			}
		}
		SetBool(Booleans.Dashing, base.player.motor.Dashing);
		if (!base.player.motor.Dashing)
		{
			if ((int)base.player.motor.LookDirection.x != 0 && !ExitingChaliceSuper())
			{
				base.transform.SetScale(base.player.motor.LookDirection.x);
			}
		}
		else
		{
			base.transform.SetScale(base.player.motor.DashDirection);
		}
	}

	public void ResetMoveX()
	{
		SetInt(Integers.MoveX, 0);
		inScaredIntro = false;
	}

	private void ChaliceDoubleJumpFX()
	{
		float value = 0f;
		if (base.player.input.GetAxis(PlayerInput.Axis.X) > 0f || (base.player.input.GetAxis(PlayerInput.Axis.X) > 0f && base.player.input.GetAxis(PlayerInput.Axis.Y) > 0f))
		{
			value = -35f;
		}
		else if (base.player.input.GetAxis(PlayerInput.Axis.X) < 0f || (base.player.input.GetAxis(PlayerInput.Axis.X) < 0f && base.player.input.GetAxis(PlayerInput.Axis.Y) > 0f))
		{
			value = 35f;
		}
		Effect effect = chaliceDoubleJumpEffect.Create(base.transform.position);
		effect.transform.SetEulerAngles(null, null, value);
	}

	private void ChaliceIncrementJumpDescendLoopCounter()
	{
		if ((int)base.player.motor.MoveDirection.y < 0)
		{
			SetInt(Integers.ChaliceJumpDescendLoopCounter, GetInt(Integers.ChaliceJumpDescendLoopCounter) + 1);
		}
	}

	private void ChaliceResetJumpDescendLoopCounter()
	{
		SetInt(Integers.ChaliceJumpDescendLoopCounter, 0);
	}

	private void ChaliceDuckDashHandling()
	{
		Play("Duck.Duck_Dash");
		AudioManager.Play("chalice_roll");
		if (chaliceInvincibleSparklesCoroutine != null)
		{
			StopCoroutine(chaliceInvincibleSparklesCoroutine);
			chaliceInvincibleSparklesCoroutine = null;
		}
		chaliceInvincibleSparklesCoroutine = StartCoroutine(chaliceInvincibleSparkle_cr());
	}

	private IEnumerator chaliceInvincibleSparkle_cr()
	{
		while (true)
		{
			float x = Random.Range(0f - base.player.colliderManager.Width, base.player.colliderManager.Width);
			float y = Random.Range(base.player.colliderManager.Height * -0.5f, base.player.colliderManager.Height * 1.5f);
			chaliceDuckDashSparkles.Create(base.player.transform.position + new Vector3(x, y, 0f));
			yield return CupheadTime.WaitForSeconds(this, 0.05f);
		}
	}

	private void ChaliceJumpHandling()
	{
		SetBool(Booleans.DoubleJump, base.player.motor.ChaliceDoubleJumped);
	}

	private void ChaliceJumpShootHandling()
	{
		bool flag = (base.player.weaponManager.IsShooting || timeSinceStoppedShooting < 0.0833f) && !base.player.motor.Grounded && !base.player.motor.Dashing && !base.player.motor.ChaliceDoubleJumped && !chaliceFellFromDuckDash && !GetBool(Booleans.ChaliceAirEX) && !hitAnimation && !super;
		chaliceJumpShootRenderers[0].enabled = flag;
		chaliceJumpShootRenderers[1].enabled = flag;
		if (!base.player.motor.Grounded)
		{
			spriteRenderer.enabled = !base.player.weaponManager.IsShooting && !(timeSinceStoppedShooting < 0.0833f);
			if (base.player.motor.ChaliceDoubleJumped || chaliceFellFromDuckDash || base.player.motor.Dashing || GetBool(Booleans.ChaliceAirEX) || hitAnimation)
			{
				spriteRenderer.enabled = true;
			}
		}
	}

	private void ChaliceAimSpriteHandling()
	{
		if (base.player.motor.Locked)
		{
			SetInt(Integers.MoveX, 0);
		}
		else
		{
			SetInt(Integers.MoveX, base.player.motor.LookDirection.x);
		}
		if (base.player.weaponManager.IsShooting || GetBool(Booleans.ChaliceOffIdle) || (GetInt(Integers.MoveX) != 0 && !base.player.motor.Dashing) || base.player.motor.Dashing || base.player.motor.DashState == LevelPlayerMotor.DashManager.State.End || !base.player.motor.Grounded || inScaredIntro)
		{
			SwitchChaliceAim(-1);
			spriteRenderer.enabled = true;
		}
		else if ((int)base.player.motor.LookDirection.x != 0)
		{
			SwitchChaliceAim(2);
			spriteRenderer.enabled = false;
			if ((int)base.player.motor.LookDirection.y > 0)
			{
				SwitchChaliceAim(1);
				spriteRenderer.enabled = false;
			}
			else if ((int)base.player.motor.LookDirection.y < 0)
			{
				SwitchChaliceAim(3);
				spriteRenderer.enabled = false;
			}
		}
		else if ((int)base.player.motor.LookDirection.y > 0)
		{
			SwitchChaliceAim(0);
			spriteRenderer.enabled = false;
		}
		else if ((int)base.player.motor.LookDirection.y < 0)
		{
			SwitchChaliceAim(4);
			spriteRenderer.enabled = false;
		}
		else
		{
			SwitchChaliceAim(-1);
			spriteRenderer.enabled = true;
		}
	}

	private void SwitchChaliceAim(int spriteToEnable)
	{
		for (int i = 0; i < chaliceSprites.Length; i++)
		{
			chaliceSprites[i].enabled = i == spriteToEnable;
		}
	}

	private void ChaliceEndAirEX()
	{
		SetBool(Booleans.ChaliceAirEX, value: false);
		if (base.player.stats.isChalice && !base.player.motor.Grounded)
		{
			switch (exDirection)
			{
			case "Forward":
				base.animator.Play(ChaliceAirEXRecovery, 3, 1f / 12f);
				break;
			case "Up":
			case "Down":
			case "Diagonal_Down":
				base.animator.Play(ChaliceAirEXRecovery, 3, 1f / 24f);
				break;
			case "Diagonal_Up":
				base.animator.Play(ChaliceAirEXRecovery, 3, 0f);
				break;
			}
		}
	}

	public void IsIntroB()
	{
		if (!base.player.stats.isChalice)
		{
			isIntroB = true;
			if ((base.player.id == PlayerId.PlayerOne && PlayerManager.player1IsMugman) || (base.player.id == PlayerId.PlayerTwo && !PlayerManager.player1IsMugman))
			{
				Play("Boil_Mugman");
			}
		}
	}

	public void CookieFail()
	{
		if (Level.Current.CurrentLevel == Levels.Bee && base.player.id == PlayerId.PlayerTwo)
		{
			base.transform.position += Vector3.left * 32f;
		}
		string text = (((base.player.id != 0 || !PlayerManager.player1IsMugman) && (base.player.id != PlayerId.PlayerTwo || PlayerManager.player1IsMugman)) ? "Cuphead" : "Mugman");
		Play("Intro_Chalice_" + text + "_Fail");
	}

	public void ScaredChalice(bool showPortal)
	{
		SetInt(Integers.MoveX, 0);
		inScaredIntro = true;
		ActivateChaliceAnimationLayers();
		base.animator.Play("Intro_Chalice_Scared", 3);
		if (showPortal)
		{
			bool flag = (base.player.id == PlayerId.PlayerOne && PlayerManager.player1IsMugman) || (base.player.id == PlayerId.PlayerTwo && !PlayerManager.player1IsMugman);
			string text = ((!flag) ? "Cuphead" : "Mugman");
			chaliceIntroAnimation.Create(base.transform.position, flag, isScared: true);
		}
	}

	public void ForceDirection()
	{
		lastTrueLookDir = base.player.motor.TrueLookDirection;
	}

	private void InitializeCurseFX()
	{
		curseEffectAngle = Random.Range(0, 360);
		if (curseCharmLevel == 4 && paladinShadows != null)
		{
			paladinShadowPosition = new Vector3[10];
			paladinShadowScale = new Vector3[10];
			paladinShadowSprite = new Sprite[10];
			for (int i = 0; i < 10; i++)
			{
				ref Vector3 reference = ref paladinShadowPosition[i];
				reference = base.transform.position;
				paladinShadowSprite[i] = spriteRenderer.sprite;
				ref Vector3 reference2 = ref paladinShadowScale[i];
				reference2 = base.transform.localScale;
			}
			paladinShadows[0].transform.position = base.transform.position;
			paladinShadows[1].transform.position = base.transform.position;
			paladinShadows[0].sprite = spriteRenderer.sprite;
			paladinShadows[1].sprite = spriteRenderer.sprite;
			paladinShadows[0].enabled = true;
			paladinShadows[1].enabled = true;
			paladinShadows[0].transform.parent = null;
			paladinShadows[1].transform.parent = null;
		}
	}

	private void HandleCurseFX()
	{
		if (PauseManager.state == PauseManager.State.Paused || !showCurseFX)
		{
			return;
		}
		curseEffectTimer += CupheadTime.Delta;
		while (curseEffectTimer >= curseEffectDelay)
		{
			Effect effect = curseEffect.Create(base.player.center + (Vector3)MathUtils.AngleToDirection(curseEffectAngle) * curseDistanceRange.RandomFloat());
			string stateName = null;
			if (curseCharmLevel < 2)
			{
				stateName = ((!Rand.Bool()) ? "Flames" : "Cloud") + Random.Range(0, 3);
			}
			if (curseCharmLevel == 2)
			{
				stateName = ((!Rand.Bool()) ? ("Dizzy" + Random.Range(0, 4)) : ("Cloud" + Random.Range(0, 3)));
			}
			if (curseCharmLevel == 3)
			{
				stateName = "Dizzy" + Random.Range(0, 4);
			}
			if (curseCharmLevel == 4)
			{
				stateName = "Sparkle" + Random.Range(0, 3);
			}
			effect.animator.Play(stateName);
			curseEffectAngle = (curseEffectAngle + curseAngleShiftRange.RandomFloat()) % 360f;
			curseEffectTimer -= curseEffectDelay;
		}
		if (curseCharmLevel == 4 && paladinShadows != null)
		{
			paladinShadows[0].enabled = !base.player.motor.Dashing;
			paladinShadows[1].enabled = !base.player.motor.Dashing;
			for (int num = 9; num > 0; num--)
			{
				ref Vector3 reference = ref paladinShadowPosition[num];
				reference = paladinShadowPosition[num - 1];
				ref Vector3 reference2 = ref paladinShadowScale[num];
				reference2 = paladinShadowScale[num - 1];
				paladinShadowSprite[num] = paladinShadowSprite[num - 1];
			}
			ref Vector3 reference3 = ref paladinShadowPosition[0];
			reference3 = base.transform.position;
			ref Vector3 reference4 = ref paladinShadowScale[0];
			reference4 = base.transform.localScale;
			paladinShadowSprite[0] = spriteRenderer.sprite;
			paladinShadows[0].transform.position = paladinShadowPosition[5];
			paladinShadows[1].transform.position = paladinShadowPosition[9];
			paladinShadows[0].transform.localScale = paladinShadowScale[5];
			paladinShadows[1].transform.localScale = paladinShadowScale[9];
			paladinShadows[0].sprite = paladinShadowSprite[5];
			paladinShadows[1].sprite = paladinShadowSprite[9];
		}
	}

	public void UpdateAnimator()
	{
		Update();
	}

	public override void OnPause()
	{
		base.OnPause();
		SetAlpha(1f);
	}

	private void OnGuiPause()
	{
	}

	private void OnGuiUnpause()
	{
	}

	public void OnShotFired()
	{
		fired = true;
	}

	public void OnRevive(Vector3 pos)
	{
		base.animator.Play("Jump");
	}

	public void OnGravityReversed()
	{
		base.transform.SetScale(null, base.player.motor.GravityReversalMultiplier);
	}

	public override void OnLevelStart()
	{
		CheckIfChaliceAndActivate();
	}

	public void OnLevelWin()
	{
		base.player.damageReceiver.OnWin();
		SetTrigger(Triggers.OnWin);
	}

	private void ActivateChaliceAnimationLayers()
	{
		base.animator.SetLayerWeight(3, 1f);
		base.animator.SetLayerWeight(4, 1f);
		SetChaliceSprites();
		chaliceActivated = true;
	}

	public void CheckIfChaliceAndActivate()
	{
		if (base.player.stats.isChalice)
		{
			ActivateChaliceAnimationLayers();
		}
	}

	private void StartChaliceIntroHold(bool fail)
	{
		if (!Level.Current.Started && !Level.Current.blockChalice)
		{
			bool flag = (!PlayerManager.player1IsMugman && base.player.id == PlayerId.PlayerOne) || (PlayerManager.player1IsMugman && base.player.id != PlayerId.PlayerOne);
			if (fail)
			{
				base.animator.Play((!flag) ? "Intro_Chalice_Mugman_Fail_Start" : "Intro_Chalice_Cuphead_Fail_Start");
				return;
			}
			base.animator.Play("Intro_Chalice_Hold");
			chaliceIntroCurrent = chaliceIntroAnimation.Create(base.transform.position + Vector3.down * base.player.motor.DistanceToGround(), !flag, isScared: false);
			SetChaliceSprites();
		}
	}

	public void PlayIntro()
	{
		SetBool(Booleans.Intro, value: true);
		string text = (((base.player.id != 0 || !PlayerManager.player1IsMugman) && (base.player.id != PlayerId.PlayerTwo || PlayerManager.player1IsMugman)) ? "Cuphead" : "Mugman");
		if (SceneLoader.CurrentLevel != Levels.Devil && SceneLoader.CurrentLevel != Levels.Saltbaker)
		{
			if (base.player.stats.isChalice)
			{
				base.animator.Play("Idle", 0);
				base.animator.Play("Intro_Chalice_" + text, 3);
				if ((bool)chaliceIntroCurrent)
				{
					chaliceIntroCurrent.EndHold();
				}
				ActivateChaliceAnimationLayers();
			}
			else if (base.player.stats.Loadout.charm != Charm.charm_chalice || Level.Current.blockChalice)
			{
				string empty = string.Empty;
				empty = ((!isIntroB) ? "Intro_" : "Intro_B_");
				Play(empty + text);
			}
		}
		else if (!base.player.stats.isChalice)
		{
			if (base.player.id == PlayerId.PlayerOne)
			{
				AudioManager.Play("player_scared_intro");
			}
			inScaredIntro = true;
			Play("Intro_Scared");
		}
	}

	public void ScaredSprite(bool facingLeft)
	{
		base.animator.enabled = false;
		base.enabled = false;
		base.player.motor.enabled = false;
		if (base.player.id == PlayerId.PlayerOne)
		{
			cuphead.GetComponent<SpriteRenderer>().sprite = cupheadScaredSprite;
			cuphead.GetComponent<SpriteRenderer>().flipX = facingLeft;
		}
		else
		{
			mugman.GetComponent<SpriteRenderer>().sprite = mugmanScaredSprite;
			mugman.GetComponent<SpriteRenderer>().flipX = facingLeft;
		}
	}

	public void LevelInit()
	{
		bool sprites = (!PlayerManager.player1IsMugman && base.player.id == PlayerId.PlayerOne) || (PlayerManager.player1IsMugman && base.player.id != PlayerId.PlayerOne);
		SetSprites(sprites);
	}

	public void SetSprites(bool isCuphead)
	{
		cuphead.SetActive(isCuphead);
		mugman.SetActive(!isCuphead);
		chalice.SetActive(value: false);
		if (isCuphead)
		{
			spriteRenderer = cuphead.GetComponent<SpriteRenderer>();
		}
		else
		{
			spriteRenderer = mugman.GetComponent<SpriteRenderer>();
		}
		tempMaterial = spriteRenderer.material;
	}

	private void SetChaliceSprites()
	{
		cuphead.SetActive(value: false);
		mugman.SetActive(value: false);
		chalice.SetActive(value: true);
		spriteRenderer = chalice.GetComponent<SpriteRenderer>();
	}

	public void EnableSpriteRenderer()
	{
		spriteRenderer.enabled = true;
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (!base.player.stats.SuperInvincible)
		{
			CupheadLevelCamera.Current.Shake(20f, 0.6f);
			if (base.player.stats.Health == 4)
			{
				AudioManager.Play("player_damage_crack_level1");
			}
			else if (base.player.stats.Health == 3)
			{
				AudioManager.Play("player_damage_crack_level2");
			}
			else if (base.player.stats.Health == 2)
			{
				AudioManager.Play("player_damage_crack_level3");
			}
			else if (base.player.stats.Health == 1)
			{
				AudioManager.Play("player_damage_crack_level4");
			}
			AudioManager.Play("player_hit");
			if (base.player.motor.Grounded)
			{
				Play("Hit.Hit_Ground");
			}
			else
			{
				Play("Hit.Hit_Air");
			}
			hitAnimation = true;
			hitEffect.Create(base.player.center, base.transform.localScale);
		}
	}

	public void OnHealerCharm()
	{
		healerCharmEffect.Create(base.player.center, base.transform.localScale, base.player);
		AudioManager.Play("sfx_player_charmhealer_extraheart");
	}

	private void OnDashStart()
	{
		hitAnimation = false;
		if ((base.player.stats.Loadout.charm == Charm.charm_smoke_dash || base.player.stats.CurseSmokeDash) && !Level.IsChessBoss)
		{
			spriteRenderer.enabled = false;
			smokeDashEffect.Create(base.player.center);
		}
	}

	private void OnDashEnd()
	{
		if ((base.player.stats.Loadout.charm == Charm.charm_smoke_dash || base.player.stats.CurseSmokeDash) && !Level.IsChessBoss)
		{
			spriteRenderer.enabled = true;
			smokeDashEffect.Create(base.player.center);
		}
		if (!base.player.motor.Grounded && base.player.stats.isChalice)
		{
			base.animator.Play((!base.player.motor.ChaliceDoubleJumped) ? ChaliceJumpDescend : ChaliceJumpBall, 3, 0f);
		}
	}

	private void OnRunDust()
	{
		if (base.enabled)
		{
			runEffect.Create(runDustRoot.position);
		}
	}

	private void OnChaliceDashSparkle()
	{
		if (base.enabled && base.player.stats.isChalice)
		{
			chaliceDashSparkle.Create(sparkleRoot.position);
		}
	}

	private void OnBurst()
	{
		powerUpBurstEffect.Create(base.player.center);
	}

	private void onHitAnimationComplete()
	{
		hitAnimation = false;
	}

	public void SetSpriteProperties(SpriteLayer layer, int order)
	{
		spriteRenderer.sortingLayerName = layer.ToString();
		spriteRenderer.sortingOrder = order;
	}

	public void ResetSpriteProperties()
	{
		spriteRenderer.sortingLayerName = SpriteLayer.Player.ToString();
		spriteRenderer.sortingOrder = ((base.player.id == PlayerId.PlayerOne) ? 1 : (-1));
	}

	private void OnParryStart()
	{
		if (!super)
		{
			if (base.player.stats.Loadout.charm == Charm.charm_parry_plus && !Level.IsChessBoss)
			{
				SetBool(Booleans.HasParryCharm, value: true);
			}
			if ((base.player.stats.Loadout.charm == Charm.charm_parry_attack || base.player.stats.CurseWhetsone) && !GetComponent<IParryAttack>().AttackParryUsed && !Level.IsChessBoss)
			{
				SetBool(Booleans.HasParryAttack, value: true);
			}
			else if (base.player.stats.Loadout.charm == Charm.charm_curse)
			{
				SetBool(Booleans.HasParryAttack, value: false);
			}
			SetTrigger(Triggers.OnParry);
		}
	}

	public void OnParrySuccess()
	{
		if (base.player.stats.Loadout.charm == Charm.charm_parry_plus && !Level.IsChessBoss)
		{
			SetBool(Booleans.HasParryCharm, value: false);
		}
		if ((base.player.stats.Loadout.charm == Charm.charm_parry_attack || base.player.stats.CurseWhetsone) && !Level.IsChessBoss)
		{
			SetBool(Booleans.HasParryAttack, value: false);
		}
		SetAlpha(1f);
		if (base.player.stats.isChalice)
		{
			if (chaliceDashEffectActive != null)
			{
				Object.Destroy(chaliceDashEffectActive.gameObject);
			}
			base.animator.Play("Jump_Launch", 3, 0f);
		}
	}

	public void OnParryPause()
	{
		if (base.gameObject.activeInHierarchy)
		{
			base.animator.enabled = false;
			spriteRenderer.GetComponent<LevelPlayerParryAnimator>().StartSet();
		}
	}

	public void OnParryAnimEnd()
	{
		ResumeNormanAnim();
	}

	public void _ChaliceStartOnIdle4()
	{
		if (base.player.stats.isChalice)
		{
			SetBool(Booleans.ChaliceOffIdle, value: false);
			base.animator.Play("IdleFromFour", 3);
		}
	}

	public void ResumeNormanAnim()
	{
		spriteRenderer.GetComponent<LevelPlayerParryAnimator>().StopSet();
		base.animator.enabled = true;
	}

	private void OnGrounded()
	{
		if (Level.Current.Started)
		{
			AudioManager.Play("player_grounded");
			groundedEffect.Create(base.transform.position, base.transform.localScale);
		}
	}

	private void OnEx()
	{
		if (base.player.stats.isChalice)
		{
			SetBool(Booleans.ChaliceOffIdle, value: true);
		}
		exDirection = "Forward";
		if ((int)base.player.motor.LookDirection.x == 0 && (int)base.player.motor.LookDirection.y > 0)
		{
			exDirection = "Up";
			AudioManager.Play("player_ex_forward_ground");
		}
		else if ((int)base.player.motor.LookDirection.x != 0 && (int)base.player.motor.LookDirection.y > 0)
		{
			exDirection = "Diagonal_Up";
			AudioManager.Play("player_ex_forward_ground");
		}
		else if ((int)base.player.motor.LookDirection.x == 0 && (int)base.player.motor.LookDirection.y < 0)
		{
			exDirection = "Down";
			AudioManager.Play("player_ex_forward_ground");
		}
		else if ((int)base.player.motor.LookDirection.x != 0 && (int)base.player.motor.LookDirection.y < 0)
		{
			exDirection = "Diagonal_Down";
			AudioManager.Play("player_ex_forward_ground");
		}
		if (exDirection == "Forward")
		{
			AudioManager.Play("player_ex_forward_ground");
		}
		string text = "Ex." + exDirection + "_";
		text = ((!base.player.motor.Grounded) ? (text + "Air") : (text + "Ground"));
		Play(text);
		SetBool(Booleans.ChaliceAirEX, !base.player.motor.Grounded);
	}

	private void OnSuper()
	{
		Super super = PlayerData.Data.Loadouts.GetPlayerLoadout(base.player.id).super;
		this.super = true;
		if (base.player.stats.isChalice)
		{
			shooting = false;
			ChaliceJumpShootHandling();
		}
		spriteRenderer.enabled = false;
		SwitchChaliceAim(-1);
	}

	private void OnSuperEnd()
	{
		super = false;
		spriteRenderer.enabled = true;
		ResetSpriteProperties();
		if (base.player.stats.isChalice)
		{
			timeSinceStoppedShooting = 1f;
			if (base.player.stats.Loadout.super == Super.level_super_chalice_shield)
			{
				StartCoroutine(end_chalice_super_cr((!base.player.motor.Grounded) ? ChaliceSuper2ReturnAir : ChaliceSuper2Return));
			}
			if (base.player.stats.Loadout.super == Super.level_super_chalice_vert_beam)
			{
				StartCoroutine(end_chalice_super_cr(ChaliceSuper1Return));
			}
		}
	}

	private bool ExitingChaliceSuper()
	{
		int shortNameHash = base.animator.GetCurrentAnimatorStateInfo(3).shortNameHash;
		return shortNameHash == ChaliceSuper1Return || shortNameHash == ChaliceSuper2Return || shortNameHash == ChaliceSuper2ReturnAir;
	}

	private IEnumerator end_chalice_super_cr(int animState)
	{
		base.animator.Play(animState, 3, 0f);
		base.animator.Update(0f);
		if (base.player.weaponManager.allowInput)
		{
			base.player.weaponManager.DisableInput();
			while (base.animator.GetCurrentAnimatorStateInfo(3).shortNameHash == animState)
			{
				yield return null;
			}
			base.player.weaponManager.EnableInput();
		}
	}

	private void _OnSuperAnimEnd()
	{
		base.player.UnpauseAll();
		base.player.motor.OnSuperEnd();
	}

	public void SetOldMaterial()
	{
		spriteRenderer.material = tempMaterial;
	}

	public void SetMaterial(Material m)
	{
		tempMaterial = spriteRenderer.material;
		spriteRenderer.material = m;
	}

	public Material GetMaterial()
	{
		return spriteRenderer.material;
	}

	public SpriteRenderer GetSpriteRenderer()
	{
		return spriteRenderer;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		dashEffect = null;
		groundedEffect = null;
		hitEffect = null;
		runEffect = null;
		smokeDashEffect = null;
		powerUpBurstEffect = null;
		cupheadScaredSprite = null;
		mugmanScaredSprite = null;
	}

	protected void Play(string animation)
	{
		base.animator.Play(animation, 0, 0f);
	}

	protected bool GetBool(int b)
	{
		return base.animator.GetBool(b);
	}

	protected void SetBool(int b, bool value)
	{
		base.animator.SetBool(b, value);
	}

	protected int GetInt(int i)
	{
		return base.animator.GetInteger(i);
	}

	protected void SetInt(int i, int value)
	{
		base.animator.SetInteger(i, value);
	}

	protected void SetTrigger(int t)
	{
		base.animator.SetTrigger(t);
	}

	protected void ResetTrigger(int t)
	{
		base.animator.ResetTrigger(t);
	}

	private void SetAlpha(float a)
	{
		Color color = spriteRenderer.color;
		color.a = a;
		spriteRenderer.color = color;
	}

	public void SetColor(Color color)
	{
		float a = spriteRenderer.color.a;
		color.a = a;
		spriteRenderer.color = color;
	}

	public void ResetColor()
	{
		float a = spriteRenderer.color.a;
		spriteRenderer.color = new Color(1f, 1f, 1f, a);
	}

	public void SetColorOverTime(Color color, float time)
	{
		StopColorCoroutine();
		colorCoroutine = setColor_cr(color, time);
		StartCoroutine(colorCoroutine);
	}

	public void StopColorCoroutine()
	{
		if (colorCoroutine != null)
		{
			StopCoroutine(colorCoroutine);
		}
		colorCoroutine = null;
	}

	private IEnumerator setColor_cr(Color color, float time)
	{
		float t = 0f;
		Color startColor = spriteRenderer.color;
		while (t < time)
		{
			float val = t / time;
			SetColor(Color.Lerp(startColor, color, val));
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		SetColor(color);
		yield return null;
	}

	private IEnumerator flash_cr()
	{
		float t3 = 0f;
		while (true)
		{
			if (!Flashing)
			{
				yield return true;
				continue;
			}
			yield return CupheadTime.WaitForSeconds(this, 0.417f);
			while (Flashing)
			{
				SetAlpha(0.3f);
				t3 = 0f;
				while (t3 < 0.05f)
				{
					if (!Flashing)
					{
						SetAlpha(1f);
						break;
					}
					t3 += base.LocalDeltaTime;
					yield return null;
				}
				if (!Flashing)
				{
					SetAlpha(1f);
					break;
				}
				SetAlpha(1f);
				t3 = 0f;
				while (t3 < 0.2f)
				{
					if (!Flashing)
					{
						SetAlpha(1f);
						break;
					}
					t3 += base.LocalDeltaTime;
					yield return null;
				}
				if (!Flashing)
				{
					SetAlpha(1f);
					break;
				}
			}
			yield return null;
		}
	}

	private void SoundIntroPowerup()
	{
		if (!intropowerupactive)
		{
			AudioManager.Play("player_powerup");
			emitAudioFromObject.Add("player_powerup");
			intropowerupactive = true;
		}
	}

	private void SoundParryAxe()
	{
		AudioManager.Play("player_parry_axe");
		emitAudioFromObject.Add("player_parry_axe");
	}
}
