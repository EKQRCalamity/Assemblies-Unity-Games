using System.Collections;
using UnityEngine;

public class ChessBOldALevel : Level
{
	private LevelProperties.ChessBOldA properties;

	[SerializeField]
	private ChessBOldALevelBishop bishop;

	[SerializeField]
	private Transform[] topPlatforms;

	[SerializeField]
	private Transform[] bottomPlatforms;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortraitMain;

	[SerializeField]
	private string _bossQuoteMain;

	public override Levels CurrentLevel => Levels.ChessBOldA;

	public override Scenes CurrentScene => Scenes.scene_level_chess_bolda;

	public override Sprite BossPortrait => _bossPortraitMain;

	public override string BossQuote => _bossQuoteMain;

	protected override void PartialInit()
	{
		properties = LevelProperties.ChessBOldA.GetMode(base.mode);
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
	}

	protected override void Start()
	{
		Level.IsChessBoss = true;
		base.Start();
		bishop.LevelInit(properties);
		Transform[] array = topPlatforms;
		foreach (Transform transform in array)
		{
			transform.transform.SetPosition(null, -360f + properties.CurrentState.stage.platformHeight * 2f);
		}
		Transform[] array2 = bottomPlatforms;
		foreach (Transform transform2 in array2)
		{
			transform2.transform.SetPosition(null, -360f + properties.CurrentState.stage.platformHeight);
		}
	}

	protected override void OnLevelStart()
	{
	}

	private IEnumerator chessbishopPattern_cr()
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
		LevelProperties.ChessBOldA.Pattern p = properties.CurrentState.NextPattern;
		yield return CupheadTime.WaitForSeconds(this, 1f);
	}
}
