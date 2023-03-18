using System;
using Framework.FrameworkCore;
using Gameplay.UI;
using UnityEngine;

namespace Framework.Managers;

public class ScreenManager : GameSystem
{
	public delegate void ScreenEvent();

	public const int VERTICAL_RESOLUTION = 360;

	public const int HORIZONTAL_RESOLUTION = 640;

	private ScalingMode mode;

	public const float PPU = 32f;

	private RenderTexture gameRenderTexture;

	private RenderTexture uiRenderTexture;

	private Material gameMaterial;

	private Material uiMaterial;

	private Camera gameCamera;

	private Camera virtualCamera;

	private Camera uiCamera;

	private int currentWidth;

	private int currentHeight;

	private float fitScale;

	private int realFitScale;

	private Dimensions fitDimension = Dimensions.Horizontal;

	public int CurrentWidth => currentWidth;

	public int CurrentHeight => currentHeight;

	public Camera VirtualCamera => virtualCamera;

	public Camera GameCamera => gameCamera;

	public Camera UICamera => uiCamera;

	public static event ScreenEvent OnLoad;

	public static event ScreenEvent OnSettingsChanged;

	public override void Initialize()
	{
		LevelManager.OnGenericsElementsLoaded += OnGenericsLoaded;
		Log.Trace("VScreen", "Virtual Screen has been initialized.");
	}

	public override void Update()
	{
		if (currentHeight != Screen.height || currentWidth != Screen.width)
		{
			FitScreenCamera();
		}
	}

	private void OnGenericsLoaded()
	{
		InitializeManager();
	}

	public void InitializeManager()
	{
		GameObject gameObject = GameObject.FindWithTag("VirtualCamera");
		GameObject gameObject2 = GameObject.FindWithTag("MainCamera");
		GameObject gameObject3 = GameObject.FindWithTag("UICamera");
		if (!gameObject || !gameObject2 || !gameObject3)
		{
			Log.Error("VScreen", "Missing components on ScreenManager.");
			return;
		}
		gameCamera = gameObject2.GetComponent<Camera>();
		virtualCamera = gameObject.GetComponent<Camera>();
		uiCamera = gameObject3.GetComponent<Camera>();
		if (!gameCamera || !virtualCamera || !uiCamera)
		{
			Log.Error("VScreen", "Missing components on ScreenManager.");
			return;
		}
		InitializeRenderTexture();
		ConfigureGameCamera();
		FitScreenCamera();
		if (ScreenManager.OnLoad != null)
		{
			ScreenManager.OnLoad();
		}
		Log.Trace("VScreen", "Virtual camera initialized.");
	}

	public bool ResolutionRequireStrategyScale(int width, int height)
	{
		bool result = false;
		float num = (float)width / 640f;
		float num2 = (float)height / 360f;
		float num3 = ((!(num < num2)) ? num2 : num);
		float num4 = (int)Math.Floor(num3);
		if (Math.Abs(num4 - num3) > Mathf.Epsilon)
		{
			result = true;
		}
		return result;
	}

	public void FitScreenCamera()
	{
		float num = (float)Screen.width / 640f;
		float num2 = (float)Screen.height / 360f;
		if (num < num2)
		{
			fitScale = num;
			fitDimension = Dimensions.Horizontal;
		}
		else
		{
			fitScale = num2;
			fitDimension = Dimensions.Vertical;
		}
		realFitScale = (int)Math.Floor(fitScale);
		if (Math.Abs((float)realFitScale - fitScale) > Mathf.Epsilon)
		{
			mode = ((UIController.instance.GetScalingStrategy() != 0) ? ScalingMode.Scale : ScalingMode.Letterbox);
		}
		float num3 = 0f;
		float num4 = ((mode != 0) ? fitScale : ((float)realFitScale));
		num3 = 1f / num4 * ((float)Screen.height / 2f);
		if ((bool)virtualCamera && (bool)uiCamera)
		{
			virtualCamera.orthographicSize = num3;
			uiCamera.orthographicSize = num3;
			currentHeight = Screen.height;
			currentWidth = Screen.width;
		}
	}

	private void InitializeRenderTexture()
	{
		gameRenderTexture = new RenderTexture(640, 360, 0)
		{
			name = "Game Texture",
			filterMode = FilterMode.Point,
			depth = 24
		};
		gameRenderTexture.Create();
		uiRenderTexture = new RenderTexture(640, 360, 0)
		{
			name = "UI Texture",
			filterMode = FilterMode.Point,
			depth = 24
		};
		uiRenderTexture.Create();
		gameMaterial = Core.Logic.CameraManager.gameQuadRenderer.material;
		gameMaterial.SetTexture(Shader.PropertyToID("_MainTex"), gameRenderTexture);
		uiMaterial = Core.Logic.CameraManager.uiQuadRenderer.material;
		uiMaterial.SetTexture(Shader.PropertyToID("_MainTex"), uiRenderTexture);
	}

	private void ConfigureGameCamera()
	{
		gameCamera.targetTexture = gameRenderTexture;
		uiCamera.targetTexture = uiRenderTexture;
	}
}
