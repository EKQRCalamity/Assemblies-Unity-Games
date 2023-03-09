using System.Collections.Generic;
using UnityEngine;

public class DicePalaceMainLevelGameInfo : AbstractMonoBehaviour
{
	private static DicePalaceMainLevelGameInfo gameInfo;

	public static int TURN_COUNTER;

	public static int PLAYER_SPACES_MOVED;

	public static List<int> SAFE_INDEXES;

	public static int[] HEART_INDEXES = new int[3];

	public static PlayersStatsBossesHub PLAYER_ONE_STATS;

	public static PlayersStatsBossesHub PLAYER_TWO_STATS;

	public static bool PLAYED_INTRO_SFX;

	public static bool IS_FIRST_ENTRY = true;

	public static int CHALICE_PLAYER = -1;

	public static DicePalaceMainLevelGameInfo GameInfo
	{
		get
		{
			if (gameInfo == null)
			{
				GameObject gameObject = new GameObject();
				gameObject.name = "GameInfo";
				gameInfo = gameObject.AddComponent<DicePalaceMainLevelGameInfo>();
			}
			return gameInfo;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		gameInfo = this;
		IS_FIRST_ENTRY = true;
		SAFE_INDEXES = new List<int>();
		ChooseHearts();
		Object.DontDestroyOnLoad(this);
	}

	public void CleanUp()
	{
		SAFE_INDEXES.Clear();
		TURN_COUNTER = 0;
		PLAYER_SPACES_MOVED = 0;
		ChooseHearts();
		PLAYER_ONE_STATS = null;
		PLAYER_TWO_STATS = null;
		PLAYED_INTRO_SFX = false;
		CHALICE_PLAYER = -1;
		IS_FIRST_ENTRY = true;
		Object.Destroy(base.gameObject);
	}

	public static void CleanUpRetry()
	{
		SAFE_INDEXES.Clear();
		TURN_COUNTER = 0;
		PLAYER_SPACES_MOVED = 0;
		ChooseHearts();
		PLAYER_ONE_STATS = null;
		PLAYER_TWO_STATS = null;
		PLAYED_INTRO_SFX = false;
		CHALICE_PLAYER = -1;
		IS_FIRST_ENTRY = true;
	}

	private static void ChooseHearts()
	{
		HEART_INDEXES[0] = Random.Range(0, 3);
		HEART_INDEXES[1] = Random.Range(4, 7);
		HEART_INDEXES[2] = Random.Range(8, 11);
	}

	public static void SetPlayersStats()
	{
		if (PLAYER_ONE_STATS == null)
		{
			PLAYER_ONE_STATS = new PlayersStatsBossesHub();
		}
		PlayerStatsManager stats = PlayerManager.GetPlayer(PlayerId.PlayerOne).stats;
		PLAYER_ONE_STATS.healerHP = stats.HealerHP;
		PLAYER_ONE_STATS.healerHPReceived = stats.HealerHPReceived;
		PLAYER_ONE_STATS.healerHPCounter = stats.HealerHPCounter;
		PLAYER_ONE_STATS.HP = stats.Health;
		PLAYER_ONE_STATS.SuperCharge = stats.SuperMeter;
		if (PlayerManager.Multiplayer)
		{
			if (PLAYER_TWO_STATS == null)
			{
				PLAYER_TWO_STATS = new PlayersStatsBossesHub();
			}
			PlayerStatsManager stats2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo).stats;
			PLAYER_TWO_STATS.healerHP = stats2.HealerHP;
			PLAYER_TWO_STATS.healerHPReceived = stats2.HealerHPReceived;
			PLAYER_TWO_STATS.healerHPCounter = stats2.HealerHPCounter;
			PLAYER_TWO_STATS.HP = stats2.Health;
			PLAYER_TWO_STATS.SuperCharge = stats2.SuperMeter;
		}
	}
}
