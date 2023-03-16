using System;
using System.Collections;
using UnityEngine;

public class AirshipJellyLevel : Level
{
	[Serializable]
	public class Prefabs
	{
	}

	private LevelProperties.AirshipJelly properties;

	[SerializeField]
	private AirshipJellyLevelJelly jelly;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	[Multiline]
	private string _bossQuote;

	public override Levels CurrentLevel => Levels.AirshipJelly;

	public override Scenes CurrentScene => Scenes.scene_level_airship_jelly;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.AirshipJelly.GetMode(base.mode);
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
		jelly.LevelInit(properties);
	}

	protected override void OnLevelStart()
	{
	}

	private IEnumerator airshipPattern_cr()
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
		LevelProperties.AirshipJelly.Pattern p = properties.CurrentState.NextPattern;
		yield return CupheadTime.WaitForSeconds(this, 1f);
	}
}
