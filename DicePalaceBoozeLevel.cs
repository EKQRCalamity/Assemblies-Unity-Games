using System.Collections;
using UnityEngine;

public class DicePalaceBoozeLevel : AbstractDicePalaceLevel
{
	private LevelProperties.DicePalaceBooze properties;

	[SerializeField]
	private Transform[] lamps;

	[SerializeField]
	private DicePalaceBoozeLevelDecanter decanter;

	[SerializeField]
	private DicePalaceBoozeLevelMartini martini;

	[SerializeField]
	private DicePalaceBoozeLevelTumbler tumbler;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	private string _bossQuote;

	public override DicePalaceLevels CurrentDicePalaceLevel => DicePalaceLevels.DicePalaceBooze;

	public override Levels CurrentLevel => Levels.DicePalaceBooze;

	public override Scenes CurrentScene => Scenes.scene_level_dice_palace_booze;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.DicePalaceBooze.GetMode(base.mode);
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
		decanter.LevelInit(properties);
		martini.LevelInit(properties);
		tumbler.LevelInit(properties);
		properties.OnBossDamaged -= base.timeline.DealDamage;
		base.timeline = new Timeline();
		base.timeline.health = properties.CurrentState.decanter.decanterHP + properties.CurrentState.martini.martiniHP + properties.CurrentState.tumbler.tumblerHP;
		Transform[] array = lamps;
		foreach (Transform lamp in array)
		{
			StartCoroutine(lamps_cr(lamp));
		}
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(dicepalaceboozePattern_cr());
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_bossPortrait = null;
	}

	private IEnumerator dicepalaceboozePattern_cr()
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
		if (properties.CurrentState.NextPattern == LevelProperties.DicePalaceBooze.Pattern.Default)
		{
			yield return null;
		}
		else
		{
			yield return CupheadTime.WaitForSeconds(this, 1f);
		}
	}

	private IEnumerator lamps_cr(Transform lamp)
	{
		float t = 0f;
		float maxSpeed2 = 0f;
		float speed2 = maxSpeed2;
		while (true)
		{
			t = 0f;
			maxSpeed2 = Random.Range(5f, 15f);
			speed2 = maxSpeed2;
			while (!CupheadLevelCamera.Current.isShaking)
			{
				yield return null;
			}
			bool movingRight = Rand.Bool();
			while (speed2 > 0f)
			{
				t = ((!movingRight) ? (t - (float)CupheadTime.Delta) : (t + (float)CupheadTime.Delta));
				float phase = Mathf.Sin(t);
				lamp.localRotation = Quaternion.Euler(new Vector3(0f, 0f, phase * speed2));
				speed2 -= 0.05f;
				yield return null;
			}
			yield return null;
		}
	}
}
