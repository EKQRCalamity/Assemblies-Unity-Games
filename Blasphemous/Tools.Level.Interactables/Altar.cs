using System.Collections;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.UI;
using Gameplay.UI.Others.MenuLogic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Interactables;

[SelectionBase]
public class Altar : Interactable
{
	private class AltarPersistenceData : PersistentManager.PersistentData
	{
		public bool lighted;

		public AltarPersistenceData(string id)
			: base(id)
		{
		}
	}

	[SerializeField]
	[BoxGroup("Config", true, false, 0)]
	private float PurgeBasePrice = 100f;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private string kneeStartId;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private string kneeEndId;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private float kneeStartDelay;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private float kneeEndDelay;

	[SerializeField]
	[BoxGroup("Event Settings", true, false, 0)]
	[ReadOnly]
	private string OnAltarUse = "ALTAR_ACTIVATED";

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	[Required]
	private SpriteRenderer entityRenderer;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	protected GameObject interactableAnimatorLevel1;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	protected GameObject interactableAnimatorLevel2;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	protected GameObject interactableAnimatorLevel3;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	protected GameObject interactableAnimatorLevel4;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	protected GameObject interactableAnimatorLevel5;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	protected GameObject interactableAnimatorLevel6;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	protected GameObject interactableAnimatorLevel7;

	private bool penitentKneeing;

	private int DialogResponse;

	private void OnAnimationEvent(string id)
	{
		if (id == "KNEE_END")
		{
			penitentKneeing = false;
		}
	}

	protected override void OnPlayerReady()
	{
		GameObject gameObject = null;
		interactableAnimatorLevel1.SetActive(value: false);
		interactableAnimatorLevel2.SetActive(value: false);
		interactableAnimatorLevel3.SetActive(value: false);
		interactableAnimatorLevel4.SetActive(value: false);
		interactableAnimatorLevel5.SetActive(value: false);
		interactableAnimatorLevel6.SetActive(value: false);
		interactableAnimatorLevel7.SetActive(value: false);
		gameObject = Core.Alms.GetAltarLevel() switch
		{
			1 => interactableAnimatorLevel1, 
			2 => interactableAnimatorLevel2, 
			3 => interactableAnimatorLevel3, 
			4 => interactableAnimatorLevel4, 
			5 => interactableAnimatorLevel5, 
			6 => interactableAnimatorLevel6, 
			7 => interactableAnimatorLevel7, 
			_ => interactableAnimatorLevel7, 
		};
		gameObject.SetActive(value: true);
		interactableAnimator = gameObject.GetComponent<Animator>();
		CheckAnimationEvents();
	}

	protected override void ObjectEnable()
	{
		if (!(base.AnimatorEvent == null))
		{
			base.AnimatorEvent.OnEventLaunched += OnAnimationEvent;
		}
	}

	protected override void ObjectDisable()
	{
		if (!(base.AnimatorEvent == null))
		{
			base.AnimatorEvent.OnEventLaunched -= OnAnimationEvent;
		}
	}

	protected override IEnumerator OnUse()
	{
		yield return ActivationLogic();
	}

	protected override void InteractionEnd()
	{
		Core.Logic.Penitent.SpriteRenderer.enabled = true;
		Core.Logic.Penitent.DamageArea.enabled = true;
	}

	protected override void OnUpdate()
	{
		if (!base.BeingUsed && base.PlayerInRange && base.InteractionTriggered)
		{
			Use();
		}
		if (base.BeingUsed)
		{
			PlayerReposition();
		}
	}

	protected override void PlayerReposition()
	{
		Core.Logic.Penitent.transform.position = interactorAnimator.transform.position;
	}

