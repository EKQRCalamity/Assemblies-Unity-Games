using System;
using System.Collections;
using UnityEngine;

public class SnowCultLevel : Level
{
	private LevelProperties.SnowCult properties;

	private const float CLIMBING_PLATFORMS_INTERAPPEAR_DELAY = 0.2f;

	private const float HEIGHT_TO_START_PHASE_THREE = -80f;

	private const float PHASE_THREE_CAMERA_POS = 950f;

	[SerializeField]
	private SnowCultLevelWizard wizard;

	[SerializeField]
	private SnowCultLevelYeti yeti;

	[SerializeField]
	private SnowCultLevelJackFrost jackFrost;

	[SerializeField]
	private Animator cultists;

	[SerializeField]
	private GameObject pit;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortraitMain;

	[SerializeField]
	private Sprite _bossPortraitPhaseTwo;

	[SerializeField]
	private Sprite _bossPortraitPhaseThree;

	[SerializeField]
	private string _bossQuoteMain;

	[SerializeField]
	private string _bossQuotePhaseTwo;

	[SerializeField]
	private string _bossQuotePhaseThree;

	private bool firstAttack = true;

	public override Levels CurrentLevel => Levels.SnowCult;

	public override Scenes CurrentScene => Scenes.scene_level_snow_cult;

	public override Sprite BossPortrait
	{
		get
		{
			switch (properties.CurrentState.stateName)
			{
			case LevelProperties.SnowCult.States.Main:
				return _bossPortraitMain;
			case LevelProperties.SnowCult.States.Yeti:
			case LevelProperties.SnowCult.States.EasyYeti:
				return _bossPortraitPhaseTwo;
			case LevelProperties.SnowCult.States.JackFrost:
				return _bossPortraitPhaseThree;
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
			case LevelProperties.SnowCult.States.Main:
				return _bossQuoteMain;
			case LevelProperties.SnowCult.States.Yeti:
			case LevelProperties.SnowCult.States.EasyYeti:
				return _bossQuotePhaseTwo;
			case LevelProperties.SnowCult.States.JackFrost:
				return _bossQuotePhaseThree;
			default:
				Debug.LogError(string.Concat("Couldn't find quote for state ", properties.CurrentState.stateName, ". Using Main."));
				return _bossQuoteMain;
			}
		}
	}

	public event Action OnYetiHitGround;

	protected override void PartialInit()
	{
		properties = LevelProperties.SnowCult.GetMode(base.mode);
		properties.OnStateChange += base.zHack_OnStateChanged;
		properties.OnBossDeath += base.zHack_OnWin;
		base.timeline = properties.CreateTimeline(base.mode);
		goalTimes = properties.goalTimes;
		properties.OnBossDamaged += base.timeline.DealDamage;
		base.PartialInit();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_bossPortraitMain = null;
		_bossPortraitPhaseTwo = null;
		_bossPortraitPhaseThree = null;
	}

	protected override void Start()
	{
		base.Start();
		yeti.LevelInit(properties);
		jackFrost.LevelInit(properties);
		wizard.LevelInit(properties);
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(snowcultPattern_cr());
	}

