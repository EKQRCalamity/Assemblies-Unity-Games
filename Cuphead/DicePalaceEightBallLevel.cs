using System.Collections;
using UnityEngine;

public class DicePalaceEightBallLevel : AbstractDicePalaceLevel
{
	private LevelProperties.DicePalaceEightBall properties;

	[SerializeField]
	private DicePalaceEightBallLevelEightBall eightBall;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	private string _bossQuote;

	public override DicePalaceLevels CurrentDicePalaceLevel => DicePalaceLevels.DicePalaceEightBall;

	public override Levels CurrentLevel => Levels.DicePalaceEightBall;

	public override Scenes CurrentScene => Scenes.scene_level_dice_palace_eight_ball;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.DicePalaceEightBall.GetMode(base.mode);
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
		eightBall.LevelInit(properties);
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(dicepalaceeightballPattern_cr());
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_bossPortrait = null;
	}

	private IEnumerator dicepalaceeightballPattern_cr()
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
		if (properties.CurrentState.NextPattern == LevelProperties.DicePalaceEightBall.Pattern.Default)
		{
			yield return null;
		}
		else
		{
			yield return CupheadTime.WaitForSeconds(this, 1f);
		}
	}
}
