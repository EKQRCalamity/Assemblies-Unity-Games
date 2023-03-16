using System.Collections;
using UnityEngine;

public class DicePalaceChipsLevel : AbstractDicePalaceLevel
{
	private LevelProperties.DicePalaceChips properties;

	[SerializeField]
	private GameObject background;

	[SerializeField]
	private DicePalaceChipsLevelChips chips;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	private string _bossQuote;

	public override DicePalaceLevels CurrentDicePalaceLevel => DicePalaceLevels.DicePalaceChips;

	public override Levels CurrentLevel => Levels.DicePalaceChips;

	public override Scenes CurrentScene => Scenes.scene_level_dice_palace_chips;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.DicePalaceChips.GetMode(base.mode);
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
		chips.LevelInit(properties);
		StartCoroutine(CupheadLevelCamera.Current.rotate_camera());
		StartCoroutine(rotate_background_cr());
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(dicepalacechipsPattern_cr());
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_bossPortrait = null;
	}

	private IEnumerator dicepalacechipsPattern_cr()
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
		if (properties.CurrentState.NextPattern == LevelProperties.DicePalaceChips.Pattern.Default)
		{
			yield return null;
		}
		else
		{
			yield return CupheadTime.WaitForSeconds(this, 1f);
		}
	}

	private IEnumerator rotate_background_cr()
	{
		float time = 1.5f;
		float t = 0f;
		while (true)
		{
			t += (float)CupheadTime.Delta;
			float phase = Mathf.Sin(t / time);
			background.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, phase * 1f));
			yield return null;
		}
	}
}
