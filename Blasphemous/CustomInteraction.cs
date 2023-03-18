using System.Collections;
using Framework.FrameworkCore;
using Framework.Managers;
using Sirenix.OdinInspector;
using Tools.Level;
using UnityEngine;

public class CustomInteraction : Interactable
{
	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool useOnce;

	[BoxGroup("Design Settings", true, false, 0)]
	[ReadOnly]
	protected override void OnUpdate()
	{
		if (!base.BeingUsed && base.PlayerInRange && !base.Consumed && base.InteractionTriggered)
		{
			Use();
		}
	}

	protected override IEnumerator OnUse()
	{
		interactableAnimator.SetTrigger("INTERACTED");
		interactorAnimator.SetTrigger("INTERACTED");
		if (RepositionBeforeInteract)
		{
			Core.Logic.Penitent.DrivePlayer.MoveToPosition(InteractionPosition(), orientation);
		}
		yield return new WaitForEndOfFrame();
	}

	private void Activate()
	{
	}

	private Vector2 InteractionPosition()
	{
		if (Waypoint != null)
		{
			return Waypoint.position;
		}
		Vector2 result = new Vector2(base.transform.position.x, base.transform.position.y);
		if (orientation == EntityOrientation.Right)
		{
			result.x -= 1f;
		}
		else
		{
			result.x += 1f;
		}
		return result;
	}

	protected override void InteractionEnd()
	{
		if (useOnce)
		{
			base.Consumed = true;
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
	}
}
