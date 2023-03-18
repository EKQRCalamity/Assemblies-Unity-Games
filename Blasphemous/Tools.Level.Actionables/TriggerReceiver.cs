using DG.Tweening;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Actionables;

public class TriggerReceiver : PersistentObject, IActionable
{
	private class TriggerReceiverPersistentData : PersistentManager.PersistentData
	{
		public bool used;

		public TriggerReceiverPersistentData(string id)
			: base(id)
		{
		}
	}

	[SerializeField]
	[BoxGroup("References", true, false, 0)]
	public Animator animator;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	protected GameObject[] target = new GameObject[0];

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	public string triggerID;

	[SerializeField]
	[BoxGroup("Debug settings", true, false, 0)]
	private bool showReactionTween;

	private bool alreadyUsed;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	protected string OnActivationSound;

	public bool Locked { get; set; }

	protected virtual void OnUsed()
	{
	}

	public void Use()
	{
		if (!alreadyUsed)
		{
			OnUsed();
			ActivateActionable(target);
			animator.SetTrigger("ACTIVATE");
			Core.Audio.PlayOneShot(OnActivationSound);
			alreadyUsed = true;
			DeactivateCollisions();
			if (showReactionTween)
			{
				base.transform.DOPunchScale(Vector3.one * 0.25f, 0.5f);
			}
		}
	}

	private void SetUsedAnimation()
	{
		animator.Play("USED");
		DeactivateCollisions();
	}

	private void DeactivateCollisions()
	{
		Collider2D component = GetComponent<Collider2D>();
		if (component != null)
		{
			component.enabled = false;
		}
	}

	private void ActivateActionable(GameObject[] gameObjects)
	{
		foreach (GameObject gameObject in gameObjects)
		{
			if (!(gameObject == null))
			{
				gameObject.GetComponent<IActionable>()?.Use();
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		TrapTriggererArea component = collision.gameObject.GetComponent<TrapTriggererArea>();
		if (component != null && component.triggerID == triggerID)
		{
			Use();
		}
	}

	public override PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		TriggerReceiverPersistentData triggerReceiverPersistentData = CreatePersistentData<TriggerReceiverPersistentData>();
		triggerReceiverPersistentData.used = alreadyUsed;
		return triggerReceiverPersistentData;
	}

	public override void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		TriggerReceiverPersistentData triggerReceiverPersistentData = (TriggerReceiverPersistentData)data;
		alreadyUsed = triggerReceiverPersistentData.used;
		if (alreadyUsed)
		{
			SetUsedAnimation();
		}
	}
}
