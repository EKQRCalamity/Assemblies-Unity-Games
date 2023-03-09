using UnityEngine;

public class AnimatableSortingOrder : MonoBehaviour
{
	private SpriteRenderer sr;

	public float sortingLayer;

	private void Start()
	{
		sr = GetComponent<SpriteRenderer>();
	}

	private void LateUpdate()
	{
		int sortingOrder = (int)sortingLayer;
		sr.sortingOrder = sortingOrder;
	}
}
