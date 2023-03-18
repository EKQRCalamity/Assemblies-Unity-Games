using System;
using System.Collections;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Sirenix.OdinInspector;
using Tools.Level;
using UnityEngine;

public class CollectibleItem : Interactable
{
	public enum CollectibleHeight
	{
		Halfheight,
		Floor
	}

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	protected CollectibleHeight height;

	protected AnimatorEvent animationEvent;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	private string collectItemSound;

	private string Animation
	{
		get
		{
			if (height == CollectibleHeight.Floor)
			{
				return "Floor Collection";
			}
			if (height == CollectibleHeight.Halfheight)
			{
				return "Halfheight Collection";
			}
			Log.Error("Collectible", "Error selecting animation.");
			return string.Empty;
		}
	}

	protected override void OnStart()
	{
		try
		{
			animationEvent = interactorAnimator.GetComponent<AnimatorEvent>();
			animationEvent.OnEventLaunched += OnEventLaunched;
		}
		catch (NullReferenceException)
		{
			Log.Error("Collectible", "Missing references on collectible. Disabling item.");
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnEventLaunched(string id)
	{
		if (id.Equals("REMOVE_ITEM") && interactableAnimator != null)
		{
			interactableAnimator.gameObject.SetActive(value: false);
		}
		if (id.Equals("INTERACTION_START"))
		{
			PlayCollectSound();
		}
	}

	protected override void OnUpdate()
	{
		if (!base.BeingUsed && base.PlayerInRange && !base.Consumed && base.InteractionTriggered)
		{
			Core.Logic.Penitent.IsPickingCollectibleItem = true;
			Use();
		}
		if (base.BeingUsed)
		{
			PlayerReposition();
		}
	}

	public void PlayCollectSound()
	{
		if (!string.IsNullOrEmpty(collectItemSound))
		{
			Core.Audio.PlaySfx(collectItemSound);
		}
	}

	protected override IEnumerator OnUse()
	{
		ShowPlayer(show: false);
		if ((bool)interactorAnimator)
		{
			interactorAnimator.Play(Animation);
		}
		yield return new WaitForEndOfFrame();
		while (!base.Consumed)
		{
			yield return new WaitForEndOfFrame();
		}
		ShowPlayer(show: true);
	}

	protected override void InteractionEnd()
	{
		base.Consumed = true;
		Core.Logic.Penitent.IsPickingCollectibleItem = false;
	}

	public override PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		InteractablePersistence interactablePersistence = CreatePersistentData<InteractablePersistence>();
		interactablePersistence.Consumed = base.Consumed;
		return interactablePersistence;
	}

	public override void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		InteractablePersistence interactablePersistence = (InteractablePersistence)data;
		base.Consumed = interactablePersistence.Consumed;
		if (base.Consumed)
		{
			interactableAnimator.gameObject.SetActive(value: false);
		}
	}
}
