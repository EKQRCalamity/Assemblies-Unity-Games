using System.Collections;
using UnityEngine;

public class TowerOfPowerLevel : Level
{
	private LevelProperties.TowerOfPower properties;

	[SerializeField]
	private TowerOfPowerLevelGameManager gameManager;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	[Multiline]
	private string _bossQuote;

	public override Levels CurrentLevel => Levels.TowerOfPower;

	public override Scenes CurrentScene => Scenes.scene_level_tower_of_power;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override float LevelIntroTime => 0f;

	public TowerOfPowerLevelGameManager GameManager => gameManager;

	protected override void PartialInit()
	{
		properties = LevelProperties.TowerOfPower.GetMode(base.mode);
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
		AbstractPlayerController[] array = players;
		foreach (AbstractPlayerController abstractPlayerController in array)
		{
			if (abstractPlayerController != null)
			{
				abstractPlayerController.gameObject.SetActive(value: false);
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (TowerOfPowerLevelGameInfo.GameInfo != null)
		{
			Level.Current.OnLoseEvent += TowerOfPowerLevelGameInfo.GameInfo.CleanUp;
		}
		base.OnLoseEvent += ResetScore;
	}

	protected override void OnPlayerJoined(PlayerId playerId)
	{
		TowerOfPowerLevelGameInfo.InitAddedPlayer(playerId, properties.CurrentState.slotMachine.DefaultStartingToken);
		base.OnPlayerJoined(playerId);
		TowerOfPowerLevelGameInfo.InitEquipment(playerId);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Level.IsTowerOfPowerMain = false;
		base.OnLoseEvent -= ResetScore;
	}

	private void ResetScore()
	{
		base.OnLoseEvent -= ResetScore;
		CleanUpScore();
	}

	protected override void CheckIfInABossesHub()
	{
		base.CheckIfInABossesHub();
		Level.IsTowerOfPower = true;
		Level.IsTowerOfPowerMain = true;
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(towerofpowerPattern_cr());
	}

	private IEnumerator towerofpowerPattern_cr()
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
		LevelProperties.TowerOfPower.Pattern p = properties.CurrentState.NextPattern;
		yield return null;
	}

	private void OnGUI()
	{
	}
}
