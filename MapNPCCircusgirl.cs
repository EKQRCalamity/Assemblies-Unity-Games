using System;
using UnityEngine;

public class MapNPCCircusgirl : MonoBehaviour
{
	[SerializeField]
	private int dialoguerVariableID = 7;

	[SerializeField]
	private string coinID = Guid.NewGuid().ToString();

	[HideInInspector]
	public bool SkipDialogueEvent;

	private void Start()
	{
		if (PlayerData.Data.coinManager.GetCoinCollected(coinID))
		{
			Dialoguer.SetGlobalFloat(dialoguerVariableID, 2f);
			return;
		}
		AddDialoguerEvents();
		OnlineManager.Instance.Interface.GetAchievement(PlayerId.PlayerOne, "FoundSecretPassage", delegate(OnlineAchievement achievement)
		{
			if (achievement.IsUnlocked)
			{
				Dialoguer.SetGlobalFloat(dialoguerVariableID, 1f);
			}
		});
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
		if (!SkipDialogueEvent && message == "GingerbreadCoin" && !PlayerData.Data.coinManager.GetCoinCollected(coinID))
		{
			Dialoguer.SetGlobalFloat(dialoguerVariableID, 2f);
			PlayerData.Data.coinManager.SetCoinValue(coinID, collected: true, PlayerId.Any);
			PlayerData.Data.AddCurrency(PlayerId.PlayerOne, 1);
			PlayerData.Data.AddCurrency(PlayerId.PlayerTwo, 1);
			PlayerData.SaveCurrentFile();
			MapEventNotification.Current.ShowEvent(MapEventNotification.Type.Coin);
		}
	}
}
