using System;
using System.Collections.Generic;

public static class CharmCurse
{
	public static readonly Levels[] CountableLevels = new Levels[34]
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

	private static List<int[]> healerCharmIntervals;

	public static int CalculateLevel(PlayerId playerId)
	{
		if (!PlayerData.Data.GetLevelData(Levels.Graveyard).completed)
		{
			return -1;
		}
		int num = PlayerData.Data.CalculateCurseCharmAccumulatedValue(playerId, CountableLevels);
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

	public static bool IsMaxLevel(PlayerId playerId)
	{
		int[] levelThreshold = WeaponProperties.CharmCurse.levelThreshold;
		return CalculateLevel(playerId) == levelThreshold.Length - 1;
	}

	public static int GetHealthModifier(int charmLevel)
	{
		if (charmLevel < 0)
		{
			return 0;
		}
		return WeaponProperties.CharmCurse.healthModifierValues[charmLevel];
	}

	public static float GetSuperMeterAmount(int charmLevel)
	{
		if (charmLevel < 0)
		{
			return 0f;
		}
		return WeaponProperties.CharmCurse.superMeterAmount[charmLevel];
	}

	public static int GetSmokeDashInterval(int charmLevel)
	{
		if (charmLevel < 0)
		{
			return 0;
		}
		return WeaponProperties.CharmCurse.smokeDashInterval[charmLevel];
	}

	public static int GetWhetstoneInterval(int charmLevel)
	{
		if (charmLevel < 0)
		{
			return 0;
		}
		return WeaponProperties.CharmCurse.whetstoneInterval[charmLevel];
	}

	public static int GetHealerInterval(int charmLevel, int hpReceived)
	{
		if (charmLevel < 0)
		{
			return 0;
		}
		if (healerCharmIntervals == null)
		{
			string[] healerInterval = WeaponProperties.CharmCurse.healerInterval;
			healerCharmIntervals = new List<int[]>(healerInterval.Length);
			string[] array = healerInterval;
			foreach (string text in array)
			{
				string[] array2 = text.Split(',');
				if (array2.Length != 3)
				{
					throw new Exception("Invalid healer intervals");
				}
				int[] array3 = new int[array2.Length];
				for (int j = 0; j < array3.Length; j++)
				{
					array3[j] = Parser.IntParse(array2[j]);
				}
				healerCharmIntervals.Add(array3);
			}
		}
		return healerCharmIntervals[charmLevel][hpReceived];
	}
}
