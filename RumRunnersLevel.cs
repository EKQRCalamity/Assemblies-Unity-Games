using System;
using System.Collections;
using UnityEngine;

public class RumRunnersLevel : Level
{
	private LevelProperties.RumRunners properties;

	private static readonly float SpiderTransitionLowerRopeDuration = 0.5f;

	private static readonly float SpiderTransitionLowerBugDuration = 0.3f;

	private static readonly float BridgeDestroyBounceHeight = -1f;

	private static readonly float AnteaterIntroWormTriggerDistance = 500f;

	[SerializeField]
	private Animator ph2SpiderAnimation;

	[SerializeField]
	private RumRunnersLevelSpider spider;

	[SerializeField]
	private RumRunnersLevelWorm worm;

	[SerializeField]
	private RumRunnersLevelAnteater anteater;

	[SerializeField]
	private RumRunnersLevelMobIntroAnimation mobIntro;

	[SerializeField]
	private Effect fullscreenDirtFX;

	[SerializeField]
	private Animator fakeBannerAnimator;

	[SerializeField]
	private GameObject[] destroyedSpritesMiddleA;

	[SerializeField]
	private GameObject[] destroyedSpritesMiddleB;

	[SerializeField]
	private GameObject[] destroyedSpritesUpperA;

	[SerializeField]
	private GameObject[] destroyedSpritesUpperB;

	[SerializeField]
	private LevelPlatform[] destroyedPlatformsMiddle;

	[SerializeField]
	private LevelPlatform[] destroyedPlatformsUpper;

	[SerializeField]
	private LevelPlatform[] swapPlatformsMappingBefore;

	[SerializeField]
	private LevelPlatform[] swapPlatformsMappingAfter;

	[SerializeField]
	private Rangef middlePlatformEffectRange;

	[SerializeField]
	private Rangef topPlatformEffectRange;

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

	public override Levels CurrentLevel => Levels.RumRunners;

	public override Scenes CurrentScene => Scenes.scene_level_rum_runners;

	public override Sprite BossPortrait
	{
		get
		{
			switch (properties.CurrentState.stateName)
			{
			case LevelProperties.RumRunners.States.Main:
				return _bossPortraitMain;
			case LevelProperties.RumRunners.States.Worm:
				return _bossPortraitPhaseTwo;
			case LevelProperties.RumRunners.States.Anteater:
				return _bossPortraitPhaseThree;
			case LevelProperties.RumRunners.States.MobBoss:
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
			case LevelProperties.RumRunners.States.Main:
				return _bossQuoteMain;
			case LevelProperties.RumRunners.States.Worm:
				return _bossQuotePhaseTwo;
			case LevelProperties.RumRunners.States.Anteater:
				return _bossQuotePhaseThree;
			case LevelProperties.RumRunners.States.MobBoss:
				return _bossQuotePhaseFour;
			default:
				Debug.LogError(string.Concat("Couldn't find quote for state ", properties.CurrentState.stateName, ". Using Main."));
				return _bossQuoteMain;
			}
		}
	}

	public event Action<Rangef> OnUpperBridgeDestroy;

	protected override void PartialInit()
	{
		properties = LevelProperties.RumRunners.GetMode(base.mode);
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
		_bossPortraitPhaseFour = null;
		if (CupheadLevelCamera.Current != null)
		{
			CupheadLevelCamera.Current.OnShakeEvent -= onShakeEventHandler;
		}
	}

