using UnityEngine;
using VisualDesignCafe.Rendering.Instancing;

[ExecuteAlways]
public class UseReferenceCamera : MonoBehaviour
{
	[SerializeField]
	private Camera _referenceCamera;

	private Camera _camera;

	private int _delay = 1;

	private void Start()
	{
		_camera = GetComponent<Camera>();
	}

	private void Update()
	{
		_delay--;
		if (_delay <= 0)
		{
			CameraRenderer camera = RendererPool.GetCamera(CameraId.GetId(_camera));
			if (camera != null && camera.ReferenceCamera != _referenceCamera)
			{
				camera.SetReferenceCamera(_referenceCamera);
			}
		}
	}
}
