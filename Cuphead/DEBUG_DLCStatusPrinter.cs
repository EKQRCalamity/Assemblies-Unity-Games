using UnityEngine;

public class DEBUG_DLCStatusPrinter : MonoBehaviour
{
	private GUIStyle style;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnGUI()
	{
		if (Time.frameCount >= 120)
		{
			if (style == null)
			{
				style = new GUIStyle(GUI.skin.GetStyle("Box"));
				style.alignment = TextAnchor.UpperLeft;
			}
			GUI.Box(new Rect(0f, 0f, 200f, 100f), "DLC Enabled: " + DLCManager.DLCEnabled());
		}
	}
}
