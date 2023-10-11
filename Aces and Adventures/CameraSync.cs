using UnityEngine;

[RequireComponent(typeof(Camera))]
[ScriptOrder(1)]
public class CameraSync : MonoBehaviour
{
	public Camera camereToSyncTo;

	public bool syncTransform;

	public bool syncAspectRatio = true;

	public bool syncFieldOfView = true;

	public bool syncNearPlane = true;

	public bool syncFarPlane = true;

	public bool syncHDR;

	public bool syncMSAA;

	public bool syncRenderPath;

	private Camera _camera;

	public static void CopyFrom(Camera camera, Camera copyFrom, bool transform = true, bool aspect = true, bool fieldOfView = true, bool nearClipPlane = true, bool farClipPlane = true, bool hdr = true, bool msaa = true, bool renderPath = true)
	{
		if ((bool)camera && (bool)copyFrom)
		{
			if (transform)
			{
				camera.transform.CopyFrom(copyFrom.transform);
			}
			if (aspect)
			{
				camera.aspect = copyFrom.aspect;
			}
			if (fieldOfView)
			{
				camera.fieldOfView = copyFrom.fieldOfView;
			}
			if (nearClipPlane)
			{
				camera.nearClipPlane = copyFrom.nearClipPlane;
			}
			if (farClipPlane)
			{
				camera.farClipPlane = copyFrom.farClipPlane;
			}
			if (hdr)
			{
				camera.allowHDR = copyFrom.allowHDR;
			}
			if (msaa)
			{
				camera.allowMSAA = copyFrom.allowMSAA;
			}
			if (renderPath)
			{
				camera.renderingPath = copyFrom.renderingPath;
			}
		}
	}

	private void Awake()
	{
		_camera = GetComponent<Camera>();
	}

	private void Update()
	{
		CopyFrom(_camera, camereToSyncTo, syncTransform, syncAspectRatio, syncFieldOfView, syncNearPlane, syncFarPlane, syncHDR, syncMSAA, syncRenderPath);
	}

	private void LateUpdate()
	{
		Update();
	}
}
