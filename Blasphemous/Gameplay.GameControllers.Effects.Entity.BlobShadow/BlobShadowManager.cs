using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Entity.BlobShadow;

public class BlobShadowManager : MonoBehaviour
{
	[SerializeField]
	protected GameObject blobShadowPrefab;

	private List<GameObject> blobShadowList = new List<GameObject>();

	public GameObject GetBlowShadow(Vector3 position)
	{
		GameObject gameObject = null;
		if (blobShadowList.Count > 0)
		{
			gameObject = blobShadowList[blobShadowList.Count - 1];
			if (!gameObject.activeSelf)
			{
				gameObject.SetActive(value: true);
			}
			blobShadowList.Remove(gameObject);
		}
		else
		{
			gameObject = Object.Instantiate(blobShadowPrefab, position, Quaternion.identity);
		}
		if (gameObject.transform.parent != base.transform)
		{
			gameObject.transform.parent = base.transform;
		}
		return gameObject;
	}

	public void StoreBlobShadow(GameObject blobShadow)
	{
		if (!blobShadowList.Contains(blobShadow))
		{
			blobShadowList.Add(blobShadow);
			blobShadow.gameObject.SetActive(value: false);
		}
	}
}
