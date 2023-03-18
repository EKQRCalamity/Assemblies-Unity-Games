using Framework.Managers;
using Gameplay.GameControllers.Camera;
using UnityEngine;

public class WaterReflectionsScript : MonoBehaviour
{
	private Material mat;

	public float baseDistanceFactor = 0.2f;

	public float maxDistanceFactor = 1f;

	public float maxcameraDistance = 1f;

	private void Awake()
	{
		mat = GetComponent<MeshRenderer>().material;
		LevelManager.OnGenericsElementsLoaded += OnGenericElementsLoaded;
	}

	private void OnGenericElementsLoaded()
	{
		LevelManager.OnGenericsElementsLoaded -= OnGenericElementsLoaded;
		CameraManager.Instance.TextureHolder.enabled = true;
	}

	private void Update()
	{
		Vector3 position = CameraManager.Instance.transform.position;
		base.transform.position = new Vector3(position.x, base.transform.position.y, base.transform.position.z);
		float num = Mathf.Abs(position.y - base.transform.position.y);
		float distanceFactor = Mathf.Lerp(baseDistanceFactor, maxDistanceFactor, num / maxcameraDistance);
		CameraManager.Instance.TextureHolder.distanceFactor = distanceFactor;
	}
}
