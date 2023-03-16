using UnityEngine;

public class ScreenshotHandler : MonoBehaviour
{
	public enum cameraType
	{
		Map,
		UI,
		Level
	}

	private static ScreenshotHandler instance;

	private bool takeScreenshotNextFrame;

	private Camera myCamera;

	private string fileName;

	private string folderName;

	private cameraType currentCameraType;

	private void Awake()
	{
		instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
		Object.Destroy(this);
	}

	public static void TakeScreenshot_Static(cameraType _camera, string _folderName, string _fileName)
	{
	}
}
