using System;
using System.Collections;
using UnityEngine;

public class TestLevel : Level
{
	[Serializable]
	public class Prefabs
	{
	}

	private LevelProperties.Test properties;

	[SerializeField]
	private TestLevelFlyingJared jared;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	[Multiline]
	private string _bossQuote;

	public override Levels CurrentLevel => Levels.Test;

	public override Scenes CurrentScene => Scenes.scene_level_test;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.Test.GetMode(base.mode);
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
		jared.LevelInit(properties);
	}

	protected override void Update()
	{
		base.Update();
		if (Input.GetKeyDown(KeyCode.Space))
		{
			LevelPlayerController player = PlayerManager.GetPlayer<LevelPlayerController>(PlayerId.PlayerOne);
			player.animationController.SetColorOverTime(Color.blue, 1f);
		}
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(testPattern_cr());
	}

	protected override void OnStateChanged()
	{
		base.OnStateChanged();
	}

	private IEnumerator testPattern_cr()
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
		LevelProperties.Test.Pattern p = properties.CurrentState.NextPattern;
		yield return CupheadTime.WaitForSeconds(this, 1f);
	}
}
