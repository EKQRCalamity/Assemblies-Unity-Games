using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class SaltbakerLevel : Level
{
	private LevelProperties.Saltbaker properties;

	private const float FIRE_OFFSET_MODIFIER = 1f;

	[SerializeField]
	private GameObject phase3BG;

	[SerializeField]
	private GameObject[] cracksBG;

	[SerializeField]
	private SaltbakerLevelCutter cutterPrefab;

	private List<SaltbakerLevelCutter> cutters;

	[SerializeField]
	private SaltbakerLevelJumper jumperPrefab;

	private List<SaltbakerLevelJumper> fires;

	[SerializeField]
	private SaltbakerLevelSaltbaker saltbaker;

	[SerializeField]
	private SaltbakerLevelBouncer saltbakerBouncer;

	[SerializeField]
	private SaltbakerLevelPillarHandler saltbakerPillarHandler;

	[SerializeField]
	private SpriteRenderer skyFront;

	[SerializeField]
	private SpriteRenderer transitionFader;

	[SerializeField]
	private SaltbakerLevelPhaseThreeToFourTransition phase3to4Transition;

	[SerializeField]
	private string saltSpillageOrderString;

	private PatternString saltSpillageOrder;

	[SerializeField]
	private string saltSpillageDelayString;

	private PatternString saltSpillageDelay;

	[SerializeField]
	private Animator groundCrack;

	[SerializeField]
	private Animator tornadoActivator;

	[SerializeField]
	private GameObject phaseFourBlurCamera;

	[SerializeField]
	private MeshRenderer phaseFourBlurTexture;

	[SerializeField]
	private float phaseFourBlurAmount = 3f;

	[SerializeField]
	private float phaseFourDimAmount = 0.8f;

	[SerializeField]
	private float phaseFourBlurDimTime = 1f;

	[SerializeField]
	private BlurOptimized phaseFourBlurController;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortraitMain;

	[SerializeField]
	private Sprite _bossPortraitPhaseTwo;

	[SerializeField]
	private Sprite _bossPortraitPhaseThree;

	[SerializeField]
	private Sprite _bossPortraitPhaseFour;

	[SerializeField]
	private string _bossQuoteMain;

	[SerializeField]
	private string _bossQuotePhaseTwo;

	[SerializeField]
	private string _bossQuotePhaseThree;

	[SerializeField]
	private string _bossQuotePhaseFour;

	private int crackOn;

	public float yScrollPos;

	[SerializeField]
	private SaltbakerLevelBGTrappedCharacter trappedCharacter;

	[SerializeField]
	private SaltbakerLevelBGTrappedCharacter trappedCharacterPhaseThree;

	public override Levels CurrentLevel => Levels.Saltbaker;

	public override Scenes CurrentScene => Scenes.scene_level_saltbaker;

	public override Sprite BossPortrait
	{
		get
		{
			switch (properties.CurrentState.stateName)
			{
			case LevelProperties.Saltbaker.States.Main:
				return _bossPortraitMain;
			case LevelProperties.Saltbaker.States.PhaseTwo:
				return _bossPortraitPhaseTwo;
			case LevelProperties.Saltbaker.States.PhaseThree:
				return _bossPortraitPhaseThree;
			case LevelProperties.Saltbaker.States.PhaseFour:
				return _bossPortraitPhaseFour;
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
			case LevelProperties.Saltbaker.States.Main:
				return _bossQuoteMain;
			case LevelProperties.Saltbaker.States.PhaseTwo:
				return _bossQuotePhaseTwo;
			case LevelProperties.Saltbaker.States.PhaseThree:
				return _bossQuotePhaseThree;
			case LevelProperties.Saltbaker.States.PhaseFour:
				return _bossQuotePhaseFour;
			default:
				Debug.LogError(string.Concat("Couldn't find quote for state ", properties.CurrentState.stateName, ". Using Main."));
				return _bossQuoteMain;
			}
		}
	}

	public bool playerLost { get; private set; }

	protected override void PartialInit()
	{
		properties = LevelProperties.Saltbaker.GetMode(base.mode);
		properties.OnStateChange += base.zHack_OnStateChanged;
		properties.OnBossDeath += base.zHack_OnWin;
		base.timeline = properties.CreateTimeline(base.mode);
		goalTimes = properties.goalTimes;
		properties.OnBossDamaged += base.timeline.DealDamage;
		base.PartialInit();
	}

	protected override void Start()
	{
		base.Start();
		trappedCharacter.Setup();
		trappedCharacterPhaseThree.Setup();
		saltbaker.LevelInit(properties);
		saltbakerBouncer.LevelInit(properties);
		saltbakerPillarHandler.LevelInit(properties);
		fires = new List<SaltbakerLevelJumper>();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_bossPortraitMain = null;
		_bossPortraitPhaseTwo = null;
		_bossPortraitPhaseThree = null;
		_bossPortraitPhaseFour = null;
	}

	protected override void OnLose()
	{
		playerLost = true;
	}

	protected override void OnStateChanged()
	{
		base.OnStateChanged();
		if (properties.CurrentState.stateName == LevelProperties.Saltbaker.States.PhaseTwo)
		{
			StopAllCoroutines();
			StartCoroutine(saltbaker.phase_one_to_two_cr());
		}
		else if (properties.CurrentState.stateName == LevelProperties.Saltbaker.States.PhaseThree)
		{
			StopAllCoroutines();
			KillFires();
			saltbaker.OnPhaseThree();
		}
		else if (properties.CurrentState.stateName == LevelProperties.Saltbaker.States.PhaseFour)
		{
			StopAllCoroutines();
			StartCoroutine(phase_three_to_four_cr());
		}
	}

	public void StartPhase3()
	{
		ClearFires();
		phase3BG.SetActive(value: true);
		bounds.bottomEnabled = false;
		GameObject.Find("Level_Ground").SetActive(value: false);
		Level.Current.SetBounds(null, null, 500, null);
		foreach (LevelPlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (allPlayer != null)
			{
				allPlayer.transform.position = new Vector3(allPlayer.transform.position.x, Level.Current.Ceiling);
				allPlayer.motor.DoPostSuperHop();
			}
		}
		SpawnCutters();
		StartCoroutine(phase_two_to_three_cr());
	}

	private IEnumerator phase_two_to_three_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		float t = 0f;
		while (t < 1f)
		{
			t += CupheadTime.FixedDelta;
			transitionFader.color = new Color(1f, 1f, 1f, Mathf.InverseLerp(1f, 0f, t));
			yield return wait;
		}
		saltbakerBouncer.gameObject.SetActive(value: true);
		saltbakerBouncer.StartBouncer(new Vector3(0f, 700f));
		transitionFader.gameObject.SetActive(value: false);
	}

	private IEnumerator phase_three_to_four_cr()
	{
		DestroyRunners();
		saltbakerBouncer.EndBouncer();
		groundCrack.Play("Start");
		StartCoroutine(flash_sky_cr());
		tornadoActivator.enabled = true;
		phase3to4Transition.StartSaltman();
		while (saltbakerBouncer != null)
		{
			yield return null;
		}
		saltbakerPillarHandler.gameObject.SetActive(value: true);
		saltbakerPillarHandler.StartPlatforms();
		yield return CupheadTime.WaitForSeconds(this, 1f);
		saltbakerPillarHandler.suppressCenterPlatforms = false;
		yield return CupheadTime.WaitForSeconds(this, 0.5f);
		groundCrack.SetTrigger("Continue");
		tornadoActivator.SetTrigger("Continue");
		saltbakerPillarHandler.StartPillarOfDoom();
		while (phase3to4Transition.enabled)
		{
			yield return null;
		}
		yield return CupheadTime.WaitForSeconds(this, 1f);
		saltbakerPillarHandler.StartHeart();
		StartCoroutine(bg_salt_spillage_cr());
		UnityEngine.Camera.main.cullingMask ^= 1 << LayerMask.NameToLayer("Renderer");
		phaseFourBlurCamera.SetActive(value: true);
		phaseFourBlurTexture.gameObject.SetActive(value: true);
		float t = 0f;
		while (t < phaseFourBlurDimTime)
		{
			t += (float)CupheadTime.Delta;
			float tNormalized = t / phaseFourBlurDimTime;
			phaseFourBlurController.blurSize = Mathf.Lerp(0f, phaseFourBlurAmount, tNormalized);
			phaseFourBlurTexture.material.color = Color.Lerp(Color.white, new Color(phaseFourDimAmount, phaseFourDimAmount, phaseFourDimAmount, 1f), tNormalized);
			yield return null;
		}
		phaseFourBlurController.blurSize = phaseFourBlurAmount;
		phaseFourBlurTexture.material.color = new Color(phaseFourDimAmount, phaseFourDimAmount, phaseFourDimAmount, 1f);
	}

	private IEnumerator flash_sky_cr()
	{
		while (true)
		{
			float dimRate = Random.Range(2f, 4f);
			skyFront.color = Color.white;
			while (skyFront.color.r > 0f)
			{
				float c = skyFront.color.r - 1f / 24f * dimRate;
				skyFront.color = new Color(c, c, c, 1f);
				yield return CupheadTime.WaitForSeconds(this, 1f / 24f);
			}
			yield return CupheadTime.WaitForSeconds(this, Random.Range(0.1f, 4f));
		}
	}

	private IEnumerator bg_salt_spillage_cr()
	{
		saltSpillageDelay = new PatternString(saltSpillageDelayString);
		saltSpillageOrder = new PatternString(saltSpillageOrderString);
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, saltSpillageDelay.PopFloat());
			groundCrack.Play("Spill", saltSpillageOrder.PopInt(), 0f);
		}
	}

	public void SpawnJumpers()
	{
		int numberFireJumpers = properties.CurrentState.jumper.numberFireJumpers;
		if (numberFireJumpers != 0)
		{
			float y = Level.Current.Ceiling;
			float num = Level.Current.Left;
			float num2 = Level.Current.Width / numberFireJumpers;
			float num3 = num + num2 / 2f;
			float jumpDelay = properties.CurrentState.jumper.jumpDelay;
			for (int i = 0; i < numberFireJumpers; i++)
			{
				float x = num3 + num2 * (float)i;
				Vector3 position = new Vector3(x, y);
				SaltbakerLevelJumper saltbakerLevelJumper = jumperPrefab.Create(position, this, properties.CurrentState.swooper, properties.CurrentState.jumper, jumpDelay * (float)i, isSwooper: false);
				saltbakerLevelJumper.GetComponent<SpriteRenderer>().sortingOrder = i + -5;
				fires.Add(saltbakerLevelJumper);
			}
		}
	}

	public void SpawnSwoopers()
	{
		float[] array = new float[2]
		{
			(float)Level.Current.Right / 2f,
			(float)Level.Current.Left / 2f
		};
		int num = properties.CurrentState.swooper.numberFireSwoopers;
		if (num > 2)
		{
			Debug.Break();
			num = 2;
		}
		if (num != 0)
		{
			float num2 = (float)(Level.Current.Left + Level.Current.Right) / 2f;
			float jumpDelay = properties.CurrentState.swooper.jumpDelay;
			for (int i = 0; i < num; i++)
			{
				SaltbakerLevelJumper saltbakerLevelJumper = jumperPrefab.Create(new Vector3(array[i], Level.Current.Ceiling), this, properties.CurrentState.swooper, properties.CurrentState.jumper, jumpDelay * (float)i, isSwooper: true);
				saltbakerLevelJumper.GetComponent<SpriteRenderer>().sortingOrder = 3 + i;
				fires.Add(saltbakerLevelJumper);
			}
		}
	}

	public void KillFires()
	{
		foreach (SaltbakerLevelJumper fire in fires)
		{
			fire.Die();
		}
	}

	public void ClearFires()
	{
		foreach (SaltbakerLevelJumper fire in fires)
		{
			if (fire != null)
			{
				Object.Destroy(fire.gameObject);
			}
		}
		fires.Clear();
	}

	public bool IsPositionAvailable(Vector3 pos, SaltbakerLevelJumper fire)
	{
		float num = fire.GetComponent<Collider2D>().bounds.size.x * 1f;
		for (int i = 0; i < fires.Count; i++)
		{
			if (fires[i] != fire)
			{
				float num2 = pos.x + num;
				float num3 = pos.x - num;
				if (num2 > fires[i].GetAimPos().x - num && num3 < fires[i].GetAimPos().x + num)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void SpawnCutters()
	{
		LevelProperties.Saltbaker.Cutter cutter = properties.CurrentState.cutter;
		if (cutter.cutterCount > 0)
		{
			cutters = new List<SaltbakerLevelCutter>();
			AbstractPlayerController next = PlayerManager.GetNext();
			float num = 50f;
			float minDistance = 50f;
			float[] array = new float[2];
			bool flag = Rand.Bool();
			List<Vector2> list = new List<Vector2>();
			float num2 = Mathf.Min(PlayerManager.GetNext().transform.position.x, PlayerManager.GetNext().transform.position.x);
			float num3 = Mathf.Max(PlayerManager.GetNext().transform.position.x, PlayerManager.GetNext().transform.position.x);
			list.Add(new Vector2((float)Level.Current.Left + num, num2));
			list.Add(new Vector2(num2, num3));
			list.Add(new Vector2(num3, (float)Level.Current.Right - num));
			list.RemoveAll((Vector2 s) => Mathf.Abs(s.x - s.y) < minDistance * 2f);
			list.Sort((Vector2 s1, Vector2 s2) => Mathf.Abs(s1.x - s1.y).CompareTo(Mathf.Abs(s2.x - s2.y)));
			if (list.Count == 3)
			{
				list.RemoveAt(0);
			}
			if (list.Count == 2)
			{
				array[0] = Mathf.Lerp(list[0].x, list[0].y, 0.5f);
				array[1] = Mathf.Lerp(list[1].x, list[1].y, 0.5f);
			}
			if (list.Count == 1)
			{
				array[0] = Mathf.Lerp(list[0].x, list[0].y, 0.333f);
				array[1] = Mathf.Lerp(list[0].x, list[0].y, 0.667f);
			}
			for (int i = 0; i < cutter.cutterCount; i++)
			{
				Vector3 position = new Vector3(array[i], (float)Level.Current.Ground + 26f);
				SaltbakerLevelCutter item = cutterPrefab.Create(position, cutter.cutterSpeed, flag, i);
				flag = !flag;
				cutters.Add(item);
			}
		}
	}

	private void DestroyRunners()
	{
		if (cutters == null)
		{
			return;
		}
		foreach (SaltbakerLevelCutter cutter in cutters)
		{
			cutter.Sink();
		}
		cutters.Clear();
	}
}
