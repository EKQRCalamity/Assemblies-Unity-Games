using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Sirenix.OdinInspector;
using Tools.Gameplay;
using UnityEngine;

namespace Tools.Level.Actionables;

public class Gate : PersistentObject, IActionable
{
	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private Animator animator;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private Collider2D collision;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool startOpen;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool persistState;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[EventRef]
	private string openSound;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[EventRef]
	private string closeSound;

	private bool open;

	private static readonly int InstaAction = Animator.StringToHash("INSTA_ACTION");

	private static readonly int OpenParam = Animator.StringToHash("OPEN");

	private SpriteRenderer SpriteRenderer { get; set; }

	public bool Locked { get; set; }

	private void Awake()
	{
		open = false;
		SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
		if (startOpen)
		{
			Open(doOpen: true, playAudio: false, instaAction: true);
		}
	}

	private void Open(bool doOpen, bool playAudio = true, bool instaAction = false)
	{
		if ((bool)animator)
		{
			animator.SetBool(InstaAction, instaAction);
			animator.SetBool(OpenParam, doOpen);
			collision.enabled = !doOpen;
			open = doOpen;
			if (playAudio && !instaAction)
			{
				PlayAudio(open);
			}
		}
	}

	public void Use()
	{
		Open(!open);
	}

	public override bool IsOpenOrActivated()
	{
		return open;
	}

	public override PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		if (!persistState)
		{
			return null;
		}
		BasicPersistence basicPersistence = CreatePersistentData<BasicPersistence>();
		basicPersistence.triggered = open;
		return basicPersistence;
	}

	public override void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		if (persistState)
		{
			BasicPersistence basicPersistence = (BasicPersistence)data;
			open = basicPersistence.triggered;
			Open(open, playAudio: false, instaAction: true);
		}
	}

	private void PlayAudio(bool isOpen)
	{
		if ((bool)SpriteRenderer && SpriteRenderer.isVisible)
		{
			Core.Audio.PlaySfx((!isOpen) ? closeSound : openSound);
		}
	}
}
