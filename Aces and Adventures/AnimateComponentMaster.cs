using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateComponentMaster : MonoBehaviour
{
	public bool useScaledTime;

	public bool delayedEnable;

	private IEnumerator _DelayedEnable()
	{
		AnimateComponent[] componentsInChildren = GetComponentsInChildren<AnimateComponent>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].useLateUpdate = false;
		}
		yield return null;
		List<AnimateComponent> disabledComponents = new List<AnimateComponent>();
		componentsInChildren = GetComponentsInChildren<AnimateComponent>(includeInactive: false);
		foreach (AnimateComponent animateComponent in componentsInChildren)
		{
			if (animateComponent.isActiveAndEnabled)
			{
				animateComponent._Reset();
				animateComponent.enabled = false;
				disabledComponents.Add(animateComponent);
			}
		}
		yield return null;
		foreach (AnimateComponent item in disabledComponents)
		{
			if ((bool)item)
			{
				item.enabled = true;
			}
		}
	}

	private void OnEnable()
	{
		if (delayedEnable)
		{
			StartCoroutine(_DelayedEnable());
		}
	}

	private void Start()
	{
		AnimateComponent[] componentsInChildren = GetComponentsInChildren<AnimateComponent>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].useScaledTime = useScaledTime;
		}
	}
}
