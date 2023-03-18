using System.Collections;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.UI;
using Gameplay.UI.Others.MenuLogic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Interactables;

[SelectionBase]
public class PrieDieu : Interactable
{
	private class PrieDieuPersistenceData : PersistentManager.PersistentData
	{
		public bool lighted;

		public PrieDieuPersistenceData(string id)
			: base(id)
		{
		}
	}

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	[Required]
	private SpriteRenderer entityRenderer;

	[SerializeField]
	[BoxGroup("Advanced Settings", true, false, 0)]
	private float initialDeadLapse;

	[SerializeField]
	[BoxGroup("Advanced Settings", true, false, 0)]
	private float initialDelay;

	[SerializeField]
	[BoxGroup("Advanced Settings", true, false, 0)]
	private float timeToPrayerMenu;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	public EntityOrientation spawnOrientation;

	[SerializeField]
	[BoxGroup("Event Settings", true, false, 0)]
	[ReadOnly]
	private string GenericFirstUse = "PRIEDIEU_ACTIVATED";

	[SerializeField]
	[BoxGroup("Event Settings", true, false, 0)]
	private string OnFirstUse;

	[SerializeField]
	[BoxGroup("Event Settings", true, false, 0)]
	private string OnReuse;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private string activationId;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private string kneeStartId;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private string kneeEndId;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private float activationDelay;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private float kneeStartDelay;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private float kneeEndDelay;

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
	[BoxGroup("Attrack Mode", true, false, 0)]
	private bool gotoAttrackWhenActivate;

	private bool penitentKneeing;

	private bool ligthed;

