using System.Collections;
using UnityEngine;

public class FlyingCowboyLevel : Level
{
	private LevelProperties.FlyingCowboy properties;

	[SerializeField]
	private FlyingCowboyLevelCowboy cowboy;

	[SerializeField]
	private FlyingCowboyLevelMeat meat;

	[SerializeField]
	private FlyingCowboyLevelBackground background;

	[SerializeField]
	private PlanePlayerDust playerDust;

	[SerializeField]
	private float playerDustSmallTrigger;

	[SerializeField]
	private float playerDustLargeTrigger;

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

	private PlanePlayerDust[] playerDusts = new PlanePlayerDust[2];

	public override Levels CurrentLevel => Levels.FlyingCowboy;

	public override Scenes CurrentScene => Scenes.scene_level_flying_cowboy;

	public override Sprite BossPortrait
	{
		get
		{
			switch (properties.CurrentState.stateName)
			{
			case LevelProperties.FlyingCowboy.States.Main:
				return _bossPortraitMain;
			case LevelProperties.FlyingCowboy.States.Vacuum:
				return _bossPortraitPhaseTwo;
			case LevelProperties.FlyingCowboy.States.Meatball:
				return _bossPortraitPhaseThree;
			case LevelProperties.FlyingCowboy.States.Sausage:
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
			case LevelProperties.FlyingCowboy.States.Main:
				return _bossQuoteMain;
			case LevelProperties.FlyingCowboy.States.Vacuum:
				return _bossQuotePhaseTwo;
			case LevelProperties.FlyingCowboy.States.Meatball:
				return _bossQuotePhaseThree;
			case LevelProperties.FlyingCowboy.States.Sausage:
				return _bossQuotePhaseFour;
			default:
				Debug.LogError(string.Concat("Couldn't find quote for state ", properties.CurrentState.stateName, ". Using Main."));
				return _bossQuoteMain;
			}
		}
	}

	protected override void PartialInit()
	{
		properties = LevelProperties.FlyingCowboy.GetMode(base.mode);
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
	}

	protected override void Start()
	{
		base.Start();
		cowboy.LevelInit(properties);
		meat.LevelInit(properties);
		playerDusts[0] = Object.Instantiate(playerDust);
		playerDusts[1] = Object.Instantiate(playerDust);
		playerDusts[0].Initialize(players[0], playerDustSmallTrigger, playerDustLargeTrigger);
		playerDusts[1].Initialize(players[1], playerDustSmallTrigger, playerDustLargeTrigger);
	}

	protected override void CreatePlayerTwoOnJoin()
	{
		base.CreatePlayerTwoOnJoin();
		playerDusts[1].Initialize(players[1], playerDustSmallTrigger, playerDustLargeTrigger);
	}

	protected override void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawRay(new Vector3(-1000f, playerDustSmallTrigger), Vector3.right * 2000f);
		Gizmos.color = Color.blue;
		Gizmos.DrawRay(new Vector3(-1000f, playerDustLargeTrigger), Vector3.right * 2000f);
	}

	protected override void OnStateChanged()
	{
		base.OnStateChanged();
		if (properties.CurrentState.stateName == LevelProperties.FlyingCowboy.States.Vacuum)
		{
			StartCoroutine(toPhase2_cr());
		}
		else if (properties.CurrentState.stateName == LevelProperties.FlyingCowboy.States.Meatball)
		{
			StopAllCoroutines();
			StartCoroutine(toPhase3_cr());
		}
		else if (properties.CurrentState.stateName == LevelProperties.FlyingCowboy.States.Sausage)
		{
			StopAllCoroutines();
			StartCoroutine(toPhase4_cr());
		}
	}

	private IEnumerator toPhase2_cr()
	{
		LevelProperties.FlyingCowboy.Pattern pattern = properties.CurrentState.PeekNextPattern;
		cowboy.OnPhase2(pattern);
		yield return cowboy.animator.WaitForAnimationToStart(this, "Ph1_To_Ph2");
		while (cowboy.state == FlyingCowboyLevelCowboy.State.PhaseTrans)
		{
			yield return null;
		}
		StartCoroutine(phase2Loop_cr());
	}

	private IEnumerator phase2Loop_cr()
	{
		bool initial = true;
		while (true)
		{
			LevelProperties.FlyingCowboy.Pattern p = properties.CurrentState.NextPattern;
			if (p == LevelProperties.FlyingCowboy.Pattern.Vacuum)
			{
				yield return StartCoroutine(vacuum_cr(initial));
			}
			else
			{
				yield return StartCoroutine(ricochet_cr(initial));
			}
			initial = false;
		}
	}

	private IEnumerator vacuum_cr(bool initial)
	{
		while (cowboy.state != 0)
		{
			yield return null;
		}
		cowboy.Vacuum(initial);
		while (cowboy.state != 0)
		{
			yield return null;
		}
	}

	private IEnumerator ricochet_cr(bool initial)
	{
		if (initial && properties.CurrentState.ricochet.useRicochet)
		{
			cowboy.animator.SetBool("OnRicochet", value: true);
		}
		while (cowboy.state != 0)
		{
			yield return null;
		}
		if (properties.CurrentState.ricochet.useRicochet)
		{
			cowboy.Ricochet();
		}
		while (cowboy.state != 0)
		{
			yield return null;
		}
	}

	private IEnumerator toPhase3_cr()
	{
		background.BeginTransition();
		if (cowboy != null)
		{
			cowboy.Death();
		}
		while (!cowboy.IsDead)
		{
			yield return null;
		}
		meat.SelectPhase(FlyingCowboyLevelMeat.MeatPhase.Sausage);
	}

	private IEnumerator toPhase4_cr()
	{
		while (cowboy.state != 0)
		{
			yield return null;
		}
		meat.SelectPhase(FlyingCowboyLevelMeat.MeatPhase.Can);
	}
}
