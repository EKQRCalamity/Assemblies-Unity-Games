using System.Collections.Generic;
using UnityEngine;

public class TowerOfPowerLevelGameInfo : AbstractMonoBehaviour
{
	private static TowerOfPowerLevelGameInfo gameInfo;

	public static List<Levels> allStageSpaces = new List<Levels>();

	public static Level.Mode[] difficultyByBossIndex;

	public static List<string> SlotOne = new List<string>();

	public static List<string> SlotTwo = new List<string>();

	public static List<string> SlotThree = new List<string>();

	public static List<string> SlotThreeChalice = new List<string>();

	public static List<string> SlotFour = new List<string>();

	public static List<string> SlotFourChalice = new List<string>();

	public static Level.Mode baseDifficulty;

	public static int CURRENT_TURN;

	public static int TURN_COUNTER;

	public static int MIN_RANK_NEED_TO_GET_TOKEN;

	public static PlayersStatsBossesHub[] PLAYER_STATS = new PlayersStatsBossesHub[2];

	public static TowerOfPowerLevelGameInfo GameInfo
	{
		get
		{
			if (gameInfo == null)
			{
				GameObject gameObject = new GameObject();
				gameObject.name = "GameInfo";
				gameInfo = gameObject.AddComponent<TowerOfPowerLevelGameInfo>();
			}
			return gameInfo;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		gameInfo = this;
		PLAYER_STATS[0] = null;
		PLAYER_STATS[1] = null;
		Object.DontDestroyOnLoad(this);
	}

	public void CleanUp()
	{
		TURN_COUNTER = 0;
		ResetWeapons(PlayerId.PlayerOne);
		if (PlayerManager.Multiplayer)
		{
			ResetWeapons(PlayerId.PlayerTwo);
		}
		PLAYER_STATS[0] = null;
		PLAYER_STATS[1] = null;
		Object.Destroy(base.gameObject);
	}

	public static void ResetTowerOfPower()
	{
		TURN_COUNTER = 0;
		CURRENT_TURN = 0;
		ResetWeapons(PlayerId.PlayerOne);
		if (PlayerManager.Multiplayer)
		{
			ResetWeapons(PlayerId.PlayerTwo);
		}
		PLAYER_STATS[0] = null;
		PLAYER_STATS[1] = null;
	}

	public static void InitAddedPlayer(PlayerId playerId, int startingToken)
	{
		PLAYER_STATS[(int)playerId] = new PlayersStatsBossesHub();
		if (CURRENT_TURN == 0)
		{
			PLAYER_STATS[(int)playerId].HP = 3;
			PLAYER_STATS[(int)playerId].BonusHP = 3;
			PLAYER_STATS[(int)playerId].SuperCharge = 0f;
			PLAYER_STATS[(int)playerId].tokenCount = startingToken;
		}
		else
		{
			PLAYER_STATS[(int)playerId].HP = 1;
			PLAYER_STATS[(int)playerId].BonusHP = 0;
			PLAYER_STATS[(int)playerId].SuperCharge = 0f;
			PLAYER_STATS[(int)playerId].tokenCount = 0;
		}
	}

	public static void ResetWeapons(PlayerId playerId)
	{
		if (PLAYER_STATS[(int)playerId] != null)
		{
			PlayerData.PlayerLoadouts.PlayerLoadout playerLoadout = PlayerData.Data.Loadouts.GetPlayerLoadout(playerId);
			playerLoadout.primaryWeapon = PLAYER_STATS[(int)playerId].basePrimaryWeapon;
			playerLoadout.secondaryWeapon = PLAYER_STATS[(int)playerId].baseSecondaryWeapon;
			playerLoadout.super = PLAYER_STATS[(int)playerId].BaseSuper;
			playerLoadout.charm = PLAYER_STATS[(int)playerId].BaseCharm;
			PlayerData.SaveCurrentFile();
		}
	}

	public static void InitEquipment(PlayerId playerId)
	{
		if (PLAYER_STATS[(int)playerId] == null)
		{
			PLAYER_STATS[(int)playerId] = new PlayersStatsBossesHub();
		}
		PlayerData.PlayerLoadouts.PlayerLoadout playerLoadout = PlayerData.Data.Loadouts.GetPlayerLoadout(playerId);
		PLAYER_STATS[(int)playerId].basePrimaryWeapon = playerLoadout.primaryWeapon;
		PLAYER_STATS[(int)playerId].baseSecondaryWeapon = playerLoadout.secondaryWeapon;
		PLAYER_STATS[(int)playerId].BaseSuper = playerLoadout.super;
		PLAYER_STATS[(int)playerId].BaseCharm = playerLoadout.charm;
	}

	public static void SetPlayersStats(PlayerId playerId)
	{
		if (PLAYER_STATS[(int)playerId] == null)
		{
			PLAYER_STATS[(int)playerId] = new PlayersStatsBossesHub();
		}
		PlayerStatsManager stats = PlayerManager.GetPlayer(playerId).stats;
		PLAYER_STATS[(int)playerId].HP = stats.Health;
		PLAYER_STATS[(int)playerId].SuperCharge = stats.SuperMeter;
	}

	public static void AddToken()
	{
		if (PLAYER_STATS[0] != null)
		{
			if (PLAYER_STATS[0].HP > 0)
			{
				PLAYER_STATS[0].tokenCount++;
			}
			if (PlayerManager.Multiplayer && PLAYER_STATS[1].HP > 0)
			{
				PLAYER_STATS[1].tokenCount++;
			}
		}
	}

	public static void ReduceToken()
	{
		PLAYER_STATS[0].tokenCount--;
		PLAYER_STATS[0].tokenCount = Mathf.Max(0, PLAYER_STATS[0].tokenCount);
		if (PlayerManager.Multiplayer)
		{
			PLAYER_STATS[1].tokenCount--;
			PLAYER_STATS[1].tokenCount = Mathf.Max(0, PLAYER_STATS[1].tokenCount);
		}
	}

	public static void ReduceToken(int playerNum)
	{
		if (PLAYER_STATS[playerNum] != null && PLAYER_STATS[playerNum].tokenCount != 0)
		{
			PLAYER_STATS[playerNum].tokenCount--;
		}
	}

	public static void SetDefaultToken(int defaultTokenCount)
	{
		if (PLAYER_STATS[0] == null)
		{
			PLAYER_STATS[0] = new PlayersStatsBossesHub();
		}
		PLAYER_STATS[0].tokenCount = defaultTokenCount;
		if (PlayerManager.Multiplayer)
		{
			if (PLAYER_STATS[1] == null)
			{
				PLAYER_STATS[1] = new PlayersStatsBossesHub();
			}
			PLAYER_STATS[1].tokenCount = defaultTokenCount;
		}
	}

	public static bool IsTokenLeft()
	{
		if (PLAYER_STATS[0] == null && PLAYER_STATS[1] == null)
		{
			return false;
		}
		int tokenCount = PLAYER_STATS[0].tokenCount;
		int num = (PlayerManager.Multiplayer ? PLAYER_STATS[1].tokenCount : 0);
		return tokenCount > 0 || num > 0;
	}

	public static bool IsTokenLeft(int playerNum)
	{
		if (PLAYER_STATS[playerNum] == null || PLAYER_STATS[playerNum].tokenCount == 0)
		{
			return false;
		}
		return true;
	}

	private void OnDestroy()
	{
		ResetWeapons(PlayerId.PlayerOne);
		if (PlayerManager.Multiplayer)
		{
			ResetWeapons(PlayerId.PlayerTwo);
		}
	}
}
