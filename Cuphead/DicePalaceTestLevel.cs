using System.Collections;
using UnityEngine;

public class DicePalaceTestLevel : AbstractDicePalaceLevel
{
	private LevelProperties.DicePalaceTest properties;

	[SerializeField]
	private DicePalaceTestLevelTest test;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	private string _bossQuote;

	public override DicePalaceLevels CurrentDicePalaceLevel => DicePalaceLevels.DicePalaceTest;

	public override Levels CurrentLevel => Levels.DicePalaceTest;

	public override Scenes CurrentScene => Scenes.scene_level_dice_palace_test;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.DicePalaceTest.GetMode(base.mode);
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
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(dicepalacetestPattern_cr());
		StartCoroutine(test.start_it_cr());
	}

	private IEnumerator dicepalacetestPattern_cr()
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
		LevelProperties.DicePalaceTest.Pattern p = properties.CurrentState.NextPattern;
		yield return CupheadTime.WaitForSeconds(this, 1f);
	}
}
