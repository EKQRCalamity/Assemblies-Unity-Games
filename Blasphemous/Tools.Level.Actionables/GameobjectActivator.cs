using Framework.FrameworkCore;
using Framework.Managers;
using Sirenix.OdinInspector;
using Tools.Gameplay;
using UnityEngine;

namespace Tools.Level.Actionables;

public class GameobjectActivator : PersistentObject, IActionable
{
	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	protected GameObject[] target = new GameObject[0];

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool persistState;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool gameobjectsActivated;

	public bool Locked { get; set; }

	private void Awake()
	{
	}

	private void SetActiveState(bool b)
	{
		gameobjectsActivated = b;
		ActivateObjects(target, gameobjectsActivated);
	}

	private void ActivateObjects(GameObject[] gameObjects, bool active)
	{
		foreach (GameObject gameObject in gameObjects)
		{
			if (!(gameObject == null))
			{
				gameObject.SetActive(active);
			}
		}
	}

	public void Use()
	{
		SetActiveState(!gameobjectsActivated);
	}

	public override bool IsOpenOrActivated()
	{
		return gameobjectsActivated;
	}

	public override PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		if (!persistState)
		{
			return null;
		}
		BasicPersistence basicPersistence = CreatePersistentData<BasicPersistence>();
		basicPersistence.triggered = gameobjectsActivated;
		return basicPersistence;
	}

	public override void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		if (persistState)
		{
			BasicPersistence basicPersistence = (BasicPersistence)data;
			gameobjectsActivated = basicPersistence.triggered;
			SetActiveState(!gameobjectsActivated);
		}
	}
}
