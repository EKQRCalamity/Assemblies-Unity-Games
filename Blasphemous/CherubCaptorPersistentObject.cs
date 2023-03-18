using System;
using System.Collections;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.UI;
using Tools.Level.Utils;
using UnityEngine;

public class CherubCaptorPersistentObject : PersistentObject
{
	private class CherubSpawnPersistenceData : PersistentManager.PersistentData
	{
		public string cherubId;

		public bool destroyed;

		public CherubSpawnPersistenceData(string id)
			: base(id)
		{
		}
	}

	public string cherubId;

	public bool destroyed;

	public CherubCaptorSpawnConfigurator spawner;

	public const int TOTAL_NUMBER_OF_CHERUBS_FOR_AC13 = 38;

	private const string RELEASE_CHERUB_SFX = "event:/Key Event/RelicCollected";

	public event Action<string> OnCherubDestroyed;

	public void OnCherubKilled()
	{
		destroyed = true;
		if (this.OnCherubDestroyed != null)
		{
			this.OnCherubDestroyed(cherubId);
		}
		spawner.DisableCherubSpawn();
		string id = cherubId;
		Core.Events.SetFlag(id, b: true);
		AddProgressToAC13();
		Camera.main.GetComponent<MonoBehaviour>().StartCoroutine(ShowPopUp());
	}

	public override PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		CherubSpawnPersistenceData cherubSpawnPersistenceData = CreatePersistentData<CherubSpawnPersistenceData>();
		cherubSpawnPersistenceData.cherubId = cherubId;
		cherubSpawnPersistenceData.destroyed = destroyed;
		Debug.Log($"<color=red>SAVING CHERUB OF ID:{cherubId}. Destroyed = {destroyed}</color>");
		return cherubSpawnPersistenceData;
	}

	public override void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		CherubSpawnPersistenceData cherubSpawnPersistenceData = (CherubSpawnPersistenceData)data;
		destroyed = cherubSpawnPersistenceData.destroyed;
		if (destroyed)
		{
			spawner.DisableCherubSpawn();
			spawner.DestroySpawnedCherub();
		}
	}

	private void AddProgressToAC13()
	{
		float progress = Core.AchievementsManager.Achievements["AC13"].Progress;
		float num = progress * 38f / 100f;
		int num2 = CountRescuedCherubs();
		if ((float)num2 > num)
		{
			Core.AchievementsManager.Achievements["AC13"].AddProgress(2.631579f);
		}
	}

	private IEnumerator ShowPopUp()
	{
		int currentNumberCherubs = CountRescuedCherubs();
		yield return new WaitForSeconds(1f);
		UIController.instance.ShowCherubPopUp(currentNumberCherubs + "/" + 38, "event:/Key Event/RelicCollected", 3f, blockPlayer: false);
	}

	public static int CountRescuedCherubs()
	{
		int num = 0;
		for (int i = 0; i < 38; i++)
		{
			string id = string.Format("RESCUED_CHERUB_{0}", (i + 1).ToString("00"));
			if (Core.Events.GetFlag(id))
			{
				num++;
			}
		}
		return num;
	}
}
