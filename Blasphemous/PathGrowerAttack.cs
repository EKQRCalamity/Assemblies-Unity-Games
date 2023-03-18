using System.Collections;
using BezierSplines;
using Sirenix.OdinInspector;
using UnityEngine;

public class PathGrowerAttack : MonoBehaviour
{
	public BezierSpline spline;

	public float maxSeconds;

	public float maxDistanceBetweenInstances = 0.3f;

	public GameObject growerPrefab;

	[Button(ButtonSizes.Small)]
	private void GrowBody()
	{
		StartCoroutine(GrowCoroutine(maxSeconds));
	}

	private void CreatePart(Vector2 pos, Vector2 dir)
	{
		GameObject gameObject = Object.Instantiate(growerPrefab, pos, Quaternion.identity);
		gameObject.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * 57.29578f);
	}

	private IEnumerator GrowCoroutine(float seconds)
	{
		float counter = 0f;
		Vector3 lastPos = Vector3.zero;
		for (; counter < seconds; counter += Time.deltaTime)
		{
			float v = counter / seconds;
			Vector3 p = spline.GetPoint(v);
			float d = Vector3.Distance(lastPos, p);
			if (d > maxDistanceBetweenInstances)
			{
				CreatePart(p, spline.GetDirection(v));
				lastPos = p;
			}
			yield return null;
		}
	}
}
