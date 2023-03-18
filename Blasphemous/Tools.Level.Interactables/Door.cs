using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.UI;
using Gameplay.UI.Widgets;
using I2.Loc;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Tools.Level.Interactables;

public class Door : Interactable
{
	private class DoorPersistenceData : PersistentManager.PersistentData
	{
		public bool closed;

		public bool objectUsed;

		public DoorPersistenceData(string id)
			: base(id)
		{
		}
	}

	[InfoBox("This level is not in the build index.This door may not work as expected.", InfoMessageType.Error, "SceneNotInBuildIndex")]
	[BoxGroup("Design Settings", true, false, 0)]
	[FormerlySerializedAs("doorId")]
	public string identificativeName;

	[BoxGroup("Design Settings", true, false, 0)]
	public bool autoEnter;

	[BoxGroup("Design Settings", true, false, 0)]
	public bool ladderRequired;

	[BoxGroup("Design Settings", true, false, 0)]
	public EntityOrientation exitOrientation;

	[BoxGroup("Design Settings", true, false, 0)]
	public bool startClosed;

	[BoxGroup("Design Settings", true, false, 0)]
	public float enterDelay = 2f;

	[BoxGroup("Design Settings", true, false, 0)]
	public bool showTextLockDoor = true;

	[BoxGroup("Design Settings", true, false, 0)]
	public string targetScene;

	[BoxGroup("Design Settings", true, false, 0)]
	public string targetDoor;

	[BoxGroup("Design Settings", true, false, 0)]
	public bool streamingLevel = true;

	[BoxGroup("Design Settings", true, false, 0)]
	[EventRef]
	public string openSound;

	[BoxGroup("Design Settings", true, false, 0)]
	public bool useFade = true;

	[BoxGroup("Design Settings", true, false, 0)]
	public bool objectNeeded;

	[BoxGroup("Design Settings", true, false, 0)]
	[ShowIf("objectNeeded", true)]
	[InventoryId(InventoryManager.ItemType.Quest)]
	public string objectId;

	[BoxGroup("Design Settings", true, false, 0)]
	[ShowIf("objectNeeded", true)]
	public bool showMessageIfNot = true;

	[BoxGroup("Design Settings", true, false, 0)]
	[ShowIf("objectNeeded", true)]
	public string soundPopupIfNot = string.Empty;

	[BoxGroup("Design Settings", true, false, 0)]
	[ShowIf("objectNeeded", true)]
	public bool showMessageOnOpen = true;

	[BoxGroup("Design Settings", true, false, 0)]
	[ShowIf("objectNeeded", true)]
	public string soundPopupOpen = string.Empty;

	[BoxGroup("Design Settings", true, false, 0)]
	[ShowIf("objectNeeded", true)]
	public bool removeObject = true;

	[BoxGroup("Attached References", true, false, 0)]
	public Transform spawnPoint;

	private bool objectUsed;

	private bool passingThrougDoor;

	private bool closed;

	public bool Closed
	{
		get
		{
			return closed;
		}
		set
		{
			closed = value;
			if (interactableAnimator != null)
			{
				interactableAnimator.SetBool("CLOSED", value);
			}
		}
	}

	public bool EnterTriggered => Core.Logic.Penitent.PlatformCharacterInput.ClampledVerticalAxis > 0f;

	public static event Core.SimpleEvent OnDoorEnter;

	public static event Core.SimpleEvent OnDoorExit;

	public void ExitFromThisDoor()
	{
		objectUsed = true;
		delayedOpen();
	}

	private IEnumerator ExitDoorSafe()
	{
		ShowPlayer(show: true);
		Core.Logic.Penitent.Physics.EnableColliders(enable: false);
		Core.Logic.Penitent.Physics.EnablePhysics(enable: false);
		yield return new WaitForEndOfFrame();
		Penitent currentPenitent = Core.Logic.Penitent;
		Vector3 spawnPointPos = spawnPoint.transform.position;
		currentPenitent.transform.position = spawnPointPos;
		currentPenitent.SetOrientation(exitOrientation);
		yield return new WaitForEndOfFrame();
		Core.Logic.Penitent.Physics.EnableColliders();
		yield return new WaitForEndOfFrame();
		Core.Logic.Penitent.Physics.EnablePhysics();
		Core.Input.SetBlocker("DOOR", blocking: false);
		Core.Logic.Penitent.Status.CastShadow = true;
		Core.Logic.Penitent.DamageArea.DamageAreaCollider.enabled = true;
		if (Door.OnDoorExit != null)
		{
			Door.OnDoorExit();
		}
	}

