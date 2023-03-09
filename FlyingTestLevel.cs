using System;
using System.Collections;
using UnityEngine;

public class FlyingTestLevel : Level
{
	[Serializable]
	public class Prefabs
	{
	}

	private LevelProperties.FlyingTest properties;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	[Multiline]
	private string _bossQuote;

	public override Levels CurrentLevel => Levels.FlyingTest;

	public override Scenes CurrentScene => Scenes.scene_level_flying_test;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.FlyingTest.GetMode(base.mode);
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
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(flyingtestPattern_cr());
	}

	private IEnumerator flyingtestPattern_cr()
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
		LevelProperties.FlyingTest.Pattern p = properties.CurrentState.NextPattern;
		yield return CupheadTime.WaitForSeconds(this, 1f);
	}
}
