using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using VisualDesignCafe.Rendering.Nature;

public class GameStepSetupEnvironmentRendering : GameStep
{
	public override void Start()
	{
		HDAdditionalCameraData component = Camera.main.GetComponent<HDAdditionalCameraData>();
		component.stopNaNs = false;
		component.antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
		component.customRenderingSettings = true;
		FrameSettings renderingPathCustomFrameSettings = component.renderingPathCustomFrameSettings;
		renderingPathCustomFrameSettings.litShaderMode = LitShaderMode.Deferred;
		component.renderingPathCustomFrameSettings = renderingPathCustomFrameSettings;
		RefreshNatureRenderer.Instance?.SetDensityAndDraw(ProfileManager.options.video.quality.foliageDensity);
	}
}
