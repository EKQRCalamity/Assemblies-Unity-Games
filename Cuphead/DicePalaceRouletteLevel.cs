using System.Collections;
using UnityEngine;

public class DicePalaceRouletteLevel : AbstractDicePalaceLevel
{
	private LevelProperties.DicePalaceRoulette properties;

	[SerializeField]
	private DicePalaceRouletteLevelRoulette roulette;

	[SerializeField]
	private DicePalaceRouletteLevelPlatform[] platforms;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	private string _bossQuote;

	public override DicePalaceLevels CurrentDicePalaceLevel => DicePalaceLevels.DicePalaceRoulette;

	public override Levels CurrentLevel => Levels.DicePalaceRoulette;

	public override Scenes CurrentScene => Scenes.scene_level_dice_palace_roulette;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.DicePalaceRoulette.GetMode(base.mode);
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
		roulette.LevelInit(properties);
		DicePalaceRouletteLevelPlatform[] array = platforms;
		foreach (DicePalaceRouletteLevelPlatform dicePalaceRouletteLevelPlatform in array)
		{
			dicePalaceRouletteLevelPlatform.Init(properties.CurrentState.platform);
		}
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(dicepalaceroulettePattern_cr());
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_bossPortrait = null;
	}

	private IEnumerator dicepalaceroulettePattern_cr()
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
		case LevelProperties.DicePalaceRoulette.Pattern.Twirl:
			yield return StartCoroutine(twirl_cr());
			break;
		case LevelProperties.DicePalaceRoulette.Pattern.Marble:
			yield return StartCoroutine(marble_cr());
			break;
		default:
			yield return CupheadTime.WaitForSeconds(this, 1f);
			break;
		}
	}

	private IEnumerator twirl_cr()
	{
		while (roulette.state != DicePalaceRouletteLevelRoulette.State.Idle)
		{
			yield return null;
		}
		roulette.StartTwirl();
		while (roulette.state != DicePalaceRouletteLevelRoulette.State.Idle)
		{
			yield return null;
		}
	}

	private IEnumerator marble_cr()
	{
		while (roulette.state != DicePalaceRouletteLevelRoulette.State.Idle)
		{
			yield return null;
		}
		roulette.StartMarbleDrop();
		while (roulette.state != DicePalaceRouletteLevelRoulette.State.Idle)
		{
			yield return null;
		}
	}
}