	private IEnumerator snowcultPattern_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		while (true)
		{
			yield return StartCoroutine(nextPattern_cr());
			yield return null;
		}
	}

	protected override void OnStateChanged()
	{
		base.OnStateChanged();
		if (properties.CurrentState.stateName == LevelProperties.SnowCult.States.Yeti)
		{
			StartCoroutine(to_phase_2_cr());
		}
		else if (properties.CurrentState.stateName == LevelProperties.SnowCult.States.JackFrost)
		{
			StartCoroutine(to_phase_3_cr());
		}
		else if (properties.CurrentState.stateName == LevelProperties.SnowCult.States.EasyYeti)
		{
			StartCoroutine(to_phase_3_easy_cr());
		}
	}

	private IEnumerator nextPattern_cr()
	{
		while (wizard != null && (wizard.Turning() || wizard.dead))
		{
			yield return null;
		}
		LevelProperties.SnowCult.Pattern p = properties.CurrentState.NextPattern;
		if (firstAttack)
		{
			while (p != LevelProperties.SnowCult.Pattern.Quad)
			{
				p = properties.CurrentState.NextPattern;
			}
			firstAttack = false;
		}
		switch (p)
		{
		case LevelProperties.SnowCult.Pattern.Switch:
			yield return StartCoroutine(switch_cr());
			break;
		case LevelProperties.SnowCult.Pattern.Eye:
			yield return StartCoroutine(eye_attack_cr());
			break;
		case LevelProperties.SnowCult.Pattern.Shard:
			yield return StartCoroutine(shard_attack_cr());
			break;
		case LevelProperties.SnowCult.Pattern.Mouth:
			yield return StartCoroutine(mouth_shot_cr());
			break;
		case LevelProperties.SnowCult.Pattern.Quad:
			yield return StartCoroutine(quad_cr());
			break;
		case LevelProperties.SnowCult.Pattern.Block:
			yield return StartCoroutine(ice_block_cr());
			break;
		case LevelProperties.SnowCult.Pattern.SeriesShot:
			yield return StartCoroutine(series_shot_cr());
			break;
		case LevelProperties.SnowCult.Pattern.Yeti:
			break;
		default:
			yield return CupheadTime.WaitForSeconds(this, 1f);
			break;
		}
	}

	private IEnumerator to_phase_2_cr()
	{
		firstAttack = false;
		wizard.ToOutro(yeti);
		yield return null;
	}

	public void CultistsSummon()
	{
		cultists.SetTrigger("Summon");
	}

	public void YetiHitGround()
	{
		if (this.OnYetiHitGround != null)
		{
			this.OnYetiHitGround();
		}
		cultists.SetTrigger("Summon");
	}

	private IEnumerator to_phase_3_easy_cr()
	{
		yeti.ToEasyPhaseThree();
		yield return null;
	}

	private IEnumerator to_phase_3_cr()
	{
		yeti.ForceOutroToStart();
		while (yeti.state != SnowCultLevelYeti.States.Idle || yeti.inBallForm)
		{
			yield return null;
		}
		cultists.SetTrigger("Summon");
		yeti.OnDeath();
		jackFrost.Intro();
		yield return CupheadTime.WaitForSeconds(this, properties.CurrentState.yeti.timeToPlatforms);
		jackFrost.CreatePlatforms();
		StartCoroutine(SFX_SNOWCULT_IcePlatformAppear_cr());
		StartCoroutine(SFX_SNOWCULT_P2_to_P3_Transition_cr());
		for (int i = 0; i < 5; i++)
		{
			jackFrost.CreateAscendingPlatform(i);
			if (i < 4)
			{
				yield return CupheadTime.WaitForSeconds(this, 0.2f);
			}
		}
		AbstractPlayerController player1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		AbstractPlayerController player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		LevelPlayerMotor p1Motor = player1.GetComponent<LevelPlayerMotor>();
		LevelPlayerMotor p2Motor = null;
		bool hasStarted = false;
		while (!hasStarted)
		{
			if (player2 != null && !player2.IsDead)
			{
				if (p2Motor == null)
				{
					p2Motor = player2.GetComponent<LevelPlayerMotor>();
				}
				if ((player1.transform.position.y > -80f && p1Motor.Grounded) || (player2.transform.position.y > -80f && p2Motor.Grounded))
				{
					hasStarted = true;
				}
			}
			else if (player1.transform.position.y > -80f && p1Motor.Grounded)
			{
				hasStarted = true;
			}
			yield return null;
		}
		Vector3 cameraEndPos = new Vector3(0f, 950f, 0f);
		float time = properties.CurrentState.yeti.timeForCameraMove;
		CupheadLevelCamera.Current.ChangeVerticalBounds(1290, 675);
		pit.SetActive(value: true);
		float cameraStartPos = CupheadLevelCamera.Current.transform.position.y;
		StartCoroutine(CupheadLevelCamera.Current.slide_camera_cr(cameraEndPos, time));
		time = 0f;
		while (time < 0.5f)
		{
			time = Mathf.InverseLerp(cameraStartPos, cameraEndPos.y, CupheadLevelCamera.Current.transform.position.y);
			yield return null;
		}
		Level.Current.SetBounds(640, 640, 1290, 675);
		while (time < 0.75f)
		{
			time = Mathf.InverseLerp(cameraStartPos, cameraEndPos.y, CupheadLevelCamera.Current.transform.position.y);
			yield return null;
		}
		jackFrost.StartPhase3();
		pit.transform.parent = null;
		while (time < 0.95f)
		{
			time = Mathf.InverseLerp(cameraStartPos, cameraEndPos.y, CupheadLevelCamera.Current.transform.position.y);
			pit.transform.localPosition = CupheadLevelCamera.Current.transform.position + Vector3.down * 500f;
			yield return null;
		}
		pit.transform.localPosition = cameraEndPos + Vector3.down * 500f;
	}

	private IEnumerator quad_cr()
	{
		while (wizard.state != 0)
		{
			yield return null;
		}
		wizard.StartQuadAttack();
		while (wizard.state != 0)
		{
			yield return null;
		}
	}

	private IEnumerator ice_block_cr()
	{
		while (wizard.state != 0)
		{
			yield return null;
		}
		wizard.Whale();
		while (wizard.state != 0)
		{
			yield return null;
		}
	}

	private IEnumerator series_shot_cr()
	{
		while (wizard.state != 0)
		{
			yield return null;
		}
		wizard.SeriesShot();
		while (wizard.state != 0)
		{
			yield return null;
		}
	}

	private IEnumerator switch_cr()
	{
		while (jackFrost.state != SnowCultLevelJackFrost.States.Idle)
		{
			yield return null;
		}
		jackFrost.StartSwitch();
		while (jackFrost.state != SnowCultLevelJackFrost.States.Idle)
		{
			yield return null;
		}
	}

	private IEnumerator eye_attack_cr()
	{
		while (jackFrost.state != SnowCultLevelJackFrost.States.Idle)
		{
			yield return null;
		}
		jackFrost.StartEyeAttack();
		while (jackFrost.state != SnowCultLevelJackFrost.States.Idle)
		{
			yield return null;
		}
	}

	private IEnumerator shard_attack_cr()
	{
		while (jackFrost.state != SnowCultLevelJackFrost.States.Idle)
		{
			yield return null;
		}
		jackFrost.StartShardAttack();
		while (jackFrost.state != SnowCultLevelJackFrost.States.Idle)
		{
			yield return null;
		}
	}

	private IEnumerator mouth_shot_cr()
	{
		while (jackFrost.state != SnowCultLevelJackFrost.States.Idle)
		{
			yield return null;
		}
		jackFrost.StartMouthShot();
		while (jackFrost.state != SnowCultLevelJackFrost.States.Idle)
		{
			yield return null;
		}
	}

	private IEnumerator SFX_SNOWCULT_IcePlatformAppear_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 0.1f);
		AudioManager.Play("sfx_dlc_snowcult_p2_iceplatform_appear");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p2_iceplatform_appear");
	}

	private IEnumerator SFX_SNOWCULT_P2_to_P3_Transition_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 3f);
		AudioManager.Play("sfx_dlc_snowcult_p2_snow_cultists_wave_hands_transition");
	}
}
