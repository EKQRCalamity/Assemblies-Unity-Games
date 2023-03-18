using System.Collections;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using UnityEngine;

namespace Tools.Level.Interactables;

public class GuiltDropCollectibleItem : CollectibleItem
{
	public float VanishAnimDuration = 0.45f;

	[EventRef]
	public string VanishingGuiltFx;

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

	protected override void OnAwake()
	{
		base.OnAwake();
		animationEvent = interactorAnimator.GetComponent<AnimatorEvent>();
	}

	protected override void OnUpdate()
	{
		if (!base.BeingUsed && base.PlayerInRange && !base.Consumed && base.InteractionTriggered)
		{
			Use();
		}
		if (base.BeingUsed)
		{
			PlayerReposition();
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

	protected override IEnumerator UseCorroutine()
	{
		InteractionStart();
		yield return new WaitForSeconds(VanishAnimDuration);
		InteractionEnd();
	}

	protected override void InteractionStart()
	{
		base.InteractionStart();
		base.gameObject.SendMessage("OnUsePre", SendMessageOptions.DontRequireReceiver);
		interactableAnimator.SetTrigger("TAKE");
		PlayVanishGuilt();
	}

	protected override void InteractionEnd()
	{
		base.Consumed = true;
		base.gameObject.SendMessage("OnUsePost", SendMessageOptions.DontRequireReceiver);
		interactableAnimator.transform.parent.gameObject.SetActive(value: false);
	}

	private void PlayVanishGuilt()
	{
		if (!string.IsNullOrEmpty(VanishingGuiltFx))
		{
			Core.Audio.PlaySfx(VanishingGuiltFx);
		}
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
