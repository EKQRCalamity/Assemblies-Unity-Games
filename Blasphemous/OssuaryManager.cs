using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Framework.Inventory;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

public class OssuaryManager : MonoBehaviour
{
	private struct OssuaryGroupStatus
	{
		public OSSUARY_GROUPS group;

		public string flag;

		public bool isCompleted;

		public OssuaryGroupStatus(OSSUARY_GROUPS g)
		{
			group = g;
			flag = $"{g.ToString()}_OSSUARY_FLAG";
			isCompleted = Core.Events.GetFlag(flag);
		}
	}

	private const string collectiblePath = "Inventory/CollectibleItem/";

	[FoldoutGroup("Main", 0)]
	public List<OssuaryItem> ossuaryItems;

	[FoldoutGroup("Activation Lists", 0)]
	public List<GameObject> activateIfOssuaryComplete;

	[FoldoutGroup("Activation Lists", 0)]
	public List<GameObject> deactivateIfOssuaryComplete;

	[FoldoutGroup("Groups", 0)]
	public List<OssuaryItemGroups> ossuaryGroups;

	[FoldoutGroup("Effects", 0)]
	public GameObject activationFX;

	[FoldoutGroup("Playmaker properties", 0)]
	public bool anyPendingCollectible;

	[FoldoutGroup("Playmaker properties", 0)]
	public int pendingRewards;

	[FoldoutGroup("Playmaker properties", 0)]
	public int alreadyClaimedRewards;

	[FoldoutGroup("Playmaker properties", 0)]
	public string CheckRewardsEvent;

	[FoldoutGroup("Debug buttons", 0)]
	[Button("Test CLAIM COLLECTIBLE ITEMS", ButtonSizes.Small)]
	public void ClaimCarriedItems()
	{
		List<string> nonClaimedCollectibles = GetNonClaimedCollectibles();
		if (nonClaimedCollectibles.Count > 0)
		{
			StartCoroutine(ClaimSequenceCoroutine(nonClaimedCollectibles, OnActivationsFinished));
		}
	}

	private void Start()
	{
		if (activationFX != null)
		{
			PoolManager.Instance.CreatePool(activationFX, 10);
		}
		foreach (OssuaryItem ossuaryItem in ossuaryItems)
		{
			ossuaryItem.gameObject.SetActive(value: false);
		}
		ActivateRetrievedCollectiblesSilently();
		anyPendingCollectible = IsThereAnyCollectibleNotClaimed();
	}

	public bool IsThereAnyCollectibleNotClaimed()
	{
		List<string> nonClaimedCollectibles = GetNonClaimedCollectibles();
		return nonClaimedCollectibles.Count != 0;
	}

	private void ActivateRetrievedCollectiblesSilently()
	{
		List<string> alreadyRetrievedCollectibles = GetAlreadyRetrievedCollectibles();
		foreach (string item in alreadyRetrievedCollectibles)
		{
			OssuaryItem ossuaryItem = ossuaryItems.Find((OssuaryItem x) => x.itemId == item);
			ossuaryItem.ActivateItemSilently();
		}
		if (IsOssuaryComplete())
		{
			CheckActivationLists();
			Core.AchievementsManager.GrantAchievement("AC19");
		}
	}

	private static void DeactivateRetrievedCollectiblesSilently()
	{
		List<string> alreadyRetrievedCollectibles = GetAlreadyRetrievedCollectibles();
		OssuaryManager ossuaryManager = UnityEngine.Object.FindObjectOfType<OssuaryManager>();
		foreach (string item in alreadyRetrievedCollectibles)
		{
			OssuaryItem ossuaryItem = ossuaryManager.ossuaryItems.Find((OssuaryItem x) => x.itemId == item);
			ossuaryItem.DeactivateItemSilently();
		}
		ossuaryManager.alreadyClaimedRewards = 0;
	}

	private void CheckActivationLists()
	{
		foreach (GameObject item in activateIfOssuaryComplete)
		{
			item.SetActive(value: true);
		}
		foreach (GameObject item2 in deactivateIfOssuaryComplete)
		{
			item2.SetActive(value: false);
		}
	}

	private bool IsOssuaryComplete()
	{
		foreach (OssuaryItem ossuaryItem in ossuaryItems)
		{
			if (!ossuaryItem.gameObject.activeInHierarchy)
			{
				return false;
			}
		}
		return true;
	}

