using System.Collections;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Interactables;

public class Chest : Interactable
{
	private class ChestPersistenceData : PersistentManager.PersistentData
	{
		public bool AlreadyUsed;

		public ChestPersistenceData(string id)
			: base(id)
		{
		}
	}

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private ChestMode mode;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	private string activationSound;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private float soundDelay;

	protected override IEnumerator OnUse()
	{
		ShowPlayer(show: false);
		Core.Audio.PlaySfx(activationSound, soundDelay);
		interactableAnimator.SetBool("USED", value: true);
		interactorAnimator.SetTrigger("USED");
		Core.Logic.Penitent.SetOrientation(base.ObjectOrientation);
		while (!base.Consumed)
		{
			yield return new WaitForEndOfFrame();
		}
		ShowPlayer(show: true);
	}

	protected override void OnUpdate()
	{
		if (!base.BeingUsed && mode == ChestMode.Interactable && base.PlayerInRange && !base.Consumed && base.InteractionTriggered)
		{
			Use();
		}
		if (base.BeingUsed)
		{
			PlayerReposition();
		}
	}

	protected override void InteractionEnd()
	{
		base.Consumed = true;
	}

	public override PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		Log.Trace("Chest", "Getting current persistence.");
		ChestPersistenceData chestPersistenceData = CreatePersistentData<ChestPersistenceData>();
		chestPersistenceData.AlreadyUsed = base.Consumed;
		return chestPersistenceData;
	}

	public override void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		Log.Trace("Chest", "Setting current persistence.");
		ChestPersistenceData chestPersistenceData = (ChestPersistenceData)data;
		base.Consumed = chestPersistenceData.AlreadyUsed;
		interactableAnimator.SetBool("NOANIMUSED", base.Consumed);
	}
}
