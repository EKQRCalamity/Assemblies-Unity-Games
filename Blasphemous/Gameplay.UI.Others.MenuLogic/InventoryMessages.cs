using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.UI.Others.MenuLogic;

public class InventoryMessages : MonoBehaviour
{
	public enum Messages
	{
		Msg_Default,
		Msg_Relics1,
		Msg_Relics2,
		Msg_Rosary1,
		Msg_Rosary2,
		Msg_QuestItem,
		Msg_Prayer1,
		Msg_Prayer2,
		Msg_Prayer3
	}

	private Messages currentMessage;

	private Dictionary<Messages, GameObject> cacheObjs = new Dictionary<Messages, GameObject>();

	private void Start()
	{
		foreach (Messages value in Enum.GetValues(typeof(Messages)))
		{
			cacheObjs[value] = base.transform.Find(value.ToString()).gameObject;
			cacheObjs[value].SetActive(value: false);
		}
		ShowMessage(Messages.Msg_Default);
	}

	public void ShowMessage(Messages message)
	{
		cacheObjs[currentMessage].SetActive(value: false);
		currentMessage = message;
		cacheObjs[currentMessage].SetActive(value: true);
	}
}
