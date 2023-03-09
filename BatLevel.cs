using System;
using System.Collections;
using UnityEngine;

public class BatLevel : Level
{
	[Serializable]
	public class Prefabs
	{
	}

	private LevelProperties.Bat properties;

	[Space(10f)]
	[SerializeField]
	private BatLevelBat bat;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	[Multiline]
	private string _bossQuote;

	public override Levels CurrentLevel => Levels.Bat;

	public override Scenes CurrentScene => Scenes.scene_level_bat;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.Bat.GetMode(base.mode);
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
		bat.LevelInit(properties);
	}

	protected override void OnLevelStart()
	{
		base.OnLevelStart();
		StartCoroutine(batPattern_cr());
		StartCoroutine(goblins_cr());
	}

	protected override void OnStateChanged()
	{
		base.OnStateChanged();
		if (properties.CurrentState.stateName == LevelProperties.Bat.States.Coffin)
		{
			StopAllCoroutines();
			StartCoroutine(phase_2_cr());
		}
		else if (properties.CurrentState.stateName == LevelProperties.Bat.States.Wolf)
		{
			StopAllCoroutines();
			StartCoroutine(phase_3_cr());
		}
	}

	private IEnumerator batPattern_cr()
	{
		yield return new WaitForSeconds(1f);
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
		default:
			yield return new WaitForSeconds(1f);
			break;
		case LevelProperties.Bat.Pattern.Bouncer:
			yield return StartCoroutine(bouncer_cr());
			break;
		case LevelProperties.Bat.Pattern.Lightning:
			yield return StartCoroutine(lightning_cr());
			break;
		}
	}

	private IEnumerator bouncer_cr()
	{
		while (bat.state != 0)
		{
			yield return null;
		}
		bat.StartBouncer();
		while (bat.state != 0)
		{
			yield return null;
		}
	}

	private IEnumerator lightning_cr()
	{
		while (bat.state != 0)
		{
			yield return null;
		}
		bat.StartLightning();
		while (bat.state != 0)
		{
			yield return null;
		}
	}

	private IEnumerator phase_2_cr()
	{
		bat.StartPhase2();
		yield return null;
	}

	private IEnumerator phase_3_cr()
	{
		bat.StartPhase3();
		yield return null;
	}

	private IEnumerator goblins_cr()
	{
		if (!properties.CurrentState.goblins.Enabled)
		{
			yield return null;
		}
		else
		{
			bat.StartGoblin();
		}
		yield return null;
	}
}
