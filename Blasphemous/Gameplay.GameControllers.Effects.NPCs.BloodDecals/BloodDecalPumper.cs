using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay.GameControllers.Effects.NPCs.BloodDecals;

public class BloodDecalPumper : MonoBehaviour
{
	public BloodDecal[] bloodDecals;

	protected Dictionary<int, BloodDecal> bloodDecalsStore = new Dictionary<int, BloodDecal>();

	protected int bloodDecalsQuantity;

	protected Gameplay.GameControllers.Entities.Entity entity;

	protected int bloodDecalRandomKey;

	protected int lastBloodDecalRandomKey;

	public bool hasPermaBlood;

	protected SpawnPoint permaBloodSpawnPoint;

	private List<int> bloodDecalsKeys = new List<int>();

	public SpawnPoint PermaBloodSpawnPoint => permaBloodSpawnPoint;

	private void Awake()
	{
		bloodDecalsQuantity = bloodDecals.Length;
		feedBloodDecalKeys(bloodDecalsQuantity);
		lastBloodDecalRandomKey = Random.Range(0, bloodDecals.Length);
		permaBloodSpawnPoint = GetComponentInChildren<SpawnPoint>();
		entity = GetComponentInParent<Gameplay.GameControllers.Entities.Entity>();
	}

	private void Update()
	{
		if (hasPermaBlood)
		{
			setPermaBloodPosition();
		}
	}

	public BloodDecal GetBloodDecal(Vector3 position)
	{
		return null;
	}

	protected void storeBloodDecal(int bloodDecalKey, BloodDecal bloodDecal)
	{
		if (!bloodDecalsStore.ContainsKey(bloodDecalKey))
		{
			bloodDecalsStore.Add(bloodDecalKey, bloodDecal);
		}
	}

	public void DisposeBloodDecal(BloodDecal bloodDecal)
	{
		if (bloodDecal != null)
		{
			bloodDecal.gameObject.SetActive(value: false);
		}
	}

	public void DrainBloodDecalPool()
	{
		if (bloodDecalsStore.Count > 0)
		{
			bloodDecalsStore.Clear();
		}
	}

	protected int getBloodDecalRandomKey(int bloodDecalsCollectionRange)
	{
		int num = 0;
		if (bloodDecalsCollectionRange > 0)
		{
			num = Random.Range(num, bloodDecalsCollectionRange);
		}
		return num;
	}

	protected int getNewBloodDecalKey()
	{
		if (bloodDecalsKeys.Count > 0)
		{
			return retrieveFirstRoundKey();
		}
		int num = 0;
		if (bloodDecalsQuantity > 1)
		{
			do
			{
				num = getBloodDecalRandomKey(bloodDecalsQuantity);
			}
			while (lastBloodDecalRandomKey == num);
		}
		return num;
	}

	private void feedBloodDecalKeys(int _bloodDecalsQuantity)
	{
		for (byte b = 0; b < _bloodDecalsQuantity; b = (byte)(b + 1))
		{
			bloodDecalsKeys.Add(b);
		}
	}

	protected int retrieveFirstRoundKey()
	{
		int index = Random.Range(0, bloodDecalsKeys.Count);
		int num = bloodDecalsKeys[index];
		bloodDecalsKeys.Remove(num);
		return num;
	}

	protected GameObject instanceBloodDecal(BloodDecal bloodDecalPrefab, Vector3 pos, Quaternion rotation)
	{
		return Object.Instantiate(bloodDecalPrefab.gameObject, pos, rotation);
	}

	public void setPermaBloodPosition()
	{
		Vector3 localPosition = permaBloodSpawnPoint.transform.localPosition;
		localPosition.x = Mathf.Abs(localPosition.x);
		if (entity.Status.Orientation == EntityOrientation.Left)
		{
			localPosition.x *= -1f;
		}
		permaBloodSpawnPoint.transform.localPosition = localPosition;
	}

	protected void addPermaBloodToSceneStore(PermaBlood permaBlood)
	{
		Vector2 position = permaBloodSpawnPoint.transform.position;
		Quaternion rotation = permaBloodSpawnPoint.transform.rotation;
		PermaBloodStorage.PermaBloodMemento memento = new PermaBloodStorage.PermaBloodMemento(permaBlood.permaBloodType, position, rotation);
		int buildIndex = SceneManager.GetActiveScene().buildIndex;
		Core.Logic.PermaBloodStore.AddPermaBloodToSceneList(buildIndex, memento);
	}
}
