using System.Collections;
using UnityEngine;

public class RobotLevel : Level
{
	private LevelProperties.Robot properties;

	[SerializeField]
	private RobotLevelRobot robot;

	[SerializeField]
	private RobotLevelHelihead heliHead;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortraitMain;

	[SerializeField]
	private Sprite _bossPortraitHeliHead;

	[SerializeField]
	private Sprite _bossPortraitInventor;

	[SerializeField]
	private string _bossQuoteMain;

	[SerializeField]
	private string _bossQuoteHeliHead;

	[SerializeField]
	private string _bossQuoteInventor;

	public override Levels CurrentLevel => Levels.Robot;

	public override Scenes CurrentScene => Scenes.scene_level_robot;

	public override Sprite BossPortrait
	{
		get
		{
			switch (properties.CurrentState.stateName)
			{
			case LevelProperties.Robot.States.Main:
			case LevelProperties.Robot.States.Generic:
				return _bossPortraitMain;
			case LevelProperties.Robot.States.HeliHead:
				return _bossPortraitHeliHead;
			case LevelProperties.Robot.States.Inventor:
				return _bossPortraitInventor;
			default:
				Debug.LogError(string.Concat("Couldn't find portrait for state ", properties.CurrentState.stateName, ". Using Main."));
				return _bossPortraitMain;
			}
		}
	}

	public override string BossQuote
	{
		get
		{
			switch (properties.CurrentState.stateName)
			{
			case LevelProperties.Robot.States.Main:
			case LevelProperties.Robot.States.Generic:
				return _bossQuoteMain;
			case LevelProperties.Robot.States.HeliHead:
				return _bossQuoteHeliHead;
			case LevelProperties.Robot.States.Inventor:
				return _bossQuoteInventor;
			default:
				Debug.LogError(string.Concat("Couldn't find quote for state ", properties.CurrentState.stateName, ". Using Main."));
				return _bossQuoteMain;
			}
		}
	}

	protected override void PartialInit()
	{
		properties = LevelProperties.Robot.GetMode(base.mode);
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
		properties.OnBossDamaged -= base.timeline.DealDamage;
		float[] array = new float[base.timeline.events.Count];
		for (int i = 0; i < base.timeline.events.Count; i++)
		{
			array[i] = base.timeline.events[i].percentage;
		}
		base.timeline = new Timeline();
		base.timeline.health = 0f;
		base.timeline.health += properties.CurrentState.hose.health;
		base.timeline.health += properties.CurrentState.orb.chestHP;
		base.timeline.health += properties.CurrentState.shotBot.hatchGateHealth;
		base.timeline.health += properties.CurrentState.heart.heartHP;
		float num = base.timeline.health;
		if (Level.Current.mode != 0)
		{
			for (int j = 0; j < array.Length; j++)
			{
				float num2 = properties.TotalHealth * ((j >= array.Length - 1) ? array[j] : (array[j] - array[j + 1]));
				Level.Current.timeline.health += num2;
			}
			base.timeline.AddEvent(new Timeline.Event(string.Empty, 1f - num / Level.Current.timeline.health));
			for (int k = 0; k < array.Length; k++)
			{
				num += properties.TotalHealth * ((k >= array.Length - 1) ? array[k] : (array[k] - array[k + 1]));
				if (k < array.Length - 1)
				{
					base.timeline.AddEvent(new Timeline.Event(string.Empty, 1f - num / Level.Current.timeline.health));
				}
			}
		}
		robot.LevelInit(properties);
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(robotPattern_cr());
	}

	protected override void OnStateChanged()
	{
		base.OnStateChanged();
		switch (properties.CurrentState.stateName)
		{
		case LevelProperties.Robot.States.HeliHead:
			StopAllCoroutines();
			robot.TriggerPhaseTwo(OnHeliheadSpawn);
			break;
		case LevelProperties.Robot.States.Inventor:
			heliHead.ChangeState();
			break;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_bossPortraitHeliHead = null;
		_bossPortraitInventor = null;
		_bossPortraitMain = null;
	}

	private IEnumerator robotPattern_cr()
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
		if (properties.CurrentState.NextPattern == LevelProperties.Robot.Pattern.Default)
		{
			yield return null;
		}
		else
		{
			yield return CupheadTime.WaitForSeconds(this, 1f);
		}
	}

	private void OnHeliheadSpawn()
	{
		StartCoroutine(spawnHeliHead_cr());
	}

	private IEnumerator spawnHeliHead_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 2.5f);
		robot.animator.SetTrigger("Phase2Transition");
		yield return robot.animator.WaitForAnimationToEnd(this, "Death Dance", waitForEndOfFrame: true);
		heliHead.GetComponent<RobotLevelHelihead>().InitHeliHead(properties);
	}
}
