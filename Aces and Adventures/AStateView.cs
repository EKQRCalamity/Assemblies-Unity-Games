using System.Collections.Generic;
using UnityEngine;

public abstract class AStateView : MonoBehaviour
{
	public BoolEvent onEnableChange;

	public BoolEvent onDisableChange;

	public List<GameObject> gameObjectsToActivateWhileEnabled;

	public List<GameObject> gameObjectsToDeactivateWhileEnabled;

	public PointerClick3D pointerClick => GetComponentInChildren<PointerClick3D>();

	public PointerOver3D pointerOver => GetComponentInChildren<PointerOver3D>();

	protected virtual void OnEnable()
	{
		foreach (GameObject item in gameObjectsToActivateWhileEnabled)
		{
			item.SetActive(value: true);
		}
		foreach (GameObject item2 in gameObjectsToDeactivateWhileEnabled)
		{
			item2.SetActive(value: false);
		}
		onEnableChange?.Invoke(arg0: true);
		onDisableChange?.Invoke(arg0: false);
	}

	protected virtual void OnDisable()
	{
		foreach (GameObject item in gameObjectsToActivateWhileEnabled)
		{
			item.SetActive(value: false);
		}
		foreach (GameObject item2 in gameObjectsToDeactivateWhileEnabled)
		{
			item2.SetActive(value: true);
		}
		onEnableChange?.Invoke(arg0: false);
		onDisableChange?.Invoke(arg0: true);
	}
}