	private static List<string> GetAlreadyRetrievedCollectibles()
	{
		List<string> list = new List<string>();
		ReadOnlyCollection<Framework.Inventory.CollectibleItem> collectibleItemOwned = Core.InventoryManager.GetCollectibleItemOwned();
		foreach (Framework.Inventory.CollectibleItem item in collectibleItemOwned)
		{
			if (item.ClaimedInOssuary)
			{
				list.Add(item.id);
			}
		}
		return list;
	}

	public static int CountAlreadyRetrievedCollectibles()
	{
		return GetAlreadyRetrievedCollectibles().Count;
	}

	public static void ResetAlreadyRetrievedCollectibles()
	{
		foreach (object value in Enum.GetValues(typeof(OSSUARY_GROUPS)))
		{
			string id = string.Concat(value, "_OSSUARY_FLAG");
			Core.Events.SetFlag(id, b: false);
		}
		DeactivateRetrievedCollectiblesSilently();
		List<string> alreadyRetrievedCollectibles = GetAlreadyRetrievedCollectibles();
		UnclaimInInventory(alreadyRetrievedCollectibles);
	}

	private List<string> GetNonClaimedCollectibles()
	{
		List<string> list = new List<string>();
		ReadOnlyCollection<Framework.Inventory.CollectibleItem> collectibleItemOwned = Core.InventoryManager.GetCollectibleItemOwned();
		foreach (Framework.Inventory.CollectibleItem item in collectibleItemOwned)
		{
			if (!item.ClaimedInOssuary)
			{
				list.Add(item.id);
			}
		}
		return list;
	}

	private void OnActivationsFinished()
	{
		List<string> nonClaimedCollectibles = GetNonClaimedCollectibles();
		ClaimInInventory(nonClaimedCollectibles);
		CheckGroupCompletion();
	}

	private void CheckGroupCompletion()
	{
		Array values = Enum.GetValues(typeof(OSSUARY_GROUPS));
		OssuaryGroupStatus[] array = new OssuaryGroupStatus[values.Length];
		pendingRewards = 0;
		alreadyClaimedRewards = values.Length;
		int num = 0;
		for (int i = 0; i < values.Length; i++)
		{
			ref OssuaryGroupStatus reference = ref array[i];
			reference = new OssuaryGroupStatus((OSSUARY_GROUPS)values.GetValue(i));
			if (array[i].isCompleted)
			{
				num++;
			}
		}
		if (num == values.Length)
		{
			Core.Events.LaunchEvent(CheckRewardsEvent, string.Empty);
			return;
		}
		alreadyClaimedRewards = num;
		for (int j = 0; j < values.Length; j++)
		{
			if (!array[j].isCompleted && CheckGroup(array[j].group))
			{
				Core.Events.SetFlag(array[j].flag, b: true);
				pendingRewards++;
			}
		}
		Core.Events.LaunchEvent(CheckRewardsEvent, string.Empty);
	}

	private bool CheckGroup(OSSUARY_GROUPS g)
	{
		List<OssuaryItem> items = ossuaryGroups.Find((OssuaryItemGroups x) => x.groupId == g).items;
		if (items == null)
		{
			Debug.LogFormat("Ossuary Group {0} has no items!", g);
			return false;
		}
		foreach (OssuaryItem item in items)
		{
			if (!item.gameObject.activeInHierarchy)
			{
				return false;
			}
		}
		return true;
	}

	private void ClaimInInventory(List<string> ids)
	{
		foreach (string id in ids)
		{
			Framework.Inventory.CollectibleItem collectibleItem = Core.InventoryManager.GetCollectibleItem(id);
			collectibleItem.ClaimedInOssuary = true;
		}
	}

	private static void UnclaimInInventory(List<string> ids)
	{
		foreach (string id in ids)
		{
			Framework.Inventory.CollectibleItem collectibleItem = Core.InventoryManager.GetCollectibleItem(id);
			collectibleItem.ClaimedInOssuary = false;
		}
	}

	private OssuaryItem GetOssuaryItem(string id)
	{
		return ossuaryItems.Find((OssuaryItem x) => x.itemId == id);
	}

	private IEnumerator ClaimSequenceCoroutine(List<string> ids, Action callback)
	{
		float timeBetweenActivations = 0.25f;
		for (int counter = 0; counter < ids.Count; counter++)
		{
			OssuaryItem oi = GetOssuaryItem(ids[counter]);
			oi.ActivateItem();
			ActivationEffect(oi.transform.position);
			yield return new WaitForSeconds(timeBetweenActivations);
		}
		callback();
	}

	private void ActivationEffect(Vector2 pos)
	{
		if (activationFX != null)
		{
			PoolManager.Instance.ReuseObject(activationFX, pos, Quaternion.identity);
		}
	}
}