	public bool Ligthed
	{
		get
		{
			return ligthed;
		}
		set
		{
			interactableAnimator.SetBool("ACTIVE", value);
			ligthed = value;
		}
	}

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
		switch (Core.Alms.GetPrieDieuLevel())
		{
		case 1:
			gameObject = interactableAnimatorLevel1;
			break;
		case 2:
			gameObject = interactableAnimatorLevel2;
			break;
		case 3:
			gameObject = interactableAnimatorLevel3;
			break;
		}
		gameObject.SetActive(value: true);
		interactableAnimator = gameObject.GetComponent<Animator>();
		CheckAnimationEvents();
	}

	protected override IEnumerator OnUse()
	{
		if (Ligthed)
		{
			yield return ReActivationLogic();
		}
		else
		{
			yield return ActivationLogic();
		}
		Core.Logic.UsePrieDieu();
		Core.Logic.BreakableManager.Reset();
	}

	protected override void InteractionEnd()
	{
		Core.Logic.Penitent.SpriteRenderer.enabled = true;
		Core.Logic.Penitent.DamageArea.enabled = true;
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

	protected override void OnUpdate()
	{
		if (!base.BeingUsed && base.PlayerInRange && base.InteractionTriggered)
		{
			Use();
		}
		else if (!base.BeingUsed && base.PlayerInRange && !Ligthed && base.InteractionTriggered)
		{
			Use();
		}
		if (base.BeingUsed)
		{
			PlayerReposition();
		}
	}

	private IEnumerator ActivationLogic()
	{
		Core.Metrics.CustomEvent("PRIEDIEU_ACTIVATED", base.name);
		Core.Audio.PlaySfxOnCatalog(activationId, activationDelay);
		LogicManager coreLogic = Core.Logic;
		coreLogic.Penitent.SpriteRenderer.enabled = false;
		coreLogic.Penitent.DamageArea.enabled = false;
		if (entityRenderer != null)
		{
			entityRenderer.flipX = Core.Logic.Penitent.Status.Orientation == EntityOrientation.Left;
		}
		interactorAnimator.SetTrigger("ACTIVATION");
		yield return new WaitForSeconds(initialDelay);
		ligthed = true;
		Core.Events.LaunchEvent(GenericFirstUse, base.name);
		Core.Events.LaunchEvent(OnFirstUse, string.Empty);
		ShallowActivationLogic();
		if (gotoAttrackWhenActivate)
		{
			Core.Logic.LoadAttrackScene();
		}
		Core.Logic.EnemySpawner.RespawnDeadEnemies();
	}

	private void ShallowActivationLogic()
	{
		Core.SpawnManager.ActivePrieDieu = this;
		Core.Logic.Penitent.Stats.Life.SetToCurrentMax();
		Core.Logic.Penitent.Stats.Flask.SetToCurrentMax();
		if (Core.Alms.GetPrieDieuLevel() > 1)
		{
			Core.Logic.Penitent.Stats.Fervour.SetToCurrentMax();
		}
		Core.Persistence.SaveGame();
	}

	private IEnumerator ReActivationLogic()
	{
		ShallowActivationLogic();
		LogicManager coreLogic = Core.Logic;
		Core.Audio.PlaySfxOnCatalog(kneeStartId, kneeStartDelay);
		coreLogic.Penitent.SpriteRenderer.enabled = false;
		coreLogic.Penitent.DamageArea.enabled = false;
		if (entityRenderer != null)
		{
			entityRenderer.flipX = Core.Logic.Penitent.Status.Orientation == EntityOrientation.Left;
		}
		interactorAnimator.SetTrigger("KNEE_START");
		penitentKneeing = true;
		Core.Events.LaunchEvent(OnReuse, string.Empty);
		while (penitentKneeing)
		{
			yield return 0;
		}
		Core.Logic.EnemySpawner.RespawnDeadEnemies();
		bool canUseInventory = HaveAnySwordHearts();
		bool canUseTeleport = Core.Alms.GetPrieDieuLevel() >= 3;
		if (canUseInventory || canUseTeleport)
		{
			yield return StartCoroutine(KneeledMenuCoroutine(canUseInventory, canUseTeleport));
		}
		interactorAnimator.SetTrigger("KNEE_END");
		Core.Audio.PlaySfxOnCatalog(kneeEndId, kneeEndDelay);
	}

	private bool HaveAnySwordHearts()
	{
		return Core.InventoryManager.GetSwordsOwned().Count > 0;
	}

	private IEnumerator KneeledMenuCoroutine(bool canUseInventory, bool canUseTeleport)
	{
		bool active = true;
		bool shownInventory = false;
		KneelPopUpWidget.Modes mode = KneelPopUpWidget.Modes.PrieDieu_all;
		if (!canUseInventory)
		{
			mode = KneelPopUpWidget.Modes.PrieDieu_teleport;
		}
		else if (!canUseTeleport)
		{
			mode = KneelPopUpWidget.Modes.PrieDieu_sword;
		}
		UIController.instance.ShowKneelMenu(mode);
		Debug.Log("<color=magenta>KNEEL MENU</color>");
		while (active)
		{
			if (canUseInventory && UIController.instance.IsInventoryMenuPressed())
			{
				if (!shownInventory)
				{
					Debug.Log("<color=magenta>OPEN INVENTORY</color>");
					UIController.instance.SelectTab(NewInventoryWidget.TabType.Sword);
					UIController.instance.ToggleInventoryMenu();
					UIController.instance.MakeKneelMenuInvisible();
					shownInventory = true;
					yield return new WaitForSeconds(0.5f);
				}
				else
				{
					Debug.Log("<color=magenta>CLOSING INVENTORY</color>");
					UIController.instance.ToggleInventoryMenu();
				}
			}
			if (shownInventory && UIController.instance.IsInventoryClosed())
			{
				Debug.Log("<color=magenta>INVENTORY IS CLOSED, CLOSING KNEEL MENU</color>");
				active = false;
			}
			else if (!shownInventory && UIController.instance.IsStopKneelPressed())
			{
				Debug.Log("<color=magenta>CLOSING KNEEL MENU DIRECTLY</color>");
				active = false;
			}
			else if (canUseTeleport && !shownInventory && UIController.instance.IsTeleportMenuPressed())
			{
				UIController.instance.HideKneelMenu();
				active = false;
				yield return StartCoroutine(UIController.instance.ShowMapTeleport());
			}
			yield return null;
		}
		UIController.instance.HideKneelMenu();
	}

	protected override void PlayerReposition()
	{
		Core.Logic.Penitent.transform.position = interactorAnimator.transform.position;
	}

	public void ShallowUse()
	{
		base.gameObject.SendMessage("OnUsePre", SendMessageOptions.DontRequireReceiver);
		ShallowActivationLogic();
		base.gameObject.SendMessage("OnUsePost", SendMessageOptions.DontRequireReceiver);
	}

	public override bool IsOpenOrActivated()
	{
		return Ligthed;
	}

	public override PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		PrieDieuPersistenceData prieDieuPersistenceData = CreatePersistentData<PrieDieuPersistenceData>();
		prieDieuPersistenceData.lighted = Ligthed;
		return prieDieuPersistenceData;
	}

	public override void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		PrieDieuPersistenceData prieDieuPersistenceData = (PrieDieuPersistenceData)data;
		Ligthed = prieDieuPersistenceData.lighted;
	}
}
