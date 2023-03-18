using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GameControllers.Environment;

public class RepeatedScrollController : MonoBehaviour
{
	public class SortByXCoord : IComparer<Transform>
	{
		public int Compare(Transform t1, Transform t2)
		{
			if (t1.position.x < t2.position.x)
			{
				return -1;
			}
			if (t1.position.x > t2.position.x)
			{
				return 1;
			}
			return 0;
		}
	}

	public Transform targetReference;

	public Vector2 scrollArea;

	public int PPU = 32;

	public Transform elementToRepeat;

	public Vector2 elementSize;

	private Vector3 oldTargetPosition;

	[Range(-1f, 1f)]
	public int speed;

	[SerializeField]
	[Range(0f, 1f)]
	private float influenceX;

	private Transform[] elements = new Transform[0];

	private SortByXCoord sort = new SortByXCoord();

	private void Awake()
	{
	}

	public void Start()
	{
		if (targetReference == null && Application.isPlaying)
		{
			targetReference = UnityEngine.Camera.main.transform;
		}
		oldTargetPosition = targetReference.position;
		int num = Mathf.CeilToInt(scrollArea.x / elementSize.x) + 1;
		elements = new Transform[num];
		float num2 = targetReference.position.x - scrollArea.x * 0.5f + elementSize.x * 0.5f;
		for (int i = 0; i < num; i++)
		{
			elements[i] = UnityEngine.Object.Instantiate(elementToRepeat, base.transform);
			Vector3 position = elements[i].position;
			position.x = num2 + (float)i * elementSize.x;
			elements[i].position = position;
		}
		elementToRepeat.gameObject.SetActive(value: false);
	}

	public void LateUpdate()
	{
		if (targetReference != null)
		{
			Move();
		}
	}

	private void Move()
	{
		Vector3 vector = targetReference.position - oldTargetPosition;
		oldTargetPosition = targetReference.position;
		float x = vector.x * (float)speed * influenceX;
		for (int i = 0; i < elements.Length; i++)
		{
			elements[i].Translate(x, 0f, 0f);
		}
		CheckBounds();
	}

	private void CheckBounds()
	{
		Vector2 vector = targetReference.position;
		Rect rect = new Rect(vector - scrollArea * 0.5f, scrollArea);
		Vector3 lhs = targetReference.position;
		Vector3 lhs2 = targetReference.position;
		int num = -1;
		for (int i = 0; i < elements.Length; i++)
		{
			vector = elements[i].position;
			Rect other = new Rect(vector - elementSize * 0.5f, elementSize);
			if (!rect.Overlaps(other))
			{
				num = i;
				continue;
			}
			lhs = Vector3.Min(lhs, other.min);
			lhs2 = Vector3.Max(lhs2, other.max);
		}
		if (num != -1)
		{
			if (lhs.x > rect.min.x)
			{
				float x = lhs.x - elementSize.x * 0.5f;
				vector = elements[num].position;
				vector.x = x;
				elements[num].position = vector;
			}
			else if (lhs2.x < rect.max.x)
			{
				float x2 = lhs2.x + elementSize.x * 0.5f;
				vector = elements[num].position;
				vector.x = x2;
				elements[num].position = vector;
			}
			Array.Sort(elements, sort);
		}
	}

	private void PixelPerfectPosition(Transform transform, int ppu)
	{
		float x = Mathf.Floor(transform.position.x * (float)ppu) / (float)ppu;
		float y = Mathf.Floor(transform.position.y * (float)ppu) / (float)ppu;
		transform.position = new Vector3(x, y, transform.position.z);
	}

	private void OnDrawGizmosSelected()
	{
		if ((bool)targetReference)
		{
			Gizmos.DrawWireCube(targetReference.position, scrollArea);
		}
		else
		{
			Gizmos.DrawWireCube(base.transform.position, scrollArea);
		}
		Gizmos.color = Color.blue;
		if (Application.isPlaying)
		{
			for (int i = 0; i < elements.Length; i++)
			{
				Gizmos.DrawWireCube(elements[i].position, elementSize);
			}
		}
		else if ((bool)elementToRepeat)
		{
			Gizmos.DrawWireCube(elementToRepeat.position, elementSize);
		}
	}
}
