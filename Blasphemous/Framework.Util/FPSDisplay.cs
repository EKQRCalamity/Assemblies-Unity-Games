using UnityEngine;

namespace Framework.Util;

public class FPSDisplay : MonoBehaviour
{
	private float _deltaTime;

	private string _label;

	private GUIStyle _style;

	private Rect _screenRect;

	private void Awake()
	{
		base.enabled = Debug.isDebugBuild;
	}

	private void Start()
	{
		_style = new GUIStyle();
		SetFontSize(_style);
		_label = "{0:0.0} ms ({1:0.} fps)";
		_screenRect = GetScreenRect();
	}

	private void Update()
	{
		_deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
	}

	private void OnGUI()
	{
		if (Debug.isDebugBuild)
		{
			float num = _deltaTime * 1000f;
			float num2 = 1f / _deltaTime;
			_style.normal.textColor = ((!(num2 >= 30f)) ? Color.red : Color.cyan);
			string text = string.Format(_label, num, num2);
			GUI.Label(_screenRect, text, _style);
		}
	}

	private Rect GetScreenRect()
	{
		int width = Screen.width;
		int height = Screen.height;
		float height2 = (float)(height * 2) / 100f;
		return new Rect(0f, 0f, width, height2);
	}

	private void SetFontSize(GUIStyle style)
	{
		if (style != null)
		{
			style.alignment = TextAnchor.UpperRight;
			style.fontSize = Screen.height * 2 / 100;
		}
	}
}
