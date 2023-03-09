using System;
using UnityEngine;

public class MapNPCNewsieCat : MonoBehaviour
{
	private const int DIALOGUER_VAR_INDEX = 24;

	private const int DIALOGUER_VAR_GOT_COIN = 39;

	private const int DIALOGUER_VAR_INTERACT_COUNTER = 40;

	[SerializeField]
	private string coinID1 = Guid.NewGuid().ToString();

	[SerializeField]
	private string coinID2 = Guid.NewGuid().ToString();

	[SerializeField]
	private string coinID3 = Guid.NewGuid().ToString();

	private void Start()
	{
		Dialoguer.SetGlobalFloat(40, 0f);
		if (!PlayerData.Data.coinManager.GetCoinCollected(coinID1))
		{
			Dialoguer.SetGlobalFloat(39, 0f);
		}
		Levels[] levels = new Levels[5]
		{
			Levels.Veggies,
			Levels.Slime,
			Levels.FlyingBlimp,
			Levels.Flower,
			Levels.Frogs
		};
		Levels[] array = new Levels[6]
		{
			Levels.OldMan,
			Levels.RumRunners,
			Levels.Airplane,
			Levels.SnowCult,
			Levels.FlyingCowboy,
			Levels.Saltbaker
		};
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (PlayerData.Data.CheckLevelCompleted(array[i]))
			{
				num++;
			}
		}
		if (num > 0)
		{
			Dialoguer.SetGlobalFloat(24, 3f);
		}
		else if (PlayerData.Data.CheckLevelCompleted(Levels.Devil))
		{
			Dialoguer.SetGlobalFloat(24, 2f);
		}
		else if (PlayerData.Data.CheckLevelsCompleted(levels))
		{
			Dialoguer.SetGlobalFloat(24, 1f);
		}
		else
		{
			Dialoguer.SetGlobalFloat(24, 0f);
		}
		AddDialoguerEvents();
	}

	private void OnDestroy()
	{
		RemoveDialoguerEvents();
	}

	public void AddDialoguerEvents()
	{
		Dialoguer.events.onMessageEvent += OnDialoguerMessageEvent;
	}

	public void RemoveDialoguerEvents()
	{
		Dialoguer.events.onMessageEvent -= OnDialoguerMessageEvent;
	}

	private void OnDialoguerMessageEvent(string message, string metadata)
	{
		if (message == "NewsieCoin" && !PlayerData.Data.coinManager.GetCoinCollected(coinID1))
		{
			Dialoguer.SetGlobalFloat(39, 1f);
			PlayerData.Data.coinManager.SetCoinValue(coinID1, collected: true, PlayerId.Any);
			PlayerData.Data.coinManager.SetCoinValue(coinID2, collected: true, PlayerId.Any);
			PlayerData.Data.coinManager.SetCoinValue(coinID3, collected: true, PlayerId.Any);
			PlayerData.Data.AddCurrency(PlayerId.PlayerOne, 3);
			PlayerData.Data.AddCurrency(PlayerId.PlayerTwo, 3);
			PlayerData.SaveCurrentFile();
			MapEventNotification.Current.ShowEvent(MapEventNotification.Type.ThreeCoins);
		}
	}
}
