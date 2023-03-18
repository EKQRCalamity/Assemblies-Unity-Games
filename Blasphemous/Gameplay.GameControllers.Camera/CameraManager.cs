using System;
using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;
using Framework.Managers;
using Gameplay.UI.Widgets;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Camera;

public class CameraManager : MonoBehaviour
{
	public const float HorizontalFollowSmoothness = 0.1f;

	public const float VerticalFollowSmoothness = 0.1f;

	public static CameraManager Instance;

	[FoldoutGroup("ScreenManager references", 0)]
	public MeshRenderer uiQuadRenderer;

	[FoldoutGroup("ScreenManager references", 0)]
	public MeshRenderer gameQuadRenderer;

	public ProCamera2D ProCamera2D { get; private set; }

	public ProCamera2DParallax ProCamera2DParallax { get; private set; }

	public ProCamera2DPixelPerfect ProCamera2DPixelPerfect { get; private set; }

	public CameraPlayerOffset CameraPlayerOffset { get; private set; }

	public ProCamera2DNumericBoundaries ProCamera2DNumericBoundaries { get; private set; }

	public ProCamera2DGeometryBoundaries ProCamera2DGeometryBoundaries { get; private set; }

	public ProCamera2DShake ProCamera2DShake { get; private set; }

	public ProCamera2DRails CameraRails { get; private set; }

	public ProCamera2DPanAndZoom PanAndZoom { get; private set; }

	public CameraPan CustomCameraPan { get; set; }

	public ShockwaveManager ShockwaveManager { get; set; }

	public ScreenMaterialEffectsManager ScreenEffectsManager { get; set; }

	public CameraRumbler CameraRumbler { get; set; }

	public CameraTextureHolder TextureHolder { get; set; }

	public bool ZoomActive { get; set; }

	public Player Rewired { get; private set; }

	public CRTEffect CrtEffect { get; private set; }

	public bool PanningRequired
	{
		get
		{
			bool result = false;
			if (!Core.Input.InputBlocked)
			{
				float axisRaw = Rewired.GetAxisRaw(20);
				float axisRaw2 = Rewired.GetAxisRaw(21);
				result = Math.Abs(axisRaw) > 0f || Mathf.Abs(axisRaw2) > 0f;
			}
			return result;
		}
	}

	public bool LevelHasGeometryBoundaries => Resources.FindObjectsOfTypeAll(typeof(BoxCollider)).Length > 0;

	private void Awake()
	{
		Instance = this;
		ProCamera2D = GetComponent<ProCamera2D>();
		TextureHolder = GetComponent<CameraTextureHolder>();
		ProCamera2DParallax = GetComponent<ProCamera2DParallax>();
		ProCamera2DPixelPerfect = GetComponent<ProCamera2DPixelPerfect>();
		CameraPlayerOffset = GetComponent<CameraPlayerOffset>();
		ProCamera2DNumericBoundaries = GetComponent<ProCamera2DNumericBoundaries>();
		ProCamera2DGeometryBoundaries = GetComponent<ProCamera2DGeometryBoundaries>();
		ProCamera2DShake = GetComponent<ProCamera2DShake>();
		CameraRails = GetComponent<ProCamera2DRails>();
		PanAndZoom = GetComponent<ProCamera2DPanAndZoom>();
		CustomCameraPan = GetComponent<CameraPan>();
		ShockwaveManager = GetComponent<ShockwaveManager>();
		ScreenEffectsManager = GetComponent<ScreenMaterialEffectsManager>();
		CameraRumbler = GetComponent<CameraRumbler>();
		CrtEffect = GetComponent<CRTEffect>();
	}

	private void OnDestroy()
	{
		StopCoroutine("EnableSmoothness");
	}

	public void UpdateNewCameraParams()
	{
		ProCamera2D.Instance.RemoveAllCameraTargets();
		ProCamera2D.Instance.AddCameraTarget(Core.Logic.Penitent.transform, 1f, 1f, 0f, new Vector2(0f, 6f));
		ProCamera2D.HorizontalFollowSmoothness = 0f;
		ProCamera2D.VerticalFollowSmoothness = 0f;
		Vector3 position = ProCamera2D.transform.position;
		Vector3 position2 = Core.Logic.Penitent.transform.position;
		Vector3 position3 = new Vector3(position2.x, position2.y, position.z);
		ProCamera2D.transform.position = position3;
		Rewired = ReInput.players.GetPlayer(0);
		ProCamera2DGeometryBoundaries.enabled = LevelHasGeometryBoundaries;
		StartCoroutine(EnableSmoothness());
	}

	private IEnumerator EnableSmoothness()
	{
		yield return new WaitForSeconds(1f);
		ProCamera2D.HorizontalFollowSmoothness = 0.1f;
		ProCamera2D.VerticalFollowSmoothness = 0.1f;
	}

	private void Update()
	{
		if ((bool)FadeWidget.instance && FadeWidget.instance.FadingIn)
		{
			ProCamera2D.Instance.MoveCameraInstantlyToPosition(ProCamera2D.Instance.CameraTargetPosition);
		}
		if (Core.Logic.CurrentState == LogicStates.Playing)
		{
			EnableCameraPan(PanningRequired && CustomCameraPan.EnableCameraPan);
		}
	}

	public void EnableCameraPan(bool enable)
	{
		if (!CustomCameraPan)
		{
			return;
		}
		if (enable)
		{
			if (!CustomCameraPan.enabled)
			{
				CustomCameraPan.enabled = true;
				CustomCameraPan.AddCameraPanToTargets();
			}
		}
		else if (CustomCameraPan.enabled)
		{
			CustomCameraPan.ProCamera2DPan.PanTarget.transform.position = Core.Logic.Penitent.transform.position;
			CustomCameraPan.RemoveCameraPanFromTargets();
			CustomCameraPan.enabled = false;
		}
	}
}
