using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData
{
	public struct LevelCoinIds
	{
		public Levels levelId;

		public string[][] coinIds;

		public LevelCoinIds(Levels level, string[][] coins)
		{
			levelId = level;
			coinIds = coins;
		}
	}

	public delegate void PlayerDataInitHandler(bool success);

	[Serializable]
	public class PlayerLoadouts
	{
		[Serializable]
		public class PlayerLoadout
		{
			public Weapon primaryWeapon;

			public Weapon secondaryWeapon;

			public Super super;

			public Charm charm;

			public bool HasEquippedSecondaryRegularWeapon { get; set; }

			public bool HasEquippedSecondarySHMUPWeapon { get; set; }

			public bool MustNotifySwitchRegularWeapon { get; set; }

			public bool MustNotifySwitchSHMUPWeapon { get; set; }

			public PlayerLoadout()
			{
				primaryWeapon = Weapon.level_weapon_peashot;
				secondaryWeapon = Weapon.None;
				super = Super.None;
				charm = Charm.None;
			}
		}

		public PlayerLoadout playerOne;

		public PlayerLoadout playerTwo;

		public PlayerLoadouts()
		{
			playerOne = new PlayerLoadout();
			playerTwo = new PlayerLoadout();
		}

		public PlayerLoadouts(PlayerLoadout playerOne, PlayerLoadout playerTwo)
		{
			this.playerOne = playerOne;
			this.playerTwo = playerTwo;
		}

		public PlayerLoadout GetPlayerLoadout(PlayerId player)
		{
			return player switch
			{
				PlayerId.PlayerOne => playerOne, 
				PlayerId.PlayerTwo => playerTwo, 
				_ => null, 
			};
		}
	}

	[Serializable]
	public class PlayerInventories
	{
		public int dummy;

		public PlayerInventory playerOne = new PlayerInventory();

		public PlayerInventory playerTwo = new PlayerInventory();

		public PlayerInventory GetPlayer(PlayerId player)
		{
			return player switch
			{
				PlayerId.PlayerOne => playerOne, 
				PlayerId.PlayerTwo => playerTwo, 
				_ => null, 
			};
		}
	}

	[Serializable]
	public class PlayerInventory
	{
		public const int STARTING_MONEY = 0;

		public int money;

		public bool newPurchase;

		public List<Weapon> _weapons;

		public List<Super> _supers;

		public List<Charm> _charms;

		public PlayerInventory()
		{
			money = 0;
			_weapons = new List<Weapon>();
			_supers = new List<Super>();
			_charms = new List<Charm>();
			_weapons.Add(Weapon.level_weapon_peashot);
			_weapons.Add(Weapon.plane_weapon_peashot);
		}

		public bool IsUnlocked(Weapon weapon)
		{
			return _weapons.Contains(weapon);
		}

		public bool IsUnlocked(Super super)
		{
			return _supers.Contains(super);
		}

		public bool IsUnlocked(Charm charm)
		{
			return _charms.Contains(charm);
		}

		public bool Buy(Weapon value)
		{
			if (IsUnlocked(value))
			{
				return false;
			}
			if (money < WeaponProperties.GetValue(value))
			{
				return false;
			}
			money -= WeaponProperties.GetValue(value);
			_weapons.Add(value);
			newPurchase = true;
			return true;
		}

		public bool Buy(Super value)
		{
			if (IsUnlocked(value))
			{
				return false;
			}
			if (money < WeaponProperties.GetValue(value))
			{
				return false;
			}
			money -= WeaponProperties.GetValue(value);
			_supers.Add(value);
			newPurchase = true;
			return true;
		}

		public bool Buy(Charm value)
		{
			if (IsUnlocked(value))
			{
				return false;
			}
			if (money < WeaponProperties.GetValue(value))
			{
				return false;
			}
			money -= WeaponProperties.GetValue(value);
			_charms.Add(value);
			newPurchase = true;
			return true;
		}
	}

	[Serializable]
	public class PlayerCoinManager
	{
		[Serializable]
		public class LevelAndCoins
		{
			public Levels level;

			public bool Coin1Collected;

			public bool Coin2Collected;

			public bool Coin3Collected;

			public bool Coin4Collected;

			public bool Coin5Collected;
		}

		public int dummy;

		public List<PlayerCoinProperties> coins = new List<PlayerCoinProperties>();

		public bool hasMigratedCoins;

		public List<LevelAndCoins> LevelsAndCoins = new List<LevelAndCoins>();

		public PlayerCoinManager()
		{
			LevelsAndCoins = new List<LevelAndCoins>();
			foreach (Levels value in Enum.GetValues(typeof(Levels)))
			{
				LevelAndCoins item = new LevelAndCoins
				{
					level = value
				};
				LevelsAndCoins.Add(item);
			}
		}

		public bool GetCoinCollected(LevelCoin coin)
		{
			return GetCoinCollected(coin.GlobalID);
		}

		public bool GetCoinCollected(string coinID)
		{
			if (ContainsCoin(coinID))
			{
				return GetCoin(coinID).collected;
			}
			return false;
		}

		public int NumCoinsCollected()
		{
			int num = 0;
			foreach (PlayerCoinProperties coin in coins)
			{
				if (coin.collected)
				{
					num++;
				}
			}
			return num;
		}

		public int NumCoinsCollected(bool DLC)
		{
			int num = 0;
			foreach (PlayerCoinProperties coin in coins)
			{
				if (coin.collected && IsDLCCoin(coin.coinID) == DLC)
				{
					num++;
				}
			}
			return num;
		}

		public void SetCoinValue(LevelCoin coin, bool collected, PlayerId player)
		{
			SetCoinValue(coin.GlobalID, collected, player);
		}

		public void SetCoinValue(string coinID, bool collected, PlayerId player)
		{
			if (ContainsCoin(coinID))
			{
				PlayerCoinProperties coin = GetCoin(coinID);
				coin.collected = collected;
				coin.player = player;
			}
			else
			{
				PlayerCoinProperties playerCoinProperties = new PlayerCoinProperties(coinID);
				playerCoinProperties.collected = collected;
				AddCoin(playerCoinProperties);
			}
		}

		private PlayerCoinProperties GetCoin(LevelCoin coin)
		{
			return GetCoin(coin.GlobalID);
		}

		private PlayerCoinProperties GetCoin(string coinID)
		{
			for (int i = 0; i < coins.Count; i++)
			{
				if (coins[i].coinID == coinID)
				{
					return coins[i];
				}
			}
			return null;
		}

		private void AddCoin(LevelCoin coin)
		{
			AddCoin(coin.GlobalID);
		}

		private void AddCoin(string coinID)
		{
			if (!ContainsCoin(coinID))
			{
				coins.Add(new PlayerCoinProperties(coinID));
			}
			RegisterCoin(coinID);
		}

		private void AddCoin(PlayerCoinProperties coin)
		{
			if (!ContainsCoin(coin.coinID))
			{
				coins.Add(coin);
			}
			RegisterCoin(coin.coinID);
		}

		private void RegisterCoin(string coinID)
		{
			PlatformingLevel platformingLevel = Level.Current as PlatformingLevel;
			if ((bool)platformingLevel)
			{
				List<LevelAndCoins> levelsAndCoins = LevelsAndCoins;
				int num = -1;
				for (int i = 0; i < levelsAndCoins.Count; i++)
				{
					if (levelsAndCoins[i].level == platformingLevel.CurrentLevel)
					{
						num = i;
					}
				}
				if (num >= 0)
				{
					for (int j = 0; j < platformingLevel.LevelCoinsIDs.Count; j++)
					{
						if (platformingLevel.LevelCoinsIDs[j].CoinID == coinID)
						{
							switch (j)
							{
							case 0:
								levelsAndCoins[num].Coin1Collected = true;
								break;
							case 1:
								levelsAndCoins[num].Coin2Collected = true;
								break;
							case 2:
								levelsAndCoins[num].Coin3Collected = true;
								break;
							case 3:
								levelsAndCoins[num].Coin4Collected = true;
								break;
							case 4:
								levelsAndCoins[num].Coin5Collected = true;
								break;
							}
							break;
						}
					}
				}
			}
			else if (Map.Current != null)
			{
				List<LevelAndCoins> levelsAndCoins2 = Data.coinManager.LevelsAndCoins;
				int num2 = -1;
				for (int k = 0; k < levelsAndCoins2.Count; k++)
				{
					if (levelsAndCoins2[k].level == Map.Current.level)
					{
						num2 = k;
					}
				}
				if (num2 >= 0)
				{
					for (int l = 0; l < Map.Current.LevelCoinsIDs.Count; l++)
					{
						if (Map.Current.LevelCoinsIDs[l].CoinID == coinID)
						{
							switch (l)
							{
							case 0:
								levelsAndCoins2[num2].Coin1Collected = true;
								break;
							case 1:
								levelsAndCoins2[num2].Coin2Collected = true;
								break;
							case 2:
								levelsAndCoins2[num2].Coin3Collected = true;
								break;
							case 3:
								levelsAndCoins2[num2].Coin4Collected = true;
								break;
							case 4:
								levelsAndCoins2[num2].Coin5Collected = true;
								break;
							}
							break;
						}
					}
				}
			}
			bool flag = true;
			Levels[] platformingLevels = Level.platformingLevels;
			foreach (Levels level in platformingLevels)
			{
				if (Data.GetNumCoinsCollectedInLevel(level) < 5)
				{
					flag = false;
				}
			}
			if (flag)
			{
				OnlineManager.Instance.Interface.UnlockAchievement(PlayerId.Any, "FoundAllLevelMoney");
			}
			if (Data.NumCoinsCollectedMainGame >= 40)
			{
				OnlineManager.Instance.Interface.UnlockAchievement(PlayerId.Any, "FoundAllMoney");
			}
		}

		private bool ContainsCoin(LevelCoin coin)
		{
			return ContainsCoin(coin.GlobalID);
		}

		private bool ContainsCoin(string coinID)
		{
			for (int i = 0; i < coins.Count; i++)
			{
				if (coins[i].coinID == coinID)
				{
					return true;
				}
			}
			return false;
		}

		private bool IsDLCCoin(LevelCoin coin)
		{
			return IsDLCCoin(coin.GlobalID);
		}

		private bool IsDLCCoin(string coinID)
		{
			int result;
			switch (coinID)
			{
			default:
				result = ((coinID == "3367b9b0-da35-4c81-a895-2720862b5b1b") ? 1 : 0);
				break;
			case "619e92f1-e0fd-4f6e-9c2d-5ce5dbaf393f":
			case "scene_level_chalice_tutorial::Level_Coin :: 578c0218-df9e-4cdd-932a-a1277b5b7129":
			case "a37b3d37-a32e-4b88-a583-34489496494d":
			case "25f15554-d229-4330-96cc-ac8a13c18ea0":
			case "eacf4228-e200-4839-9d79-3439cfcc5824":
			case "47f7edb1-b5c5-4afb-9acb-a46f5e6df557":
			case "3826615a-498b-4158-af7b-0d01acbc18c8":
			case "d52b1cc6-414c-4a7c-9f8a-250316566d58":
			case "fc2c48cd-5dec-472a-ae18-dccfc94232c6":
			case "16732bc8-7230-467a-a9ac-ff9c62ab7657":
			case "e0c6e8bc-0c56-4e52-a9a1-c53887f5ca4c":
			case "19090606-09e8-4e56-92ac-e08200926b94":
			case "39bfe6d8-0dbc-4886-9998-52c67b57969e":
			case "7f3422f5-6650-497f-9c35-9735b64100d6":
			case "9970ad6a-560a-4ae3-9d15-a6b636b67024":
				result = 1;
				break;
			}
			return (byte)result != 0;
		}
	}

	[Serializable]
	public class PlayerCoinProperties
	{
		public string coinID = string.Empty;

		public bool collected;

		public PlayerId player = PlayerId.None;

		public PlayerCoinProperties()
		{
		}

		public PlayerCoinProperties(LevelCoin coin)
		{
			coinID = coin.GlobalID;
		}

		public PlayerCoinProperties(string coinID)
		{
			this.coinID = coinID;
		}
	}

	[Serializable]
	public class MapData
	{
		public enum EntryMethod
		{
			None,
			DiceHouseLeft,
			DiceHouseRight,
			Boatman
		}

		public Scenes mapId;

		public bool sessionStarted;

		public bool hasVisitedDieHouse;

		public bool hasKingDiceDisappeared;

		public Vector3 playerOnePosition = Vector3.zero;

		public Vector3 playerTwoPosition = Vector3.zero;

		[NonSerialized]
		public EntryMethod enteringFrom;
	}

	[Serializable]
	public class MapDataManager
	{
		public Scenes currentMap = Scenes.scene_map_world_1;

		public List<MapData> mapData;

		public MapDataManager()
		{
			mapData = new List<MapData>();
		}

		public MapData GetCurrentMapData()
		{
			return GetMapData(currentMap);
		}

		public MapData GetMapData(Scenes map)
		{
			for (int i = 0; i < this.mapData.Count; i++)
			{
				if (this.mapData[i].mapId == map)
				{
					return this.mapData[i];
				}
			}
			MapData mapData = new MapData();
			mapData.mapId = map;
			this.mapData.Add(mapData);
			return mapData;
		}
	}

	[Serializable]
	public class PlayerLevelDataManager
	{
		public int dummy;

		public List<PlayerLevelDataObject> levelObjects;

		public PlayerLevelDataManager()
		{
			levelObjects = new List<PlayerLevelDataObject>();
			Levels[] values = EnumUtils.GetValues<Levels>();
			foreach (Levels levels in values)
			{
				PlayerLevelDataObject item = new PlayerLevelDataObject(levels)
				{
					levelID = levels
				};
				levelObjects.Add(item);
			}
		}

		public PlayerLevelDataObject GetLevelData(Levels levelID)
		{
			for (int i = 0; i < levelObjects.Count; i++)
			{
				if (levelObjects[i].levelID == levelID)
				{
					return levelObjects[i];
				}
			}
			PlayerLevelDataObject playerLevelDataObject = new PlayerLevelDataObject(levelID);
			levelObjects.Add(playerLevelDataObject);
			return playerLevelDataObject;
		}
	}

	[Serializable]
	public class PlayerLevelDataObject
	{
		public Levels levelID;

		public bool completed;

		public bool completedAsChaliceP1;

		public bool completedAsChaliceP2;

		public bool played;

		public LevelScoringData.Grade grade;

		public Level.Mode difficultyBeaten;

		public float bestTime = float.MaxValue;

		public bool curseCharmP1;

		public bool curseCharmP2;

		public int bgmPlayListCurrent;

		public PlayerLevelDataObject(Levels id)
		{
			levelID = id;
		}
	}

	[Serializable]
	public class PlayerStats
	{
		public int dummy;

		public PlayerStat playerOne = new PlayerStat();

		public PlayerStat playerTwo = new PlayerStat();

		public PlayerStat GetPlayer(PlayerId player)
		{
			return player switch
			{
				PlayerId.PlayerOne => playerOne, 
				PlayerId.PlayerTwo => playerTwo, 
				_ => null, 
			};
		}
	}

	[Serializable]
	public class PlayerStat
	{
		public int numDeaths;

		public int numParriesInRow;

		public PlayerStat()
		{
			numDeaths = 0;
			numParriesInRow = 0;
		}

		public int DeathCount()
		{
			return numDeaths;
		}

		public void Die()
		{
			numDeaths++;
		}
	}

	public static readonly LevelCoinIds[] platformingCoinIDs = new LevelCoinIds[6]
	{
		new LevelCoinIds(Levels.Platforming_Level_1_1, new string[5][]
		{
			new string[1] { "scene_level_platforming_1_1F::Level_Coin :: 5fd52d1b-a7f2-43a6-80e2-cb170cbc7d4d" },
			new string[1] { "scene_level_platforming_1_1F::Level_Coin :: 63c021bf-52f0-41de-bedf-c77117d244cc" },
			new string[1] { "scene_level_platforming_1_1F::Level_Coin :: 245037a6-1fa2-4167-a631-0723abff8138" },
			new string[1] { "scene_level_platforming_1_1F::Level_Coin :: eaefb009-c117-4b9a-96c1-7abc5558d213" },
			new string[1] { "scene_level_platforming_1_1F::Level_Coin :: 5526f7bc-a902-4c13-9e7a-1632a5abe378" }
		}),
		new LevelCoinIds(Levels.Platforming_Level_1_2, new string[5][]
		{
			new string[1] { "scene_level_platforming_1_2F::Level_Coin :: 323989de-349e-4740-a764-dbc12217a27c" },
			new string[1] { "scene_level_platforming_1_2F::Level_Coin :: 55a46261-b14c-4065-9ada-18524eaed9f3" },
			new string[1] { "scene_level_platforming_1_2F::Level_Coin :: da0983f6-62d4-4ace-81f2-cad7181d5fe9" },
			new string[1] { "scene_level_platforming_1_2F::Level_Coin :: 7088ec51-4792-49c0-ab2c-c45ec9deb9f0" },
			new string[1] { "scene_level_platforming_1_2F::Level_Coin :: e02954c1-ff76-4ba4-849f-90aae53a7787" }
		}),
		new LevelCoinIds(Levels.Platforming_Level_2_1, new string[5][]
		{
			new string[1] { "scene_level_platforming_2_1F::Level_Coin :: 24ef654a-a65b-4a1c-b5e5-c3c64e250646" },
			new string[1] { "scene_level_platforming_2_1F::Level_Coin :: b8d96f03-d264-4a61-9ab9-07de34f660aa" },
			new string[1] { "scene_level_platforming_2_1F::Level_Coin :: 383d9b3b-c280-4825-a6b3-1a21fe42d0ac" },
			new string[1] { "scene_level_platforming_2_1F::Level_Coin :: f1b99bcd-0fa8-4aac-9a54-f310e173ddf9" },
			new string[1] { "scene_level_platforming_2_1F::Level_Coin :: c763ef21-2ee7-491c-a143-b906856fed6c" }
		}),
		new LevelCoinIds(Levels.Platforming_Level_2_2, new string[5][]
		{
			new string[8] { "scene_level_platforming_2_2F::Level_Coin :: 9025a0e9-fff1-4f14-93d1-1930eef27405", "scene_level_platforming_2_2F::Level_Coin :: abbfb110-69d1-4948-9c70-223c6425c6f5", "scene_level_platforming_2_2F::Level_Coin :: 159497a2-3ded-4c0e-8852-4f6c41046df7", "scene_level_platforming_2_2F::Level_Coin :: 22bd722b-bf79-438b-92b0-56c638ae7114", "scene_level_platforming_2_2F::Level_Coin :: 84c8547b-b9b8-4fe9-9b0f-75980a3f5454", "scene_level_platforming_2_2F::Level_Coin :: d8d1b996-c4ef-4586-9c69-a3f18ebaeece", "scene_level_platforming_2_2F::Level_Coin :: 79695e06-f5c3-4826-96e8-5318399cdaf0", "scene_level_platforming_2_2F::Level_Coin :: 3aa60c71-a8c9-4b44-b53e-f954c9c70b29" },
			new string[1] { "scene_level_platforming_2_2F::Level_Coin :: 284ea6f9-5db4-4f80-b0e5-1d9513a8acb7" },
			new string[1] { "scene_level_platforming_2_2F::Level_Coin :: 43a8fc82-b8b8-4a92-b56f-c3e718b46b2c" },
			new string[1] { "scene_level_platforming_2_2F::Level_Coin :: bf86d025-4524-4ce8-ba07-540ef3f61ed8" },
			new string[1] { "scene_level_platforming_2_2F::Level_Coin :: a7c0e2b9-9560-4ed7-a3a4-428365222cb9" }
		}),
		new LevelCoinIds(Levels.Platforming_Level_3_1, new string[5][]
		{
			new string[8] { "scene_level_platforming_3_1F::Level_Coin :: 26ba2e1d-4b0a-4964-ba4d-f58655ef47db", "scene_level_platforming_3_1F::Level_Coin :: 8d1cd543-fa2f-41d6-9e50-d8ea356c9d26", "scene_level_platforming_3_1F::Level_Coin :: 90912b91-c396-429a-b061-0af90b666a0f", "scene_level_platforming_3_1F::Level_Coin :: 7a4de11e-fed9-479a-8ace-57bb7a00baa7", "scene_level_platforming_3_1F::Level_Coin :: eabb3294-336c-4615-8975-210343a039b5", "scene_level_platforming_3_1F::Level_Coin :: 6c032ae2-7bb9-4236-abc4-c27177201615", "scene_level_platforming_3_1F::Level_Coin :: 6fcd5ca7-9953-4343-a7f4-55d3fbc7d287", "scene_level_platforming_3_1F::Level_Coin :: e280e5f3-9fa1-4587-9139-84c127413e7a" },
			new string[8] { "scene_level_platforming_3_1F::Level_Coin :: 0f13fbe6-1041-445f-97ed-1bbe2cb0339e", "scene_level_platforming_3_1F::Level_Coin :: 9aa051bf-5ec9-47b2-93f5-09f1495e78f2", "scene_level_platforming_3_1F::Level_Coin :: 4f4c2a23-244a-484b-84c9-ca5c6fc4e6bb", "scene_level_platforming_3_1F::Level_Coin :: 5c1e1ce4-055a-4ed6-8f5a-c667dbcac5af", "scene_level_platforming_3_1F::Level_Coin :: c1b74075-ae62-45ab-8d60-08286a35936f", "scene_level_platforming_3_1F::Level_Coin :: 5e1c290f-e2a4-4410-a52c-ba41ce7e56c5", "scene_level_platforming_3_1F::Level_Coin :: b18f581d-67b3-4020-b031-3a5bb62a9fa1", "scene_level_platforming_3_1F::Level_Coin :: 7b9e2b26-9132-4558-922b-ea400d4fdb0f" },
			new string[8] { "scene_level_platforming_3_1F::Level_Coin :: 0086a9b3-87b8-4406-b97b-b94a1fd60bb0", "scene_level_platforming_3_1F::Level_Coin :: 273f231f-11d1-42db-888d-7d78696b934b", "scene_level_platforming_3_1F::Level_Coin :: cab629f0-54fa-43d3-8573-5d82db28e5c9", "scene_level_platforming_3_1F::Level_Coin :: b9ffa14a-984d-426b-8a96-7e71c58d8542", "scene_level_platforming_3_1F::Level_Coin :: b0f7e7a4-16a8-4a58-9abc-51f2aac1aac3", "scene_level_platforming_3_1F::Level_Coin :: 2b7cac59-e975-47f2-bf74-e49cb612266a", "scene_level_platforming_3_1F::Level_Coin :: e74bfad7-8657-4d6b-b853-9fef027e8600", "scene_level_platforming_3_1F::Level_Coin :: 6153c3cb-493f-465e-b6b2-dcddd5c0c50e" },
			new string[8] { "scene_level_platforming_3_1F::Level_Coin :: 0a6fbbe4-5c13-4b17-9b58-91e7bbdacde4", "scene_level_platforming_3_1F::Level_Coin :: f72bdba8-cc0b-4d17-a83f-9892c3507b1c", "scene_level_platforming_3_1F::Level_Coin :: 5eaabcfd-0101-4ff5-92f4-a65d885be960", "scene_level_platforming_3_1F::Level_Coin :: 036a2830-7c80-443b-b9f6-1576dbf5cb33", "scene_level_platforming_3_1F::Level_Coin :: 1c782442-7e15-4a48-a66b-19c5a862e61e", "scene_level_platforming_3_1F::Level_Coin :: 01b6dc66-dd9a-4a6f-ac4d-e93a173395ef", "scene_level_platforming_3_1F::Level_Coin :: 4ca8faee-fb21-4f5a-b521-4deb89d853c3", "scene_level_platforming_3_1F::Level_Coin :: 5bfd4fdf-546c-4751-b2a7-eb99c7cdd2f4" },
			new string[8] { "scene_level_platforming_3_1F::Level_Coin :: beb664ad-5577-4055-9164-b1b2f77430f3", "scene_level_platforming_3_1F::Level_Coin :: 2ffc0eef-d922-4825-bfb4-7377c16e197d", "scene_level_platforming_3_1F::Level_Coin :: 0c636a66-f96c-4046-9ccd-12897ab77649", "scene_level_platforming_3_1F::Level_Coin :: 267b5e81-84e6-4297-848c-bea5549b1690", "scene_level_platforming_3_1F::Level_Coin :: 76e64c16-b4d3-472f-85ae-d1dbd5c055e3", "scene_level_platforming_3_1F::Level_Coin :: 05b62218-8f30-4d74-bab4-7d27f4e0ab90", "scene_level_platforming_3_1F::Level_Coin :: 6222ae58-b0c8-44e8-81f6-417f00cc1be1", "scene_level_platforming_3_1F::Level_Coin :: 54c21221-4a03-4437-a2bd-a5972c3e2bfc" }
		}),
		new LevelCoinIds(Levels.Platforming_Level_3_2, new string[5][]
		{
			new string[1] { "scene_level_platforming_3_2F::Level_Coin :: 5da68904-6505-4841-9684-71d2931c1bd6" },
			new string[1] { "scene_level_platforming_3_2F::Level_Coin :: 999c9b0d-d554-471d-ad96-ee6d57ccfd19" },
			new string[1] { "scene_level_platforming_3_2F::Level_Coin :: cf0a7cae-d8d9-4be0-9502-8b8544606e04" },
			new string[1] { "scene_level_platforming_3_2F::Level_Coin :: e671db16-cf6e-421c-937c-2b6f5c7ad0e7" },
			new string[1] { "scene_level_platforming_3_2F::Level_Coin :: 084a7b75-e752-452f-8710-687db1e165fe" }
		})
	};

	private const string KEY = "cuphead_player_data_v1_slot_";

	private static readonly string[] SAVE_FILE_KEYS = new string[3] { "cuphead_player_data_v1_slot_0", "cuphead_player_data_v1_slot_1", "cuphead_player_data_v1_slot_2" };

	public static readonly Weapon[] WeaponsDLC = new Weapon[3]
	{
		Weapon.level_weapon_wide_shot,
		Weapon.level_weapon_upshot,
		Weapon.level_weapon_crackshot
	};

	public static readonly Charm[] CharmsDLC = new Charm[3]
	{
		Charm.charm_chalice,
		Charm.charm_healer,
		Charm.charm_curse
	};

	private static string emptyDialoguerState = string.Empty;

	private static int _CurrentSaveFileIndex = 0;

	private static bool _initialized = false;

	public static bool inGame = false;

	private static PlayerData[] _saveFiles;

	private static PlayerDataInitHandler _playerDatatInitHandler;

	public bool isPlayer1Mugman;

	public bool hasMadeFirstPurchase;

	public bool hasBeatenAnyBossOnEasy;

	public bool hasBeatenAnyDLCBossOnEasy;

	public bool hasUnlockedFirstSuper;

	public bool shouldShowShopkeepTooltip;

	public bool shouldShowTurtleTooltip;

	public bool shouldShowCanteenTooltip;

	public bool shouldShowForkTooltip;

	public bool shouldShowKineDiceTooltip;

	public bool shouldShowMausoleumTooltip;

	public bool hasUnlockedBoatman;

	public bool shouldShowBoatmanTooltip;

	public bool shouldShowChaliceTooltip;

	public bool hasTalkedToChaliceFan;

	public int[] curseCharmPuzzleOrder;

	public bool curseCharmPuzzleComplete;

	public MapCastleZones.Zone currentChessBossZone;

	public List<MapCastleZones.Zone> usedChessBossZones = new List<MapCastleZones.Zone>();

	public int chessBossAttemptCounter;

	public bool djimmiActivatedCountedWish;

	public bool djimmiActivatedInfiniteWishBaseGame;

	public bool djimmiActivatedInfiniteWishDLC;

	public int djimmiWishes = 3;

	public bool djimmiFreed;

	public bool djimmiFreedDLC;

	public int dummy;

	[SerializeField]
	private PlayerLoadouts loadouts = new PlayerLoadouts();

	[SerializeField]
	private bool _isHardModeAvailable;

	[SerializeField]
	private bool _isHardModeAvailableDLC;

	[SerializeField]
	private bool _isTutorialCompleted;

	[SerializeField]
	private bool _isFlyingTutorialCompleted;

	[SerializeField]
	private bool _isChaliceTutorialCompleted;

	[SerializeField]
	private PlayerInventories inventories = new PlayerInventories();

	public string dialoguerState;

	[SerializeField]
	public PlayerCoinManager coinManager = new PlayerCoinManager();

	private PlayerCoinManager levelCoinManager = new PlayerCoinManager();

	public bool unlockedBlackAndWhite;

	public bool unlocked2Strip;

	public bool unlockedChaliceRecolor;

	public bool vintageAudioEnabled;

	public bool pianoAudioEnabled;

	public BlurGamma.Filter filter;

	[SerializeField]
	private MapDataManager mapDataManager = new MapDataManager();

	[SerializeField]
	private PlayerLevelDataManager levelDataManager = new PlayerLevelDataManager();

	[SerializeField]
	private PlayerStats statictics = new PlayerStats();

	public static int CurrentSaveFileIndex
	{
		get
		{
			return Mathf.Clamp(_CurrentSaveFileIndex, 0, SAVE_FILE_KEYS.Length - 1);
		}
		set
		{
			_CurrentSaveFileIndex = Mathf.Clamp(value, 0, SAVE_FILE_KEYS.Length - 1);
			Data.LoadDialogueVariables();
		}
	}

	public static bool Initialized
	{
		get
		{
			return _initialized;
		}
		private set
		{
			_initialized = value;
		}
	}

	public static PlayerData Data => GetDataForSlot(CurrentSaveFileIndex);

	public PlayerLoadouts Loadouts => loadouts;

	public bool IsHardModeAvailable
	{
		get
		{
			return _isHardModeAvailable;
		}
		set
		{
			_isHardModeAvailable = value;
		}
	}

	public bool IsHardModeAvailableDLC
	{
		get
		{
			return _isHardModeAvailableDLC;
		}
		set
		{
			_isHardModeAvailableDLC = value;
		}
	}

	public bool IsTutorialCompleted
	{
		get
		{
			return _isTutorialCompleted;
		}
		set
		{
			_isTutorialCompleted = value;
		}
	}

	public bool IsFlyingTutorialCompleted
	{
		get
		{
			return _isFlyingTutorialCompleted;
		}
		set
		{
			_isFlyingTutorialCompleted = value;
		}
	}

	public bool IsChaliceTutorialCompleted
	{
		get
		{
			return _isChaliceTutorialCompleted;
		}
		set
		{
			_isChaliceTutorialCompleted = value;
		}
	}

	public int NumCoinsCollected => coinManager.NumCoinsCollected();

	public int NumCoinsCollectedMainGame => coinManager.NumCoinsCollected(DLC: false);

	public MapData CurrentMapData => mapDataManager.GetCurrentMapData();

	public Scenes CurrentMap
	{
		get
		{
			return mapDataManager.currentMap;
		}
		set
		{
			mapDataManager.currentMap = value;
		}
	}

	public PlayerData()
	{
		if (string.IsNullOrEmpty(emptyDialoguerState))
		{
			Dialoguer.Initialize();
			emptyDialoguerState = Dialoguer.GetGlobalVariablesState();
		}
		dialoguerState = emptyDialoguerState;
	}

	public static PlayerData GetDataForSlot(int slot)
	{
		if (_saveFiles == null || _saveFiles.Length != SAVE_FILE_KEYS.Length)
		{
			_saveFiles = new PlayerData[SAVE_FILE_KEYS.Length];
			for (int i = 0; i < SAVE_FILE_KEYS.Length; i++)
			{
				_saveFiles[i] = new PlayerData();
			}
		}
		if (_saveFiles[slot].curseCharmPuzzleOrder == null || _saveFiles[slot].curseCharmPuzzleOrder.Length == 0)
		{
			_saveFiles[slot].CreateCursePuzzleVariables();
		}
		return _saveFiles[slot];
	}

	private void CreateCursePuzzleVariables()
	{
		List<int> list = new List<int>();
		list.Add(0);
		list.Add(1);
		list.Add(2);
		list.Add(3);
		list.Add(4);
		list.Add(5);
		list.Add(6);
		list.Add(7);
		List<int> list2 = list;
		int num = 0;
		curseCharmPuzzleOrder = new int[3];
		for (int i = 0; i < curseCharmPuzzleOrder.Length; i++)
		{
			num = UnityEngine.Random.Range(0, list2.Count);
			curseCharmPuzzleOrder[i] = list2[num];
			list2.Remove(list2[num]);
		}
	}

	public static void ClearSlot(int slot)
	{
		if (_saveFiles != null && _saveFiles.Length == SAVE_FILE_KEYS.Length)
		{
			ResetDialoguer();
			_saveFiles[slot] = new PlayerData();
			Save(slot);
		}
	}

	public static void Init(PlayerDataInitHandler handler)
	{
		_saveFiles = new PlayerData[SAVE_FILE_KEYS.Length];
		for (int i = 0; i < SAVE_FILE_KEYS.Length; i++)
		{
			_saveFiles[i] = new PlayerData();
		}
		_playerDatatInitHandler = handler;
		OnlineManager.Instance.Interface.InitializeCloudStorage(PlayerId.PlayerOne, OnCloudStorageInitialized);
	}

	private void LoadDialogueVariables()
	{
		Dialoguer.Initialize();
		Dialoguer.EndDialogue();
		Dialoguer.SetGlobalVariablesState(dialoguerState);
	}

	private static void OnCloudStorageInitialized(bool success)
	{
		if (!success)
		{
			_playerDatatInitHandler(success: false);
		}
		else
		{
			OnlineManager.Instance.Interface.LoadCloudData(SAVE_FILE_KEYS, OnLoaded);
		}
	}

	private static void OnLoaded(string[] data, CloudLoadResult result)
	{
		switch (result)
		{
		case CloudLoadResult.Failed:
			Debug.LogError("[PlayerData] LOAD FAILED");
			OnlineManager.Instance.Interface.LoadCloudData(SAVE_FILE_KEYS, OnLoaded);
			return;
		case CloudLoadResult.NoData:
			Debug.LogError("[PlayerData] No data. Saving default data to cloud");
			SaveAll();
			return;
		}
		bool flag = false;
		for (int i = 0; i < SAVE_FILE_KEYS.Length; i++)
		{
			if (data[i] == null)
			{
				continue;
			}
			PlayerData playerData = null;
			try
			{
				playerData = JsonUtility.FromJson<PlayerData>(data[i]);
				if (playerData != null && !playerData.coinManager.hasMigratedCoins)
				{
					playerData = Migrate(playerData);
					flag = true;
				}
			}
			catch (ArgumentException ex)
			{
				Debug.LogError("Unable to parse player data. " + ex.StackTrace);
			}
			if (playerData == null)
			{
				Debug.LogError("[PlayerData] Data could not be unserialized for key: " + SAVE_FILE_KEYS[i]);
			}
			else
			{
				_saveFiles[i] = playerData;
			}
		}
		Initialized = true;
		if (flag)
		{
			SaveAll();
		}
		if (_playerDatatInitHandler != null)
		{
			_playerDatatInitHandler(success: true);
			_playerDatatInitHandler = null;
		}
	}

	public static PlayerData Migrate(PlayerData playerData)
	{
		for (int i = 0; i < playerData.coinManager.LevelsAndCoins.Count; i++)
		{
			PlayerCoinManager.LevelAndCoins levelAndCoins = new PlayerCoinManager.LevelAndCoins();
			levelAndCoins.level = playerData.coinManager.LevelsAndCoins[i].level;
			playerData.coinManager.LevelsAndCoins[i] = levelAndCoins;
		}
		bool flag = false;
		for (int j = 0; j < playerData.coinManager.coins.Count; j++)
		{
			string coinID = playerData.coinManager.coins[j].coinID;
			flag = false;
			for (int k = 0; k < platformingCoinIDs.Length; k++)
			{
				List<PlayerCoinManager.LevelAndCoins> levelsAndCoins = playerData.coinManager.LevelsAndCoins;
				int index = -1;
				for (int l = 0; l < levelsAndCoins.Count; l++)
				{
					if (levelsAndCoins[l].level == platformingCoinIDs[k].levelId)
					{
						index = l;
					}
				}
				for (int m = 0; m < platformingCoinIDs[k].coinIds.Length; m++)
				{
					string coinID2 = platformingCoinIDs[k].coinIds[m][0];
					for (int n = 0; n < platformingCoinIDs[k].coinIds[m].Length; n++)
					{
						if (coinID == platformingCoinIDs[k].coinIds[m][n])
						{
							playerData.coinManager.coins[j].coinID = coinID2;
							flag = true;
							switch (m)
							{
							case 0:
								levelsAndCoins[index].Coin1Collected = true;
								break;
							case 1:
								levelsAndCoins[index].Coin2Collected = true;
								break;
							case 2:
								levelsAndCoins[index].Coin3Collected = true;
								break;
							case 3:
								levelsAndCoins[index].Coin4Collected = true;
								break;
							case 4:
								levelsAndCoins[index].Coin5Collected = true;
								break;
							}
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		playerData.coinManager.hasMigratedCoins = true;
		return playerData;
	}

	private static string GetSaveFileKey(int fileIndex)
	{
		return SAVE_FILE_KEYS[fileIndex];
	}

	private static void Save(int fileIndex)
	{
		_saveFiles[fileIndex].dialoguerState = Dialoguer.GetGlobalVariablesState();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary[SAVE_FILE_KEYS[fileIndex]] = JsonUtility.ToJson(_saveFiles[fileIndex]);
		OnlineManager.Instance.Interface.SaveCloudData(dictionary, OnSaved);
	}

	private static void SaveAll()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		for (int i = 0; i < SAVE_FILE_KEYS.Length; i++)
		{
			dictionary[SAVE_FILE_KEYS[i]] = JsonUtility.ToJson(_saveFiles[i]);
		}
		OnlineManager.Instance.Interface.SaveCloudData(dictionary, OnSavedAll);
	}

	private static void OnSaved(bool success)
	{
		if (!success)
		{
			Debug.LogError("[PlayerData] SAVE FAILED. Retrying...");
			Save(CurrentSaveFileIndex);
		}
	}

	private static void OnSavedAll(bool success)
	{
		if (success)
		{
			Initialized = true;
			if (_playerDatatInitHandler != null)
			{
				_playerDatatInitHandler(success: true);
				_playerDatatInitHandler = null;
			}
		}
		else
		{
			Debug.LogError("[PlayerData] SAVE FAILED. Retrying...");
			SaveAll();
		}
	}

	public static void SaveCurrentFile()
	{
		Save(CurrentSaveFileIndex);
	}

	public static void ResetDialoguer()
	{
		Dialoguer.SetGlobalVariablesState(emptyDialoguerState);
	}

	public static void ResetAll()
	{
		for (int i = 0; i < SAVE_FILE_KEYS.Length; i++)
		{
			ClearSlot(i);
		}
	}

	public static void Unload()
	{
		_saveFiles = null;
	}

	public bool IsUnlocked(PlayerId player, Weapon value)
	{
		return player switch
		{
			PlayerId.PlayerOne => inventories.GetPlayer(PlayerId.PlayerOne).IsUnlocked(value), 
			PlayerId.PlayerTwo => inventories.GetPlayer(PlayerId.PlayerTwo).IsUnlocked(value), 
			PlayerId.Any => inventories.GetPlayer(PlayerId.PlayerOne).IsUnlocked(value) || inventories.GetPlayer(PlayerId.PlayerTwo).IsUnlocked(value), 
			_ => false, 
		};
	}

	public bool IsUnlocked(PlayerId player, Super value)
	{
		return player switch
		{
			PlayerId.PlayerOne => inventories.GetPlayer(PlayerId.PlayerOne).IsUnlocked(value), 
			PlayerId.PlayerTwo => inventories.GetPlayer(PlayerId.PlayerTwo).IsUnlocked(value), 
			PlayerId.Any => inventories.GetPlayer(PlayerId.PlayerOne).IsUnlocked(value) || inventories.GetPlayer(PlayerId.PlayerTwo).IsUnlocked(value), 
			_ => false, 
		};
	}

	public bool IsUnlocked(PlayerId player, Charm value)
	{
		return player switch
		{
			PlayerId.PlayerOne => inventories.GetPlayer(PlayerId.PlayerOne).IsUnlocked(value), 
			PlayerId.PlayerTwo => inventories.GetPlayer(PlayerId.PlayerTwo).IsUnlocked(value), 
			PlayerId.Any => inventories.GetPlayer(PlayerId.PlayerOne).IsUnlocked(value) || inventories.GetPlayer(PlayerId.PlayerTwo).IsUnlocked(value), 
			_ => false, 
		};
	}

	public bool HasNewPurchase(PlayerId player)
	{
		return player switch
		{
			PlayerId.PlayerOne => inventories.GetPlayer(PlayerId.PlayerOne).newPurchase, 
			PlayerId.PlayerTwo => inventories.GetPlayer(PlayerId.PlayerTwo).newPurchase, 
			PlayerId.Any => inventories.GetPlayer(PlayerId.PlayerOne).newPurchase || inventories.GetPlayer(PlayerId.PlayerTwo).newPurchase, 
			_ => false, 
		};
	}

	public void ResetHasNewPurchase(PlayerId player)
	{
		switch (player)
		{
		case PlayerId.PlayerOne:
			inventories.GetPlayer(PlayerId.PlayerOne).newPurchase = false;
			break;
		case PlayerId.PlayerTwo:
			inventories.GetPlayer(PlayerId.PlayerTwo).newPurchase = false;
			break;
		case PlayerId.Any:
			inventories.GetPlayer(PlayerId.PlayerOne).newPurchase = false;
			inventories.GetPlayer(PlayerId.PlayerTwo).newPurchase = false;
			break;
		}
	}

	public bool Buy(PlayerId player, Weapon value)
	{
		return inventories.GetPlayer(player).Buy(value);
	}

	public bool Buy(PlayerId player, Super value)
	{
		return inventories.GetPlayer(player).Buy(value);
	}

	public bool Buy(PlayerId player, Charm value)
	{
		return inventories.GetPlayer(player).Buy(value);
	}

	public void Gift(PlayerId player, Weapon value)
	{
		inventories.GetPlayer(player)._weapons.Add(value);
	}

	public void Gift(PlayerId player, Super value)
	{
		inventories.GetPlayer(player)._supers.Add(value);
	}

	public void Gift(PlayerId player, Charm value)
	{
		inventories.GetPlayer(player)._charms.Add(value);
	}

	public int NumWeapons(PlayerId player)
	{
		return inventories.GetPlayer(player)._weapons.Count;
	}

	public int NumCharms(PlayerId player)
	{
		return inventories.GetPlayer(player)._charms.Count;
	}

	public int NumSupers(PlayerId player)
	{
		return inventories.GetPlayer(player)._supers.Count;
	}

	public int GetCurrency(PlayerId player)
	{
		return inventories.GetPlayer(player).money;
	}

	public void AddCurrency(PlayerId player, int value)
	{
		inventories.GetPlayer(player).money += value;
	}

	public void ResetLevelCoinManager()
	{
		levelCoinManager = new PlayerCoinManager();
	}

	public bool GetCoinCollected(LevelCoin coin)
	{
		return coinManager.GetCoinCollected(coin);
	}

	public void SetLevelCoinCollected(LevelCoin coin, bool collected, PlayerId player)
	{
		levelCoinManager.SetCoinValue(coin, collected, player);
	}

	public int GetNumCoinsCollectedInLevel(Levels level)
	{
		List<PlayerCoinManager.LevelAndCoins> levelsAndCoins = coinManager.LevelsAndCoins;
		for (int i = 0; i < levelsAndCoins.Count; i++)
		{
			if (levelsAndCoins[i].level == level)
			{
				int num = 0;
				if (levelsAndCoins[i].Coin1Collected)
				{
					num++;
				}
				if (levelsAndCoins[i].Coin2Collected)
				{
					num++;
				}
				if (levelsAndCoins[i].Coin3Collected)
				{
					num++;
				}
				if (levelsAndCoins[i].Coin4Collected)
				{
					num++;
				}
				if (levelsAndCoins[i].Coin5Collected)
				{
					num++;
				}
				return num;
			}
		}
		return 0;
	}

	public void ApplyLevelCoins()
	{
		foreach (PlayerCoinProperties coin in levelCoinManager.coins)
		{
			coinManager.SetCoinValue(coin.coinID, coin.collected, coin.player);
			if (coin.collected)
			{
				Data.AddCurrency(PlayerId.PlayerOne, 1);
				Data.AddCurrency(PlayerId.PlayerTwo, 1);
			}
		}
		levelCoinManager = new PlayerCoinManager();
	}

	public MapData GetMapData(Scenes map)
	{
		return mapDataManager.GetMapData(map);
	}

	public PlayerLevelDataObject GetLevelData(Levels levelID)
	{
		return levelDataManager.GetLevelData(levelID);
	}

	public int CountLevelsCompleted(Levels[] levels)
	{
		int num = 0;
		foreach (Levels levelID in levels)
		{
			PlayerLevelDataObject levelData = GetLevelData(levelID);
			if (levelData.completed)
			{
				num++;
			}
		}
		return num;
	}

	public bool CheckLevelsCompleted(Levels[] levels)
	{
		foreach (Levels levelID in levels)
		{
			PlayerLevelDataObject levelData = GetLevelData(levelID);
			if (!levelData.completed)
			{
				return false;
			}
		}
		return true;
	}

	public bool CheckLevelCompleted(Levels level)
	{
		PlayerLevelDataObject levelData = GetLevelData(level);
		if (!levelData.completed)
		{
			return false;
		}
		return true;
	}

	public int CountLevelsHaveMinGrade(Levels[] levels, LevelScoringData.Grade minGrade)
	{
		int num = 0;
		foreach (Levels levelID in levels)
		{
			PlayerLevelDataObject levelData = GetLevelData(levelID);
			if (levelData.completed && levelData.grade >= minGrade)
			{
				num++;
			}
		}
		return num;
	}

	public bool CheckLevelsHaveMinGrade(Levels[] levels, LevelScoringData.Grade minGrade)
	{
		foreach (Levels levelID in levels)
		{
			PlayerLevelDataObject levelData = GetLevelData(levelID);
			if (!levelData.completed || levelData.grade < minGrade)
			{
				return false;
			}
		}
		return true;
	}

	public int CountLevelsHaveMinDifficulty(Levels[] levels, Level.Mode minDifficulty)
	{
		int num = 0;
		foreach (Levels levelID in levels)
		{
			PlayerLevelDataObject levelData = GetLevelData(levelID);
			if (levelData.completed && levelData.difficultyBeaten >= minDifficulty)
			{
				num++;
			}
		}
		return num;
	}

	public bool CheckLevelsHaveMinDifficulty(Levels[] levels, Level.Mode minDifficulty)
	{
		foreach (Levels levelID in levels)
		{
			PlayerLevelDataObject levelData = GetLevelData(levelID);
			if (!levelData.completed || levelData.difficultyBeaten < minDifficulty)
			{
				return false;
			}
		}
		return true;
	}

	public int CountLevelsChaliceCompleted(Levels[] levels, PlayerId playerId)
	{
		int num = 0;
		foreach (Levels levelID in levels)
		{
			if ((playerId == PlayerId.PlayerOne && GetLevelData(levelID).completedAsChaliceP1) || (playerId == PlayerId.PlayerTwo && GetLevelData(levelID).completedAsChaliceP2))
			{
				num++;
			}
		}
		return num;
	}

	private static float CurseCharmValue(Levels level)
	{
		List<Levels> list = new List<Levels>(Level.world1BossLevels);
		if (Array.IndexOf(Level.world1BossLevels, level) >= 0)
		{
			return 2f;
		}
		if (Array.IndexOf(Level.world2BossLevels, level) >= 0)
		{
			return 2.5f;
		}
		if (Array.IndexOf(Level.world3BossLevels, level) >= 0)
		{
			return 3f;
		}
		if (Array.IndexOf(Level.world4MiniBossLevels, level) >= 0)
		{
			return 1f;
		}
		switch (level)
		{
		case Levels.DicePalaceMain:
			return 1f;
		case Levels.Devil:
			return 4f;
		default:
			if (Array.IndexOf(Level.worldDLCBossLevels, level) >= 0)
			{
				return 3f;
			}
			if (level == Levels.Saltbaker)
			{
				return 4f;
			}
			return 0f;
		}
	}

	private int completionPercentageOnly_CalculateCurseCharmLevel(PlayerId playerId)
	{
		if (!GetLevelData(Levels.Graveyard).completed)
		{
			return -1;
		}
		Levels[] levels = new Levels[34]
		{
			Levels.Veggies,
			Levels.Slime,
			Levels.FlyingBlimp,
			Levels.Flower,
			Levels.Frogs,
			Levels.Baroness,
			Levels.Clown,
			Levels.FlyingGenie,
			Levels.Dragon,
			Levels.FlyingBird,
			Levels.Bee,
			Levels.Pirate,
			Levels.SallyStagePlay,
			Levels.Mouse,
			Levels.Robot,
			Levels.FlyingMermaid,
			Levels.Train,
			Levels.DicePalaceBooze,
			Levels.DicePalaceChips,
			Levels.DicePalaceCigar,
			Levels.DicePalaceDomino,
			Levels.DicePalaceEightBall,
			Levels.DicePalaceFlyingHorse,
			Levels.DicePalaceFlyingMemory,
			Levels.DicePalaceRabbit,
			Levels.DicePalaceRoulette,
			Levels.DicePalaceMain,
			Levels.Devil,
			Levels.Airplane,
			Levels.RumRunners,
			Levels.OldMan,
			Levels.SnowCult,
			Levels.FlyingCowboy,
			Levels.Saltbaker
		};
		int num = CalculateCurseCharmAccumulatedValue(playerId, levels);
		int[] levelThreshold = WeaponProperties.CharmCurse.levelThreshold;
		for (int i = 0; i < levelThreshold.Length; i++)
		{
			if (num < levelThreshold[i])
			{
				return i - 1;
			}
		}
		return levelThreshold.Length - 1;
	}

	private bool completionPercentageOnly_CurseCharmIsMaxLevel(PlayerId playerId)
	{
		int[] levelThreshold = WeaponProperties.CharmCurse.levelThreshold;
		return completionPercentageOnly_CalculateCurseCharmLevel(playerId) == levelThreshold.Length - 1;
	}

	public int CalculateCurseCharmAccumulatedValue(PlayerId playerId, Levels[] levels)
	{
		float num = 0f;
		foreach (Levels levels2 in levels)
		{
			PlayerLevelDataObject levelData = GetLevelData(levels2);
			if (playerId == PlayerId.PlayerOne && levelData.curseCharmP1)
			{
				num += CurseCharmValue(levels2);
			}
			else if (playerId == PlayerId.PlayerTwo && levelData.curseCharmP2)
			{
				num += CurseCharmValue(levels2);
			}
		}
		return (int)num;
	}

	public float GetCompletionPercentage()
	{
		List<Levels> list = new List<Levels>();
		list.AddRange(Level.world1BossLevels);
		list.AddRange(Level.world2BossLevels);
		list.AddRange(Level.world3BossLevels);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		foreach (Levels item in list)
		{
			PlayerLevelDataObject levelData = GetLevelData(item);
			if (levelData.completed)
			{
				num++;
				switch (levelData.difficultyBeaten)
				{
				case Level.Mode.Normal:
					num2++;
					break;
				case Level.Mode.Hard:
					num2++;
					num8++;
					break;
				}
			}
		}
		Levels[] platformingLevels = Level.platformingLevels;
		foreach (Levels levelID in platformingLevels)
		{
			PlayerLevelDataObject levelData2 = GetLevelData(levelID);
			if (levelData2.completed)
			{
				num3++;
			}
		}
		num5 = coinManager.NumCoinsCollected(DLC: false);
		num4 = NumSupers(PlayerId.PlayerOne);
		PlayerLevelDataObject levelData3 = GetLevelData(Levels.DicePalaceMain);
		if (levelData3.completed)
		{
			num6++;
			if (levelData3.difficultyBeaten == Level.Mode.Hard)
			{
				num9++;
			}
		}
		PlayerLevelDataObject levelData4 = GetLevelData(Levels.Devil);
		if (levelData4.completed)
		{
			num7++;
			if (levelData4.difficultyBeaten == Level.Mode.Hard)
			{
				num10++;
			}
		}
		return (float)num * 1.5f + (float)num3 * 1.5f + (float)num5 * 0.5f + (float)num4 * 1.5f + (float)(num2 * 2) + (float)(num6 * 3) + (float)(num7 * 4) + (float)(num8 * 5) + (float)(num9 * 7) + (float)(num10 * 8);
	}

	public float GetCompletionPercentageDLC()
	{
		if (!DLCManager.DLCEnabled())
		{
			return 0f;
		}
		List<Levels> list = new List<Levels>();
		list.AddRange(Level.worldDLCBossLevels);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		foreach (Levels item in list)
		{
			PlayerLevelDataObject levelData = GetLevelData(item);
			if (levelData.completed)
			{
				num++;
				switch (levelData.difficultyBeaten)
				{
				case Level.Mode.Normal:
					num2++;
					break;
				case Level.Mode.Hard:
					num2++;
					num3++;
					break;
				}
			}
		}
		num6 = coinManager.NumCoinsCollected(DLC: true);
		PlayerLevelDataObject levelData2 = GetLevelData(Levels.Saltbaker);
		if (levelData2.completed)
		{
			num4++;
			if (levelData2.difficultyBeaten == Level.Mode.Hard)
			{
				num5++;
			}
		}
		if (curseCharmPuzzleComplete)
		{
			num7++;
		}
		if (GetLevelData(Levels.Graveyard).completed)
		{
			num8++;
		}
		if (completionPercentageOnly_CurseCharmIsMaxLevel(PlayerId.PlayerOne) || completionPercentageOnly_CurseCharmIsMaxLevel(PlayerId.PlayerTwo))
		{
			num9++;
		}
		return (float)num * 3.5f + (float)num2 * 5f + (float)num3 * 4.5f + (float)num4 * 6f + (float)num5 * 6f + (float)num6 * 1f + (float)num7 * 1f + (float)num8 * 3f + (float)num9 * 3f;
	}

	public int DeathCount(PlayerId player)
	{
		return player switch
		{
			PlayerId.PlayerOne => statictics.GetPlayer(PlayerId.PlayerOne).DeathCount(), 
			PlayerId.PlayerTwo => statictics.GetPlayer(PlayerId.PlayerTwo).DeathCount(), 
			PlayerId.Any => statictics.GetPlayer(PlayerId.PlayerOne).DeathCount() + statictics.GetPlayer(PlayerId.PlayerTwo).DeathCount(), 
			_ => 0, 
		};
	}

	public void Die(PlayerId player)
	{
		switch (player)
		{
		case PlayerId.PlayerOne:
			statictics.GetPlayer(PlayerId.PlayerOne).Die();
			break;
		case PlayerId.PlayerTwo:
			statictics.GetPlayer(PlayerId.PlayerTwo).Die();
			break;
		}
	}

	public int GetNumParriesInRow(PlayerId player)
	{
		return player switch
		{
			PlayerId.PlayerOne => statictics.GetPlayer(PlayerId.PlayerOne).numParriesInRow, 
			PlayerId.PlayerTwo => statictics.GetPlayer(PlayerId.PlayerTwo).numParriesInRow, 
			PlayerId.Any => Mathf.Max(statictics.GetPlayer(PlayerId.PlayerOne).numParriesInRow, statictics.GetPlayer(PlayerId.PlayerTwo).numParriesInRow), 
			_ => 0, 
		};
	}

	public void SetNumParriesInRow(PlayerId player, int numParriesInRow)
	{
		switch (player)
		{
		case PlayerId.PlayerOne:
			statictics.GetPlayer(PlayerId.PlayerOne).numParriesInRow = numParriesInRow;
			break;
		case PlayerId.PlayerTwo:
			statictics.GetPlayer(PlayerId.PlayerTwo).numParriesInRow = numParriesInRow;
			break;
		}
	}

	public void IncrementKingOfGamesCounter()
	{
		if (CountLevelsCompleted(Level.kingOfGamesLevels) != Level.kingOfGamesLevels.Length)
		{
			chessBossAttemptCounter++;
		}
	}

	public void ResetKingOfGamesCounter()
	{
		chessBossAttemptCounter = 0;
	}

	public bool TryActivateDjimmi()
	{
		if (DjimmiFreedCurrentRegion())
		{
			if (DjimmiActivatedCurrentRegion())
			{
				if (CurrentMap == Scenes.scene_map_world_DLC)
				{
					djimmiActivatedInfiniteWishDLC = false;
				}
				else
				{
					djimmiActivatedInfiniteWishBaseGame = false;
				}
				AudioManager.Play("sfx_worldmap_djimmi_deactivate");
			}
			else
			{
				if (CurrentMap == Scenes.scene_map_world_DLC)
				{
					djimmiActivatedInfiniteWishDLC = true;
				}
				else
				{
					djimmiActivatedInfiniteWishBaseGame = true;
				}
				MapEventNotification.Current.ShowEvent(MapEventNotification.Type.DjimmiFreed);
			}
			SaveCurrentFile();
			return true;
		}
		if (djimmiActivatedCountedWish)
		{
			djimmiActivatedCountedWish = false;
			djimmiWishes++;
			SaveCurrentFile();
			AudioManager.Play("sfx_worldmap_djimmi_deactivate");
			return true;
		}
		if (!djimmiActivatedCountedWish && djimmiWishes > 0)
		{
			djimmiActivatedCountedWish = true;
			djimmiWishes--;
			SaveCurrentFile();
			MapEventNotification.Current.ShowEvent(MapEventNotification.Type.Djimmi);
			return true;
		}
		return false;
	}

	public bool DjimmiActivatedCurrentRegion()
	{
		return (CurrentMap != Scenes.scene_map_world_DLC) ? DjimmiActivatedBaseGame() : DjimmiActivatedDLC();
	}

	public bool DjimmiActivatedBaseGame()
	{
		return djimmiActivatedCountedWish || djimmiActivatedInfiniteWishBaseGame;
	}

	public bool DjimmiActivatedDLC()
	{
		return djimmiActivatedCountedWish || djimmiActivatedInfiniteWishDLC;
	}

	public bool DjimmiFreedCurrentRegion()
	{
		return (CurrentMap != Scenes.scene_map_world_DLC) ? DjimmiFreedBaseGame() : DjimmiFreedDLC();
	}

	public bool DjimmiFreedBaseGame()
	{
		return CheckLevelsCompleted(Level.world1BossLevels) && CheckLevelsCompleted(Level.world2BossLevels) && CheckLevelsCompleted(Level.world3BossLevels) && CheckLevelsCompleted(Level.world4BossLevels) && CheckLevelsCompleted(Level.platformingLevels);
	}

	public bool DjimmiFreedDLC()
	{
		return CheckLevelsCompleted(Level.worldDLCBossLevelsWithSaltbaker) && CheckLevelsCompleted(Level.kingOfGamesLevels);
	}

	public void DeactivateDjimmi()
	{
		if (DjimmiFreedCurrentRegion())
		{
			if (CurrentMap == Scenes.scene_map_world_DLC)
			{
				djimmiActivatedInfiniteWishDLC = false;
			}
			else
			{
				djimmiActivatedInfiniteWishBaseGame = false;
			}
		}
		else
		{
			djimmiActivatedCountedWish = false;
		}
		SaveCurrentFile();
	}
}
