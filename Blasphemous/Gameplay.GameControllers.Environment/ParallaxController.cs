using UnityEngine;

namespace Gameplay.GameControllers.Environment;

[ExecuteInEditMode]
public class ParallaxController : MonoBehaviour
{
	public Transform target;

	public int gridSize = 32;

	[SerializeField]
	private ParallaxData[] layers;

	private Vector3 oldTargetPosition;

	[SerializeField]
	[Range(0f, 1f)]
	private float influenceX;

	[SerializeField]
	[Range(0f, 1f)]
	private float influenceY;

	private Vector3[] prePixelPerfect;

	public ParallaxData[] Layers => layers;

	public void Start()
	{
		if (target == null && Application.isPlaying)
		{
			target = UnityEngine.Camera.main.transform;
		}
		oldTargetPosition = base.transform.position;
		prePixelPerfect = new Vector3[layers.Length];
		for (int i = 0; i < prePixelPerfect.Length; i++)
		{
			if (layers[i].layer != null)
			{
				ref Vector3 reference = ref prePixelPerfect[i];
				reference = layers[i].layer.transform.position;
			}
		}
	}

	public void LateUpdate()
	{
		if (target != null)
		{
			MoveLayers();
		}
	}

	private void MoveLayers()
	{
		Vector3 vector = target.position - oldTargetPosition;
		oldTargetPosition = target.position;
		for (int i = 0; i < layers.Length; i++)
		{
			float num = Mathf.Floor(layers[i].speed * (float)gridSize) / (float)gridSize;
			GameObject layer = layers[i].layer;
			float x = vector.x * num * influenceX;
			float y = vector.y * num * influenceY;
			if (layer != null)
			{
				layer.transform.position = prePixelPerfect[i];
				layer.transform.Translate(x, y, 0f);
				ref Vector3 reference = ref prePixelPerfect[i];
				reference = layer.transform.position;
				PixelPerfectPosition(layer.transform);
			}
		}
	}

	private void PixelPerfectPosition(Transform transform)
	{
		float x = Mathf.Floor(transform.position.x * (float)gridSize) / (float)gridSize;
		float y = Mathf.Floor(transform.position.y * (float)gridSize) / (float)gridSize;
		transform.position = new Vector3(x, y, transform.position.z);
	}
}
