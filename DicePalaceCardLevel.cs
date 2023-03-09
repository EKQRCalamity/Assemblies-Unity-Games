using System.Collections;
using UnityEngine;

public class DicePalaceCardLevel : AbstractDicePalaceLevel
{
	private LevelProperties.DicePalaceCard properties;

	[SerializeField]
	private DicePalaceCardGameManager gameManager;

	[SerializeField]
	private DicePalaceCardLevelCard card;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	private string _bossQuote;

	public override DicePalaceLevels CurrentDicePalaceLevel => DicePalaceLevels.DicePalaceCard;

	public override Levels CurrentLevel => Levels.DicePalaceCard;

	public override Scenes CurrentScene => Scenes.scene_level_dice_palace_card;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.DicePalaceCard.GetMode(base.mode);
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
		card.LevelInit(properties);
		gameManager.GameSetup(properties);
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(dicepalacecardPattern_cr());
		StartCoroutine(gameManager.start_game_cr());
	}

	private IEnumerator dicepalacecardPattern_cr()
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
		LevelProperties.DicePalaceCard.Pattern p = properties.CurrentState.NextPattern;
		yield return CupheadTime.WaitForSeconds(this, 1f);
	}
}
