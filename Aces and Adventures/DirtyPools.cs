using System.Collections.Generic;
using UnityEngine;

public static class DirtyPools
{
	private static readonly List<GameObject> _PendingAutoRepoolingObjects = new List<GameObject>();

	private static readonly List<GameObject> _AutoRepoolingObjects = new List<GameObject>();

	private static GameObject _Instantiate(GameObject blueprint, bool autoRepool = true, Vector3? position = null, Quaternion? rotation = null)
	{
		GameObject gameObject = Object.Instantiate(blueprint, position ?? blueprint.transform.position, rotation ?? blueprint.transform.rotation);
		if (position.HasValue)
		{
			gameObject.transform.position = position.Value;
		}
		if (rotation.HasValue)
		{
			gameObject.transform.rotation = rotation.Value;
		}
		_PendingAutoRepoolingObjects.Add(gameObject);
		return gameObject;
	}

	public static void Update()
	{
		bool flag = false;
		for (int i = 0; i < _AutoRepoolingObjects.Count; i++)
		{
			GameObject gameObject = _AutoRepoolingObjects[i];
			if (!gameObject || !gameObject.activeSelf)
			{
				Object.Destroy(gameObject);
				_AutoRepoolingObjects[i] = null;
				flag = true;
			}
		}
		if (flag)
		{
			_AutoRepoolingObjects.RemoveAllNull();
		}
		bool flag2 = false;
		for (int j = 0; j < _PendingAutoRepoolingObjects.Count; j++)
		{
			GameObject gameObject2 = _PendingAutoRepoolingObjects[j];
			if (!gameObject2 || gameObject2.activeSelf)
			{
				if ((bool)gameObject2)
				{
					_AutoRepoolingObjects.Add(gameObject2);
				}
				_PendingAutoRepoolingObjects[j] = null;
				flag2 = true;
			}
		}
		if (flag2)
		{
			_PendingAutoRepoolingObjects.RemoveAllNull();
		}
	}

	public static GameObject Unpool(GameObject blueprint, bool setActive = true, bool autoRepool = true)
	{
		GameObject gameObject = _Instantiate(blueprint, autoRepool);
		gameObject.SetActive(setActive);
		return gameObject;
	}

	public static GameObject Unpool(GameObject blueprint, Transform parent, bool setActive = true, bool autoRepool = true, bool worldPositionStays = false)
	{
		GameObject gameObject = _Instantiate(blueprint, autoRepool);
		gameObject.transform.SetParent(parent, worldPositionStays);
		gameObject.SetActive(setActive);
		return gameObject;
	}

	public static GameObject Unpool(GameObject blueprint, Vector3 position, Quaternion? rotation = null, Transform parent = null, bool setActive = true, bool autoRepool = true)
	{
		GameObject gameObject = _Instantiate(blueprint, autoRepool, position, rotation);
		gameObject.transform.SetParent(parent, worldPositionStays: true);
		gameObject.SetActive(setActive);
		return gameObject;
	}

	public static GameObject TryUnpool(GameObject blueprint, Vector3 position, Quaternion? rotation = null, Transform parent = null, bool setActive = true, bool autoRepool = true)
	{
		if (!blueprint)
		{
			return null;
		}
		return Unpool(blueprint, position, rotation, parent, setActive, autoRepool);
	}
}
