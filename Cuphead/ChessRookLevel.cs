using System.Collections;
using UnityEngine;

public class ChessRookLevel : ChessLevel
{
	private LevelProperties.ChessRook properties;

	[SerializeField]
	private ChessRookLevelRook rook;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortraitMain;

	[SerializeField]
	private string _bossQuoteMain;

	public override Levels CurrentLevel => Levels.ChessRook;

	public override Scenes CurrentScene => Scenes.scene_level_chess_rook;

	public override Sprite BossPortrait => _bossPortraitMain;

	public override string BossQuote => _bossQuoteMain;

	protected override void PartialInit()
	{
		properties = LevelProperties.ChessRook.GetMode(base.mode);
		properties.OnStateChange += base.zHack_OnStateChanged;
		properties.OnBossDeath += base.zHack_OnWin;
		base.timeline = properties.CreateTimeline(base.mode);
		goalTimes = properties.goalTimes;
		properties.OnBossDamaged += base.timeline.DealDamage;
		base.PartialInit();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_bossPortraitMain = null;
		rook = null;
	}

	protected override void Start()
	{
		Level.IsChessBoss = true;
		base.Start();
		rook.LevelInit(properties);
	}

	protected override void OnLevelStart()
	{
	}

	protected override void OnStateChanged()
	{
		base.OnStateChanged();
		rook.OnPhaseChange();
	}

	private IEnumerator chessrookPattern_cr()
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
		LevelProperties.ChessRook.Pattern p = properties.CurrentState.NextPattern;
		yield return CupheadTime.WaitForSeconds(this, 1f);
	}
}
