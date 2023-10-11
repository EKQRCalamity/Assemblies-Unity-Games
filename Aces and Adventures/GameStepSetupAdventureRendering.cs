using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class GameStepSetupAdventureRendering : GameStep
{
	public override void Start()
	{
		HDAdditionalCameraData component = Camera.main.GetComponent<HDAdditionalCameraData>();
		component.stopNaNs = true;
		component.antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
		component.customRenderingSettings = true;
		FrameSettings renderingPathCustomFrameSettings = component.renderingPathCustomFrameSettings;
		renderingPathCustomFrameSettings.litShaderMode = LitShaderMode.Forward;
		component.renderingPathCustomFrameSettings = renderingPathCustomFrameSettings;
	}
}
