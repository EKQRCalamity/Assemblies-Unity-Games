using UnityEngine;

namespace Framework.Util;

[ExecuteInEditMode]
public class PixelMovement : MonoBehaviour
{
	private Vector3 realPosition;

	private Vector3 oldPosition;

	private Vector3 movement;

	public Vector3 RealPosition => realPosition;

	private void Awake()
	{
		realPosition = base.transform.position;
	}

	private void Update()
	{
		movement = base.transform.position - oldPosition;
		oldPosition = base.transform.position;
		realPosition += movement;
	}

	private void LateUpdate()
	{
		PixelPerfectPosition();
	}

	private void PixelPerfectPosition()
	{
		float x = Mathf.Floor(base.transform.position.x * 32f) / 32f;
		float y = Mathf.Floor(base.transform.position.y * 32f) / 32f;
		base.transform.position = new Vector3(x, y, base.transform.position.z);
	}
}