	private IEnumerator ActivationLogic()
	{
		if (RepositionBeforeInteract)
		{
			Core.Logic.Penitent.DrivePlayer.MoveToPosition(Waypoint.position, orientation);
			do
			{
				yield return null;
			}
			while (Core.Logic.Penitent.DrivePlayer.Casting);
		}
		Core.Logic.Penitent.SpriteRenderer.enabled = false;
		Core.Logic.Penitent.DamageArea.enabled = false;
		Core.Audio.PlaySfxOnCatalog(kneeStartId, kneeStartDelay);
		if (entityRenderer != null)
		{
			entityRenderer.flipX = Core.Logic.Penitent.Status.Orientation == EntityOrientation.Left;
		}
		interactorAnimator.SetTrigger("KNEE_START");
		penitentKneeing = true;
		Core.Events.LaunchEvent(OnAltarUse, base.name);
		while (penitentKneeing)
		{
			yield return null;
		}
		yield return new WaitForSecondsRealtime(Core.Alms.Config.InitialDelay);
		if (Core.Alms.IsMaxTier())
		{
			yield return ConfessorCourrutine();
		}
		else if (Core.Alms.GetAltarLevel() > 1)
		{
			yield return StartCoroutine(KneeledMenuCoroutine());
		}
		else
		{
			yield return StartCoroutine(UIController.instance.ShowAlmsWidgetCourrutine());
		}
		interactorAnimator.SetTrigger("KNEE_END");
		Core.Audio.PlaySfxOnCatalog(kneeEndId, kneeEndDelay);
	}

	private IEnumerator KneeledMenuCoroutine()
	{
		bool active = true;
		UIController.instance.ShowKneelMenu(KneelPopUpWidget.Modes.Altar);
		while (active)
		{
			if (UIController.instance.IsInventoryMenuPressed())
			{
				UIController.instance.MakeKneelMenuInvisible();
				yield return ConfessorCourrutine();
				active = false;
			}
			if (UIController.instance.IsTeleportMenuPressed())
			{
				UIController.instance.HideKneelMenu();
				yield return StartCoroutine(UIController.instance.ShowAlmsWidgetCourrutine());
				active = false;
			}
			if (UIController.instance.IsStopKneelPressed())
			{
				active = false;
			}
			yield return null;
		}
		UIController.instance.HideKneelMenu();
	}

	private IEnumerator ConfessorCourrutine()
	{
		int guilt = Core.GuiltManager.GetDropsCount();
		if (guilt == 0)
		{
			yield return ShowMessage("MSG_CONFESSOR_NOGUILT", 2.5f);
		}
		else
		{
			float meaCulpa = Core.Logic.Penitent.Stats.MeaCulpa.Final + 1f;
			float purgePrice = meaCulpa * PurgeBasePrice * (float)guilt;
			if (Core.Alms.GetAltarLevel() >= 6)
			{
				purgePrice = 0f;
			}
			yield return StartConversationAndWait("DLG_QT_CONFESSOR", (int)purgePrice);
			if (DialogResponse == 0)
			{
				if (Core.Logic.Penitent.Stats.Purge.Current < purgePrice)
				{
					yield return StartConversationAndWait("DLG_CONFESSOR_NOTENOUGHPURGE", 0);
				}
				else
				{
					Core.GuiltManager.ResetGuilt(restoreDropTears: true);
					Core.Logic.Penitent.Stats.Purge.Current -= purgePrice;
					yield return new WaitForSecondsRealtime(1.5f);
					Core.Audio.PlaySfx("event:/Key Event/GetCulpa");
					yield return ShowMessage("MSG_CONFESSOR_GUILTREMOVED", 3f);
				}
			}
		}
		yield return new WaitForSeconds(0.5f);
	}

	private IEnumerator ShowMessage(string Id, float time)
	{
		Core.Dialog.ShowMessage(Id, 0, string.Empty, time);
		yield return new WaitForSecondsRealtime(time);
	}

	private IEnumerator StartConversationAndWait(string Id, int purge)
	{
		int noResponse = (DialogResponse = -10);
		Core.Dialog.OnDialogFinished += DialogEnded;
		Core.Logic.Penitent.Animator.SetBool("IS_DIALOGUE_MODE", value: true);
		Core.Dialog.StartConversation(Id, modal: true, useOnlyLast: false, hideWidget: true, purge);
		while (DialogResponse == noResponse)
		{
			yield return null;
		}
		yield return null;
	}

	private void DialogEnded(string id, int response)
	{
		Core.Dialog.OnDialogFinished -= DialogEnded;
		Core.Logic.Penitent.Animator.SetBool("IS_DIALOGUE_MODE", value: false);
		DialogResponse = response;
	}

	public override PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		return CreatePersistentData<AltarPersistenceData>();
	}

	public override void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		AltarPersistenceData altarPersistenceData = (AltarPersistenceData)data;
	}
}
