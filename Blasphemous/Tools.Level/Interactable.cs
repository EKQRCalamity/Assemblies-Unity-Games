using System.Collections;
using Framework.FrameworkCore;
using Framework.Inventory;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Rewired;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Tools.UI;
using UnityEngine;

namespace Tools.Level;

[SelectionBase]
public class Interactable : PersistentObject, IActionable
{
	protected class InteractablePersistence : PersistentManager.PersistentData
	{
		public bool Consumed;

		public InteractablePersistence(string id)
			: base(id)
		{
		}
	}

	[SerializeField]
	[BoxGroup("Interaction Reposition", true, false, 0)]
	protected bool RepositionBeforeInteract = true;

	[SerializeField]
	[BoxGroup("Interaction Reposition", true, false, 0)]
	[ShowIf("RepositionBeforeInteract", true)]
	protected Transform Waypoint;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	protected bool needObject;

	[ShowIf("needObject", true)]
	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	protected InventoryObjectInspector requiredItem;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	protected bool interactableWhileJumping;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	protected EntityOrientation orientation;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 10)]
	protected CollisionSensor[] sensors;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 10)]
	protected Animator interactableAnimator;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 10)]
	protected Animator interactorAnimator;

	private bool consumed;

	private bool blocked;

	protected AnimatorEvent AnimatorEvent { get; private set; }

	public bool OverlappedInteractor { get; set; }

	public bool BeingUsed { get; private set; }

	public EntityOrientation ObjectOrientation
	{
		get
		{
			return orientation;
		}
		set
		{
			orientation = value;
			RefreshOrientaiton();
		}
	}

	public bool Locked
	{
		get
		{
			OnBlocked(blocked);
			return blocked;
		}
		set
		{
			blocked = value;
			if (blocked && this.OnLocked != null)
			{
				this.OnLocked();
			}
			if (blocked && Interactable.SLocked != null)
			{
				Interactable.SLocked(this);
			}
			if (!blocked && this.OnUnlocked != null)
			{
				this.OnUnlocked();
			}
			if (blocked && Interactable.SUnlocked != null)
			{
				Interactable.SUnlocked(this);
			}
		}
	}

	public EntityOrientation PlayerDirection => (base.transform.InverseTransformPoint(Core.Logic.Penitent.transform.position).x < 0f) ? EntityOrientation.Left : EntityOrientation.Right;

	public bool InteractionTriggered => ReInput.isReady && ReInput.players.GetPlayer(0).GetButtonDown(8) && (!Core.Logic.Penitent.IsJumping || interactableWhileJumping) && !Core.Logic.Penitent.IsGrabbingCliffLede && !Core.Input.InputBlocked && !OverlappedInteractor;

	public bool PlayerInRange { get; set; }

	public bool Consumed
	{
		get
		{
			return consumed;
		}
		set
		{
			if (consumed != value)
			{
				if (consumed && Interactable.SConsumed != null)
				{
					Interactable.SConsumed(this);
				}
				consumed = value;
			}
		}
	}

	public static event Core.InteractableEvent SConsumed;

	public static event Core.InteractableEvent Created;

	public static event Core.InteractableEvent SPenitentExit;

	public static event Core.InteractableEvent SPenitentEnter;

	public static event Core.InteractableEvent SLocked;

	public static event Core.InteractableEvent SUnlocked;

	public static event Core.InteractableEvent SInteractionStarted;

	public static event Core.InteractableEvent SInteractionEnded;

	public event Core.SimpleEvent OnStartUsing;

	public event Core.SimpleEvent OnStopUsing;

	public event Core.SimpleEvent OnLocked;

	public event Core.SimpleEvent OnUnlocked;

	public virtual bool CanBeConsumed()
	{
		return !Consumed;
	}

	public virtual bool AllwaysShowIcon()
	{
		return false;
	}

	public void Use()
	{
		StartCoroutine(UseCorroutine());
	}

	public void UseEvenIfInputBlocked()
	{
		StartCoroutine(UseEvenIfInputBlockedCorroutine());
	}

	protected virtual void InteractionStart()
	{
	}

	protected virtual void InteractionEnd()
	{
	}

	protected virtual IEnumerator OnUse()
	{
		yield return 0;
	}

	protected virtual IEnumerator UseCorroutine()
	{
		if (!Core.Input.InputBlocked && !Locked)
		{
			if (RepositionBeforeInteract)
			{
				Core.Logic.Penitent.DrivePlayer.OnStopMotion += OnStopReposition;
			}
			if (this.OnStartUsing != null)
			{
				this.OnStartUsing();
			}
			if (Interactable.SInteractionStarted != null && !RepositionBeforeInteract)
			{
				Interactable.SInteractionStarted(this);
			}
			Log.Trace("Interactable", "Starting using: " + base.name);
			base.gameObject.SendMessage("OnUsePre", SendMessageOptions.DontRequireReceiver);
			yield return StartCoroutine(OnUse());
			base.gameObject.SendMessage("OnUsePost", SendMessageOptions.DontRequireReceiver);
			Log.Trace("Interactable", "Finished using: " + base.name);
			if (this.OnStopUsing != null)
			{
				this.OnStopUsing();
			}
			if (Interactable.SInteractionEnded != null)
			{
				Interactable.SInteractionEnded(this);
			}
			yield return null;
		}
	}

	protected virtual IEnumerator UseEvenIfInputBlockedCorroutine()
	{
		if (!Locked)
		{
			if (RepositionBeforeInteract)
			{
				Core.Logic.Penitent.DrivePlayer.OnStopMotion += OnStopReposition;
			}
			if (this.OnStartUsing != null)
			{
				this.OnStartUsing();
			}
			if (Interactable.SInteractionStarted != null && !RepositionBeforeInteract)
			{
				Interactable.SInteractionStarted(this);
			}
			Log.Trace("Interactable", "Starting using: " + base.name);
			base.gameObject.SendMessage("OnUsePre", SendMessageOptions.DontRequireReceiver);
			yield return StartCoroutine(OnUse());
			base.gameObject.SendMessage("OnUsePost", SendMessageOptions.DontRequireReceiver);
			Log.Trace("Interactable", "Finished using: " + base.name);
			if (this.OnStopUsing != null)
			{
				this.OnStopUsing();
			}
			if (Interactable.SInteractionEnded != null)
			{
				Interactable.SInteractionEnded(this);
			}
			yield return null;
		}
	}

	private void OnStopReposition()
	{
		if (Interactable.SInteractionStarted != null)
		{
			Interactable.SInteractionStarted(this);
		}
		Core.Logic.Penitent.DrivePlayer.OnStopMotion -= OnStopReposition;
	}

	protected virtual void OnPlayerReady()
	{
	}

	protected virtual void OnAwake()
	{
	}

	protected virtual void OnDispose()
	{
	}

	protected virtual void OnStart()
	{
	}

	protected virtual void ObjectEnable()
	{
	}

	protected virtual void ObjectDisable()
	{
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void OnEditorValidate()
	{
	}

	protected virtual void TriggerEnter(Entity entity)
	{
	}

	protected virtual void TriggerExit(Entity entity)
	{
	}

	protected virtual void OnBlocked(bool blocked)
	{
	}

	protected virtual void PlayerReposition()
	{
		Core.Logic.Penitent.transform.position = interactorAnimator.transform.position;
	}

	private void OnPenitentReady(Penitent penitent)
	{
		OnPlayerReady();
		SpawnManager.OnPlayerSpawn -= OnPenitentReady;
	}

	private void Awake()
	{
		SpawnManager.OnPlayerSpawn += OnPenitentReady;
		for (int i = 0; i < sensors.Length; i++)
		{
			if (!(sensors[i] == null))
			{
				sensors[i].OnEntityEnter += OnEntityEnter;
				sensors[i].OnEntityExit += OnEntityExit;
				sensors[i].SensorTriggerStay += SensorTriggerStay;
			}
		}
		CheckAnimationEvents();
		OnAwake();
		if (Interactable.Created != null)
		{
			Interactable.Created(this);
		}
	}

	protected void CheckAnimationEvents()
	{
		if (AnimatorEvent != null)
		{
			AnimatorEvent.OnEventLaunched -= OnEventLaunched;
		}
		if (interactorAnimator != null)
		{
			AnimatorEvent = interactorAnimator.GetComponent<AnimatorEvent>();
		}
		if (AnimatorEvent != null)
		{
			AnimatorEvent.OnEventLaunched += OnEventLaunched;
		}
	}

	private void OnDestroy()
	{
		SpawnManager.OnPlayerSpawn -= OnPenitentReady;
		for (int i = 0; i < sensors.Length; i++)
		{
			if (!(sensors[i] == null))
			{
				sensors[i].OnEntityEnter -= OnEntityEnter;
				sensors[i].OnEntityExit -= OnEntityExit;
				sensors[i].SensorTriggerStay -= SensorTriggerStay;
			}
		}
		if (AnimatorEvent != null)
		{
			AnimatorEvent.OnEventLaunched -= OnEventLaunched;
		}
		if (RepositionBeforeInteract && (bool)Core.Logic.Penitent)
		{
			Core.Logic.Penitent.DrivePlayer.OnStopMotion -= OnStopReposition;
		}
		OnDispose();
	}

	private void OnEventLaunched(string id)
	{
		if (id == "INTERACTION_START")
		{
			BeingUsed = true;
			Core.Input.SetBlocker("INTERACTABLE", blocking: true);
			InteractionStart();
		}
		else if (id == "INTERACTION_END")
		{
			BeingUsed = false;
			Core.Input.SetBlocker("INTERACTABLE", blocking: false);
			InteractionEnd();
		}
	}

	private void Start()
	{
		OnStart();
	}

	private void Update()
	{
		OnUpdate();
	}

	private void OnEnable()
	{
		ObjectEnable();
	}

	private void OnDisable()
	{
		ObjectDisable();
	}

	private void OnEntityEnter(Entity entity)
	{
		Locked = !HasRequiredItem();
		if (entity.CompareTag("Penitent"))
		{
			PlayerInRange = true;
			if (Interactable.SPenitentEnter != null)
			{
				Interactable.SPenitentEnter(this);
			}
		}
		TriggerEnter(entity);
	}

	private void OnEntityExit(Entity entity)
	{
		if (entity.CompareTag("Penitent"))
		{
			PlayerInRange = false;
			if (Interactable.SPenitentExit != null)
			{
				Interactable.SPenitentExit(this);
			}
		}
		TriggerExit(entity);
	}

	private void SensorTriggerStay(Collider2D col)
	{
		if (col.CompareTag("Penitent"))
		{
			PlayerInRange = true;
		}
	}

	private bool HasRequiredItem()
	{
		if (needObject && requiredItem != null && !requiredItem.id.IsNullOrWhitespace())
		{
			BaseInventoryObject invObject = requiredItem.GetInvObject();
			return Core.InventoryManager.IsBaseObjectEquipped(invObject);
		}
		return true;
	}

	public void EnableInputIcon(bool enable)
	{
		InputNotifier componentInChildren = base.gameObject.GetComponentInChildren<InputNotifier>();
		if (componentInChildren != null)
		{
			componentInChildren.enabled = enable;
		}
	}

	protected void RefreshOrientaiton()
	{
		if (!(interactableAnimator == null))
		{
			SpriteRenderer[] componentsInChildren = interactableAnimator.GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].flipX = orientation == EntityOrientation.Left;
			}
		}
	}

	protected virtual void ShowPlayer(bool show)
	{
		Core.Logic.Penitent.SpriteRenderer.enabled = show;
		Core.Logic.Penitent.DamageArea.enabled = show;
	}

	private void OnValidate()
	{
		RefreshOrientaiton();
		OnEditorValidate();
	}
}
