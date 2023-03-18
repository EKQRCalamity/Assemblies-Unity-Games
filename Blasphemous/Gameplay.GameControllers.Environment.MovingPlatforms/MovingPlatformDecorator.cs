using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.MovingPlatforms;

public class MovingPlatformDecorator : MonoBehaviour
{
	public List<Material> materialsLinkedToSpeed;

	public float maxSpeed;

	public float minSpeed;

	public bool isVertical;

	private StraightMovingPlatform plat;

	public float shaderToUnitFactor = 0.2f;

	private void Awake()
	{
		plat = GetComponent<StraightMovingPlatform>();
	}

	private void Update()
	{
		foreach (Material item in materialsLinkedToSpeed)
		{
			SetSpeed(item);
		}
	}

	private float ConvertSpeed(Vector3 v)
	{
		float num = 0f;
		num = ((!isVertical) ? v.x : v.y);
		return (0f - num) * shaderToUnitFactor;
	}

	private void SetSpeed(Material m)
	{
		float value = ConvertSpeed(plat.GetVelocity());
		m.SetFloat("_Speed", value);
	}
}
