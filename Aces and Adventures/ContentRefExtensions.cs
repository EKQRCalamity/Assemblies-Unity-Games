using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;

public static class ContentRefExtensions
{
	public static bool IsValid(this ContentRef cRef)
	{
		return cRef?.hasContent ?? false;
	}

	public static bool Exists(this ContentRef cRef)
	{
		return cRef?.exists ?? false;
	}

	public static bool ShouldSerialize(this ContentRef cRef)
	{
		return cRef?.hasSavedContent ?? false;
	}

	public static C DataOrDefault<C>(this DataRef<C> dataRef) where C : IDataContent
	{
		if (!dataRef.IsValid())
		{
			return DataRef<C>.DefaultData;
		}
		return dataRef.data;
	}

	public static bool CanRevertChanges<C>(this DataRef<C> dataRef) where C : IDataContent
	{
		return dataRef?.hasSavedContent ?? false;
	}

	public static bool SaveCanBeOverriden(this ContentRef cRef)
	{
		if (cRef.IsValid())
		{
			return cRef.saveCanBeOverriden;
		}
		return false;
	}

	public static bool CanUpload(this ContentRef cRef)
	{
		if (cRef != null && cRef.canUpload)
		{
			return Steam.CanUseWorkshop;
		}
		return false;
	}

	public static bool CanInspectWorkshopItem(this ContentRef cRef)
	{
		if ((bool)cRef && cRef.hasPublishedFileId)
		{
			return Steam.CanUseWorkshop;
		}
		return false;
	}

	public static string GetFriendlyName(this ContentRef cRef)
	{
		if (!cRef.IsValid())
		{
			return "NULL";
		}
		return cRef.friendlyName;
	}

	public static string SpecificTypeFriendly(this ContentRef cRef)
	{
		if (cRef == null)
		{
			return "NULL";
		}
		return cRef.specificTypeFriendly;
	}

	public static ContentInstallStatus GetInstallStatus(this ContentRef contentRef, uint timeUpdated)
	{
		if ((bool)contentRef)
		{
			if (!contentRef.createdByOtherUser || contentRef.lastUpdateTime >= timeUpdated)
			{
				return ContentInstallStatus.Installed;
			}
			return ContentInstallStatus.Update;
		}
		return ContentInstallStatus.New;
	}

	public static void OnEditRequest(this ContentRef dataRef, Transform parent = null, Action<ContentRef> onClose = null)
	{
		if (!dataRef.IsValid() || !dataRef.isDataRef)
		{
			return;
		}
		if (SceneRef.DataRefCreationScenesByType.ContainsKey(dataRef.dataType))
		{
			string title = (dataRef.Exists() ? ("Edit " + dataRef.specificTypeFriendly + ": " + dataRef.GetFriendlyName()) : ("Create New " + dataRef.specificTypeFriendly));
			SceneRef editScene = SceneRef.DataRefCreationScenesByType[dataRef.dataType];
			if (SceneManager.GetActiveScene().buildIndex != editScene.buildIndex)
			{
				UIUtil.CreatePopup(title, UIUtil.CreateMessageBox("Would you like to transition to the <b>" + dataRef.specificTypeFriendly + "</b> scene? All changes made in this scene will be saved.", TextAlignmentOptions.Left, 32, 600, 300, 24f), null, parent: parent, buttons: new string[2] { "Cancel", title }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
				{
					if (!(s != title))
					{
						SceneRef.DisableEventSystemUntilSceneTransition();
						DataRefControl.SaveAllActiveControls();
						Job.Process(Job.WaitForDepartmentEmpty(Department.Content)).Immediately().Do(delegate
						{
							LoadScreenView.Load(editScene);
						});
					}
				});
				return;
			}
			IDataRefControl mainControlForDataType = DataRefControl.GetMainControlForDataType(dataRef);
			if (!ContentRef.Equal(mainControlForDataType.dataRef, dataRef))
			{
				UIUtil.CreatePopup(title, UIUtil.CreateMessageBox("Would you like to <b>" + title + "</b>? All changes to <b>" + mainControlForDataType.dataRef.GetFriendlyName() + "</b> will be saved.", TextAlignmentOptions.Left, 32, 600, 300, 24f), null, parent: parent, buttons: new string[2] { "Cancel", title }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
				{
					if (!(s != title))
					{
						InputManager.SetEventSystemEnabled(dataRef, enabled: false);
						DataRefControl.SaveAllActiveControls();
						Job.Process(Job.WaitForDepartmentEmpty(Department.Content)).Immediately().Do(delegate
						{
							mainControlForDataType.SetDataRefIfValid(dataRef);
							InputManager.SetEventSystemEnabled(dataRef, enabled: true);
						});
					}
				});
			}
			else
			{
				GameObject mainContent3 = UIUtil.CreateMessageBox("You are currently editing <b>" + dataRef.GetFriendlyName() + "</b>", TextAlignmentOptions.Left, 32, 600, 300, 24f);
				Transform parent4 = parent;
				UIUtil.CreatePopup("Currently Editing", mainContent3, null, null, null, null, null, null, true, true, null, null, null, parent4, null, null);
			}
		}
		else
		{
			DataRefControl.CreateLiveEditPopup(dataRef, parent, onClose);
		}
	}

