using System.Collections;
using UnityEngine;

public class RotatingUISprite : MonoBehaviour
{
	[SerializeField]
	private float speed = 1f;

	[SerializeField]
	private int frameRate = 12;

	private void Start()
	{
		StartCoroutine(rotate_cr());
	}

	private IEnumerator rotate_cr()
	{
		while (true)
		{
			base.transform.Rotate(new Vector3(0f, 0f, speed / (float)frameRate));
			yield return CupheadTime.WaitForSeconds(this, 1f / (float)frameRate);
		}
	}
}