	private bool CheckRequirements()
	{
		bool flag = true;
		if (objectNeeded && !objectUsed)
		{
			QuestItem questItem = Core.InventoryManager.GetQuestItem(objectId);
			flag = (bool)questItem && Core.InventoryManager.IsQuestItemOwned(questItem);
			if (!flag && showMessageIfNot)
			{
				string valueWithParam = Core.Localization.GetValueWithParam(ScriptLocalization.UI_Inventory.TEXT_DOOR_NEED_OBJECT, "object_caption", questItem.caption);
				UIController.instance.ShowPopUp(valueWithParam, soundPopupIfNot);
			}
			if (flag)
			{
				if (showMessageOnOpen)
				{
					UIController.instance.ShowPopUpObjectUse(questItem.caption, soundPopupOpen);
					flag = false;
					StartCoroutine(WaitForPopupToEnter());
				}
				if (removeObject)
				{
					Core.InventoryManager.RemoveQuestItem(questItem);
				}
			}
		}
		return flag;
	}

	private void EnterDoor()
	{
		if (!ladderRequired || Core.Logic.Penitent.IsClimbingLadder)
		{
			if (base.PlayerInRange && !closed)
			{
				interactorAnimator.SetTrigger("OPEN_ENTER");
			}
			else if (base.PlayerInRange && closed)
			{
				interactorAnimator.SetTrigger("CLOSED_ENTER");
			}
			objectUsed = true;
			Core.Input.SetBlocker("DOOR", blocking: true);
			FadeWidget.OnFadeShowEnd += OnFadeShowEnd;
			FadeWidget.instance.Fade(toBlack: true, 0.2f, (!autoEnter) ? enterDelay : 0f);
			passingThrougDoor = true;
			if (autoEnter)
			{
				Core.Logic.Penitent.Physics.EnablePhysics(enable: false);
			}
			Core.Logic.Penitent.Status.CastShadow = false;
			Core.Logic.Penitent.DamageArea.DamageAreaCollider.enabled = false;
			if (Door.OnDoorEnter != null)
			{
				Door.OnDoorEnter();
			}
		}
	}

	protected override void InteractionStart()
	{
		ShowPlayer(show: false);
	}

	protected override void InteractionEnd()
	{
	}

	private IEnumerator WaitForPopupToEnter()
	{
		while (UIController.instance.IsShowingPopUp())
		{
			yield return new WaitForEndOfFrame();
		}
		EnterDoor();
	}

	protected override IEnumerator OnUse()
	{
		yield return 0;
	}

	protected override void OnAwake()
	{
		Closed = startClosed;
	}

	protected override void OnStart()
	{
		base.AnimatorEvent.OnEventLaunched += OnEventLaunched;
		base.Locked = false;
		interactableAnimator.SetBool("CLOSED", closed);
		if (autoEnter)
		{
			EnableInputIcon(enable: false);
		}
	}

	private void OnEventLaunched(string id)
	{
		if (id == "OPEN_DOOR")
		{
			Core.Audio.PlaySfx(openSound);
			Closed = false;
		}
	}

	protected override void OnUpdate()
	{
		if (Core.Logic == null || !(Core.Logic.Penitent == null))
		{
			if (base.PlayerInRange && base.InteractionTriggered && !base.Locked && CheckRequirements())
			{
				EnterDoor();
			}
			if (base.PlayerInRange && autoEnter && !base.Locked && !passingThrougDoor && CheckRequirements())
			{
				EnterDoor();
			}
			if (base.BeingUsed)
			{
				PlayerReposition();
			}
		}
	}

	private void OnFadeShowEnd()
	{
		ProCamera2D.Instance.HorizontalFollowSmoothness = 0f;
		ProCamera2D.Instance.VerticalFollowSmoothness = 0f;
		FadeWidget.OnFadeShowEnd -= OnFadeShowEnd;
		OnDoorActivated();
	}

	private void OnDoorActivated()
	{
		Log.Trace("Door", "Door has been activated.");
		Core.SpawnManager.SpawnFromDoor(targetScene, targetDoor, useFade);
	}

	[UsedImplicitly]
	public bool SceneNotInBuildIndex()
	{
		return SceneManager.GetActiveScene().buildIndex == -1;
	}

	public void InstaOpen()
	{
		Invoke("delayedOpen", 0.1f);
	}

	private void delayedOpen()
	{
		closed = false;
		interactableAnimator.SetTrigger("INSTA_OPEN");
		interactableAnimator.SetBool("CLOSED", value: false);
	}

	public override bool IsOpenOrActivated()
	{
		return !Closed;
	}

	public bool CanBeUsed()
	{
		return targetDoor != null && targetScene != null;
	}

	public override PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		DoorPersistenceData doorPersistenceData = CreatePersistentData<DoorPersistenceData>();
		doorPersistenceData.objectUsed = objectUsed;
		doorPersistenceData.closed = Closed;
		return doorPersistenceData;
	}

	public override void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		DoorPersistenceData doorPersistenceData = (DoorPersistenceData)data;
		objectUsed = doorPersistenceData.objectUsed;
		Closed = doorPersistenceData.closed;
		if (!Closed)
		{
			interactableAnimator.SetTrigger("INSTA_OPEN");
		}
		OnBlocked(base.Locked);
	}
}
