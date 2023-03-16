using System.Collections;
using UnityEngine;

public class DicePalaceFlyingMemoryLevel : AbstractDicePalaceLevel
{
	private LevelProperties.DicePalaceFlyingMemory properties;

	[SerializeField]
	private DicePalaceFlyingMemoryLevelStuffedToy stuffedToy;

	[SerializeField]
	private DicePalaceFlyingMemoryLevelGameManager gameManager;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	private string _bossQuote;

	public override DicePalaceLevels CurrentDicePalaceLevel => DicePalaceLevels.DicePalaceFlyingMemory;

	public override Levels CurrentLevel => Levels.DicePalaceFlyingMemory;

	public override Scenes CurrentScene => Scenes.scene_level_dice_palace_flying_memory;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.DicePalaceFlyingMemory.GetMode(base.mode);
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
		gameManager.LevelInit(properties);
		stuffedToy.LevelInit(properties);
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(dicepalaceflyingmemoryPattern_cr());
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_bossPortrait = null;
	}

	private IEnumerator dicepalaceflyingmemoryPattern_cr()
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
		if (properties.CurrentState.NextPattern == LevelProperties.DicePalaceFlyingMemory.Pattern.Default)
		{
			yield return null;
		}
		else
		{
			yield return CupheadTime.WaitForSeconds(this, 1f);
		}
	}
}
