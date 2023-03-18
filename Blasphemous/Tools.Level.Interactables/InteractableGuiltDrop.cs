using System.Collections.Generic;
using Framework.Managers;
using Gameplay.GameControllers.Penitent.Effects;
using Gameplay.UI;
using I2.Loc;
using UnityEngine;

namespace Tools.Level.Interactables;

public class InteractableGuiltDrop : MonoBehaviour
{
	public string sound = "event:/Key Event/UseQuestItem";

	public float timeToWait = 2f;

	private string dropId = string.Empty;

	private List<Interactable> overlappedInteractables = new List<Interactable>();

	private CollectibleItem guiltDropCollective;

	public void SetDropId(string dropid)
	{
		dropId = dropid;
	}

	private void Start()
	{
		guiltDropCollective = GetComponent<CollectibleItem>();
		SetOverlappedInteractable();
	}

	private void Update()
	{
		for (int i = 0; i < overlappedInteractables.Count; i++)
		{
			overlappedInteractables[i].OverlappedInteractor = true;
		}
	}

	private void SetOverlappedInteractable()
	{
		Interactable[] array = Object.FindObjectsOfType<Interactable>();
		foreach (Interactable interactable in array)
		{
			if (!interactable.Equals(guiltDropCollective) && Vector2.Distance(interactable.transform.position, base.transform.position) < 1f)
			{
				interactable.OverlappedInteractor = true;
				overlappedInteractables.Add(interactable);
			}
		}
	}

	private void ReleaseOverlappedDoors()
	{
		if (overlappedInteractables.Count > 0)
		{
			for (int i = 0; i < overlappedInteractables.Count; i++)
			{
				overlappedInteractables[i].OverlappedInteractor = false;
			}
			overlappedInteractables.Clear();
		}
	}

	private void OnUsePost()
	{
		if (!Core.Logic.Penitent.Dead)
		{
			Core.GuiltManager.OnDropTaken(dropId);
			UIController.instance.ShowPopUp(ScriptLocalization.UI.GET_GUILTDROP_TEXT, sound, timeToWait, blockPlayer: false);
			GuiltDropRecover componentInChildren = Core.Logic.Penitent.GetComponentInChildren<GuiltDropRecover>();
			if ((bool)componentInChildren)
			{
				componentInChildren.TriggerEffect();
			}
			ReleaseOverlappedDoors();
		}
	}
}
