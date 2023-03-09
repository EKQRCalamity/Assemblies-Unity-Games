using System.Collections;
using UnityEngine;

public class DicePalaceDominoLevel : AbstractDicePalaceLevel
{
	private LevelProperties.DicePalaceDomino properties;

	[SerializeField]
	private DicePalaceDominoLevelDomino domino;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	private string _bossQuote;

	public override DicePalaceLevels CurrentDicePalaceLevel => DicePalaceLevels.DicePalaceDomino;

	public override Levels CurrentLevel => Levels.DicePalaceDomino;

	public override Scenes CurrentScene => Scenes.scene_level_dice_palace_domino;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.DicePalaceDomino.GetMode(base.mode);
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
		domino.LevelInit(properties);
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(dicepalacedominoPattern_cr());
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_bossPortrait = null;
	}

	private IEnumerator dicepalacedominoPattern_cr()
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
		switch (properties.CurrentState.NextPattern)
		{
		case LevelProperties.DicePalaceDomino.Pattern.Boomerang:
			yield return StartCoroutine(boomerang_cr());
			break;
		case LevelProperties.DicePalaceDomino.Pattern.BouncyBall:
			yield return StartCoroutine(bouncyball_cr());
			break;
		default:
			yield return CupheadTime.WaitForSeconds(this, 1f);
			break;
		}
	}

	private IEnumerator boomerang_cr()
	{
		while (domino.state != 0)
		{
			yield return null;
		}
		domino.OnBoomerang();
		while (domino.state != 0)
		{
			yield return null;
		}
	}

	private IEnumerator bouncyball_cr()
	{
		while (domino.state != 0)
		{
			yield return null;
		}
		domino.OnBouncyBall();
		while (domino.state != 0)
		{
			yield return null;
		}
	}
}