	public static bool IsValid(this IDataRefControl iDataRefControl)
	{
		return iDataRefControl?.isValid ?? false;
	}

	public static ContentRef GetDataRef(this IDataRefControl iDataRefControl)
	{
		return iDataRefControl?.dataRef;
	}

	public static TableReference GetTableReference(this ContentRef dataRef)
	{
		return LocalizationSettings.StringDatabase.GetTable(dataRef.specificTypeFriendly).TableCollectionName;
	}

	public static ushort Version(this ContentRef.MetaData metaData)
	{
		return metaData?.version ?? 0;
	}

	public static DataRef<AbilityData> BaseAbilityRef(this DataRef<AbilityData> abilityDataRef)
	{
		while ((bool)abilityDataRef.data.upgradeOf)
		{
			abilityDataRef = abilityDataRef.data.upgradeOf;
		}
		return abilityDataRef;
	}

	public static float GetUnlockWeight(this DataRef<AbilityData> abilityDataRef)
	{
		return ContentRef.Defaults.data.GetAbilityUnlockWeight(abilityDataRef);
	}

	public static PoolKeepItemListHandle<DataRef<AbilityData>> GetUpgradeHierarchy(this DataRef<AbilityData> abilityRef, bool sort = true)
	{
		PoolKeepItemListHandle<DataRef<AbilityData>> poolKeepItemListHandle = Pools.UseKeepItemList<DataRef<AbilityData>>();
		if (!abilityRef.data.characterClass.HasValue)
		{
			return poolKeepItemListHandle.Add(abilityRef);
		}
		DataRef<AbilityData> a2 = abilityRef.BaseAbilityRef();
		foreach (DataRef<AbilityData> ability in AbilityData.GetAbilities(abilityRef.data.characterClass.Value))
		{
			if (ContentRef.Equal(a2, ability.BaseAbilityRef()))
			{
				poolKeepItemListHandle.Add(ability);
			}
		}
		if (sort)
		{
			poolKeepItemListHandle.value.Sort((DataRef<AbilityData> a, DataRef<AbilityData> b) => a.data.rank - b.data.rank);
		}
		return poolKeepItemListHandle;
	}

	public static bool CanBeUnlocked(this DataRef<AdventureData> adventureDataRef)
	{
		if (IOUtil.IsDemo)
		{
			return adventureDataRef.data.unlockedForDemo;
		}
		return true;
	}

	public static bool TrackedByAchievements(this DataRef<AdventureData> adventureDataRef)
	{
		if ((bool)adventureDataRef)
		{
			return adventureDataRef.data.trackedByAchievements;
		}
		return false;
	}

	public static bool CanBeUnlocked(this DataRef<GameData> gameDataRef)
	{
		if (IOUtil.IsDemo)
		{
			return gameDataRef.data.unlockedForDemo;
		}
		return true;
	}

	public static DataRef<EnemyData> BaseRef(this DataRef<EnemyData> enemyDataRef)
	{
		while ((bool)enemyDataRef.data.upgradeOf)
		{
			enemyDataRef = enemyDataRef.data.upgradeOf;
		}
		return enemyDataRef;
	}

	public static PoolKeepItemListHandle<DataRef<EnemyData>> GetUpgradeHierarchy(this DataRef<EnemyData> enemyDataRef)
	{
		PoolKeepItemListHandle<DataRef<EnemyData>> poolKeepItemListHandle = Pools.UseKeepItemList<DataRef<EnemyData>>();
		DataRef<EnemyData> a2 = enemyDataRef.BaseRef();
		foreach (DataRef<EnemyData> item in DataRef<EnemyData>.All)
		{
			if (ContentRef.Equal(a2, item.BaseRef()))
			{
				poolKeepItemListHandle.Add(item);
			}
		}
		poolKeepItemListHandle.value.Sort((DataRef<EnemyData> a, DataRef<EnemyData> b) => a.data.rank - b.data.rank);
		return poolKeepItemListHandle;
	}
}
