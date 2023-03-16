using System.Collections;
using UnityEngine;

public class ChessBOldBLevel : Level
{
	private LevelProperties.ChessBOldB properties;

	[SerializeField]
	private ChessBOldBLevelBoss boss;

	[SerializeField]
	private ChessBOldBLevelGameManager gameManager;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	[Multiline]
	private string _bossQuote;

	public override Levels CurrentLevel => Levels.ChessBOldB;

	public override Scenes CurrentScene => Scenes.scene_level_chess_boldb;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.ChessBOldB.GetMode(base.mode);
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
		boss.LevelInit(properties);
		gameManager.SetupGameManager(properties);
	}

	protected override void OnStateChanged()
	{
		base.OnStateChanged();
		gameManager.OnStateChanged();
		boss.OnStateChanged();
	}

	protected override void OnLevelStart()
	{
	}

	private IEnumerator ChessBOldBPattern_cr()
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
		LevelProperties.ChessBOldB.Pattern p = properties.CurrentState.NextPattern;
		yield return CupheadTime.WaitForSeconds(this, 1f);
	}
}
