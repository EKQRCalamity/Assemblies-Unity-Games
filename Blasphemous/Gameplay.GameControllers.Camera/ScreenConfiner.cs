using UnityEngine;

namespace Gameplay.GameControllers.Camera;

public class ScreenConfiner : MonoBehaviour
{
	[SerializeField]
	private CameraNumericBoundaries cameraNumericBoundaries;

	[SerializeField]
	private GameObject levelLeftBoundaryPrefab;

	private GameObject levelLeftBoundary;

	public void EnableBoundary()
	{
		Debug.Log("enable");
		if ((bool)levelLeftBoundary)
		{
			levelLeftBoundary.gameObject.GetComponent<Collider2D>().enabled = true;
		}
	}

	public void DisableBoundary()
	{
		if ((bool)levelLeftBoundary)
		{
			levelLeftBoundary.gameObject.GetComponent<Collider2D>().enabled = false;
		}
	}

	private void Start()
	{
		levelLeftBoundary = Object.Instantiate(position: new Vector2(cameraNumericBoundaries.LeftBoundary, cameraNumericBoundaries.TopBoundary / 2f), original: levelLeftBoundaryPrefab, rotation: Quaternion.identity);
		levelLeftBoundary.transform.SetParent(base.transform, worldPositionStays: true);
		DisableBoundary();
	}

	private void LateUpdate()
	{
		Vector2 vector = new Vector2(cameraNumericBoundaries.LeftBoundary, cameraNumericBoundaries.TopBoundary / 2f);
		levelLeftBoundary.transform.position = vector;
	}
}
