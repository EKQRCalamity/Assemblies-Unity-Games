using System.Collections.Generic;
using Gameplay.UI.Others.MenuLogic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay.UI.Others;

public class KeepFocus : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup checkGroup;

	[SerializeField]
	private List<GameObject> allowedObjects = new List<GameObject>();

	[SerializeField]
	private GameObject firstSelected;

	[SerializeField]
	private BasicUIBlockingWidget parentBlockingWidget;

	private GameObject lastFocusObject;

	private bool isActive;

	private bool firstActivation;

	private const float ALPHA_EPSYLON = 0.9f;

	private void Awake()
	{
		isActive = true;
		firstActivation = false;
		if ((bool)firstSelected)
		{
			if (!allowedObjects.Contains(firstSelected))
			{
				allowedObjects.Add(firstSelected);
			}
			if ((bool)EventSystem.current)
			{
				SelectFirstSelected();
			}
		}
	}

	private void SelectFirstSelected()
	{
		if ((!checkGroup || checkGroup.interactable) && (!checkGroup || !(checkGroup.alpha < 0.9f)) && (!checkGroup || checkGroup.gameObject.activeInHierarchy))
		{
			EventSystem.current.SetSelectedGameObject(null);
			EventSystem.current.SetSelectedGameObject(firstSelected);
			firstActivation = true;
		}
	}

	private void Update()
	{
		if ((bool)checkGroup && (!checkGroup.interactable || checkGroup.alpha < 0.9f || !checkGroup.gameObject.activeInHierarchy || (parentBlockingWidget != null && !parentBlockingWidget.IsActive())))
		{
			if ((bool)EventSystem.current && EventSystem.current.currentSelectedGameObject != null && allowedObjects.Contains(EventSystem.current.currentSelectedGameObject))
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
			return;
		}
		if (!firstActivation && (bool)firstSelected && (bool)EventSystem.current)
		{
			SelectFirstSelected();
		}
		bool flag = false;
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if ((bool)currentSelectedGameObject)
		{
			if (allowedObjects.Contains(currentSelectedGameObject) && currentSelectedGameObject.activeInHierarchy)
			{
				lastFocusObject = currentSelectedGameObject;
			}
			else
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		if ((bool)lastFocusObject && lastFocusObject.activeInHierarchy)
		{
			EventSystem.current.SetSelectedGameObject(lastFocusObject);
			return;
		}
		if ((bool)firstSelected && firstSelected.activeInHierarchy)
		{
			EventSystem.current.SetSelectedGameObject(firstSelected);
			return;
		}
		foreach (GameObject allowedObject in allowedObjects)
		{
			if ((bool)allowedObject && allowedObject.activeInHierarchy)
			{
				EventSystem.current.SetSelectedGameObject(allowedObject);
				break;
			}
		}
	}
}
