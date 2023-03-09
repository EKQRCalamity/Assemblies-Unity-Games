using System.Collections;
using UnityEngine;

public class ChessKnightLevel : ChessLevel
{
	private LevelProperties.ChessKnight properties;

	[SerializeField]
	private ChessKnightLevelKnight knight;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortraitMain;

	[SerializeField]
	private string _bossQuoteMain;

	public override Levels CurrentLevel => Levels.ChessKnight;

	public override Scenes CurrentScene => Scenes.scene_level_chess_knight;

	public override Sprite BossPortrait => _bossPortraitMain;

	public override string BossQuote => _bossQuoteMain;

	protected override void PartialInit()
	{
		properties = LevelProperties.ChessKnight.GetMode(base.mode);
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
		knight = null;
	}

	protected override void Start()
	{
		Level.IsChessBoss = true;
		base.Start();
		knight.LevelInit(properties);
		foreach (LevelPlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (allPlayer != null)
			{
				allPlayer.gameObject.layer = 31;
				Transform[] componentsInChildren = allPlayer.gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform in componentsInChildren)
				{
					transform.gameObject.layer = 31;
				}
			}
		}
	}

	protected override void OnPlayerJoined(PlayerId playerId)
	{
		base.OnPlayerJoined(playerId);
		AbstractPlayerController player = PlayerManager.GetPlayer(playerId);
		if ((bool)player)
		{
			SpriteRenderer[] componentsInChildren = player.GetComponentsInChildren<SpriteRenderer>();
			foreach (SpriteRenderer spriteRenderer in componentsInChildren)
			{
				spriteRenderer.gameObject.layer = 31;
			}
		}
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(chessknightPattern_cr());
	}

	private IEnumerator chessknightPattern_cr()
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
		case LevelProperties.ChessKnight.Pattern.Short:
			yield return StartCoroutine(short_cr());
			break;
		case LevelProperties.ChessKnight.Pattern.Long:
			yield return StartCoroutine(long_cr());
			break;
		case LevelProperties.ChessKnight.Pattern.Up:
			yield return StartCoroutine(up_cr());
			break;
		default:
			yield return CupheadTime.WaitForSeconds(this, 1f);
			break;
		}
	}

	private IEnumerator short_cr()
	{
		while (knight.state != ChessKnightLevelKnight.State.Idle)
		{
			yield return null;
		}
		knight.Short();
		while (knight.state != ChessKnightLevelKnight.State.Idle)
		{
			yield return null;
		}
	}

	private IEnumerator long_cr()
	{
		while (knight.state != ChessKnightLevelKnight.State.Idle)
		{
			yield return null;
		}
		knight.Long();
		while (knight.state != ChessKnightLevelKnight.State.Idle)
		{
			yield return null;
		}
	}

	private IEnumerator up_cr()
	{
		while (knight.state != ChessKnightLevelKnight.State.Idle)
		{
			yield return null;
		}
		knight.Up();
		while (knight.state != ChessKnightLevelKnight.State.Idle)
		{
			yield return null;
		}
	}
}
