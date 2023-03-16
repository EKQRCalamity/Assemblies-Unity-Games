using System.Collections;
using UnityEngine;

public class ChessBishopLevel : ChessLevel
{
	private LevelProperties.ChessBishop properties;

	[SerializeField]
	private ChessBishopLevelBishop bishop;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	[Multiline]
	private string _bossQuote;

	public override Levels CurrentLevel => Levels.ChessBishop;

	public override Scenes CurrentScene => Scenes.scene_level_chess_bishop;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.ChessBishop.GetMode(base.mode);
		properties.OnStateChange += base.zHack_OnStateChanged;
		properties.OnBossDeath += base.zHack_OnWin;
		base.timeline = properties.CreateTimeline(base.mode);
		goalTimes = properties.goalTimes;
		properties.OnBossDamaged += base.timeline.DealDamage;
		base.PartialInit();
	}

	protected override void Start()
	{
		Level.IsChessBoss = true;
		base.Start();
		bishop.LevelInit(properties);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		bishop = null;
		_bossPortrait = null;
	}

	protected override void OnStateChanged()
	{
		base.OnStateChanged();
		bishop.StartNewPhase();
	}

	private IEnumerator bishopPattern_cr()
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
		LevelProperties.ChessBishop.Pattern p = properties.CurrentState.NextPattern;
		yield return CupheadTime.WaitForSeconds(this, 1f);
	}
}
