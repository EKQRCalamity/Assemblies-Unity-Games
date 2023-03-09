using System.Collections;
using UnityEngine;

public class AirshipClamLevel : Level
{
	private LevelProperties.AirshipClam properties;

	[SerializeField]
	private AirshipClamLevelClam clam;

	private bool attacking;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	[Multiline]
	private string _bossQuote;

	public override Levels CurrentLevel => Levels.AirshipClam;

	public override Scenes CurrentScene => Scenes.scene_level_airship_clam;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.AirshipClam.GetMode(base.mode);
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
		clam.LevelInit(properties);
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(airshipclamPattern_cr());
	}

	private IEnumerator airshipclamPattern_cr()
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
		case LevelProperties.AirshipClam.Pattern.Spit:
			StartCoroutine(spit_cr());
			break;
		case LevelProperties.AirshipClam.Pattern.Barnacles:
			StartCoroutine(barnacles_cr());
			break;
		default:
			yield return CupheadTime.WaitForSeconds(this, 1f);
			break;
		}
	}

	private IEnumerator spit_cr()
	{
		if (!attacking)
		{
			clam.OnSpitStart(EndAttack);
			attacking = true;
		}
		while (attacking)
		{
			yield return null;
		}
	}

	private IEnumerator barnacles_cr()
	{
		if (!attacking)
		{
			clam.OnBarnaclesStart(EndAttack);
			attacking = true;
		}
		while (attacking)
		{
			yield return null;
		}
	}

	private void EndAttack()
	{
		attacking = false;
	}
}
