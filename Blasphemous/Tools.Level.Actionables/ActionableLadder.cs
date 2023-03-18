using System.Collections;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Sirenix.OdinInspector;
using Tools.Gameplay;
using UnityEngine;

namespace Tools.Level.Actionables;

public class ActionableLadder : PersistentObject, IActionable
{
	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private Animator animator;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private BoxCollider2D collision;

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

	public TileableBeamLauncher tileableLadder;

	public float maxSeconds = 1f;

	private bool open;

	public float maxRange = 15f;

	public bool Locked { get; set; }

	private void Awake()
	{
		open = false;
		Open(startOpen, playAudio: false, automatic: true);
	}

	private void Open(bool b, bool playAudio = true, bool automatic = false)
	{
		open = b;
		if (open)
		{
			StartGrow((!automatic) ? maxSeconds : 0f);
		}
		else
		{
			tileableLadder.maxRange = 0f;
		}
		if (playAudio)
		{
			Core.Audio.PlaySfx((!open) ? closeSound : openSound);
		}
	}

	private IEnumerator GrowCoroutine(float maxLength, float seconds)
	{
		float counter = 0f;
		while (counter < seconds)
		{
			float i = Mathf.Lerp(0f, maxLength, counter / seconds);
			tileableLadder.maxRange = i;
			counter += Time.deltaTime;
			yield return null;
		}
		tileableLadder.maxRange = maxLength;
		collision.enabled = true;
		collision.size += new Vector2(0f, maxLength);
		collision.offset -= new Vector2(0f, maxLength / 2f);
	}

	private void StartGrow(float seconds)
	{
		StartCoroutine(GrowCoroutine(maxRange, seconds));
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
			Open(open, playAudio: false, automatic: true);
		}
	}
}
