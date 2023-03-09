using System.Collections;
using UnityEngine;

public class ChessKingLevel : Level
{
	private LevelProperties.ChessKing properties;

	[SerializeField]
	private ChessKingLevelKing king;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	[Multiline]
	private string _bossQuote;

	public override Levels CurrentLevel => Levels.ChessKing;

	public override Scenes CurrentScene => Scenes.scene_level_chess_king;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.ChessKing.GetMode(base.mode);
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
		king.LevelInit(properties);
	}

	protected override void OnLevelStart()
	{
		king.StartGame();
	}

	protected override void OnStateChanged()
	{
		base.OnStateChanged();
		king.StateChange();
	}

	private IEnumerator chesskingPattern_cr()
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
		LevelProperties.ChessKing.Pattern p = properties.CurrentState.NextPattern;
		yield return CupheadTime.WaitForSeconds(this, 1f);
	}
}
