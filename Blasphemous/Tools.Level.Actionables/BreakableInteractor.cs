using System;
using DG.Tweening;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Actionables;

[RequireComponent(typeof(BreakableObject))]
public class BreakableInteractor : Interactable
{
	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	protected GameObject[] InteractionTargets;

	[Tooltip("A Lapse before interaction starts")]
	public float InteractionTimeout;

	private BreakableObject BreakableTrigger;

	protected override void OnAwake()
	{
		base.OnAwake();
		BreakableTrigger = GetComponent<BreakableObject>();
		BreakableObject breakableTrigger = BreakableTrigger;
		breakableTrigger.OnBreak = (Core.SimpleEvent)Delegate.Combine(breakableTrigger.OnBreak, new Core.SimpleEvent(OnBroken));
	}

	private void OnBroken()
	{
		DOTween.Sequence().SetDelay(InteractionTimeout).OnComplete(delegate
		{
			ActivateActionable(InteractionTargets);
		});
	}

	private void ActivateActionable(GameObject[] gameObjects)
	{
		foreach (GameObject gameObject in gameObjects)
		{
			if ((bool)gameObject)
			{
				gameObject.GetComponent<IActionable>()?.Use();
			}
		}
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		BreakableObject breakableTrigger = BreakableTrigger;
		breakableTrigger.OnBreak = (Core.SimpleEvent)Delegate.Remove(breakableTrigger.OnBreak, new Core.SimpleEvent(OnBroken));
	}
}
