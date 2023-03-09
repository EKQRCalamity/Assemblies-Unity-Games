using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TowerOfPowerLevelGameManager : LevelProperties.TowerOfPower.Entity
{
	public enum Charm_Slot
	{
		charm_health_up_1,
		charm_health_up_2,
		charm_super_builder,
		charm_smoke_dash,
		charm_parry_plus,
		charm_pit_saver,
		charm_parry_attack,
		charm_chalice,
		charm_directional_dash,
		None,
		charm_extra_token
	}

	public enum Weapon_Slot
	{
		level_weapon_peashot,
		level_weapon_spreadshot,
		level_weapon_arc,
		level_weapon_homing,
		level_weapon_exploder,
		level_weapon_boomerang,
		level_weapon_charge,
		level_weapon_bouncer,
		level_weapon_wide_shot,
		plane_weapon_peashot,
		plane_weapon_laser,
		plane_weapon_bomb,
		plane_chalice_weapon_3way,
		arcade_weapon_peashot,
		arcade_weapon_rocket_peashot,
		None
	}

	public enum Super_Slot
	{
		level_super_beam,
		level_super_ghost,
		level_super_invincible,
		level_super_chalice_shmup,
		level_super_chalice_vert_beam,
		level_super_chalice_shield,
		plane_super_bomb,
		plane_super_chalice_stream,
		None
	}

	private const string PREWEAPON_NAME = "level_weapon_";

	private const string PRESUPER_NAME = "level_super_";

	private const string PRESUPER_CHALICE_NAME = "level_super_chalice_";

	private const string PRECHARM_NAME = "charm_";

	private const string PRECHARM_CHALICE_NAME = "charm_chalice_";

	[SerializeField]
	private float advanceDelay = 10f;

	private CupheadInput.AnyPlayerInput anyInput;

	private int[] bonusHP = new int[2];

	private int[] bonusToken = new int[2];

	private List<Levels>[] BossPools;

	private List<Levels>[] ShmupPools;

	private List<Levels>[] KingDicePools;

	private List<int> ShmupPlacement = new List<int>();

	private int[] SlotMachineWeapon2Attempt = new int[2];

	public bool[] slotsAreSpinning = new bool[2];

	public bool[] slotsCanSpinAgain = new bool[2];

	public bool[] slotsConfirm = new bool[2];

	private bool[] slotsDone = new bool[2] { true, true };

	private int waitForButtonRelease;

	[SerializeField]
	private TowerOfPowerScorecard scorecard;

	public bool showingScorecard;

	[SerializeField]
	private bool debugForceSlotMachineEveryTurn;

	[SerializeField]
	private bool debugForceSlotMachineAfterOneFight;

	[SerializeField]
	private bool debugSkipToLastFight;

	public override void LevelInit(LevelProperties.TowerOfPower properties)
	{
		base.LevelInit(properties);
		anyInput = new CupheadInput.AnyPlayerInput();
		TowerOfPowerLevelGameInfo.CURRENT_TURN = TowerOfPowerLevelGameInfo.TURN_COUNTER;
		if (TowerOfPowerLevelGameInfo.CURRENT_TURN == 0)
		{
			TowerOfPowerLevelGameInfo.baseDifficulty = Level.Current.mode;
			TowerOfPowerLevelGameInfo.SetDefaultToken(properties.CurrentState.slotMachine.DefaultStartingToken);
			TowerOfPowerLevelGameInfo.MIN_RANK_NEED_TO_GET_TOKEN = properties.CurrentState.slotMachine.MinRankToGainToken;
			InitDifficultyBossByIndex();
			InitPools();
			SetTowerBosses();
			InitSlotMachine();
			TowerOfPowerLevelGameInfo.InitEquipment(PlayerId.PlayerOne);
			if (PlayerManager.Multiplayer)
			{
				TowerOfPowerLevelGameInfo.InitEquipment(PlayerId.PlayerTwo);
			}
			if (debugSkipToLastFight)
			{
				TowerOfPowerLevelGameInfo.TURN_COUNTER = TowerOfPowerLevelGameInfo.allStageSpaces.Count - 1;
				TowerOfPowerLevelGameInfo.CURRENT_TURN = TowerOfPowerLevelGameInfo.TURN_COUNTER;
			}
		}
		StartCoroutine(main_cr());
	}

	public void ChangePlayersWeapon(PlayerId playerId)
	{
		if (playerId == PlayerId.PlayerTwo && !PlayerManager.Multiplayer)
		{
			return;
		}
		bool flag = TowerOfPowerLevelGameInfo.PLAYER_STATS[(int)playerId].BaseCharm == Charm.charm_chalice;
		bonusHP[(int)playerId] = 0;
		bonusToken[(int)playerId] = 0;
		SlotMachineWeapon2Attempt[(int)playerId] = 0;
		Weapon_Slot weaponSlotEnumByName = GetWeaponSlotEnumByName(TowerOfPowerLevelGameInfo.SlotOne.RandomChoice());
		int count = TowerOfPowerLevelGameInfo.SlotTwo.Count;
		int num = UnityEngine.Random.Range(0, count);
		Weapon_Slot weaponSlotEnumByName2 = GetWeaponSlotEnumByName(TowerOfPowerLevelGameInfo.SlotTwo[num]);
		while (weaponSlotEnumByName2 == weaponSlotEnumByName)
		{
			num++;
			SlotMachineWeapon2Attempt[(int)playerId]++;
			if (num >= count)
			{
				num = 0;
			}
			weaponSlotEnumByName2 = GetWeaponSlotEnumByName(TowerOfPowerLevelGameInfo.SlotTwo[num]);
			if (SlotMachineWeapon2Attempt[(int)playerId] == count)
			{
				Debug.LogError("The slotTwo list needs at least two kinds of weapon. Modify the Tower of Power in the Level Editor--Slot Two weapon in the SlotMachine section.");
				break;
			}
		}
		Charm_Slot charmSlotEnumByName;
		Super_Slot superSlotEnumByName;
		if (flag)
		{
			charmSlotEnumByName = GetCharmSlotEnumByName(TowerOfPowerLevelGameInfo.SlotThreeChalice.RandomChoice());
			superSlotEnumByName = GetSuperSlotEnumByName(TowerOfPowerLevelGameInfo.SlotFourChalice.RandomChoice());
		}
		else
		{
			charmSlotEnumByName = GetCharmSlotEnumByName(TowerOfPowerLevelGameInfo.SlotThree.RandomChoice());
			superSlotEnumByName = GetSuperSlotEnumByName(TowerOfPowerLevelGameInfo.SlotFour.RandomChoice());
		}
		if (charmSlotEnumByName == Charm_Slot.charm_extra_token)
		{
			bonusToken[(int)playerId] = 1;
		}
		switch (charmSlotEnumByName)
		{
		case Charm_Slot.charm_health_up_1:
			bonusHP[(int)playerId] = 1;
			break;
		case Charm_Slot.charm_health_up_2:
			bonusHP[(int)playerId] = 2;
			break;
		}
		PlayerData.PlayerLoadouts.PlayerLoadout playerLoadout = PlayerData.Data.Loadouts.GetPlayerLoadout(playerId);
		playerLoadout.primaryWeapon = GetWeaponEnumByName(weaponSlotEnumByName.ToString());
		playerLoadout.secondaryWeapon = GetWeaponEnumByName(weaponSlotEnumByName2.ToString());
		playerLoadout.charm = GetCharmEnumByName(charmSlotEnumByName.ToString());
		playerLoadout.super = GetSuperEnumByName(superSlotEnumByName.ToString());
	}

	private IEnumerator startMiniBoss_cr(Levels level)
	{
		TowerOfPowerLevelGameInfo.SetPlayersStats(PlayerId.PlayerOne);
		if (PlayerManager.Multiplayer)
		{
			TowerOfPowerLevelGameInfo.SetPlayersStats(PlayerId.PlayerTwo);
		}
		Level.ScoringData.time += Level.Current.LevelTime;
		SceneLoader.LoadLevel(level, SceneLoader.Transition.Fade);
		yield return null;
	}

	public static Weapon_Slot GetWeaponSlotEnumByName(string Name)
	{
		return (Weapon_Slot)Enum.Parse(typeof(Weapon_Slot), Name);
	}

	public static Charm_Slot GetCharmSlotEnumByName(string Name)
	{
		return (Charm_Slot)Enum.Parse(typeof(Charm_Slot), Name);
	}

	public static Super_Slot GetSuperSlotEnumByName(string Name)
	{
		return (Super_Slot)Enum.Parse(typeof(Super_Slot), Name);
	}

	public static Weapon GetWeaponEnumByName(string Name)
	{
		Weapon result = Weapon.None;
		if (Enum.IsDefined(typeof(Weapon), Name))
		{
			result = (Weapon)Enum.Parse(typeof(Weapon), Name);
		}
		return result;
	}

	public static Charm GetCharmEnumByName(string Name)
	{
		Charm result = Charm.None;
		if (Enum.IsDefined(typeof(Charm), Name))
		{
			result = (Charm)Enum.Parse(typeof(Charm), Name);
		}
		return result;
	}

	public static Super GetSuperEnumByName(string Name)
	{
		Super result = Super.None;
		if (Enum.IsDefined(typeof(Super), Name))
		{
			result = (Super)Enum.Parse(typeof(Super), Name);
		}
		return result;
	}

	private void InitPools()
	{
		InitBossPools();
		InitShmupPools();
		InitKingDicePools();
	}

	private void InitBossPools()
	{
		string[] array = base.properties.CurrentState.bossesPropertises.PoolOneString.Split(',');
		string[] array2 = base.properties.CurrentState.bossesPropertises.PoolTwoString.Split(',');
		string[] array3 = base.properties.CurrentState.bossesPropertises.PoolThreeString.Split(',');
		BossPools = new List<Levels>[3];
		BossPools[0] = new List<Levels>();
		for (int i = 0; i < array.Length; i++)
		{
			BossPools[0].Add(Level.GetEnumByName(array[i]));
		}
		BossPools[1] = new List<Levels>();
		for (int j = 0; j < array2.Length; j++)
		{
			BossPools[1].Add(Level.GetEnumByName(array2[j]));
		}
		BossPools[2] = new List<Levels>();
		for (int k = 0; k < array3.Length; k++)
		{
			BossPools[2].Add(Level.GetEnumByName(array3[k]));
		}
	}

	private void InitShmupPools()
	{
		ShmupPlacement.Clear();
		string[] array = base.properties.CurrentState.bossesPropertises.ShmupPoolOneString.Split(',');
		string[] array2 = base.properties.CurrentState.bossesPropertises.ShmupPoolTwoString.Split(',');
		string[] array3 = base.properties.CurrentState.bossesPropertises.ShmupPoolThreeString.Split(',');
		List<string> list = base.properties.CurrentState.bossesPropertises.ShmupPlacementString.Split(',').ToList();
		string shmupCountString = base.properties.CurrentState.bossesPropertises.ShmupCountString;
		string[] array4 = shmupCountString.Split(',');
		int num = Parser.IntParse(array4[UnityEngine.Random.Range(0, array4.Length)]);
		if (num > 0)
		{
			do
			{
				int placement = Parser.IntParse(list[UnityEngine.Random.Range(0, list.Count)]);
				ShmupPlacement.Add(placement);
				list.RemoveAll((string x) => x == placement.ToString());
			}
			while (ShmupPlacement.Count < num && list.Count != 0);
		}
		ShmupPools = new List<Levels>[3];
		ShmupPools[0] = new List<Levels>();
		for (int i = 0; i < array.Length; i++)
		{
			ShmupPools[0].Add(Level.GetEnumByName(array[i]));
		}
		ShmupPools[1] = new List<Levels>();
		for (int j = 0; j < array2.Length; j++)
		{
			ShmupPools[1].Add(Level.GetEnumByName(array2[j]));
		}
		ShmupPools[2] = new List<Levels>();
		for (int k = 0; k < array3.Length; k++)
		{
			ShmupPools[2].Add(Level.GetEnumByName(array3[k]));
		}
	}

	private void InitKingDicePools()
	{
		string[] array = base.properties.CurrentState.bossesPropertises.KingDicePoolOneString.Split(',');
		string[] array2 = base.properties.CurrentState.bossesPropertises.KingDicePoolTwoString.Split(',');
		string[] array3 = base.properties.CurrentState.bossesPropertises.KingDicePoolThreeString.Split(',');
		string[] array4 = base.properties.CurrentState.bossesPropertises.KingDicePoolFourString.Split(',');
		int kingDiceMiniBossCount = base.properties.CurrentState.bossesPropertises.KingDiceMiniBossCount;
		KingDicePools = new List<Levels>[4];
		KingDicePools[0] = new List<Levels>();
		for (int i = 0; i < array.Length; i++)
		{
			KingDicePools[0].Add(Level.GetEnumByName(array[i]));
		}
		KingDicePools[1] = new List<Levels>();
		for (int j = 0; j < array2.Length; j++)
		{
			KingDicePools[1].Add(Level.GetEnumByName(array2[j]));
		}
		KingDicePools[2] = new List<Levels>();
		for (int k = 0; k < array3.Length; k++)
		{
			KingDicePools[2].Add(Level.GetEnumByName(array3[k]));
		}
		KingDicePools[3] = new List<Levels>();
		for (int l = 0; l < array4.Length; l++)
		{
			KingDicePools[3].Add(Level.GetEnumByName(array4[l]));
		}
	}

	private void SetTowerBosses()
	{
		TowerOfPowerLevelGameInfo.allStageSpaces.Clear();
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (i == 2 && j == 2)
				{
					int kingDiceMiniBossCount = base.properties.CurrentState.bossesPropertises.KingDiceMiniBossCount;
					for (int k = 0; k < kingDiceMiniBossCount; k++)
					{
						SetKingDiceBosses(k);
					}
					TowerOfPowerLevelGameInfo.allStageSpaces.Add(Levels.DicePalaceMain);
					if (base.properties.CurrentState.bossesPropertises.DevilFinalBoss)
					{
						TowerOfPowerLevelGameInfo.allStageSpaces.Add(Levels.Devil);
					}
				}
				else
				{
					int count = TowerOfPowerLevelGameInfo.allStageSpaces.Count;
					if (ShmupPlacement.Contains(count + 1))
					{
						AddShmupInTower(ShmupPlacement.IndexOf(count + 1));
					}
					else
					{
						AddBossInTower(i);
					}
				}
			}
		}
	}

	private void AddBossInTower(int tier)
	{
		BossPools[tier].RemoveAll((Levels x) => TowerOfPowerLevelGameInfo.allStageSpaces.Contains(x));
		if (BossPools[tier].Count == 0)
		{
			Debug.LogError("Number of Boss in the pool " + tier + " is empty.");
			return;
		}
		Levels randLv = BossPools[tier].RandomChoice();
		if (TowerOfPowerLevelGameInfo.allStageSpaces.Contains(randLv))
		{
			Debug.LogError("RemoveAll(x => allStageSpaces.Contains(x) don't work like experted");
			return;
		}
		TowerOfPowerLevelGameInfo.allStageSpaces.Add(randLv);
		BossPools[tier].RemoveAll((Levels x) => x == randLv);
	}

	private void AddShmupInTower(int tier)
	{
		ShmupPools[tier].RemoveAll((Levels x) => TowerOfPowerLevelGameInfo.allStageSpaces.Contains(x));
		if (ShmupPools[tier].Count == 0)
		{
			Debug.LogError("Number of Boss in the pool " + tier + " is empty.");
			return;
		}
		Levels randLv = ShmupPools[tier].RandomChoice();
		if (TowerOfPowerLevelGameInfo.allStageSpaces.Contains(randLv))
		{
			Debug.LogError("RemoveAll(x => allStageSpaces.Contains(x) don't work like experted");
			return;
		}
		TowerOfPowerLevelGameInfo.allStageSpaces.Add(randLv);
		ShmupPools[tier].RemoveAll((Levels x) => x == randLv);
	}

	private void SetKingDiceBosses(int tier)
	{
		KingDicePools[tier].RemoveAll((Levels x) => TowerOfPowerLevelGameInfo.allStageSpaces.Contains(x));
		if (KingDicePools[tier].Count == 0)
		{
			Debug.LogError("Number of Boss in the pool " + tier + " is empty.");
			return;
		}
		Levels randLv = KingDicePools[tier].RandomChoice();
		if (TowerOfPowerLevelGameInfo.allStageSpaces.Contains(randLv))
		{
			Debug.LogError("RemoveAll(x => allStageSpaces.Contains(x) don't work like expected");
			return;
		}
		TowerOfPowerLevelGameInfo.allStageSpaces.Add(randLv);
		KingDicePools[tier].RemoveAll((Levels x) => x == randLv);
	}

	private void InitDifficultyBossByIndex()
	{
		string[] array = base.properties.CurrentState.bossesPropertises.MiniBossDifficultyByIndex.Split(',');
		TowerOfPowerLevelGameInfo.difficultyByBossIndex = new Level.Mode[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			int result = 0;
			Parser.IntTryParse(array[i], out result);
			TowerOfPowerLevelGameInfo.difficultyByBossIndex[i] = (Level.Mode)result;
		}
	}

	private void InitSlotMachine()
	{
		string[] array = base.properties.CurrentState.slotMachine.SlotOneWeapon.Split(',');
		string[] array2 = base.properties.CurrentState.slotMachine.SlotTwoWeapon.Split(',');
		string[] array3 = base.properties.CurrentState.slotMachine.SlotThreeCharm.Split(',');
		string[] array4 = base.properties.CurrentState.slotMachine.SlotThreeChalice.Split(',');
		string[] array5 = base.properties.CurrentState.slotMachine.SlotFourSuper.Split(',');
		string[] array6 = base.properties.CurrentState.slotMachine.SlotFourChalice.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			string item = ((!(array[i] != "None")) ? array[i] : ("level_weapon_" + array[i]));
			TowerOfPowerLevelGameInfo.SlotOne.Add(item);
		}
		for (int j = 0; j < array2.Length; j++)
		{
			string item = ((!(array2[j] != "None")) ? array2[j] : ("level_weapon_" + array2[j]));
			TowerOfPowerLevelGameInfo.SlotTwo.Add(item);
		}
		for (int k = 0; k < array3.Length; k++)
		{
			string item = ((!(array3[k] != "None")) ? array3[k] : ("charm_" + array3[k]));
			TowerOfPowerLevelGameInfo.SlotThree.Add(item);
		}
		for (int l = 0; l < array4.Length; l++)
		{
			string item = ((!(array4[l] != "None")) ? array4[l] : ("charm_" + array4[l]));
			TowerOfPowerLevelGameInfo.SlotThreeChalice.Add(item);
		}
		for (int m = 0; m < array5.Length; m++)
		{
			string item = ((!(array5[m] != "None")) ? array5[m] : ("level_super_" + array5[m]));
			TowerOfPowerLevelGameInfo.SlotFour.Add(item);
		}
		for (int n = 0; n < array6.Length; n++)
		{
			string item = ((!(array6[n] != "None")) ? array6[n] : ("level_super_chalice_" + array6[n]));
			TowerOfPowerLevelGameInfo.SlotFourChalice.Add(item);
		}
	}

	private void SetDifficulty()
	{
		int num = TowerOfPowerLevelGameInfo.CURRENT_TURN;
		if (num >= TowerOfPowerLevelGameInfo.difficultyByBossIndex.Length)
		{
			num = TowerOfPowerLevelGameInfo.difficultyByBossIndex.Length - 1;
		}
		Level.SetCurrentMode(TowerOfPowerLevelGameInfo.difficultyByBossIndex[num]);
	}

	private void RevivePlayer(PlayerId playerId)
	{
		PlayerStatsManager stats = PlayerManager.GetPlayer(playerId).stats;
		TowerOfPowerLevelGameInfo.PLAYER_STATS[(int)playerId].HP = 3;
		TowerOfPowerLevelGameInfo.PLAYER_STATS[(int)playerId].BonusHP = 3;
		stats.SetHealth(3);
	}

	private IEnumerator main_cr()
	{
		int turn = TowerOfPowerLevelGameInfo.CURRENT_TURN;
		List<Levels> allStage = TowerOfPowerLevelGameInfo.allStageSpaces;
		if (turn > 0)
		{
			showingScorecard = true;
			while (SceneLoader.CurrentlyLoading)
			{
				yield return null;
			}
			scorecard.gameObject.SetActive(value: true);
			while (!scorecard.done)
			{
				yield return null;
			}
			showingScorecard = false;
			scorecard.gameObject.SetActive(value: false);
			if (PlayerManager.GetPlayer(PlayerId.PlayerOne).IsDead && TowerOfPowerLevelGameInfo.IsTokenLeft(0))
			{
				TowerOfPowerLevelGameInfo.ReduceToken(0);
				RevivePlayer(PlayerId.PlayerOne);
			}
			if (PlayerManager.Multiplayer && PlayerManager.GetPlayer(PlayerId.PlayerTwo).IsDead && TowerOfPowerLevelGameInfo.IsTokenLeft(1))
			{
				TowerOfPowerLevelGameInfo.ReduceToken(1);
				RevivePlayer(PlayerId.PlayerTwo);
			}
		}
		if ((turn != 0 && turn % 3 == 0 && turn < 8) || allStage[turn] == Levels.Devil || debugForceSlotMachineEveryTurn || (turn == 1 && debugForceSlotMachineAfterOneFight))
		{
			if (!PlayerManager.GetPlayer(PlayerId.PlayerOne).IsDead)
			{
				ChangePlayersWeapon(PlayerId.PlayerOne);
				slotsDone[0] = false;
				StartCoroutine(play_slot_machine_cr(PlayerId.PlayerOne));
			}
			else
			{
				slotsDone[0] = true;
			}
			if (PlayerManager.Multiplayer && !PlayerManager.GetPlayer(PlayerId.PlayerTwo).IsDead)
			{
				slotsDone[1] = false;
				ChangePlayersWeapon(PlayerId.PlayerTwo);
				StartCoroutine(play_slot_machine_cr(PlayerId.PlayerTwo));
			}
			else
			{
				slotsDone[1] = true;
			}
		}
		else
		{
			StartCoroutine(go_to_next_level_cr());
		}
		yield return null;
	}

	private IEnumerator spin_slot_machine_cr(PlayerId playerId)
	{
		yield return null;
	}

	private IEnumerator stop_slot_machine_cr(PlayerId playerId)
	{
		while (true)
		{
			if (PauseManager.state == PauseManager.State.Paused)
			{
				waitForButtonRelease = 3;
				yield return null;
			}
			if (PlayerManager.GetPlayer(playerId).input.actions.GetButtonUp(13))
			{
				waitForButtonRelease = 0;
			}
			if (waitForButtonRelease == 0 && PlayerManager.GetPlayer(playerId).input.actions.GetButtonDown(13))
			{
				break;
			}
			if (waitForButtonRelease > 0)
			{
				waitForButtonRelease--;
			}
			yield return slowdown_slots_cr();
		}
		yield return null;
	}

	private IEnumerator slowdown_slots_cr()
	{
		yield return null;
	}

	private IEnumerator play_slot_machine_cr(PlayerId playerId)
	{
		yield return spin_slot_machine_cr(playerId);
		slotsConfirm[(int)playerId] = false;
		slotsCanSpinAgain[(int)playerId] = false;
		slotsAreSpinning[(int)playerId] = true;
		yield return stop_slot_machine_cr(playerId);
		slotsAreSpinning[(int)playerId] = false;
		slotsConfirm[(int)playerId] = true;
		bool playAgain = false;
		while (!PlayerManager.GetPlayerInput(playerId).GetButtonDown(13))
		{
			if (TowerOfPowerLevelGameInfo.PLAYER_STATS[(int)playerId].tokenCount > 0)
			{
				slotsCanSpinAgain[(int)playerId] = true;
				if (PlayerManager.GetPlayer(playerId).input.actions.GetButtonDown(7))
				{
					playAgain = true;
					TowerOfPowerLevelGameInfo.PLAYER_STATS[(int)playerId].tokenCount--;
					ChangePlayersWeapon(playerId);
					break;
				}
			}
			yield return null;
		}
		slotsConfirm[(int)playerId] = false;
		slotsCanSpinAgain[(int)playerId] = false;
		yield return null;
		if (playAgain)
		{
			yield return play_slot_machine_cr(playerId);
			yield break;
		}
		slotsDone[(int)playerId] = true;
		bool chaliceCharmEquipped = TowerOfPowerLevelGameInfo.PLAYER_STATS[(int)playerId].BaseCharm == Charm.charm_chalice;
		PlayerData.PlayerLoadouts.PlayerLoadout P1loadout = PlayerData.Data.Loadouts.GetPlayerLoadout(playerId);
		if (bonusHP[(int)playerId] > 0)
		{
			P1loadout.charm = ((!chaliceCharmEquipped) ? Charm.None : Charm.charm_chalice);
		}
		if (bonusToken[(int)playerId] > 0)
		{
			P1loadout.charm = ((!chaliceCharmEquipped) ? Charm.None : Charm.charm_chalice);
		}
		PlayerStatsManager playerStats = PlayerManager.GetPlayer(playerId).stats;
		int hp = playerStats.Health + bonusHP[(int)playerId];
		if (hp > 8)
		{
			hp = 8;
		}
		TowerOfPowerLevelGameInfo.PLAYER_STATS[(int)playerId].HP = hp;
		TowerOfPowerLevelGameInfo.PLAYER_STATS[(int)playerId].BonusHP = hp - 3;
		playerStats.SetHealth(hp);
		bonusHP[(int)playerId] = 0;
		TowerOfPowerLevelGameInfo.PLAYER_STATS[(int)playerId].tokenCount += bonusToken[(int)playerId];
		bonusToken[(int)playerId] = 0;
		if (slotsDone[0] && slotsDone[1])
		{
			yield return go_to_next_level_cr();
		}
	}

	public bool SlotsDone()
	{
		if (PlayerManager.Multiplayer)
		{
			return slotsDone[0] && slotsDone[1];
		}
		return slotsDone[0];
	}

	private IEnumerator go_to_next_level_cr()
	{
		while (SceneLoader.CurrentlyLoading)
		{
			yield return null;
		}
		while (true)
		{
			if (PauseManager.state == PauseManager.State.Paused)
			{
				waitForButtonRelease = 3;
				yield return null;
			}
			if (anyInput.GetButtonUp(CupheadButton.Accept))
			{
				waitForButtonRelease = 0;
			}
			if (waitForButtonRelease == 0 && anyInput.GetButtonDown(CupheadButton.Accept))
			{
				break;
			}
			if (waitForButtonRelease > 0)
			{
				waitForButtonRelease--;
			}
			yield return null;
		}
		int currentLevel = TowerOfPowerLevelGameInfo.CURRENT_TURN;
		if (TowerOfPowerLevelGameInfo.CURRENT_TURN < TowerOfPowerLevelGameInfo.allStageSpaces.Count)
		{
			if (TowerOfPowerLevelGameInfo.PLAYER_STATS[0] != null)
			{
				SetDifficulty();
				yield return StartCoroutine(startMiniBoss_cr(TowerOfPowerLevelGameInfo.allStageSpaces[currentLevel]));
			}
		}
		else
		{
			SceneLoader.LoadLastMap();
		}
	}
}