	protected override void Start()
	{
		base.Start();
		spider.LevelInit(properties);
		worm.LevelInit(properties);
		anteater.LevelInit(properties);
		CupheadLevelCamera.Current.OnShakeEvent += onShakeEventHandler;
	}

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(new Vector3(topPlatformEffectRange.minimum, -1000f), new Vector3(topPlatformEffectRange.minimum, 1000f));
		Gizmos.DrawLine(new Vector3(topPlatformEffectRange.maximum, -1000f), new Vector3(topPlatformEffectRange.maximum, 1000f));
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(new Vector3(middlePlatformEffectRange.minimum, -1000f), new Vector3(middlePlatformEffectRange.minimum, 1000f));
		Gizmos.DrawLine(new Vector3(middlePlatformEffectRange.maximum, -1000f), new Vector3(middlePlatformEffectRange.maximum, 1000f));
	}

	protected override void OnStateChanged()
	{
		base.OnStateChanged();
		StopAllCoroutines();
		if (properties.CurrentState.stateName == LevelProperties.RumRunners.States.Worm)
		{
			StartCoroutine(worm_cr());
		}
		else if (properties.CurrentState.stateName == LevelProperties.RumRunners.States.Anteater)
		{
			StartCoroutine(anteater_cr());
		}
		else if (properties.CurrentState.stateName == LevelProperties.RumRunners.States.MobBoss)
		{
			StartCoroutine(winFakeout_cr());
		}
	}

	private IEnumerator worm_cr()
	{
		spider.Die();
		while (spider != null)
		{
			yield return null;
		}
		yield return CupheadTime.WaitForSeconds(this, 0.2f);
		worm.Setup();
		worm.StartBarrels();
		ph2SpiderAnimation.gameObject.SetActive(value: true);
		yield return ph2SpiderAnimation.WaitForAnimationToEnd(this, "Ph2");
		yield return CupheadTime.WaitForSeconds(this, SpiderTransitionLowerRopeDuration);
		worm.StartWorm(mobIntro.bugGirlDamage);
		yield return CupheadTime.WaitForSeconds(this, SpiderTransitionLowerBugDuration);
		ph2SpiderAnimation.SetTrigger("End");
		RumRunnersLevelPh2StartAnimation ph2 = ph2SpiderAnimation.GetComponent<RumRunnersLevelPh2StartAnimation>();
		while (!ph2.dropped)
		{
			yield return null;
		}
		worm.introDrop = true;
	}

	private IEnumerator anteater_cr()
	{
		worm.StartDeath();
		yield return worm.animator.WaitForAnimationToEnd(this, "Fall");
		while (Mathf.Abs(worm.transform.position.x) > AnteaterIntroWormTriggerDistance)
		{
			yield return null;
		}
		anteater.gameObject.SetActive(value: true);
		yield return anteater.animator.WaitForAnimationToEnd(this, "Intro");
		anteater.StartAnteater();
	}

	public void DestroyMiddleBridge()
	{
		destroyPlatforms(destroyedPlatformsMiddle, destroyedSpritesMiddleA, middlePlatformEffectRange);
	}

	public void DestroyUpperBridge()
	{
		if (this.OnUpperBridgeDestroy != null)
		{
			this.OnUpperBridgeDestroy(topPlatformEffectRange);
		}
		destroyPlatforms(destroyedPlatformsUpper, destroyedSpritesUpperA, topPlatformEffectRange);
	}

	public void ShatterBridges()
	{
		destroyPlatforms(null, destroyedSpritesMiddleB);
		destroyPlatforms(null, destroyedSpritesUpperB);
		float[] array = new float[4] { 0f, 0.25f, 0.5f, 0.75f };
		array.Shuffle();
		float num = 1280f / camera.zoom;
		for (int i = 0; i < array.Length; i++)
		{
			float value = UnityEngine.Random.Range(array[i], array[i] + array[1]);
			value = MathUtilities.LerpMapping(value, 0f, 1f, (0f - num) * 0.4f, num * 0.4f, clamp: true);
			FullscreenDirt(1, value, 0.15f, 0.3f);
		}
		CupheadLevelCamera.Current.Shake(55f, 0.5f, bypassEvent: true);
	}

	private void destroyPlatforms(LevelPlatform[] colliders, GameObject[] sprites, Rangef checkRange = default(Rangef))
	{
		if (colliders != null)
		{
			foreach (LevelPlatform levelPlatform in colliders)
			{
				LevelPlatform levelPlatform2 = null;
				int num = Array.IndexOf(swapPlatformsMappingBefore, levelPlatform);
				if (num >= 0)
				{
					levelPlatform2 = swapPlatformsMappingAfter[num];
				}
				if (!levelPlatform.GetComponentInChildren<LevelPlayerController>())
				{
					continue;
				}
				LevelPlayerController[] componentsInChildren = levelPlatform.GetComponentsInChildren<LevelPlayerController>();
				LevelPlayerController[] array = componentsInChildren;
				foreach (LevelPlayerController levelPlayerController in array)
				{
					if (!checkRange.ContainsExclusive(levelPlayerController.transform.position.x) && levelPlatform2 != null)
					{
						levelPlatform2.AddChild(levelPlayerController.transform);
					}
					else
					{
						levelPlayerController.motor.OnTrampolineKnockUp(BridgeDestroyBounceHeight);
					}
				}
			}
			foreach (LevelPlatform levelPlatform3 in colliders)
			{
				UnityEngine.Object.Destroy(levelPlatform3.gameObject);
			}
		}
		if (sprites != null)
		{
			foreach (GameObject gameObject in sprites)
			{
				gameObject.SetActive(value: false);
			}
		}
	}

	private IEnumerator winFakeout_cr()
	{
		anteater.FakeDeathStart();
		AudioManager.Play("level_announcer_knockout_bell");
		AudioManager.Play("sfx_dlc_rumrun_vx_fakeannouncer_knockout");
		StartCoroutine(stingSound_cr());
		Vector3 bannerPosition = fakeBannerAnimator.transform.position;
		bannerPosition += CupheadLevelCamera.Current.transform.position;
		fakeBannerAnimator.transform.position = bannerPosition;
		fakeBannerAnimator.SetTrigger("Banner");
		Coroutine dirtCoroutine = StartCoroutine(fakeDeathDirt_cr());
		yield return fakeBannerAnimator.WaitForAnimationToEnd(this, "Banner");
		StopCoroutine(dirtCoroutine);
		anteater.FakeDeathContinue();
	}

	private IEnumerator fakeDeathDirt_cr()
	{
		float elapsedTime = 0f;
		float delay = 0.4f;
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, delay);
			elapsedTime += delay;
			delay = Mathf.Lerp(0.4f, 0.8f, elapsedTime / 1.5f);
			FullscreenDirt(2);
		}
	}

	private IEnumerator stingSound_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 0.1f);
		AudioManager.Play("sfx_dlc_rumrun_fake_levelbossdefeatsting");
	}

	public void FullscreenDirt(int count, float? positionX = null, float customInitialDelay = -1f, float customIntraDelay = -1f)
	{
		StartCoroutine(dirtFX_cr(count, positionX, customInitialDelay, customIntraDelay));
	}

	private IEnumerator dirtFX_cr(int count, float? positionX, float customInitialDelay, float customIntraDelay)
	{
		MinMax[] DirtRandomizationX = new MinMax[2]
		{
			new MinMax(-50f, 0f),
			new MinMax(0f, 50f)
		};
		MinMax WaitRange = new MinMax(0.08f, 0.12f);
		MinMax previousSpawnRange = ((!Rand.Bool()) ? new MinMax(0.5f, 1f) : new MinMax(0f, 0.5f));
		for (int i = 0; i < count; i++)
		{
			float initialDelay = WaitRange.RandomFloat();
			if (customInitialDelay >= 0f)
			{
				initialDelay = customInitialDelay;
			}
			yield return CupheadTime.WaitForSeconds(this, initialDelay);
			Vector3 position = new Vector3(0f, 360f / camera.zoom);
			if (!positionX.HasValue)
			{
				MinMax minMax = ((previousSpawnRange.min != 0f) ? new MinMax(0f, 0.5f) : new MinMax(0.5f, 1f));
				position.x = 1280f * minMax.RandomFloat() - 640f;
				previousSpawnRange = minMax;
			}
			else
			{
				position.x = positionX.Value;
			}
			DirtRandomizationX.Shuffle();
			for (int spawn = 0; spawn < DirtRandomizationX.Length; spawn++)
			{
				fullscreenDirtFX.Create(position + new Vector3(DirtRandomizationX[spawn].RandomFloat(), 0f));
				float intraDelay = UnityEngine.Random.Range(0.1f, 0.2f);
				if (customIntraDelay >= 0f)
				{
					intraDelay = UnityEngine.Random.Range(customIntraDelay * 0.8f, customIntraDelay * 1.2f);
				}
				yield return CupheadTime.WaitForSeconds(this, intraDelay);
			}
		}
	}

	private void onShakeEventHandler(float amount, float time)
	{
		FullscreenDirt(2);
	}

	protected override void PlayAnnouncerReady()
	{
		AudioManager.Play("sfx_dlc_rumrun_vx_fakeannouncer_ready");
	}

	protected override void PlayAnnouncerBegin()
	{
		AudioManager.Play("sfx_dlc_rumrun_vx_fakeannouncer_begin");
	}

	protected override IEnumerator knockoutSFX_cr()
	{
		AudioManager.Play("level_announcer_knockout_bell");
		AudioManager.Play("sfx_DLC_RUMRUN_VX_AnnouncerClearThroat");
		yield return CupheadTime.WaitForSeconds(this, 1.4f);
		AudioManager.Play("level_boss_defeat_sting");
	}

	public static float GroundWalkingPosY(Vector2 position, Collider2D collider, float offset = 0f, float rayLength = 200f)
	{
		int layerMask = 1048576;
		position.x = Mathf.Clamp(position.x, Level.Current.Left, Level.Current.Right);
		Vector3 vector = new Vector3(position.x, position.y);
		RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, Vector3.down, rayLength, layerMask);
		if (raycastHit2D.collider != null)
		{
			float y = raycastHit2D.point.y;
			float num = ((!collider) ? 0f : (0f - collider.offset.y + collider.bounds.size.y / 2f));
			return y + num + offset;
		}
		return position.y;
	}
}
