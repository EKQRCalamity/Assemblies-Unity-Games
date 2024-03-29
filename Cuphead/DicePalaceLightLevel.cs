using System.Collections;
using UnityEngine;

public class DicePalaceLightLevel : AbstractDicePalaceLevel
{
	private LevelProperties.DicePalaceLight properties;

	[SerializeField]
	private RumRunnersLevelWorm lightBoss;

	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	private string _bossQuote;

	public override DicePalaceLevels CurrentDicePalaceLevel => DicePalaceLevels.DicePalaceLight;

	public override Levels CurrentLevel => Levels.DicePalaceLight;

	public override Scenes CurrentScene => Scenes.scene_level_dice_palace_light;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.DicePalaceLight.GetMode(base.mode);
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
		StartCoroutine(dicepalacelightPattern_cr());
	}

	private IEnumerator dicepalacelightPattern_cr()
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
		LevelProperties.DicePalaceLight.Pattern p = properties.CurrentState.NextPattern;
		yield return CupheadTime.WaitForSeconds(this, 1f);
	}
}
