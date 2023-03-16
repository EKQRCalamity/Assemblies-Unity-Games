using System.Collections;
using UnityEngine;

public class DicePalaceRabbitLevel : AbstractDicePalaceLevel
{
	private LevelProperties.DicePalaceRabbit properties;

	[SerializeField]
	private DicePalaceRabbitLevelRabbit rabbit;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	private string _bossQuote;

	public override DicePalaceLevels CurrentDicePalaceLevel => DicePalaceLevels.DicePalaceRabbit;

	public override Levels CurrentLevel => Levels.DicePalaceRabbit;

	public override Scenes CurrentScene => Scenes.scene_level_dice_palace_rabbit;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.DicePalaceRabbit.GetMode(base.mode);
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
		rabbit.LevelInit(properties);
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(dicepalacerabbitPattern_cr());
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_bossPortrait = null;
	}

	private IEnumerator dicepalacerabbitPattern_cr()
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
		case LevelProperties.DicePalaceRabbit.Pattern.MagicWand:
			yield return StartCoroutine(magicwand_cr());
			break;
		case LevelProperties.DicePalaceRabbit.Pattern.MagicParry:
			yield return StartCoroutine(magicparry_cr());
			break;
		default:
			yield return CupheadTime.WaitForSeconds(this, 1f);
			break;
		}
	}

	private IEnumerator magicwand_cr()
	{
		while (rabbit.state != 0)
		{
			yield return null;
		}
		rabbit.OnMagicWand();
		while (rabbit.state != 0)
		{
			yield return null;
		}
	}

	private IEnumerator magicparry_cr()
	{
		while (rabbit.state != 0)
		{
			yield return null;
		}
		rabbit.OnMagicParry();
		while (rabbit.state != 0)
		{
			yield return null;
		}
	}
}
