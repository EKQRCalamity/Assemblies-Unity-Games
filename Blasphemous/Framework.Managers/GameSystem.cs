using UnityEngine;

namespace Framework.Managers;

public class GameSystem
{
	private static GUIStyle logStyle;

	private static Texture2D background;

	private const string FONT_NAME = "consolefont";

	public bool ShowDebug;

	private int posYGUI;

	private const int FontSize = 10;

	private const int SeparationY = 3;

	public virtual void Initialize()
	{
	}

	public virtual void AllPreInitialized()
	{
	}

	public virtual void AllInitialized()
	{
	}

	public virtual void Awake()
	{
	}

	public virtual void Start()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void Dispose()
	{
	}

	public virtual void OnGUI()
	{
	}

	protected void DebugResetLine()
	{
		posYGUI = 10;
	}

	protected void DebugDrawTextLine(string text, int posx = 10, int sizex = 1500)
	{
		if (logStyle == null)
		{
			int num = 1;
			Color[] array = new Color[num * num];
			for (int i = 0; i < array.Length; i++)
			{
				ref Color reference = ref array[i];
				reference = new Color(0f, 0f, 0f, 0.2f);
			}
			background = new Texture2D(num, num, TextureFormat.ARGB32, mipmap: false);
			background.SetPixels(array);
			background.Apply();
			logStyle = new GUIStyle();
			logStyle.font = Resources.Load<Font>("consolefont");
			logStyle.normal.textColor = new Color(255f, 255f, 255f);
			logStyle.normal.background = background;
			logStyle.fontSize = 10;
		}
		Rect position = new Rect(posx, posYGUI, sizex, 13f);
		GUI.Label(position, text, logStyle);
		posYGUI += 13;
	}
}
