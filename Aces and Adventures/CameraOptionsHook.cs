using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraOptionsHook : MonoBehaviour
{
	private void Awake()
	{
		ProfileManager.options.video.ApplyChanges();
	}
}
