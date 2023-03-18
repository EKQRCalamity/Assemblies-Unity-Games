using System;
using System.Collections.Generic;
using Gameplay.GameControllers.Camera;
using UnityEngine;

public class ScrollableModulesManager : MonoBehaviour
{
	public float speed = 1f;

	public float moduleHeight = 10f;

	public float backgroundHeight = 10f;

	public List<GameObject> backgrounds;

	public List<GameObject> modules;

	private float _totalHeight;

	public float _lastHeight;

	public float _lastBg;

	public CameraNumericBoundaries camNumBound;

	public List<GameObject> scrollableItems;

	public bool scrollActive;

	public GameObject deathTrapCollider;

	public event Action<float> OnUpdateHeight;

	public void UpdateTotalHeight(float totalHeight)
	{
		if (totalHeight - _lastHeight > moduleHeight)
		{
			CycleNewModule();
			_lastHeight = totalHeight;
		}
		if (totalHeight - _lastBg > backgroundHeight)
		{
			CycleNewBG();
			_lastBg = totalHeight;
		}
	}

	public void ActivateDeathCollider()
	{
		deathTrapCollider.SetActive(value: true);
	}

	private void LateUpdate()
	{
		if (!scrollActive)
		{
			return;
		}
		float num = Time.deltaTime * speed;
		_totalHeight += num;
		foreach (GameObject scrollableItem in scrollableItems)
		{
			scrollableItem.transform.position += Vector3.up * num;
		}
		camNumBound.BottomBoundary += num;
		camNumBound.TopBoundary += num;
		camNumBound.SetBoundaries();
		if (this.OnUpdateHeight != null)
		{
			this.OnUpdateHeight(num);
		}
		UpdateTotalHeight(_totalHeight);
	}

	private void CycleNewModule()
	{
		GameObject gameObject = PopModule();
		gameObject.transform.position = modules[modules.Count - 1].transform.position + Vector3.up * moduleHeight;
		PushModule(gameObject);
	}

	private void CycleNewBG()
	{
		GameObject gameObject = PopBG();
		gameObject.transform.position = backgrounds[0].transform.position + Vector3.up * backgroundHeight;
		PushBG(gameObject);
	}

	private GameObject PopBG()
	{
		GameObject gameObject = backgrounds[0];
		backgrounds.Remove(gameObject);
		return gameObject;
	}

	private void PushBG(GameObject go)
	{
		backgrounds.Add(go);
	}

	private GameObject PopModule()
	{
		GameObject gameObject = modules[0];
		modules.Remove(gameObject);
		return gameObject;
	}

	private void PushModule(GameObject go)
	{
		modules.Add(go);
	}
}
