using System;
using System.Collections.Generic;
using Tools.Level.Actionables;
using UnityEngine;

namespace Gameplay.GameControllers.Environment;

public class AshPlatformFightManager : MonoBehaviour
{
	[Serializable]
	public struct AshPlatformsByIndex
	{
		public int index;

		public AshPlatform platform;
	}

	public float heightLimit = 5f;

	public bool heightLimitOn = true;

	public List<AshPlatformsByIndex> platforms;

	public float timeBetweenIndexes = 3f;

	public int maxIndex = 5;

	public float currentCounter;

	public int currentIndex = -1;

	public float platformActivationTime = 2f;

	public bool activated;

	private void Update()
	{
		if (activated)
		{
			CheckIndex();
		}
	}

	public void Activate()
	{
		currentCounter = 0f;
		currentIndex = 0;
		activated = true;
	}

	public void Deactivate()
	{
		activated = false;
		foreach (AshPlatformsByIndex platform in platforms)
		{
			if (platform.platform.showing)
			{
				platform.platform.Hide(1f);
			}
		}
	}

	private void CheckIndex()
	{
		if (currentCounter < timeBetweenIndexes)
		{
			currentCounter += Time.deltaTime;
			return;
		}
		currentCounter = 0f;
		currentIndex = (currentIndex + 1) % maxIndex;
		ActivateAllWithIndex(currentIndex);
	}

	private void ActivateAllWithIndex(int i)
	{
		foreach (AshPlatformsByIndex platform in platforms)
		{
			if (platform.index == i && (!heightLimitOn || platform.platform.transform.position.y < base.transform.position.y + heightLimit))
			{
				platform.platform.Show();
				platform.platform.Hide(platformActivationTime);
			}
		}
	}

	public void DeactivateRecursively(AshPlatform a)
	{
		List<GameObject> targets = a.GetTargets();
		foreach (GameObject item in targets)
		{
			if (item == null)
			{
				break;
			}
			AshPlatform component = item.GetComponent<AshPlatform>();
			if (a != null)
			{
				DeactivateRecursively(component);
			}
		}
		a.Hide(0f);
	}

	public void DeactivateGroup(AshPlatform a)
	{
		DeactivateRecursively(a);
		a.gameObject.SetActive(value: false);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine((Vector2)base.transform.position - Vector2.right * 10f + Vector2.up * heightLimit, (Vector2)base.transform.position + Vector2.right * 10f + Vector2.up * heightLimit);
		foreach (AshPlatformsByIndex platform in platforms)
		{
			switch (platform.index)
			{
			case 0:
				Gizmos.color = Color.red;
				break;
			case 1:
				Gizmos.color = Color.blue;
				break;
			case 2:
				Gizmos.color = Color.green;
				break;
			case 3:
				Gizmos.color = Color.yellow;
				break;
			}
			Gizmos.DrawCube(platform.platform.transform.position + Vector3.right, new Vector3(1f, 0.5f, 1f));
		}
		for (int i = 0; i < 4; i++)
		{
			switch (i)
			{
			case 0:
				Gizmos.color = Color.red;
				break;
			case 1:
				Gizmos.color = Color.blue;
				break;
			case 2:
				Gizmos.color = Color.green;
				break;
			case 3:
				Gizmos.color = Color.yellow;
				break;
			}
			Vector2 vector = (Vector2)base.transform.position + Vector2.down * 6f - Vector2.right * 2f + Vector2.right * i;
			Gizmos.DrawSphere(vector, 0.25f);
		}
	}
}
