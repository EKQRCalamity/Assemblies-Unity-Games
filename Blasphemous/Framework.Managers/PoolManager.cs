using System.Collections.Generic;
using Framework.Pooling;
using UnityEngine;

namespace Framework.Managers;

public class PoolManager : MonoBehaviour
{
	public class ObjectInstance
	{
		private readonly GameObject gameObject;

		private readonly bool hasPoolObjectComponent;

		private readonly PoolObject[] poolObjectScripts;

		private readonly Transform transform;

		public GameObject GameObject => gameObject;

		public ObjectInstance(GameObject objectInstance)
		{
			gameObject = objectInstance;
			transform = gameObject.transform;
			gameObject.SetActive(value: false);
			if ((bool)gameObject.GetComponent<PoolObject>())
			{
				hasPoolObjectComponent = true;
				poolObjectScripts = gameObject.GetComponents<PoolObject>();
			}
		}

		public void Reuse(Vector3 position, Quaternion rotation)
		{
			if (!gameObject)
			{
				return;
			}
			gameObject.SetActive(value: true);
			transform.position = position;
			transform.rotation = rotation;
			if (hasPoolObjectComponent)
			{
				PoolObject[] array = poolObjectScripts;
				foreach (PoolObject poolObject in array)
				{
					poolObject.OnObjectReuse();
				}
			}
		}

		public void SetParent(Transform parent)
		{
			transform.parent = parent;
		}
	}

	private static PoolManager _instance;

	private readonly Dictionary<int, Queue<ObjectInstance>> poolDictionary = new Dictionary<int, Queue<ObjectInstance>>();

	public static PoolManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Object.FindObjectOfType<PoolManager>();
			}
			return _instance;
		}
	}

	public void CreatePool(GameObject prefab, int poolSize)
	{
		int instanceID = prefab.GetInstanceID();
		if (!poolDictionary.ContainsKey(instanceID))
		{
			poolDictionary.Add(instanceID, new Queue<ObjectInstance>());
			GameObject gameObject = new GameObject(prefab.name + " pool");
			gameObject.transform.parent = base.transform;
			for (int i = 0; i < poolSize; i++)
			{
				ObjectInstance objectInstance = new ObjectInstance(Object.Instantiate(prefab));
				poolDictionary[instanceID].Enqueue(objectInstance);
				objectInstance.SetParent(gameObject.transform);
			}
			return;
		}
		GameObject gameObject2 = GameObject.Find(prefab.name + " pool");
		if (!(gameObject2 == null))
		{
			for (int j = 0; j < poolSize; j++)
			{
				ObjectInstance objectInstance2 = new ObjectInstance(Object.Instantiate(prefab));
				poolDictionary[instanceID].Enqueue(objectInstance2);
				objectInstance2.SetParent(gameObject2.transform);
			}
		}
	}

	public ObjectInstance ReuseObject(GameObject prefab, Vector3 position, Quaternion rotation, bool createPoolIfNeeded = false, int poolSize = 1)
	{
		int instanceID = prefab.GetInstanceID();
		ObjectInstance objectInstance = null;
		if (poolDictionary.ContainsKey(instanceID))
		{
			objectInstance = poolDictionary[instanceID].Dequeue();
			poolDictionary[instanceID].Enqueue(objectInstance);
			objectInstance.Reuse(position, rotation);
		}
		else if (createPoolIfNeeded)
		{
			CreatePool(prefab, poolSize);
			objectInstance = ReuseObject(prefab, position, rotation);
		}
		return objectInstance;
	}
}
