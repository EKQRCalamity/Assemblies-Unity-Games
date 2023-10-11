using System.Collections.Generic;
using UnityEngine;

public class UPools : MonoBehaviour
{
	public struct PoolTransformData
	{
		private readonly Vector3 _localPosition;

		private readonly Vector3 _localScale;

		private readonly Quaternion _localRotation;

		private readonly Vector2 _anchorMin;

		private readonly Vector2 _anchorMax;

		private readonly Vector2 _pivot;

		public PoolTransformData(GameObject blueprint)
		{
			_localPosition = blueprint.transform.localPosition;
			_localScale = blueprint.transform.localScale;
			_localRotation = blueprint.transform.localRotation;
			_anchorMin = default(Vector2);
			_anchorMax = default(Vector2);
			_pivot = default(Vector2);
			RectTransform rectTransform = blueprint.transform as RectTransform;
			if ((bool)rectTransform)
			{
				_anchorMin = rectTransform.anchorMin;
				_anchorMax = rectTransform.anchorMax;
				_pivot = rectTransform.pivot;
			}
		}

		public void ResetTransformData(GameObject gameObject)
		{
			gameObject.transform.localPosition = _localPosition;
			gameObject.transform.localScale = _localScale;
			gameObject.transform.localRotation = _localRotation;
			RectTransform rectTransform = gameObject.transform as RectTransform;
			if ((bool)rectTransform)
			{
				rectTransform.anchorMin = _anchorMin;
				rectTransform.anchorMax = _anchorMax;
				rectTransform.pivot = _pivot;
			}
		}
	}

	private static UPools _instance;

	private readonly Dictionary<int, Queue<GameObject>> _pools = new Dictionary<int, Queue<GameObject>>();

	private readonly Dictionary<int, PoolTransformData> _poolTransformData = new Dictionary<int, PoolTransformData>();

	private readonly Dictionary<GameObject, int> _unpooledObjects = new Dictionary<GameObject, int>();

	private readonly List<GameObject> _pendingAutoRepoolingObjects = new List<GameObject>();

	private readonly List<GameObject> _autoRepoolingObjects = new List<GameObject>();

	public static UPools Instance => ManagerUtil.GetSingletonInstance(ref _instance, createSeparateGameObject: true);

	public static void Clear(bool updateBeforeClearing = false)
	{
		if (!_instance)
		{
			return;
		}
		if (updateBeforeClearing)
		{
			_instance.LateUpdate();
		}
		_instance.gameObject.DestroyChildren();
		foreach (Queue<GameObject> value in _instance._pools.Values)
		{
			value.Clear();
		}
		_instance._pendingAutoRepoolingObjects.Clear();
	}

	private void LateUpdate()
	{
		bool flag = false;
		for (int i = 0; i < _autoRepoolingObjects.Count; i++)
		{
			GameObject gameObject = _autoRepoolingObjects[i];
			if (!gameObject || !gameObject.activeSelf)
			{
				Repool(gameObject, isAutoRepool: true);
				_autoRepoolingObjects[i] = null;
				flag = true;
			}
		}
		if (flag)
		{
			_autoRepoolingObjects.RemoveAllNull();
		}
		bool flag2 = false;
		for (int j = 0; j < _pendingAutoRepoolingObjects.Count; j++)
		{
			GameObject gameObject2 = _pendingAutoRepoolingObjects[j];
			if (gameObject2.activeSelf)
			{
				_autoRepoolingObjects.Add(gameObject2);
				_pendingAutoRepoolingObjects[j] = null;
				flag2 = true;
			}
		}
		if (flag2)
		{
			_pendingAutoRepoolingObjects.RemoveAllNull();
		}
		DirtyPools.Update();
	}

	public GameObject Unpool(GameObject blueprint, bool autoRepool = true, Vector3? position = null, Quaternion? rotation = null)
	{
		int instanceID = blueprint.GetInstanceID();
		if (!_pools.ContainsKey(instanceID))
		{
			_pools.Add(instanceID, new Queue<GameObject>());
			_poolTransformData.Add(instanceID, new PoolTransformData(blueprint));
		}
		Queue<GameObject> queue = _pools[instanceID];
		GameObject gameObject;
		do
		{
			if (queue.Count == 0)
			{
				queue.Enqueue(Object.Instantiate(blueprint, position ?? blueprint.transform.position, rotation ?? blueprint.transform.rotation).SetName(blueprint.name));
			}
			gameObject = queue.Dequeue();
		}
		while (!gameObject);
		if (position.HasValue)
		{
			gameObject.transform.position = position.Value;
		}
		if (rotation.HasValue)
		{
			gameObject.transform.rotation = rotation.Value;
		}
		_unpooledObjects.Add(gameObject, instanceID);
		if (autoRepool)
		{
			_pendingAutoRepoolingObjects.Add(gameObject);
		}
		return gameObject;
	}

	public bool Repool(GameObject gameObject, bool isAutoRepool)
	{
		if (!_unpooledObjects.ContainsKey(gameObject))
		{
			return false;
		}
		if (!gameObject)
		{
			_unpooledObjects.Remove(gameObject);
			return false;
		}
		int key = _unpooledObjects[gameObject];
		_pools[key].Enqueue(gameObject);
		_unpooledObjects.Remove(gameObject);
		if (!isAutoRepool)
		{
			gameObject.SetActive(value: false);
		}
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		_poolTransformData[key].ResetTransformData(gameObject);
		return true;
	}

	public bool ForceAutoRepool(GameObject gameObject)
	{
		int num;
		if (!_pendingAutoRepoolingObjects.Remove(gameObject))
		{
			num = (_autoRepoolingObjects.Remove(gameObject) ? 1 : 0);
			if (num == 0)
			{
				goto IL_0029;
			}
		}
		else
		{
			num = 1;
		}
		Repool(gameObject, isAutoRepool: false);
		goto IL_0029;
		IL_0029:
		return (byte)num != 0;
	}

	public bool MarkForAutoRepool(GameObject gameObject)
	{
		bool num = _pendingAutoRepoolingObjects.Remove(gameObject);
		if (num)
		{
			_autoRepoolingObjects.Add(gameObject);
		}
		return num;
	}
}
