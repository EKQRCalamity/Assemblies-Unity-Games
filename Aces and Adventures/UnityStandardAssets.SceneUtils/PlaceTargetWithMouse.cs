using UnityEngine;

namespace UnityStandardAssets.SceneUtils;

public class PlaceTargetWithMouse : MonoBehaviour
{
	public float surfaceOffset = 1.5f;

	public GameObject setTargetOn;

	private void Update()
	{
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo))
		{
			setTargetOn.transform.position = hitInfo.point + hitInfo.normal * surfaceOffset;
			setTargetOn.transform.forward = -hitInfo.normal;
		}
	}
}
