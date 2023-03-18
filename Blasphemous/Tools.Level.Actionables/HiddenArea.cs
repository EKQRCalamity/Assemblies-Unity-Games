using DG.Tweening;
using DG.Tweening.Core.Surrogates;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Sirenix.OdinInspector;
using Tools.Gameplay;
using UnityEngine;

namespace Tools.Level.Actionables;

[SelectionBase]
[RequireComponent(typeof(UniqueId))]
public class HiddenArea : PersistentObject, IActionable
{
	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private float disappearTime = 1f;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool triggerOnPlayerEnter;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	private string disappearSound;

	private bool triggered;

	public static Core.SimpleEvent OnUse;

	public bool Locked { get; set; }

	public void Use()
	{
		Core.Metrics.CustomEvent("SECRET_DISCOVERED", base.name);
		Core.Audio.PlaySfx(disappearSound);
		FadeRenderers(disappearTime);
		if (OnUse != null)
		{
			OnUse();
		}
	}

	public void PreviouslyUsed()
	{
		FadeRenderers(disappearTime);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Penitent"))
		{
			Use();
		}
	}

	private void FadeRenderers(float time)
	{
		triggered = true;
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		Collider2D[] componentsInChildren2 = GetComponentsInChildren<Collider2D>();
		foreach (SpriteRenderer rend in componentsInChildren)
		{
			DOTween.To(() => rend.color, delegate(ColorWrapper x)
			{
				rend.color = x;
			}, new Color(rend.color.r, rend.color.g, rend.color.b, 0f), time);
		}
		Collider2D[] array = componentsInChildren2;
		foreach (Collider2D collider2D in array)
		{
			collider2D.enabled = false;
		}
	}

	public override PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		BasicPersistence basicPersistence = CreatePersistentData<BasicPersistence>();
		basicPersistence.triggered = triggered;
		return basicPersistence;
	}

	public override void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		BasicPersistence basicPersistence = (BasicPersistence)data;
		if (basicPersistence.triggered)
		{
			PreviouslyUsed();
		}
	}
}
