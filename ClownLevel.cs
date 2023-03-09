using System.Collections;
using UnityEngine;

public class ClownLevel : Level
{
	private LevelProperties.Clown properties;

	[SerializeField]
	private ClownLevelClown clown;

	[SerializeField]
	private ClownLevelClownHelium clownHelium;

	[SerializeField]
	private ClownLevelClownHorse clownHorse;

	[SerializeField]
	private ClownLevelClownSwing clownSwing;

	[SerializeField]
	private ClownLevelCoasterHandler coasterHandler;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortraitMain;

	[SerializeField]
	private Sprite _bossPortraitHeliumTank;

	[SerializeField]
	private Sprite _bossPortraitCarouselHorse;

	[SerializeField]
	private Sprite _bossPortraitSwing;

	[SerializeField]
	private string _bossQuoteMain;

	[SerializeField]
	private string _bossQuoteHeliumTank;

	[SerializeField]
	private string _bossQuoteCarouselHorse;

	[SerializeField]
	private string _bossQuoteSwing;

	public override Levels CurrentLevel => Levels.Clown;

	public override Scenes CurrentScene => Scenes.scene_level_clown;

	public override Sprite BossPortrait
	{
		get
		{
			switch (properties.CurrentState.stateName)
			{
			case LevelProperties.Clown.States.Main:
			case LevelProperties.Clown.States.Generic:
				return _bossPortraitMain;
			case LevelProperties.Clown.States.HeliumTank:
				return _bossPortraitHeliumTank;
			case LevelProperties.Clown.States.CarouselHorse:
				return _bossPortraitCarouselHorse;
			case LevelProperties.Clown.States.Swing:
				return _bossPortraitSwing;
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
			case LevelProperties.Clown.States.Main:
			case LevelProperties.Clown.States.Generic:
				return _bossQuoteMain;
			case LevelProperties.Clown.States.HeliumTank:
				return _bossQuoteHeliumTank;
			case LevelProperties.Clown.States.CarouselHorse:
				return _bossQuoteCarouselHorse;
			case LevelProperties.Clown.States.Swing:
				return _bossQuoteSwing;
			default:
				Debug.LogError(string.Concat("Couldn't find quote for state ", properties.CurrentState.stateName, ". Using Main."));
				return _bossQuoteMain;
			}
		}
	}

	protected override void PartialInit()
	{
		properties = LevelProperties.Clown.GetMode(base.mode);
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
		coasterHandler.LevelInit(properties);
		clown.LevelInit(properties);
		clownHelium.LevelInit(properties);
		clownHorse.LevelInit(properties);
		clownSwing.LevelInit(properties);
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(clownPattern_cr());
	}

	protected override void OnStateChanged()
	{
		base.OnStateChanged();
		if (properties.CurrentState.stateName == LevelProperties.Clown.States.HeliumTank)
		{
			StopAllCoroutines();
			StartCoroutine(helium_tank_cr());
		}
		else if (properties.CurrentState.stateName == LevelProperties.Clown.States.CarouselHorse)
		{
			StopAllCoroutines();
			StartCoroutine(carousel_horse_cr());
		}
		else if (properties.CurrentState.stateName == LevelProperties.Clown.States.Swing)
		{
			StopAllCoroutines();
			StartCoroutine(swing_cr());
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_bossPortraitCarouselHorse = null;
		_bossPortraitHeliumTank = null;
		_bossPortraitMain = null;
		_bossPortraitSwing = null;
	}

	private IEnumerator clownPattern_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		while (true)
		{
			yield return StartCoroutine(nextPattern_cr());
			yield return null;
		}
	}

	private IEnumerator nextPattern_cr()
	{
		if (properties.CurrentState.NextPattern == LevelProperties.Clown.Pattern.Default)
		{
			yield return null;
		}
		else
		{
			yield return CupheadTime.WaitForSeconds(this, 1f);
		}
	}

	private IEnumerator bumper_car_cr()
	{
		clown.StartBumperCar();
		yield return null;
	}

	private IEnumerator helium_tank_cr()
	{
		clown.EndBumperCar();
		if (coasterHandler.isRunning)
		{
			coasterHandler.finalRun = true;
		}
		if (properties.CurrentState.heliumClown.coasterOn)
		{
			while (coasterHandler.finalRun)
			{
				yield return null;
			}
			coasterHandler.StartCoaster();
		}
		yield return null;
	}

	private IEnumerator carousel_horse_cr()
	{
		clownHelium.StartDeath();
		if (coasterHandler.isRunning)
		{
			coasterHandler.finalRun = true;
		}
		if (properties.CurrentState.horse.coasterOn)
		{
			while (coasterHandler.finalRun)
			{
				yield return null;
			}
			coasterHandler.StartCoaster();
		}
		yield return null;
	}

	private IEnumerator swing_cr()
	{
		clownHorse.StartDeath();
		if (coasterHandler.isRunning)
		{
			coasterHandler.finalRun = true;
		}
		while (coasterHandler.finalRun)
		{
			yield return null;
		}
		coasterHandler.StartCoaster();
		yield return null;
	}
}
