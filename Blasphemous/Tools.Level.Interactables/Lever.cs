using System;
using System.Collections;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Interactables;

public class Lever : Interactable
{
	private class LeverPersistenceData : PersistentManager.PersistentData
	{
		public bool AlreadyUsed;

		public LeverPersistenceData(string id)
			: base(id)
		{
		}
	}

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	protected GameObject[] target = new GameObject[0];

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool IsBlocked;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private float animationDelay;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private LeverMode mode;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private LeverAction action;

	[SerializeField]
	[BoxGroup("Event Settings", true, false, 0)]
	protected string onActivation;

	[SerializeField]
	[BoxGroup("Event Settings", true, false, 0)]
	protected string onDeactivation;

	[SerializeField]
	[BoxGroup("Event Settings", true, false, 0)]
	protected string onLocked;

	[SerializeField]
	[BoxGroup("Event Settings", true, false, 0)]
	protected string onUnlocked;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	protected string activationSound;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	protected float soundDelay;

	private bool active;

	private void Start()
	{
		base.AnimatorEvent.OnEventLaunched += OnEventLaunched;
	}

	protected override IEnumerator OnUse()
	{
		Penitent penitent = Core.Logic.Penitent;
		penitent.SpriteRenderer.enabled = false;
		penitent.DamageArea.enabled = false;
		penitent.SetOrientation(base.ObjectOrientation);
		PlayAudio(activationSound);
		interactableAnimator.SetBool("INSTANT_ANIM", value: false);
		interactableAnimator.SetBool("ACTIVE", !base.Consumed);
		interactorAnimator.SetTrigger((!base.Consumed) ? "DOWN" : "UP");
		yield return new WaitForSeconds(animationDelay);
	}

	[Button("RESET LEVER UP", ButtonSizes.Large)]
	public void SetLeverUp()
	{
		Debug.Log(base.gameObject.name + ": SET LEVER UP");
		PlayAudio(activationSound);
		interactableAnimator.SetBool("ACTIVE", value: false);
		StartCoroutine(DelayedCallback(OnLeverUpFinished, animationDelay));
	}

	[Button("SET LEVER UP INSTANTLY", ButtonSizes.Large)]
	public void SetLeverUpInstantly()
	{
		Debug.Log(base.gameObject.name + ": SET LEVER UP instant");
		interactableAnimator.Play("Up", 0, 1f);
		interactableAnimator.SetBool("ACTIVE", value: false);
		base.Consumed = false;
	}

	[Button("BLOCK LEVER DOWN", ButtonSizes.Large)]
	public void SetLeverDown()
	{
		Debug.Log(base.gameObject.name + ": SET LEVER DOWN");
		PlayAudio(activationSound);
		interactableAnimator.SetBool("ACTIVE", value: true);
		base.Consumed = true;
		StartCoroutine(DelayedCallback(OnLeverDownFinished, animationDelay));
	}

	[Button("SET LEVER DOWN INSTANTLY", ButtonSizes.Large)]
	public void SetLeverDownInstantly()
	{
		Debug.Log(base.gameObject.name + ": SET LEVER DOWN instant");
		interactableAnimator.Play("Down", 0, 1f);
		interactableAnimator.SetBool("ACTIVE", value: true);
		base.Consumed = true;
	}

	private void OnLeverDownFinished()
	{
		active = false;
		Core.Events.LaunchEvent((!active) ? onDeactivation : onActivation, string.Empty);
	}

	private void OnLeverUpFinished()
	{
		base.Consumed = false;
		active = true;
		Core.Events.LaunchEvent((!active) ? onDeactivation : onActivation, string.Empty);
	}

	protected IEnumerator DelayedCallback(Action callback, float seconds)
	{
		yield return new WaitForSeconds(seconds);
		callback();
	}

	protected override void InteractionEnd()
	{
		base.Consumed = !base.Consumed;
		Penitent penitent = Core.Logic.Penitent;
		penitent.SpriteRenderer.enabled = true;
		penitent.DamageArea.enabled = true;
		active = !active;
		Core.Events.LaunchEvent((!active) ? onDeactivation : onActivation, string.Empty);
	}

	private void OnEventLaunched(string id)
	{
		if (id.Equals("ACTIVATE"))
		{
			ActivateActionable(target);
		}
	}

	protected override void OnBlocked(bool blocked)
	{
		base.OnBlocked(blocked);
		Core.Events.LaunchEvent((!blocked) ? onUnlocked : onLocked, string.Empty);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!base.BeingUsed && base.PlayerInRange && base.InteractionTriggered && CanBeConsumed())
		{
			Use();
		}
		if (base.BeingUsed)
		{
			PlayerReposition();
		}
	}

	public override bool CanBeConsumed()
	{
		return mode == LeverMode.MultipleActivation || !base.Consumed;
	}

	protected override void PlayerReposition()
	{
		Core.Logic.Penitent.transform.position = interactableAnimator.transform.position;
	}

	private void ActivateActionable(GameObject[] gameObjects)
	{
		foreach (GameObject gameObject in gameObjects)
		{
			if (!(gameObject == null))
			{
				IActionable component = gameObject.GetComponent<IActionable>();
				if (component != null && action == LeverAction.Interact)
				{
					component.Use();
				}
				if (component != null && action == LeverAction.Unlock)
				{
					component.Locked = false;
				}
			}
		}
	}

	private void PlayAudio(string activationSoundKey)
	{
		SpriteRenderer componentInChildren = GetComponentInChildren<SpriteRenderer>();
		if ((bool)componentInChildren && componentInChildren.isVisible)
		{
			Core.Audio.PlaySfx(activationSoundKey, soundDelay);
		}
	}

	public override PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		LeverPersistenceData leverPersistenceData = CreatePersistentData<LeverPersistenceData>();
		leverPersistenceData.AlreadyUsed = base.Consumed;
		return leverPersistenceData;
	}

	public override void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		LeverPersistenceData leverPersistenceData = (LeverPersistenceData)data;
		base.Consumed = leverPersistenceData.AlreadyUsed;
		if (base.Consumed)
		{
			SetLeverDownInstantly();
		}
		else
		{
			SetLeverUpInstantly();
		}
	}

	private void OnDestroy()
	{
		base.AnimatorEvent.OnEventLaunched -= OnEventLaunched;
	}
}
