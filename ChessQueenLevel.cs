using System.Collections;
using UnityEngine;

public class ChessQueenLevel : ChessLevel
{
	private LevelProperties.ChessQueen properties;

	[SerializeField]
	private ChessQueenLevelQueen queen;

	[SerializeField]
	private Animator[] mouseAnimator;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortraitMain;

	[SerializeField]
	private string _bossQuoteMain;

	public bool cannonBlastFXVariant;

	public override Levels CurrentLevel => Levels.ChessQueen;

	public override Scenes CurrentScene => Scenes.scene_level_chess_queen;

	public override Sprite BossPortrait => _bossPortraitMain;

	public override string BossQuote => _bossQuoteMain;

	protected override void PartialInit()
	{
		properties = LevelProperties.ChessQueen.GetMode(base.mode);
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
		queen = null;
		mouseAnimator = null;
	}

	protected override void Start()
	{
		Level.IsChessBoss = true;
		base.Start();
		queen.LevelInit(properties);
	}

	public override void OnLevelEnd()
	{
		base.OnLevelEnd();
		float num = Random.Range(0f, 1f);
		mouseAnimator[0].Play("Win", 0, num);
		mouseAnimator[1].Play("Win", 0, num + 0.33f);
		mouseAnimator[2].Play("Win", 0, num + 0.66f);
		mouseAnimator[0].Play("Idle", 1, 0f);
		mouseAnimator[1].Play("Idle", 1, 0.33f);
		mouseAnimator[2].Play("Idle", 1, 0.66f);
	}

	protected override void OnStateChanged()
	{
		base.OnStateChanged();
		queen.StateChanged();
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(chessQueenPattern_cr());
	}

	private IEnumerator chessQueenPattern_cr()
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
		case LevelProperties.ChessQueen.Pattern.Lightning:
			yield return StartCoroutine(lightning_cr());
			break;
		case LevelProperties.ChessQueen.Pattern.Egg:
			yield return StartCoroutine(egg_cr());
			break;
		default:
			yield return CupheadTime.WaitForSeconds(this, 1f);
			break;
		}
	}

	public bool NextPatternIsEgg()
	{
		if (properties.CurrentState.PeekNextPattern == LevelProperties.ChessQueen.Pattern.Egg)
		{
			LevelProperties.ChessQueen.Pattern nextPattern = properties.CurrentState.NextPattern;
			return true;
		}
		return false;
	}

	private IEnumerator lightning_cr()
	{
		while (queen.state != 0)
		{
			yield return null;
		}
		queen.StartLightning();
		while (queen.state != 0)
		{
			yield return null;
		}
	}

	private IEnumerator egg_cr()
	{
		while (queen.state != 0)
		{
			yield return null;
		}
		queen.StartEgg();
		while (queen.state != 0)
		{
			yield return null;
		}
	}
}
