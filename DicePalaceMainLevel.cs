using System.Collections;
using UnityEngine;

public class DicePalaceMainLevel : AbstractDicePalaceLevel
{
	private LevelProperties.DicePalaceMain properties;

	[SerializeField]
	private DicePalaceMainLevelGameManager gameManager;

	[SerializeField]
	private DicePalaceMainLevelKingDice kingDice;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	private string _bossQuote;

	public override DicePalaceLevels CurrentDicePalaceLevel => DicePalaceLevels.DicePalaceMain;

	public override Levels CurrentLevel => Levels.DicePalaceMain;

	public override Scenes CurrentScene => Scenes.scene_level_dice_palace_main;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	public DicePalaceMainLevelGameManager GameManager => gameManager;

	protected override void PartialInit()
	{
		properties = LevelProperties.DicePalaceMain.GetMode(base.mode);
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
		gameManager.LevelInit(properties);
		kingDice.LevelInit(properties);
		if (PlayerManager.GetPlayer(PlayerId.PlayerOne).stats.isChalice)
		{
			DicePalaceMainLevelGameInfo.CHALICE_PLAYER = 0;
		}
		else if (PlayerManager.GetPlayer(PlayerId.PlayerTwo) != null && PlayerManager.GetPlayer(PlayerId.PlayerTwo).stats.isChalice)
		{
			DicePalaceMainLevelGameInfo.CHALICE_PLAYER = 1;
		}
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(dicepalacemainPattern_cr());
	}

	protected override void CheckIfInABossesHub()
	{
		base.CheckIfInABossesHub();
		if (!isTowerOfPower)
		{
			Level.IsDicePalaceMain = true;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_bossPortrait = null;
	}

	private IEnumerator dicepalacemainPattern_cr()
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
		LevelProperties.DicePalaceMain.Pattern p = properties.CurrentState.NextPattern;
		yield return CupheadTime.WaitForSeconds(this, 1f);
	}
}
